using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace PatronageBLStream.Model
{
    public class PhotoModel
    {
        public string Path { get; private set; }
        public string FileName { get; set; }
        public BitmapImage Photo { get; private set; }
        public ImageProperties Properties { get; private set; }
        public StorageFile StorageFile { get; set; }
        public BitmapImage ThumbnailImage { get; set; }

        public PhotoModel(BitmapImage photo, string path)
        {
            Photo = photo;
            Path = path;
        }

        public PhotoModel(BitmapImage photo, string path, BitmapImage thumbnailImage, ImageProperties properties, StorageFile storageFile)
        {
            Photo = photo;
            Path = path;
            ThumbnailImage = thumbnailImage;
            Properties = properties;
            StorageFile = storageFile;
        }

    }
}
