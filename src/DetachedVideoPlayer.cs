using LibVLCSharp.Shared;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;
using Windows.Graphics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

namespace On_Stream_SFX_VFX_Overlay_Integration.src
{
    public sealed partial class DetachedVideoPlayer : Window
    {
		private LibVLC? _libVLC;
		public MediaPlayer _vlcPlayer;
		private AppWindow _appWindow;
		private IntPtr _hwnd;

		public DetachedVideoPlayer(LibVLC libVLC)
		{
			_libVLC = libVLC;
			_vlcPlayer = new MediaPlayer(_libVLC);
			
			// Getting hwnd
			_hwnd = WindowNative.GetWindowHandle(this);
			
			// Apply window flags
			ApplyOverlayStyles(_hwnd);

			// Get AppWindow once activated
			var windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
			_appWindow = AppWindow.GetFromWindowId(windowId);

			if (_appWindow != null)
			{
				_appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
				_appWindow.Resize(new SizeInt32(2560, 1440));
				_appWindow.IsShownInSwitchers = false;

				var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
				_appWindow.Move(new PointInt32(
					(int)primaryScreen.Bounds.Left,
					(int)primaryScreen.Bounds.Top)
				);
			}

			this.Activate();

			this.Closed += OnClosed;
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
		}



		public async Task PlayVideo(string filePath, double volume = 1.0f)
		{
			if (string.IsNullOrEmpty(filePath) || _vlcPlayer == null)
			{
				Debug.WriteLine("Can't play video: file path or AppWindow is missing");
				return;
			}

			try
			{
				using var media = new Media(_libVLC, new Uri(filePath));
				_vlcPlayer.Media = media;
				_vlcPlayer.Volume = (int)(volume * 100);

				// Adds media player to window HWND
				_vlcPlayer.Hwnd = _hwnd;

				_appWindow.Show();

				_vlcPlayer.Play();

				ForceTopMost();

				Debug.WriteLine("Video play called");
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error while playing video: " + ex.Message);
			}

			//_vlcPlayer.EndReached += (s, e) => StopAndHide();
		}



		private void ForceTopMost()
		{
			const int SWP_NOSIZE = 0x0001;
			const int SWP_NOMOVE = 0x002;
			const int HWND_TOPMOST = -1;

			SetWindowPos(_hwnd, new nint(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
		}



		// Stops media and detaches hwnd
		public async Task StopAndHide()
		{
			try
			{
				var stopTask = Task.Run(() => _vlcPlayer.Stop());
				if (await Task.WhenAny(stopTask, Task.Delay(1000)) == stopTask)
				{
					Debug.WriteLine("Stop complete");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Stop() failed or blocked: " + ex.Message + " - ignoring");
			}

			_vlcPlayer.Media = null;
			
			_vlcPlayer.Hwnd = IntPtr.Zero;

			this.Close();
		}



		private void OnClosed(object sender, WindowEventArgs args)
		{
			Debug.WriteLine("closed overlay, cleaning up LibVLC");
			_vlcPlayer?.Dispose();
			_libVLC = null;
		}





		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		// Ajout du DllImport manquant pour SetWindowPos
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	}
}