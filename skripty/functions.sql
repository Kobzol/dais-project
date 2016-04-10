CREATE FUNCTION GetUserRole
(@p_user_id INT)
RETURNS VARCHAR(255)
AS
BEGIN
	DECLARE @v_user_role VARCHAR(255)

	SELECT @v_user_role = Role.name
	FROM Role
	JOIN "User" ON Role.roleId = "User".roleId AND "User".userId = @p_user_id

	RETURN @v_user_role
END
GO

CREATE FUNCTION CanViewExamResult
(@p_user_id IdType, @p_exam_result_id IdType)
RETURNS BIT
AS
BEGIN
	DECLARE @v_user_role VARCHAR(255) = dbo.GetUserRole(@p_user_id)

	IF @v_user_role = 'admin'
		RETURN 1

	DECLARE @v_result_owner INT,
			@v_exam_owner INT

	SELECT @v_result_owner = ExamResult.ownerId, @v_exam_owner = Exam.ownerId
	FROM ExamResult
	JOIN Exam ON Exam.examId = ExamResult.examId AND examResult.examResultId = @p_exam_result_id

	IF @v_user_role = 'lector' AND (@p_user_id = @v_exam_owner OR @p_user_id = @v_result_owner)
		RETURN 1

	IF @v_user_role = 'student' AND @p_user_id = @v_result_owner
		RETURN 1

	RETURN 0		
END
GO

CREATE FUNCTION CanCreateTests
(@p_user_id IdType)
RETURNS BIT
AS
BEGIN
	IF dbo.GetUserRole(@p_user_id) IN ('admin', 'lector')
		RETURN 1
	
	RETURN 0
END
GO

CREATE PROCEDURE CreateQuestion
(@p_user_id IdType, @p_text NVARCHAR(MAX), @p_type VARCHAR(10), @p_answers QuestionAnswerType READONLY, @p_question_id IdType OUT)
AS
BEGIN
	IF dbo.CanCreateTests(@p_user_id) != 1
		THROW 50001, 'User does not have enough privileges', 1

	BEGIN TRY
		BEGIN TRANSACTION

		INSERT INTO Question(ownerId, [text], [type])
		VALUES (@p_user_id, @p_text, @p_type)

		DECLARE answers CURSOR LOCAL FOR (SELECT [text], correct FROM @p_answers)
		DECLARE @v_index INT = 0,
				@v_text NVARCHAR(MAX),
				@v_correct BIT,
				@v_question_id IdType = SCOPE_IDENTITY()	-- id of inserted question

		SET @p_question_id = @v_question_id

		OPEN answers
		
		WHILE 1=1
		BEGIN
			FETCH NEXT FROM answers INTO @v_text, @v_correct

			IF @@FETCH_STATUS != 0
				BREAK

			INSERT INTO QuestionAnswer(questionId, [index], [text], correct)
			VALUES (@v_question_id, @v_index, @v_text, @v_correct)

			SET @v_index = @v_index + 1
		END

		IF @p_type = 'open'
		BEGIN
			IF @v_index < 1
				THROW 50003, 'Open questions must have at least one answer', 1
		END
		ELSE IF @p_type = 'closed'
		BEGIN
			IF @v_index < 2
				THROW 50005, 'Closed questions must have at least two answers', 1
		END

		CLOSE answers
		DEALLOCATE answers

		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		THROW
	END CATCH
END
GO

CREATE PROCEDURE DeleteQuestion
(@p_question_id IdType)
AS
BEGIN
	IF EXISTS (SELECT * FROM ExamQuestion WHERE questionId = @p_question_id)
		THROW 50009, 'You can''t delete question that is being used in a test', 1

	BEGIN TRY
		BEGIN TRANSACTION

		DELETE FROM QuestionAnswer
		WHERE questionId = @p_question_id

		DELETE FROM	Question
		WHERE questionId = @p_question_id

		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		THROW
	END CATCH
END
GO

