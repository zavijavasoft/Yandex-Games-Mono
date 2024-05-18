/// <summary>
/// Yandex Games startup configuration
/// </summary>
public class YandexGamesStartupConfig {

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