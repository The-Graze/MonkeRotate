using UnityEngine;

using MonkeSwim.Config;

#if GAME
using MonkeSwim.Tools.Trackers;
using MonkeSwim.Tools.Averages;
#endif

namespace MonkeSwim.Managers
{
    public class SwimManager : MonoBehaviour
    {
        [Tooltip("wether or not the swim movement should be enabled globally")]
        [SerializeField] public bool Global = false;

        [Tooltip("settings to use as default by trigger zones or when Global is enabled")]
        [SerializeField] public SwimSettings GlobalSettings = null;
#if GAME
        private SwimHandTracker rightHand = null;
        private SwimHandTracker leftHand = null;
        private MovementManager movementManager = null;

        private SwimSettingsAverage settings;


        public static SwimManager Instance { get; private set; }

        public void Awake()
        {
            if (Instance != null) {
                GameObject.Destroy(this);
                return;
            }

            Instance = this;

            VmodMonkeMapLoader.Events.OnMapEnter += MapEnterCallback;

            Debug.Log("SwimManager: lets go swimming XD");
        }

        public void Start()
        {
            rightHand = GorillaLocomotion.Player.Instance.rightHandFollower.gameObject.AddComponent<SwimHandTracker>();
            rightHand.Controller = MonkeSwimManager.Instance.RightController;
            rightHand.enabled = false;

            leftHand = GorillaLocomotion.Player.Instance.leftHandFollower.gameObject.AddComponent<SwimHandTracker>();
            leftHand.Controller = MonkeSwimManager.Instance.LeftController;
            leftHand.enabled = false;

            movementManager = MonkeSwimManager.Instance.Movement;

            if (GlobalSettings == null) {
                GlobalSettings = gameObject.AddComponent<SwimSettings>();
            }

            this.enabled = false;
        }

        public void OnDestroy()
        {
            if (Instance == this) Instance = null;
            VmodMonkeMapLoader.Events.OnMapEnter -= MapEnterCallback;
        }

        public void LateUpdate()
        {

            // add resistence first so swim velocities have priority
            // Vector3 resistenceVelocity = (Vector3.zero - movementManager.Velocity) * -1f;
            // movementManager.AddPlayerVelocity(resistenceVelocity.normalized, settings.Resistence, resistenceVelocity.magnitude);

            movementManager.AddPlayerResistence(settings.Resistence);

            float rightHandSpeed = rightHand.speed;
            float leftHandSpeed = leftHand.speed;

            // Debug.Log("SwimManager: right hand speed: " + rightHandSpeed);
            // Debug.Log("SwimManager: left hand speed: " + leftHandSpeed);
            // Debug.Log(string.Format("SwimmManager: SwimSettings :\n MaxSpeed: {0} \nAcelleration: {1} \nResistence: {2}", settings.MaxSpeed, settings.Acceleration, settings.Resistence));


            float precision = 0.001f;

            //so tracking inaccuracy doesn't cause movement
            bool rightHandMoving = rightHandSpeed > precision;
            bool leftHandMoving = leftHandSpeed > precision;

            // moving the decimal place up 2
            rightHandSpeed *= 100f;
            leftHandSpeed *= 100f;

            Vector3 lookDirectionAssist = movementManager.LookDirection * 0.2f;

            if (rightHandMoving) {
                movementManager.AddPlayerVelocity((rightHand.Direction + lookDirectionAssist).normalized, settings.Acceleration * rightHandSpeed, settings.MaxSpeed);
            }

            if (leftHandMoving) {
                movementManager.AddPlayerVelocity((leftHand.Direction + lookDirectionAssist).normalized, settings.Acceleration * leftHandSpeed, settings.MaxSpeed);
            }
        }

        public void AddSettings(bool useGlobalSettings, SwimSettings newSettings)
        {
            if(settings.Amount == 0) {
                if (useGlobalSettings) {
                    settings = new SwimSettingsAverage(GlobalSettings.MaxSpeed, GlobalSettings.Acceleration, GlobalSettings.Resistence);
                
                } else {
                    settings = new SwimSettingsAverage(newSettings.MaxSpeed, newSettings.Acceleration, newSettings.Resistence);                }

            } else {
                settings += (useGlobalSettings ? GlobalSettings : newSettings);
            }

            this.enabled = true;
            rightHand.enabled = true;
            leftHand.enabled = true;
        }

        public void RemoveSettings(bool useGlobalSettings, SwimSettings oldSettings)
        {
            if(settings.Amount == 0) {
                Debug.LogWarning("monkeswim: trying to remove settings that don't exist, how did this happen?");
                Debug.Log("monkeswim: setting settings to global settings");

                settings = new SwimSettingsAverage(GlobalSettings.MaxSpeed, GlobalSettings.Acceleration, GlobalSettings.Resistence, 0);
                if (Global) {
                    enabled = true;
                    rightHand.enabled = true;
                    leftHand.enabled = true;
                }
                return;
            }

            if (useGlobalSettings) {
                settings -= GlobalSettings;
            
            } else {
                settings -= oldSettings;
            }

            if(settings.Amount == 0) {
                if (Global) {
                    settings = new SwimSettingsAverage(GlobalSettings.MaxSpeed, GlobalSettings.Acceleration, GlobalSettings.Resistence, 0);
                    return;
                
                } else {
                    enabled = false;
                    rightHand.enabled = false;
                    leftHand.enabled = false;
                }
            }
        }


        public void MapEnterCallback(bool enter)
        {
            if (enter) {
                if (Global) {
                    enabled = true;
                    rightHand.enabled = true;
                    leftHand.enabled = true;
                }

                settings = new SwimSettingsAverage(GlobalSettings.MaxSpeed, GlobalSettings.Acceleration, GlobalSettings.Resistence, 0);
                return;
            }

            enabled = false;
            rightHand.enabled = false;
            leftHand.enabled = false;
        }

#endif
    }
}