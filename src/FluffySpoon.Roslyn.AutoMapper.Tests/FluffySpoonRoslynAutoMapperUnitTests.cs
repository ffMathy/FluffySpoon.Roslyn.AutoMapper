using System;
using AutoMapper;
using FluffySpoon.Roslyn.AutoMapper.Tests.Helpers;
using FluffySpoon.Roslyn.AutoMapper.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluffySpoon.Roslyn.AutoMapper.Tests
{
    [TestClass]
    public class UnitTest : DiagnosticVerifier
    {
        [TestInitialize]
        public void Initialize()
        {
            FluffySpoonRoslynAutoMapperAnalyzer.ThrowErrorsOnDiagnostics = true;
        }

        [TestMethod]
        public void MapperCall_Configured_NoDiagnosticShowsUp()
        {
            var test = @"
using AutoMapper;
class Program
{   
    static void Main()
    {
        IMapper mapper = new Mapper(new MapperConfiguration(ctx => ctx
            .CreateMap<ClassToMapFrom, ClassToMapTo>()));

        mapper.Map<ClassToMapTo>(new ClassToMapFrom());
    }
}

class ClassToMapTo { }

class ClassToMapFrom { }
".Trim();

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void MapperCall_ConfiguredButWithWrongMappingCall_DiagnosticShowsUp()
        {
            var test = @"
using AutoMapper;
class Program
{   
    static void Main()
    {
        IMapper mapper = new Mapper(new MapperConfiguration(ctx => ctx
            .CreateMap<ClassToMapFromUnknown, ClassToMapTo>()));

        mapper.Map<ClassToMapTo>(new ClassToMapFrom());
    }
}

class ClassToMapTo { }

class ClassToMapFrom { }

class ClassToMapFromUnknown { }
".Trim();

            var expected = new DiagnosticResult
            {
                Id = "FluffySpoonRoslynAutoMapper",
                Message = "This particular AutoMapper mapping combination was not configured anywhere.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 9, 9)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MapperCall_NotConfigured_DiagnosticShowsUp()
        {
            var test = @"
using AutoMapper;
class Program
{   
    static void Main() {
        IMapper mapper = null;
        mapper.Map<ClassToMapTo>(new ClassToMapFrom());
    }
}

class ClassToMapTo { }

class ClassToMapFrom { }
".Trim();

            var expected = new DiagnosticResult
            {
                Id = "FluffySpoonRoslynAutoMapper",
                Message = "This particular AutoMapper mapping combination was not configured anywhere.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 6, 9)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MapperCall_NotImported_NoDiagnosticShowsUp()
        {
            FluffySpoonRoslynAutoMapperAnalyzer.ThrowErrorsOnDiagnostics = false;

            var test = @"
class Program
{   
    static void Main() {
        IMapper mapper = null;
        mapper.Map<ClassToMapTo>(new ClassToMapFrom());
    }
}

class ClassToMapTo { }

class ClassToMapFrom { }
".Trim();

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void NoCode_NoDiagnosticsShowUp()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new FluffySpoonRoslynAutoMapperAnalyzer();
        }
    }
}
