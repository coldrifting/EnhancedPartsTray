using BepInEx;
using JetBrains.Annotations;
using SpaceWarp;
using SpaceWarp.API.Mods;
using BepInEx.Configuration;
using KSP.Messages;
using KSP.Game;
using HarmonyLib;

namespace EnhancedPartsTray;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class EnhancedPartsTray : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;
    
    // Config
    internal static ConfigEntry<int> ConfigWidth = null!;
    internal static ConfigEntry<int> ConfigNumCells = null!;
    internal static ConfigEntry<IconType> ConfigTagScale = null!;
    
    private bool _changesQueued;
    
    public override void OnInitialized()
    {
        base.OnInitialized();
        
        var harmony = new Harmony("com.coldrifting.enhancedPartsTray");
        harmony.PatchAll();

        // Config
        ConfigWidth = Config.Bind(
            "Settings",
            "Part Tray Width",
            650,
            new ConfigDescription("The width of the parts tray in the VAB", new AcceptableValueRange<int>(455, 700)));

        ConfigNumCells = Config.Bind(
            "Settings",
            "Parts Per Row",
            3,
            new ConfigDescription("The number of parts to display on each line in the VAB",
                new AcceptableValueRange<int>(3, 9)));

        ConfigTagScale = Config.Bind(
            "Settings",
            "Part Size Tag Display",
            IconType.Default,
            new ConfigDescription("The scale to display the colorful size tags at. Set to 0 to disable"));
        
        // Setup data
        Data.Setup();
        
        // Game Message Callbacks
        var messages = GameManager.Instance.Game.Messages;
        messages.Subscribe<EscapeMenuClosedMessage>(ListenEscClosed);
        messages.Subscribe<OABLoadedMessage>(ListenOABLoaded);
        
        // Config callbacks
        Config.SettingChanged += QueueChanges;
        Config.ConfigReloaded += ApplyChanges;
    }

    // Wait to apply changes until the mod menu is closed
    private void ListenEscClosed(MessageCenterMessage msg)
    {
        if (!_changesQueued)
        {
            return;
        }
        
        _changesQueued = false;
        Data.Apply();
    }

    private static void ListenOABLoaded(MessageCenterMessage msg)
    {
        Data.Apply();
    }

    private void QueueChanges(object sender, SettingChangedEventArgs settingChangedEventArgs)
    {
        _changesQueued = true;
    }

    private static void ApplyChanges(object sender, EventArgs eventArgs)
    {
        Data.Apply();
    }
}


    
public enum IconType
{
    Default,
    Small,
    Dot,
    Hidden
}