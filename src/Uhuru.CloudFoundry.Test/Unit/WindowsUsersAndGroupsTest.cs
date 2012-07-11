using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.Test.Unit
{
    [TestClass]
    public class WindowsUsersAndGroupsTest
    {
        [TestMethod()]
        [TestCategory("Unit")]
        public void BasicTest()
        {
            string rnd = (DateTime.Now.Ticks % 100).ToString();
            string userbase = "UhuruTestUser" + rnd;
            string user1 = userbase + "1";
            string user2 = userbase + "2";
            string groupbase = "UhuruTestGroup" + rnd;
            string group1 = groupbase + "1";
            string group2 = groupbase + "2";


            WindowsUsersAndGroups.CreateUser(user1, "test1234#");
            WindowsUsersAndGroups.CreateUser(user2, "test1234#", "Delete me pls...");
            WindowsUsersAndGroups.DeleteUser(user2);

            WindowsUsersAndGroups.CreateGroup(group1);
            WindowsUsersAndGroups.CreateGroup(group2, "delete me too...");
            WindowsUsersAndGroups.DeleteGroup(group2);

            Assert.IsFalse(WindowsUsersAndGroups.IsUserMemberOfGroup(user1, group1));
            WindowsUsersAndGroups.AddUserToGroup(user1, group1);
            Assert.IsTrue(WindowsUsersAndGroups.IsUserMemberOfGroup(user1, group1));
            WindowsUsersAndGroups.RemoveUserFromGroup(user1, group1);
            Assert.IsFalse(WindowsUsersAndGroups.IsUserMemberOfGroup(user1, group1));

            WindowsUsersAndGroups.DeleteGroup(group1);
            WindowsUsersAndGroups.DeleteUser(user1);
        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void StressTest()
        {
            int n = 200;

            string rnd = (DateTime.Now.Ticks % 100).ToString();
            string userbase = "UTestUser" + rnd;
            string user1 = userbase + "1";
            string user2 = userbase + "2";
            string groupbase = "UTestGroup" + rnd;
            string group1 = groupbase + "1";
            string group2 = groupbase + "2";

            for (int i = 0; i < n; i++)
            {
                WindowsUsersAndGroups.CreateUser(user1 + i.ToString(), "test1234#");
                WindowsUsersAndGroups.CreateUser(user2 + i.ToString(), "test1234#", "Delete me pls...");
                WindowsUsersAndGroups.DeleteUser(user2 + i.ToString());

                WindowsUsersAndGroups.CreateGroup(group1 + i.ToString());
                WindowsUsersAndGroups.CreateGroup(group2 + i.ToString(), "delete me too...");
                WindowsUsersAndGroups.DeleteGroup(group2 + i.ToString());
            }

            for (int i = 0; i < n; i++)
            {
                Assert.IsFalse(WindowsUsersAndGroups.IsUserMemberOfGroup(user1 + i.ToString(), group1 + i.ToString()));
                WindowsUsersAndGroups.AddUserToGroup(user1 + i.ToString(), group1 + i.ToString());
                Assert.IsTrue(WindowsUsersAndGroups.IsUserMemberOfGroup(user1 + i.ToString(), group1 + i.ToString()));
                WindowsUsersAndGroups.RemoveUserFromGroup(user1 + i.ToString(), group1 + i.ToString());
                Assert.IsFalse(WindowsUsersAndGroups.IsUserMemberOfGroup(user1 + i.ToString(), group1 + i.ToString()));
            }

            bool fail = false;
            Exception ex = null;

            for (int i = 0; i < n; i++)
            {
                try
                {
                    WindowsUsersAndGroups.DeleteGroup(group1 + i.ToString());
                }
                catch (Exception e)
                {
                    ex = e;
                    fail = true;
                }

                try
                {
                    WindowsUsersAndGroups.DeleteUser(user1 + i.ToString());
                }
                catch (Exception e)
                {
                    ex = e;
                    fail = true;
                }
            }

            if (fail)
            {
                throw ex;
            }
        }
    }
}
