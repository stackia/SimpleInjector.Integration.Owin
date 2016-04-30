using System;

namespace SimpleInjector.Integration.Owin
{
    internal static class Requires
    {
        internal static void IsNotNull(object instance, string paramName)
        {
            if (instance == null)
                throw new ArgumentNullException(paramName);
        }
    }
}