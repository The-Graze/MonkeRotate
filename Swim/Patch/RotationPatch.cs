using HarmonyLib;
using System.Reflection;

using Photon.Pun;
using Photon.Voice.PUN;

using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace MonkeSwim.Patch
{
    [HarmonyPatch]
    internal static class RotationPatch
    {
        private static MethodInfo StoreVelocities;
        private static MethodInfo PositionWithOffset;
        private static MethodInfo CurrentRightHandPosition;
        private static MethodInfo CurrentLeftHandPosition;
        private static MethodInfo IterativeCollisionSphereCast;
        private static MethodInfo AntiTeleportTechnology;

        public static bool modEnabled = true;

        private static Quaternion axisLockedRotation; // players rotating if it was locked on global Vector3.up axis
        private static Quaternion playerUpRotation; // players up direction rotation in global space

        private static Quaternion bodyRotation; // players body rotation for the gorilla model and body collider
        private static Quaternion bodyLockedRotation;

        public static Quaternion PlayerLockedRotation { get { return axisLockedRotation; } }
        public static Quaternion PlayerUpRotation { get { return playerUpRotation; } }

        public static Quaternion BodyRotation { get { return bodyRotation; } }
        public static Quaternion BodyLockedRotation { get { return bodyLockedRotation; } }

        public static void Init()
        {
            StoreVelocities = AccessTools.Method(typeof(GorillaLocomotion.Player), "StoreVelocities");
            Debug.Log("StoreVelocities == null? " + (StoreVelocities == null));

            CurrentLeftHandPosition = AccessTools.Method(typeof(GorillaLocomotion.Player), "CurrentLeftHandPosition");
            Debug.Log("CurrentLeftHandPosition == null? " + (CurrentLeftHandPosition == null));

            CurrentRightHandPosition = AccessTools.Method(typeof(GorillaLocomotion.Player), "CurrentRightHandPosition");
            Debug.Log("CurrentRightHandPosition == null? " + (CurrentRightHandPosition == null));

            AntiTeleportTechnology = AccessTools.Method(typeof(GorillaLocomotion.Player), "AntiTeleportTechnology");
            Debug.Log("AntiTeleportTechnology == null? " + (AntiTeleportTechnology == null));

            PositionWithOffset = AccessTools.Method(typeof(GorillaLocomotion.Player), "PositionWithOffset", new System.Type[]{typeof(Transform), typeof(Vector3) });
            Debug.Log("PositionWithOffset == null? " + (PositionWithOffset == null));

            IterativeCollisionSphereCast = AccessTools.Method(typeof(GorillaLocomotion.Player), "IterativeCollisionSphereCast", new System.Type[] { typeof(Vector3), typeof(float), typeof(Vector3),
                                                                                                                                                                     typeof(float), typeof(Vector3).MakeByRefType(),
                                                                                                                                                                     typeof(bool), typeof(float).MakeByRefType() });
            Debug.Log("IterativeCollisionSphereCast == null? " + (IterativeCollisionSphereCast == null));
        }

        [HarmonyPatch(typeof(GorillaLocomotion.Player))]
        [HarmonyPrefix, HarmonyPatch("Update", MethodType.Normal)]
        internal static bool Prefix_PlayerUpdate(GorillaLocomotion.Player __instance, ref bool ___leftHandColliding, 
                                                                                      ref bool ___rightHandColliding,
                                                                                      ref bool ___wasLeftHandTouching,
                                                                                      ref bool ___wasRightHandTouching,
                                                                                      ref float ___slipPercentage,
                                                                                      ref Vector3 ___rigidBodyMovement, 
                                                                                      ref Vector3 ___firstIterationLeftHand,
                                                                                      ref Vector3 ___firstIterationRightHand,
                                                                                      ref Vector3 ___distanceTraveled,
                                                                                      ref Vector3 ___lastLeftHandPosition,
                                                                                      ref Vector3 ___lastRightHandPosition,
                                                                                      ref Vector3 ___finalPosition,
                                                                                      ref Vector3 ___denormalizedVelocityAverage,
                                                                                      ref Vector3 ___bodyOffsetVector,
                                                                                      ref Rigidbody ___playerRigidBody,
                                                                                      ref RaycastHit ___hitInfo)
        {
            if (!modEnabled) return true;

            SavePlayerRotation(__instance);

            ___leftHandColliding = false;
            ___rightHandColliding = false;
            ___rigidBodyMovement = Vector3.zero;
            ___firstIterationLeftHand = Vector3.zero;
            ___firstIterationRightHand = Vector3.zero;

            __instance.bodyCollider.transform.position = (Vector3)PositionWithOffset.Invoke(__instance, new object[] { __instance.headCollider.transform, __instance.bodyOffset }) + (__instance.turnParent.transform.rotation * ___bodyOffsetVector);
            __instance.bodyCollider.transform.rotation = bodyRotation;

            AntiTeleportTechnology.Invoke(__instance, null);

            Vector3 downDir = __instance.turnParent.transform.up * -1f;


            // calculate left hand movement
            Vector3 currentHandPosition = (Vector3)CurrentLeftHandPosition.Invoke(__instance, null);
            ___distanceTraveled = currentHandPosition - ___lastLeftHandPosition + downDir * 2f * 9.8f * Time.deltaTime * Time.deltaTime;

            // left hand collision check
            object[] itCollideParems = new object[] { ___lastLeftHandPosition, __instance.minimumRaycastDistance, ___distanceTraveled, __instance.defaultPrecision, ___finalPosition, true, ___slipPercentage };
            bool itCollideResult = (bool)IterativeCollisionSphereCast.Invoke(__instance, itCollideParems);
            ___finalPosition = (Vector3)itCollideParems[4];
            ___slipPercentage = (float)itCollideParems[6];

            if (itCollideResult) {
                
                if (___wasLeftHandTouching && (___slipPercentage == __instance.defaultSlideFactor || ___slipPercentage == 0.001f)) {
                    ___firstIterationLeftHand = ___lastLeftHandPosition - currentHandPosition;

                } else {
                    ___firstIterationLeftHand = ___finalPosition - currentHandPosition;
                }

                ___playerRigidBody.velocity = Vector3.zero;
                ___leftHandColliding = true;
                __instance.leftHandMaterialTouchIndex = __instance.currentMaterialIndex;
                __instance.leftHandSurfaceOverride = __instance.currentOverride;
            }


            // calculate right hand movement
            currentHandPosition = (Vector3)CurrentRightHandPosition.Invoke(__instance, null);
            ___distanceTraveled = currentHandPosition - ___lastRightHandPosition + downDir * 2f * 9.8f * Time.deltaTime * Time.deltaTime;

            // right hand collision check
            itCollideParems = new object[] { ___lastRightHandPosition, __instance.minimumRaycastDistance, ___distanceTraveled, __instance.defaultPrecision, ___finalPosition, true, ___slipPercentage };
            itCollideResult = (bool)IterativeCollisionSphereCast.Invoke(__instance, itCollideParems);
            ___finalPosition = (Vector3)itCollideParems[4];
            ___slipPercentage = (float)itCollideParems[6];

            if (itCollideResult) {
                
                if(___wasRightHandTouching && (___slipPercentage == __instance.defaultSlideFactor || ___slipPercentage == 0.001f)) {
                    ___firstIterationRightHand = ___lastRightHandPosition - currentHandPosition;

                } else {
                    ___firstIterationRightHand = ___finalPosition - currentHandPosition;
                }

                ___playerRigidBody.velocity = Vector3.zero;
                ___rightHandColliding = true;
                __instance.rightHandMaterialTouchIndex = __instance.currentMaterialIndex;
                __instance.rightHandSurfaceOverride = __instance.currentOverride;
            }

            if ((___leftHandColliding || ___wasLeftHandTouching) && (___rightHandColliding || ___wasRightHandTouching)) {
                ___rigidBodyMovement = (___firstIterationLeftHand + ___firstIterationRightHand) / 2f;

            } else {
                ___rigidBodyMovement = ___firstIterationLeftHand + ___firstIterationRightHand;
            }

            RaycastHit hitInfo = ___hitInfo;

            // head collision check
            itCollideParems = new object[] { __instance.lastHeadPosition, __instance.headCollider.radius, __instance.headCollider.transform.position + ___rigidBodyMovement - __instance.lastHeadPosition, __instance.defaultPrecision, ___finalPosition, false, ___slipPercentage};
            itCollideResult = (bool)IterativeCollisionSphereCast.Invoke(__instance, itCollideParems);
            ___finalPosition = (Vector3)itCollideParems[4];
            ___slipPercentage = (float)itCollideParems[6];

            if (itCollideResult) {
                ___rigidBodyMovement = ___finalPosition - __instance.lastHeadPosition;

                bool raycastResult = Physics.Raycast( __instance.lastHeadPosition,
                                                      __instance.headCollider.transform.position - __instance.lastHeadPosition + ___rigidBodyMovement,
                                                      out hitInfo,
                                                      (__instance.headCollider.transform.position - __instance.lastHeadPosition + ___rigidBodyMovement).magnitude + __instance.headCollider.radius * __instance.defaultPrecision * 0.999f,
                                                      __instance.locomotionEnabledLayers.value);

                ___hitInfo = hitInfo;

                if (raycastResult) {
                    ___rigidBodyMovement = __instance.lastHeadPosition - __instance.headCollider.transform.position;
                }
            }


            if (___rigidBodyMovement != Vector3.zero) {
                __instance.transform.position = __instance.transform.position + ___rigidBodyMovement;
            }

            __instance.lastHeadPosition = __instance.headCollider.transform.position;


            // calclulate left hand movement again
            currentHandPosition = (Vector3)CurrentLeftHandPosition.Invoke(__instance, null);
            ___distanceTraveled = currentHandPosition - ___lastLeftHandPosition;

            // left hand collision check again
            itCollideParems = new object[] { ___lastLeftHandPosition, __instance.minimumRaycastDistance, ___distanceTraveled, __instance.defaultPrecision, ___finalPosition, (!___leftHandColliding && !___wasLeftHandTouching) || (!___rightHandColliding && !___wasRightHandTouching), ___slipPercentage };
            itCollideResult = (bool)IterativeCollisionSphereCast.Invoke(__instance, itCollideParems);
            ___finalPosition = (Vector3)itCollideParems[4];
            ___slipPercentage = (float)itCollideParems[6];

            if (itCollideResult) {
                ___lastLeftHandPosition = ___finalPosition;
                ___leftHandColliding = true;
                __instance.leftHandMaterialTouchIndex = __instance.currentMaterialIndex;
                __instance.leftHandSurfaceOverride = __instance.currentOverride;

            } else {
                ___lastLeftHandPosition = currentHandPosition;
            }


            //calculate right hand movement again
            currentHandPosition = (Vector3)CurrentRightHandPosition.Invoke(__instance, null);
            ___distanceTraveled = currentHandPosition - ___lastRightHandPosition;

            // right hand collision check again
            itCollideParems = new object[] { ___lastRightHandPosition, __instance.minimumRaycastDistance, ___distanceTraveled, __instance.defaultPrecision, ___finalPosition, (!___leftHandColliding && !___wasLeftHandTouching) || (!___rightHandColliding && !___wasRightHandTouching), ___slipPercentage };
            itCollideResult = (bool)IterativeCollisionSphereCast.Invoke(__instance, itCollideParems);
            ___finalPosition = (Vector3)itCollideParems[4];
            ___slipPercentage = (float)itCollideParems[6];

            if (itCollideResult) {
                ___lastRightHandPosition = ___finalPosition;
                ___rightHandColliding = true;
                __instance.rightHandMaterialTouchIndex = __instance.currentMaterialIndex;
                __instance.rightHandSurfaceOverride = __instance.currentOverride;

            } else {
                ___lastRightHandPosition = currentHandPosition;
            }

            StoreVelocities.Invoke(__instance, null);

            if ((___rightHandColliding || ___leftHandColliding) && !__instance.disableMovement && !__instance.didATurn && ___denormalizedVelocityAverage.magnitude > __instance.velocityLimit) {
                if (___denormalizedVelocityAverage.magnitude * __instance.jumpMultiplier > __instance.maxJumpSpeed) {
                    ___playerRigidBody.velocity = ___denormalizedVelocityAverage.normalized * __instance.maxJumpSpeed;

                } else {
                    ___playerRigidBody.velocity = __instance.jumpMultiplier * ___denormalizedVelocityAverage;
                }
            }

            if (___leftHandColliding && ((Vector3)CurrentLeftHandPosition.Invoke(__instance, null)).magnitude > __instance.unStickDistance && !Physics.SphereCast(__instance.headCollider.transform.position,
                                                                                                                                                                   __instance.minimumRaycastDistance * __instance.defaultPrecision,
                                                                                                                                                                   ((Vector3)CurrentLeftHandPosition.Invoke(__instance, null)) - __instance.headCollider.transform.position,
                                                                                                                                                                   out hitInfo,
                                                                                                                                                                   (((Vector3)CurrentLeftHandPosition.Invoke(__instance, null)) - __instance.headCollider.transform.position).magnitude - __instance.minimumRaycastDistance,
                                                                                                                                                                   __instance.locomotionEnabledLayers.value))
            {
                ___lastLeftHandPosition = (Vector3)CurrentLeftHandPosition.Invoke(__instance, null);
                ___leftHandColliding = false;
            }

            ___hitInfo = hitInfo;

            if (___rightHandColliding && ((Vector3)CurrentRightHandPosition.Invoke(__instance, null)).magnitude > __instance.unStickDistance && !Physics.SphereCast(__instance.headCollider.transform.position,
                                                                                                                                                                    __instance.minimumRaycastDistance * __instance.defaultPrecision,
                                                                                                                                                                    ((Vector3)CurrentRightHandPosition.Invoke(__instance, null)) - __instance.headCollider.transform.position,
                                                                                                                                                                    out hitInfo,
                                                                                                                                                                    (((Vector3)CurrentRightHandPosition.Invoke(__instance, null)) - __instance.headCollider.transform.position).magnitude - __instance.minimumRaycastDistance,
                                                                                                                                                                    __instance.locomotionEnabledLayers.value)) 
            {
                ___lastRightHandPosition = (Vector3)CurrentRightHandPosition.Invoke(__instance, null);
                ___rightHandColliding = false;
            }

            ___hitInfo = hitInfo;

            __instance.leftHandFollower.position = ___lastLeftHandPosition;
            __instance.rightHandFollower.position = ___lastRightHandPosition;
            ___wasLeftHandTouching = ___leftHandColliding;
            ___wasRightHandTouching = ___rightHandColliding;

            return false;
        }

        [HarmonyPatch(typeof(VRRig))]
        [HarmonyPrefix, HarmonyPatch("LateUpdate", MethodType.Normal)]
        internal static bool VRRig_TransformOverride(VRRig __instance, float ___ratio)
        {
            // tbh this would've been better as a transpiler, but i don't really understand those yet
            // and i wanted to do this in a way that is more quest port friendly.

            // what i'm doing here is rotating the camera to be inline with world axis and store the y rotation in in float
            // apply the new rotation to the gorilla so its y rotation is inline with the camera, but its still in world axis,
            // so afterwards rotating the gorilla to be inline with the camera axis.
            // the original position update was already applying the offsets correctly so i just copied that.
            // rest is just re-implenting all the hand update stuff because we need to skip the original function but those need to run
            // i can't run this as a postfix and ignore the handupdates because rotating the gorilla will also rotate all the positions and rotations
            // that the hand updates had set, causing their positions to be out of place

            //if mod is off skip this function
            if (!modEnabled) return true;

            if (__instance.isOfflineVRRig || __instance.photonView.IsMine) {

                Transform gorilla = __instance.transform;
                Transform camera = __instance.mainCamera.transform;

                //converting camera's rotation to local and saving its y euler value
                // float cameraY = (Quaternion.Inverse(__instance.playerOffsetTransform.rotation) * camera.rotation).eulerAngles.y;

                //set the gorillas rotation based on the cameras Y value
                // gorilla.eulerAngles = new Vector3(0f, cameraY, 0f);

                //convert the rotation back to global space
                // gorilla.rotation = __instance.playerOffsetTransform.rotation * gorilla.rotation;

                gorilla.rotation = bodyRotation;

                //set the gorilla position with the offests rotated
                gorilla.position = camera.position + (__instance.headConstraint.rotation * __instance.head.trackingPositionOffset) + (gorilla.rotation * __instance.headBodyOffset);

                // update the head
                __instance.head.MapMine(___ratio, __instance.playerOffsetTransform);

                //update right hand
                __instance.rightHand.MapMine(___ratio, __instance.playerOffsetTransform);
                __instance.rightIndex.MapMyFinger(__instance.lerpValueFingers);
                __instance.rightMiddle.MapMyFinger(__instance.lerpValueFingers);
                __instance.rightThumb.MapMyFinger(__instance.lerpValueFingers);

                //update left hand
                __instance.leftHand.MapMine(___ratio, __instance.playerOffsetTransform);
                __instance.leftIndex.MapMyFinger(__instance.lerpValueFingers);
                __instance.leftMiddle.MapMyFinger(__instance.lerpValueFingers);
                __instance.leftThumb.MapMyFinger(__instance.lerpValueFingers);

                // i don't think this is needed on pc, but its in the original source and i want to change as little as possible
                // looks like on quest this makes the gorilla invisble when you bring up quest menu
                if (XRSettings.loadedDeviceName == "Oculus" && ((__instance.isOfflineVRRig && !PhotonNetwork.InRoom) || (!__instance.isOfflineVRRig && PhotonNetwork.InRoom))) {
                    __instance.mainSkin.enabled = OVRManager.hasInputFocus;
                }

                if (OpenVR.Overlay != null && ((__instance.isOfflineVRRig && !PhotonNetwork.InRoom) || !__instance.isOfflineVRRig && PhotonNetwork.InRoom)) {
                    __instance.mainSkin.enabled = !OpenVR.Overlay.IsDashboardVisible();
                }

                // looks like this stuff is for turning voice chat on or off
                if (!__instance.isOfflineVRRig) {
                    if (__instance.photonView == null || __instance.photonView.Owner == null) {
                        Object.Destroy(__instance.gameObject);
                    }

                    object myObject;
                    if (__instance.photonView.Owner.CustomProperties.TryGetValue("matIndex", out myObject) && (int)myObject != __instance.setMatIndex) {
                        __instance.ChangeMaterial((int)myObject);
                    }

                    bool audioEnabled = !__instance.GetComponent<PhotonVoiceView>().SpeakerInUse.gameObject.GetComponent<AudioSource>().enabled;
                    __instance.GetComponent<PhotonVoiceView>().SpeakerInUse.gameObject.GetComponent<AudioSource>().enabled = (GorillaComputer.instance.voiceChatOn == "TRUE" && !__instance.muted);

                    if (audioEnabled && GorillaComputer.instance.voiceChatOn == "TRUE" && !__instance.muted) {
                        __instance.GetComponent<PhotonVoiceView>().SpeakerInUse.RestartPlayback();
                    }
                }

                // run the base functions else statement if its a networked player, the base else statement
                // just lerps the positions without being constrained to anny axis, used for connected players
                // so this shouldn't break anything
            } else return true;

            //skips the original function
            return false;
        }

        [HarmonyPatch(typeof(VRMap))]
        [HarmonyPrefix, HarmonyPatch("MapMine", MethodType.Normal)]
        internal static bool MapMine(VRMap __instance, ref float ratio, ref Transform playerOffsetTransform)
        {
            // all i've really done here is replace Vector3.up with playerOffsetTrasnform.up
            // and use the players up rotation to localise the device rotation and position
            // xr device information is always based on your play space and not scene or world information
            // so if you were standing perfectly upright looking straight forward, your headset up would be 
            // equivilent to Vector3.up

            // Quaternion targetRotation = xrDeviceRotation * Quaternion.Euler(__instance.trackingRotationOffset);
            // targetRotation = playerUpRotation * targetRotation;
            // __instance.rigTarget.rotation = targetRotation;

            //if mod is off, run original code
            if (!modEnabled) return true;

            InputDevice xrDevice = InputDevices.GetDeviceAtXRNode(__instance.vrTargetNode);

            Quaternion xrDeviceRotation;
            xrDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out xrDeviceRotation);


            xrDeviceRotation = xrDeviceRotation * Quaternion.Euler(__instance.trackingRotationOffset);
            xrDeviceRotation = playerUpRotation * xrDeviceRotation;

            __instance.rigTarget.rotation = xrDeviceRotation;

            if (__instance.overrideTarget != null) {
                __instance.rigTarget.RotateAround(__instance.overrideTarget.position, playerOffsetTransform.up, axisLockedRotation.eulerAngles.y);
                __instance.rigTarget.position = __instance.overrideTarget.position + __instance.rigTarget.rotation * __instance.trackingPositionOffset;
                return false;
            }

            Vector3 xrDeviceLocation;
            xrDevice.TryGetFeatureValue(CommonUsages.devicePosition, out xrDeviceLocation);
            xrDeviceLocation = playerUpRotation * xrDeviceLocation;

            __instance.rigTarget.position = xrDeviceLocation + __instance.rigTarget.rotation * __instance.trackingPositionOffset + playerOffsetTransform.position;
            __instance.rigTarget.RotateAround(playerOffsetTransform.position, playerOffsetTransform.up, axisLockedRotation.eulerAngles.y);

            return false;
        }

        // turn is used to rotate the player with stick inputs, its also axis locked to Vector3.up
        [HarmonyPatch(typeof(GorillaLocomotion.Player))]
        [HarmonyPrefix, HarmonyPatch("Turn", MethodType.Normal)]
        internal static bool RotatePlayer(GorillaLocomotion.Player __instance, ref float degrees, ref Vector3 ___denormalizedVelocityAverage, ref Vector3[] ___velocityHistory)
        {
            // what i think is going on is velocity history is used to average out velocity over time for smoother movement, so,
            // this functions rotates the velocity direction in the reverse of the direction you rotated with inputs in order 
            // to preserve the velocity direction so you don't turn in the air

            //if mod is disabled run original code
            if (!modEnabled) return true;

            // this turns the player
            __instance.turnParent.transform.RotateAround(__instance.headCollider.transform.position, __instance.turnParent.transform.up, degrees);


            // rotate degrees to be in players local orientation
            Quaternion localDegrees = playerUpRotation * Quaternion.Euler(0f, degrees, 0f);

            // this stuff preserves the velocity direction
            ___denormalizedVelocityAverage = localDegrees * ___denormalizedVelocityAverage;

            for (int i = 0; i < ___velocityHistory.Length; i++) {
                ___velocityHistory[i] = localDegrees * ___velocityHistory[i];
            }

            __instance.didATurn = true;

            //player rotation has been modified so we need to update it again
            SavePlayerRotation(__instance);

            return false;
        }

        private static void SavePlayerRotation(GorillaLocomotion.Player playerInstance)
        {
            Transform playerTransform = playerInstance.turnParent.transform;
            if (playerTransform == null) return;

            //save the players rotation on the global Vector3.up axis
            axisLockedRotation = Quaternion.FromToRotation(playerTransform.up, Vector3.up) * playerTransform.rotation;

            //save the player up rotation in global space
            playerUpRotation = Quaternion.FromToRotation(Vector3.up, playerTransform.up);

            float cameraY = (Quaternion.Inverse(playerInstance.turnParent.transform.rotation) * playerInstance.headCollider.transform.rotation).eulerAngles.y;
            bodyLockedRotation = Quaternion.Euler(0f, cameraY, 0f);
            bodyRotation = playerTransform.rotation * bodyLockedRotation;
        }
    }
}
