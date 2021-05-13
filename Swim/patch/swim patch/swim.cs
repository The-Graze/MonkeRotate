using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;
using GorillaLocomotion;

namespace MonkeSwim
{
    //class for handling average direction vector of the players swing
    class AverageVelocityDirection
    {
        public float speed { get; private set; }
        public Vector3 direction { get; private set; }

        //update this at the end of each update
        public static Vector3 lastParentPosition { private get; set; }

        private static Player playerInstance;

        //amount of directions accumulated
        private int vectorAmount;

        private Vector3 velocityDirectionAccumulator;

        private FieldInfo lastPosition;
        private MethodInfo currentPosition;

        private AverageVelocityDirection() { }

        //constructor
        public AverageVelocityDirection(string lastPos, string currentPos)
        {
            speed = 0f;
            vectorAmount = 0;
            velocityDirectionAccumulator = Vector3.zero;

            lastPosition = AccessTools.Field(typeof(Player), lastPos);
            currentPosition = AccessTools.Method(typeof(Player), currentPos);
        }

        public void Reset()
        {
            speed = 0f;
            vectorAmount = 0;
            velocityDirectionAccumulator = Vector3.zero;
        }

        public void Update()
        {
            if (playerInstance != null) {
                Vector3 lastPos = (Vector3)lastPosition.GetValue(playerInstance);
                Vector3 currentPos = (Vector3)currentPosition.Invoke(playerInstance, null);

                Vector3 lastLocalised = lastPos - lastParentPosition;
                Vector3 currentLocalized = currentPos - playerInstance.transform.position;

                ++vectorAmount;

                Vector3 newDir = lastLocalised - currentLocalized;

                velocityDirectionAccumulator += vectorAmount == 1 ? newDir : newDir / 2;
                direction = (velocityDirectionAccumulator / vectorAmount).normalized;

                speed = newDir.magnitude;
            }
        }

        public static void SetPlayer(Player instance) { playerInstance = instance; }
    }

    [HarmonyPatch(typeof(Player))]
    internal class Swim
    {
        public static bool canFly = false;
        public static float dragValue = 0f;
        public static float swimMultiplier = 0f;
        public static float maxSwimSpeed = 0f;

        private static bool useDefault = false;
        private static bool useGlobal = false;

        private static Rigidbody playerRigidRef;

        private static InputDevice rInputDevice;
        private static InputDevice lInputDevice;

        private static AverageVelocityDirection leftHand = null;
        private static AverageVelocityDirection rightHand = null;


        //player.update()
        //before function
        [HarmonyPrefix, HarmonyPatch("Update", MethodType.Normal)]
        internal static void Prefix(Player __instance, out Vector3 __state)
        {
            __state = Vector3.zero;
            Vector3 velocity = Vector3.zero;

            if (canFly) {

                bool rightInput = CheckInput(rInputDevice);
                bool leftInput = CheckInput(lInputDevice);

                Vector3 leftVelocity = Vector3.zero;
                Vector3 rightVelocity = Vector3.zero;

                if (rightInput || leftInput) {

                    float precision = 0.001f;
                    Vector3 cameraOffset = new Vector3(0f, Camera.main.transform.forward.y, 0f);

                    float rightSpeed = 0f;

                    if (rightInput) {
                        rightHand.Update();
                        rightSpeed = rightHand.speed;

                        if (rightSpeed >= precision && !float.IsNaN(rightSpeed)) rightVelocity = (rightHand.direction + cameraOffset).normalized * (swimMultiplier * rightSpeed);
                    }

                    float leftSpeed = 0f;

                    if (leftInput) {
                        leftHand.Update();
                        leftSpeed = leftHand.speed;

                        if (leftSpeed >= precision && !float.IsNaN(leftSpeed)) leftVelocity = (leftHand.direction + cameraOffset).normalized * (swimMultiplier * rightSpeed);
                    }

                    //Debug.Log("right velocity: " + rightSpeed);
                    //Debug.Log("left velocity: " + leftSpeed);

                    //if the hands are still but buttons are still pressed, reset th accumulators to zero              
                    if (rightSpeed == 0f) rightHand.Reset();
                    if (leftSpeed == 0f) leftHand.Reset();

                    velocity = leftVelocity + rightVelocity;

                    //Debug.Log("velocity: " + velocity);

                    //if no inputs then reset the accumulators
                } else {
                    if (!rightInput) rightHand.Reset();
                    if (!leftInput) leftHand.Reset();
                }

                //only go as fast as the maximum swim speed
                float speed = velocity.magnitude;
                if (speed > maxSwimSpeed) velocity = velocity.normalized * maxSwimSpeed;
            }

            __state = velocity;
            //end of function
        }

