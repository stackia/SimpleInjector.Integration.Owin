using System;

namespace SimpleInjector.Integration.Owin
{
    /// <summary>
    /// Defines a lifestyle that caches instances during the execution of a single OWIN Request.
    /// Unless explicitly stated otherwise, instances created by this lifestyle will be disposed at the end
    /// of the OWIN request.
    /// 
    /// OWIN application should call <c>UseOwinRequestLifestyle()</c> on OWIN app builder as early as possible to enable this lifestyle.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>OwinRequestLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(new OwinRequestLifestyle());
    /// ]]></code>
    /// </example>
    public sealed class OwinRequestLifestyle : ScopedLifestyle
    {
        internal static readonly OwinRequestLifestyle WithDisposal = new OwinRequestLifestyle();

        internal static readonly OwinRequestLifestyle Disposeless = new OwinRequestLifestyle(false);

        internal const string ScopeKey = "OwinRequestScope";

        /// <summary>Initializes a new instance of the <see cref="OwinRequestLifestyle"/> class. The instance
        /// will ensure that created and cached instance will be disposed after the execution of the OWIN
        /// request ended and when the created object implements <see cref="IDisposable"/>.</summary>
        public OwinRequestLifestyle() : this(true)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="OwinRequestLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenOwinRequestEnds">
        /// Specifies whether the created and cached instance will be disposed after the execution of the OWIN
        /// request ended and when the created object implements <see cref="IDisposable"/>. 
        /// </param>
        public OwinRequestLifestyle(bool disposeInstanceWhenOwinRequestEnds)
            : base("OWIN Request", disposeInstanceWhenOwinRequestEnds)
        {
        }

        /// <summary>
        /// Creates a delegate that upon invocation return the current <see cref="T:SimpleInjector.Scope"/> for this
        ///             lifestyle and the given <paramref name="container"/>, or null when the delegate is executed outside
        ///             the context of such scope.
        /// </summary>
        /// <param name="container">The container for which the delegate gets created.</param>
        /// <returns>
        /// A <see cref="T:System.Func`1"/> delegate. This method should never return null.
        /// </returns>
        protected override Func<Scope> CreateCurrentScopeProvider(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            return () => OwinContextProvider.GetCurrentContext()?.Get<Scope>(ScopeKey);
        }
    }
}