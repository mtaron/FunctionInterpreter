using System.Collections.Generic;

namespace FunctionInterpreter.Parse
{
    internal class NonterminalSyntaxNode : SyntaxNode
    {
        public NonterminalSyntaxNode(NodeType nodeType)
            : base(nodeType)
        {
            Children = new List<SyntaxNode>();
        }

        public NonterminalSyntaxNode(NodeType nodeType, SyntaxNode child)
            : this(nodeType)
        {
            Children.Add(child);
        }

        public IList<SyntaxNode> Children { get; }
    }
}
