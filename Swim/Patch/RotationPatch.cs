/*
    everything in here is copyright owned by the gorilla tag developer.
    there's a lot of copy-paste code from gorilla tag, modified slightly in order to not break
    the player rigging when rotation the player off the worlds up axis
 */

using HarmonyLib;
using System.Reflection;

using Photon.Pun;
using Photon.Voice.PUN;

using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

using GorillaLocomotion;
using GorillaNetworking;

namespace MonkeSwim.Patch
{
    [HarmonyPatch]
    internal static class RotationPatch
    {
        public static bool ModEnabled {
            get => modEnabled;
            set => modEnabled = value && reflectionInfoFound;          
		}
           
        private static bool modEnabled = false;
        private static bool reflectionInfoFound = false;

        // reflection methods for player update
        private static MethodInfo StoreVelocities;
        private static MethodInfo PositionWithOffset;
        private static MethodInfo CurrentRightHandPosition;
        private static MethodInfo CurrentLeftHandPosition;
        private static MethodInfo IterativeCollisionSphereCast;
        private static MethodInfo AntiTeleportTechnology;
        private static MethodInfo FirstHandIteration;
        private static MethodInfo FinalHandPosition;
        private static MethodInfo MaxSphereSizeForNoOverlap;

        // reflection methods for VRRig lateupdate
        private static MethodInfo CheckForEarlyAccess;


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
            CheckForEarlyAccess = AccessTools.Method(typeof(VRRig), "CheckForEarlyAccess");

            StoreVelocities = AccessTools.Method(typeof(GorillaLocomotion.Player), "StoreVelocities");
            // Debug.Log("StoreVelocities == null? " + (StoreVelocities == null));

            CurrentLeftHandPosition = AccessTools.Method(typeof(GorillaLocomotion.Player), "CurrentLeftHandPosition");
            // Debug.Log("CurrentLeftHandPosition == null? " + (CurrentLeftHandPosition == null));

            CurrentRightHandPosition = AccessTools.Method(typeof(GorillaLocomotion.Player), "CurrentRightHandPosition");
            // Debug.Log("CurrentRightHandPosition == null? " + (CurrentRightHandPosition == null));

            AntiTeleportTechnology = AccessTools.Method(typeof(GorillaLocomotion.Player), "AntiTeleportTechnology");
            // Debug.Log("AntiTeleportTechnology == null? " + (AntiTeleportTechnology == null));

            PositionWithOffset = AccessTools.Method(typeof(GorillaLocomotion.Player), "PositionWithOffset", new System.Type[]{ typeof(Transform), typeof(Vector3) });
            // Debug.Log("PositionWithOffset == null? " + (PositionWithOffset == null));

            MaxSphereSizeForNoOverlap = AccessTools.Method(typeof(GorillaLocomotion.Player), "MaxSphereSizeForNoOverlap", new System.Type[] { typeof(float), typeof(Vector3), typeof(float).MakeByRefType() });

            IterativeCollisionSphereCast = AccessTools.Method(typeof(GorillaLocomotion.Player), "IterativeCollisionSphereCast", new System.Type[] { 
                                                                                                                                                    typeof(Vector3), 
                                                                                                                                                    typeof(float), 
                                                                                                                                                    typeof(Vector3), 
                                                                                                                                                    typeof(Vector3).MakeByRefType(),
                                                                                                                                                    typeof(bool), 
                                                                                                                                                    typeof(float).MakeByRefType(),
                                                                                                                                                    typeof(RaycastHit).MakeByRefType(),
                                                                                                                                                    typeof(bool)
                                                                                                                                                   } );
            // Debug.Log("IterativeCollisionSphereCast == null? " + (IterativeCollisionSphereCast == null));

            FirstHandIteration = AccessTools.Method(typeof(GorillaLocomotion.Player), "FirstHandIteration", new System.Type[] { 
                                                                                                                                typeof(Transform),
                                                                                                                                typeof(Vector3),
                                                                                                                                typeof(Vector3),
                                                                                                                                typeof(bool),
                                                                                                                                typeof(bool).MakeByRefType(),
                                                                                                                                typeof(Vector3).MakeByRefType(),
                                                                                                                                typeof(float).MakeByRefType(),
                                                                                                                                typeof(bool).MakeByRefType(),
                                                                                                                                typeof(Vector3).MakeByRefType(),
                                                                                                                                typeof(bool).MakeByRefType(),
                                                                                                                                typeof(int).MakeByRefType(),
                                                                                                                                typeof(GorillaSurfaceOverride).MakeByRefType()
                                                                                                                               } );

