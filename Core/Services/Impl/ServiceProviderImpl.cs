using System;
using System.Collections.Generic;

namespace Snowman.Core.Services.Impl;

/// <summary>
/// Default implementation of service provider.
/// </summary>
internal class ServiceProviderImpl : IServiceProvider
{
    private readonly Dictionary<Type, IService> _services = [];

    public ServiceProviderImpl()
    {
        RegisterDefaultServices();
    }
    
    public void RegisterService<T>(T service) where T : IService
    {
        _services[typeof(T)] = service;
    }
    
    public T GetService<T>() where T : IService
    {
        return (T)_services[typeof(T)];
    }

    private void RegisterDefaultServices()
    {
        // order is important - first services with no dependant services
        RegisterService<IEventManager>(new EventManagerImpl());
        RegisterService<IDrawingService>(new DrawingServiceImpl());
        RegisterService<IDatasetImagesService>(new DatasetImagesServiceImpl(this));
        RegisterService<IEntityManager>(new EntityManagerImpl(this));
    }
}
