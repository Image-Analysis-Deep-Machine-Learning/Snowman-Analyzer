using System;
using System.Collections.Generic;

namespace Snowman.Core.Services.Impl;

/// <summary>
/// Default implementation of service provider.
/// </summary>
internal class ServiceProviderImpl : IServiceProvider
{
    private readonly Dictionary<Type, IService> _services = [];
    
    // TODO: let the service provider create the instances?
    public void RegisterService<T>(T service) where T : IService
    {
        _services[typeof(T)] = service;
    }
    
    public T GetService<T>() where T : IService
    {
        return (T)_services[typeof(T)];
    }
}