CREATE PROCEDURE UpdateQuestion
(@p_question_id IdType, @p_text NVARCHAR(MAX), @p_type VARCHAR(10), @p_answers QuestionAnswerType READONLY)
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM Question WHERE questionId = @p_question_id)
		THROW 50020, 'You are updating a question that doesn''t exist', 1

	BEGIN TRANSACTION
	BEGIN TRY
		UPDATE Question
		SET [text] = @p_text
		WHERE questionId = @p_question_id;

		DELETE FROM QuestionAnswer
		WHERE questionId = @p_question_id;

		DECLARE answers CURSOR LOCAL FOR (SELECT [text], correct FROM @p_answers)
		DECLARE @v_text NVARCHAR(MAX)
		DECLARE @v_correct BIT
		DECLARE @v_index INT = 0

		OPEN answers
		
		WHILE 1=1
		BEGIN
			FETCH NEXT FROM answers INTO @v_text, @v_correct

			IF @@FETCH_STATUS != 0
				BREAK

			INSERT INTO QuestionAnswer(questionId, [index], [text], correct)
			VALUES (@p_question_id, @v_index, @v_text, @v_correct)

			SET @v_index = @v_index + 1
		END

		IF @p_type = 'open'
		BEGIN
			IF @v_index < 1
				THROW 50003, 'Open questions must have at least one answer', 1
		END
		ELSE IF @p_type = 'closed'
		BEGIN
			IF @v_index < 2
				THROW 50005, 'Closed questions must have at least two answers', 1
		END

		CLOSE answers
		DEALLOCATE answers

		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		THROW
	END CATCH
END
GO

CREATE TRIGGER DisableChangeOfQuestionType
ON Question
AFTER UPDATE
AS
DECLARE questions CURSOR LOCAL FOR
(
	SELECT inserted.[type], deleted.[type]
	FROM inserted
	JOIN deleted ON inserted.questionId = deleted.questionId
)
DECLARE @v_old_type VARCHAR(10),
		@v_new_type VARCHAR(10);
BEGIN
	OPEN questions

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM questions INTO @v_new_type, @v_old_type

		IF @@FETCH_STATUS != 0
			BREAK
		
		IF @v_new_type != @v_old_type
			THROW 50007, 'You can''t change the type of a question after it is created', 1
	END

	CLOSE questions
	DEALLOCATE questions
END
GO

/*CREATE TRIGGER CheckAnswerCount
ON QuestionAnswer
AFTER DELETE
AS
DECLARE questions CURSOR LOCAL FOR
(
	SELECT questionId
	FROM deleted
)
DECLARE @v_question_id IdType
DECLARE @v_question_count INT
DECLARE @v_question_type VARCHAR(10)
BEGIN
	OPEN questions

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM questions INTO @v_question_id

		IF @@FETCH_STATUS != 0
			BREAK

		SELECT @v_question_count = COUNT(*)
		FROM QuestionAnswer
		WHERE questionId = @v_question_id

		SELECT @v_question_type = [type]
		FROM Question
		WHERE questionId = @v_question_id

		IF @v_question_type = 'open'
		BEGIN
			IF @v_question_count < 1
				THROW 50008, 'Open questions must have at least one answer', 1
		END
		ELSE IF @v_question_type = 'closed'
		BEGIN
			IF @v_question_count < 2
				THROW 50009, 'Closed questions must have at least two answers', 1
		END
	END

	CLOSE questions
	DEALLOCATE questions
END
GO
DROP TRIGGER CheckAnswerCount;

CREATE PROCEDURE CreateExam
(@p_user_id IdType, @p_name VARCHAR(255), @p_timelimit INT, @p_minimum_points INTEGER,
 @p_maximum_attempts INTEGER, @p_start_date DATETIME2, @p_end_date DATETIME2)
 AS
 BEGIN
	IF dbo.CanCreateTests(@p_user_id) != 1
		THROW 50001, 'User does not have enough privileges', 1

	INSERT INTO Exam(ownerId, name, timelimit, minimum_points, maximum_attempts, [start_date], end_date)
	VALUES (@p_user_id, @p_name, @p_timelimit, @p_minimum_points, @p_maximum_attempts, @p_start_date, @p_end_date) 
 END
GO*/

