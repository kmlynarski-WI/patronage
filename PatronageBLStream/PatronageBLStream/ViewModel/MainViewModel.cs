using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace PatronageBLStream.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private int _bitmapCount;
        private readonly List<string> _path; 
        private BitmapImage _bitmapImageSource;
        private readonly List<BitmapImage> _bitmapList;
        private readonly List<ImageProperties> _imagePropertieses;
        private string _imagePropertiesesText;

        public MainViewModel()
        {
            _bitmapCount = 0;
            _path = new List<string>();
            _bitmapList = new List<BitmapImage>();
            _imagePropertieses = new List<ImageProperties>();
            GetNextImageCommand = new RelayCommand(GetNextImage);

            InitializeGallery();
        }

        private async void InitializeGallery() 
        {
            try
            {
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery,
                    new[] {".jpeg", ".jpg", ".png", ".gif"}) {FolderDepth = FolderDepth.Deep};
                var query = Windows.Storage.KnownFolders.PicturesLibrary.CreateFileQueryWithOptions(queryOptions);
                var files = await query.GetFilesAsync();

                if (files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        var stream = await file.OpenReadAsync();
                        var bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(stream);
                        _bitmapList.Add(bitmapImage);
                        _path.Add(file.Path);

                        Windows.Storage.FileProperties.ImageProperties properties = await GetImagePropertiesAsync((StorageFile)file);

                    }
                }

                if (_bitmapList.Count >= 1)
                {
                    BitmapImageSource = _bitmapList[_bitmapCount];
                    ImagePropertiesesText = GetImageDetails(_imagePropertieses[_bitmapCount]);
                }
            }
            catch (Exception ex)
            {
                //MessageDialog message = new MessageDialog(ex.Message);
                //await message.ShowAsync();
            }
        }

        public BitmapImage BitmapImageSource
        {
            get
            {
                return _bitmapImageSource;
            }
            private set
            {
                if (_bitmapImageSource == value) return;
                _bitmapImageSource = value;
                RaisePropertyChanged("BitmapImageSource");
            }
        }

        public string ImagePropertiesesText
        {
            get { return _imagePropertiesesText;}
            private set
            {
                if (_imagePropertiesesText == value) return;
                _imagePropertiesesText = value;
                RaisePropertyChanged("ImagePropertiesesText");
            }
        }

        public ImageProperties Properties
        {
            get; private set;
        }

        private async Task<ImageProperties> GetImagePropertiesAsync(Windows.Storage.StorageFile file)
        {
            ImageProperties properties = await file.Properties.GetImagePropertiesAsync();
            _imagePropertieses.Add(properties);
            return properties;
        } 

        private static string GetImageDetails(ImageProperties properties)
        {
            var data = "";

            var title = properties.Title;
            data += ("Title: " + title);
            var width = Convert.ToString(properties.Width);
            if(width != null)
                data += ("\nWidth: " + width);
            var height = Convert.ToString(properties.Height);
            if(height != null)
                data += ("\tHeight: " + height);
            var dateTaken = properties.DateTaken;
            data += ("\nDate taken: " + dateTaken);
            var latitude = Convert.ToString(properties.Latitude);
            if (latitude != null)
                data += ("\nLatitude: " + latitude);
            var longitude = Convert.ToString(properties.Longitude);
            if (longitude != null)
                data += ("\tLongitude: " + longitude);
            var cameraModel = properties.CameraModel;
            data += ("\nCamera model: " + cameraModel);
            var cameraManufacture = properties.CameraManufacturer;
            data += ("\tCamera manufacture: " + cameraManufacture);

            return data;
        }

        public ICommand GetNextImageCommand
        {
            get; private set;
        }

        private void GetNextImage()
        {
            if (_bitmapList == null) return;
            if (_bitmapCount < _bitmapList.Count - 1)
            {
                _bitmapCount++;
            }
            else if (_bitmapCount == _bitmapList.Count - 1)
            {
                _bitmapCount = 0;
            }
            BitmapImageSource = _bitmapList[_bitmapCount];
            ImagePropertiesesText = GetImageDetails(_imagePropertieses[_bitmapCount]);
        }
    }
}

