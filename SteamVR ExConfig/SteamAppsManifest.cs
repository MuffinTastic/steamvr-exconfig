using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamVR_ExConfig;

public class SteamAppsManifestLanguage
{
    [JsonPropertyName( "name" )]
    public required string Name { get; set; }
}

public class SteamAppsManifestApp
{
    [JsonPropertyName( "app_key" )]
    public required string AppKey { get; set; }
    [JsonPropertyName( "strings" )]
    public required Dictionary<string, SteamAppsManifestLanguage> Strings { get; set; }
}

public class SteamAppsManifest
{
    [JsonPropertyName( "applications" )]
    public required List<SteamAppsManifestApp> Applications { get; set; }

    // --- //

    private const string LanguageEnUS = "en_us";

    public string? GetNameForKey( string key )
    {
        var app = Applications.Where( a => a.AppKey == key ).FirstOrDefault();
        if ( app is null )
            return null;

        var name = app.Strings
            .Where( kv => kv.Key == LanguageEnUS )
            .Select( kv => kv.Value.Name )
            .FirstOrDefault();

        return name;
    }

    // --- //

    public static SteamAppsManifest Load( string filePath )
    {
        using ( var stream = File.OpenRead( filePath ) )
        {
            var manifest = JsonSerializer.Deserialize<SteamAppsManifest>( stream, JsonSerializerOptions.Default );

            if ( manifest is null )
                throw new JsonException( "Couldn't parse SteamApps manifest - got null object" );

            return manifest;
        }
    }
}