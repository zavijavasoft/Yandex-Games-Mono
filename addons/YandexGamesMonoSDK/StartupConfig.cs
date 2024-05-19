/// <summary>
/// Yandex Games startup configuration
/// </summary>
public class YandexGamesStartupConfig {

    /// <summary>
    /// If true, the SDK will be started next after _Ready() method
    /// No need to set ReadyAllowed property
    /// </summary>
    public bool startOnReady = false;

    /// <summary>
    /// The {scoped: true} parameter is required for the player info request.
    /// <see cref="https://yandex.ru/dev/games/doc/ru/sdk/sdk-player#getplayer"/>
    /// </summary>
    public bool isPlayerScoped = true;

    /// <summary>
    /// The default leaderboard name
    /// </summary>
    public string defaultLeaderboard = "default";
    
}