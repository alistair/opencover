﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Symbols;
using OpenCover.Test.Samples;
using log4net;
using File = OpenCover.Framework.Model.File;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class CecilSymbolManagerTests
    {
        private CecilSymbolManager _reader;
        private string _location;
        private Mock<ICommandLine> _mockCommandLine;
        private Mock<IFilter> _mockFilter;
        private Mock<ILog> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockCommandLine = new Mock<ICommandLine>();
            _mockFilter = new Mock<IFilter>();
            _mockLogger = new Mock<ILog>();
            _location = Path.Combine(Environment.CurrentDirectory, "OpenCover.Test.dll");

            _reader = new CecilSymbolManager(_mockCommandLine.Object, _mockFilter.Object, _mockLogger.Object, null);
            _reader.Initialise(_location, "OpenCover.Test");
        }

        [TearDown]
        public void Teardown()
        {
            //_reader.Dispose();
        }

        [Test]
        public void GetFiles_Returns_AllFiles_InModule()
        {
            //arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var files = _reader.GetFiles();

            //assert
            Assert.NotNull(files);
            Assert.AreNotEqual(0, files.GetLength(0));
        }

        [Test]
        public void GetInstrumentableTypes_Returns_AllTypes_InModule_ThatCanBeInstrumented()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var types = _reader.GetInstrumentableTypes();

            // assert
            Assert.NotNull(types);
            Assert.AreNotEqual(0, types.GetLength(0));
        }

        [Test]
        public void GetInstrumentableTypes_Does_Not_Return_Structs_With_No_Instrumentable_Code()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var types = _reader.GetInstrumentableTypes();

            // assert
            Assert.NotNull(types);
            Assert.IsNull(types.FirstOrDefault(x => x.FullName == typeof(NotCoveredStruct).FullName));
        }

        [Test]
        public void GetInstrumentableTypes_Does_Return_Structs_With_Instrumentable_Code()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var types = _reader.GetInstrumentableTypes();

            // assert
            Assert.NotNull(types);
            Assert.IsNotNull(types.FirstOrDefault(x => x.FullName == typeof(CoveredStruct).FullName));
        }

        [Test]
        public void GetMethodsForType_Returns_AllDeclared_ForType()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredMethodClass).FullName);


            // act
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // assert
            Assert.IsNotNull(methods);
        }

        [Test]
        public void GetSequencePointsForMethodToken()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredMethodClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetSequencePointsForToken(methods[0].MetadataToken);
            // assert

            Assert.IsNotNull(points);
        }

        [Test]
        public void GetBranchPointsForMethodToken_OneBranch()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasSingleDecision")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(2, points.Count());
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(0, points[0].Path);
            Assert.AreEqual(1, points[1].Path);
        }

        [Test]
        public void GetBranchPointsForMethodToken_TwoBranch()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasTwoDecisions")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(4, points.Count());
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(points[2].Offset, points[3].Offset);
        }

        [Test]
        public void GetBranchPointsForMethodToken_Switch()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.First(x => x.FullName == typeof(DeclaredConstructorClass).FullName);
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetBranchPointsForToken(methods.First(x => x.Name.Contains("::HasSwitch")).MetadataToken);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(4, points.Count());
            Assert.AreEqual(points[0].Offset, points[1].Offset);
            Assert.AreEqual(points[0].Offset, points[2].Offset);
            Assert.AreEqual(3, points[3].Path);
        }

        [Test]
        public void GetSequencePointsForToken_HandlesUnknownTokens()
        {
            // arrange

            // act
            var points = _reader.GetSequencePointsForToken(0);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(0, points.Count());

        }

        [Test]
        public void ModulePath_Returns_Name_Of_Module()
        {
            // arrange, act, assert
            Assert.AreEqual(_location, _reader.ModulePath);
        }

        [Test]
        public void SourceAssembly_Returns_Null_On_Failure()
        {
            // arrange
            _reader.Initialise("", "");

            // act
            var val = _reader.SourceAssembly;

            // assert
            Assert.IsNull(val);    
        }

        [Test]
        public void GetComplexityForToken_HandlesUnknownTokens()
        {
            // arrange

            // act
            var complexity = _reader.GetCyclomaticComplexityForToken(0);

            // assert
            Assert.IsNotNull(complexity);
            Assert.AreEqual(0, 0);
        }

        [Test]
        public void GetComplexityForMethodToken_TwoBranch()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(DeclaredConstructorClass).FullName).First();
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var complexity = _reader.GetCyclomaticComplexityForToken(methods.Where(x => x.Name.Contains("::HasTwoDecisions")).First().MetadataToken);

            // assert
            Assert.AreEqual(3, complexity);
        }

        [Test]
        public void AbstractPropertyGetters_AreNotReturned()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(AbstractBase).FullName).First();

            // act
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // assert
            Assert.AreEqual(0, methods.Count(x=>x.IsGetter));
        }

        [Test]
        public void AbstractPropertySetters_AreNotReturned()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(AbstractBase).FullName).First();

            // act
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // assert
            Assert.AreEqual(0, methods.Count(x => x.IsSetter));
        }

        [Test]
        public void AbstractMethods_AreNotReturned()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(AbstractBase).FullName).First();

            // act
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // assert
            Assert.AreEqual(0, methods.Count(x => !x.IsGetter && !x.IsSetter && !x.IsConstructor));
        }

        [Test]
        public void GetSequencePointsFor_OverridePropertyGetter()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var type = types.Where(x => x.FullName == typeof(Concrete).FullName).First();
            var methods = _reader.GetMethodsForType(type, new File[0]);

            // act
            var points = _reader.GetSequencePointsForToken(methods.First(x => x.IsGetter).MetadataToken);

            // assert
            Assert.IsNotNull(points);
