using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluffySpoon.Roslyn.AutoMapper
{
    class InvocationExpressionSyntaxVisitor : CSharpSyntaxRewriter
    {
        public ICollection<InvocationExpressionSyntax> Expressions { get; }

        public InvocationExpressionSyntaxVisitor()
        {
            Expressions = new HashSet<InvocationExpressionSyntax>();
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            node = (InvocationExpressionSyntax) base.VisitInvocationExpression(node);
            Expressions.Add(node);

            return node;
        }
    }
}
