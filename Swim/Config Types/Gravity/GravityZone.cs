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
        public float maxGravityStrength;

        [Tooltip("enable this for the games global gravity to apply")]
        [SerializeField] public bool UseGravity = false;

        [Header("Rotation Settings")]
        [SerializeField] protected bool rotatePlayer;
        [SerializeField] protected float rotationSpeed;

#if EDITOR
        public bool showDirection { get; set; } = true;
        public float arrowScale { get; set; } = 1f;
        public Color arrowColor { get; set; } = Color.blue;
#endif

#if GAME
        protected Vector3 gravityDirection;
        protected Collider playerCollided = null;
        static protected MovementManager movementManager = null;

        public virtual void Awake()
        {
            // idk wtf i was thinking here
            // gravityDirection = (gameObject.transform.up * gravityStrength).normalized;

            gravityDirection = gameObject.transform.up;

            Debug.Log("GravityZone: Awake");
            Debug.Log("GravityZone: gravityStrength: " + gravityStrength);
            Debug.Log("GravityZone: maxGravityStrength: " + maxGravityStrength);
            Debug.Log("GravityZone: rotatePlayer: " + rotatePlayer);
            Debug.Log("GravityZone: rotationSpeed: " + rotationSpeed);

            // gravityStrength *= 0.01f;
        }

        public virtual void Start()
        {
            if (MonkeSwimManager.Instance == null) {
                Object.Destroy(this);
                return;

            } else if (movementManager != null && movementManager == MonkeSwimManager.Instance.Movement) return;

            movementManager = MonkeSwimManager.Instance.Movement;
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (playerCollided == null && collider.name.Equals("Body Collider")) playerCollided = collider;
            else return;

            if (UseGravity) {
                movementManager.EnableGravity(true);

            } else {
                movementManager.DisableGravity(true);
            }

            UpdatedGravity();

            /*
            AddGravity();
            AddRotation();
            */
        }

        public void OnTriggerExit(Collider collider)
        {
            if (playerCollided != collider) return;


            if (UseGravity) {
                movementManager.EnableGravity(false);

            } else {
                movementManager.DisableGravity(false);
            }

            ResetSettings();

            /*
            RemoveGravity();
            RemoveRotation();    
            */
        }
        public void OnTriggerStay(Collider collider)
        {
            if (playerCollided != collider) return;
            // Debug.Log("GravityZone: OnTriggerStay");
            UpdatedGravity();
        }

        //main function for applying gravity each frame
        protected virtual void UpdatedGravity()
        {
            movementManager.AddPlayerVelocity(gravityDirection, gravityStrength, maxGravityStrength);

            if (rotatePlayer) {
                movementManager.RotatePlayer(gravityDirection, rotationSpeed);
            }
        }

        protected virtual void ResetSettings()
        {
            playerCollided = null;
        }

        /*
        protected void AddGravity()
        {
            movementManager.AddDirection(gravityDirection * gravityStrength, gravityStrength, MovementManager.DirectionType.Gravity);
        }

        protected void RemoveGravity()
        {
            movementManager.RemoveDirection(gravityDirection * gravityStrength, gravityStrength, MovementManager.DirectionType.Gravity);
        }

        protected void AddRotation()
        {
            if (!rotatePlayer) return;
            movementManager.AddDirection(gravityDirection * rotationSpeed, rotationSpeed, MovementManager.DirectionType.Rotation);
        }

        protected void RemoveRotation()
        {
            if (!rotatePlayer) return;
            movementManager.RemoveDirection(gravityDirection * rotationSpeed, rotationSpeed, MovementManager.DirectionType.Rotation);
        }
        */

#endif
    }
}