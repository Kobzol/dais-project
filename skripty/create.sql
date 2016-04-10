-- Generated by Oracle SQL Developer Data Modeler 4.0.3.853
--   at:        2015-04-27 15:49:54 CEST
--   site:      SQL Server 2008
--   type:      SQL Server 2008

CREATE TYPE IdType
FROM NUMERIC(28)
GO

CREATE TYPE QuestionAnswerType AS
TABLE ([text] NVARCHAR(MAX), correct BIT)
GO

CREATE Type ExamResultQuestionType AS
TABLE (questionId IdType, [text] NVARCHAR(MAX))


CREATE
  TABLE Exam
  (
    examId           NUMERIC (28) NOT NULL IDENTITY NOT FOR REPLICATION ,
    ownerId          NUMERIC (28) NOT NULL ,
    name             VARCHAR (255) NOT NULL ,
    timelimit        INTEGER NOT NULL ,
    minimum_points   INTEGER NOT NULL ,
    maximum_attempts INTEGER NOT NULL ,
    start_date DATETIME2 NOT NULL ,
    end_date DATETIME2 NOT NULL ,
    created_at DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP ,
    CONSTRAINT Exam_PK PRIMARY KEY CLUSTERED (examId)
WITH
  (
    ALLOW_PAGE_LOCKS = ON ,
    ALLOW_ROW_LOCKS  = ON
  )
  ON "default"
  )
  ON "default"
GO
ALTER TABLE Exam
ADD
CHECK ( [timelimit] > 0 )
GO
ALTER TABLE Exam
ADD
CHECK ( [minimum_points] >= 0 )
GO
ALTER TABLE Exam
ADD
CHECK ( [maximum_attempts] > 0 )
GO
ALTER TABLE Exam
ADD
CHECK ( [end_date] > [start_date] )
GO

CREATE
  TABLE ExamAnswer
  (
    examAnswerId NUMERIC (28) NOT NULL IDENTITY NOT FOR REPLICATION ,
    examResultId NUMERIC (28) NOT NULL ,
    questionId   NUMERIC (28) NOT NULL ,
    text NVARCHAR (MAX) NOT NULL ,
    points DECIMAL (5,2) NOT NULL ,
    CONSTRAINT ExamAnswer_PK PRIMARY KEY CLUSTERED (examAnswerId)
WITH
  (
    ALLOW_PAGE_LOCKS = ON ,
    ALLOW_ROW_LOCKS  = ON
  )
  ON "default"
  )
  ON "default"
GO
ALTER TABLE ExamAnswer
ADD
CHECK ( [points] >= 0 )
GO

CREATE
  TABLE ExamQuestion
  (
    questionId NUMERIC (28) NOT NULL ,
    examId     NUMERIC (28) NOT NULL ,
    "index"    INTEGER NOT NULL ,
    points     DECIMAL (5,2) NOT NULL ,
    CONSTRAINT EXAM_QUESTION_PK PRIMARY KEY CLUSTERED (questionId, examId)
WITH
  (
    ALLOW_PAGE_LOCKS = ON ,
    ALLOW_ROW_LOCKS  = ON
  )
  ON "default"
  )
  ON "default"
GO
ALTER TABLE ExamQuestion
ADD
CHECK ( [points] > 0 )
GO

CREATE
  TABLE ExamResult
  (
    examResultId NUMERIC (28) NOT NULL IDENTITY NOT FOR REPLICATION ,
    examId       NUMERIC (28) NOT NULL ,
    ownerId      NUMERIC (28) NOT NULL ,
    state        VARCHAR (255) NOT NULL ,
    points       DECIMAL (10,2) NOT NULL ,
    finished_at DATETIME2 ,
    created_at DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP ,
    CONSTRAINT ExamResult_PK PRIMARY KEY CLUSTERED (examResultId)
WITH
  (
    ALLOW_PAGE_LOCKS = ON ,
    ALLOW_ROW_LOCKS  = ON
  )
  ON "default"
  )
  ON "default"
GO
ALTER TABLE ExamResult
ADD
CHECK ( state IN ('created', 'expired', 'finished') )
GO
ALTER TABLE ExamResult
ADD
CHECK ( [points] >= 0 )
GO

CREATE
  TABLE Question
  (
    questionId NUMERIC (28) NOT NULL IDENTITY NOT FOR REPLICATION ,
    ownerId    NUMERIC (28) NOT NULL ,
    text NVARCHAR (MAX) NOT NULL ,
    type VARCHAR (10) NOT NULL ,
    CONSTRAINT Question_PK PRIMARY KEY CLUSTERED (questionId)
WITH
  (
    ALLOW_PAGE_LOCKS = ON ,
    ALLOW_ROW_LOCKS  = ON
  )
  ON "default"
  )
  ON "default"
