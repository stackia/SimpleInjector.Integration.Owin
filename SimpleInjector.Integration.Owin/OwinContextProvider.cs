using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Microsoft.Owin;

namespace SimpleInjector.Integration.Owin
{
    /// <summary>
    ///     <see cref="IOwinContext"/> Provider
    /// </summary>
    public class OwinContextProvider
    {
        internal const string OwinContextKey = "IOwinContext";

        /// <summary>
        ///     Get current <see cref="IOwinContext"/>, or <c>null</c> if no <see cref="IOwinContext"/> found.
        /// </summary>
        public IOwinContext Current => GetCurrentContext();

        internal static IOwinContext GetCurrentContext()
        {
            var context = (IOwinContext) CallContext.LogicalGetData(OwinContextKey);
            if (context != null) return context;
            try
            {
                // If OWIN is hosted with IIS, different middlewares may execute in different IIS stages.
                // CallContext doesn't preserve among different stages, so we have to use HttpContext.Current to retrive OwinContext.
                return new OwinContext((IDictionary<string, object>) HttpContext.Current.Items["owin.Environment"]);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}