using HarmonyLib;
using System.Reflection;

namespace MonkeSwim.Patch
{
    class MonkeSwimPatch
    {
        private static Harmony thisInstance;

        public static bool isPatched { get; private set; }
        public static string instanceID = PluginInfo.GUID;

        internal static void ApplyPatch()
        {
            if (!isPatched)
            {
                if (thisInstance == null) { thisInstance = new Harmony(instanceID); }
                thisInstance.PatchAll(Assembly.GetExecutingAssembly());
                isPatched = true;
            }
        }

        internal static void RemovePatch()
        {
            if (thisInstance != null && isPatched)
            {
                thisInstance.UnpatchAll(instanceID);
                isPatched = false;
            }
        }
    }
 }
