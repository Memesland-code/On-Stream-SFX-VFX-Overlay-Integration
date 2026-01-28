using LibVLCSharp.Shared;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using On_Stream_SFX_VFX_Overlay_Integration.src;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace On_Stream_SFX_VFX_Overlay_Integration
{
    public sealed partial class MainWindow : Window
    {
        private readonly ObservableCollection<ButtonInstance> _buttons = new();
        private StorageFile _currentSelectedFile;
        private DetachedVideoPlayer _videoPlayer;

        public MainWindow()
        {
            InitializeComponent();

            MediaListView.ItemsSource = _buttons;
        }

        private async void PickFile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };

            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mov");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                _currentSelectedFile = file;
                StatusText.Text = $"Ready to add {file.Name}";
                AddToListButton.IsEnabled = true;
            }
        }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSelectedFile == null) return;

            var item = new ButtonInstance
            {
                Name = _currentSelectedFile.Name,
                FilePath = _currentSelectedFile.Path
            };

            _buttons.Add(item);

            StatusText.Text = $"Added {item.Name}";
            AddToListButton.IsEnabled = false;
            _currentSelectedFile = null; // Refresh selected button
        }

        /*
        private async void TestPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ButtonInstance item)
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                    var source = MediaSource.CreateFromStorageFile(file);
                    Player.Source = source;
                    Player.MediaPlayer.Play();

                    StatusText.Text = $"Testing {item.Name}";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Error while testing {ex.Message}";
                }

                if (_videoPlayer == null)
                {
                    _videoPlayer = new DetachedVideoPlayer();
                    _videoPlayer.Activate();
                }

                _videoPlayer.PlayVideo(item.FilePath, 0.5);
                await Task.Delay(0);
                _videoPlayer.StopAndHide();
            }
        }
        */



        private async void TestPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not ButtonInstance item)
            {
                StatusText.Text = "Error: invalid button or item";
                return;
            }

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                var source = MediaSource.CreateFromStorageFile(file);

                PlayerControl.MediaPlayer?.Pause();
                PlayerControl.MediaPlayer.MediaEnded -= OnMediaEnded;
                PlayerControl.Source = null;

                PlayerControl.Source = source;
                PlayerControl.MediaPlayer.MediaEnded += OnMediaEnded;
                PlayerControl.MediaPlayer.Play();

                StatusText.Text = $"Testing media: {item.Name}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error while playing media: {ex.Message}";
            }

            if (_videoPlayer == null)
            {
                _videoPlayer = new DetachedVideoPlayer();
                _videoPlayer.Activate();
            }

            _videoPlayer.PlayVideo(item.FilePath, 0.5);
            await Task.Delay(0);
            _videoPlayer.StopAndHide();
        }



        private void OnMediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                sender.Pause();

                PlayerControl.Source = null;

                StatusText.Text = "Media ended";

                _videoPlayer.StopAndHide();

                PlayerControl.MediaPlayer.MediaEnded -= OnMediaEnded;
            });
        }



        private void MediaListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MediaListView.SelectedItem is ButtonInstance item)
            {
                new NotImplementedException();
            }
        }
    }
}
