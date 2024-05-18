using Godot;
using System.Collections.Generic;

/// <summary>
/// This class is a facade for the Yandex Games SDK.
/// </summary>
public abstract partial class YandexGamesSdkFacade : Node
{
    /// <summary>
    /// Property with Yandex Games SDK environment.
    /// Available after sucessfull YaGames.init call.
    /// </summary>
    public YGEnvironment Environment { get; private set; } =  new YGEnvironment();

    /// <summary>
    /// Property with player profile.
    /// Available after sucessfull ysdk.getPlayer() call when user is authorized.
    public YGProfile Profile { get; private set; } = new YGProfile();

    /// <summary>
    /// Property with all leaderboard descriptions.
    /// Available after sucessfull ysdk.getLeaderboards() call.
    /// Represents a dictionary with leaderboard name as a key and <see cref="YGLeaderboardDecription"/> as a value.
    /// </summary>
    public Dictionary<string, YGLeaderboardDecription> LeaderboardsDescriptions { get; private set;}
        = new Dictionary<string, YGLeaderboardDecription>();

    /// <summary>
    /// Property with leaderboard entry.
    /// Available after sucessfull ysdk.getLeaderboardPlayerEntry() call.
    /// </summary>
    public YGLeaderboardEntry LeaderboardEntry { get; private set; } = new YGLeaderboardEntry();

    /// <summary>
    /// Property with leaderboard entries.
    /// Available after sucessfull ysdk.getLeaderboardEntries() call.
    /// </summary>
    public YGLeaderboardEntryResult LeaderboardEntryResult { get; private set; } = new YGLeaderboardEntryResult();

    /// <summary>
    /// This signal is emitted when the SDK is initialized.
    /// You can retrive environment data from <see cref="Environment"/>.
    /// </summary>
    [Signal]
    public delegate void OnInitGame();

    /// <summary>
    /// This signal is emitted when interstiaial ad is shown.
    /// </summary>
    /// <param name="wasShown"> true if the ad was shown, false otherwise</param>
    /// <param name="error"> true if an error occurred, false otherwise</param>
    /// <param name="errorMessage"> error message</param>
    [Signal]
    public delegate void OnShowFullscreenAdv(bool wasShown, bool error, string errorMessage);

    /// <summary>
    /// This signal is emitted when rewarded video is shown.
    /// </summary>
    /// <param name="rewardedResult"> result value from <see cref="YGRewardedResult"/>YGRewardedResult constants</param>
    /// <param name="errorMessage"> error message</param>
    [Signal]
    public delegate void OnShowRewardedVideo(int rewardedResult, string errorMessage);

    /// <summary>
    /// This signal is emitted when the player data is received.
    /// </summary>
    /// <param name="jsonData"> JSON string with player data</param>
    [Signal]
    public delegate void OnGetData(string jsonData);

    /// <summary>
    /// This signal is emitted when payments are received.
    /// </summary>
    [Signal]
    public delegate void OnGetPayments();

    /// <summary>
    /// This signal is emitted when the player info is received.
    /// </summary>
    /// <param name="authorized"> true if the player is authorized, false otherwise</param>
    [Signal]
    public delegate void OnGetPlayer(bool authorized);

    [Signal]
    public delegate void OnPurchaseThen(/*object purchase*/);

    [Signal]
    public delegate void OnPurchaseCatch(int purchase_id);

    [Signal]
    public delegate void OnGetPurchases(/*List<object> purchases*/);

    [Signal]
    public delegate void OnGetLeaderboards(bool playerHasEntry);

    /// <summary>
    /// This signal is emitted when the leaderboard description is received.
    /// </summary>
    /// <param name="leaderboard"> leaderboard name </param>
    [Signal]
    public delegate void OnGetLeaderboardDescription(string leaderboard);

    /// <summary>
    /// This signal is emitted when the leaderboard entries are received.
    /// </summary>
    /// <param name="leaderboard"> leaderboard name </param>
    [Signal]
    public delegate void OnGetLeaderboardEntries(string leaderboard);

    /// <summary>
    /// This signal is emitted when the player's leaderboard entry is received.
    /// </summary>
    /// <param name="leaderboard"> leaderboard name </param>
    [Signal]
    public delegate void OnGetLeaderboardPlayerEntry(string leaderboard);


    [Signal]
    public delegate void OnIsAvailableMethod(bool available, string method_name);

    [Signal]
    public delegate void OnCanReview(bool canReview);

    [Signal]
    public delegate void OnRequestReview(bool requestReview);

    virtual public void LaunchSdk(YandexGamesStartupConfig config) {}

    /// <summary>
    /// Shows a fullscreen ad. See <see cref="OnShowFullscreenAdv"/> to handle the result.
    /// </summary>
    virtual public void ShowFullscreenAdv() {}

    /// <summary>
    /// Shows a rewarded video. See <see cref="OnShowRewardedVideo"/> to handle the result.
    /// </summary>
    virtual public void ShowRewardedVideo() {}

    /// <summary>
    /// Gets the player data. See <see cref="OnGetData"/> to handle the result.
    /// </summary>
    virtual public void GetData(){}

    /// <summary>
    /// Sets the player data.
    /// </summary>
    /// <param name="data"> JSON string with player data</param>
    virtual public void SetData(string data) {}

    /// <summary>
    /// Gets the leaderboard description. See <see cref="OnGetLeaderboardDescription"/> to handle the result.
    /// </summary>
    /// <param name="leaderboard"> leaderboard name </param>
    virtual public void GetLeaderboardDescription(string leaderboard) {}

    /// <summary>
    /// Gets the player's leaderboard entry. See <see cref="OnGetLeaderboardPlayerEntry"/> to handle the result.
    /// </summary>
    /// <param name="leaderboard"> leaderboard name </param>
    virtual public void GetLeaderboardPlayerEntry(string leaderboard) {}

    /// <summary>
    /// Gets the leaderboard entries. See <see cref="OnGetLeaderboardEntries"/> to handle the result.
    /// </summary>
    /// <param name="leaderboard"> leaderboard name </param>
    /// <param name="includeUser"> true if it need to add user score to result, false otherwise </param>
    /// <param name="quantityAround"> number of entries around user score </param>
    /// <param name="quantityTop"> number of top entries </param>
    virtual public void GetLeaderboardEntries(string leaderboard,
        bool includeUser,
        int quantityAround,
        int quantityTop) {}

    /// <summary>
    /// Sets the leaderboard score.
    /// </summary>
    /// <param name="leaderboard"> leaderboard name </param>
    /// <param name="score"> score value </param>
    /// <param name="extraData"> optional extra data </param>
    virtual public void SetLeaderboardScore(string leaderboard, int score, string extraData) {}
 
}