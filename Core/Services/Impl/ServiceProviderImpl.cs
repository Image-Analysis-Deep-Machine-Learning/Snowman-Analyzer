using System;
using System.Collections.Generic;

namespace Snowman.Core.Services.Impl;

/// <summary>
/// Default implementation of service provider domain-specific to MainWindow.
/// </summary>
internal class ServiceProviderImpl : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = [];

    // TODO: let the service provider create the instances?
    public void RegisterService<T>(T service) where T : notnull
    {
        _services[typeof(T)] = service;
    }
    
    public T GetService<T>()
    {
        return (T)_services[typeof(T)];
    }
}
