using UnityEngine;

namespace MonkeSwim.Config
{
    [System.Serializable]
    public class MonkeSwimSettings : MonoBehaviour
    {
        public float MaxSpeed = 6.5f;
        public float Acceleration = 1.3f;
        public float Resistence = 0.2f;

        [Tooltip("only works for up and down currently")]
        public float GravityAmount = 0f;

#if GAME
        public void SetSettings(MonkeSwimSettings newSettings)
        {
            MaxSpeed = newSettings.MaxSpeed;
            Acceleration = newSettings.Acceleration;
            Resistence = newSettings.Resistence;
            GravityAmount = newSettings.GravityAmount;
        }

        public string Print()
        {
            return string.Format("[MonkeSwim] MaxSpeed: {0}\n" +
                                 "[MonkeSwim] Acceleration: {1}\n" +
                                 "[MonkeSwim] Resistences: {2}\n" +
                                 "[MonkeSwim] GravityAmount: {3}",
                                  MaxSpeed, Acceleration, Resistence, GravityAmount);
        }
#endif
    }

    class SwimTrigger : MonkeSwimSettings
    {

        [Tooltip("enable the settings in ZoneSettings to apply")]
        public bool OverrideGlobal = false;

#if GAME
        public void OnTriggerEnter(Collider collider)
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has entered trigger " + gameObject.name);

            Managers.Swim.AddSettings(OverrideGlobal, (MonkeSwimSettings)this);
        }

        public void OnTriggerExit(Collider collider)
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has left trigger " + gameObject.name);

            Managers.Swim.RemoveSettings(OverrideGlobal, (MonkeSwimSettings)this);
        }
#endif
    }
}