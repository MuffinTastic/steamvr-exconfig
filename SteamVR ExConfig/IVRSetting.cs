namespace SteamVR_ExConfig;

internal interface IVRSetting
{
    public string ReadableName { get; }
    public bool Enabled { get; }
    public void SetEnabled( bool enabled );
}
