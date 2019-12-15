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

        private readonly ICollection<INamedTypeSymbol> _mapperTypes;
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

            _mapperTypes = new []
            {
                _semanticModel.Compilation.GetTypeByMetadataName("AutoMapper.IMapper"),
                _semanticModel.Compilation.GetTypeByMetadataName("AutoMapper.IRuntimeMapper")
            };

            _configurationType = _semanticModel.Compilation.GetTypeByMetadataName("AutoMapper.IProfileExpression");

            MappingCalls = new Dictionary<MappingPair, ICollection<Location>>();
            ConfigurationCalls = new HashSet<MappingPair>();
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            node = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);

            if (_mapperTypes == null || _configurationType == null)
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

            var classType = method.ContainingType;
            if (!IsMapperType(classType))
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
            
            var sourceType = GetTypeFromSymbol(argumentSymbolInformation.Symbol);
            if (sourceType == null) 
                return null;

            var destinationType = method.ReturnType;

            var call = new MappingPair()
            {
                SourceType = sourceType,
                DestinationType = destinationType
            };

            return call;
        }

        private bool IsMapperType(INamedTypeSymbol classType)
        {
            return
                _mapperTypes.Any(classType.Equals) ||
                classType.AllInterfaces.Any(IsMapperType);
        }

        private static ITypeSymbol GetTypeFromSymbol(ISymbol symbol)
        {
            switch (symbol)
            {
                case IMethodSymbol methodSymbol:
                    return methodSymbol.IsImplicitlyDeclared ?
                        methodSymbol.ReceiverType :
                        methodSymbol.ReturnType;

                case ILocalSymbol localSymbol:
                    return localSymbol.Type;

                case IPropertySymbol propertySymbol:
                    return propertySymbol.Type;

                default:
                    return null;
            }
        }
    }
}
