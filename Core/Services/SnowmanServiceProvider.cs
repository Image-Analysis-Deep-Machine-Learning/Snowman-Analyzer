using System;
using System.Collections.Generic;

namespace Snowman.Core.Services;

/// <summary>
/// Default implementation of service provider domain-specific to MainWindow.
/// </summary>
public class SnowmanServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = [];

    public void RegisterService<T>(T service) where T : notnull
    {
        _services[typeof(T)] = service;
    }
    
    public T GetService<T>()
    {
        return (T)_services[typeof(T)];
    }
}
