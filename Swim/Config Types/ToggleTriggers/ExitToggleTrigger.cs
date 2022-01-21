using UnityEngine;

namespace MonkeSwim.Config
{
    public abstract class ExitToggleTrigger : ToggleBase
    {
#if GAME
        // private bool hasToggled = false;
        // public bool CanToggle { get; set; } = true;

        protected override void PlayerExit()
        {
            base.PlayerExit();

            target.SetActive(!target.activeSelf);
        }
#endif
    }
}