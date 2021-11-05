using BepInEx;


namespace MonkeSwim
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("Gorilla Tag.exe")]
    public class MonkeSwim : BaseUnityPlugin
    {
        public void Awake()
        {
            Patch.RotationPatch.Init();
            Patch.MonkeSwimPatch.ApplyPatch();
        }
    }

}
