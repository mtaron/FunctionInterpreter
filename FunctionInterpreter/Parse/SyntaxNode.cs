using System.Diagnostics;

namespace FunctionInterpreter.Parse
{
    [DebuggerDisplay("{Type}")]
    internal abstract class SyntaxNode
    {
        protected SyntaxNode(NodeType nodeType)
        {
            Type = nodeType;
        }

        public NodeType Type { get; }
    }
}
