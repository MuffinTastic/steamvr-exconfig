﻿using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamVR_ExConfig;


public class OpenVRPaths
{
    public required string ConfigPath { get; set; }
    public required string VRAppConfigPath { get; set; }
    public required string RuntimePath { get; set; }
    public required List<string> ExternalDrivers { get; set; }

    // --- //

    private static string OpenVRPathsFile = "openvr/openvrpaths.vrpath";
    private static string VRAppConfigDirName = "vrappconfig";

    public static OpenVRPaths Load( Config config )
    {
        if ( config.OpenVRRegistryFilePath is null )
        {
            config.OpenVRRegistryFilePath = FindRegistryPath();
            config.SaveToFile();
        }

        return ReadRegistryFile( config.OpenVRRegistryFilePath );
    }

    private static string FindRegistryPath()
    {
        string? pathBase = null;

        if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            pathBase = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
        //else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
        //    openvrPathsBase = ;

        if ( pathBase is null )
            throw new ArgumentNullException( "OpenVR Path Base is null - unsupported platform?" );

        return Path.Combine( pathBase, OpenVRPathsFile );
    }

    private static OpenVRPaths ReadRegistryFile( string filePath )
    {
        using ( var stream = File.OpenRead( filePath ) )
        {
            var openvrPathsJson = JsonSerializer.Deserialize<OpenVRPathsJson>( stream, JsonSerializerOptions.Default );

            if ( openvrPathsJson is null )
                throw new ArgumentNullException( "OpenVR registry contained a null object" );

            var configPath = openvrPathsJson.Config[0];
            var vrAppConfigPath = Path.Combine( configPath, VRAppConfigDirName );
            var runtimePath = openvrPathsJson.Runtime[0];
            var externalDrivers = openvrPathsJson.ExternalDrivers;

            var openVRPaths = new OpenVRPaths()
            {
                ConfigPath = configPath,
                VRAppConfigPath = vrAppConfigPath,
                RuntimePath = runtimePath,
                ExternalDrivers = externalDrivers
            };

            return openVRPaths;
        }
    }
}

public class OpenVRPathsJson
{
    [JsonPropertyName( "config" )]
    public required List<string> Config { get; set; }
    
    [JsonPropertyName( "runtime" )]
    public required List<string> Runtime { get; set; }

    [JsonPropertyName( "external_drivers" )]
    public required List<string> ExternalDrivers { get; set; }
}