using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Snowman.Core.Services;

public interface IProjectService : IService
{
    public Task OpenDataset(string file);
    public Task OpenProject(IStorageFile file);
    public Task SaveProject(IStorageFile file);
}
