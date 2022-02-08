using UnityEngine;

namespace MonkeSwim.Config
{
    public abstract class PlayerTrigger : MonoBehaviour
    {
#if GAME
        protected bool playerCollided = false;
        protected Collider playerCollider = null;

        private void OnTriggerEnter(Collider collider)
        {
            if (!playerCollided && collider.name.Equals("Body Collider")) {
                playerCollider = collider;
                PlayerEnter();
            }
        }

       private void OnTriggerExit(Collider collider)
        {
            if (playerCollided && collider == playerCollider) {
                PlayerExit();
            }
        }

        protected virtual void PlayerEnter()
        {
            playerCollided = true;
        }

        protected virtual void PlayerExit()
        {
            playerCollider = null;
            playerCollided = false;         
        }
#endif
    }
}