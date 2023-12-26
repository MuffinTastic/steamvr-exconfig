using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamVR_ExConfig;

public class VRAppConfig
{
    [JsonPropertyName( "autolaunch" )]
    public bool Autolaunch { get; set; } = false;

    [JsonPropertyName( "last_launch_time" )]
    public string? LastLaunchTime { get; set; } = "0";
}

public class VRAppSetting : IVRSetting
{
    public string? ReadableName { get; set; }
    public string? ConfigFilePath { get; set; }

    private VRAppConfig? Config { get; set; }

    // --- //

    public bool Enabled => Config!.Autolaunch;

    private bool Dirty = false;

    public void SetEnabled( bool enabled )
    {
        Config!.Autolaunch = enabled;
        Dirty = true;
    }

    // --- //

    public void SaveToFile()
    {
        if ( Dirty )
        {
            using ( var stream = File.Open( ConfigFilePath!, FileMode.Create ) )
            {
                var options = new JsonSerializerOptions() { WriteIndented = true };
                stream.Write( Encoding.Default.GetBytes( JsonSerializer.Serialize( Config!, options ) ) );
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

    public static List<VRAppSetting> GetVRAppSettings( SteamConfig steamConfig )
    {
        var paths = Directory.EnumerateFiles( steamConfig.VRAppConfigPath! ).Where( p => p.EndsWith( ".vrappconfig" ) );

        List<VRAppSetting> vrApps = new();

        foreach ( var path in paths )
        {
            if ( !path.EndsWith( ".vrappconfig" ) )
                continue;

            var filename = Path.GetFileNameWithoutExtension( path );
            var name = GetVRAppName( steamConfig, filename );

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

    private static string? GetVRAppName( SteamConfig steamConfig, string filename )
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

                name = steamConfig.GetNameForAppID( appID );
            }
        }

        return name;
    }

    private static VRAppConfig? ReadVRAppConfig( string path )
    {
        VRAppConfig? appConfig = null;

        try
        {
            using ( var stream = File.OpenRead( path ) )
            {
                appConfig = JsonSerializer.Deserialize<VRAppConfig>( stream, JsonSerializerOptions.Default );
            }
        }
        catch ( Exception ex )
        {
            Debug.WriteLine( $"Couldn't read VR App Config {path} - {ex}" );
        }

        return appConfig;
    }
}
