using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Windows.Forms;
using Windows.Graphics;
using WinRT.Interop;
using System.Runtime.InteropServices;
using Windows.Media.Core;

namespace On_Stream_SFX_VFX_Overlay_Integration.src
{
    public sealed partial class DetachedVideoPlayer : Window
    {
        private MediaPlayerElement _mediaPlayer;
        private AppWindow _appWindow;
        private SizeInt32 _screenSize;
        private bool _isInitialized = false;

        // Base init of window
        public void VideoOverlayWindow()
        {
            // Init XAML content - minimal
            var rootGrid = new Grid
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0))
            };

            _mediaPlayer = new MediaPlayerElement
            {
                AreTransportControlsEnabled = false,
                AutoPlay = true,
                Stretch = Stretch.UniformToFill,
                IsFullWindow = true
            };

            rootGrid.Children.Add(_mediaPlayer);
            Content = rootGrid;

            //Get HWND and AppWindow
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            // Manage window position and size on screen
            _appWindow.Title = "";
            _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

            var primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen != null)
            {
                _screenSize = new SizeInt32(primaryScreen.Bounds.Width, primaryScreen.Bounds.Height);
            } else
            {
                _screenSize = new SizeInt32(1920, 1080);
            }
            _appWindow.Resize(_screenSize);

            _appWindow.Move(new PointInt32(
                (int)primaryScreen.Bounds.Left,
                (int)primaryScreen.Bounds.Top));

            ApplyOverlayStyles(hwnd);

            // Set always visible and at top
            _appWindow.IsShownInSwitchers = false;
            this.Activated += OnActivated;
            //_appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        }



        // Event to check window initialization
        private void OnActivated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.CodeActivated || args.WindowActivationState == WindowActivationState.PointerActivated)
            {
                _isInitialized = true;
                this.Activated -= OnActivated;
            }
        }



        // Window info and parameters for play
        private void ApplyOverlayStyles(IntPtr hwnd)
        {
            const int GWL_EXSTYLE = -20;

            // Essential flags
            const int WS_EX_LAYERED     = 0x00080000;   // Transparency
            const int WS_EX_TRANSPARENT = 0x00000020;   // Clicks go through
            const int WS_EX_NOACTIVE    = 0x08000000;   // Never gets focus
            const int WS_EX_TOOLWINDOW  = 0x00000080;   // Not shown in tasks bar
            const int WS_EX_TOPMOST     = 0x00000008;   // Always on top

            int currentStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            uint newStyle = (uint)currentStyle;
            newStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;

            SetWindowLong(hwnd, GWL_EXSTYLE, (int)newStyle);
        }



        // Play video on main screen
        public void PlayVideo(string filePath, double volume = 1.0)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            // Forces window init if still not done
            if (!_isInitialized)
            {
                this.Activate();
            }

            // Manage source play
            DispatcherQueue.TryEnqueue(() =>
            {
                var source = MediaSource.CreateFromUri(new Uri(filePath));
                _mediaPlayer.Source = source;
                _mediaPlayer.MediaPlayer.Volume = volume;
                _mediaPlayer.MediaPlayer.Play();

                _appWindow.Show();
            });
        }



        // Stop video and hide window
        public void StopAndHide()
        {
            _mediaPlayer.MediaPlayer.Pause();
            _appWindow.Hide();
        }





        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}