using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;

namespace FunctionInterpreter.Test
{
    public static class InvariantCompiler
    {
        public static CompileResult Compile(string expression)
        {
            return Compiler.Compile(expression, cultureInfo: CultureInfo.InvariantCulture);
        }

        public static Func<double, double> CompileFunction(string expression)
        {
            CompileResult result = Compile(expression);
            result.IsSuccess.Should().BeTrue();
            return result.Functions.Single();
        }

        public static CompileResult Compile(IEnumerable<string> functions)
        {
            return Compiler.Compile(functions, cultureInfo: CultureInfo.InvariantCulture);
        }
    }
}
