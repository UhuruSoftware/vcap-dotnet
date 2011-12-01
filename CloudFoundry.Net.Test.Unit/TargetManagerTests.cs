using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Uhuru.Utilities;
using System.Diagnostics;
using Uhuru.CloudFoundry.UI;


namespace CloudFoundry.Net.Test.Unit
{
    public class TargetManagerTests
    {
        [Test]
        public void TestEncryption()
        {
            CloudTarget target = new CloudTarget("dev", CloudCredentialsEncryption.GetSecureString("mypass"), "api.uhurucloud.net");
            Assert.AreEqual(CloudCredentialsEncryption.GetUnsecureString(target.Password), "mypass");
        }

        [Test]
        public void TestManager()
        {
            CloudTargetManager manager = new CloudTargetManager();

            CloudTarget target = new CloudTarget("dev@uhurucloud.org", 
                CloudCredentialsEncryption.GetSecureString("password1234!"),
                "api.uhurucloud.net");

            manager.SaveTarget(target);

            Assert.IsTrue(manager.GetTargets().Any(t => t.TargetId == target.TargetId));

            manager.RemoveTarget(target);

            Assert.IsFalse(manager.GetTargets().Any(t => t.TargetId == target.TargetId));
        }
    }
}
