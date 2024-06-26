using BepInEx;

namespace MonkeRotate
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    internal class MonkeRotate : BaseUnityPlugin
    {
        MonkeRotate()
        {
            Patch.MonkeRotatePatch.ApplyPatch();
            Patch.RotationPatch.Init();
            Patch.CineMachinePatch.Init();
            Patch.RotationPatch.ModEnabled = true;
        }
    }
}
