using UnityEngine;

namespace MonkeSwim.Config
{
    public abstract class ToggleBase : PlayerTrigger
    {
        public GameObject target;

#if GAME
        // private bool hasToggled = false;
        // public bool CanToggle { get; set; } = true;

        protected virtual void Awake ()
        {
            if (target == null) gameObject.SetActive(false);
        }
#endif
    }
}