using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamVR_ExConfig;

public class Config
{
    [JsonPropertyName( "openvrpath" ), JsonRequired]
    public string? OpenVRRegistryFilePath { get; set; }

    // --- //

    private const string ConfigPath = "./config.json";

    public void SaveToFile()
    {
        Debug.WriteLine( $"Saving config to {ConfigPath}" );

        using ( var stream = File.Open( ConfigPath, FileMode.Create ) )
        {
            var options = new JsonSerializerOptions() { WriteIndented = true };
            stream.Write( Encoding.Default.GetBytes( JsonSerializer.Serialize( this, options ) ) );
        }
    }

    // --- //

    public static Config Load()
    {
        Config? config = null;

        try
        {
            config = LoadFromFile();

            if ( config is null )
                Debug.WriteLine( $"Config file {ConfigPath} contained a null object" );
        }
        catch ( FileNotFoundException )
        {
            Debug.WriteLine( $"Config file {ConfigPath} not found" );
        }

        return config ?? new Config();
    }

    private static Config? LoadFromFile()
    {
        using ( var stream = File.OpenRead( ConfigPath ) )
        {
            return JsonSerializer.Deserialize<Config>( stream, JsonSerializerOptions.Default );
        }
    }
}
