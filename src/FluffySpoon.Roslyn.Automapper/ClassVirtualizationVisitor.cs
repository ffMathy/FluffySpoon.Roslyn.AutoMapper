using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluffySpoon.Roslyn.Automapper
{
    class ClassVirtualizationVisitor : CSharpSyntaxRewriter
    {
        private ICollection<string> _classes;

        public ClassVirtualizationVisitor()
        {
            _classes = new HashSet<string>();
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

            string className = node.Identifier.ValueText;
            _classes.Add(className); // save your visited classes

            return node;
        }
    }
}