CREATE PROCEDURE DeleteExam
(@p_exam_id IdType, @p_user_id IdType)
AS
BEGIN
	IF (SELECT ownerId FROM Exam WHERE examId = @p_exam_id) != @p_user_id
		THROW 50010, 'User can delete only tests that he created', 1

	IF EXISTS (SELECT * FROM ExamResult WHERE examId = @p_exam_id)
		THROW 50018, 'You cannot delete this test because it has been already used', 1

	BEGIN TRANSACTION
	BEGIN TRY
		DELETE FROM ExamQuestion
		WHERE examId = @p_exam_id

		DELETE FROM Exam
		WHERE examId = @p_exam_id

		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		THROW
	END CATCH
END
GO

CREATE PROCEDURE AddQuestionToExam
(@p_user_id IdType, @p_exam_id IdType, @p_question_id IdType, @p_index INT, @p_points DECIMAL(5,2))
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM Exam WHERE examId = @p_exam_id AND ownerId = @p_user_id)
		THROW 50006, 'The given exam does not belong to the given user', 1

	IF EXISTS (SELECT * FROM ExamQuestion WHERE questionId = @p_question_id AND examId = @p_exam_id) -- if this question is in the test, update it's index
	BEGIN
		UPDATE ExamQuestion
		SET [index] = @p_index, points = @p_points
		WHERE questionId = @p_question_id AND examId = @p_exam_id

		RETURN
	END

	BEGIN TRY
		BEGIN TRANSACTION

		IF NOT EXISTS (SELECT * FROM ExamQuestion WHERE examId = @p_exam_id) -- if this is the first question, set index to 0
			SET @p_index = 0
		ELSE IF NOT EXISTS (SELECT * FROM ExamQuestion WHERE examId = @p_exam_id AND [index] = @p_index)	-- set the index to MAX(index) + 1
			SELECT @p_index = MAX([index]) + 1
			FROM ExamQuestion
			WHERE examId = @p_exam_id
		ELSE
		BEGIN						-- move all the bigger indices up
			UPDATE ExamQuestion
			SET [index] = [index] + 1
			WHERE examId = @p_exam_id AND [index] >= @p_index
		END

		INSERT INTO ExamQuestion(questionId, examId, [index], points)
		VALUES(@p_question_id, @p_exam_id, @p_index, @p_points)

		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		THROW
	END CATCH
END
GO

CREATE FUNCTION IsExamActive
(@p_exam_id IdType)
RETURNS BIT
AS
BEGIN
	IF EXISTS (SELECT * FROM Exam WHERE examId = @p_exam_id AND GETDATE() BETWEEN [start_date] AND [end_date])
		RETURN 1

	RETURN 0
END
GO

CREATE FUNCTION CountExamAttempts
(@p_exam_id IdType, @p_user_id IdType)
RETURNS INT
AS
BEGIN
	DECLARE @v_count INT

	SELECT @v_count = COUNT(*)
	FROM ExamResult
	WHERE examId = @p_exam_id AND ownerId = @p_user_id

	RETURN @v_count
END
GO

CREATE PROCEDURE CreateExamResult
(@p_user_id IdType, @p_exam_id IdType)
AS
DECLARE @v_maximum_attempts INT
BEGIN
	BEGIN TRANSACTION
	BEGIN TRY
		IF dbo.IsExamActive(@p_exam_id) != 1
			THROW 50011, 'The exam is not active right now', 1

		SELECT @v_maximum_attempts = maximum_attempts
		FROM Exam
		WHERE examId = @p_exam_id

		IF dbo.CountExamAttempts(@p_exam_id, @p_user_id) = @v_maximum_attempts
			THROW 50012, 'The user has exhausted his attempts at this test', 1

		IF EXISTS (SELECT * FROM ExamResult WHERE ownerId = @p_user_id AND [state] = 'created')
			THROW 50013, 'You are already doing another test', 1

		INSERT INTO ExamResult(examId, ownerId, [state], points)
		VALUES(@p_exam_id, @p_user_id, 'created', 0)

		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		THROW
	END CATCH
END
GO

