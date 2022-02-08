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
        private GameObject playerBody = null;

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

        public void Awake()
        {
            this.enabled = false;
            playerTurnParent = GorillaLocomotion.Player.Instance.turnParent;
            playerBody = GorillaLocomotion.Player.Instance.bodyCollider.gameObject;
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
                RotatePlayer(Vector3.up, 90f, fixedDelta: false);
            }

            UpdateGravityState();
        }

        public void RotatePlayer(Vector3 direction, float rotationSpeed, bool fixedDelta)
        {
            float deltaTime = fixedDelta ? Time.fixedDeltaTime : Time.deltaTime;

            // actual player position is offset from the turn parent
            // calculate the offset in local space of the player body without scale
            Vector3 parentOffset = Quaternion.Inverse(playerTurnParent.transform.rotation) * (playerTurnParent.transform.position - playerBody.transform.position);

            // calculate the new rotation
            Quaternion newRotation = Quaternion.FromToRotation(playerTurnParent.transform.up, direction) * playerTurnParent.transform.rotation;
            newRotation = Quaternion.RotateTowards(playerTurnParent.transform.rotation, newRotation, rotationSpeed * deltaTime);

            // apply the rotation to the offset
            parentOffset = newRotation * parentOffset;

            // move the turn parents position to where its new offset is after rotating
            playerTurnParent.transform.position = playerBody.transform.position + parentOffset;

            // apply the new rotation
            playerTurnParent.transform.rotation = newRotation;
        }

        public void RegisterRotationIntent(bool intent)
        {
            if (intent) {
                ++rotatePlayerAmount;

            } else {
                --rotatePlayerAmount;
            }
        }

        public void AddPlayerVelocity(Vector3 direction, float speed, float max)
        {
            // speed *= (fixedDelta ? Time.fixedDeltaTime : Time.deltaTime);

            Vector3 velocity = playerRigidBody.velocity;
            Vector3 newVelocity = direction * speed;
            Vector3 maxVelocity = direction * max;

            velocity.TryUpdateAndClampThis(newVelocity, maxVelocity);

            playerRigidBody.velocity = velocity;

            ClampToTerminalSpeed();
        }

        public void AddPlayerResistence(float resistence, bool fixedDelta)
        {
            // playerRigidBody.velocity = Vector3.MoveTowards(playerRigidBody.velocity, Vector3.zero, resistence * 0.001f);
            playerRigidBody.velocity = Vector3.MoveTowards(playerRigidBody.velocity, Vector3.zero, resistence * (fixedDelta ? Time.fixedDeltaTime : Time.deltaTime));
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
