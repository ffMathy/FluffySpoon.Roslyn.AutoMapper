using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluffySpoon.Roslyn.AutoMapper
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FluffySpoonRoslynAutoMapperAnalyzer : DiagnosticAnalyzer
    {
        public static bool ThrowErrorsOnDiagnostics
        {
            get; set;
        }

        public const string DiagnosticId = "FluffySpoonRoslynAutoMapper";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "CodeQuality";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        private void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            ThrowOnDiagnostics(context);
            AnalyzeMapperCalls(context);
        }

        private static void AnalyzeMapperCalls(SemanticModelAnalysisContext context)
        {
            var mappingCallVisitor = new AutoMapperCallVisitor(context.SemanticModel);
            mappingCallVisitor.Visit(context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken));

            foreach (var mappingCall in mappingCallVisitor.MappingCalls)
            {
                if (mappingCallVisitor.ConfigurationCalls.Contains(mappingCall.Key))
                    continue;

                CreateDiagnosticForMappingCall(context, mappingCall);
            }
        }

        private static void CreateDiagnosticForMappingCall(SemanticModelAnalysisContext context, KeyValuePair<MappingPair, ICollection<Location>> mappingCall)
        {
            foreach (var location in mappingCall.Value)
            {
                var diagnostic = Diagnostic.Create(Rule, location);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void ThrowOnDiagnostics(SemanticModelAnalysisContext context)
        {
            if (!ThrowErrorsOnDiagnostics) 
                return;

            var diagnostics = context.SemanticModel.GetDiagnostics();
            if (diagnostics.Length <= 0)
                return;

            var detailString = diagnostics
                .Select(x => x.ToString())
                .Aggregate("\n", (a, b) => a + "\n" + b);
            throw new InvalidOperationException("The semantic model contains errors." + detailString);
        }
    }
}
