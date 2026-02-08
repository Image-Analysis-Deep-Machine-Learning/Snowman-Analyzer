using Avalonia.Platform.Storage;

namespace Snowman.Core.Services.Impl;

public class StorageProviderServiceImpl : IStorageProviderService
{
    private readonly IStorageProvider _storageProvider;

    public StorageProviderServiceImpl(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }
    
    public IStorageProvider GetStorageProvider()
    {
        return _storageProvider;
    }
}