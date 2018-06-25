using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using FunctionInterpreter.Parse;

[assembly: InternalsVisibleTo("FunctionInterpreter.Test")]

namespace FunctionInterpreter
{
    /// <summary>
    /// Compiles strings into executable functions.
    /// </summary>
    public static class Compiler
    {
        private const NumberStyles NumberStyle =
            NumberStyles.AllowDecimalPoint
            | NumberStyles.AllowExponent
            | NumberStyles.AllowLeadingWhite
            | NumberStyles.AllowTrailingWhite;

        /// <summary>
        /// Compiles a string into a single variable executable function.
        /// </summary>
        /// <param name="function">A single variable function where 'x' is the variable.</param>
        /// <param name="angleType">The angle type used by trigonometric functions.</param>
        /// <param name="cultureInfo">
        /// Provide culture-specific formatting information. Default value is CultureInfo.CurrentCulture.
        /// </param>
        /// <returns>The executable function, or null if compilation failed.</returns>
        /// <example>
        /// <code>
        /// Func<double, double> sin = Compiler.CompileFunction("sin(x)");
        /// </code>
        /// </example>
        public static Func<double, double> CompileFunction(
            string function,
            AngleType angleType = AngleType.Radian,
            CultureInfo cultureInfo = null)
        {
            CompileResult result = Compile(function, angleType, cultureInfo);
            if (result.IsSuccess)
            {
                return result.Functions[0];
            }

            return null;
        }

        /// <summary>
        /// Compiles a string into a single variable executable function.
        /// </summary>
        /// <param name="function">A single variable function where 'x' is the variable.</param>
        /// <param name="angleType">The angle type used by trigonometric functions.</param>
        /// <param name="cultureInfo">
        /// Provide culture-specific formatting information. Default value is CultureInfo.CurrentCulture.
        /// </param>
        /// <returns>The compilation results.</returns>
        /// <example>
        /// <code>
        /// CompileResult result = Compiler.CompileFunction("sin(x)");
        /// Func<double, double> sin = result.Functions[0];
        /// </code>
        /// </example>
        public static CompileResult Compile(
            string function,
            AngleType angleType = AngleType.Radian,
            CultureInfo cultureInfo = null)
            => Compile(new string[] { function }, angleType, cultureInfo);

        /// <summary>
        /// Compiles strings describing single variable funtions into executable functions.
        /// </summary>
        /// <param name="functions">
        /// An enumerable containing single variable functions.
        /// Functions may reference other functions provided they have been defined using the form "f(x) = ..."
        /// </param>
        /// <param name="angleType">The angle type used by trigonometric functions.</param>
        /// <param name="cultureInfo">
        /// Provide culture-specific formatting information. Default value is CultureInfo.CurrentCulture.
        /// </param>
        /// <returns>The result of the compilation.</returns>
        /// <example>
        /// <code>
        /// CompileResult result = Compile.Compile(new string[]
        /// {
        ///    "f(x) = x",
        ///    "g(x) = f(x + 2)"
        /// });
        /// Func<double, double> f = result.Functions[0];
        /// Func<double, double> g = result.Functions[1];
        /// </code>
        /// </example>
        public static CompileResult Compile(
            IEnumerable<string> functions,
            AngleType angleType = AngleType.Radian,
            CultureInfo cultureInfo = null)
        {
            if (functions == null)
            {
                throw new ArgumentNullException(nameof(functions));
            }

            if (cultureInfo == null)
            {
                cultureInfo = CultureInfo.CurrentCulture;
            }

            var context = new CompilationContext(cultureInfo, angleType);
            IReadOnlyDictionary<string, string> functionMap = context.CreateFunctionMap(functions);
            if (context.HasErrors)
            {
                return context.ToResult();
            }

            var parsedFunctions = new Dictionary<string, SyntaxNode>(functionMap.Count);
            foreach (KeyValuePair<string, string> namedFunction in functionMap)
            {
                string functionName = namedFunction.Key;
                string expression = namedFunction.Value;

                if (string.IsNullOrEmpty(expression))
                {
                    context.AddError(ErrorType.ExpressionExpected);
                    return context.ToResult();
                }

                context.CurrentFunctionName = functionName;
                SyntaxNode root = Parser.Parse(expression, context);
                if (context.HasErrors)
                {
                    return context.ToResult();
                }

                parsedFunctions.Add(functionName, root);
            }

            IEnumerable<string> orderedFunctions = context.Symbols.GetFunctionInCompilationOrder();
            if (orderedFunctions == null)
            {
                context.AddError(new CompileError(ErrorType.CyclicFunctions));
                return context.ToResult();
            }

            foreach (string functionName in orderedFunctions)
            {
                if (!parsedFunctions.TryGetValue(functionName, out SyntaxNode root))
                {
                    string errorMessage = ErrorResources.GetString(ErrorType.UnknownFunction, functionName);
                    context.AddError(new CompileError(ErrorType.UnknownFunction, errorMessage));
                    return context.ToResult();
                }

                Func<double, double> function = Compile(root, context);
                if (context.HasErrors)
                {
                    return context.ToResult();
                }

                context.Symbols.FinalizeFunction(functionName, function);
            }

            return context.ToResult();
        }

