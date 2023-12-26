﻿using System;
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
    public string? InternalName { get; set; }

    [JsonPropertyName( "alwaysActivate" )]
    public bool AlwaysActivate { get; set; }
}

public class OpenVRPaths
{
    [JsonPropertyName( "external_drivers" )]
    public List<string>? ExternalDrivers { get; set; }
}

public class VRDriverSetting : IVRSetting
{
    [JsonPropertyName( "enable" )]
    public bool Enabled { get; set; }

    [JsonPropertyName( "blocked_by_safe_mode" )]
    public bool BlockedBySafeMode { get; set; }

    // --- //

    [JsonIgnore]
    public string InternalName { get; set; }

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
    public string? ConfigFilePath { get; set; }

    public List<VRDriverSetting>? DriverSettings;

    // --- //

    public void SaveToFile()
    {
        // Make a backup just so people don't scream at me...
        var backupFileDest = ConfigFilePath! + ExConfigBackupFileExt;

        if ( !File.Exists( backupFileDest ) )
            File.Copy( ConfigFilePath!, backupFileDest );


        Dictionary<string, object>? vrSettings;

        using ( var stream = File.OpenRead( ConfigFilePath! ) )
        {
            vrSettings = JsonSerializer.Deserialize<Dictionary<string, object>>( stream, JsonSerializerOptions.Default );
        }

        if ( vrSettings is null )
        {
            throw new JsonException( "Couldn't parse SteamVR settings - got null object" );
        }

        // Wipe existing disabled drivers

        foreach ( var key in vrSettings.Keys )
        {
            var driverMatch = SteamVRDriverRegex.Match( key );
            if ( driverMatch.Groups.Count != 2 )
                continue;

            string driverName = driverMatch.Groups[1].Value;

            vrSettings.Remove( key );

            Debug.WriteLine( $"Wiped {key}" );
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

        using ( var stream = File.Open( ConfigFilePath!, FileMode.Create ) )
        {
            var jsonOutput = JsonSerializer.Serialize( vrSettings, options );
            Debug.WriteLine( jsonOutput );
            stream.Write( Encoding.Default.GetBytes( jsonOutput ) );
        }
    }

    // --- //

    private const string SteamVRAppID = "250820";
    private const string SteamVRConfigFile = "steamvr.vrsettings";
    private const string ExConfigBackupFileExt = ".excfg.bkp";

    private static Regex SteamVRDriverRegex = new Regex( @"^driver_(?!lighthouse)(.+)$", RegexOptions.Compiled );
    private static string SteamVRDriverPath = "drivers";

    private static string OpenVRPathsFile = "openvr/openvrpaths.vrpath";

    private static Dictionary<string, string> KnownVRDrivers = new Dictionary<string, string>()
    {
        { "gamepad", "Gamepad Support" },
    };

    public static SteamVRConfig GetVRConfig( SteamConfig steamConfig )
    {
        var configFilePath = Path.Combine( steamConfig.ConfigPath!, SteamVRConfigFile );

        var steamVRConfig = new SteamVRConfig()
        {
            ConfigFilePath = configFilePath
        };

        try
        {
            var installPath = steamConfig.GetInstallLocationForAppID( SteamVRAppID );
            Debug.WriteLine( $"SteamVR Install Path: {installPath}" );
            var driverPath = Path.Combine( installPath!, SteamVRDriverPath );
            Debug.WriteLine( $"SteamVR Driver Path: {driverPath}" );


            var driverDirs = Directory.EnumerateDirectories( driverPath! ).ToList();
            var externalDriverPaths = GetExternalDriverDirectories();

            if ( externalDriverPaths is not null )
            {
                driverDirs.AddRange( externalDriverPaths );
            }

            using ( var stream = File.OpenRead( steamVRConfig.ConfigFilePath ) )
            {
                var vrSettings = JsonSerializer.Deserialize<Dictionary<string, object>>( stream, JsonSerializerOptions.Default );

                steamVRConfig.DriverSettings = GetVRDriverSettings( vrSettings!, driverDirs );
            }
        }
        catch ( Exception ex )
        {
            Debug.WriteLine( $"Couldn't read SteamVR config {configFilePath} - {ex}" );
            steamVRConfig.DriverSettings = new();
        }


        return steamVRConfig;
    }

    private static List<string>? GetExternalDriverDirectories()
    {
        string? openvrPathsBase = null;


        if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            openvrPathsBase = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
        //else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
        //    openvrPathsBase = ;

        if ( openvrPathsBase is null )
            return null;

        var openvrPathsFilePath = Path.Combine( openvrPathsBase, OpenVRPathsFile );
        List<string>? externalDriverPaths = null;

        try
        {
            using ( var stream = File.OpenRead( openvrPathsFilePath ) )
            {
                var openvrPaths = JsonSerializer.Deserialize<OpenVRPaths>( stream, JsonSerializerOptions.Default );
                externalDriverPaths = openvrPaths?.ExternalDrivers;
            }
        }
        catch ( Exception ex )
        {
            Debug.WriteLine( $"Couldn't retreive OpenVR paths: {ex}" );
        }

        return externalDriverPaths;
    }

    private static List<VRDriverSetting> GetVRDriverSettings( Dictionary<string, object> vrSettings, List<string> driverDirs )
    {
        List<VRDriverSetting> driverSettings = new();

        // Get existing (disabled) drivers first)

        Dictionary<string, VRDriverSetting> existingSettings = new();

        foreach ( var item in vrSettings )
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

                if ( existingSettings.TryGetValue( driverManifest!.InternalName, out var existingSetting ) )
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
                driverSetting.ReadableName = GetKnownVRDriverName( driverManifest.InternalName );

                driverSettings.Add( driverSetting );
            }
        }

        return driverSettings;
    }

    private static string GetKnownVRDriverName( string driverName )
    {
        string? name = driverName;

        // Match from Steam app manifests
        if ( KnownVRDrivers.GetValueOrDefault( driverName ) is string knownName )
        {
            name = knownName;
        }

        return name;
    }
}
