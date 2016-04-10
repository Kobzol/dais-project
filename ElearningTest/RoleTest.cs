using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elearning.Database;
using Elearning.Properties;
using Elearning.ORM;

namespace DatabaseTest
{
    [TestClass]
    public class RoleTest : BaseTest
    {
        [TestMethod]
        public void TestRoleCreation()
        {
            Role role = RoleTable.Insert(this.connection, "test");
            Role other = RoleTable.GetRoleById(this.connection, role.Id);

            Assert.AreEqual(role.Name, other.Name, "Jména rolí se neshodují");

            Assert.IsTrue(RoleTable.Delete(this.connection, role));

            try
            {
                RoleTable.GetRoleById(this.connection, role.Id);
                Assert.Fail("Role pořád existuje");
            }
            catch (DatabaseException)
            {

            }
        }

        [TestMethod]
        public void TestRoleUpdate()
        {
            Role role = RoleTable.Insert(this.connection, "test");
            Assert.AreEqual("test", role.Name);

            role.Name = "test2";

            Assert.IsTrue(RoleTable.Update(this.connection, role));

            role = RoleTable.GetRoleById(this.connection, role.Id);

            Assert.AreEqual("test2", role.Name);

            Assert.IsTrue(RoleTable.Delete(this.connection, role));
        }

        [TestMethod]
        public void TestRoleSelection()
        {
            Role role = RoleTable.GetRoleById(this.connection, 1);

            Assert.AreEqual("admin", role.Name);
        }  
    }
}
