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
    public required string Path { get; set; }
    public required HashSet<string> Apps { get; set; }
}

public class SteamLibraries
{
    public required List<SteamLibrary> Libraries { get; set; }

    // --- //

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

    private const string SteamAppsPath = "steamapps";
    private const string CommonPath = "common";

    private const string SteamLibraryFolderVDF = "libraryfolders.vdf";
    private const string SteamAppManifestFormat = "appmanifest_{0}.acf";

    public static List<SteamLibrary> GetLibraries( OpenVRPaths openVRPaths )
    {
        List<SteamLibrary> libraries = new();

        try
        {
            KVObject? librariesData;

            var vdfPath = Path.Combine( openVRPaths.ConfigPath, SteamLibraryFolderVDF );
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
