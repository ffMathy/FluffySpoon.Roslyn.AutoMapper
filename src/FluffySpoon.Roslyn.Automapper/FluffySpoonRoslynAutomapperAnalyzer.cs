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
    public class FluffySpoonRoslynAutomapperAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FluffySpoonRoslynAutoMapper";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "CodeQuality";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.InvocationExpression);
            context.RegisterSemanticModelAction(AnalyzeSymbol);
        }

        private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var call = GetMappingCallFromContext(context);
            if (call == null)
                return;

            var hasMappingTypeDefined = IsMappingCallDefined(context, call);
        }

        private bool IsMappingCallDefined(MappingPair? call)
        {

        }

        private static MappingPair? GetMappingCallFromContext(SemanticModelAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            var info = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);

            var method = info.Symbol as IMethodSymbol;
            if (method == null)
                return null;

            if (method.Arity != 1)
                return null;

            if (method.TypeArguments.Length != 1)
                return null;

            var mapper = context.SemanticModel.Compilation.GetTypeByMetadataName("AutoMapper.IMapper");
            if (mapper == null)
                return null;

            var sourceType = method.Parameters
                .Select(x => x.Type)
                .SingleOrDefault();

            var destinationType = method
                .ReturnType;

            if (!method.ReturnType.Equals(destinationType))
                return null;

            var call = new MappingPair()
            {
                SourceType = sourceType,
                DestinationType = destinationType
            };
            return call;
        }

        //private void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        //{
        //    var semanticModel = context.SemanticModel;

        //    var classVisitor = new ClassVirtualizationVisitor();
        //    classVisitor.Visit(semanticModel.SyntaxTree.GetRoot(context.CancellationToken));

        //    var classes = classVisitor.Expressions;
        //}
    }
}
