﻿using UnityEngine;

#if GAME
using MonkeSwim.Managers;
#endif

namespace MonkeRotate.Config
{
    public class GravityZone : PlayerTrigger
    {
        [Header("Gravity Settings")]
        [Tooltip("negative number pulls, positive number expels")]
        public float gravityStrength;
        public float maxGravityStrength;

        [Tooltip("enable this for the games global gravity to apply")]
        [SerializeField] public bool UseWorldGravity = false;

        [Header("Rotation Settings")]
        [Tooltip("If enabled, rotate the play away from gravity direction to be upside down")]
        [SerializeField] protected bool invertRotationDirection = false;
        [SerializeField] protected bool rotatePlayer;
        [SerializeField] protected float rotationSpeed;

        protected bool rotationIntent;

        public bool RotationIntent {
            get { return rotationIntent; }
        }

#if EDITOR
        public bool showDirection { get; set; } = true;
        public float arrowScale { get; set; } = 1f;
        public Color arrowColor { get; set; } = Color.blue;
#endif

#if GAME
        protected Vector3 gravityDirection;
        // protected Collider playerCollided = null;
        static protected MovementManager movementManager = null;

        public virtual void Awake()
        {
            gravityDirection = gameObject.transform.up;
            rotationIntent = rotatePlayer;
            gravityStrength *= 0.01f;

            if (gravityStrength > 0)
                gravityDirection *= -1f;

            /*
            Debug.Log("GravityZone: Awake");
            Debug.Log("GravityZone: gravityStrength: " + gravityStrength);
            Debug.Log("GravityZone: maxGravityStrength: " + maxGravityStrength);
            Debug.Log("GravityZone: rotatePlayer: " + rotatePlayer);
            Debug.Log("GravityZone: rotationSpeed: " + rotationSpeed);
            */
        }

        public virtual void Start()
        {
            if (MonkeSwimManager.Instance == null) {
                Object.Destroy(this);
                return;

            } else if (movementManager != null && movementManager == MonkeSwimManager.Instance.Movement) return;

            movementManager = MonkeSwimManager.Instance.Movement;
        }

        protected override void PlayerEnter()
        {
            base.PlayerEnter();

            // Debug.Log("monkeswim player entered gravity zone");

            if (UseWorldGravity) {
                movementManager.EnableGravity(true);

            } else {
                movementManager.DisableGravity(true);
            }

            if (rotationIntent) movementManager.RegisterRotationIntent(true);

            UpdatedGravity();
        }

        protected override void PlayerExit()
        {
            // Debug.Log("player left gravity zone");

            base.PlayerExit();

            if (UseWorldGravity) {
                movementManager.EnableGravity(false);

            } else {
                movementManager.DisableGravity(false);
            }

            if(rotationIntent) movementManager.RegisterRotationIntent(false);
        }

        private void OnTriggerStay(Collider collider)
        {
            if (!isPlayerCollided || playerCollider != collider) return;
            // Debug.Log("GravityZone: OnTriggerStay");
            UpdatedGravity();
        }

        //main function for applying each physics update
        protected virtual void UpdatedGravity()
        {
            if (gravityStrength != 0) {
                Vector3 gravDir = !invertRotationDirection ? gravityDirection : gravityDirection * -1f;
                movementManager.AddPlayerVelocity(gravDir, gravityStrength, maxGravityStrength);
            }

            if (rotatePlayer) {
                movementManager.RotatePlayer(gravityDirection, rotationSpeed, fixedDelta: true);
            }
        }

#endif
    }
}