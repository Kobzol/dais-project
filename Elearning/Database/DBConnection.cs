using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elearning.Database
{
    public class DBConnection : IDisposable
    {
        private readonly string server;
        private readonly string user;
        private readonly string password;

        private SqlConnection connection;
        public SqlConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        private SqlTransaction activeTransaction;

        public DBConnection(string server, string user, string password)
        {
            this.server = server;
            this.user = user;
            this.password = password;
        }

        public void Connect()
        {
            this.Close();

            this.connection = new SqlConnection(this.GenerateConnectionString());
            this.connection.Open();
        }
        public void Close()
        {
            if (this.IsConnected())
            {
                this.connection.Close();
                this.connection.Dispose();
                this.connection = null;
            }
        }
        public bool IsConnected()
        {
            if (this.connection != null)
            {
                return this.connection.State == System.Data.ConnectionState.Open;
            }
            else return false;
        }

        public SqlCommand GetCommand(string commandText)
        {
            if (this.IsTransactionActive())
            {
                return new SqlCommand(commandText, this.connection, this.activeTransaction);
            }
            else return new SqlCommand(commandText, this.connection);
        }

        public SqlTransaction BeginTransaction()
        {
            if (this.IsTransactionActive())
            {
                throw new DatabaseException("A transaction is already active.");
            }

            return this.activeTransaction = this.connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
        }
        public void Commit()
        {
            if (this.IsTransactionActive())
            {
                this.activeTransaction.Commit();
                this.ClearTransaction();
            }
        }
        public void Rollback()
        {
            if (this.IsTransactionActive())
            {
                this.activeTransaction.Rollback();
                this.ClearTransaction();
            }
        }

        public int GetLastInsertedId()
        {
            SqlCommand command = this.GetCommand("SELECT @@IDENTITY");

            return Int32.Parse(command.ExecuteScalar().ToString());
        }

        private void ClearTransaction()
        {
            this.activeTransaction.Dispose();
            this.activeTransaction = null;
        }
        private bool IsTransactionActive()
        {
            return this.activeTransaction != null;
        }
        private string GenerateConnectionString()
        {
            return String.Format("Data Source={0};Persist Security Info=True;User ID={1};Password={2}", this.server, this.user, this.password);
        }

        public void Dispose()
        {
            if (this.IsConnected())
            {
                this.Close();
            }
        }
    }
}
