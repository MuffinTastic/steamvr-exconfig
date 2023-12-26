using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SteamVR_ExConfig;

public class VRDriverManifest
{
    [JsonPropertyName( "name" )]
    public required string InternalName { get; set; }

    [JsonPropertyName( "alwaysActivate" )]
    public bool AlwaysActivate { get; set; } = false;
}

public class VRDriverSetting : IVRSetting
{
    [JsonPropertyName( "enable" )]
    public bool Enabled { get; set; }

    [JsonPropertyName( "blocked_by_safe_mode" )]
    public bool BlockedBySafeMode { get; set; }

    // --- //

    [JsonIgnore]
    public string? InternalName { get; set; }

    [JsonIgnore]
    public string ReadableName { get; set; }

    [JsonIgnore]
    public bool Dirty { get; set; }

    public void SetEnabled( bool enabled )
    {
        Enabled = enabled;
        Dirty = true;
    }
}

public class SteamVRConfig
{
    public required List<VRAppSetting> AppSettings;

    public required string ConfigFilePath { get; set; }
    public required List<VRDriverSetting> DriverSettings;

    // --- //

    public void Save()
    {
        foreach ( var app in AppSettings )
        {
            app.Save();
        }

        SaveDrivers();
    }

    private void SaveDrivers()
    {
        // Make a backup just so people don't scream at me...
        var backupFileDest = ConfigFilePath + ExConfigBackupFileExt;

        if ( !File.Exists( backupFileDest ) )
            File.Copy( ConfigFilePath, backupFileDest );


        var vrSettings = ReadSteamVRConfig( ConfigFilePath );

        // Wipe existing disabled drivers

        foreach ( var key in vrSettings.Keys )
        {
            var driverMatch = SteamVRDriverRegex.Match( key );
            if ( driverMatch.Groups.Count != 2 )
                continue;

            vrSettings.Remove( key );
        }

        // Add our own disabled drivers

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        foreach ( var driverSetting in DriverSettings! )
        {
            if ( driverSetting.Enabled )
                continue;

            var element = JsonSerializer.SerializeToElement( driverSetting, options );
            vrSettings.Add( "driver_" + driverSetting.InternalName, element );
        }

        using ( var stream = File.Open( ConfigFilePath, FileMode.Create ) )
        {
            var jsonOutput = JsonSerializer.Serialize( vrSettings, options );
            Debug.WriteLine( jsonOutput );
            stream.Write( Encoding.Default.GetBytes( jsonOutput ) );
        }
    }

    // --- //

    private const string SteamVRConfigFile = "steamvr.vrsettings";
    private const string ExConfigBackupFileExt = ".excfg.bkp";

    private static Regex SteamVRDriverRegex = new Regex( @"^driver_(?!lighthouse)(.+)$", RegexOptions.Compiled );
    private static string SteamVRDriverDirName = "drivers";

    private static Dictionary<string, string> KnownDrivers = new Dictionary<string, string>()
    {
        { "gamepad", "Gamepad Support" },
    };

    public static SteamVRConfig GetVRConfig( OpenVRPaths openVRPaths, SteamLibraries steamLibraries )
    {
        var appSettings = VRAppSetting.GetAppSettings( openVRPaths, steamLibraries );


        var configFilePath = Path.Combine( openVRPaths.ConfigPath, SteamVRConfigFile );
        var driverPath = Path.Combine( openVRPaths.RuntimePath, SteamVRDriverDirName );

        Debug.WriteLine( $"SteamVR Install Path: {openVRPaths.RuntimePath}" );

        var driverDirs = Directory.EnumerateDirectories( driverPath ).ToList();
        driverDirs.AddRange( openVRPaths.ExternalDrivers );

        var driverSettings = GetDriverSettings( configFilePath, driverDirs );


        var steamVRConfig = new SteamVRConfig()
        {
            AppSettings = appSettings,
            ConfigFilePath = configFilePath,
            DriverSettings = driverSettings,
        };

        return steamVRConfig;
    }

    private static List<VRDriverSetting> GetDriverSettings( string configFilePath, List<string> driverDirs )
    {
        var vrConfig = ReadSteamVRConfig( configFilePath );

        List<VRDriverSetting> driverSettings = new();

        // Get existing (disabled) drivers first)

        Dictionary<string, VRDriverSetting> existingSettings = new();

        foreach ( var item in vrConfig )
        {
            var jsonKey = item.Key;
            var jsonValue = (JsonElement) item.Value;

            var driverMatch = SteamVRDriverRegex.Match( jsonKey );
            if ( driverMatch.Groups.Count != 2 )
                continue;

            string driverName = driverMatch.Groups[1].Value;

            var existingSetting = JsonSerializer.Deserialize<VRDriverSetting>( jsonValue, JsonSerializerOptions.Default );

            existingSettings.Add( driverName, existingSetting! );
        }

        // Now populate the driver settings list with existing disabled and implicitly enabled settings
        // That's to say, if it's not blocked or disabled, we create a setting that's enabled, and merge it all together

        foreach ( var driverDir in driverDirs )
        {
            using ( var manifestStream = File.OpenRead( Path.Combine( driverDir, "driver.vrdrivermanifest" ) ) )
            {
                var driverManifest = JsonSerializer.Deserialize<VRDriverManifest>( manifestStream, JsonSerializerOptions.Default );
                if ( !(driverManifest?.AlwaysActivate ?? false ) )
                    continue;

                VRDriverSetting driverSetting;

                if ( existingSettings.TryGetValue( driverManifest.InternalName, out var existingSetting ) )
                {
                    if ( existingSetting.BlockedBySafeMode )
                    {
                        // I'm not implementing the UI for this. You can't make me!
                        existingSetting.BlockedBySafeMode = false;
                        existingSetting.SetEnabled( false );
                    }

                    driverSetting = existingSetting;
                }
                else
                {
                    driverSetting = new VRDriverSetting()
                    {
                        Enabled = true,
                        BlockedBySafeMode = false,
                    };
                }

                driverSetting.InternalName = driverManifest.InternalName;
                driverSetting.ReadableName = GetKnownDriverName( driverManifest.InternalName );

                driverSettings.Add( driverSetting );
            }
        }

        return driverSettings;
    }

    private static string GetKnownDriverName( string driverName )
    {
        string? name = driverName;

        // Match from Steam app manifests
        if ( KnownDrivers.GetValueOrDefault( driverName ) is string knownName )
        {
            name = knownName;
        }

        return name;
    }

    private static Dictionary<string, object> ReadSteamVRConfig( string filePath )
    {
        using ( var stream = File.OpenRead( filePath ) )
        {
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>( stream, JsonSerializerOptions.Default );
            
            if ( config is null )
                throw new JsonException( "Couldn't parse SteamVR settings - got null object" );

            return config;
        }
    }
}
