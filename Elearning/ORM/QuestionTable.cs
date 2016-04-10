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
    public class QuestionTable : Table
    {
        public const string TABLE_NAME = "[Question]";

        private const string SQL_SELECT_QUESTION_BY_ID =
            "SELECT Question.questionId, Question.ownerId, Question.text as q_text, Question.type, QuestionAnswer.[index], QuestionAnswer.text as qa_text, QuestionAnswer.correct " +
            "FROM Question " +
            "JOIN QuestionAnswer ON Question.questionId = QuestionAnswer.questionId " +
            "WHERE Question.questionId = @questionId";

        private const string SQL_SELECT_ALL_QUESTIONS_NO_ANSWERS =
            "SELECT Question.questionId, Question.ownerId, Question.text as q_text, Question.type " +
            "FROM Question";

        public static Question InsertQuestion(DBConnection connection, User creator, string text, string type, List<QuestionAnswer> answers)
        {
            return QuestionTable.InsertQuestion(connection, creator.Id, text, type, answers);
        }
        public static Question InsertQuestion(DBConnection connection, int creatorId, string text, string type, List<QuestionAnswer> answers)
        {
            SqlCommand command = connection.GetCommand("CreateQuestion");
            command.CommandType = CommandType.StoredProcedure;
            command.AddParameter("@p_user_id", SqlDbType.Int, creatorId);
            command.AddParameter("@p_text", SqlDbType.NVarChar, -1, text);
            command.AddParameter("@p_type", SqlDbType.VarChar, 10, type);
            command.Parameters.Add("@p_question_id", SqlDbType.Int).Direction = ParameterDirection.Output;

            SqlParameter answerList = new SqlParameter("@p_answers", SqlDbType.Structured);
            answerList.TypeName = "dbo.QuestionAnswerType";
            answerList.Value = DatabaseHelper.CreateAnswerTable(answers);

            command.Parameters.Add(answerList);

            try
            {
                int rows = command.ExecuteNonQuery();

                if (rows > 0)
                {
                    int questionId = Convert.ToInt32(command.Parameters["@p_question_id"].Value);
                    Question question = new Question(questionId, creatorId, text, type);
                    int index = 0;

                    foreach (QuestionAnswer answer in answers)
                    {
                        question.AddAnswer(new QuestionAnswer(questionId, index++, answer.Text, answer.Correct));
                    }

                    return question;
                }
                else throw new DatabaseException("Nepodařilo se vytvořit testovou otázku");
            }
            catch (SqlException e)
            {
                throw new DatabaseException(e.Message);
            }
        }
        public static Question GetQuestionById(DBConnection connection, int questionId)
        {
            SqlCommand command = connection.GetCommand(QuestionTable.SQL_SELECT_QUESTION_BY_ID);
            command.AddParameter("@questionId", SqlDbType.Int, questionId);

            try
            {
                Question question = null;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (question == null)
                        {
                            question = QuestionTable.ReadQuestion(reader);
                        }

                        QuestionAnswer answer = new QuestionAnswer(question.Id, reader.GetColumnValue<int>("index"), reader.GetColumnValue<string>("qa_text"), reader.GetColumnValue<bool>("correct"));
                        question.AddAnswer(answer);
                    }
                }

                if (question == null)
                {
                    throw new DatabaseException("Question does not exist");
                }

                return question;
            }
            catch (Exception e)
            {
                throw new DatabaseException(e.Message);
            }
        }
        public static bool UpdateQuestion(DBConnection connection, Question question)
        {
            SqlCommand command = connection.GetCommand("UpdateQuestion");
            command.CommandType = CommandType.StoredProcedure;
            command.AddParameter("@p_question_id", SqlDbType.Int, question.Id);
            command.AddParameter("@p_text", SqlDbType.NVarChar, -1, question.Text);
            command.AddParameter("@p_type", SqlDbType.VarChar, 10, question.Type);

            SqlParameter answerList = new SqlParameter("@p_answers", SqlDbType.Structured);
            answerList.TypeName = "dbo.QuestionAnswerType";
            answerList.Value = DatabaseHelper.CreateAnswerTable(question.Answers);

            command.Parameters.Add(answerList);

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
        public static bool DeleteQuestion(DBConnection connection, Question question)
        {
            try
            {
                SqlCommand command = connection.GetCommand("DeleteQuestion");
                command.CommandType = CommandType.StoredProcedure;
                command.AddParameter("@p_question_id", SqlDbType.Int, question.Id);

                command.ExecuteNonQuery();

                return true;
            }
            catch (SqlException e)
            {
                return false;
            }
        }

        public static List<Question> GetAllQuestionsWithoutAnswers(DBConnection connection)
        {
            SqlCommand command = connection.GetCommand(QuestionTable.SQL_SELECT_ALL_QUESTIONS_NO_ANSWERS);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                List<Question> questions = new List<Question>();

                while (reader.Read())
                {
                    questions.Add(QuestionTable.ReadQuestion(reader));
                }

                return questions;
            }
        }

        public static List<int> GenerateRandomQuestions(DBConnection connection, int count)
        {
            SqlCommand command = connection.GetCommand("GenerateRandomQuestions");
            command.CommandType = CommandType.StoredProcedure;
            command.AddParameter("@p_count", SqlDbType.Int, count);

            List<int> questionIds = new List<int>();

            try
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questionIds.Add((int)reader.GetColumnValue<decimal>("questionId"));
                    }
                }

                return questionIds;
            }
            catch (SqlException e)
            {
                throw new DatabaseException(e.Message);
            }
        }

        private static Question ReadQuestion(SqlDataReader reader)
        {
            return new Question(
                (int) reader.GetColumnValue<decimal>("questionId"),
                (int) reader.GetColumnValue<decimal>("ownerId"),
                reader.GetColumnValue<string>("q_text"),
                reader.GetColumnValue<string>("type")
            );
        }
    }
}
