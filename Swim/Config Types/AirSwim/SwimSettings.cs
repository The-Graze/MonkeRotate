using UnityEngine;

namespace MonkeRotate.Config
{
    [System.Serializable]
    public class SwimSettings : MonoBehaviour
    {
        public float MaxSpeed = 6.5f;
        public float Acceleration = 1.3f;
        public float Resistence = 0.2f;


#if GAME
        public void SetSettings(SwimSettings newSettings)
        {
            MaxSpeed = newSettings.MaxSpeed;
            Acceleration = newSettings.Acceleration;
            Resistence = newSettings.Resistence;
        }

        public string Print()
        {
            return string.Format("[MonkeSwim] MaxSpeed: {0}\n" +
                                 "[MonkeSwim] Acceleration: {1}\n" +
                                 "[MonkeSwim] Resistences: {2}\n",
                                  MaxSpeed, Acceleration, Resistence);
        }
#endif
    }
}