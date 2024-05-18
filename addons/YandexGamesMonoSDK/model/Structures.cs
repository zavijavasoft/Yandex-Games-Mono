using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Yandex Games SDK Environment
/// <see cref="https://yandex.ru/dev/games/doc/ru/sdk/sdk-environment"/> 
/// </summary>
public class YGEnvironment {
    public string appId;
    public string lang;
    public string tld;
    public string payload;

}

/// <summary>
/// Yandex Games player profile
/// <see cref="https://yandex.ru/dev/games/doc/ru/sdk/sdk-player#profile-data"/>
/// </summary>
/// <remarks>
/// Data depends on the user's authorization status.
/// </remarks>
public class YGProfile {

    /// <summary>
    /// Authorization status
    /// if true, the user is authorized and the profile data is valid
    /// </summary>
    public bool authorized;
    public string uniqueId;
    public string name;
    public string avatarUrlSmall;
    public string avatarUrlMedium;
    public string avatarUrlLarge;

    public readonly Dictionary<int, string> IDsPerGame = new ();
}

/// <summary>
/// Yandex Games leaderboard description
/// <see cref="https://yandex.ru/dev/games/doc/ru/sdk/sdk-leaderboard#format-otveta"/>
/// </summary>
public class YGLeaderboardDecription
{
    public int appId;
    public bool defaultLeaderboard;
    public bool sortOrder;
    public int decimalOffset;
    public string descriptionType;
    public string name;
    public Dictionary<string, string> title;
}

/// <summary>
/// Yandex Games leaderboard player entry
/// <see cref="https://yandex.ru/dev/games/doc/ru/sdk/sdk-leaderboard#format-otveta1"/>
/// </summary>
public class YGLeaderboardEntry
{
    public int score;
    public string extraData;
    public int rank;
    public string getAvatarSmall;
    public string getAvatarMedium;
    public string getAvatarLarge;
    public string getAvatarRetinaSmall;
    public string getAvatarRetinaMedium;
    public string getAvatarRetinaLarge;
    public string lang;
    public string publicName;
    public string uniqueID;
    public string avatarPermission;
    public string publicNamePermission;
    public string formattedScore;
}

/// <summary>
/// Yandex Games leaderboard entries result
/// <see cref="https://yandex.ru/dev/games/doc/ru/sdk/sdk-leaderboard#format-otveta2"/>
/// </summary>
public class YGLeaderboardEntryResult {
    public YGLeaderboardDecription description = new();
    public List<(int, int)> ranges = new();
    public int userRank;
    public List<YGLeaderboardEntry> entries = new();

    /// <summary>
    /// Reset all fields to default values
    /// </summary>
    public void Reset()
    {
        description = new();
        ranges = new();
        userRank = 0;
        entries = new();
    }
}

/// <summary>
/// Yandex Games rewarded video result.
/// Maps callbacks from Yandex Games SDK to integer values.
/// <see cref="https://yandex.ru/dev/games/doc/ru/sdk/sdk-adv#primer1"/>
/// </summary>
public class YGRewardedResult
{
    public const int Open = 0;
    public const int Rewarded = 1;
    public const int Closed = 2;
    public const int Error = -1;
}