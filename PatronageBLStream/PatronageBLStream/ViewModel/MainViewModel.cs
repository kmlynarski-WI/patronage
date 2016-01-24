using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.BulkAccess;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace PatronageBLStream.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private int _bitmapCount;
        private List<string> _path; 
        private BitmapImage _bitmapImageSource;
        private readonly List<BitmapImage> _bitmapList; 

        public MainViewModel()
        {
            _bitmapCount = 0;
            _path = new List<string>();
            _bitmapList = new List<BitmapImage>();
            InitializeGallery();
        }

        private async void InitializeGallery() 
        {
            try
            {
                var files = await KnownFolders.PicturesLibrary.GetFilesAsync();

                if (files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        var stream = await file.OpenReadAsync();
                        var bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(stream);
                        _bitmapList.Add(bitmapImage);
                        _path.Add(file.Path);
                    }
                }

                if (_bitmapList.Count >= 1 )
                {
                    BitmapImageSource = _bitmapList[_bitmapCount];
                }
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message);
                await message.ShowAsync();
            }

        }

        public BitmapImage BitmapImageSource
        {
            get
            {
                return _bitmapImageSource;
            }
            set
            {
                if (_bitmapImageSource != value)
                {
                    _bitmapImageSource = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}

