using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using ValveKeyValue;

namespace SteamVR_ExConfig;

public class SteamLibrary
{
    public string? Path { get; set; }
    public HashSet<string>? Apps { get; set; }
}

public class SteamConfig
{
    public string? SteamPath { get; set; }

    [JsonIgnore]
    public string? ConfigPath { get; set; }
    [JsonIgnore]
    public string? VRAppConfigPath { get; set; }

    [JsonIgnore]
    public List<SteamLibrary>? Libraries { get; set; }

    // --- //

    public void SaveToFile()
    {
        try
        {
            using ( var stream = File.Open( SteamPathFile, FileMode.Create ) )
            {
                var options = new JsonSerializerOptions() { WriteIndented = true };
                stream.Write( Encoding.Default.GetBytes( JsonSerializer.Serialize( this, options ) ) );
            }
        }
        catch ( Exception ex )
        {
            Debug.WriteLine( $"Couldn't save ExConfig file {SteamPathFile}: {ex}" );
        }
    }

    public string? GetNameForAppID ( string appID )
    {
        var result = GetAppManifest( appID );
        if ( result is null )
            return null;

        var manifest = result.Value.Item2;

        return manifest["name"].ToString();
    }

    public string? GetInstallLocationForAppID( string appID )
    {
        var result = GetAppManifest( appID );
        if ( result is null )
            return null;

        var libraryPath = result.Value.Item1;
        var manifest = result.Value.Item2;
        var installDir = manifest["installdir"].ToString();

        return Path.Combine( libraryPath, CommonPath, installDir! );
    }

    // --- //


    private (string, KVObject)? GetAppManifest( string appID )
    {
        // Get which library this app is installed in

        string? libraryPath = null;

        foreach ( var library in Libraries! )
        {
            if ( library.Apps!.Contains( appID ) )
            {
                libraryPath = library.Path;
                break;
            }
        }

        // Not installed, we have no manifest to return
        if ( libraryPath is null )
            return null;


        var manifestName = string.Format( SteamAppManifestFormat, appID );
        KVObject? manifest = null;

        try
        {
            var manifestPath = Path.Combine( libraryPath, manifestName );

            using ( var stream = File.OpenRead( manifestPath ) )
            {
                var kv = KVSerializer.Create( KVSerializationFormat.KeyValues1Text );
                manifest = kv.Deserialize( stream );
            }
        }
        catch ( FileNotFoundException ex )
        {
            Debug.WriteLine( $"Couldn't find Steam App Manifest for {appID} - {ex.Message}" );
            return null;
        }
        catch ( Exception ex )
        {
            Debug.WriteLine( $"Couldn't read Steam App Manifest for {appID} - {ex}" );
            return null;
        }

        if ( manifest is null )
            return null;
        else
            return ( libraryPath, manifest! );
    }

    // --- //

    private const string SteamPathFile = "./steampath.json";

    private const string SteamRegistrySubKey = "SOFTWARE\\WOW6432Node\\Valve\\Steam";
    private const string DefaultWindowsSteamPath = "C:\\Program Files (x86)\\Steam";
    private const string DefaultLinuxSteamPath = "~/.steam/steam";

    private const string SteamConfigPath = "config";
    private const string SteamVRAppConfigPath = "vrappconfig";
    private const string SteamAppsPath = "steamapps";
    private const string CommonPath = "common";

    private const string SteamLibraryFolderVDF = "libraryfolders.vdf";
    private const string SteamAppManifestFormat = "appmanifest_{0}.acf";

    public static SteamConfig Load()
    {
        SteamConfig? config = null;

        try
        {
            config = LoadFromFile();

            Debug.WriteLine( $"Loaded Steam Path" );
        }
        catch ( Exception ex )
        {
            Debug.WriteLine( $"Couldn't read Steam Path {SteamPathFile} - {ex.Message}" );
        }

        if ( config is null )
        {
            Debug.WriteLine( "Generating Steam Path" );
            // Get Steam path first.

            var steamPath = AutoDetectSteamInstall();

            if ( steamPath is null )
            {
                if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
                    steamPath = DefaultWindowsSteamPath;
                else if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
                    steamPath = DefaultLinuxSteamPath;

                Debug.WriteLine( $"Couldn't autodetect Steam installation, falling back on default path" );
            }

            Debug.WriteLine( $"Steam path: {steamPath}" );


            config = new SteamConfig()
            {
                SteamPath = steamPath!,
            };

            config.SaveToFile();
        }

        config.ConfigPath = Path.Combine( config.SteamPath!, SteamConfigPath );
        config.VRAppConfigPath = Path.Combine( config.SteamPath!, SteamConfigPath, SteamVRAppConfigPath );

        config.Libraries = GetSteamLibraries( config.SteamPath! );

        return config;
    }

    private static SteamConfig? LoadFromFile()
    {
        using ( var stream = File.OpenRead( SteamPathFile ) )
        {
            return JsonSerializer.Deserialize<SteamConfig>( stream, JsonSerializerOptions.Default );
        }
    }

    private static string? AutoDetectSteamInstall()
    {
        if ( !RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            return null;

        Debug.WriteLine( "Autodetecting Steam installation from Windows Registry" );

        RegistryKey? key = Registry.LocalMachine.OpenSubKey( SteamRegistrySubKey );
        if ( key is null )
            return null;

        return key.GetValue( "InstallPath" ) as string;
    }

    private static List<SteamLibrary> GetSteamLibraries( string steamPath )
    {
        List<SteamLibrary> libraries = new();

        try
        {
            KVObject? librariesData;

            var vdfPath = Path.Combine( steamPath, SteamConfigPath, SteamLibraryFolderVDF );
            using ( var stream = File.OpenRead( vdfPath ) )
            {
                var kv = KVSerializer.Create( KVSerializationFormat.KeyValues1Text );
                librariesData = kv.Deserialize( stream );
            }

            foreach ( var libraryData in librariesData )
            {
                var path = libraryData["path"].ToString();
                var appsEnumerator = libraryData["apps"] as IEnumerable<KVObject>;
                var apps = appsEnumerator!.Select( app => app.Name.ToString() ).ToHashSet();

                if ( !apps.Any() )
                    continue;

                var library = new SteamLibrary()
                {
                    Path = Path.Combine( path!, SteamAppsPath ),
                    Apps = apps
                };

                libraries.Add( library );
            }
        }
        catch ( Exception ex )
        {
            Debug.WriteLine( $"Couldn't retrieve libraries: {ex}" );
        }

        return libraries;
    }
}
