using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IC6.EmotionAPI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture _mediaCapture;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_mediaCapture == null)
            {
                await InitializeCameraAsync();
            }

        }

        /// <summary>
        /// Initializes the MediaCapture.
        /// </summary>
        /// <returns></returns>
        private async Task InitializeCameraAsync()
        {

            // Attempt to get the front camera if one is available, but use any camera device if not
            var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Front);

            if (cameraDevice == null)
            {
                Debug.WriteLine("No camera device found!");
                return;
            }

            // Create MediaCapture and its settings
            _mediaCapture = new MediaCapture();


            var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

            // Initialize MediaCapture
            try
            {
                await _mediaCapture.InitializeAsync(settings);
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine("The app was denied access to the camera");
            }
        }

        /// <summary>
        /// Attempts to find and return a device mounted on the panel specified, and on failure to find one it will return the first device listed
        /// </summary>
        /// <param name="desiredPanel">The desired panel on which the returned device should be mounted, if available</param>
        /// <returns></returns>
        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                using (var stream = new InMemoryRandomAccessStream())
                {
                    await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

                    stream.Seek(0);

                    var emotion = await MakeRequest(stream.AsStream());

                    if (emotion == null)
                    {
                        await new MessageDialog("Emotions non detected.").ShowAsync();

                        return;
                    }

                    var imgBrush = new ImageBrush();

                    if (emotion.Scores.Sadness > emotion.Scores.Happiness)
                    {
                        imgBrush.ImageSource = new BitmapImage(new Uri(@"ms-appx://IC6.EmotionAPI/Assets/sad.jpg"));
                    }
                    else
                    {
                        imgBrush.ImageSource = new BitmapImage(new Uri(@"ms-appx://IC6.EmotionAPI/Assets/happy.jpg"));
                    }

                    myGrid.Background = imgBrush;

                }

            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task<Emotion> MakeRequest(Stream stream)
        {
            var apiClient = new Microsoft.ProjectOxford.Emotion.EmotionServiceClient("f1b67ad2720944018881b6f8761dff9a");
            var results = await apiClient.RecognizeAsync(stream);

            if (results == null) return null;

            return results.FirstOrDefault();
        }


    }
}
