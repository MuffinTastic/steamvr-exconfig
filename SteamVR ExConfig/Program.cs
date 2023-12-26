using System.Diagnostics;

namespace SteamVR_ExConfig;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        var steamConfig = SteamConfig.Load();
        var apps = VRAppSetting.GetVRAppSettings( steamConfig );
        var vrConfig = SteamVRConfig.GetVRConfig( steamConfig );

        ApplicationConfiguration.Initialize();
        Application.Run( new MainForm( apps, vrConfig ) );
    }
}