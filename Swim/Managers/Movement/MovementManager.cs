using System.Reflection;
using UnityEngine;
using HarmonyLib;

using MonkeSwim.Tools.Averages;
using MonkeSwim.Tools;

namespace MonkeSwim.Managers
{
    public class MovementManager : MonoBehaviour
    {
        private Rigidbody playerRigidBody = null;
        private GameObject playerTurnParent = null;

        private Counter enableGravityAmount = new Counter(0u);
        private Counter disableGravityAmount = new Counter(0u);
        private Counter rotatePlayerAmount = new Counter(0u);

        public bool UseGravity { get; set; }
        public bool ResetPlayerRotation { get; set; }
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

        public void RegisterRotationIntent() => ++rotatePlayerAmount;
        public void DeRegisterRotationIntent() => --rotatePlayerAmount;

        public void Awake()
        {
            this.enabled = false;
            playerTurnParent = GorillaLocomotion.Player.Instance.turnParent;
            playerRigidBody = (Rigidbody)AccessTools.Field(typeof(GorillaLocomotion.Player), "playerRigidBody").GetValue(GorillaLocomotion.Player.Instance);

            VmodMonkeMapLoader.Events.OnMapEnter += MapLeftCallback;

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

            if(ResetPlayerRotation && !rotatePlayerAmount) {
                RotatePlayer(Vector3.up, 90f);
            }
        }

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

            enableGravityAmount.value = 0;
            disableGravityAmount.value = 0;
            rotatePlayerAmount.value = 0;

            playerTurnParent.transform.rotation = Patch.RotationPatch.PlayerLockedRotation;
            playerRigidBody.useGravity = true;
            enabled = false;
        }
    }
}
