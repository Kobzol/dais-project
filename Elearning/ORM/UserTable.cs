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
    public class UserTable : Table
    {
        public const string TABLE_NAME = "[User]";

        private const string SQL_SELECT_ALL_USERS =
             "SELECT [User].userId, [User].name, [User].surname, [User].created_at, Role.roleId, Role.name AS role_name " +
            "FROM [User] " +
            "JOIN Role ON [User].roleId = Role.roleId ";

        private const string SQL_SELECT_USER_BY_ID =
            "SELECT [User].name, [User].surname, [User].created_at, Role.roleId, Role.name AS role_name " +
            "FROM [User] " +
            "JOIN Role ON [User].roleId = Role.roleId " +
            "WHERE [User].userId = @userId";

        public static User Insert(DBConnection connection, Role role, string name, string surname)
        {
            return UserTable.Insert(connection, role.Id, name, surname);
        }
        public static User Insert(DBConnection connection, int roleId, string name, string surname)
        {
            try
            {
                connection.BeginTransaction();

                SqlCommand command = connection.GetCommand(String.Format("INSERT INTO {0}(roleId, name, surname) VALUES(@roleId, @name, @surname)", UserTable.TABLE_NAME));
                command.AddParameter("@roleId", SqlDbType.Int, roleId);
                command.AddParameter("@name", SqlDbType.VarChar, 100, name);
                command.AddParameter("@surname", SqlDbType.VarChar, 100, surname);

                int rows = command.ExecuteNonQuery();
                int id = connection.GetLastInsertedId();

                if (rows > 0)
                {
                    connection.Commit();

                    return new User(id, RoleTable.GetRoleById(connection, roleId), name, surname, DateTime.Now);
                }
                else throw new DatabaseException("Couldn't create user");
            }
            catch (Exception e)
            {
                connection.Rollback();
                throw e;
            }
        }
        public static User GetUserById(DBConnection connection, int userId)
        {
            SqlCommand command = connection.GetCommand(UserTable.SQL_SELECT_USER_BY_ID);
            command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;

            using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    return UserTable.ReadUser(reader, userId);
                }
                else throw new DatabaseException(String.Format("There is no user with id {0} in the database.", userId));
            }
        }
        public static bool Update(DBConnection connection, User user)
        {
            SqlCommand command = connection.GetCommand(String.Format("UPDATE {0} SET roleId = @role, name = @name, surname = @surname WHERE userId = @id", UserTable.TABLE_NAME));

            command.AddParameter("@role", SqlDbType.Int, user.Role.Id);
            command.AddParameter("@name", SqlDbType.VarChar, 100, user.Name);
            command.AddParameter("@surname", SqlDbType.VarChar, 100, user.Surname);
            command.AddParameter("@id", SqlDbType.Int, user.Id);

            return command.ExecuteNonQuery() > 0;
        }
        public static bool Delete(DBConnection connection, User user)
        {
            SqlCommand command = Table.GenerateDeleteById(connection, UserTable.TABLE_NAME, "userId", user.Id);

            return command.ExecuteNonQuery() > 0;
        }

        public static List<User> GetAllUsers(DBConnection connection)
        {
            SqlCommand command = connection.GetCommand(UserTable.SQL_SELECT_ALL_USERS);
            
            using (SqlDataReader reader = command.ExecuteReader())
            {
                List<User> users = new List<User>();

                while (reader.Read())
                {
                    users.Add(UserTable.ReadUser(reader));
                }

                return users;
            }
        }

        public static bool CanCreateTests(DBConnection connection, User user)
        {
            SqlCommand command = connection.GetCommand("dbo.CanCreateTests");
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@p_user_id", SqlDbType.Int).Value = user.Id;
            command.AddReturnValue(SqlDbType.Bit);

            command.ExecuteScalar();

            return command.GetReturnValue<bool>();
        }
        public static bool CanDisplayExamResult(DBConnection connection, User user, int examResultId)
        {
            SqlCommand command = connection.GetCommand("CanViewExamResult");
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@p_user_id", SqlDbType.Int).Value = user.Id;
            command.Parameters.Add("@p_exam_result", SqlDbType.Int).Value = examResultId;
            command.AddReturnValue(SqlDbType.Bit);

            command.ExecuteScalar();

            return command.GetReturnValue<bool>();
        }
        public static int CountExamAttempts(DBConnection connection, User user, int examResultId)
        {
            SqlCommand command = connection.GetCommand("CountExamAttempts");
            command.CommandType = CommandType.StoredProcedure;
            command.AddParameter("@p_exam_id", SqlDbType.Int, examResultId);
            command.AddParameter("@p_user_id", SqlDbType.Int, user.Id);
            command.AddReturnValue(SqlDbType.Int);

            command.ExecuteNonQuery();

            return command.GetReturnValue<int>();
        }

        private static User ReadUser(SqlDataReader reader)
        {
            return UserTable.ReadUser(reader, (int) reader.GetColumnValue<decimal>("userId"));
        }
        private static User ReadUser(SqlDataReader reader, int userId)
        {
            return new User(
                userId,
                new Role(
                            (int) reader.GetColumnValue<decimal>("roleId"),
                            reader.GetColumnValue<string>("role_name")
                ),
                reader.GetColumnValue<string>("name"),
                reader.GetColumnValue<string>("surname"),
                reader.GetColumnValue<DateTime>("created_at")
            );
        }
    }
}
