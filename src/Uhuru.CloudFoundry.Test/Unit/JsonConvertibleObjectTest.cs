using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.Utilities;
using Uhuru.Utilities.Json;

namespace Uhuru.CloudFoundry.Test.Unit
{
    [TestClass]
    public class JsonConvertibleObjectTest
    {

    /// <summary>
    ///This is a test class for JsonConvertibleObjectTest and is intended
    ///to contain all JsonConvertibleObjectTest Unit Tests
    ///</summary>


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
            bar,
            [JsonName("enum3")]
            foo1,
            [JsonName("enum4")]
            bar2
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
        public void DeserializeFromJsonTest()
        {
            string json = @"{""bar"":""foo""}";
            testclass tc = new testclass();
            tc.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(json));
            Assert.AreEqual("foo", tc.testfield);
        }

        [TestMethod()]
        [TestCategory("Unit")]
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
        [TestCategory("Unit")]
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
        [TestCategory("Unit")]
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


        private class EnumHash : JsonConvertibleObject
        {
            [JsonName("foo")]
            public HashSet<testenum> foo = null;
        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void DeserializeEnumHashsetJsonTest()
        {
            testenum asd = JsonConvertibleObject.ObjectToValue<testenum>("enum4");
            Assert.AreEqual(testenum.bar2, asd);
        }

        private class EnumHashNullable : JsonConvertibleObject
        {
            [JsonName("foo")]
            public testenum? foo;
        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void DeserializeEnumNullableJsonTest()
        {
            EnumHashNullable nullable = new EnumHashNullable();
            nullable.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(@"{""foo"":""enum4""}"));

            Assert.AreEqual(testenum.bar2, nullable.foo);


            EnumHashNullable nullable2 = new EnumHashNullable();
            nullable.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(@"{}"));

            Assert.AreEqual(null, nullable2.foo);

        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void SerializeEnumNullableJsonTest()
        {
            EnumHashNullable nullable = new EnumHashNullable();
            nullable.foo = testenum.bar2;
            Assert.AreEqual(@"{""foo"":""enum4""}", nullable.SerializeToJson());

            EnumHashNullable nullable2 = new EnumHashNullable();
            Assert.AreEqual(@"{}", nullable2.SerializeToJson());

        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void SerializeDEAHeatbeatJsonTest()
        {
            // Arrange
            Uhuru.CloudFoundry.DEA.Messages.HeartbeatMessage message = new DEA.Messages.HeartbeatMessage();

            // Act
            string serializedJson = message.SerializeToJson();

            // Assert
            Assert.AreEqual(@"{""droplets"":[]}", serializedJson);
        }


        class parenttest : JsonConvertibleObject
        {
            public class childtest : JsonConvertibleObject
            {
                [JsonName("x")]
                public int x;
            }

            [JsonName("v1")]
            public int v1;

            [JsonName("nv1")]
            public int? nv1;

            [JsonName("nv2")]
            public int? nv2;

            [JsonName("child")]
            public childtest ct;

            [JsonName("child2")]
            public childtest ct2;
        }

        [TestMethod()]
        [TestCategory("Unit")]
        public void SerializeComplicatedJsonTest()
        {
            //Arrange

            parenttest a = new parenttest();
            a.v1 = 1;
            a.nv1 = 1;
            a.nv2 = null;
            a.ct = new parenttest.childtest();
            a.ct.x = 1;
            a.ct2 = null;

            //Act
            string jString = a.SerializeToJson();


            //Assert
            Assert.AreEqual(@"{""v1"":1,""nv1"":1,""child"":{""x"":1}}", jString);
        }


        [TestMethod()]
        [TestCategory("Unit")]
        public void DeserializeComplicatedJsonTest()
        {
            //Arrange
            string jString = @"{""v1"":1,""nv1"":1,""nv2"":null,""child"":{""x"":1}}";

            //Act
            parenttest a = new parenttest();
            a.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(jString));

            //Assert
            Assert.AreEqual(a.v1, 1);
            Assert.AreEqual(a.nv1, 1);
            Assert.AreEqual(a.nv2, null);
            Assert.AreNotEqual(a.ct, null);
            Assert.AreEqual(a.ct.x, 1);
            Assert.AreEqual(a.ct2, null);

        }

    }
}
