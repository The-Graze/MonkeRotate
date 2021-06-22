using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;
using GorillaLocomotion;

namespace MonkeSwim.Managers
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
                //Debug.Log("hand speed = " + speed);
            }
        }

        public static void SetPlayer(Player instance) { playerInstance = instance; }
    }

    [HarmonyPatch(typeof(Player))]
    internal static class Swim
    {
        public static bool canFly = false;
    
        private static Rigidbody playerRigidRef;

        private static InputDevice rInputDevice;
        private static InputDevice lInputDevice;

        private static AverageVelocityDirection leftHand = null;
        private static AverageVelocityDirection rightHand = null;

        private static Config.MonkeSwimConfig swimConfig;
        private static Config.MonkeSwimSettings swimSettings;
        private static Config.MonkeSwimSettings swimSettingsAverage;

        //counts how many settings applied to swimSettings.
        //in the case of overlapping zones, settings are an average of the combined settings
        private static uint settingsApplied;

        //since we are modify gravity, store the maps default for when you're not in a zone
        public static Vector3 defaultGraivty;

        //player.update()
        //before function
        [HarmonyPrefix, HarmonyPatch("Update", MethodType.Normal)]
        internal static void Prefix(Player __instance, out Vector3 __state)
        {
            __state = Vector3.zero;
            Vector3 velocity = Vector3.zero;

            if (canFly) {
                if (settingsApplied > 0 || swimConfig.EntireMap) {
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

                            if (rightSpeed >= precision && !float.IsNaN(rightSpeed)) 
                                rightVelocity = (rightHand.direction + cameraOffset).normalized * (swimSettingsAverage.Acceleration * rightSpeed);
                        }

                        float leftSpeed = 0f;

                        if (leftInput) {
                            leftHand.Update();
                            leftSpeed = leftHand.speed;

                            if (leftSpeed >= precision && !float.IsNaN(leftSpeed)) 
                                leftVelocity = (leftHand.direction + cameraOffset).normalized * (swimSettingsAverage.Acceleration * rightSpeed);
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
                    if (speed > swimSettingsAverage.MaxSpeed) velocity = velocity.normalized * swimSettingsAverage.MaxSpeed;
                }
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
                if (settingsApplied > 0 || swimConfig.EntireMap) {
                    bool rigidExists = (playerRigidRef != null);
                    if (__state != Vector3.zero && rigidExists) playerRigidRef.velocity += __state;

                    //so we don't end up flying too fast.
                    if (playerRigidRef.velocity.magnitude > swimSettingsAverage.MaxSpeed && rigidExists)
                        playerRigidRef.velocity = playerRigidRef.velocity.normalized * swimSettingsAverage.MaxSpeed;

                    AverageVelocityDirection.lastParentPosition = __instance.transform.position;
                }
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

            swimConfig = null;
            swimSettings = new Config.MonkeSwimSettings();
            swimSettingsAverage = new Config.MonkeSwimSettings();

            //end of function
        }

        //this function should only be called when map is loaded
        internal static void StartMod()
        {
            //making sure all values are reset to zero when changing out a map
            swimSettings.MaxSpeed = 0f;
            swimSettings.Acceleration = 0f;
            swimSettings.Resistence = 0f;
            swimSettings.GravityAmount = 0f;

            swimSettingsAverage.SetSettings(swimSettings);

            swimConfig = null;

            swimConfig = GameObject.FindObjectOfType<Config.MonkeSwimConfig>();

            if (swimConfig != null) {
                if (swimConfig.GlobalSwimSettings == null) swimConfig.GlobalSwimSettings = new Config.MonkeSwimSettings();
                Debug.Log("MonkeSwimConfig found, mod will be enabled");
                if (swimConfig.EntireMap) swimSettings.SetSettings(swimConfig.GlobalSwimSettings);

                Debug.Log("global swim settings\n" + swimConfig.GlobalSwimSettings.Print());

            } else Debug.Log("monkeswimconfig is null");
                
        }

        public static void AddSettings(bool overrideGlobal, Config.MonkeSwimSettings settings)
        {
            if (!overrideGlobal) settings = swimConfig.GlobalSwimSettings;

            if (settingsApplied == 0 && swimConfig.EntireMap) swimSettings.SetSettings(settings);
            else {
                swimSettings.MaxSpeed += settings.MaxSpeed;
                swimSettings.Acceleration += settings.Acceleration;
                swimSettings.Resistence += settings.Resistence;
                swimSettings.GravityAmount += settings.GravityAmount;
            }

            ++settingsApplied;

            //making sure last parent position is up to date if the mod is getting enabled when this function is called
            AverageVelocityDirection.lastParentPosition = Player.Instance.transform.position;

            Debug.Log("adding settings:\n" + settings.Print());

            UpdateSettings();
        }

        public static void RemoveSettings(bool overrideGlobal, Config.MonkeSwimSettings settings)
        {
            if (!overrideGlobal) settings = swimConfig.GlobalSwimSettings;

            --settingsApplied;

            if (settingsApplied == 0 && swimConfig.EntireMap) swimSettings.SetSettings(swimConfig.GlobalSwimSettings);
            else {
                swimSettings.MaxSpeed -= settings.MaxSpeed;
                swimSettings.Acceleration -= settings.Acceleration;
                swimSettings.Resistence -= settings.Resistence;
                swimSettings.GravityAmount -= settings.GravityAmount;
            }

            Debug.Log("removing settings:\n" + settings.Print());

            UpdateSettings();
        }

        public static void UpdateSettings()
        {
            if (settingsApplied == 0) {
                //swim settings should be set to global settings in RemoveSettings()
                swimSettingsAverage.SetSettings(swimSettings);
                if (!swimConfig.EntireMap) {
                    swimSettingsAverage.Resistence = 0f;
                    swimSettingsAverage.GravityAmount = defaultGraivty.y;
                }

            } else { 
                //lots of conditional statements just to make sure we don't accidentally divide by 0
                swimSettingsAverage.MaxSpeed = (swimSettings.MaxSpeed != 0f ? swimSettings.MaxSpeed / settingsApplied : 0f);
                swimSettingsAverage.Acceleration = (swimSettings.Acceleration != 0f ? swimSettings.Acceleration / settingsApplied : 0f);
                swimSettingsAverage.Resistence = (swimSettings.Resistence != 0f ? swimSettings.Resistence / settingsApplied : 0f);
                swimSettingsAverage.GravityAmount = (swimSettings.GravityAmount != 0f ? swimSettings.GravityAmount / settingsApplied : 0f);   
            } 
            
            if(playerRigidRef != null) playerRigidRef.drag = swimSettingsAverage.Resistence;
            Physics.gravity = new Vector3(0f, swimSettingsAverage.GravityAmount, 0f);
            Debug.Log("Average settings:\n" + swimSettingsAverage.Print());
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
            if (!toEnable) {
                if (playerRigidRef != null) {
                    playerRigidRef.drag = 0f;
                }
                canFly = false;

            } else {
                if (swimConfig != null) { 
                UpdateSettings();
                AverageVelocityDirection.lastParentPosition = Player.Instance.transform.position;
                canFly = true;
                }   
            }

            Debug.Log("canFly = " + canFly.ToString());
        }

        //end of class
    }

}

