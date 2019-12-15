using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluffySpoon.Roslyn.AutoMapper
{
    struct MappingPair
    {
        public ITypeSymbol SourceType { get; set; }
        public ITypeSymbol DestinationType { get; set; }
    }
}