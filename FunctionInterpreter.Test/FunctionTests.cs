using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionInterpreter.Test
{
    [TestClass]
    public class FunctionTests
    {
        [TestMethod]
        public void FunctionCallWithVariable_SinFunction()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("sin(x)");

            function(0).Should().Be(0);
            function(0.5 * Math.PI).Should().BeApproximately(1, 1E-15);
            function(Math.PI).Should().BeApproximately(0, 1E-15);
        }

        [TestMethod]
        public void FunctionName_Null()
        {
            CompileResult result = InvariantCompiler.Compile("=x");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle(e => e.Type == ErrorType.InvalidFunctionName);
        }

        [TestMethod]
        public void FunctionName_Empty()
        {
            CompileResult result = InvariantCompiler.Compile(" = x");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle(e => e.Type == ErrorType.InvalidFunctionName);
        }

        [TestMethod]
        public void FunctionName_InvalidStart()
        {
            CompileResult result = InvariantCompiler.Compile("1= x");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle(e => e.Type == ErrorType.InvalidFunctionName);
        }

        [TestMethod]
        public void FunctionName_InvalidChars()
        {
            CompileResult result = InvariantCompiler.Compile("a!= x");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle(e => e.Type == ErrorType.InvalidFunctionName);
        }

        [TestMethod]
        public void CustomFunction()
        {
            var functions = new string[]
            {
                "f(x) = x",
                "g(x) = f(x + 2)"
            };

            CompileResult result = InvariantCompiler.Compile(functions);
            result.IsSuccess.Should().BeTrue();

            result.Functions.Count.Should().Be(2);
            result.Functions[0].Should().NotBeNull();

            Func<double, double> g = result.Functions[1];
            g.Should().NotBeNull();

            g(0).Should().Be(2);
            g(-2).Should().Be(0);
            g(2).Should().Be(4);

            result.Functions[1](2).Should().Be(result.Functions[0](4));
        }

        [TestMethod]
        public void DegreeAngleType_SinFunction()
        {
            CompileResult result = Compiler.Compile("sin(x)", AngleType.Degree, CultureInfo.InvariantCulture);
            result.IsSuccess.Should().BeTrue();

            Func<double, double> function = result.Functions[0];
            function(0).Should().Be(0);
            function(90).Should().BeApproximately(1, 1E-15);
            function(180).Should().BeApproximately(0, 1E-15);
        }

        [TestMethod]
        public void FunctionCallWithMultiplArguments()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("log(x, 2)");

            function(1).Should().Be(0);
        }

        [TestMethod]
        public void FunctionComposition()
        {
            Func<double, double> function = InvariantCompiler.CompileFunction("sin(cos(x))");
            function.Should().NotBeNull();
        }

        [TestMethod]
        public void EmptyFunctionCall_ParseErrors()
        {
            CompileResult result = InvariantCompiler.Compile("tan()");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle(e => e.Type == ErrorType.InvalidTerm);
        }

        [TestMethod]
        public void EmptyFunctionCallWithNegation_ParseErrors()
        {
            CompileResult result = InvariantCompiler.Compile("tan(-)");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle(e => e.Type == ErrorType.InvalidTerm);
        }

        [TestMethod]
        public void CircularReference_ParseErrors()
        {
            CompileResult result = InvariantCompiler.Compile("f = x + f(x)");
            result.IsSuccess.Should().BeFalse();

            result.Errors.Should().ContainSingle(e => e.Type == ErrorType.CyclicFunctions);
        }

        [TestMethod]
        public void LocalizedListSeperator()
        {
            CompileResult result = Compiler.Compile("max( 1,5; 23,000) ", cultureInfo: new CultureInfo("de-DE"));
            result.IsSuccess.Should().BeTrue();

            result.Functions.Single()(0).Should().Be(23);
        }
    }
}
