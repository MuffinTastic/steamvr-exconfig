using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SteamVR_ExConfig;

public class VRAppConfig
{
    [JsonPropertyName( "autolaunch" )]
    public bool Autolaunch { get; set; } = false;

    [JsonPropertyName( "last_launch_time" )]
    public string LastLaunchTime { get; set; } = "0";
}

public class VRAppSetting : IVRSetting
{
    public required string ReadableName { get; set; }
    public required string ConfigFilePath { get; set; }

    public required VRAppConfig Config { get; set; }

    // --- //

    public bool Enabled => Config.Autolaunch;

    private bool Dirty = false;

    public void SetEnabled( bool enabled )
    {
        Config.Autolaunch = enabled;
        Dirty = true;
    }

    // --- //

    public void Save()
    {
        if ( Dirty )
        {
            using ( var stream = File.Open( ConfigFilePath, FileMode.Create ) )
            {
                var options = new JsonSerializerOptions() { WriteIndented = true };
                stream.Write( Encoding.Default.GetBytes( JsonSerializer.Serialize( Config, options ) ) );
            }

            Debug.WriteLine( $"Updated {ConfigFilePath} - Set autolaunch to {Enabled}" );
        }

        Dirty = false;
    }

    // --- //

    private static Regex SteamVRAppRegex = new Regex( @"^steam\.(?:app|overlay)\.(\d+)$", RegexOptions.Compiled );

    private static List<string> RejectPrefixes = new List<string>() { "steam.app", "revive.app" };

    // Common VR Apps that don't go through the Steam store
    private static Dictionary<string, string> KnownVRApps = new Dictionary<string, string>()
    {
        { "openvr.tool.steamvr_environments", "SteamVR Environments" },
        { "openvr.tool.steamvr_room_setup", "SteamVR Room Setup" },
        { "openvr.tool.steamvr_tutorial", "SteamVR Tutorial" },
        { "pushrax.SpaceCalibrator", "OpenVR Space Calibrator" },
        { "revive.dashboard.overlay", "Revive Dashboard Overlay" },
        { "slimevr.steamvr.feeder", "SlimeVR Feeder" }
    };

    public static List<VRAppSetting> GetAppSettings( OpenVRPaths openVRPaths, SteamLibraries steamLibraries )
    {
        var paths = Directory.EnumerateFiles( openVRPaths.VRAppConfigPath ).Where( p => p.EndsWith( ".vrappconfig" ) );

        //var steamAppsManifest = SteamAppsManifest.Load( openVRPaths );
        List<VRAppSetting> vrApps = new();

        foreach ( var path in paths )
        {
            if ( !path.EndsWith( ".vrappconfig" ) )
                continue;

            var filename = Path.GetFileNameWithoutExtension( path );
            var name = GetVRAppName( steamLibraries, filename );

            // This lets us skip known non-existent/rejected apps
            if ( name is null )
                continue;

            // Read from config
            var config = ReadVRAppConfig( path );

            if ( config is null )
                config = new VRAppConfig();

            var app = new VRAppSetting()
            {
                ReadableName = name,
                ConfigFilePath = path,
                Config = config
            };

            vrApps.Add( app );
        }

        return vrApps;
    }

    private static string? GetVRAppName( SteamLibraries steamLibraries, string filename )
    {
        string? name = filename;

        foreach ( var prefix in RejectPrefixes )
        {
            if ( filename.StartsWith( prefix ) )
                return null;
        }

        // Match from Steam app manifests
        if ( KnownVRApps.GetValueOrDefault( filename ) is string knownName )
        {
            name = knownName;
        }
        else
        {
            var steamMatch = SteamVRAppRegex.Match( filename );
            if ( steamMatch.Groups.Count == 2 )
            {
                string appID = steamMatch.Groups[1].Value;

                name = steamLibraries.GetNameForAppID( appID );
            }
        }

        return name;
    }

    private static VRAppConfig? ReadVRAppConfig( string path )
    {
        using ( var stream = File.OpenRead( path ) )
        {
            return JsonSerializer.Deserialize<VRAppConfig>( stream, JsonSerializerOptions.Default );
        }
    }
}
