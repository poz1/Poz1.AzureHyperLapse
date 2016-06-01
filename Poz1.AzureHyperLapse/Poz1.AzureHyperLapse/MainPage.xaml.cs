using Microsoft.ProjectOxford.Video.Contract;
using PCLStorage;
using Plugin.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Poz1.AzureHyperLapse
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private const string _subKey = “__INSERT YOUR SUBSCRIPTION KEY__”;

        private TimeSpan _queryTime = TimeSpan.FromSeconds(20);

        private ObservableCollection<string> _logList;
        public ObservableCollection<string> LogList
        {
            get { return _logList; }
            set
            {
                if (_logList != value)
                {
                    _logList = value;
                    RaisePropertyChanged();
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            LogList = new ObservableCollection<string>();

            PickVideoButton.Clicked += PickVideoButton_Clicked;
        }

        private async void PickVideoButton_Clicked(object sender, EventArgs e)
        {
            if (CrossMedia.Current.IsPickVideoSupported)
            {
                try
                {
                    var video = await CrossMedia.Current.PickVideoAsync();
                    var stream = video.GetStream();
                    await StabilizeVideo(stream);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    LogList.Add(ex.Message.ToUpper());
                }
            }
        }

        private async Task StabilizeVideo(Stream videoStream)
        {
            Debug.WriteLine("Start stabilizing");
            LogList.Add("Start stabilizing".ToUpper());

            Microsoft.ProjectOxford.Video.VideoServiceClient client = new Microsoft.ProjectOxford.Video.VideoServiceClient(_subKey);
            client.Timeout = TimeSpan.FromMinutes(10);

            Debug.WriteLine("Start uploading video");
            LogList.Add("Start uploading video".ToUpper());

            Operation operation = await client.CreateOperationAsync(videoStream, new VideoStabilizationOperationSettings());

            Debug.WriteLine("Uploading video done");
            LogList.Add("Uploading video done".ToUpper());

            OperationResult result = await client.GetOperationResultAsync(operation);
            while (result.Status != OperationStatus.Succeeded && result.Status != OperationStatus.Failed)
            {
                Debug.WriteLine("Server status: {0}, wait {1} seconds...", result.Status, _queryTime.TotalSeconds);
                LogList.Add(("Server status: " + result.Status + ", wait " + _queryTime.TotalSeconds + " Seconds...").ToUpper());

                await Task.Delay(_queryTime);
                result = await client.GetOperationResultAsync(operation);
            }
            Debug.WriteLine("Finish processing with server status: " + result.Status);
            LogList.Add(("Finish processing with server status: " + result.Status).ToUpper());

            if (result.Status == OperationStatus.Succeeded)
            {
                Debug.WriteLine("Start downloading result video");
                LogList.Add("Start downloading result video".ToUpper());

                using (Stream resultStream = await client.GetResultVideoAsync(result.ResourceLocation))
                {
                    IFolder rootFolder = FileSystem.Current.LocalStorage;

                    // create a folder, if one does not exist already
                    IFolder folder = await rootFolder.CreateFolderAsync("StabilizedVideos", CreationCollisionOption.OpenIfExists);

                    // create a file, overwriting any existing file
                    IFile file = await folder.CreateFileAsync("MyStabilizedFile.mp4", CreationCollisionOption.ReplaceExisting);

                    // populate the file with some text
                    var fileStream = await file.OpenAsync(FileAccess.ReadAndWrite);
                    await resultStream.CopyToAsync(fileStream);
                }
                Debug.WriteLine("Downloading result video done");
                LogList.Add("Downloading result video done".ToUpper());
            }
            else
            {
                Debug.WriteLine("Fail reason: " + result.Message);
                LogList.Add("Fail reason: " + result.Message.ToUpper());
            }
        }

        //-- INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
