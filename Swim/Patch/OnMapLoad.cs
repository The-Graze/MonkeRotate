using System.Reflection;
using HarmonyLib;
using UnityEngine;
using VmodMonkeMapLoader.Behaviours;

using UnityEngine.XR;

namespace MonkeSwim.Patch
{
    [HarmonyPatch(typeof(MapLoader))]
    class MapEnter
    {
        [HarmonyPostfix, HarmonyPatch("JoinGame")]
        private static void MapLoaded(MapLoader __instance, ref GameObject ____mapInstance)
        {
            if (____mapInstance != null) {
                Managers.Swim.StartMod();
                Managers.Swim.defaultGraivty = Physics.gravity;
                Managers.Swim.EnableMod(true);
            }
        }
    }

    [HarmonyPatch(typeof(Teleporter))]
    class MapLeave
    {
        [HarmonyPrefix, HarmonyPatch("Trigger")]
        private static void MapExit(Teleporter __instance) { if(__instance.TeleporterType == TeleporterType.Treehouse) Managers.Swim.EnableMod(false); }
    }


    // current version of gorilla tag has OVRManager enabled causing null error spam
    [HarmonyPatch(typeof(OVRManager))]
    class OVRoff
    {
        [HarmonyPrefix, HarmonyPatch("Update")]
        private static bool TurnOff(OVRManager __instance)
        {
            if (XRSettings.loadedDeviceName == "Oculus") return true;

            Debug.Log("turning off OVRManager");

            __instance.enabled = false;
            return false;
        }
    }
}
