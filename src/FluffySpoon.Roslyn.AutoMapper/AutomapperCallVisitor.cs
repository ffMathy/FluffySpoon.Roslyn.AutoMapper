using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluffySpoon.Roslyn.AutoMapper
{
    class AutoMapperCallVisitor : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;

        private readonly INamedTypeSymbol _mapperType;
        private readonly INamedTypeSymbol _configurationType;

        public IDictionary<MappingPair, ICollection<Location>> MappingCalls
        {
            get;
        }

        public ICollection<MappingPair> ConfigurationCalls
        {
            get;
        }

        public AutoMapperCallVisitor(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;

            _mapperType = _semanticModel.Compilation.GetTypeByMetadataName("AutoMapper.IMapper");
            _configurationType = _semanticModel.Compilation.GetTypeByMetadataName("AutoMapper.IProfileExpression");

            MappingCalls = new Dictionary<MappingPair, ICollection<Location>>();
            ConfigurationCalls = new HashSet<MappingPair>();
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            node = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);

            if (_mapperType == null || _configurationType == null)
                return node;

            var mappingCall = GetMappingCallFromContext(node);
            if (mappingCall != null)
            {
                if (!MappingCalls.ContainsKey(mappingCall.Value))
                    MappingCalls.Add(mappingCall.Value, new HashSet<Location>());

                MappingCalls[mappingCall.Value].Add(node.GetLocation());
            }
            else
            {
                var configurationCall = GetConfigurationCallFromContext(node);
                if (configurationCall != null)
                {
                    ConfigurationCalls.Add(configurationCall.Value);
                }
            }

            return node;
        }

        private MappingPair? GetConfigurationCallFromContext(InvocationExpressionSyntax invocation)
        {
            var info = _semanticModel.GetSymbolInfo(invocation);

            var method = info.Symbol as IMethodSymbol;
            if (method == null)
                return null;

            if (!method.ContainingType.Equals(_configurationType))
                return null;

            if (method.Name != "CreateMap")
                return null;

            if (method.TypeArguments.Length != 2)
                return null;

            var sourceType = method
                .TypeArguments
                .FirstOrDefault();

            var destinationType = method
                .TypeArguments
                .LastOrDefault();

            var call = new MappingPair()
            {
                SourceType = sourceType,
                DestinationType = destinationType
            };
            return call;
        }

        private MappingPair? GetMappingCallFromContext(InvocationExpressionSyntax invocation)
        {
            var info = _semanticModel.GetSymbolInfo(invocation);

            var method = info.Symbol as IMethodSymbol;
            if (method == null)
                return null;

            if (!method.ContainingType.Equals(_mapperType))
                return null;

            if (method.Name != "Map")
                return null;

            if (method.Arity != 1)
                return null;

            if (method.TypeArguments.Length != 1)
                return null;

            var argument = invocation
                .ArgumentList
                .Arguments
                .Single();
            var argumentSymbolInformation = _semanticModel.GetSymbolInfo(argument.Expression);

            var argumentSymbol = argumentSymbolInformation.Symbol as IMethodSymbol;
            if (argumentSymbol == null)
                return null;

            var sourceType = argumentSymbol.ReceiverType;

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
    }
}
