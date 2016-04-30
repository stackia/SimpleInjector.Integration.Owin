using System;
using System.Runtime.Remoting.Messaging;
using Owin;

namespace SimpleInjector.Integration.Owin
{
    /// <summary>
    /// Extension methods for integrating Simple Injector with OWIN applications.
    /// </summary>
    public static class SimpleInjectorOwinExtensions
    {
        /// <summary>
        /// Enable <see cref="OwinRequestLifestyle"/> for Simple Injector.
        /// This should be registered to OWIN middleware pipeline as early as possible.
        /// </summary>
        /// <param name="app">OWIN app builder.</param>
        public static void UseOwinRequestLifestyle(this IAppBuilder app)
        {
            app.Use(async (context, next) =>
            {
                CallContext.LogicalSetData(OwinContextProvider.OwinContextKey, context);
                var scope = new Scope();
                context.Set(OwinRequestLifestyle.ScopeKey, scope);
                try
                {
                    await next.Invoke();
                }
                finally
                {
                    scope.Dispose();
                }
            });
        }

        /// <summary>
        /// Registers that one instance of <typeparamref name="TConcrete"/> will be returned for every OWIN
        /// request and ensures that -if <typeparamref name="TConcrete"/> implements 
        /// <see cref="IDisposable"/>- this instance will get disposed on the end of the OWIN request. 
        /// 
        /// OWIN application should call <c>UseOwinRequestLifestyle()</c> on OWIN app builder as early as possible to enable this lifestyle.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TConcrete"/> has already been registered.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <typeparamref name="TConcrete"/> is a type
        /// that can not be created by the container.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null
        /// reference.</exception>
        public static void RegisterPerOwinRequest<TConcrete>(this Container container)
            where TConcrete : class
        {
            Requires.IsNotNull(container, nameof(container));

            container.Register<TConcrete, TConcrete>(OwinRequestLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers that one instance of <typeparamref name="TImplementation"/> will be returned for every 
        /// OWIN request every time a <typeparamref name="TService"/> is requested and ensures that -if 
        /// <typeparamref name="TImplementation"/> implements <see cref="IDisposable"/>- this instance 
        /// will get disposed on the end of the OWIN request.
        /// 
        /// OWIN application should call <c>UseOwinRequestLifestyle()</c> on OWIN app builder as early as possible to enable this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve the instances.
        /// </typeparam>
        /// <typeparam name="TImplementation">The concrete type that will be registered.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when an 
        /// the <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <typeparamref name="TImplementation"/> 
        /// type is not a type that can be created by the container.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null
        /// reference.</exception>
        public static void RegisterPerOwinRequest<TService, TImplementation>(this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            Requires.IsNotNull(container, nameof(container));

            container.Register<TService, TImplementation>(OwinRequestLifestyle.WithDisposal);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>
        /// and the returned instance will be reused for the duration of a single OWIN request and ensures that,
        /// if the returned instance implements <see cref="IDisposable"/>, that instance will get
        /// disposed on the end of the OWIN request.
        /// 
        /// OWIN application should call <c>UseOwinRequestLifestyle()</c> on OWIN app builder as early as possible to enable this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="container"/> or <paramref name="instanceCreator"/> are null
        /// references.</exception>
        public static void RegisterPerOwinRequest<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            RegisterPerOwinRequest(container, instanceCreator, true);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <typeparamref name="TService"/>
        /// and the returned instance will be reused for the duration of a single OWIN request and ensures that,
        /// if the returned instance implements <see cref="IDisposable"/>, and
        /// <paramref name="disposeInstanceWhenOwinRequestEnds"/> is set to <b>true</b>, that instance will get
        /// disposed on the end of the OWIN request.
        /// 
        /// OWIN application should call <c>UseOwinRequestLifestyle()</c> on OWIN app builder as early as possible to enable this lifestyle.
        /// </summary>
        /// <typeparam name="TService">The interface or base type that can be used to retrieve instances.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="instanceCreator">The delegate that allows building or creating new instances.</param>
        /// <param name="disposeInstanceWhenOwinRequestEnds">If set to <c>true</c>, the instance will get disposed
        /// when it implements <see cref="IDisposable"/> at the end of the OWIN request.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this container instance is locked and can not be altered, or when the
        /// <typeparamref name="TService"/> has already been registered.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="container"/> or <paramref name="instanceCreator"/> are null
        /// references.</exception>
        public static void RegisterPerOwinRequest<TService>(this Container container,
            Func<TService> instanceCreator, bool disposeInstanceWhenOwinRequestEnds) where TService : class
        {
            Requires.IsNotNull(container, nameof(container));
            Requires.IsNotNull(instanceCreator, nameof(instanceCreator));

            container.Register(instanceCreator,
                disposeInstanceWhenOwinRequestEnds
                    ? OwinRequestLifestyle.WithDisposal
                    : OwinRequestLifestyle.Disposeless);
        }
    }
}