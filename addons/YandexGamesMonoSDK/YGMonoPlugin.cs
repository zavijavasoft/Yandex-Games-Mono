#if TOOLS

using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public class YGMonoPlugin : EditorPlugin
{
    private const string _print = "Addon:YandexGamesMonoSDK, YGMonoPlugin.cs";

    public override void _EnterTree()
    {
        GD.Print($"{_print}, _enter_tree() add_autoload_singleton('YandexGames')");
        AddAutoloadSingleton("YandexGames", "res://addons/YandexGamesMonoSDK/YandexGamesMono.cs");
    }

    public override void _ExitTree()
    {
        GD.Print($"{_print}, _exit_tree() remove_autoload_singleton('YandexGames')");
        RemoveAutoloadSingleton("YandexGames");
    }
}

#endif