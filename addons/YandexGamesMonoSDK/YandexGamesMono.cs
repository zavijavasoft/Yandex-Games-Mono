using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Yandex Games Mono SDK implementation.
/// </summary>
public class YandexGamesMono : YandexGamesSdkFacade
{

    const string TAG = "YandexGamesMono: ";
    private bool _notSupported = false;
    private bool _launchAllowed = false;
    private bool _enteredToTree = false;

    private JavaScriptObject _ysdk = null;

    private JavaScriptObject _payments = null;

    private JavaScriptObject _leaderboards = null;

    private JavaScriptObject _player = null;

    private JavaScriptObject _window = null;
    private JavaScriptObject Window { 
        get
        { 
            if (_window == null)
            {
                _window = JavaScript.GetInterface("window");
            }
            return _window;
        } 
    }

    private bool _ready = false;
    private bool _readyAllowed = false;
    public bool ReadyAllowed
    {
        get => _readyAllowed;
        set
        {
            _readyAllowed = value;
            MaybeReady();
        }
    }


    private YandexGamesStartupConfig _config;

    public override void _Ready()
    {
        _notSupported = !CheckPlatform();
        _enteredToTree = true;
        MaybeLaunched();
    }

    /// <summary>
    /// Launch Yandex Games SDK.
    /// </summary>
    /// <param name="config">Startup configuration </param>
    public override void LaunchSdk(YandexGamesStartupConfig config)
    {
        _config = config;
        _launchAllowed = true;
        MaybeLaunched();
    }

    private void MaybeLaunched()
    {
        if (_notSupported)
        {
            return;
        }
        if (_ysdk == null && _launchAllowed && _enteredToTree)
        {
            InitGame();
        }
        
    }

    /**
    *   Game start - MaybeReady()
    *   https://yandex.ru/dev/games/doc/ru/sdk/sdk-gameready
    */
    private void MaybeReady()
    {
        if (ReadyAllowed && _ysdk != null && !_ready)
        {

            // ysdk.features.LoadingAPI?.ready();
            DebugPrint("Game ready");
            JSWrapper.Call(_ysdk, "features.LoadingAPI.ready");
            _ready = true;
        }
    }
    /**
    *   Game start - InitGame()
    *
    *   https://yandex.ru/dev/games/doc/en/sdk/sdk-gameready
    *   auto-call from _ready()
    */
    private void InitGame()
    {
        DebugPrint("InitGame");
        JSWrapper.AsyncCall(Window, "YaGames.init")
            .Then(this, nameof(JsCallbackInitGame))
            .Catch(this, nameof(JsCallbackSdkError));
    }

    private void JsCallbackInitGame(object [] args)
    {
        DebugPrint("JsCallbackInitGame");
        _ysdk = args[0] as JavaScriptObject;
        SetUpEnvironment();

        EmitSignal(nameof(OnInitGame));

        GetPayments();
        GetPlayer();

        MaybeReady();
    }

    private void SetUpEnvironment()
    {
        // YSDK environment https://yandex.ru/dev/games/doc/ru/sdk/sdk-environment
        Environment.appId = JSWrapper.GetString(_ysdk, "environment.app.id");
        Environment.lang = JSWrapper.GetString(_ysdk, "environment.i18n.lang");
        Environment.tld = JSWrapper.GetString(_ysdk, "environment.i18n.tld");
        Environment.payload = JSWrapper.GetString(_ysdk, "environment.payload");

        DebugPrint("Environment:",
            "app.id", Environment.appId,
            ", lang:", Environment.lang,
            ", tld:", Environment.tld,
            ", payload:", Environment.payload);

    }   

    private void GetPayments() {
        DebugPrint("GetPayments");
        var result = _ysdk.Call("getPayments") as JavaScriptObject;
        var promise = new JavaScriptPromise(result, Window);

        promise.Then(this, nameof(JsCallbackGetPayments)).Catch(this,nameof(JsCallbackSdkError));

        EmitSignal(nameof(OnGetPayments));
    }

    private void JsCallbackGetPayments(object [] args)
    {
        DebugPrint("Payments is available");
        _payments = args[0] as JavaScriptObject;

        EmitSignal(nameof(OnGetPayments));
    }

    private void GetLeaderboards()
    {
        DebugPrint("GetLeaderboards");
        JSWrapper.AsyncCall(_ysdk, "getLeaderboards")
            .Then(this, nameof(JsCallbackGetLeaderboards))
            .Catch(this, nameof(JsCallbackSdkError));
    }

    private void JsCallbackGetLeaderboards(object [] args)
    {
        DebugPrint("Leaderboards is available");
        _leaderboards = args[0] as JavaScriptObject;

        GetLeaderboardDescription(_config.defaultLeaderboard);
    }

