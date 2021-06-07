using System.Reflection;
using HarmonyLib;
using UnityEngine;
using VmodMonkeMapLoader.Behaviours;


namespace MonkeSwim.Patch
{
    [HarmonyPatch(typeof(MapLoader))]
    class MapEnter
    {
        [HarmonyPostfix, HarmonyPatch("JoinGame")]
        private static void MapLoaded(MapLoader __instance, ref GameObject ____mapInstance)
        {
            if (____mapInstance != null) {
                Swim.StartMod();
                Swim.defaultGraivty = Physics.gravity;
                Swim.EnableMod(true);
            }
        }
    }

    [HarmonyPatch(typeof(Teleporter))]
    class MapLeave
    {
        [HarmonyPrefix, HarmonyPatch("Trigger")]
        private static void MapExit(Teleporter __instance) { if(__instance.TeleporterType == TeleporterType.Treehouse) Swim.EnableMod(false); }
    }
}
