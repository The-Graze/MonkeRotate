using UnityEngine;
using UnityEditor;

namespace MonkeSwim.Config
{
    public class OddPlanetZone : PlanetZone
    {
        [Header("box constraint for where gravity center can be")]
        [SerializeField] protected Vector3 minConstraints;
        [SerializeField] protected Vector3 maxConstraints;

#if EDITOR
        public bool ShowWireCube { get; set; }
        public Color WireCubeColour { get; set; }

        public Vector3 MinConstraints { get { return minConstraints; } private set { } }
        public Vector3 MaxConstraints { get { return maxConstraints; } private set { } }

        public Vector3 GravityPosition { get; set; }
#endif

#if GAME
        protected Quaternion inverseRotation;

        public override void Awake()
        {
            inverseRotation = Quaternion.Inverse(transform.rotation);
            base.Awake();
        }

        protected override Vector3 CalculateGravity()
        {
            Vector3 playerPos = inverseRotation * (playerCollided.transform.position - transform.position);
            Vector3 gravPoint = transform.rotation * Clamp(playerPos, minConstraints, maxConstraints) + transform.position;

            return playerCollided.transform.position - gravPoint;
        }
#endif

        public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(value.x, min.x, max.x),
                               Mathf.Clamp(value.y, min.y, max.y),
                               Mathf.Clamp(value.z, min.z, max.z));
        }

    }
}