CREATE PROCEDURE HandInExamResult
(@p_user_id IdType, @p_exam_id IdType, @p_answers ExamResultQuestionType READONLY)
AS
	DECLARE @v_timelimit INT,
			@v_exam_result_id IdType,
			@v_question_id IdType,
			@v_points INT,
			@v_total_points INT = 0,
			@v_text NVARCHAR(MAX);
BEGIN
	IF NOT EXISTS (SELECT * FROM ExamResult WHERE examId = @p_exam_id AND ownerId = @p_user_id)
		THROW 50014, 'You didn''t create an attempt to solve this exam', 1

	SELECT @v_timelimit = timelimit
	FROM Exam
	WHERE examId = @p_exam_id

	SELECT @v_exam_result_id = examResultId
	FROM ExamResult
	WHERE examId = @p_exam_id AND ownerId = @p_user_id AND [state] = 'created' 

	IF DATEADD(minute, @v_timelimit, (SELECT [created_at] FROM ExamResult WHERE examId = @p_exam_id AND ownerId = @p_user_id)) < GETDATE()
	BEGIN
		UPDATE ExamResult
		SET [state] = 'expired'
		WHERE examResultId = @v_exam_result_id;

		RETURN
	END

	BEGIN TRY
		BEGIN TRANSACTION

		DECLARE questions CURSOR LOCAL FOR
		(
			SELECT eq.questionId, eq.points, pa.[text]
			FROM ExamQuestion eq
			LEFT JOIN @p_answers pa ON pa.questionId = eq.questionId
			WHERE examId = @p_exam_id
		)

		OPEN questions

		WHILE 1=1
		BEGIN
			FETCH NEXT FROM questions INTO @v_question_id, @v_points, @v_text

			IF @@FETCH_STATUS != 0
				BREAK
		
			IF @v_text NOT IN (SELECT [text] FROM QuestionAnswer WHERE questionId = @v_question_id AND correct = 1)
				SET @v_points = 0
			
			INSERT INTO ExamAnswer(examResultId, questionId, [text], points)
			VALUES (@v_exam_result_id, @v_question_id, @v_text, @v_points)
			
			SET @v_total_points = @v_total_points + @v_points
		END

		UPDATE ExamResult
		SET points = @v_total_points, [state] = 'finished', finished_at = GETDATE()
		WHERE examResultId = @v_exam_result_id

		CLOSE questions
		DEALLOCATE questions

		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		THROW
	END CATCH
END
GO

CREATE PROCEDURE GenerateRandomQuestions
(@p_count INT)
AS
	DECLARE @v_questions TABLE (questionId IdType);
	DECLARE	@v_question_count INT,
			@v_random FLOAT,
			@v_question_id IdType,
			@v_max_id IdType;
BEGIN
	SELECT @v_question_count = COUNT(*)
	FROM Question

	IF (@v_question_count) < @p_count
		THROW 50015, 'There are not enough questions', 1

	BEGIN TRANSACTION

	SELECT @v_max_id = MAX(questionId)
	FROM Question

	WHILE @p_count > 0
	BEGIN
		SET @v_random = RAND()
		SET @v_question_id = FLOOR(@v_max_id * @v_random)

		IF EXISTS (SELECT * FROM Question WHERE questionId = @v_question_id) AND NOT EXISTS (SELECT * FROM @v_questions WHERE questionId = @v_question_id)
		BEGIN
			INSERT INTO @v_questions(questionId)
			VALUES(@v_question_id)

			SET @p_count = @p_count - 1
		END
	END

	COMMIT;

	SELECT questionId FROM @v_questions;

	RETURN
END
GO

CREATE PROCEDURE GetActiveExams
AS
BEGIN
	SELECT examId
	FROM Exam
	WHERE dbo.IsExamActive(examId) = 1
END
GO

CREATE PROCEDURE ShowExamResult
(@p_user_id IdType, @p_exam_result_id IdType)
AS
BEGIN
	IF dbo.CanViewExamResult(@p_user_id, @p_exam_result_id) != 1
		THROW 50019, 'You can''t view this exam''s result', 1

	SELECT [User].name, [User].surname, points
	FROM ExamResult
	JOIN [User] ON ExamResult.ownerId = [User].userId
	WHERE ExamResult.examResultId = @p_exam_result_id
END
GO