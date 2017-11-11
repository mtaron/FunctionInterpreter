using System;
using System.Collections.Generic;
using System.Linq;

namespace FunctionInterpreter
{
    internal sealed class SymbolTable
    {
        private static readonly IReadOnlyDictionary<string, Func<double, double>> MonadicFunctions = new Dictionary<string, Func<double, double>>
        {
            { "abs", x => Math.Abs(x) },
            { "acos", x => Math.Acos(x) },
            { "asin", x => Math.Asin(x) },
            { "atan", x => Math.Atan(x) },
            { "ceiling", x => Math.Ceiling(x) },
            { "cos", x => Math.Cos(x) },
            { "cosh", x => Math.Cosh(x) },
            { "floor", x => Math.Floor(x) },
            { "log", x => Math.Log(x) },
            { "log10", x => Math.Log10(x) },
            { "round", x => Math.Round(x) },
            { "sin", x => Math.Sin(x) },
            { "sinh", x => Math.Sinh(x) },
            { "sqrt", x => Math.Sqrt(x) },
            { "tan", x => Math.Tan(x) },
            { "tanh", x => Math.Tanh(x) },
        };

        private static readonly IReadOnlyDictionary<string, Func<double, double, double>> DyadicFunctions = new Dictionary<string, Func<double, double, double>>
        {
            { "log", (a, newBase) => Math.Log(a, newBase) },
            { "max", (a, b) => Math.Max(a, b) },
            { "min", (a, b) => Math.Min(a, b) },
        };

        private static readonly IReadOnlyDictionary<string, Func<double, double>> DegreeFunctions = new Dictionary<string, Func<double, double>>
        {
            { "acos", x => Degrees.Acos(x) },
            { "asin", x => Degrees.Asin(x) },
            { "atan", x => Degrees.Atan(x) },
            { "cos", x => Degrees.Cos(x) },
            { "cosh", x => Degrees.Cosh(x) },
            { "sin", x => Degrees.Sin(x) },
            { "sinh", x => Degrees.Sinh(x) },
            { "tan", x => Degrees.Tan(x) },
            { "tanh", x => Degrees.Tanh(x) },
        };

        private readonly Graph<string> _functionGraph = new Graph<string>();
        private readonly Dictionary<string, Func<double, double>> _customFunctions = new Dictionary<string, Func<double, double>>();
        private readonly List<string> _customFunctionOrder = new List<string>();

        public IEnumerable<string> GetDependentFunctions(string name)
        {
            if (!_functionGraph.Contains(name))
            {
                return Enumerable.Empty<string>();
            }

            return _functionGraph.GetClosure(name);
        }

        public double? ResolveIndentifier(string name)
        {
            if (string.Equals(name, "pi", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "\u03C0", StringComparison.OrdinalIgnoreCase))
            {
                return Math.PI;
            }

            if (string.Equals(name, "e", StringComparison.OrdinalIgnoreCase))
            {
                return Math.E;
            }

            return null;
        }

        public Func<double, double> ResolveMonadicFunction(string name, AngleType angleType)
        {
            Func<double, double> function = null;
            if (angleType == AngleType.Degree)
            {
                if (DegreeFunctions.TryGetValue(name, out function))
                {
                    return function;
                }
            }

            if (MonadicFunctions.TryGetValue(name, out function))
            {
                return function;
            }

            return null;
        }

        public void SetFunctionReference(string function, string reference)
        {
            if (string.IsNullOrEmpty(function)
                || string.IsNullOrEmpty(reference)
                || !_functionGraph.Contains(reference)
                || CompilationContext.IsGeneratedFunctionName(reference))
            {
                return;
            }

            _functionGraph.AddEdge(reference, function);
        }

        public void AddCustomFunction(string name, int index)
        {
            _functionGraph.AddNode(name);
            _customFunctionOrder.Insert(index, name);
        }

        public IEnumerable<Func<double, double>> GetCustomFunctions()
        {
            foreach (string function in _customFunctionOrder)
            {
                yield return _customFunctions[function];
            }
        }

        public IEnumerable<string> GetFunctionInCompilationOrder()
        {
            return _functionGraph.TopologicalSort();
        }

        public Func<double, double> ResolveCustomFunction(string name)
        {
            if (!_customFunctions.TryGetValue(name, out Func<double, double> function))
            {
                return null;
            }

            return function;
        }

        public void FinalizeFunction(string name, Func<double, double> function)
        {
            _customFunctions[name] = function;
        }

        public Func<double, double, double> ResolveDyadicFunction(string name)
        {
            if (DyadicFunctions.TryGetValue(name, out Func<double, double, double> function))
            {
                return function;
            }

            return null;
        }

        public bool IsKnownFunction(string functionName)
        {
            return MonadicFunctions.ContainsKey(functionName)
                || DyadicFunctions.ContainsKey(functionName)
                || _functionGraph.Contains(functionName);
        }
    }
}
