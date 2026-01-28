using CommunityToolkit.Mvvm.ComponentModel;

namespace On_Stream_SFX_VFX_Overlay_Integration
{
    // Represent each of the buttons with wall their settings
    public partial class ButtonInstance : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string filePath = string.Empty;

        //TODO delay, cost, volume, cooldown, others?
    }
}
