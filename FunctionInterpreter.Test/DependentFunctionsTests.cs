using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionInterpreter.Test
{
    [TestClass]
    public class DependentFunctionsTests
    {
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("f")]
        public void Errors_NoDependentFunctions(string functionName)
        {
            CompileResult result = Compiler.Compile("////");
            IEnumerable<string> functions = result.GetDependentFunctions(functionName);
            functions.Should().BeEmpty();
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("f")]
        [DataRow("sin")]
        public void SingleUnnamedFunction_NoDependentFunctions(string functionName)
        {
            CompileResult result = Compiler.Compile("sin(x)");
            IEnumerable<string> functions = result.GetDependentFunctions(functionName);
            functions.Should().BeEmpty();
        }

        [TestMethod]
        public void GetDependentFunctions_SingleNamedFunction()
        {
            CompileResult result = Compiler.Compile("f(x) = sin(x)");
            result.GetDependentFunctions("f").Should().ContainSingle(s => s == "f");
            result.GetDependentFunctions("sin").Should().BeEmpty();
        }

        [TestMethod]
        public void GetDependentFunctions_SimpleGraph()
        {
            var functions = new string[]
            {
                "f(x) = x",
                "g(x) = f(x) + f(x) + 2",
                "h(x) = g(x) / 2"
            };

            CompileResult result = InvariantCompiler.Compile(functions);
            result.IsSuccess.Should().BeTrue();

            string[] dependents = result.GetDependentFunctions("f").ToArray();
            dependents.Should().Contain(new string[] { "f", "g", "h" });
        }
    }
}
