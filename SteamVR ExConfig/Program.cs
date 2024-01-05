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
            var vrConfig = SteamVRConfig.GetVRConfig( openVRPaths, steamLibraries );

            ApplicationConfiguration.Initialize();
            Application.Run( new MainForm( vrConfig ) );

        }
        catch ( Exception ex )
        {
            MessageBox.Show( ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
        }
    }
}