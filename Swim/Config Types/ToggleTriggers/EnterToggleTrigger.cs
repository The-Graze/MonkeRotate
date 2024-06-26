using UnityEngine;

namespace MonkeRotate.Config
{
    public class EnterToggleTrigger : ToggleBase
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