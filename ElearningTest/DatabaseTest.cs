using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elearning.Database;
using Elearning.Properties;
using Elearning.ORM;

namespace DatabaseTest
{
    [TestClass]
    public class DatabaseTest : BaseTest
    {
        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            this.connection.GetCommand("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Test') DROP TABLE Test").ExecuteNonQuery();
            this.connection.GetCommand("CREATE TABLE Test(id INT IDENTITY, text VARCHAR(255))").ExecuteNonQuery();
        }

        [TestCleanup]
        public override void Teardown()
        {
            this.connection.GetCommand("DROP TABLE Test").ExecuteNonQuery();
            base.Teardown();
        }

        [TestMethod]
        public void TestRollback()
        {
            Assert.AreEqual(0, this.connection.GetCommand("SELECT COUNT(*) FROM Test").ExecuteScalar());
            
            this.connection.BeginTransaction();

            this.connection.GetCommand("INSERT INTO Test VALUES('test')").ExecuteNonQuery();
            Assert.AreEqual(1, this.connection.GetCommand("SELECT COUNT(*) FROM Test").ExecuteScalar());

            this.connection.Rollback();

            Assert.AreEqual(0, this.connection.GetCommand("SELECT COUNT(*) FROM Test").ExecuteScalar());
        }

        [TestMethod]
        public void TestCommit()
        {
            Assert.AreEqual(0, this.connection.GetCommand("SELECT COUNT(*) FROM Test").ExecuteScalar());
            
            this.connection.BeginTransaction();
            this.connection.GetCommand("INSERT INTO Test VALUES('test')").ExecuteNonQuery();
            this.connection.Commit();

            Assert.AreEqual(1, this.connection.GetCommand("SELECT COUNT(*) FROM Test").ExecuteScalar());

            this.connection.GetCommand("DELETE FROM Test").ExecuteNonQuery();
            Assert.AreEqual(0, this.connection.GetCommand("SELECT COUNT(*) FROM Test").ExecuteScalar());
        }
    }
}
