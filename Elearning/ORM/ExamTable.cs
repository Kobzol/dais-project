using Elearning.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elearning.ORM
{
    public class ExamTable : Table
    {
        public const string TABLE_NAME = "[Exam]";

        private const string SQL_INSERT_EXAM =
            "INSERT INTO [Exam](ownerId, name, timelimit, minimum_points, maximum_attempts, start_date, end_date) " +
            "VALUES(@ownerId, @name, @timelimit, @minimum_points, @maximum_attempts, @start_date, @end_date)";

        private const string SQL_GET_EXAM_BY_ID =
            "SELECT ownerId, name, timelimit, minimum_points, maximum_attempts, start_date, end_date, created_at, ExamQuestion.questionId, ExamQuestion.[index], ExamQuestion.points " +
            "FROM Exam " +
            "LEFT JOIN ExamQuestion ON Exam.examId = ExamQuestion.examId " + 
            "WHERE Exam.examId = @examId";

        private const string SQL_UPDATE_EXAM =
            "UPDATE Exam " +
            "SET name = @name, timelimit = @timelimit, minimum_points = @minimum_points, maximum_attempts = @maximum_attempts, start_date = @start_date, end_date = @end_date " +
            "WHERE examId = @examId";

        public static Exam InsertExam(DBConnection connection, User owner, string name, int timelimit, int minimum_points, int maximum_attempts, DateTime startDate, DateTime endDate)
        {
            return ExamTable.InsertExam(connection, owner.Id, name, timelimit, minimum_points, maximum_attempts, startDate, endDate);
        }
        public static Exam InsertExam(DBConnection connection, int ownerId, string name, int timelimit, int minimum_points, int maximum_attempts, DateTime startDate, DateTime endDate)
        {
            SqlCommand command = connection.GetCommand(ExamTable.SQL_INSERT_EXAM);
            command.AddParameter("@ownerId", SqlDbType.Int, ownerId);
            command.AddParameter("@name", SqlDbType.VarChar, 255, name);
            command.AddParameter("@timelimit", SqlDbType.Int, timelimit);
            command.AddParameter("@minimum_points", SqlDbType.Int, minimum_points);
            command.AddParameter("@maximum_attempts", SqlDbType.Int, maximum_attempts);
            command.AddParameter("@start_date", SqlDbType.DateTime2, startDate);
            command.AddParameter("@end_date", SqlDbType.DateTime2, endDate);

            try
            {
                int rows = command.ExecuteNonQuery();

                if (rows > 0)
                {
                    return new Exam(connection.GetLastInsertedId(), ownerId, name, timelimit, minimum_points, maximum_attempts, startDate, endDate, DateTime.Now);
                }
                else throw new DatabaseException("Nepodařilo se vytvořit test");
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }
        }
        public static Exam GetExamById(DBConnection connection, int examId)
        {
            SqlCommand command = connection.GetCommand(ExamTable.SQL_GET_EXAM_BY_ID);
            command.AddParameter("@examId", SqlDbType.Int, examId);

            try
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Exam exam = null;

                    while (reader.Read())
                    {
                        if (exam == null)
                        {
                            exam = new Exam(examId,
                                            (int) reader.GetColumnValue<decimal>("ownerId"),
                                            reader.GetColumnValue<string>("name"),
                                            reader.GetColumnValue<int>("timelimit"),
                                            reader.GetColumnValue<int>("minimum_points"),
                                            reader.GetColumnValue<int>("maximum_attempts"),
                                            reader.GetColumnValue<DateTime>("start_date"),
                                            reader.GetColumnValue<DateTime>("end_date"),
                                            reader.GetColumnValue<DateTime>("created_at")
                            );
                        }

                        object questionId = reader.GetValue(reader.GetOrdinal("questionId"));

                        if (!DBNull.Value.Equals(questionId))
                        {
                            ExamQuestion examQuestion = new ExamQuestion(
                                (int)reader.GetColumnValue<decimal>("questionId"),
                                examId,
                                reader.GetColumnValue<int>("index"),
                                reader.GetColumnValue<decimal>("points")
                            );

                            exam.AddQuestion(examQuestion);
                        }
                    }

                    if (exam == null)
                    {
                        throw new DatabaseException("Couldn't find exam");
                    }

                    return exam;
                }
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }
        }
        public static bool UpdateExam(DBConnection connection, Exam exam)
        {
            try
            {
                SqlTransaction transaction = connection.BeginTransaction();

                SqlCommand command = connection.GetCommand(ExamTable.SQL_UPDATE_EXAM);
                command.AddParameter("@name", SqlDbType.VarChar, 255, exam.Name);
                command.AddParameter("@timelimit", SqlDbType.Int, exam.Timelimit);
                command.AddParameter("@minimum_points", SqlDbType.Int, exam.MinimumPoints);
                command.AddParameter("@maximum_attempts", SqlDbType.Int, exam.MaximumAttempts);
                command.AddParameter("@start_date", SqlDbType.DateTime2, exam.StartDate);
                command.AddParameter("@end_date", SqlDbType.DateTime2, exam.EndDate);
                command.AddParameter("@examId", SqlDbType.Int, exam.Id);

                int rows = command.ExecuteNonQuery();

                if (rows > 0)
                {
                    command = connection.GetCommand("DELETE FROM ExamQuestion WHERE examId = @examId");
                    command.AddParameter("@examId", SqlDbType.Int, exam.Id);

                    command.ExecuteNonQuery();

                    DataTable questions = DatabaseHelper.CreateExamQuestionTable(exam.Questions);

                    using (SqlBulkCopy copier = new SqlBulkCopy(connection.Connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        copier.DestinationTableName = "ExamQuestion";
                        copier.WriteToServer(questions);
                    }

                    connection.Commit();

                    return true;
                }
                else throw new DatabaseException("Neexistuje test s id " + exam.Id);
            }
            catch (Exception e)
            {
                connection.Rollback();

                return false;
            }
        }
        public static bool DeleteExam(DBConnection connection, Exam exam, User user)
        {
            return ExamTable.DeleteExam(connection, exam, user.Id);
        }
        public static bool DeleteExam(DBConnection connection, Exam exam, int userId)
        {
            SqlCommand command = connection.GetCommand("DeleteExam");
            command.CommandType = CommandType.StoredProcedure;
            command.AddParameter("@p_exam_id", SqlDbType.Int, exam.Id);
            command.AddParameter("@p_user_id", SqlDbType.Int, userId);

            try
            {
                return command.ExecuteNonQuery() > 0;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public static bool IsExamActive(DBConnection connection, Exam exam)
        {
            SqlCommand command = connection.GetCommand("IsExamActive");
            command.CommandType = CommandType.StoredProcedure;
            command.AddParameter("@p_exam_id", SqlDbType.Int, exam.Id);

            command.AddReturnValue(SqlDbType.Bit);

            command.ExecuteScalar();

            return command.GetReturnValue<bool>();
        }
        public static List<int> GetActiveExams(DBConnection connection)
        {
            List<int> activeExams = new List<int>();

            SqlCommand command = connection.GetCommand("GetActiveExams");
            command.CommandType = CommandType.StoredProcedure;

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    activeExams.Add((int) reader.GetColumnValue<decimal>("examId"));
                }
            }

            return activeExams;
        }
    }
}
