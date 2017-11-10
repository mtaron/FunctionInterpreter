using System;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionInterpreter.Test
{
    [TestClass]
    public class IdentifierTests
    {
        [TestMethod]
        public void VariableOnly_LinearFunction()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("x");

            function(0).Should().Be(0);
            function(1).Should().Be(1);
            function(-1).Should().Be(-1);
        }

        [TestMethod]
        public void UnknownIdentifier()
        {
            CompileResult result = InvariantCompiler.Compile("a");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle();
            CompileError error = result.Errors.Single();
            error.Position.Should().Be(0);
            error.Type.Should().Be(ErrorType.UnknownIdentifier);
        }

        [TestMethod]
        public void Pi()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("pi");

            const double expectedValue = Math.PI;
            function(0).Should().Be(expectedValue);
            function(5).Should().Be(expectedValue);
            function(-5).Should().Be(expectedValue);
        }

        [TestMethod]
        public void E()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("e");

            const double expectedValue = Math.E;
            function(0).Should().Be(expectedValue);
            function(0.1).Should().Be(expectedValue);
            function(-0.1).Should().Be(expectedValue);
        }
    }
}
