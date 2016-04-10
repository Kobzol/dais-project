using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using Elearning.ORM;
using Elearning.Database;
using Elearning.Properties;

namespace Elearning
{
    class Program
    {
        static void Main(string[] args)
        {
            using (DBConnection connection = new DBConnection(Settings.Default.DBServer, Settings.Default.DBLogin, Settings.Default.DBPassword))
            {
                connection.Connect();

                List<User> users = UserTable.GetAllUsers(connection);

                foreach (User user in users)
                {
                    Console.WriteLine(user.Name);
                }
            }

            Console.ReadLine();
        }
    }
}
