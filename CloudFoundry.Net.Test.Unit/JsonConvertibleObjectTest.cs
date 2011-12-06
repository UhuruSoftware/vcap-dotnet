// -----------------------------------------------------------------------
// <copyright file="JsonConvertibleObjectTest.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using Uhuru.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CloudFoundry.Net.Test.Unit
{
    /// <summary>
    ///This is a test class for JsonConvertibleObjectTest and is intended
    ///to contain all JsonConvertibleObjectTest Unit Tests
    ///</summary>
    [TestClass()]
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
        public void DeserializeFromJsonTest()
        {
            string json = @"{""bar"":""foo""}";
            testclass tc = new testclass();
            tc.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(json));
            Assert.AreEqual("foo", tc.testfield);
        }

        [TestMethod()]
        public void DeserializeFromArrayJsonTest()
        {
            string json = @"[{""bar"":""foo""},1]";

            object[] objects = JsonConvertibleObject.DeserializeFromJsonArray(json);

            testclass tc = new testclass();
            tc.FromJsonIntermediateObject(objects[0]);
            Assert.AreEqual("foo", tc.testfield);

            int i = JsonConvertibleObject.ObjectToValue<int>(objects[1]);
            Assert.AreEqual(1, i);
        }

        [TestMethod()]
        public void DeserializeFromEnumJsonTest()
        {
            string json = @"{""bar"":""foo"",""blah"":""enum2"",""field3"":""yyy""}";

            testclass tc = new testclass();
            tc.testfield = "asdasd";
            tc.testfield2 = testenum.foo;
            tc.testfield3 = testenum0.xxx;
            tc.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(json));
            Assert.AreEqual("foo", tc.testfield);
            Assert.AreEqual(testenum.bar, tc.testfield2);
            Assert.AreEqual(testenum0.yyy, tc.testfield3);

        }

        [TestMethod()]
        public void SerializeEnumJsonTest()
        {
            string json = @"{""bar"":""foo"",""blah"":""enum2"",""field3"":""yyy""}";

            testclass tc = new testclass();
            tc.testfield = "asdasd";
            tc.testfield2 = testenum.foo;
            tc.testfield3 = testenum0.xxx;
            tc.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(json));
            Assert.AreEqual("foo", tc.testfield);
            Assert.AreEqual(testenum.bar, tc.testfield2);
            Assert.AreEqual(testenum0.yyy, tc.testfield3);

            string json2 = tc.SerializeToJson();

            Assert.AreEqual(json, json2);
        }
    }
}
