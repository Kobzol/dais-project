using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.ORM;

namespace Elearning.Database
{
    public static class DatabaseHelper
    {
        public const string BindVariablePrefix = "@";
        private const string RETURN_VALUE_NAME = "@return_value";

        public static string DBEscape(object input)
        {
            if (input is String)
            {
                return "'" + input.ToString() + "'";
            }
            else return input.ToString();
        }
        public static T GetColumnValue<T>(this SqlDataReader reader, string name)
        {
            int index = reader.GetOrdinal(name);

            if (reader.IsDBNull(index))
            {
                return default(T);
            }
            else return reader.GetFieldValue<T>(index);
        }
        public static string GenerateValues(params object[] values)
        {
            StringBuilder sb = new StringBuilder(values.Length * 10);

            foreach (object value in values)
            {
                sb.Append(DatabaseHelper.DBEscape(value));
                sb.Append(',');
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
        public static string GenerateBindedVariables(int count)
        {
            StringBuilder sb = new StringBuilder(count * 2);

            for (int i = 0; i < count; i++)
            {
                sb.Append(DatabaseHelper.BindVariablePrefix);
                sb.Append(i);
                sb.Append(',');
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        public static string GetBoundVariableByIndex(int index)
        {
            return String.Format(DatabaseHelper.BindVariablePrefix + "{0}", index);
        }

        public static void AddParameter<T>(this SqlCommand command, string name, SqlDbType type, T value)
        {
            command.Parameters.Add(name, type).Value = value;
        }
        public static void AddParameter<T>(this SqlCommand command, string name, SqlDbType type, int size, T value)
        {
            command.Parameters.Add(name, type, size).Value = value;
        }
        public static void AddParameter<T>(this SqlCommand command, string name, DBColumnAttribute attribute, T value)
        {
            command.AddParameter(name, attribute.DbType, attribute.Size, value);
        }

        public static void AddReturnValue(this SqlCommand command, SqlDbType type)
        {
            command.Parameters.Add(DatabaseHelper.RETURN_VALUE_NAME, type).Direction = ParameterDirection.ReturnValue;
        }
        public static T GetReturnValue<T>(this SqlCommand command)
        {
            return (T) command.Parameters[DatabaseHelper.RETURN_VALUE_NAME].Value;
        }

        public static DataTable CreateAnswerTable(List<QuestionAnswer> answers)
        {
            DataTable table = new DataTable();
            table.Columns.Add("text", typeof(string));
            table.Columns.Add("correct", typeof(bool));

            foreach (QuestionAnswer answer in answers)
            {
                table.Rows.Add(answer.Text, answer.Correct);
            }

            return table;
        }
        public static DataTable CreateExamQuestionTable(List<ExamQuestion> questions)
        {
            DataTable table = new DataTable();
            table.Columns.Add("questionId", typeof(int));
            table.Columns.Add("examId", typeof(int));
            table.Columns.Add("index", typeof(int));
            table.Columns.Add("points", typeof(decimal));

            foreach (ExamQuestion question in questions)
            {
                table.Rows.Add(question.QuestionId, question.ExamId, question.Index, question.Points);
            }

            return table;
        }
        public static DataTable CreateExamAnswerTable(List<ExamAnswer> answers)
        {
            DataTable table = new DataTable();
            table.Columns.Add("questionId", typeof(int));
            table.Columns.Add("text", typeof(string));

            foreach (ExamAnswer answer in answers)
            {
                table.Rows.Add(answer.QuestionId, answer.Text);
            }

            return table;
        }
    }
}
