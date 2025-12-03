using System;
using Avalonia.Platform.Storage;

namespace Snowman.Factories;

//TODO: this class might be a good candidate for DI later and/or offer more than a getter method
public static class StorageProviderFactory
{
    private static IStorageProvider? _storageProvider;

    public static IStorageProvider GetStorageProvider()
    {
        if (_storageProvider is null) throw new NullReferenceException(nameof(_storageProvider));
        return _storageProvider;
    }

    public static void InitializeStorageProvider(IStorageProvider storageProvider)
    {
        if (_storageProvider is null) _storageProvider = storageProvider;
    }
}
