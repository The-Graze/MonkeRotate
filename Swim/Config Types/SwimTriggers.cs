using UnityEngine;

namespace MonkeSwim.Config
{
    class SwimTrigger : MonoBehaviour
    {
        public bool OverrideGlobal = false;
        public MonkeSwimSettings ZoneSettings;

    #if GAME
        public void OnTriggerEnter(Collider collider)
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has entered trigger " + gameObject.name);
            Debug.Log("setting stats of " + transform.parent.localPosition);

            /* old stuff
            Swim.SetStats(transform.parent.localPosition);
            Swim.EnableMod(true);
            */
        }
        public void OnTriggerExit(Collider collider)
        {
            if (!collider.gameObject.name.Equals("Body Collider")) return;

            Debug.Log(collider.gameObject.name + " has left trigger " + gameObject.name);
            //Swim.EnableMod(false);
        }
    #endif
    }
}