    private void GetPlayer()
    {
        DebugPrint("GetPlayer");
        JSWrapper.AsyncCall(_ysdk, "getPlayer", new object [] { "{scopes: true}" })
            .Then(this, nameof(JsCallbackGetPlayer))
            .Catch(this, nameof(JsCallbackSdkError));
    }

    private void JsCallbackGetPlayer(object [] args)
    {
        _player = args[0] as JavaScriptObject;
        var mode = JSWrapper.Call(_player, "getMode") as string;
        DebugPrint("Player is available in mode: ", mode);
        if (mode != "lite")
        {
            SetUpPlayer();
            GetData();
        }
        else 
        {
            EmitSignal(nameof(OnGetPlayer), false);
            EmitSignal(nameof(OnGetData), "");
        }
        GetLeaderboards();
    }

    private void SetUpPlayer()
    {
        if (_player != null)
        {
            Profile.authorized = true;
            Profile.uniqueId = _player.Call("getUniqueID") as string;
            Profile.name = _player.Call("getName") as string;
            Profile.avatarUrlSmall = _player.Call("getPhoto", "small") as string;
            Profile.avatarUrlMedium = _player.Call("getPhoto", "medium") as string;
            Profile.avatarUrlLarge = _player.Call("getPhoto", "large") as string;
            JSWrapper.AsyncCall(_player, "getIDsPerGame")
                .Then(this, nameof(JsCallbackGetPlayerIdsPerGame))
                .Catch(this, nameof(JsCallbackSdkError));

            DebugPrint("Player:",
                "uniqueId", Profile.uniqueId,
                ", name:", Profile.name,
                ", avatarUrlSmall:", Profile.avatarUrlSmall,
                ", avatarUrlMedium:", Profile.avatarUrlMedium,
                ", avatarUrlLarge:", Profile.avatarUrlLarge);
        }
    }

    private void JsCallbackGetPlayerIdsPerGame(object [] args)
    {
        var ids = args[0] as JavaScriptObject;
        var array = new JavaScriptArray(ids).ToList();
        DebugPrint($"Player IDs per game is available: {array}");
        if (array != null)
        {
            foreach (JavaScriptObject id in array.Cast<JavaScriptObject>())
            {
                int appId = id.Get("appID") as int? ?? -1;
                string userID = id.Get("userID") as string;
                Profile.IDsPerGame.Add(appId, userID);
            }
        }
        EmitSignal(nameof(OnGetPlayer), true);
    }

    public override void GetData()
    {
        DebugPrint("GetData");
        var result = _player.Call("getData") as JavaScriptObject;
        var promise = new JavaScriptPromise(result, Window);

        promise.Then(this, nameof(JsCallbackGetData)).Catch(this,nameof(JsCallbackSdkError));
    }

    private void JsCallbackGetData(object [] args)
    {
        var data = args[0] as string;
        DebugPrint("Data is available ", data);
        EmitSignal(nameof(OnGetData), data??"");
    }

    public override void SetData(string data)
    {
        DebugPrint("SetData", data);
        _ysdk.Call("setData", data);
    }

    public override void GetLeaderboardDescription(string leaderboard)
    {
        DebugPrint("GetLeaderboardDescription", leaderboard);
        var result =_leaderboards.Call("getLeaderboardDescription", leaderboard) as JavaScriptObject;
        var promise = new JavaScriptPromise(result, Window);

        promise.Then(this, nameof(JsCallbackGetLeaderboardDescription))
            .Catch(this,nameof(JsCallbackSdkError));
    }

    private void JsCallbackGetLeaderboardDescription(object [] args)
    {
        var description = args[0] as JavaScriptObject;
        string lb = ParseLeaderboardDecription(description);
        EmitSignal(nameof(OnGetLeaderboardDescription), lb);
    }

    private string ParseLeaderboardDecription(JavaScriptObject lbDescription,
                    YGLeaderboardDecription extraLbDescription = null)
    {
        var name = lbDescription.Get("name") as string;
        DebugPrint($"ParseLeaderboardDecription {name}");
        var lb = extraLbDescription;
        if (lb == null)
        {
            lb = LeaderboardsDescriptions.ContainsKey(name)
                ? LeaderboardsDescriptions[name]
                : null;
            if (lb == null)
            {
                lb = new YGLeaderboardDecription() { name = name };
                LeaderboardsDescriptions.Add(name, lb);
            }
        }
        else {
            lb.name = name;
        }
        lb.appId = lbDescription.Get("appID") as int? ?? -1;
        lb.defaultLeaderboard = lbDescription.Get("default") as bool? ?? false;
        lb.sortOrder = JSWrapper.GetBool(lbDescription, "description.invert_sort_order");
        lb.descriptionType = JSWrapper.GetString(lbDescription, "description.score_format.type");
        lb.decimalOffset = JSWrapper.GetInt(lbDescription, "description.score_format.options.decimal_offset");

        var title = lbDescription.Get("title") as JavaScriptObject;
        var titleDict = new JavaScriptDictionary(title);
        lb.title = titleDict
            .ToDictionary()
            .Select(x => new KeyValuePair<string, string>(x.Key, x.Value as string))
            .ToDictionary(x => x.Key, x => x.Value);
        DebugPrint("Leaderboard description: ",
            lb.name, lb.appId, lb.defaultLeaderboard,
            lb.sortOrder, lb.descriptionType, lb.decimalOffset, lb.title.Count);
        for (int i = 0; i < lb.title.Count; i++)
        {
            DebugPrint($"Title {i}: {lb.title.ElementAt(i).Key} - {lb.title.ElementAt(i).Value}");
        }
        return name;
    }