            FinalHandPosition = AccessTools.Method(typeof(GorillaLocomotion.Player), "FinalHandPosition", new System.Type[] {
                                                                                                                              typeof(Transform),
                                                                                                                              typeof(Vector3),
                                                                                                                              typeof(Vector3),
                                                                                                                              typeof(bool),
                                                                                                                              typeof(bool),
                                                                                                                              typeof(bool).MakeByRefType(),
                                                                                                                              typeof(bool),
                                                                                                                              typeof(bool).MakeByRefType(),
                                                                                                                              typeof(int),
                                                                                                                              typeof(int).MakeByRefType(),
                                                                                                                              typeof(GorillaSurfaceOverride),
                                                                                                                              typeof(GorillaSurfaceOverride).MakeByRefType()
                                                                                                                             } );

            if(CheckForEarlyAccess == null || StoreVelocities == null 
                || CurrentLeftHandPosition == null   || CurrentRightHandPosition == null
                || AntiTeleportTechnology == null    || PositionWithOffset == null
                || MaxSphereSizeForNoOverlap == null ||IterativeCollisionSphereCast == null
                || FirstHandIteration == null        || FinalHandPosition == null)
            {
                reflectionInfoFound = false;

			} else {
                reflectionInfoFound = true;
			}
        }

        #region PLAYERUPDATE
        [HarmonyPatch(typeof(GorillaLocomotion.Player))]
        [HarmonyPrefix, HarmonyPatch("LateUpdate", MethodType.Normal)]
        internal static bool Prefix_PlayerUpdate(GorillaLocomotion.Player __instance, ref bool ___leftHandColliding, 
                                                                                      ref bool ___rightHandColliding,
                                                                                      ref bool ___areBothTouching,
                                                                                      ref int ___touchPoints,
                                                                                      ref float ___slipPercentage,
                                                                                      ref float ___tempRealTime,
                                                                                      ref float ___calcDeltaTime,
                                                                                      ref float ___lastRealTime,
                                                                                      ref float ___averageSlipPercentage,
                                                                                      ref Vector3 ___rigidBodyMovement, 
                                                                                      ref Vector3 ___firstIterationLeftHand,
                                                                                      ref Vector3 ___firstIterationRightHand,
                                                                                      ref Vector3 ___distanceTraveled,
                                                                                      ref Vector3 ___lastLeftHandPosition,
                                                                                      ref Vector3 ___lastRightHandPosition,
                                                                                      ref Vector3 ___finalPosition,
                                                                                      ref Vector3 ___denormalizedVelocityAverage,
                                                                                      ref Vector3 ___bodyOffsetVector,
                                                                                      ref Vector3 ___slideAverage,
                                                                                      ref Vector3 ___slideAverageNormal,
                                                                                      ref Vector3 ___junkNormal,
                                                                                      ref Vector3 ___rightHandSurfaceDirection,
                                                                                      ref Vector3 ___leftHandSurfaceDirection,
                                                                                      ref Vector3 ___surfaceDirection,
                                                                                      ref Rigidbody ___playerRigidBody,
                                                                                      ref RaycastHit ___hitInfo,
                                                                                      ref RaycastHit ___tempHitInfo,
                                                                                      ref RaycastHit ___hitInfoLeft,
                                                                                      ref RaycastHit ___hitInfoRight,
                                                                                      ref RaycastHit ___junkHit)
        {
            if (!modEnabled) return true;

            SavePlayerRotation(__instance);

            ___leftHandColliding = false;
            ___rightHandColliding = false;
            __instance.rightHandSlide = false;
            __instance.leftHandSlide = false;

            ___rigidBodyMovement = Vector3.zero;
            ___firstIterationLeftHand = Vector3.zero;
            ___firstIterationRightHand = Vector3.zero;

            __instance.rightHandSlideNormal = __instance.turnParent.transform.up;
            __instance.leftHandSlideNormal = __instance.turnParent.transform.up;

            Vector3 downDir = __instance.transform.up * -1f;

			if (__instance.debugMovement) {
                ___tempRealTime = Time.time;
                ___calcDeltaTime = Time.deltaTime;
                ___lastRealTime = ___tempRealTime;

			} else {
                ___tempRealTime = Time.realtimeSinceStartup;
                ___calcDeltaTime = ___tempRealTime = ___lastRealTime;
                ___lastRealTime = ___tempRealTime;

                if(___calcDeltaTime > 0.1f)
                    ___calcDeltaTime = 0.05f;
			}

            if (!__instance.didAJump) {
                if (__instance.wasLeftHandTouching || __instance.wasRightHandTouching) {
                    __instance.transform.position = __instance.transform.position + 4.95f * downDir * ___calcDeltaTime * ___calcDeltaTime;

                    if (Vector3.Dot(___denormalizedVelocityAverage, ___slideAverageNormal) <= 0f && Vector3.Dot(downDir, ___slideAverageNormal) <= 0f) {
                        __instance.transform.position = __instance.transform.position - Vector3.Project(Mathf.Min(__instance.stickDepth, Vector3.Project(___denormalizedVelocityAverage, ___slideAverageNormal).magnitude * ___calcDeltaTime) * ___slideAverageNormal, downDir);
                    }
                }

                if (__instance.wasLeftHandSlide || __instance.wasRightHandSlide) {
                    __instance.transform.position = __instance.transform.position + ___slideAverage * ___calcDeltaTime;
                    ___slideAverage += 9.8f * downDir * ___calcDeltaTime;
                }
            }

            object[] funcParems = null;

            /* first iteration left hand */
            // variables passed to the functions
            funcParems = new object[] { __instance.leftHandTransform,           __instance.leftHandOffset, 
                                        ___lastLeftHandPosition,                __instance.wasLeftHandSlide, 
                                        __instance.wasLeftHandTouching,         ___firstIterationLeftHand,
                                        __instance.leftHandSlipPercentage,      __instance.leftHandSlide,
                                        __instance.leftHandSlideNormal,         ___leftHandColliding,
                                        __instance.leftHandMaterialTouchIndex,  __instance.leftHandSurfaceOverride };

            // run GorillaLocomotion.Player.FirstHandIteration(...)
            FirstHandIteration.Invoke(__instance, funcParems);

            // assign the out values 
            __instance.wasLeftHandTouching  = (bool)funcParems[4];
            ___firstIterationLeftHand = (Vector3)funcParems[5];
            __instance.leftHandSlipPercentage = (float)funcParems[6];
            __instance.leftHandSlide = (bool)funcParems[7];
            __instance.leftHandSlideNormal = (Vector3)funcParems[8];
            ___leftHandColliding = (bool)funcParems[9];
            __instance.leftHandMaterialTouchIndex = (int)funcParems[10];
            __instance.leftHandSurfaceOverride = (GorillaSurfaceOverride)funcParems[11];

            /* first iteration right hand */
            // variables passed to the function
            funcParems = new object[] { __instance.rightHandTransform,           __instance.rightHandOffset,
                                        ___lastRightHandPosition,                __instance.wasRightHandSlide,
                                        __instance.wasRightHandTouching,         ___firstIterationRightHand,
                                        __instance.rightHandSlipPercentage,      __instance.rightHandSlide,
                                        __instance.rightHandSlideNormal,         ___rightHandColliding,
                                        __instance.rightHandMaterialTouchIndex,  __instance.rightHandSurfaceOverride };

            // run GorillaLocomotion.Player.FirstHandIteration(...)
            FirstHandIteration.Invoke(__instance, funcParems);

            // assign the out values
            __instance.wasRightHandTouching = (bool)funcParems[4];
            ___firstIterationRightHand = (Vector3)funcParems[5];
            __instance.rightHandSlipPercentage = (float)funcParems[6];
            __instance.rightHandSlide = (bool)funcParems[7];
            __instance.rightHandSlideNormal = (Vector3)funcParems[8];
            ___rightHandColliding = (bool)funcParems[9];
            __instance.rightHandMaterialTouchIndex = (int)funcParems[10];
            __instance.rightHandSurfaceOverride = (GorillaSurfaceOverride)funcParems[11];

            /* calculate player movement vector */
            ___touchPoints = 0;
            ___rigidBodyMovement = Vector3.zero;

            // add left hand position change
            if (___leftHandColliding || __instance.wasLeftHandTouching) {
                ___rigidBodyMovement += ___firstIterationLeftHand;
                ++___touchPoints;
			}

            // add right hand position change
            if (___rightHandColliding || __instance.wasRightHandTouching) {
                ___rigidBodyMovement += ___firstIterationLeftHand;
                ++___touchPoints;
            }

            if (___touchPoints != 0)
                ___rigidBodyMovement /= (float)___touchPoints;

            // clamp movement vector to a collision point
            // variables passed to the function
            funcParems = new object[] { __instance.lastHeadPosition,
                                        __instance.headCollider.radius * 0.9f,
                                        __instance.headCollider.transform.position + ___rigidBodyMovement - __instance.lastHeadPosition,
                                        ___finalPosition,
                                        false,
                                        ___slipPercentage,
                                        ___junkHit,
                                        true };

            // store the function result
            bool resultBool = false;

            // run GorillaLocomotion.Player.IterativeCollisionSphereCast(...)
             resultBool = (bool)IterativeCollisionSphereCast.Invoke(__instance, funcParems);

            // assign the out values
            ___finalPosition = (Vector3)funcParems[3];
            ___slipPercentage = (float)funcParems[5];
            ___junkHit = (RaycastHit)funcParems[6];

			// check the result and update the movement vector if true
			if (resultBool) {
                ___rigidBodyMovement = ___finalPosition - __instance.headCollider.transform.position;
			}

            // check if we might hit in the future
            resultBool = Physics.Raycast(__instance.lastHeadPosition,
                                         __instance.headCollider.transform.position - __instance.lastHeadPosition + ___rigidBodyMovement,
                                         out ___hitInfo,
                                         (__instance.headCollider.transform.position - __instance.lastHeadPosition + ___rigidBodyMovement).magnitude,
                                         __instance.locomotionEnabledLayers.value);

            // if true clamp, clamp the movement vector to the difference in head position
            if (resultBool) {
                ___rigidBodyMovement = __instance.lastHeadPosition - __instance.headCollider.transform.position;
			}

            // move the player to the new positon
            if (___rigidBodyMovement != Vector3.zero) {
                __instance.transform.position = __instance.transform.position + ___rigidBodyMovement;
            }

            /* update last position variables */
            __instance.lastHeadPosition = __instance.headCollider.transform.position;
            ___areBothTouching = (!___leftHandColliding && !__instance.wasLeftHandTouching) || (!___rightHandColliding && !__instance.wasRightHandTouching);

            // update left hand
            // variables passed to the function
            funcParems = new object[] { __instance.leftHandTransform,
                                        __instance.leftHandOffset,
                                        ___lastLeftHandPosition,
                                        ___areBothTouching,
                                        ___leftHandColliding,
                                        ___leftHandColliding,
                                        __instance.leftHandSlide,
                                        __instance.leftHandSlide,
                                        __instance.leftHandMaterialTouchIndex,
                                        __instance.leftHandMaterialTouchIndex,
                                        __instance.leftHandSurfaceOverride,
                                        __instance.leftHandSurfaceOverride };

            // run GorillaLocomotion.Player.FinalHandPosition(...)
            ___lastLeftHandPosition = (Vector3)FinalHandPosition.Invoke(__instance, funcParems);

            // asign the out values
            ___leftHandColliding = (bool)funcParems[5];
            __instance.leftHandSlide = (bool)funcParems[7];
            __instance.leftHandMaterialTouchIndex = (int)funcParems[9];
            __instance.leftHandSurfaceOverride = (GorillaSurfaceOverride)funcParems[11];

            // update right hand
            // variables passed to the function
            funcParems = new object[] { __instance.rightHandTransform,
                                        __instance.rightHandOffset,
                                        ___lastRightHandPosition,
                                        ___areBothTouching,
                                        ___rightHandColliding,
                                        ___rightHandColliding,
                                        __instance.rightHandSlide,
                                        __instance.rightHandSlide,
                                        __instance.rightHandMaterialTouchIndex,
                                        __instance.rightHandMaterialTouchIndex,
                                        __instance.rightHandSurfaceOverride,
                                        __instance.rightHandSurfaceOverride };

            // run GorillaLocomotion.Player.FinalHandPosition(...)
            ___lastRightHandPosition = (Vector3)FinalHandPosition.Invoke(__instance, funcParems);

            // asign the out values
            ___rightHandColliding = (bool)funcParems[5];
            __instance.rightHandSlide = (bool)funcParems[7];
            __instance.rightHandMaterialTouchIndex = (int)funcParems[9];
            __instance.rightHandSurfaceOverride = (GorillaSurfaceOverride)funcParems[11];

            StoreVelocities.Invoke(__instance, null);
            __instance.didAJump = false;

            /* slide stuff?? */
            if (__instance.leftHandSlide || __instance.rightHandSlide) {
                ___slideAverage = Vector3.zero;
                ___touchPoints = 0;
                ___averageSlipPercentage = 0f;

                if (__instance.leftHandSlide) {
                    ___slideAverageNormal += __instance.leftHandSlideNormal.normalized;
                    ___averageSlipPercentage += __instance.leftHandSlipPercentage;
                    ++___touchPoints;
                }

                if (__instance.rightHandSlide) {
                    ___slideAverageNormal += __instance.rightHandSlideNormal.normalized;
                    ___averageSlipPercentage += __instance.rightHandSlipPercentage;
                    ++___touchPoints;
                }

                ___slideAverageNormal = ___slideAverageNormal.normalized;
                ___averageSlipPercentage /= ___touchPoints;

                if (___touchPoints == 1) {
                    ___surfaceDirection = __instance.rightHandSlide ? Vector3.ProjectOnPlane(__instance.rightHandTransform.forward, __instance.rightHandSlideNormal) : Vector3.ProjectOnPlane(__instance.leftHandTransform.forward, __instance.leftHandSlideNormal);

                    if (Vector3.Dot(___slideAverage, ___surfaceDirection) > 0f) {
                        ___slideAverage = Vector3.Project(___slideAverage, Vector3.Slerp(___slideAverage, ___surfaceDirection.normalized * ___slideAverage.magnitude, __instance.slideControl));

                    } else {
                        ___slideAverage = Vector3.Project(___slideAverage, Vector3.Slerp(___slideAverage, -___surfaceDirection.normalized * ___slideAverage.magnitude, __instance.slideControl));
                    }
                }

                if (!__instance.wasLeftHandSlide && !__instance.wasRightHandSlide) {
                    ___slideAverage = (Vector3.Dot(___playerRigidBody.velocity, ___slideAverageNormal) <= 0f) ? Vector3.ProjectOnPlane(___playerRigidBody.velocity, ___slideAverageNormal) : ___playerRigidBody.velocity;

                } else {
                    ___slideAverage = (Vector3.Dot(___slideAverage, ___slideAverageNormal) <= 0f) ? Vector3.ProjectOnPlane(___slideAverage, ___slideAverageNormal) : ___slideAverage;
                }

                ___slideAverage = ___slideAverage.normalized * Mathf.Min(___slideAverage.magnitude, Mathf.Max(0.5f, ___denormalizedVelocityAverage.magnitude * 2f));
                ___playerRigidBody.velocity = Vector3.zero;

            } else if (___leftHandColliding || ___rightHandColliding) {
                if (!__instance.didATurn) {
                    ___playerRigidBody.velocity = Vector3.zero;

                } else {
                    ___playerRigidBody.velocity = ___playerRigidBody.velocity.normalized * Mathf.Min(2f, ___playerRigidBody.velocity.magnitude);
                }

            } else if (__instance.wasLeftHandSlide || __instance.wasRightHandSlide) {
                ___playerRigidBody.velocity = (Vector3.Dot(___slideAverage, ___slideAverageNormal) <= 0f) ? Vector3.ProjectOnPlane(___slideAverage, ___slideAverageNormal) : ___slideAverage;
            }

            if ((___rightHandColliding || ___leftHandColliding) && !__instance.disableMovement && !__instance.didATurn) {
                if (__instance.rightHandSlide || __instance.leftHandSlide) { 
                    if (Vector3.Project(___denormalizedVelocityAverage, ___slideAverageNormal).magnitude > __instance.slideVelocityLimit 
                        && Vector3.Dot(___denormalizedVelocityAverage, ___slideAverageNormal) > 0f 
                        && Vector3.Project(___denormalizedVelocityAverage, ___slideAverageNormal).magnitude > Vector3.Project(___slideAverage, ___slideAverageNormal).magnitude) 
                    {
                        Debug.Log("MonkeSwim did a sliding jump");
                        __instance.leftHandSlide = false;
                        __instance.rightHandSlide = false;
                        __instance.didAJump = true;
                        ___playerRigidBody.velocity = Mathf.Min(__instance.maxJumpSpeed, __instance.jumpMultiplier * Vector3.Project(___denormalizedVelocityAverage, ___slideAverageNormal).magnitude) * ___slideAverageNormal.normalized + Vector3.ProjectOnPlane(___slideAverage, ___slideAverageNormal);
					}
                
                } else if (___denormalizedVelocityAverage.magnitude > __instance.velocityLimit) {
                    Debug.Log("MonkeSwim did a regular jump");
                    __instance.didAJump = true;
                    ___playerRigidBody.velocity = Mathf.Min(__instance.maxJumpSpeed, __instance.jumpMultiplier * ___denormalizedVelocityAverage.magnitude) * ___denormalizedVelocityAverage.normalized;
				}
			}

            Vector3 currentHandPos;

            // store left hand postiion
            if (___leftHandColliding) {
                currentHandPos = (Vector3)CurrentLeftHandPosition.Invoke(__instance, null);
                
                if((currentHandPos - ___lastLeftHandPosition).magnitude > __instance.unStickDistance && !Physics.SphereCast(__instance.headCollider.transform.position, 
                                                                                                                            __instance.minimumRaycastDistance * __instance.defaultPrecision, 
                                                                                                                            currentHandPos - __instance.headCollider.transform.position, 
                                                                                                                            out ___hitInfo, 
                                                                                                                            (currentHandPos - __instance.headCollider.transform.position).magnitude - __instance.minimumRaycastDistance, __instance.locomotionEnabledLayers.value)) 
                {
                    ___lastLeftHandPosition = currentHandPos;
                    ___leftHandColliding = false;
				}
            }

            // store right hand position
            if (___rightHandColliding) {
                currentHandPos = (Vector3)CurrentRightHandPosition.Invoke(__instance, null);

                if ((currentHandPos - ___lastRightHandPosition).magnitude > __instance.unStickDistance && !Physics.SphereCast(__instance.headCollider.transform.position,
                                                                                                                            __instance.minimumRaycastDistance * __instance.defaultPrecision,
                                                                                                                            currentHandPos - __instance.headCollider.transform.position,
                                                                                                                            out ___hitInfo,
                                                                                                                            (currentHandPos - __instance.headCollider.transform.position).magnitude - __instance.minimumRaycastDistance, __instance.locomotionEnabledLayers.value)) {
                    ___lastRightHandPosition = currentHandPos;
                    ___rightHandColliding = false;
                }
            }

            __instance.leftHandFollower.position = ___lastLeftHandPosition;
            __instance.rightHandFollower.position = ___lastRightHandPosition;
            __instance.wasLeftHandTouching = ___leftHandColliding;
            __instance.wasRightHandTouching = ___rightHandColliding;
            __instance.wasLeftHandSlide = __instance.leftHandSlide;
            __instance.wasRightHandSlide = __instance.rightHandSlide;
            __instance.didATurn = false;
            __instance.BodyCollider();

            return false;
        }

