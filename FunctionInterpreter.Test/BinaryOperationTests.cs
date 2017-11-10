using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionInterpreter.Test
{
    [TestClass]
    public class BinaryOperationTests
    {
        [DataTestMethod]
        [DataRow("2 + 5", 7, DisplayName = "Integer")]
        [DataRow("0.50 + 7.2865", 7.7865, DisplayName = "Decimal")]
        [DataRow("1+1", 2, DisplayName = "NoWhitespace")]
        [DataRow(" 2+1.1", 3.1, DisplayName = "LeadingWhitespace")]
        [DataRow("789.1+10   ", 799.1, DisplayName = "TrailingWhitespace")]
        [DataRow("1 + 2 + 3 + 4 + 5", 15, DisplayName = "Multiple")]
        [DataRow("((1 + 2)+ 3 + 4) + (5+1)", 16, DisplayName = "Grouped")]
        [DataRow("pi + 0", Math.PI, DisplayName = "Pi")]
        public void ConstantAddition(string expression, double expectedValue)
        {
            Func<double, double> function = InvariantCompiler.CompileFunction(expression);

            function(0).Should().Be(expectedValue);
            function(-100).Should().Be(expectedValue);
            function(100).Should().Be(expectedValue);
        }

        [TestMethod]
        public void ParameterAddition()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("x + 1");

            function(0).Should().Be(1);
            function(1).Should().Be(2);
            function(-1).Should().Be(0);
        }

        [TestMethod]
        public void Exponents_RightAssociative()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("4^3^2");

            const double expectedValue = 262144;
            function(0).Should().Be(expectedValue);
            function(1).Should().Be(expectedValue);
            function(-1).Should().Be(expectedValue);
        }

        [TestMethod]
        public void OrderOfOperations()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("2^3+5*2-6/3");

            const double expectedValue = 16;
            function(0).Should().Be(expectedValue);
            function(100).Should().Be(expectedValue);
            function(-100).Should().Be(expectedValue);
        }

        [DataTestMethod]
        [DataRow("2 * 11", 22, DisplayName = "Integer")]
        [DataRow("1.5 * .2", 0.3, DisplayName = "Decimal")]
        [DataRow("-2.12*2 *-1.01", 4.2824, DisplayName = "MultipleTerms")]
        public void ConstantMultiplication(string expression, double expectedValue)
        {
            Func<double, double> function = InvariantCompiler.CompileFunction(expression);

            const double precision = 10E-6;
            function(0).Should().BeApproximately(expectedValue, precision);
            function(-5).Should().BeApproximately(expectedValue, precision);
            function(5).Should().BeApproximately(expectedValue, precision);
        }

        [DataTestMethod]
        [DataRow("2 x", DisplayName = "Space")]
        [DataRow("2x", DisplayName = "NoSpace")]
        [DataRow("x 2", DisplayName = "ReverseOrderSpace")]
        [DataRow("2.0x", DisplayName = "DecimalNoSpace")]
        [DataRow("2.0  x", DisplayName = "DecimalSpaces")]
        [DataRow("2(x)", DisplayName = "ParensNoSpace")]
        [DataRow("2 ( x )", DisplayName = "ParensWithSpace")]
        [DataRow("(x)2", DisplayName = "ReverseParensNoSpace")]
        [DataRow("(x) 2.0", DisplayName = "ReverseParensWithSpace")]
        public void ImpliedMultiplication_NumberVariable(string expression)
        {
            Func<double, double> function = InvariantCompiler.CompileFunction(expression);

            function(0).Should().Be(0);
            function(1).Should().Be(2);
            function(-1).Should().Be(-2);
        }

        [TestMethod]
        public void ImpliedMultiplication_NumberFunction_NoSpace()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("2sin(x)");

            function(0).Should().Be(0);
        }
    }
}
