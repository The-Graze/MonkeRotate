using HarmonyLib;
using System.Reflection;

namespace MonkeSwim.Patch
{
    class AirPatch
    {
        private static Harmony thisInstance;

        public static bool isPatched { get; private set; }
        public static string thisID = "com.ahauntedarmy.gorillatag.monkeswim";

        internal static void ApplyPatch()
        {
            if (!isPatched)
            {
                if (thisInstance == null) { thisInstance = new Harmony(thisID); }
                thisInstance.PatchAll(Assembly.GetExecutingAssembly());
                isPatched = true;
            }
        }

        internal static void RemovePatch()
        {
            if (thisInstance != null && isPatched)
            {
                thisInstance.UnpatchAll(thisID);
                isPatched = false;
            }
        }
    }
 }
