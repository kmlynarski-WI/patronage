using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PatronageBLStream.Messenger;
using PatronageBLStream.Model;

namespace PatronageBLStream.ViewModel
{
    public class PhotoListViewModel : ViewModelBase
    {
        private ObservableCollection<PhotoModel> photoCollection;
        private List<PhotoModel> _photoList; 

        public PhotoListViewModel()
        {
            photoCollection = new ObservableCollection<PhotoModel>();
            //PhotoListViewCommand = new RelayCommand(PhotoListView);
            BackCommand = new RelayCommand(BackPage);
            SelectItemCommand = new RelayCommand<string>(SelectItem);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<MvvmMessage>(this, x => PhotoListView(x.DataList));
        }

       // public ICommand PhotoListViewCommand{ get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand SelectItemCommand { get; set; }

        private void PhotoListView(List<PhotoModel> photoModels)
        {
            _photoList = photoModels;
            if (_photoList != null)
            {
                PhotoCollection.Clear();
                // _photoList=GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<MvvmMessage>(this, x=>x.DataList);
                for (int i = 0; i < _photoList.Count; i++)
                {
                    PhotoCollection.Add(new PhotoModel(_photoList[i].ThumbnailImage,
                        Path.GetFileName(_photoList[i].Path)));
                }
            }
        }

        public ObservableCollection<PhotoModel> PhotoCollection
        {
            get { return photoCollection; }
            set
            {
                if (photoCollection != value)
                {
                    photoCollection = value;
                    RaisePropertyChanged("PhotoCollection");
                }
            }

        } 

        private void BackPage()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            // Navigate back if possible, and if the event has not 
            // already been handled .
            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
            }
        }

        private void SelectItem(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            int index = _photoList.FindIndex(x => Path.GetFileName(x.Path) == path);
            //return _photoList[index];
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<MvvmMessage>(new MvvmMessage(index));
            BackPage();
        }



        //private async void GetPhotoCollection()
        //{
        //    var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery,
        //        new[] {".jpeg", ".jpg" /*, ".png"*/})
        //    {FolderDepth = FolderDepth.Deep};
        //    var query = Windows.Storage.KnownFolders.PicturesLibrary.CreateFileQueryWithOptions(queryOptions);
        //    var files = await query.GetFilesAsync();
        //    ;
        //    var thumbnail = new BitmapImage();
        //    foreach (var file in files)
        //    {
        //        //photoCollection?.Add(file);
        //        StorageItemThumbnail x = await file.GetThumbnailAsync(ThumbnailMode.PicturesView);
        //        thumbnail.SetSource(x);
        //        if (_thumbnailList != null) _thumbnailList.Add(thumbnail);
        //    }
        //}
    }
}
