using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionInterpreter.Test
{
    [TestClass]
    public class LiternalTests
    {
        [DataTestMethod]
        [DataRow(0, DisplayName = "Zero")]
        [DataRow(5, DisplayName = "PositiveInteger")]
        [DataRow(-25, DisplayName = "NegativeInteger")]
        [DataRow(0.756821, DisplayName = "PositiveDecimal")]
        [DataRow(-5789.24579, DisplayName = "NegativeDecimal")]
        public void Constant(double value)
        {
            Func<double, double> function = InvariantCompiler.CompileFunction(value.ToString());

            function(0).Should().Be(value);
            function(-100).Should().Be(value);
            function(100).Should().Be(value);
        }

        [TestMethod]
        public void BeginsWithDecimal()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction(".5");

            const double expectedValue = 0.5;
            function(0).Should().Be(expectedValue);
            function(1).Should().Be(expectedValue);
            function(-1).Should().Be(expectedValue);
        }

        [TestMethod]
        public void EndsWithDecimal()
        {
            CompileResult result= InvariantCompiler.Compile("14.");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle();
            CompileError error = result.Errors.Single();
            error.Position.Should().Be(2);
            error.Type.Should().Be(ErrorType.InvalidNumber);
        }

        [TestMethod]
        public void SingleDecimal()
        {
            CompileResult result = InvariantCompiler.Compile(".");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle();
            CompileError error = result.Errors.Single();
            error.Position.Should().Be(0);
            error.Type.Should().Be(ErrorType.InvalidTerm);
        }

        [TestMethod]
        public void MultipleDecimals()
        {
            CompileResult result = InvariantCompiler.Compile("5.7.11");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle();
            CompileError error = result.Errors.Single();
            error.Position.Should().Be(3);
            error.Type.Should().Be(ErrorType.InvalidNumber);
        }

        [DataTestMethod]
        [DataRow("1E10", DisplayName = "Uppercase")]
        [DataRow("1e10", DisplayName = "Lowercase")]
        [DataRow("1E+10", DisplayName = "UppercaseWithSign")]
        [DataRow("1e+10", DisplayName = "LowercaseWithSign")]
        public void PositiveExponent(string value)
        {
            Func<double, double> function = InvariantCompiler.CompileFunction(value);

            const double expectedValue = 1E10;
            function(0).Should().Be(expectedValue);
            function(500).Should().Be(expectedValue);
            function(-500).Should().Be(expectedValue);
        }

        [TestMethod]
        public void NegativeExponent()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("-4E-10");

            const double expectedValue = -4E-10;
            function(0).Should().Be(expectedValue);
            function(5).Should().Be(expectedValue);
            function(-5).Should().Be(expectedValue);
        }

        [DataTestMethod]
        [DataRow("1e1.1", DisplayName = "Postive")]
        [DataRow("2E-10.35759", DisplayName = "Negative")]
        public void FractionalExponent_Unsupported(string value)
        {
            CompileResult result = InvariantCompiler.Compile(value);
            result.IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        public void CommaDecimalSeperator()
        {
            CompileResult result = Compiler.Compile("2,5", cultureInfo: new CultureInfo("de-DE"));
            result.IsSuccess.Should().BeTrue();

            result.Functions.Single()(0).Should().Be(2.5);
        }
    }
}
