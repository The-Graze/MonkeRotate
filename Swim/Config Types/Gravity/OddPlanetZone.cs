using UnityEngine;
using UnityEditor;

#if GAME
using MonkeSwim.Tools;
#endif

namespace MonkeSwim.Config
{
    public class OddPlanetZone : PlanetZone
    {
        [Header("box constraint for where gravity center can be")]
        [SerializeField] protected Vector3 minConstraints;
        [SerializeField] protected Vector3 maxConstraints;

#if EDITOR
        public bool ShowWireCube { get; set; } = true;
        public Color WireCubeColour { get; set; } = Color.red;

        public Vector3 MinConstraints { get { return minConstraints; } private set { } }
        public Vector3 MaxConstraints { get { return maxConstraints; } private set { } }

        public Vector3 GravityPosition { get; set; }
#endif

#if GAME
        protected Quaternion inverseRotation;

        public override void Awake()
        {
            base.Awake();
            inverseRotation = Quaternion.Inverse(transform.rotation);
        }

        protected override Vector3 FindPlayerOffset()
        {
            Vector3 playerPos = inverseRotation * (playerCollided.transform.position - gameObject.transform.position);
            return playerCollided.transform.position - (gameObject.transform.rotation * playerPos.Clamp(minConstraints, maxConstraints) + gameObject.transform.position);

            // return playerCollided.transform.position - gravPoint;
        }
#endif
    }
}