        //player.update()
        //after function
        [HarmonyPostfix, HarmonyPatch("Update", MethodType.Normal)]
        internal static void Postfix(Player __instance, Vector3 __state)
        {
            if (canFly) {
                bool rigidExists = (playerRigidRef != null);
                if (__state != Vector3.zero && rigidExists) playerRigidRef.velocity += __state;

                //so we don't end up flying too fast.
                if (playerRigidRef.velocity.magnitude > maxSwimSpeed && rigidExists) playerRigidRef.velocity = playerRigidRef.velocity.normalized * maxSwimSpeed;

                AverageVelocityDirection.lastParentPosition = __instance.transform.position;
            }

            //end of function
        }

        //player.awake()
        //after function
        [HarmonyPostfix, HarmonyPatch("Awake", MethodType.Normal)]
        internal static void InitializeObjects(Player __instance)
        {
            rInputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            lInputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

            playerRigidRef = (Rigidbody)AccessTools.Field(typeof(Player), "playerRigidBody").GetValue(__instance);

            leftHand = new AverageVelocityDirection("lastLeftHandPosition", "CurrentLeftHandPosition");
            rightHand = new AverageVelocityDirection("lastRightHandPosition", "CurrentRightHandPosition");

            AverageVelocityDirection.SetPlayer(__instance);
            AverageVelocityDirection.lastParentPosition = __instance.transform.position;


            //end of function
        }

        internal static void StartMod()
        {
            //Debug.Log("attemempting to find air swim config");

            GameObject airSwimConfig = GameObject.Find("AirSwimConfig");

            Debug.Log("airswimconfig = " + (airSwimConfig != null).ToString());

            if (airSwimConfig != null) {

                Debug.Log("air swim exists");

                Transform waterSwim = airSwimConfig.transform.Find("WaterSwimTriggers");
                Transform useDefaults = airSwimConfig.transform.Find("AirSwimConfigDefault");
                Transform globalSettings = airSwimConfig.transform.Find("AirSwimConfigGlobal");

                if (globalSettings != null) {
                    Debug.Log("use custom global settings");
                    maxSwimSpeed = globalSettings.localPosition.x;
                    swimMultiplier = globalSettings.localPosition.y;
                    dragValue = globalSettings.localPosition.z;

                    useGlobal = true;

                }else if (useDefaults != null) {
                    Debug.Log("use default settings");
                    maxSwimSpeed = 6.5f;
                    swimMultiplier = 1.1f;
                    dragValue = 0f;

                    useDefault = true;

                } else { 
                    Debug.Log("use custom settings in AirSwimConfig");
                    maxSwimSpeed = airSwimConfig.transform.localPosition.x;
                    swimMultiplier = airSwimConfig.transform.localPosition.y;
                    dragValue = airSwimConfig.transform.localPosition.z;
                }

                if (waterSwim != null) {
                    GameObject waterObject = waterSwim.gameObject;

                    if (waterObject != null) {
                        Collider[] triggers = waterObject.GetComponentsInChildren<Collider>(true);
                        Debug.Log("amount of potential triggers found: " + triggers.Length);

                        foreach (Collider areaTrigger in triggers) {
                            if (areaTrigger != null && areaTrigger.isTrigger) {
                                Debug.Log("adding trigger");
                                areaTrigger.gameObject.AddComponent<SwimTrigger>();
                            }
                        }

                    }

                    EnableMod(false);

                } else EnableMod(true);
            }

        }

        private static bool CheckInput(InputDevice input)
        {
            bool flag = false;

            input.TryGetFeatureValue(CommonUsages.triggerButton, out flag);
            if (flag) return flag;

            //input.TryGetFeatureValue(CommonUsages.gripButton, out flag);
            return flag;
        }

        public static void EnableMod(bool toEnable)
        {
            if (toEnable) {
               //Debug.Log("mod enable is true");
                if (playerRigidRef != null) {
                    playerRigidRef.drag = dragValue;
                    playerRigidRef.useGravity = false;
                }

            } else {
                //Debug.Log("mod enable is false");
                if (playerRigidRef != null) {
                    playerRigidRef.drag = 0f;
                    playerRigidRef.useGravity = true;
                }
            }

            canFly = toEnable;
            Debug.Log("canFly = " + canFly.ToString());
        }

        public static void SetStats(Vector3 stats)
        {
            if (useDefault || useGlobal) return;

            maxSwimSpeed = stats.x;
            swimMultiplier = stats.y;
            dragValue = stats.z;
        }

        //end of class
    }

    class SwimTrigger : MonoBehaviour
    {
        public void OnTriggerEnter(Collider collider)
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has entered trigger " + gameObject.name);
            Debug.Log("setting stats of " + transform.parent.localPosition);
            Swim.SetStats(transform.parent.localPosition);
            Swim.EnableMod(true);
        }
        public void OnTriggerExit(Collider collider) 
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has left trigger " + gameObject.name);
            Swim.EnableMod(false); 
        }
    }
}