        private static Func<double, double> Compile(SyntaxNode node, CompilationContext context)
        {
            switch (node.Type)
            {
                case NodeType.Addition:
                case NodeType.Division:
                case NodeType.Modulus:
                case NodeType.Multiplication:
                case NodeType.Power:
                case NodeType.Subtraction:
                    return CompileBinaryOperation((NonterminalSyntaxNode)node, context);
                case NodeType.FunctionCall:
                    return CompileFunctionCall((NonterminalSyntaxNode)node, context);
                case NodeType.Identifier:
                    return CompileIdentifier((TerminalSyntaxNode)node, context);
                case NodeType.Negation:
                    return CompileUnaryOperation((NonterminalSyntaxNode)node, context);
                case NodeType.Number:
                    return CompileNumber((TerminalSyntaxNode)node, context);
                default:
                    throw new InvalidOperationException($"Unsupported compilation of NodeType {node.Type}");
            }
        }

        private static Func<double, double> CompileBinaryOperation(NonterminalSyntaxNode node, CompilationContext context)
        {
            Func<double, double> left = Compile(node.Children[0], context);
            if (left == null)
            {
                return null;
            }

            Func<double, double> right = Compile(node.Children[1], context);
            if (right == null)
            {
                return null;
            }

            switch (node.Type)
            {
                case NodeType.Addition:
                    return x => left(x) + right(x);
                case NodeType.Division:
                    return x => left(x) / right(x);
                case NodeType.Modulus:
                    return x => left(x) % right(x);
                case NodeType.Multiplication:
                    return x => left(x) * right(x);
                case NodeType.Power:
                    return x => Math.Pow(left(x), right(x));
                case NodeType.Subtraction:
                    return x => left(x) - right(x);
                default:
                    throw new InvalidOperationException($"Unsupported binary operation of NodeType {node.Type}");
            }
        }

        private static Func<double, double> CompileUnaryOperation(NonterminalSyntaxNode node, CompilationContext context)
        {
            Func<double, double> operand = Compile(node.Children[0], context);
            if (operand == null)
            {
                return null;
            }

            return x => -1.0 * operand(x);
        }

        private static Func<double, double> CompileFunctionCall(NonterminalSyntaxNode node, CompilationContext context)
        {
            int childCount = node.Children.Count;
            if (childCount < 2)
            {
                context.AddError(ErrorType.ArgumentExpected);
                return null;
            }

            var functionNode = (TerminalSyntaxNode)node.Children[0];
            string functionName = functionNode.Token.Text;
            if (childCount == 2)
            {
                Func<double, double> monadicFunction = context.ResolveMonadicFunction(functionName);
                if (monadicFunction == null)
                {
                    context.AddErrorWithParameter(ErrorType.UnknownFunction, functionNode);
                    return null;
                }

                Func<double, double> argument = Compile(node.Children[1], context);
                if (argument == null)
                {
                    return null;
                }

                return x => monadicFunction(argument(x));
            }

            if (childCount == 3)
            {
                Func<double, double, double> dyadicFunction = context.Symbols.ResolveDyadicFunction(functionName);
                if (dyadicFunction == null)
                {
                    context.AddErrorWithParameter(ErrorType.UnknownFunction, functionNode);
                    return null;
                }

                Func<double, double> firstArgument = Compile(node.Children[1], context);
                if (firstArgument == null)
                {
                    return null;
                }

                Func<double, double> secondArgument = Compile(node.Children[2], context);
                if (secondArgument == null)
                {
                    return null;
                }

                return x => dyadicFunction(firstArgument(x), secondArgument(x));
            }

            context.AddError(ErrorType.ExcessArguments);
            return null;
        }

        private static Func<double, double> CompileIdentifier(TerminalSyntaxNode node, CompilationContext context)
        {
            string name = node.Token.Text;
            if (string.Equals(name, "x", StringComparison.OrdinalIgnoreCase))
            {
                return x => x;
            }

            double? constant = context.Symbols.ResolveIndentifier(name);
            if (constant == null)
            {
                ErrorType error = context.Symbols.IsKnownFunction(name) ?
                    ErrorType.ParenthesesRequired
                    : ErrorType.UnknownIdentifier;

                context.AddErrorWithParameter(error, node);
                return null;
            }

            return x => constant.Value;
        }

        private static Func<double, double> CompileNumber(TerminalSyntaxNode node, CompilationContext context)
        {
            if (!double.TryParse(node.Token.Text, NumberStyle, context.CultureInfo, out double constant))
            {
                context.AddError(ErrorType.InvalidNumber, node);
                return null;
            }

            return x => constant;
        }
    }
}
