using Avalonia.Platform.Storage;

namespace Snowman.Core.Services;

public interface IStorageProviderService : IService
{
    public IStorageProvider GetStorageProvider();
}
