using UnityEngine;

namespace MonkeSwim.Config
{
    public abstract class EnterToggleTrigger : ToggleBase
    {
#if GAME
        // private bool hasToggled = false;
        // public bool CanToggle { get; set; } = true;

        protected override void PlayerEnter()
        {
            base.PlayerEnter();

            target.SetActive(!target.activeSelf);
        }
#endif
    }
}