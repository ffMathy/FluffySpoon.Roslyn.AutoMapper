using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using FluffySpoon.Roslyn.AutoMapper;

namespace FluffySpoon.Roslyn.AutoMapper.Test
{
    [TestClass]
    public class UnitTest : DiagnosticVerifier
    {
        [TestMethod]
        public void MapperCall_NoDiagnosticsShowUp()
        {
            var test = @"
class Program
{   
    static void Main() {
        AutoMapper.IMapper mapper = null;
        mapper.Map<ClassToMapTo>(null);
    }
}

class ClassToMapTo { }";
            VerifyCSharpDiagnostic(test);

            //var expected = new DiagnosticResult
            //{
            //    Id = "FluffySpoonRoslynAutoMapper",
            //    Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
            //    Severity = DiagnosticSeverity.Warning,
            //    Locations =
            //        new[] {
            //                new DiagnosticResultLocation("Test0.cs", 11, 15)
            //            }
            //};

            //VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void NoCode_NoDiagnosticsShowUp()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new FluffySpoonRoslynAutomapperAnalyzer();
        }
    }
}
