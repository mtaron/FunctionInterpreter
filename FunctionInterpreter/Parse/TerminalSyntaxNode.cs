namespace FunctionInterpreter.Parse
{
    internal class TerminalSyntaxNode : SyntaxNode
    {
        public TerminalSyntaxNode(NodeType nodeType, in Token token)
            : base(nodeType)
        {
            Token = token;
        }

        public Token Token { get; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Token.Text))
            {
                return Token.Text;
            }

            return base.ToString();
        }
    }
}