    private bool CheckPlatform()
    {
        if (!OS.HasFeature("HTML5"))
        {
            GD.Print("Yandex Games Mono SDK is not supported on not-HTML5 platform");
            return false;
        }

        GD.Print("Yandex Games Mono SDK works on HTML5 platform");
        return true;
    }

    private void JsCallbackSdkError(object [] args)
    {
        DebugPrint("JsCallbackSdkError", args[0] as string);
    }

    public override void SetLeaderboardScore(string leaderboard, int score, string extraData = null) {
        DebugPrint("SetLeaderboardScore", leaderboard, score, extraData);
        _leaderboards.Call("setLeaderboardScore", leaderboard, score, extraData);
    }

    public override void GetLeaderboardPlayerEntry(string leaderboard) {
        DebugPrint("GetLeaderboardPlayerEntry", leaderboard);
        var result = _leaderboards.Call("getLeaderboardPlayerEntry", leaderboard) as JavaScriptObject;
        var promise = new JavaScriptPromise(result, Window);

        promise.Then(this, nameof(JsCallbackGetLeaderboardPlayerEntry))
            .Catch(this,nameof(JsErrorCallbackGetLeaderboardPlayerEntry));
    }

    private void JsCallbackGetLeaderboardPlayerEntry(object [] args)
    {
        DebugPrint("Leaderboard player entry is available");
        var entry = args[0] as JavaScriptObject;
        ParseLeaderboardEntry(entry);
        EmitSignal(nameof(OnGetLeaderboards), true);
    }

    private void ParseLeaderboardEntry(JavaScriptObject entry, YGLeaderboardEntry lbEntry = null)
    {
        lbEntry ??= LeaderboardEntry;
        lbEntry.score = entry.Get("score") as int? ?? 0;
        lbEntry.extraData = entry.Get("extraData") as string;
        lbEntry.rank = entry.Get("rank") as int? ?? -1;
        lbEntry.getAvatarSmall = JSWrapper.Call(entry, "player.getAvatarSrc",
            new object[] { "small" }) as string;
        lbEntry.getAvatarMedium = JSWrapper.Call(entry, "player.getAvatarSrc",
            new object[] { "medium" }) as string;
        lbEntry.getAvatarLarge = JSWrapper.Call(entry, "player.getAvatarSrc",
            new object[] { "large" }) as string;
        lbEntry.getAvatarRetinaSmall = JSWrapper.Call(entry, "player.getAvatarSrcSet",
            new object[] { "small" }) as string;
        lbEntry.getAvatarRetinaMedium = JSWrapper.Call(entry, "player.getAvatarSrcSet",
            new object[] { "medium" }) as string;
        lbEntry.getAvatarRetinaLarge = JSWrapper.Call(entry, "player.getAvatarSrcSet",
            new object[] { "large" }) as string;
        lbEntry.lang = entry.Get("player.lang") as string;
        lbEntry.publicName = entry.Get("player.publicName") as string;
        lbEntry.uniqueID = entry.Get("player.uniqueID") as string;
        lbEntry.avatarPermission = entry.Get("player.scopePermissions.avatar") as string;
        lbEntry.publicNamePermission = entry.Get("player.scopePermissions.public_name") as string;
        lbEntry.formattedScore = entry.Get("formattedScore") as string;
    }
    private void JsErrorCallbackGetLeaderboardPlayerEntry(object [] args)
    {
        DebugPrint("Leaderboard player entry is available");
        EmitSignal(nameof(OnGetLeaderboards), false);
    }

    public override void GetLeaderboardEntries(string leaderboard,
        bool includeUser = false,
        int quantityAround = 5,
        int quantityTop = 5) 
    {
        DebugPrint($"GetLeaderboardEntries {leaderboard}");
        var result = _leaderboards.Call("getLeaderboardEntries", leaderboard,
            includeUser, quantityAround, quantityTop) as JavaScriptObject;
        var promise = new JavaScriptPromise(result, Window);

        promise.Then(this, nameof(JsCallbackGetLeaderboardEntries))
            .Catch(this,nameof(JsCallbackSdkError));

    }

