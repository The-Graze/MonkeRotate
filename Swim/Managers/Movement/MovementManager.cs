using System.Reflection;
using UnityEngine;
using HarmonyLib;

using MonkeSwim.Tools.Averages;
using MonkeSwim.Tools;

namespace MonkeSwim.Managers
{
    public class MovementManager : MonoBehaviour
    {
        /*
        private AverageDirection rotationDirection;
        private AverageDirection velocityDirection;
        private AverageDirection gravityDirection;
        */

        private Rigidbody playerRigidBody = null;
        private GameObject playerTurnParent = null;

        private uint enableGravityAmount = 0;
        private uint disableGravityAmount = 0;

        /*
        public enum DirectionType
        {
            Rotation,
            Velocity,
            Gravity
        }
        */

        public bool UseGravity { get; set; }
        public float TerminalVelocity { get; set; }

        public Vector3 Velocity {
            get { return playerRigidBody.velocity; }
        }

        public Vector3 Forward {
            get { return playerTurnParent.transform.forward; }
        }

        public Vector3 Right {
            get { return playerTurnParent.transform.right; }
        }

        public Vector3 Up {
            get { return playerTurnParent.transform.up; }
        }

        public Vector3 LookDirection {
            get { return Camera.main.transform.forward; }
        }

        public void Awake()
        {
            this.enabled = false;
            playerTurnParent = GorillaLocomotion.Player.Instance.turnParent;
            playerRigidBody = (Rigidbody)AccessTools.Field(typeof(GorillaLocomotion.Player), "playerRigidBody").GetValue(GorillaLocomotion.Player.Instance);

            VmodMonkeMapLoader.Events.OnMapEnter += MapLeftCallback;

            /*
            rotationDirection = AverageDirection.Zero;
            velocityDirection = AverageDirection.Zero;
            gravityDirection = AverageDirection.Zero;
            */

        }

        public void OnDestroy()
        {
            VmodMonkeMapLoader.Events.OnMapEnter -= MapLeftCallback;
        }


        // make sure we don't go passed terminal speed if other velocity modifiers are present
        public void LateUpdate()
        {
            // Debug.Log("MovementManager: TerimanlVelocity: " + TerminalVelocity);
            ClampToTerminalSpeed();
            // Debug.Log("MovmementManager: Player Velocity: " + playerRigidBody.velocity.magnitude);
        }

        /*
        public void Update()
        {
            gravity = gravityDirection.Direction * gravityDirection.Speed * Time.deltaTime;
            velocity = velocityDirection.Direction * velocityDirection.Speed * Time.deltaTime;

            Vector3 rotationDir = rotationDirection.Amount > 0 ? rotationDirection.Direction : Vector3.up;

            if (rotationDir != playerTurnParent.transform.up) {
                Quaternion newRotation = Quaternion.FromToRotation(playerTurnParent.transform.up, rotationDir) * playerTurnParent.transform.rotation;
                newRotation = Quaternion.RotateTowards(playerTurnParent.transform.rotation, newRotation, rotationDirection.Speed * Time.deltaTime);

                playerTurnParent.transform.rotation = newRotation;
            }
        }

        // in LateUpdate() because player.Update() modifies velocity directly potentially overriding our additions
        public void LateUpdate()
        {
            if (playerRigidBody == null || playerTurnParent == null) return;

            playerRigidBody.useGravity = !(gravityDirection.Amount > 0);

            if (gravityDirection.Amount > 0) {
                Vector3 gravity = gravityDirection.Direction * gravityDirection.Speed * Time.deltaTime;
                playerRigidBody.AddForce(gravity, ForceMode.VelocityChange);

            }

            if (velocityDirection.Amount > 0) {
                Vector3 velocity = velocityDirection.Direction * velocityDirection.Speed * Time.deltaTime;
                playerRigidBody.AddForce(velocity, ForceMode.Impulse);

            }
        }

        public void AddDirection(Vector3 direction, float strength, DirectionType dirType)
        {
            AverageDirection newdDir = new AverageDirection(direction, strength);

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
            AverageDirection newDir = new AverageDirection(direction, strength);

            switch (dirType) {
                case DirectionType.Rotation:
                    rotationDirection -= newDir; break;

                case DirectionType.Velocity:
                    velocityDirection -= newDir; break;

                case DirectionType.Gravity:
                    gravityDirection -= newDir; break;
            }
        }
        */

        public void RotatePlayer(Vector3 direction, float rotationSpeed)
        {
            Quaternion newRotation = Quaternion.FromToRotation(playerTurnParent.transform.up, direction) * playerTurnParent.transform.rotation;
            newRotation = Quaternion.RotateTowards(playerTurnParent.transform.rotation, newRotation, rotationSpeed * Time.deltaTime);

            playerTurnParent.transform.rotation = newRotation;
        }

        public void AddPlayerVelocity(Vector3 direction, float speed, float max)
        {
            speed *= 0.01f;

            Vector3 velocity = playerRigidBody.velocity;
            Vector3 newVelocity = direction * speed;
            Vector3 maxVelocity = direction * max;

            velocity.TryUpdateAndClampThis(newVelocity, maxVelocity);

            playerRigidBody.velocity = velocity;

            ClampToTerminalSpeed();
        }

        public void AddPlayerResistence(float resistence)
        {
            playerRigidBody.velocity = Vector3.MoveTowards(playerRigidBody.velocity, Vector3.zero, resistence * 0.001f);
        }

        private void ClampToTerminalSpeed()
        {
            Vector3 currentVelocity = playerRigidBody.velocity;

            if(currentVelocity.magnitude > TerminalVelocity) {
                currentVelocity = currentVelocity.normalized * TerminalVelocity;
                playerRigidBody.velocity = currentVelocity;
            }
        }
        public void EnableGravity(bool enable)
        {
            if (enable) {
                ++enableGravityAmount;

            } else {
                --enableGravityAmount;
            }

            UpdateGravityState();
        }

        public void DisableGravity(bool disable)
        {
            if(disable) {
                ++disableGravityAmount;

            } else {
                --disableGravityAmount;
            }

            UpdateGravityState();
        }

        private void UpdateGravityState()
        {
            if (enableGravityAmount > disableGravityAmount) {
                playerRigidBody.useGravity = true;

            } else if (enableGravityAmount < disableGravityAmount) {
                playerRigidBody.useGravity = false;

            } else {
                playerRigidBody.useGravity = UseGravity;
            }
        }

        private void MapLeftCallback(bool enter)
        {
            if (enter) {
                UpdateGravityState();
                enabled = true;
                return;
            }

            enableGravityAmount = 0;
            disableGravityAmount = 0;

            playerTurnParent.transform.rotation = Patch.RotationPatch.PlayerLockedRotation;
            playerRigidBody.useGravity = true;
            enabled = false;
        }
    }
}
