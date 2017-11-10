using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionInterpreter.Test
{
    [TestClass]
    public class UnaryOperationTests
    {
        [TestMethod]
        public void UnaryMinus_Variable()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("-x");

            function(0).Should().Be(0);
            function(1).Should().Be(-1);
            function(-1).Should().Be(1);
        }

        [TestMethod]
        public void UnaryMinus_Whitespace()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction(" -  x ");

            function(0).Should().Be(0);
            function(1).Should().Be(-1);
            function(-1).Should().Be(1);
        }

        [TestMethod]
        public void UnaryMinus_Number()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction(" -  5.3 ");

            const double expectedValue = -5.3;
            function(0).Should().Be(expectedValue);
            function(1).Should().Be(expectedValue);
            function(-1).Should().Be(expectedValue);
        }

        [TestMethod]
        public void UnaryOperatorPrecedence_Power()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("-2^x");

            double expectedValue = -1 * Math.Pow(2, 10);
            function(10).Should().Be(expectedValue);
        }

        [TestMethod]
        public void UnaryOperatorPrecedence_Multiplication()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("-x*4+2");

            double expectedValue = -2 * 4 + 2;
            function(2).Should().Be(expectedValue);
        }

        [TestMethod]
        public void UnaryOperatorPrecedence_Addition()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("-x+4+2");

            double expectedValue = -10 + 4 + 2;
            function(10).Should().Be(expectedValue);
        }

        [TestMethod]
        public void NegationSymbol()
        {
            CompileResult result = InvariantCompiler.Compile("-");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle();
            CompileError error = result.Errors.Single();
            error.Position.Should().Be(0);
            error.Type.Should().Be(ErrorType.InvalidTerm);
        }
    }
}
