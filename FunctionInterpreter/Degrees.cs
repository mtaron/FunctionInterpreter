using System;

namespace FunctionInterpreter
{
    internal static class Degrees
    {
        private const double Degree = 0.017453292519943295769236907684886127134428718885417d;

        public static double Acos(double degree)
        {
            return Math.Acos(DegreeToRadian(degree));
        }

        public static double Asin(double degree)
        {
            return Math.Asin(DegreeToRadian(degree));
        }

        public static double Atan(double degree)
        {
            return Math.Atan(DegreeToRadian(degree));
        }

        public static double Cos(double degree)
        {
            return Math.Cos(DegreeToRadian(degree));
        }

        public static double Cosh(double degree)
        {
            return Math.Cosh(DegreeToRadian(degree));
        }

        public static double Sin(double degree)
        {
            return Math.Sin(DegreeToRadian(degree));
        }

        public static double Sinh(double degree)
        {
            return Math.Sinh(DegreeToRadian(degree));
        }

        public static double Tan(double degree)
        {
            return Math.Tan(DegreeToRadian(degree));
        }

        public static double Tanh(double degree)
        {
            return Math.Tanh(DegreeToRadian(degree));
        }

        private static double DegreeToRadian(double degree)
        {
            return degree * Degree;
        }
    }
}
