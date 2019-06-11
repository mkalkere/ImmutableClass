﻿using ImmutableClassLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImmutableClassLibraryTests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class Tests
    {
        [Test]
        public void TestCreatePerson()
        {
            var immutablePerson =
                ImmutableClass.Create<Person>
                (
                    "{\"FirstName\":\"Mallikh\"," +
                    "\"LastName\":\"Kalkere\"}"
                );

            //OR

            immutablePerson =
                ImmutableClass.Create<Person>
                (
                    new Person() { FirstName = "Mallikh", LastName = "Kalkere" }
                );
        }

        [Test]
        public void CanCreatePersonStruct()
        {
            var sut = new PersonX() { FirstName = "Mallikh", LastName = "Kalkere" };
            sut.Items = new List<string>();
            sut.FirstName = "mallikh";
            sut.Items.Add("Structs are not immutable!");
        }

        [Test]
        public void CanCreateSimpleImmutableClass()
        {
            var sut = new SimpleImmutableClass("Mallikh", "Kalkere", new List<string>());
            sut.Items.Add("BCA");
            sut.Items.Add("MCA");
            sut.Items.Add("MBA");
        }

        [Test]
        public void CanGetToken()
        {
            var token = ImmutableClass.Create<ImmutableTest>(
                JsonConvert.SerializeObject(
                    new { FirstName = "Mallikh", LastName = "Kalkere" }))
                .ToString().Substring(2, 32);

            Regex.Match(@"^[{(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$", token);
        }

        [Test]
        public void VerifyFromAnonymousObject()
        {
            var obj = new { FirstName = "Mallikh", LastName = "Kalkere" };
            var json = JsonConvert.SerializeObject(obj);
            var sut = ImmutableClass.Create<ImmutableTest>(json);
        }

        [Test]
        public void VerifyRoundTripWithNoException()
        {
            var json =
                "{\"FirstName\":\"John\",\"LastName\":\"Petersen\",\"Schools\":{\"MBA\":{\"Institution\":\"St. Joseph\'s University\",\"Year\":\"1993\",\"Degree\":\"MBA\"},\"JD\":{\"Institution\":\"Rutgers University School of Law\",\"Year\":\"2004\",\"Degree\":\"JD\"},\"BS\":{\"Institution\":\"Mansfield University\",\"Year\":\"1988\",\"Degree\":\"BS\"}}}";

            var sut = ImmutableClass.Create<ImmutableTest>(json);

            json = sut.ToString();
            var token = json.Substring(2, 36);

            var jObject = JObject.Parse(json);

            var jx = jObject.ToString();

            jObject[token]["FirstName"] = "JOHN";

            jObject[token]["Schools"]["MBA"]["Degree"] = "M.B.A.";

            json = jObject.ToString();


            json = json.Substring(45, (json.Length - 46));


            sut = ImmutableClass.Create<ImmutableTest>(json);

            token = sut.ToString().Substring(2, 36);
            Assert.AreEqual("M.B.A.", sut.Schools["MBA"].Degree);
        }

        [Test]
        public void CanDeserializeNewInstanceWithUpdatedFirstName()
        {
            var expected = "JOHN";

            var json =
                "{\"FirstName\":\"John\",\"LastName\":\"Petersen\",\"Schools\":{\"MBA\":{\"Institution\":\"St. Joseph\'s University\",\"Year\":\"1993\",\"Degree\":\"MBA\"},\"JD\":{\"Institution\":\"Rutgers University School of Law\",\"Year\":\"2004\",\"Degree\":\"JD\"},\"BS\":{\"Institution\":\"Mansfield University\",\"Year\":\"1988\",\"Degree\":\"BS\"}}}";


            var sut = ImmutableClass.Create<ImmutableTest>(json.Replace("John", expected));


            Assert.AreEqual(expected, sut.FirstName);
        }

        [Test]
        public void VerifyJsonToString()
        {
            var sut = ImmutableClass.Create<ImmutableTest>("{\"FirstName\":\"John\",\"LastName\":\"Petersen\"}");
        }

        [Test]
        public void VerifyStrictCreate()
        {
            var expected = "An immutable object can only be created via the static Create<T> method.";

            var exception = Assert.Throws<ImmutableObjectInvalidCreationException>(() =>
            {
                var sut = new Person(true);
            });

            Assert.AreEqual(expected, exception.Message);
        }

        [Test]
        public void AttemptDictionaryImmutablePropertyWithExceptionTrueThrowsException()
        {
            var expected = "An immutable object cannot be changed after it has been created.";

            var exception = Assert.Throws<ImmutableObjectEditException>(() =>
            {
                var obj = new
                {
                    FirstName = "John",
                    LastName = "Petersen"
                };

                var sut = ImmutableClass.Create<ImmutableTest>(JsonConvert.SerializeObject(obj));
                sut.FirstName = "FOO";
            });

            Assert.AreEqual(expected, exception.Message);
        }

        [Test]
        public void AttemptToDefineInvalidPropertyTypeThrowsException()
        {
            var expected =
                "Properties of an instance of ImmutableClass may only contain the following types: Boolean, Byte, SByte, Char, Decimal, Double, Single, Int32, UInt32, Int64, UInt64, Int16, UInt16, String, ImmutableArray, ImmutableDictionary, ImmutableList, ImmutableQueue, ImmutableSortedSet, ImmutableStack or ImmutableClass. Invalid property types:    List";


            var exception = Assert.Throws<InvalidDataTypeException>(
                () => { ImmutableClass.Create(new InvalidImmutableTestDefintion()); }
            );

            Assert.AreEqual(expected, exception.Message);
            Console.WriteLine();
        }
        public class InvalidImmutableTestDefintion : ImmutableClass
        {
            public string FirstName { get; set; }
            public List<string> InvalidProperty { get; set; }
        }

        [Test]
        public void TestCreatePerson2()
        {
            var json =
                "{\"FirstName\":\"John\",\"LastName\":\"Petersen\",\"Address\":{},\"Schools\":[\"Mansfield\",\"St. Joseph's\",\"Rutgers\"]}";

            var person = ImmutableClass.Create<Person>(json);

            Assert.AreEqual("John", person.FirstName);
        }
    }

    public class Person : ImmutableClass
    {
        public Person() : base(false)
        {
        }

        public Person(bool strictCreate) : base(strictCreate)
        {
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set => Setter(
                MethodBase
                .GetCurrentMethod()
                .Name
                .Substring(4),
                value,
                ref _firstName);
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set => Setter(
                MethodBase
                .GetCurrentMethod()
                .Name
                .Substring(4),
                value,
                ref _lastName);
        }

        private ImmutableArray<string> _schools;
        public ImmutableArray<string> Schools
        {
            get => _schools;
            set => Setter(
                MethodBase
                .GetCurrentMethod()
                .Name
                .Substring(4),
                value,
                ref _schools);
        }

    }

    [ExcludeFromCodeCoverage]
    public struct PersonX
    {
        public string FirstName;
        public string LastName;
        public List<string> Items;
    }

    [ExcludeFromCodeCoverage]
    public class SimpleImmutableClass
    {
        //Private setters are implicit
        public string FirstName { get; }
        public string LastName { get; }
        public List<string> Items { get; }
        public SimpleImmutableClass(string firstName, string lastName, List<string> items)
        {
            FirstName = firstName;
            LastName = lastName;
            Items = items;
        }
    }
    public class ImmutableTest : ImmutableClass
    {
        private string _firstName;

        public string FirstName
        {
            get => _firstName;
            set => Setter(MethodBase.GetCurrentMethod().Name.Substring(4), value, ref _firstName);
        }

        private string _lastName;

        public string LastName
        {
            get => _lastName;
            set => Setter(MethodBase.GetCurrentMethod().Name.Substring(4), value, ref _lastName);
        }

        private ImmutableDictionary<string, School> _schools;

        public ImmutableDictionary<string, School> Schools
        {
            get => _schools;
            set => Setter(MethodBase.GetCurrentMethod().Name.Substring(4), value, ref _schools);
        }
    }
    public class School : ImmutableClass
    {
        private string _institution;

        public string Institution
        {
            get => _institution;
            set => Setter(MethodBase.GetCurrentMethod().Name.Substring(4), value, ref _institution);
        }

        private string _year;

        public string Year
        {
            get => _year;
            set => Setter(MethodBase.GetCurrentMethod().Name.Substring(4), value, ref _year);
        }

        private string _degree;

        public string Degree
        {
            get => _degree;
            set => Setter(MethodBase.GetCurrentMethod().Name.Substring(4), value, ref _degree);
        }
    }
}
