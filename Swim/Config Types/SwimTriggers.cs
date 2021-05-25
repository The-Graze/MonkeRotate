using UnityEngine;

namespace MonkeSwim.Config
{
    class SwimTrigger : MonkeSwimSettings
    {

        [Tooltip("enable the settings in ZoneSettings to apply")]
        public bool OverrideGlobal = false;

#if GAME
        public void OnTriggerEnter(Collider collider)
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has entered trigger " + gameObject.name);

            Swim.AddSettings(OverrideGlobal, (MonkeSwimSettings)this);
        }

        public void OnTriggerExit(Collider collider)
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has left trigger " + gameObject.name);

            Swim.RemoveSettings(OverrideGlobal, (MonkeSwimSettings)this);
        }
#endif
    }
}