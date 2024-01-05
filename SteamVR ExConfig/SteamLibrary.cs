using System.Diagnostics;
using ValveKeyValue;

namespace SteamVR_ExConfig;

public class SteamLibrary
{
    public required string Path { get; set; }
    public required HashSet<string> Apps { get; set; }
}

/// <summary>
/// Steam Libraries!
/// We need this because OpenVR is too dumb to tell us if an app is really installed.
/// While we're at it, we can also read the names.
/// </summary>
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

    // --- //


    private (string, KVObject)? GetAppManifest( string appID )
    {
        // Get which library this app is installed in

        string? libraryPath = Libraries
            .Where( l => l.Apps.Contains( appID ) )
            .Select( l => l.Path )
            .FirstOrDefault();

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

        if ( manifest is null )
            return null;
        else
            return ( libraryPath, manifest! );
    }

    // --- //

    private const string SteamAppsPath = "steamapps";

    private const string SteamLibraryFolderVDF = "libraryfolders.vdf";
    private const string SteamAppManifestFormat = "appmanifest_{0}.acf";

    public static SteamLibraries GetLibraries( OpenVRPaths openVRPaths )
    {
        List<SteamLibrary> libraries = new();

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

        return new SteamLibraries()
        {
            Libraries = libraries
        };
    }
}