        #endregion

        #region VRRIGUPDATE
        [HarmonyPatch(typeof(VRRig))]
        [HarmonyPrefix, HarmonyPatch("LateUpdate", MethodType.Normal)]
        internal static bool VRRig_TransformOverride(VRRig __instance, ref float ___ratio, 
                                                                       ref float ___timeSpawned,
                                                                       ref float[] ___speedArray,
                                                                       ref int ___tempMatIndex, 
                                                                       ref Photon.Realtime.Player ___tempPlayer)
        {
            //if mod is off skip this function
            if (!modEnabled) return true;

            if (!__instance.isOfflineVRRig && (__instance.photonView == null || __instance.photonView.Owner == null || (__instance.photonView.Owner != null && PhotonNetwork.CurrentRoom.Players.TryGetValue(__instance.photonView.Owner.ActorNumber, out ___tempPlayer) && ___tempPlayer == null))) {
                
                GorillaParent.instance.vrrigs.Remove(__instance);

                if(__instance.photonView != null) {
                    GorillaParent.instance.vrrigDict.Remove(__instance.photonView.Owner);
                }

                Object.Destroy(__instance);
            }

            if (GorillaGameManager.instance != null)
            {
                ___speedArray = GorillaGameManager.instance.LocalPlayerSpeed();
                Player.Instance.jumpMultiplier = ___speedArray[1];
                Player.Instance.maxJumpSpeed = ___speedArray[0];
            
            } else {
                Player.Instance.jumpMultiplier = 1.1f;
                Player.Instance.maxJumpSpeed = 6.5f;
            }

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

            // networked other player sync / lerp stuff
            } else {

                if (__instance.kickMe && PhotonNetwork.IsMasterClient) {
                    __instance.kickMe = false;
                    PhotonNetwork.CloseConnection(__instance.photonView.Owner);
                }

                // body lerp
                if (Time.time > ___timeSpawned + __instance.doNotLerpConstant) {
                    __instance.transform.position = Vector3.Lerp(__instance.transform.position, __instance.syncPos, __instance.lerpValueBody * 0.66f);

                } else {
                    __instance.transform.position = __instance.syncPos;
                }

                __instance.transform.rotation = Quaternion.Lerp(__instance.transform.rotation, __instance.syncRotation, __instance.lerpValueBody);

                // head lerp
                __instance.head.syncPos = Quaternion.FromToRotation(__instance.transform.up, Vector3.up) * __instance.transform.rotation * -__instance.headBodyOffset;
                __instance.head.MapOther(__instance.lerpValueBody);

                // right hand lerp
                __instance.rightHand.MapOther(__instance.lerpValueBody);
                __instance.rightIndex.MapOtherFinger((float)char.GetNumericValue(__instance.handSync.ToString().PadLeft(6)[5]) / 10f, __instance.lerpValueFingers);
                __instance.rightMiddle.MapOtherFinger((float)char.GetNumericValue(__instance.handSync.ToString().PadLeft(6)[4]) / 10f, __instance.lerpValueFingers);
                __instance.rightThumb.MapOtherFinger((float)char.GetNumericValue(__instance.handSync.ToString().PadLeft(6)[3]) / 10f, __instance.lerpValueFingers);

                // left hand lerp
                __instance.leftHand.MapOther(__instance.lerpValueBody);
                __instance.leftIndex.MapOtherFinger((float)char.GetNumericValue(__instance.handSync.ToString().PadLeft(6)[2]) / 10f, __instance.lerpValueFingers);
                __instance.leftMiddle.MapOtherFinger((float)char.GetNumericValue(__instance.handSync.ToString().PadLeft(6)[1]) / 10f, __instance.lerpValueFingers);
                __instance.leftThumb.MapOtherFinger((float)char.GetNumericValue(__instance.handSync.ToString().PadLeft(6)[0]) / 10f, __instance.lerpValueFingers);

                // initialize cosmetics
                if (!__instance.initializedCosmetics && GorillaTagManager.instance != null && GorillaTagManager.instance.playerCosmeticsLookup.TryGetValue(__instance.photonView.Owner.UserId, out __instance.tempString)) {
                    __instance.initializedCosmetics = true;
                    __instance.concatStringOfCosmeticsAllowed = __instance.tempString;
                    CheckForEarlyAccess.Invoke(__instance, null);
                    __instance.SetCosmeticsActive();
                }
            }

