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
using System.Threading.Tasks;
using System.Diagnostics;

namespace On_Stream_SFX_VFX_Overlay_Integration.src
{
    public sealed partial class DetachedVideoPlayerOld : Window
    {
        private MediaPlayerElement _mediaPlayer;
        private AppWindow _appWindow;
        private bool _isVisible = false;

        // Base init of window
        public void VideoOverlayWindow()
        {
            // Init XAML content - minimal
            var root = new Grid
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

            root.Children.Add(_mediaPlayer);
            Content = root;

            //Get HWND and AppWindow
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            ApplyOverlayStyles(hwnd);

            this.Activated += (s, e) =>
            {
                if (_appWindow == null)
                {
                    _appWindow = AppWindow.GetFromWindowId(windowId);
                }

			    // Manage window position and size on screen
                _appWindow.Title = "";
    			_appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
	    		_appWindow.Resize(new SizeInt32(2560, 1440));
		    
                var primaryScreen = Screen.PrimaryScreen;
			    _appWindow.Move(new PointInt32((int)primaryScreen.Bounds.Left, (int)primaryScreen.Bounds.Top));
			    
                _appWindow.IsShownInSwitchers = false;
            };

            this.Activate();
        }



        // Window info and parameters for play
        private void ApplyOverlayStyles(IntPtr hwnd)
        {
            // Essentials flags
            const int GWL_EXSTYLE = -20;
            const int WS_EX_LAYERED     = 0x00080000;   // Transparency
            const int WS_EX_TRANSPARENT = 0x00000020;   // Clicks go through
            const int WS_EX_NOACTIVE    = 0x08000000;   // Never gets focus
            const int WS_EX_TOOLWINDOW  = 0x00000080;   // Not shown in tasks bar
            const int WS_EX_TOPMOST     = 0x00000008;   // Always on top

            int style = GetWindowLong(hwnd, GWL_EXSTYLE);

            uint newStyle = (uint)style | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;

            SetWindowLong(hwnd, GWL_EXSTYLE, (int)newStyle);
        }



        // Play video on main screen
        public void PlayVideo(string filePath, double volume = 1.0)
        {
            if (string.IsNullOrEmpty(filePath) || _appWindow == null)
            {
                Debug.WriteLine("Can't play medie: invalid path or AppWindow is null");
                return;
            }

            var source = MediaSource.CreateFromUri(new Uri(filePath));
			_mediaPlayer.Source = source;
			_mediaPlayer.MediaPlayer.Volume = volume;
			_mediaPlayer.MediaPlayer.Play();

			_appWindow.Show();
        }



        // Stop video and hide window
        public void StopAndHide()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.MediaPlayer.Pause();
                _mediaPlayer.Source = null;
            }
    
            if (_appWindow != null)
            {
                _appWindow.Hide();
            }
        }





        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}