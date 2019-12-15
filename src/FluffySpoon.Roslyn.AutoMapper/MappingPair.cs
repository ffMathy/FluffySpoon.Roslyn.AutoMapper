using Microsoft.CodeAnalysis;

namespace FluffySpoon.Roslyn.AutoMapper
{
    struct MappingPair
    {
        public ITypeSymbol SourceType { get; set; }
        public ITypeSymbol DestinationType { get; set; }
    }
}