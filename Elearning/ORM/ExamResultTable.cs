using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.Database;

namespace Elearning.ORM
{
    public class ExamResultTable : Table
    {
        public const string TABLE_NAME = "[ExamResult]";

        private const string SQL_SELECT_EXAMRESULT_BY_ID =
            "SELECT examId, ownerId, state, points, finished_at, created_at " +
            "FROM ExamResult " +
            "WHERE examResultId = @examResultId";

        private const string SQL_SELECT_EXAMRESULTS_BY_EXAM =
           "SELECT examResultId, examId, ownerId, state, points, finished_at, created_at " +
           "FROM ExamResult " +
           "WHERE examId = @examId";

        public static int InsertExamResult(DBConnection connection, int examId, int userId)
        {
            SqlCommand command = connection.GetCommand("CreateExamResult");
            command.CommandType = CommandType.StoredProcedure;
            command.AddParameter("@p_user_id", SqlDbType.Int, userId);
            command.AddParameter("@p_exam_id", SqlDbType.Int, examId);

            command.ExecuteNonQuery();
            return connection.GetLastInsertedId();
        }
        public static ExamResult GetExamResultById(DBConnection connection, int examResultId)
        {
            SqlCommand command = connection.GetCommand(ExamResultTable.SQL_SELECT_EXAMRESULT_BY_ID);
            command.AddParameter("@examResultId", SqlDbType.Int, examResultId);

            using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    return ExamResultTable.ReadExamResult(reader, examResultId);
                }
                else throw new DatabaseException("Tento výsledek testu nebyl nalezen");
            }
        }
        public static bool HandInExamResult(DBConnection connection, ExamResult examResult, List<ExamAnswer> answers)
        {
            SqlCommand command = connection.GetCommand("HandInExamResult");
            command.CommandType = CommandType.StoredProcedure;
            command.AddParameter("@p_user_id", SqlDbType.Int, examResult.OwnerId);
            command.AddParameter("@p_exam_id", SqlDbType.Int, examResult.ExamId);
            command.AddParameter("@p_answers", SqlDbType.Structured, DatabaseHelper.CreateExamAnswerTable(answers));

            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public static List<ExamResult> GetResultsByExam(DBConnection connection, Exam exam)
        {
            return ExamResultTable.GetResultsByExam(connection, exam.Id);
        }
        public static List<ExamResult> GetResultsByExam(DBConnection connection, int examId)
        {
            SqlCommand command = connection.GetCommand(ExamResultTable.SQL_SELECT_EXAMRESULTS_BY_EXAM);
            command.AddParameter("@examId", SqlDbType.Int, examId);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                List<ExamResult> results = new List<ExamResult>();

                while (reader.Read())
                {
                    results.Add(ExamResultTable.ReadExamResult(reader));
                }

                return results;
            }
        }

        private static ExamResult ReadExamResult(SqlDataReader reader)
        {
            return ExamResultTable.ReadExamResult(reader, (int) reader.GetColumnValue<decimal>("examResultId"));
        }
        private static ExamResult ReadExamResult(SqlDataReader reader, int examResultId)
        {
            return new ExamResult(
                examResultId,
                (int) reader.GetColumnValue<decimal>("examId"),
                (int) reader.GetColumnValue<decimal>("ownerId"),
                reader.GetColumnValue<string>("state"),
                reader.GetColumnValue<decimal>("points"),
                reader.GetColumnValue<DateTime>("created_at"),
                reader.GetColumnValue<DateTime?>("finished_at")
            );
        }
    }
}
