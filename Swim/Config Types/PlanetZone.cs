using UnityEngine;

namespace MonkeSwim.Config
{
    public class PlanetZone : GravityZone
    {
        [Tooltip("how close to the center of the zone to enable rotating the player")]
        [SerializeField] protected float rotationDistance;

        [Tooltip("if enabled, always rotates the player")]
        [SerializeField] protected bool alwaysRotate = true;

#if EDITOR
        public bool ShowWireSphere { get; set; }
        public bool ShowSolidSphere { get; set; }
        public Color WireSphereColour { get; set; }
        public Color SolidSphereColour { get; set; }
        public float RotationDistance { get { return rotationDistance; } private set { } }
#endif

#if GAME
        private bool rotatingPlayer; // saves the state of rotatePlayer
        protected float sqrDistance; // using distance squared for distance check is faster than magnitude

        public override void Awake()
        {
            rotatingPlayer = rotatePlayer;
            rotatePlayer = false;
            sqrDistance = rotationDistance * rotationDistance;
            base.Awake();
        }


        protected override void UpdatedGravity()
        {
            // need to remove the gravity information from the previous frame before applying new information
            RemoveGravity();
            RemoveRotation();

            // find the new gravity direction
            gravityDirection = CalculateGravity();

            // check if we should be rotating the player
            rotatePlayer = alwaysRotate || (rotatingPlayer && gravityDirection.sqrMagnitude < sqrDistance);

            // direction should be normalized
            gravityDirection = Vector3.Normalize(gravityDirection);

            AddGravity();
            AddRotation();
        }

        // override this for different gravity behaviour
        protected virtual Vector3 CalculateGravity()
        {
            return playerCollided.transform.position - transform.position;
        }
#endif
    }
}