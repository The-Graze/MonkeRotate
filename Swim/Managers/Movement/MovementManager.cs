using System.Reflection;
using UnityEngine;
using HarmonyLib;

using MonkeSwim.Tools;

namespace MonkeSwim.Managers
{
    public class MovementManager : MonoBehaviour
    {
        private GorillaLocomotion.Player playerInstance = null;
        private FieldInfo lastLeftHandPosition = null;
        private FieldInfo lastRightHandPosition = null;
        private FieldInfo lastPlayerPosition = null;

        private Rigidbody playerRigidBody = null;
        private GameObject playerTurnParent = null;
        private GameObject player = null;
        private GameObject playerBody = null;

        private Counter enableGravityAmount = new Counter(0u);
        private Counter disableGravityAmount = new Counter(0u);
        private Counter rotatePlayerAmount = new Counter(0u);

        private bool enabledRotation = false;

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

            playerInstance = GorillaLocomotion.Player.Instance;
            player = playerInstance.gameObject;
            playerTurnParent = GorillaLocomotion.Player.Instance.turnParent;
            playerBody = GorillaLocomotion.Player.Instance.bodyCollider.gameObject;

            playerRigidBody = (Rigidbody)AccessTools.Field(typeof(GorillaLocomotion.Player), "playerRigidBody").GetValue(GorillaLocomotion.Player.Instance);
            lastLeftHandPosition = AccessTools.Field(typeof(GorillaLocomotion.Player), "lastLeftHandPosition");
            lastRightHandPosition = AccessTools.Field(typeof(GorillaLocomotion.Player), "lastRightHandPosition");
            lastPlayerPosition = AccessTools.Field(typeof(GorillaLocomotion.Player), "lastPosition");

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
            Vector3 playerUpDirection = player.transform.up;
            if (direction == playerUpDirection)
                return;

            float deltaTime = fixedDelta ? Time.fixedDeltaTime : Time.deltaTime;

            Quaternion newRotation = Quaternion.FromToRotation(playerUpDirection, direction) * player.transform.rotation;
            SetPlayerRotation(Quaternion.RotateTowards(player.transform.rotation, newRotation, rotationSpeed * deltaTime));
        }

        public void SetPlayerRotation(Quaternion rotation)
		{
            if (!enabledRotation)
                return;

            // store position before rotation
            // Vector3 lastPos = player.transform.position;
            // Vector3 lastLeftPos = playerInstance.leftHandTransform.position;
            // Vector3 lastRightPos = playerInstance.rightHandTransform.position;
            Vector3 lastHeadPos = playerInstance.headCollider.transform.position;

            // actual player position is offset from the turn parent
            // calculate the offset in local space of the player body without scale
            Vector3 parentOffset = Quaternion.Inverse(player.transform.rotation) * (player.transform.position - playerBody.transform.position);

            // apply the rotation to the offset
            parentOffset = rotation * parentOffset;

            // move the turn parents position to where its new offset is after rotating
            player.transform.position = playerBody.transform.position + parentOffset;

            // apply the new rotation
           player.transform.rotation = rotation;

            // calculate position change offsets after rotation
            // Vector3 lastPosOffset = player.transform.position - lastPos;
            // Vector3 lastLeftPosOffset = playerInstance.leftHandTransform.position - lastLeftPos;
            // Vector3 lastRightPosOffset = playerInstance.rightHandTransform.position - lastRightPos;
            Vector3 lastHeadPosOFfset = playerInstance.headCollider.transform.position - lastHeadPos;
            
            Vector3 tempVec;

            // add the offsets to the lastposition information so rotation moving the hand and body doesn't add to player velocity
            GorillaLocomotion.Player.Instance.lastHeadPosition += lastHeadPosOFfset;

            tempVec = (Vector3)lastPlayerPosition.GetValue(playerInstance);
            // lastPlayerPosition.SetValue(playerInstance, tempVec + lastPosOffset);

            // tempVec = (Vector3)lastRightHandPosition.GetValue(playerInstance);
            // lastRightHandPosition.SetValue(playerInstance, tempVec + lastRightPosOffset);

            // tempVec = (Vector3)lastLeftHandPosition.GetValue(playerInstance);
            // lastLeftHandPosition.SetValue(playerInstance, tempVec + lastLeftPosOffset);
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
            if (enter && !this.enabled) {
                UpdateGravityState();
                this.enabled = true;

                enabledRotation = playerInstance != null && lastLeftHandPosition != null && lastRightHandPosition != null && lastPlayerPosition != null ;

                return;
            }

            if (!this.enabled)
                return;

            enableGravityAmount.value = 0;
            disableGravityAmount.value = 0;
            rotatePlayerAmount.value = 0;

            // playerTurnParent.transform.rotation = Patch.RotationPatch.PlayerLockedRotation;
            SetPlayerRotation(Quaternion.FromToRotation(player.transform.up, Vector3.up) * player.transform.rotation);
            playerRigidBody.useGravity = true;
            this.enabled = false;
        }
    }
}
