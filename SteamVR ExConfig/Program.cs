namespace SteamVR_ExConfig;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            var config = Config.Load();
            var openVRPaths = OpenVRPaths.Load( config );
            var steamLibraries = SteamLibraries.GetLibraries( openVRPaths );
            var steamVRConfig = SteamVRConfig.GetVRConfig( openVRPaths, steamLibraries );

            ApplicationConfiguration.Initialize();
            Application.Run( new MainForm( config, steamVRConfig ) );

        }
        catch ( Exception ex )
        {
            MessageBox.Show( ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
        }
    }
}