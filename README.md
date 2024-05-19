# Yandex-Games-Mono
[![Godot](https://img.shields.io/badge/Godot%20Engine-3.5.3-blue.svg)](https://github.com/godotengine/godot/)
![GitHub License](https://img.shields.io/github/license/zavijavasoft/Yandex-Games-Mono)
![GitHub Repo stars](https://img.shields.io/github/stars/zavijavasoft/Yandex-Games-Mono)

__If you have ideas for improvement, write me to [telegram group](https://t.me/zavijavasoft_feedback) / Issues / pull, I’ll read everything__

__My Telegram group [@zavijavasoft_feedback](https://t.me/zavijavasoft_feedback)__

## How to use

### Prerequisites

* Add `<script src="https://yandex.ru/games/sdk/v2"></script>` to Project/export/html5/head include.
### Launch SDK

- In your Godot singleton create variable and bind it to `YandexGamesMono` singleton, then connect all its signals to handlers:
```csharp
private YandexGamesMono _ysdk;

public override _Ready()
{
	_ysdk = GetNode<YandexGamesMono>("/root/YandexGames");
	_ysdk.Connect(nameof(YandexGamesMono.OnInitGame), this, nameof(YourHandler_OnInitGame));
	...
}
```

- In suitable place of your singleton lifecycle call `LaunchSdk`:
```csharp
	_ysdk.LaunchSdk(new YandexGamesStartupConfig() {
			startOnReady = false,
			defaultLeaderboard = "BestResults",
			isPlayerScoped = true
	});
```

* After that the addon automatically calls:
	* `initGame()`  with signal `OnInitGame` on completion,
	* `getPayments()` with signal `OnGetPayments` on completion,
	* `getPlayer()` with signal `OnGetPlayer` on completion,
	* `getData()` with signal `OnGetData` on completion,
	* `getLeaderboards()`, with signal `OnGetLeaderboardDesription` on completion (if you do not use leaderboards, just pass `null` to `defaultLeaderboard` startup param of `LaunchSdk` and `getLeaderboards` won't be called),
	* if your passed `startOnReady = true` to startup param of `LaunchSdk`, immediately after calling functions above inner `ysdk.features.LoadingAPI.ready()` will be called. Otherwise you must set `_ysdk.ReadyAllowed = true` when your game will be ready for interaction with user.
* For more understanding, you can read [YandexGamesMono.cs](addons/YandexGamesMonoSDK/YandexGamesMono.cs) and sdk documentation

When `OnInitGame` signal is received, you can access to Yandex SDK environment, i. e. server-defined language from `YandexGamesMono.Environment` property:

```csharp
public void YourHandler_OnInitGame()
{
	string language = _ysdk.Environment.lang;
	...
}
```

Signal `OnGetPayments` doesn't need special handling, just inform client that payments are available.

When `OnGetPlayer` signal is received, you can access to player's data from `YandexGamesMono.Profile` property if player is authorized in Yandex Games:

```csharp
public void YourHandler_OnGetPlayer(bool authorized)
{
	if (authorized) {
		string uuid = _ysdk.Profile.uniqueId,
		string name = _api.Profile.name, // only for isPlayerScope == true
		...
	}
}
```

Remember, if you pass `isPlayerScope = false` to `LaunchSdk` startup parameter, only `Profile.uniqueId` will be available.

> [!IMPORTANT]
> When the game is ready to be shown to the player, set `YandexGamesMono.ReadyAllowed = true` - for example, after loading the save `getData()`, you can call this function.


> [!IMPORTANT]
> Readme is under costruction. Coming soon...

