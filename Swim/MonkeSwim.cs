using BepInEx;


namespace MonkeSwim
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("Gorilla Tag.exe")]
    public class MonkeSwim : BaseUnityPlugin
    {
        // don't need anything here
        // just need the dll loaded into the game
    }

}
