using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.Test.Unit
{
    [TestClass]
    public class JsonConvertibleObjectTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        private enum testenum0
        {
            xxx,
            yyy
        }

        private enum testenum
        {
            [JsonName("enum1")]
            foo,
            [JsonName("enum2")]
            bar
        }

        private class testclass : JsonConvertibleObject
        {
            [JsonName("bar")]
            public string testfield;

            [JsonName("blah")]
            public testenum testfield2;

            [JsonName("field3")]
            public testenum0 testfield3;
        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void TC001_DeserializeFromJsonTest()
        {
            //Arrange
            string json = @"{""bar"":""foo""}";
            testclass tc = new testclass();
            //Act
            tc.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(json));
            //Assert
            Assert.AreEqual("foo", tc.testfield);
        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void TC002_DeserializeFromArrayJsonTest()
        {
            //Arrange
            string json = @"[{""bar"":""foo""},1]";
            object[] objects = JsonConvertibleObject.DeserializeFromJsonArray(json);
            testclass tc = new testclass();

            //Act
            tc.FromJsonIntermediateObject(objects[0]);
            Assert.AreEqual("foo", tc.testfield);
            int i = JsonConvertibleObject.ObjectToValue<int>(objects[1]);

            //Assert
            Assert.AreEqual(1, i);
        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void TC003_DeserializeFromEnumJsonTest()
        {
            //Arrange
            string json = @"{""bar"":""foo"",""blah"":""enum2"",""field3"":""yyy""}";
            testclass tc = new testclass();

            //Act
            tc.testfield = "asdasd";
            tc.testfield2 = testenum.foo;
            tc.testfield3 = testenum0.xxx;
            tc.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(json));

            //Assert
            Assert.AreEqual("foo", tc.testfield);
            Assert.AreEqual(testenum.bar, tc.testfield2);
            Assert.AreEqual(testenum0.yyy, tc.testfield3);

        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void TC004_SerializeEnumJsonTest()
        {
            //Arrange
            string json = @"{""bar"":""foo"",""blah"":""enum2"",""field3"":""yyy""}";
            testclass tc = new testclass();

            //Act
            tc.testfield = "asdasd";
            tc.testfield2 = testenum.foo;
            tc.testfield3 = testenum0.xxx;
            tc.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(json));
            Assert.AreEqual("foo", tc.testfield);
            Assert.AreEqual(testenum.bar, tc.testfield2);
            Assert.AreEqual(testenum0.yyy, tc.testfield3);

            //Assert
            string json2 = tc.SerializeToJson();
            Assert.AreEqual(json, json2);
        }
    }
}
