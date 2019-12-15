﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public ICollection<MappingPair> MappingCalls
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

            MappingCalls = new HashSet<MappingPair>();
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
                MappingCalls.Add(mappingCall.Value);
            }
            else
            {
                var configurationCall = GetConfigurationCallFromContext(node);
                if (configurationCall != null)
                    ConfigurationCalls.Add(configurationCall.Value);
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
    }
}