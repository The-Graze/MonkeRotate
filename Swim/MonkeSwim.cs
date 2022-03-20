using BepInEx;

namespace MonkeSwim
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("Gorilla Tag.exe")]
    internal class MonkeSwim : BaseUnityPlugin
    {
        private void Awake()
        {
            Patch.RotationPatch.Init();
            Patch.CineMachinePatch.Init();
            // Patch.MonkeSwimPatch.ApplyPatch();
            // Patch.RotationPatch.ModEnabled = true;
        }
    }
}
