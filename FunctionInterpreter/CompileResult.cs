using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionInterpreter
{
    public class CompileResult
    {
        private readonly SymbolTable _symbols;

        private CompileResult(IReadOnlyList<CompileError> errors)
        {
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
            get { return !Errors.Any(); }
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
