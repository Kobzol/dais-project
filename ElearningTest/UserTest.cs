using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elearning.Database;
using Elearning.Properties;
using Elearning.ORM;

namespace DatabaseTest
{
    [TestClass]
    public class UserTest : BaseTest
    {
        [TestMethod]
        public void TestUserCreation()
        {
            User user = UserTable.Insert(this.connection, 1, "John", "Tester");
            User dbUser = UserTable.GetUserById(this.connection, this.connection.GetLastInsertedId());

            Assert.AreEqual(user.Id, dbUser.Id);
            Assert.AreEqual(user.Role.Id, dbUser.Role.Id);
            Assert.AreEqual(user.Name, dbUser.Name);
            Assert.AreEqual(user.Surname, dbUser.Surname);

            Assert.IsTrue(UserTable.Delete(this.connection, dbUser));

            try
            {
                UserTable.GetUserById(this.connection, user.Id);
                Assert.Fail("Uživatel pořád existuje");
            }
            catch (DatabaseException)
            {

            }
        }

        [TestMethod]
        public void TestUserUpdate()
        {
            User user = UserTable.Insert(this.connection, 1, "John", "Tester");
            user.Name = "Jack";
            user.Surname = "Trader";
            user.Role = RoleTable.GetRoleById(this.connection, 2);

            Assert.IsTrue(UserTable.Update(this.connection, user));

            User changedUser = UserTable.GetUserById(this.connection, user.Id);

            Assert.AreEqual(user.Name, changedUser.Name);
            Assert.AreEqual(user.Surname, changedUser.Surname);
            Assert.AreEqual(user.Role.Id, changedUser.Role.Id);

            Assert.IsTrue(UserTable.Delete(this.connection, changedUser));
        }

        [TestMethod]
        public void TestUserPrivileges()
        {
            User user = UserTable.Insert(this.connection, 2, "John", "Tester");

            Assert.IsTrue(UserTable.CanCreateTests(this.connection, user));

            UserTable.Delete(this.connection, user);
        }
    }
}