#if DEBUG
            Assert.AreEqual(3, points.Count());
#else
            Assert.AreEqual(1, points.Count());
#endif

        }

        [Test]
        public void Can_Exclude_A_Class_By_An_Attribute()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof (Concrete).MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByAttribute(It.Is<ICustomAttributeProvider>(y => y.MetadataToken.ToInt32() == token)))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();

            Assert.True(types.Any());
            Assert.True(types.First(x => x.FullName == typeof(Concrete).FullName).SkippedDueTo == SkippedMethod.Attribute);
        }

        [Test]
        public void Can_Exclude_A_Class_By_An_Filter()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((assemblyName, className) => className != typeof(Concrete).FullName);

            var types = _reader.GetInstrumentableTypes();

            Assert.True(types.Any());
            Assert.True(types.First(x => x.FullName == typeof(Concrete).FullName).SkippedDueTo == SkippedMethod.Filter);
        }

        [Test]
        public void Can_Exclude_A_Property_By_An_Attribute()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof(Concrete).GetMethod("get_Name").MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByAttribute(It.Is<ICustomAttributeProvider>(y => y.MetadataToken.ToInt32() == token)))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var target = types.First(x => x.FullName == typeof(Concrete).FullName);
            var methods = _reader.GetMethodsForType(target, new File[0]);

            Assert.True(methods.Any());
            Assert.True(methods.First(y => y.Name.EndsWith("::get_Name()")).SkippedDueTo == SkippedMethod.Attribute);
        }

        [Test]
        public void Can_Exclude_A_Ctor_By_An_Attribute()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof(Concrete).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null).MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByAttribute(It.Is<ICustomAttributeProvider>(y => y.MetadataToken.ToInt32() == token)))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var target = types.First(x => x.FullName == typeof(Concrete).FullName);
            var methods = _reader.GetMethodsForType(target, new File[0]);

            Assert.True(methods.Any());
            Assert.True(methods.First(y => y.Name.EndsWith("::.ctor()")).SkippedDueTo == SkippedMethod.Attribute);
        }

        [Test]
        public void Can_Exclude_A_Method_By_An_Attribute()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof(Concrete).GetMethod("Method").MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByAttribute(It.Is<ICustomAttributeProvider>(y => y.MetadataToken.ToInt32() == token)))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var target = types.First(x => x.FullName == typeof(Concrete).FullName);
            var methods = _reader.GetMethodsForType(target, new File[0] );

            Assert.True(methods.Count() > 0);
            Assert.True(methods.Where(y => y.Name.EndsWith("::Method()")).First().SkippedDueTo == SkippedMethod.Attribute);
        }

        [Test]
        public void Can_Exclude_A_Method_By_An_FileFilter()
        {
            // arrange
            _mockFilter
                .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var token = typeof(Concrete).GetMethod("Method").MetadataToken;
            _mockFilter
                .Setup(x => x.ExcludeByFile(It.Is<string>(y => !string.IsNullOrWhiteSpace(y) && y.EndsWith("Samples.cs"))))
                .Returns(true);

            var types = _reader.GetInstrumentableTypes();
            var target = types.First(x => x.FullName == typeof(Concrete).FullName);
            var methods = _reader.GetMethodsForType(target, new File[0]);

            Assert.True(methods.Count() > 0);
            Assert.True(methods.Where(y => y.Name.EndsWith("::Method()")).First().SkippedDueTo == SkippedMethod.File);
        }

    }
}