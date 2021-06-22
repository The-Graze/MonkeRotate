using UnityEngine;

#if GAME
using MonkeSwim.Managers;
#endif

namespace MonkeSwim.Config
{
    public class GravityZone : MonoBehaviour
    {
        [Header("Gravity Settings")]
        [Tooltip("negative number pulls, positive number expels")]
        public float gravityStrength;

        [Header("Rotation Settings")]
        [SerializeField] protected bool rotatePlayer;
        [SerializeField] protected float rotationSpeed;

#if EDITOR
        public bool showDirection { get; set; }
        public float arrowScale { get; set; }
        public Color arrowColor { get; set; }
#endif

#if GAME
        protected Vector3 gravityDirection;
        protected Collider playerCollided = null;

        public virtual void Awake()
        {
            gravityDirection = (gameObject.transform.up * gravityStrength).normalized;
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (playerCollided == null && collider.name.Equals("Body Collider")) playerCollided = collider;
            else return;

            AddGravity();
            AddRotation();
        }

        public void OnTriggerExit(Collider collider)
        {
            if (playerCollided != collider) return;

            playerCollided = null;

            RemoveGravity();
            RemoveRotation();    
        }
        public void OnTriggerStay(Collider collider)
        {
            if (playerCollided != collider) return;
            UpdatedGravity();
        }

        //main function for applying gravity each frame
        protected virtual void UpdatedGravity()
        {
        }

        protected void AddGravity()
        {
            Managers.MovementManager.Instance?.AddDirection(gravityDirection * gravityStrength, gravityStrength, MovementManager.DirectionType.Gravity);
        }

        protected void RemoveGravity()
        {
            Managers.MovementManager.Instance?.RemoveDirection(gravityDirection * gravityStrength, gravityStrength, MovementManager.DirectionType.Gravity);
        }

        protected void AddRotation()
        {
            if (!rotatePlayer) return;
            Managers.MovementManager.Instance?.AddDirection(gravityDirection * rotationSpeed, rotationSpeed, MovementManager.DirectionType.Rotation);
        }

        protected void RemoveRotation()
        {
            if (!rotatePlayer) return;
            Managers.MovementManager.Instance?.RemoveDirection(gravityDirection * rotationSpeed, rotationSpeed, MovementManager.DirectionType.Rotation);
        }

#endif
    }
}