using System.Globalization;
using FunctionInterpreter.Properties;

namespace FunctionInterpreter
{
    internal static class ErrorResources
    {
        public static string GetString(ErrorType error)
        {
            return Resources.ResourceManager.GetString(error.ToString(), CultureInfo.CurrentUICulture);
        }

        public static string GetString(ErrorType error, string parameter)
        {
            string errorMessage = GetString(error);
            return string.Format(CultureInfo.CurrentUICulture, errorMessage, parameter);
        }
    }
}