            // looks like this stuff is for turning voice chat on or off
            if (!__instance.isOfflineVRRig) {
                
                if (PhotonNetwork.IsMasterClient && GorillaGameManager.instance == null) {
                    PhotonNetwork.InstantiateRoomObject("GorillaPrefabs/GorillaTagManager", Vector3.zero, Quaternion.identity, 0);
                }

                ___tempMatIndex =  (GorillaGameManager.instance != null ? GorillaGameManager.instance.MyMatIndex(__instance.photonView.Owner) : 0);
                if(__instance.setMatIndex != ___tempMatIndex) {
                    __instance.setMatIndex = ___tempMatIndex;
                    __instance.ChangeMaterialLocal(__instance.setMatIndex);
                }

                __instance.GetComponent<PhotonVoiceView>().SpeakerInUse.enabled = GorillaComputer.instance.voiceChatOn == "TRUE" && !__instance.muted;
            }

            //skips the original function
            return false;
        }
        #endregion

        #region RIGMAPPING
        [HarmonyPatch(typeof(VRMap))]
        [HarmonyPrefix, HarmonyPatch("MapMine", MethodType.Normal)]
        internal static bool MapMine(VRMap __instance, ref float ratio, ref Transform playerOffsetTransform)
        {
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
        #endregion

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
            Transform playerTransform = playerInstance.transform;
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