    private void JsCallbackGetLeaderboardEntries(object [] args)
    {
        DebugPrint("Leaderboard entries is available");
        var result = args[0] as JavaScriptObject;
        LeaderboardEntryResult.Reset();
        var leaderboard = result.Get("leaderboard") as JavaScriptObject;
        var name = ParseLeaderboardDecription(leaderboard, LeaderboardEntryResult.description);

        var ranges = result.Get("ranges") as JavaScriptObject;
        var rangesArray = new JavaScriptArray(ranges).ToList();

        var entries = result.Get("entries") as JavaScriptObject;
        var array = new JavaScriptArray(entries).ToList();
        if (array != null)
        {
            foreach (JavaScriptObject entry in array.Cast<JavaScriptObject>())
            {
                var lbEntry = new YGLeaderboardEntry();
                ParseLeaderboardEntry(entry, lbEntry);
                LeaderboardEntryResult.entries.Add(lbEntry);
            }
        }
        EmitSignal(nameof(OnGetLeaderboardEntries), name);
    }

    private void JsErrorCallbackGetLeaderboardEntries(object [] args)
    {
        DebugPrint("Leaderboard entries is available");
        EmitSignal(nameof(OnGetLeaderboards), false);
    }

    public override void ShowFullscreenAdv()
    {
        DebugPrint("ShowFullscreenAdv");
        var cbWrapper = new JavaScriptCallbackWrapper(_ysdk, "adv.showFullscreenAdv")
            .AddCallback("OnClose", this, nameof(JsCallbackShowFullscreenAdvClose))
            .AddCallback("OnError", this, nameof(JsCallbackShowFullscreenAdvError));
        cbWrapper.Call();    
    }

    private void JsCallbackShowFullscreenAdvClose(object [] args)
    {
        bool wasShown = args[0] as bool? ?? false;
        DebugPrint($"Fullscreen adv is closed, wasShown: {wasShown}");
        EmitSignal(nameof(OnShowFullscreenAdv), wasShown, false, "");
    }

    private void JsCallbackShowFullscreenAdvError(object [] args)
    {
        string error = args[0] as string;
        DebugPrint("Fullscreen adv error");
        EmitSignal(nameof(OnShowFullscreenAdv), false, true, error);
    }

    public override void ShowRewardedVideo()
    {
        DebugPrint("ShowRewardedVideo");
        var cbWrapper = new JavaScriptCallbackWrapper(_ysdk, "adv.showRewardedVideo")
            .AddCallback("OnOpen", this, nameof(JsCallbackShowRewardedVideoOpen))
            .AddCallback("OnRewarded", this, nameof(JsCallbackShowRewardedVideoRewarded))
            .AddCallback("OnClose", this, nameof(JsCallbackShowRewardedVideoClose))
            .AddCallback("OnError", this, nameof(JsCallbackShowRewardedVideoError));
        cbWrapper.Call();
    }

    private void JsCallbackShowRewardedVideoOpen(object [] args)
    {
        DebugPrint("Rewarded video is opened");
        EmitSignal(nameof(OnShowRewardedVideo), YGRewardedResult.Open, "");
    }

    private void JsCallbackShowRewardedVideoRewarded(object [] args)
    {
        DebugPrint("Rewarded video is rewarded");
        EmitSignal(nameof(OnShowRewardedVideo), YGRewardedResult.Rewarded, "");
    }

    private void JsCallbackShowRewardedVideoClose(object [] args)
    {
        bool wasShown = args[0] as bool? ?? false;
        DebugPrint($"Rewarded video is closed, wasShown: {wasShown}");
        EmitSignal(nameof(OnShowRewardedVideo), YGRewardedResult.Closed, "");
    }

    private void JsCallbackShowRewardedVideoError(object [] args)
    {
        string error = args[0] as string;
        DebugPrint("Rewarded video error");
        EmitSignal(nameof(OnShowRewardedVideo), YGRewardedResult.Error, error);
    }

    private void DebugPrint(params object[] args)
    {
#if YG_DEBUG
        object[] sparsedArgs = new object[args.Length * 2];
        for (int i = 0; i < args.Length; i++)
        {
            sparsedArgs[i * 2] = args[i];
            sparsedArgs[i * 2 + 1] = " ";
        }
        object[] debugArgs = new object[sparsedArgs.Length + 1];
        debugArgs[0] = TAG;
        sparsedArgs.CopyTo(debugArgs, 1);
        GD.Print(debugArgs);
#endif
    }

    private void DebugJSConsoleLogObject(object [] args)
    {
#if YG_DEBUG
        JavaScriptObject jsWindow = JavaScript.GetInterface("console");
        jsWindow.Call("log", TAG, args);
#endif
    }
}
