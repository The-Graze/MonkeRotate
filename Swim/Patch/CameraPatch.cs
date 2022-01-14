using HarmonyLib;
using UnityEngine;
using GorillaLocomotion;

namespace MonkeSwim.Patch
{
    [HarmonyPatch(typeof(GorillaCameraFollow))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    internal class ExamplePatch
    {
        private static bool Prefix(GorillaCameraFollow __instance)
        {
            Quaternion playerRotation = Player.Instance.turnParent.transform.rotation;

            Vector3 headForward = __instance.playerHead.localRotation * Vector3.forward;

            // grab the y angle
            Vector3 vector = Vector3.ProjectOnPlane(headForward, Vector3.up);
            float yAngle = Vector3.SignedAngle(Vector3.forward, vector, Vector3.up);

            // grab the x angle
            Vector3 axisOFfset = Quaternion.AngleAxis(90f, Vector3.up) * vector;
            Vector3 to = Vector3.ProjectOnPlane(headForward, axisOFfset);
            float xAngle = Vector3.SignedAngle(Vector3.up, to, axisOFfset) - 90f;

            __instance.transform.rotation = playerRotation * Quaternion.Euler(xAngle, yAngle, 0f);

            return false;
        }
    }
}