using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PatronageBLStream.Messenger;
using PatronageBLStream.Model;
using NavigationService = PatronageBLStream.Navigation.NavigationService;

namespace PatronageBLStream.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private int _bitmapCount;
        private readonly List<string> _path; 
        private BitmapImage _bitmapImageSource;
        private readonly List<BitmapImage> _bitmapList;
        private List<PhotoModel> _photo; 
        private readonly List<ImageProperties> _imagePropertieses;
        private string _imagePropertiesesText;
        //private readonly List<BitmapImage> _thumbnailList;
        public MediaCapture Capture { get; }
        private DataTransferManager TransferManager { get; }
        private readonly List<StorageFile> _photoList = new List<StorageFile>();
        private readonly NavigationService _navigationService = new NavigationService();
        //private ObservableCollection<StorageFile> photoCollection; 

        public MainViewModel()
        {
            _bitmapCount = 0;
            _path = new List<string>();
            _bitmapList = new List<BitmapImage>();
            _photo=new List<PhotoModel>();
            _imagePropertieses = new List<ImageProperties>();
            GetNextImageCommand = new RelayCommand(GetNextImage);
            TakePhotoCommand = new RelayCommand(TakePhoto);
            SharePhotoCommand = new RelayCommand(SharePhoto);
            PhotoListViewCommand = new RelayCommand(PhotoList);

            //_thumbnailList = new List<BitmapImage>();
            var photoView = SystemNavigationManager.GetForCurrentView();
            photoView.AppViewBackButtonVisibility=AppViewBackButtonVisibility.Collapsed;
            //photoCollection=new ObservableCollection<StorageFile>();
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<MvvmMessage>(this, x=>SelectItemIndex(x.BitmapCount));

            InitializeGallery();
        }

        public ICommand GetNextImageCommand{ get; private set; }
        public ICommand TakePhotoCommand{ get; private set; }
        public ICommand SharePhotoCommand{ get; private set; }
        public ICommand PhotoListViewCommand { get; set; }

        private async void InitializeGallery() 
        {
            try
            {
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery,
                    new[] {".jpeg", ".jpg"/*, ".png"*/}) {FolderDepth = FolderDepth.Deep};
                var query = Windows.Storage.KnownFolders.PicturesLibrary.CreateFileQueryWithOptions(queryOptions);
                var files = await query.GetFilesAsync();


                if (files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        var bitmapImage = new BitmapImage();
                        var _thumbnailList = new BitmapImage();

                        _photoList.Add(file);
                        await bitmapImage.SetSourceAsync(await file.OpenReadAsync());
                        _bitmapList.Add(bitmapImage);
                        _path.Add(file.Path);

                        Windows.Storage.FileProperties.ImageProperties properties = await GetImagePropertiesAsync(file);

                        StorageItemThumbnail x = await file.GetThumbnailAsync(ThumbnailMode.PicturesView);
                        // await thumbnail.SetSource(file.GetThumbnailAsync(ThumbnailMode.ListView));
                        _thumbnailList.SetSource(x);
                        var data = new PhotoModel(bitmapImage, file.Path, _thumbnailList, properties, file);
                        _photo.Add(data);
                    }
                }

                if (_bitmapList.Count >= 1)
                {
                    BitmapImageSource = _bitmapList[_bitmapCount];
                    ImagePropertiesesText = GetImageDetails(_photo[_bitmapCount].Properties);
                }
            }
            catch (Exception ex)
            {
                //MessageDialog message = new MessageDialog(ex.Message);
            }
        }

        #region Method Task 1
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
        #endregion

        #region Method Task 2
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

            //public ImageProperties Properties{ get; private set; }

            private async Task<ImageProperties> GetImagePropertiesAsync(Windows.Storage.StorageFile file)
            {
                ImageProperties properties = await file.Properties.GetImagePropertiesAsync();
                _imagePropertieses.Add(properties);
                return properties;
            } 

            private static string GetImageDetails(ImageProperties properties)
            {
                var data = "";

                var title = Path.GetFileName(properties.Title);
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
        #endregion

        #region Method Task 3
            private async void TakePhoto()
            {
                var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                DeviceInformation cameraDevice =
                    allVideoDevices.FirstOrDefault(
                        x =>
                            x.EnclosureLocation != null &&
                            x.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back);

                cameraDevice = cameraDevice ?? allVideoDevices.FirstOrDefault();

                if (cameraDevice == null)
                {
                    var msgDialog = new MessageDialog("No camera device found.");
                }

                try
                {
                    CameraCaptureUI captureUi = new CameraCaptureUI();
                    captureUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;

                    StorageFile photo = await captureUi.CaptureFileAsync(CameraCaptureUIMode.Photo);

                    if (photo != null)
                    {
                        var result = Path.GetRandomFileName() + DateTime.Now.ToString("yyyy-m-d dddd") + ".jpg";
                        var resultCopy =
                            await
                                photo.CopyAsync(KnownFolders.CameraRoll, result, NameCollisionOption.GenerateUniqueName);
                        _bitmapList.Clear();
                        InitializeGallery();
                    }
                }
                catch (Exception ex)
                {
                    MessageDialog msgMessageDialog = new MessageDialog(ex.Message);
                }

            }

            private void SharePhoto()
            {
                DataTransferManager.ShowShareUI();
                //DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                ShareSourceLoad();
            }

            private void ShareSourceLoad()
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.DataRequested);
            }

            private void DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
            {
                List<IStorageItem> storageItems = new List<IStorageItem> {_photoList[_bitmapCount]};
                DataRequest request = e.Request;
                request.Data.Properties.Title = "Share photo";
                request.Data.Properties.Description = "Select the application you want to share photos.";
                request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromFile(_photoList[_bitmapCount]);
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(_photoList[_bitmapCount]));
            }

            private void PhotoList()
            {
                _navigationService.Navigate(typeof (View.PhotoListView));
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<MvvmMessage>(new MvvmMessage(this._photo));
            }

            private void SelectItemIndex(int index)
            {
                _bitmapCount = index;
                BitmapImageSource = _photo[_bitmapCount].Photo;
                ImagePropertiesesText = GetImageDetails(_photo[index].Properties);
            }
        #endregion
    }
}

