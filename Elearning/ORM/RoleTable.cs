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
    public class RoleTable : Table
    {
        public const string TABLE_NAME = "[Role]";

        public static Role Insert(DBConnection connection, string name)
        {
            try
            {
                connection.BeginTransaction();

                SqlCommand command = connection.GetCommand(String.Format("INSERT INTO {0} VALUES(@name)", RoleTable.TABLE_NAME));
                command.AddParameter("@name", SqlDbType.VarChar, 255, name);

                int rows = command.ExecuteNonQuery();
                int id = connection.GetLastInsertedId();

                if (rows > 0)
                {
                    connection.Commit();

                    return new Role(id, name);
                }
                else throw new DatabaseException("Couldn't create role");
            }
            catch (Exception e)
            {
                connection.Rollback();
                throw e;
            }
        }
        public static Role GetRoleById(DBConnection connection, int id)
        {
            SqlCommand command = Table.GenerateSelectAllById(connection, RoleTable.TABLE_NAME, "roleId", id);
            
            using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    Role role = new Role(id, reader.GetColumnValue<string>("name"));

                    return role;
                }
                else throw new DatabaseException(String.Format("There is no role with id {0} in the database.", id));
            }
        }
        public static bool Update(DBConnection connection, Role role)
        {
            ColumnSet columnSet = Role.GetColumnSet(typeof(Role));

            SqlCommand command = connection.GetCommand(String.Format("UPDATE {0} SET name = @name WHERE roleId = @roleId",RoleTable.TABLE_NAME));

            command.AddParameter("@name", SqlDbType.VarChar, 255, role.Name);
            command.AddParameter("@roleId", SqlDbType.Int, role.Id);

            return command.ExecuteNonQuery() > 0;
        }
        public static bool Delete(DBConnection connection, Role role)
        {
            return Table.GenerateDeleteById(connection, RoleTable.TABLE_NAME, "roleId", role.Id).ExecuteNonQuery() > 0;;
        }
    }
}
