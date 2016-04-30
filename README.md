# SimpleInjector.Integration.Owin

The Simple Injector OWIN Integration package adds a lifestyle to the Simple Injector called 'Per OWIN Request', which allows instances to live within a single OWIN request and get disposed when the request ends.

## How to use

In your OWIN Startup class, register this middleware as early as possible:

```c#
public void Configuration(IAppBuilder app)
{
    app.UseOwinRequestLifestyle();
    // other middlewares...
}
```

So it can detect when the OWIN request ends and dispose the scope.

Then you can register your services:

```c#
container.RegisterPerOwinRequest<IMyService, MyService>();
```

or

```c#
container.Options.DefaultScopedLifestyle = new OwinRequestLifestyle();
container.Register<IMyService, MyService>(Lifestyle.Scoped);
```

## Difference with the official way (Execution Context Scope)...

[The official integration guide](http://simpleinjector.readthedocs.io/en/latest/owinintegration.html) recommends using execution context scope. Well, it seems no problem but when you host your OWIN application on IIS (use Microsoft.Owin.Host.SystemWeb), you'll find sometimes your execution context scope disappears and new service instances are created.

See this SO question:
http://stackoverflow.com/questions/29194836/passing-logical-call-context-from-owin-pipeline-to-webapi-controller

This library try using CallContext to retrive OwinContext, if fails then use HttpContext.Current. So it can properly handle such problem when you host your app on IIS.
