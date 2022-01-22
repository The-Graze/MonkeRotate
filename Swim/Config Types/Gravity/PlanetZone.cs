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
        public bool ShowWireSphere { get; set; } = true;
        public bool ShowSolidSphere { get; set; }
        public Color WireSphereColour { get; set; } = Color.green;
        public Color SolidSphereColour { get; set; } = Color.green;
        public float RotationDistance { get { return rotationDistance; } private set { } }
#endif

#if GAME
        private bool rotatingPlayer; // saves the state of rotatePlayer
        protected float sqrDistance; // using distance squared for distance check is faster than magnitude

        public override void Awake()
        {
            base.Awake();
            rotatingPlayer = rotatePlayer;
            rotatePlayer = false;
            sqrDistance = rotationDistance * rotationDistance;
        }


        protected override void UpdatedGravity()
        {
            // find the new gravity direction
            gravityDirection = FindPlayerOffset();

            // check if we should be rotating the player
            rotatePlayer = alwaysRotate || (rotatingPlayer && gravityDirection.sqrMagnitude < sqrDistance);

            // direction should be normalized
            gravityDirection = Vector3.Normalize(gravityDirection);

            base.UpdatedGravity();
        }

        // override this for different gravity behaviour
        // player offset is used to find the gravity direction and player distance from gravity center
        protected virtual Vector3 FindPlayerOffset()
        {
            return playerCollider.gameObject.transform.position - gameObject.transform.position;
        }

        protected override void PlayerExit()
        {
            base.PlayerExit();
            rotatePlayer = false;
        }
#endif
    }
}