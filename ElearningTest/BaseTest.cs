using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elearning.Properties;

namespace DatabaseTest
{
    [TestClass]
    public class BaseTest
    {
        protected DBConnection connection;

        [TestInitialize]
        public virtual void Setup()
        {
            this.connection = new DBConnection(Settings.Default.DBServer, Settings.Default.DBLogin, Settings.Default.DBPassword);
            this.connection.Connect(); 
        }

        [TestCleanup]
        public virtual void Teardown()
        {
            this.connection.Close();
        }
    }
}
