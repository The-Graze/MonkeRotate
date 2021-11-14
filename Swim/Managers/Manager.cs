using UnityEngine;

#if GAME
using System;
using UnityEngine.XR;

using MonkeSwim.Tools;
#endif

namespace MonkeSwim.Managers
{
    public class MonkeSwimManager : MonoBehaviour
    {
        [Tooltip("wether or not for global gravity to apply")]
        [SerializeField] public bool useGravity = true;

        [Tooltip("wether or not you want players rotation to reset to the world up position after leaving a rotation or gravity zone")]
        [SerializeField] public bool resetPlayerRotation = false;

        [Tooltip("the max speed the player can move at")]
        [SerializeField] public float terminalVelocity = 15f;
#if GAME
        public MovementManager Movement { get; private set; }

        // public Action UpdateCallBack;
        // public Action LateUpdateCallBack;

        public InputController RightController { get; private set; }
        public InputController LeftController { get; private set; }
        public static MonkeSwimManager Instance { get; private set; }
        
        public void Awake()
        {
            if (Instance != null && Instance != this) UnityEngine.Object.Destroy(this);
            else Instance = this;

            Movement = gameObject.AddComponent<MovementManager>();
            Movement.TerminalVelocity = terminalVelocity;
            Movement.UseGravity = useGravity;
            Movement.ResetPlayerRotation = resetPlayerRotation;

            GameObject rightHandObject = new GameObject();
            GameObject leftHandObject = new GameObject();

            rightHandObject.transform.parent = gameObject.transform;
            leftHandObject.transform.parent = gameObject.transform;

            LeftController = leftHandObject.AddComponent<InputController>();
            RightController = rightHandObject.AddComponent<InputController>();

            if (LeftController != null) LeftController.ControllerNode = XRNode.LeftHand;
            if (RightController != null) RightController.ControllerNode = XRNode.RightHand;

            VmodMonkeMapLoader.Events.OnMapEnter += MapEnterCallback;
        }

        /*
        public void Update()
        {
            // call manager updates in here
            UpdateCallBack();
            Movement.Update();
        }

        public void LateUpdate()
        {
            // anything that needs to happen after player update goes here
            LateUpdateCallBack();
            Movement.LateUpdate();
        }
        */

        public void OnDestroy()
        {
            if (Instance == this) {
                Instance = null;
                VmodMonkeMapLoader.Events.OnMapEnter -= MapEnterCallback;
            }

        }

        public void MapEnterCallback(bool enter)
        {
            if(enter) {

                // if the map rotates the player, apply the rotation patch
                foreach(Config.GravityZone rotZones in Resources.FindObjectsOfTypeAll<Config.GravityZone>()) {

                    if (rotZones.RotationIntent) {
                        foreach (Patch.NetworkRotation netRot in GameObject.FindObjectsOfType<Patch.NetworkRotation>()) {
                            netRot.enabled = true;
                        }

                        Patch.MonkeSwimPatch.ApplyPatch();
                        break;
                    }
                }

                return;
            }

            // disable all the network rotation sync classes
            foreach (Patch.NetworkRotation netRot in GameObject.FindObjectsOfType<Patch.NetworkRotation>()) {
                netRot.enabled = false;
            }

            Patch.MonkeSwimPatch.RemovePatch();
        }
#endif
    }
}