using LibVLCSharp.Shared;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;
using Windows.Graphics;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace On_Stream_SFX_VFX_Overlay_Integration.src
{
    public sealed partial class DetachedVideoPlayer : Window
    {
		private LibVLC _libVLC;
		private MediaPlayer _mediaPlayer;
		private AppWindow _appWindow;
		private IntPtr _hwnd;

		public DetachedVideoPlayer()
		{
			// Init LibVLC
			Core.Initialize();

			_libVLC = new LibVLC("--no-osd", "--no-video-title-shown");

			// Creating video player
			_mediaPlayer = new MediaPlayer(_libVLC);

			// Getting hwnd
			_hwnd = WindowNative.GetWindowHandle(this);

			// Apply window flags
			ApplyOverlayStyles(_hwnd);

			// Get AppWindow once activated
			var windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
			_appWindow = AppWindow.GetFromWindowId(windowId);

			if (_appWindow != null)
			{
				_appWindow.Title = "";
				_appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
				_appWindow.Resize(new Windows.Graphics.SizeInt32(2560, 1440));
				_appWindow.IsShownInSwitchers = false;

				var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
				_appWindow.Move(new Windows.Graphics.PointInt32(
					(int)primaryScreen.Bounds.Left,
					(int)primaryScreen.Bounds.Top)
				);
			}
		}



		private void ApplyOverlayStyles(IntPtr hwnd)
		{
			const int GWL_EXSTYLE = -20;
			const uint WS_EX_LAYERED =		0x00080000;
			const uint WS_EX_TRANSPARENT =	0x00000020;
			const uint WS_EX_NOACTIVE =		0x08000000;
			const uint WS_EX_TOOLWINDOW =	0x00000080;
			const uint WS_EX_TOPMOST =		0x00000008;

			int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
			uint newExStyle = (uint)exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
			SetWindowLong(hwnd, GWL_EXSTYLE, (int)newExStyle);

			// Sets window transparent - makes LibVLC draw directly in HWND with alpha
			SetLayeredWindowAttributes(hwnd, 0, 255, 0x02);
		}



		public void PlayVideo(string filePath, double volume = 1.0f)
		{
			if (string.IsNullOrEmpty(filePath) || _appWindow == null)
			{
				Debug.WriteLine("Can play video: file path or AppWindow is missing");
				return;
			}

			using var media = new Media(_libVLC, new Uri(filePath));
			_mediaPlayer.Media = media;
			_mediaPlayer.Volume = (int)(volume * 100);

			// Adds media player to window HWND
			_mediaPlayer.Hwnd = _hwnd;
			_appWindow.Show();
			_mediaPlayer.Play();
		}



		public void StopAndHide()
		{
			// Stops media and detaches hwnd
			_mediaPlayer?.Stop();
			_mediaPlayer.Media = null;
			_mediaPlayer.Hwnd = IntPtr.Zero;

			if (_appWindow != null)
			{
				_appWindow.Hide();
			}

			this.Closed += OnClosed;
		}



		private void OnClosed(object sender, WindowEventArgs args)
		{
			_mediaPlayer?.Dispose();
			_libVLC?.Dispose();
		}





		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
	}
}