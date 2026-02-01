using CommunityToolkit.Mvvm.ComponentModel;

namespace On_Stream_SFX_VFX_Overlay_Integration
{
	public enum MediaType
	{
		Audio,
		Video,
        None
	}

	// Represent each of the buttons with wall their settings
	public partial class ButtonInstance : ObservableObject
    {
        [ObservableProperty]
        private MediaType mediaType;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string filePath = string.Empty;

        //TODO delay, cost, volume, cooldown, others?
    }
}
