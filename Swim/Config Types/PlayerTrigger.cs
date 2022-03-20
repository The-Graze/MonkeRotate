using UnityEngine;

namespace MonkeSwim.Config
{
    public abstract class PlayerTrigger : MonoBehaviour
    {
#if GAME
        protected bool isPlayerCollided = false;
        protected Collider playerCollider = null;

        private void OnTriggerEnter(Collider collider)
        {
            if (!isPlayerCollided && collider.name.Equals("Body Collider")) {
                // Debug.Log("player entered call");
                playerCollider = collider;
                PlayerEnter();
            }
        }

       private void OnTriggerExit(Collider collider)
        {
            if (isPlayerCollided && collider == playerCollider) {
                // Debug.Log("player exit called");
                PlayerExit();
            }
        }

        protected virtual void PlayerEnter()
        {
            isPlayerCollided = true;
        }

        protected virtual void PlayerExit()
        {
            playerCollider = null;
            isPlayerCollided = false;         
        }
#endif
    }
}