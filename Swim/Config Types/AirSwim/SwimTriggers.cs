using UnityEngine;

namespace MonkeSwim.Config
{
    class SwimTrigger : SwimSettings
    {
        [Tooltip("enable this setting to use global settings")]
        public bool UseGlobalSettings = true;

#if GAME
        protected static Managers.SwimManager swimManager = null;

        public void Start()
        {
            if (swimManager != null && swimManager == Managers.SwimManager.Instance) return;
            swimManager = Managers.SwimManager.Instance;
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has entered trigger " + gameObject.name);

            swimManager.AddSettings(UseGlobalSettings, (SwimSettings)this);
        }

        public void OnTriggerExit(Collider collider)
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has left trigger " + gameObject.name);

            swimManager.RemoveSettings(UseGlobalSettings, (SwimSettings)this);
        }
#endif
    }
}