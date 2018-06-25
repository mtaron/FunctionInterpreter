using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionInterpreter
{
    /// <summary>
    /// Contains the result of a successful or failed compilation.
    /// </summary>
    public readonly struct CompileResult
    {
        private readonly SymbolTable _symbols;

        private CompileResult(IReadOnlyList<CompileError> errors)
        {
            _symbols = null;
            Errors = errors;
        }

        private CompileResult(SymbolTable symbols)
        {
            _symbols = symbols;
            Errors = Enumerable.Empty<CompileError>();
        }

        internal static CompileResult Create(CompilationContext context)
        {
            if (context.HasErrors)
            {
                return new CompileResult(context.Errors);
            }
            else
            {
                return new CompileResult(context.Symbols);
            }
        }

        public bool IsSuccess
        {
            get => !Errors.Any();
        }

        public IReadOnlyList<Func<double, double>> Functions
        {
            get
            {
                if (_symbols == null)
                {
                    return new Func<double,double>[0];
                }

                return _symbols.GetCustomFunctions().ToList();
            }
        }

        public IEnumerable<CompileError> Errors { get; }

        /// <summary>
        /// Gets all user defined functions that depend on the given function.
        /// </summary>
        /// <example>
        /// <code>
        /// CompileResult result = Compile.Compile(new string[]
        /// {
        ///    "f(x) = x",
        ///    "g(x) = f(x + 2)",
        ///    "h(x) = g(x)/2"
        /// });
        /// string[] dependents = result.GetDependentFunctions("f").ToArray();
        /// </code>
        /// dependents will contain f, g, and h since they would all change if
        /// f changed.
        /// </example>
        public IEnumerable<string> GetDependentFunctions(string functionName)
        {
            if (_symbols == null || string.IsNullOrEmpty(functionName))
            {
                return Enumerable.Empty<string>();
            }

            return _symbols.GetDependentFunctions(functionName);
        }
    }
}
