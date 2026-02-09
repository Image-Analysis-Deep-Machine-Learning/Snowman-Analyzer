using System.Collections.Generic;
using Avalonia;
using Avalonia.Media.Imaging;
using Snowman.Data;

namespace Snowman.Core.Services;

public interface IDatasetImagesService : IService
{
    public void NextFrame();
    public void PrevFrame();
    public int CurrentFrameIndex();
    public int MaxFrameIndex();
    public void SkipToFrame(int index);
    public Bitmap ThumbnailAt(int index);
    public void LoadNewImageList(IEnumerable<Image> imageList, string baseFolder);
    public Size GetImageSize();
}
