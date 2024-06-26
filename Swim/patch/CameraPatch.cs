using System.Reflection;
using HarmonyLib;

using UnityEngine;
using Cinemachine;
using Cinemachine.Utility;

using GorillaLocomotion;

namespace MonkeRotate.Patch
{
    [HarmonyPatch(typeof(GorillaCameraFollow))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    internal static class ShoulderCameraPatch
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

    [HarmonyPatch]
    internal static class CineMachinePatch
    {
        private static MethodInfo PullTwoardsStartOnCollision;

        public static void Init()
        {
            PullTwoardsStartOnCollision = AccessTools.Method(typeof(Cinemachine3rdPersonFollow), "PullTowardsStartOnCollision", new System.Type[] { typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType(), typeof(LayerMask).MakeByRefType(), typeof(float)}); ;
        }

        
        [HarmonyPatch(typeof(Cinemachine3rdPersonFollow))]
        [HarmonyPrefix, HarmonyPatch("PositionCamera", MethodType.Normal)]
        private static bool PositionCameraPatch(Cinemachine3rdPersonFollow __instance, ref CameraState curState, ref float deltaTime, ref float ___PreviousHeadingAngle, ref Vector3 ___PreviousFollowTargetPosition)
        {
            bool time = deltaTime >= 0f;

            Vector3 followTargetPosition = __instance.FollowTargetPosition;
            Vector3 prevTargetPosition = (time ? ___PreviousFollowTargetPosition : followTargetPosition);
            Vector3 offset = Quaternion.Inverse(curState.RawOrientation) * (followTargetPosition - prevTargetPosition);

            if (time) {
                offset = __instance.VirtualCamera.DetachedFollowTargetDamp(offset, __instance.Damping, deltaTime);
            }

            offset = prevTargetPosition + curState.RawOrientation * offset;
            Vector3 forward = Player.Instance.turnParent.transform.forward;
            // Vector3 forward = Quaternion.FromToRotation(Vector3.forward, Player.Instance.transform.forward) * Vector3.forward;
            // Vector3 forward = Vector3.forward;
            Vector3 up = Player.Instance.turnParent.transform.up;
            Vector3 rotatedForward = __instance.FollowTargetRotation * Vector3.forward;

            float headAngle = UnityVectorExtensions.SignedAngle(forward, rotatedForward.ProjectOntoPlane(up), up);
            float prevHeadAngle = (time ? ___PreviousHeadingAngle : headAngle);
            float angle = headAngle - prevHeadAngle;
            ___PreviousHeadingAngle = headAngle;

            offset = (___PreviousFollowTargetPosition = __instance.FollowTargetPosition + Quaternion.AngleAxis(angle, up) * (offset - followTargetPosition));

            Vector3 root;
            Vector3 hand;
            __instance.GetRigPositions(out root, out _, out hand);

            object[] funcParems = new object[] { root, hand, __instance.CameraCollisionFilter, __instance.CameraRadius * 1.05f };
            hand = (Vector3)PullTwoardsStartOnCollision.Invoke(__instance, funcParems);

            Vector3 rayEnd = hand - rotatedForward * __instance.CameraDistance;

            funcParems = new object[] { hand, rayEnd, __instance.CameraCollisionFilter, __instance.CameraRadius };
            rayEnd = (curState.RawPosition = (Vector3)PullTwoardsStartOnCollision.Invoke(__instance, funcParems));

            curState.RawOrientation = __instance.FollowTargetRotation;
            curState.ReferenceLookAt = rayEnd + 1000f * rotatedForward;
            curState.ReferenceUp = up;
            
            return false;
        }
        

        [HarmonyPatch(typeof(Cinemachine3rdPersonFollow))]
        [HarmonyPrefix, HarmonyPatch("GetRigPositions", MethodType.Normal)]
        private static bool RigPositionPatch(Cinemachine3rdPersonFollow __instance, out Vector3 root, out Vector3 shoulder, out Vector3 hand, ref float ___PreviousHeadingAngle, ref Vector3 ___PreviousFollowTargetPosition)
        {
            root = ___PreviousFollowTargetPosition;

            Vector3 vecLerp = Vector3.Lerp(Vector3.Reflect(__instance.ShoulderOffset, Vector3.right), __instance.ShoulderOffset, __instance.CameraSide);
            Vector3 armLength = new Vector3(0f, __instance.VerticalArmLength, 0f);

            shoulder = root + Player.Instance.turnParent.transform.rotation * Quaternion.AngleAxis(___PreviousHeadingAngle, Vector3.up) * vecLerp;
            hand = shoulder + __instance.FollowTargetRotation * armLength;

            return false;
        }
    }
}