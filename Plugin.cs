using MelonLoader;
using CustomRPCs;

[assembly: MelonInfo(typeof(Plugin), ModInfo.MOD_NAME, ModInfo.MOD_VERSION, ModInfo.MOD_AUTHOR, $"{ModInfo.MOD_LINK}/releases/latest/download/Release.zip")]
[assembly: MelonGame("Landfall Games", "Knightfall")]

namespace CustomRPCs;

internal class Plugin : MelonMod
{
    public static MelonPreferences_Entry<bool> ENABLED;

    public override void OnInitializeMelon()
    {
        InitConfig();

        if (!ENABLED.Value)
        {
            UnloadMod();
            return;
        }

        LoggerInstance.Msg($"Plugin {ModInfo.MOD_NAME} loaded successfully!");
    }

    /// <summary>
    /// Load or create all MelonLoader config entries.
    /// Stored in `UserData/MelonPreferences.cfg`.
    /// </summary>
    private void InitConfig()
    {
        var mlCategory = MelonPreferences.CreateCategory("CustomRPCs", "CustomRPCs settings");

        ENABLED = mlCategory.CreateEntry("Enabled", true);
    }

    /// <summary>
    /// Remove all Harmony Patches.
    /// </summary>
    private void UnloadMod() => HarmonyInstance.UnpatchSelf();
}
