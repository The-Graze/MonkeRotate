using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace MonkeSwim.Managers
{
    public class MovementManager : MonoBehaviour
    {
        private Utils.AverageDirection rotationDirection;
        private Utils.AverageDirection velocityDirection;
        private Utils.AverageDirection gravityDirection;

        private Rigidbody playerRigidBody = null;
        private GameObject playerTurnParent = null;

        private static MovementManager instance = null;

        public static MovementManager Instance { get { return instance; } private set { } }

        public enum DirectionType
        {
            Rotation,
            Velocity,
            Gravity
        }

        public void Awake()
        {
            // only one instance of this should exist, not static because this only needs to exist on supported maps
            if (instance != null && instance != this) Object.Destroy(this);
            else instance = this;

            playerTurnParent = GorillaLocomotion.Player.Instance.turnParent;
            playerRigidBody = (Rigidbody)AccessTools.Field(typeof(GorillaLocomotion.Player), "playerRigidBody").GetValue(GorillaLocomotion.Player.Instance);

            rotationDirection = Utils.AverageDirection.Zero;
            velocityDirection = Utils.AverageDirection.Zero;
            gravityDirection = Utils.AverageDirection.Zero;
        }

        // in LateUpdate() because player.Update() modifies velocity directly potentially overriding our additions
        public void LateUpdate()
        {
           if(playerRigidBody == null || playerTurnParent == null) return;

            playerRigidBody.useGravity = !(gravityDirection.Directions > 0);

            if (gravityDirection.Directions > 0) {
                Vector3 gravity = gravityDirection.Direction * gravityDirection.Speed * Time.deltaTime;
                playerRigidBody.AddForce(gravity, ForceMode.VelocityChange);    
            
            }

            if (velocityDirection.Directions > 0) {
                Vector3 velocity = velocityDirection.Direction * velocityDirection.Speed * Time.deltaTime;
                playerRigidBody.AddForce(velocity, ForceMode.Impulse);

            }

            Vector3 rotationDir = rotationDirection.Directions > 0 ? rotationDirection.Direction : Vector3.up;

            if (rotationDir != playerTurnParent.transform.up) {
                Quaternion newRotation = Quaternion.FromToRotation(playerTurnParent.transform.up, rotationDir) * playerTurnParent.transform.rotation;
                newRotation = Quaternion.RotateTowards(playerTurnParent.transform.rotation, newRotation, rotationDirection.Speed * Time.deltaTime);

                playerTurnParent.transform.rotation = newRotation;
            }
        }

        public void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public void AddDirection(Vector3 direction, float strength, DirectionType dirType)
        {
            Utils.AverageDirection newdDir = new Utils.AverageDirection(direction, strength);

            switch (dirType) {
                case DirectionType.Rotation:
                    rotationDirection += newdDir; break;

                case DirectionType.Velocity:
                    velocityDirection += newdDir; break;

                case DirectionType.Gravity:
                    gravityDirection += newdDir; break;
            }
        }

        public void RemoveDirection(Vector3 direction, float strength, DirectionType dirType)
        {
            Utils.AverageDirection newDir = new Utils.AverageDirection(direction, strength);

            switch (dirType) {
                case DirectionType.Rotation:
                    rotationDirection -= newDir; break;

                case DirectionType.Velocity:
                    velocityDirection -= newDir; break;

                case DirectionType.Gravity:
                    gravityDirection -= newDir; break;
            }
        }
    }
}