GO
ALTER TABLE Question
ADD
CHECK ( type IN ('closed', 'open') )
GO

CREATE
  TABLE QuestionAnswer
  (
    questionId NUMERIC (28) NOT NULL ,
    "index"    INTEGER NOT NULL ,
    text NVARCHAR (MAX) NOT NULL ,
    correct BIT NOT NULL ,
    CONSTRAINT QuestionAnswer_PK PRIMARY KEY CLUSTERED (questionId, "index")
WITH
  (
    ALLOW_PAGE_LOCKS = ON ,
    ALLOW_ROW_LOCKS  = ON
  )
  ON "default"
  )
  ON "default"
GO

CREATE
  TABLE Role
  (
    roleId NUMERIC (28) NOT NULL IDENTITY NOT FOR REPLICATION ,
    name   VARCHAR (255) NOT NULL ,
    CONSTRAINT Role_PK PRIMARY KEY CLUSTERED (roleId)
WITH
  (
    ALLOW_PAGE_LOCKS = ON ,
    ALLOW_ROW_LOCKS  = ON
  )
  ON "default"
  )
  ON "default"
GO

CREATE
  TABLE "User"
  (
    userId  NUMERIC (28) NOT NULL IDENTITY NOT FOR REPLICATION ,
    roleId  NUMERIC (28) NOT NULL ,
    name    VARCHAR (100) NOT NULL ,
    surname VARCHAR (100) NOT NULL ,
    created_at DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP ,
    CONSTRAINT User_PK PRIMARY KEY CLUSTERED (userId)
WITH
  (
    ALLOW_PAGE_LOCKS = ON ,
    ALLOW_ROW_LOCKS  = ON
  )
  ON "default"
  )
  ON "default"
GO

ALTER TABLE ExamAnswer
ADD CONSTRAINT ExamAnswer_ExamResult_FK FOREIGN KEY
(
examResultId
)
REFERENCES ExamResult
(
examResultId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO

ALTER TABLE ExamAnswer
ADD CONSTRAINT ExamAnswer_Question_FK FOREIGN KEY
(
questionId
)
REFERENCES Question
(
questionId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO

ALTER TABLE ExamResult
ADD CONSTRAINT ExamResult_Exam_FK FOREIGN KEY
(
examId
)
REFERENCES Exam
(
examId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO

ALTER TABLE ExamResult
ADD CONSTRAINT ExamResult_User_FK FOREIGN KEY
(
ownerId
)
REFERENCES "User"
(
userId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO

ALTER TABLE Exam
ADD CONSTRAINT Exam_User_FK FOREIGN KEY
(
ownerId
)
REFERENCES "User"
(
userId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO

ALTER TABLE ExamQuestion
ADD CONSTRAINT FK_ASS_3 FOREIGN KEY
(
questionId
)
REFERENCES Question
(
questionId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO

ALTER TABLE ExamQuestion
ADD CONSTRAINT FK_ASS_4 FOREIGN KEY
(
examId
)
REFERENCES Exam
(
examId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO

ALTER TABLE QuestionAnswer
ADD CONSTRAINT QuestionAnswer_Question_FK FOREIGN KEY
(
questionId
)
REFERENCES Question
(
questionId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO

ALTER TABLE Question
ADD CONSTRAINT Question_User_FK FOREIGN KEY
(
ownerId
)
REFERENCES "User"
(
userId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO

ALTER TABLE "User"
ADD CONSTRAINT User_Role_FK FOREIGN KEY
(
roleId
)
REFERENCES Role
(
roleId
)
ON
DELETE
  NO ACTION ON
UPDATE NO ACTION
GO


-- Oracle SQL Developer Data Modeler Summary Report: 
-- 
-- CREATE TABLE                             8
-- CREATE INDEX                             0
-- ALTER TABLE                             19
-- CREATE VIEW                              0
-- CREATE PACKAGE                           0
-- CREATE PACKAGE BODY                      0
-- CREATE PROCEDURE                         0
-- CREATE FUNCTION                          0
-- CREATE TRIGGER                           0
-- ALTER TRIGGER                            0
-- CREATE DATABASE                          0
-- CREATE DEFAULT                           0
-- CREATE INDEX ON VIEW                     0
-- CREATE ROLLBACK SEGMENT                  0
-- CREATE ROLE                              0
-- CREATE RULE                              0
-- CREATE PARTITION FUNCTION                0
-- CREATE PARTITION SCHEME                  0
-- 
-- DROP DATABASE                            0
-- 
-- ERRORS                                   0
-- WARNINGS                                 0
