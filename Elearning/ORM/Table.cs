using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.Database;
using System.Data.SqlClient;
using System.Reflection;

namespace Elearning.ORM
{
    public class Table
    {
        protected static SqlCommand GenerateSelectAllById(DBConnection connection, string tableName, string idAttrib, int idValue)
        {
            SqlCommand command = connection.GetCommand(String.Format("SELECT * FROM {0} WHERE {1}=@id", tableName, idAttrib));
            command.Parameters.Add("@id", System.Data.SqlDbType.Int, idValue).Value = idValue;
            command.Prepare();

            return command;
        }
        protected static SqlCommand GenerateInsert(DBConnection connection, string tableName, TableRecord record)
        {
            List<DBColumnValue> values = record.GetColumnValuesEscaped();
            string bindedVariables = DatabaseHelper.GenerateBindedVariables(values.Count);

            SqlCommand command = connection.GetCommand(String.Format("INSERT INTO {0} VALUES({1})", tableName, bindedVariables));

            for (int i = 0; i < values.Count; i++)
            {
                command.Parameters.Add(DatabaseHelper.GetBoundVariableByIndex(i), values[i].Attribute.DbType, values[i].Attribute.Size).Value = values[i].Value;
            }
            
            command.Prepare();

            return command;
        }
        protected static SqlCommand GenerateUpdateById(DBConnection connection, string tableName, List<string> columns, List<object> values, string idAttrib, int idValue)
        {
            if (columns.Count != values.Count)
            {
                throw new DatabaseException("Attribute count must match value count");
            }

            int index = 0;
            string valuesStringified = values.Aggregate((object sum, object next) =>
            {
                return sum.ToString() + columns[index++] + "=" + DatabaseHelper.DBEscape(next) + ",";
            }).ToString();

            valuesStringified = valuesStringified.Substring(0, valuesStringified.Length - 1);

            SqlCommand command = connection.GetCommand(String.Format("UPDATE {0} SET {1} WHERE {2}=@id", tableName, valuesStringified, idAttrib));
            command.Parameters.Add("@id", System.Data.SqlDbType.Int, idValue).Value = idValue;
            command.Prepare();

            return command;
        }
        protected static SqlCommand GenerateDeleteById(DBConnection connection, string tableName, string idAttrib, int idValue)
        {
            SqlCommand command = connection.GetCommand(String.Format("DELETE FROM {0} WHERE {1}=@id", tableName, idAttrib));
            command.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = idValue;
            command.Prepare();

            return command;
        }
    }
}
