using UnityEngine;

namespace MonkeSwim.Config
{
    [System.Serializable]
    public class MonkeSwimConfig : MonoBehaviour
    {
        [Tooltip("this overrides GlobalSettings and any custom settinsg inside a zone.\n" +
                 "defaults: MaxSpeed 6.5f, Acceleration 1.1f, Resistence 0f, GravityAmount 0f")]
        public static bool UseDefault = false;

        [Tooltip("if this is true the mod will be enabled accross the entire map")]
        public static bool EntireMap = false;

        [Tooltip("these settings will apply to every zone unless overriden by the trigger")]
        public static MonkeSwimSettings GlobalSettings;

        //settings that will be used if UseDefault is true
        public static readonly MonkeSwimSettings DefaultSettings = new MonkeSwimSettings{ MaxSpeed = 6.5f, Acceleration = 1.1f, Resistence = 0f, GravityAmount = 0f };
    }

    [System.Serializable]
    public struct MonkeSwimSettings
    {
        public float MaxSpeed;
        public float Acceleration;
        public float Resistence;
        public float GravityAmount;
    }
}