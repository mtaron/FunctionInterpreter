using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FunctionInterpreter.Parse;
using FunctionInterpreter.Properties;

namespace FunctionInterpreter
{
    internal class CompilationContext
    {
        private const string FunctionPrefix = "_";
        private readonly List<CompileError> _errors = new List<CompileError>();

        public CompilationContext(CultureInfo cultureInfo, AngleType angleType)
        {
            CultureInfo = cultureInfo;
            AngleType = angleType;
        }

        public string CurrentFunctionName { get; set; }

        public CultureInfo CultureInfo { get; }

        public AngleType AngleType { get; }

        public SymbolTable Symbols { get; } = new SymbolTable();

        public IReadOnlyList<CompileError> Errors
        {
            get { return _errors; }
        }

        public bool HasErrors
        {
            get { return _errors.Count > 0; }
        }

        public CompileResult ToResult()
        {
            return CompileResult.Create(this);
        }

        public static bool IsGeneratedFunctionName(string functionName)
        {
            return functionName.StartsWith(FunctionPrefix, StringComparison.CurrentCultureIgnoreCase);
        }

        public IReadOnlyDictionary<string, string> CreateFunctionMap(IEnumerable<string> functions)
        {
            var functionMap = new Dictionary<string, string>();

            int functionIndex = 0;
            foreach (string function in functions)
            {
                if (string.IsNullOrEmpty(function))
                {
                    AddError(ErrorType.ExpressionExpected);
                    break;
                }

                string[] parts = function.Split(new char[] { '=' }, 2);
                int equalsIndex = function.IndexOf('=');
                if (equalsIndex < 0)
                {
                    string functionName = FunctionPrefix + functionIndex.ToString(CultureInfo.InvariantCulture);
                    functionMap.Add(functionName, function);
                    Symbols.AddCustomFunction(functionName, functionIndex);
                }
                else
                {
                    string functionName = ValidateFunctionName(parts[0]);
                    if (string.IsNullOrEmpty(functionName))
                    {
                        break;
                    }

                    functionMap.Add(functionName, parts[1]);
                    Symbols.AddCustomFunction(functionName, functionIndex);
                }

                functionIndex++;
            }

            return functionMap;
        }

        public void SetFunctionReference(string reference)
        {
            Symbols.SetFunctionReference(CurrentFunctionName, reference);
        }

        public Func<double, double> ResolveMonadicFunction(string name)
        {
            return Symbols.ResolveMonadicFunction(name, AngleType)
                ?? Symbols.ResolveCustomFunction(name);
        }

        public void AddError(ErrorType error, TerminalSyntaxNode node)
        {
            int? position = node.Token?.Start;
            _errors.Add(new CompileError(error, position));
        }

        public void AddErrorWithParameter(ErrorType error, TerminalSyntaxNode node)
        {
            int? position = node.Token?.Start;
            string errorMessage = ErrorResources.GetString(error);
            errorMessage = string.Format(CultureInfo.CurrentUICulture, errorMessage, node.ToString());
            var compileError = new CompileError(error, errorMessage, position);
            AddError(new CompileError(error, position));
        }

        public void AddError(ErrorType error, int? position = null)
        {
            _errors.Add(new CompileError(error, position));
        }

        public void AddError(CompileError error)
        {
            _errors.Add(error);
        }

        private string ValidateFunctionName(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                AddError(new CompileError(ErrorType.InvalidFunctionName, Resources.FunctionNameExpected));
                return null;
            }

            functionName = functionName.Trim();

            if (!char.IsLetter(functionName.First()))
            {
                AddError(new CompileError(ErrorType.InvalidFunctionName, Resources.FunctionNameStartError));
                return null;
            }

            int parenIndex = functionName.IndexOf('(');
            if (parenIndex >= 0)
            {
                functionName = functionName.Substring(0, parenIndex);
            }

            if (functionName.Skip(1).Any(c => !char.IsLetterOrDigit(c)))
            {
                AddError(new CompileError(ErrorType.InvalidFunctionName, Resources.FunctionNameCharError));
                return null;
            }

            return functionName;
        }
    }
}
