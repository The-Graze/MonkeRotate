using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace MonkeSwim.Patch
{
    internal class NetworkRotation : MonoBehaviour, IPunObservable
    {
        private VRRig networkedPlayer;
        private PhotonView playerPhotonView;

        public VRRig NetworkedPlayer {
            get { return networkedPlayer; }
            set { if(!(value == null || value == networkedPlayer)) networkedPlayer = value; } 
        }

        public PhotonView PlayerPhotonView {
            get { return playerPhotonView; }
            set {
                if (!(value == null || value == playerPhotonView)) {
                    playerPhotonView = value;
                    playerPhotonView.ObservedComponents.Add(this);
                }
            }
        }

        void OnEnable()
        {
            Debug.Log("NetworkRotation: added rotation sync to network");

            playerPhotonView?.ObservedComponents.Add(this);
        }

        void OnDisable()
        {
            Debug.Log("NetworkRotation: removed rotation sync from network");
            playerPhotonView?.ObservedComponents.Remove(this);
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            Debug.Log(playerPhotonView.Owner.NickName);
            // Debug.Log("onphotonserializeview called");

            if (stream.IsWriting) {
                stream.SendNext(Mathf.RoundToInt(networkedPlayer.transform.rotation.eulerAngles.x));
                stream.SendNext(Mathf.RoundToInt(networkedPlayer.transform.rotation.eulerAngles.z));

                // Debug.Log("NetworkPatch: stream is writing");
                return;
            }

            // Debug.Log("stream is reading");

            Vector3 eulerRotation = Vector3.zero;
            eulerRotation.y = networkedPlayer.syncRotation.eulerAngles.y;


            object xAaxis = stream.ReceiveNext();
            object zAxis = stream.ReceiveNext();

            // Debug.Log("X Axis == " + xAaxis.GetType().ToString());
            // Debug.Log("Z Axis == " + zAxis.GetType().ToString());

            if(xAaxis.GetType() == typeof(int)) { 
                eulerRotation.x = (int)xAaxis;
            }

            if (zAxis.GetType() == typeof(int)) {
                eulerRotation.z = (int)zAxis;
            }

            networkedPlayer.syncRotation.eulerAngles = eulerRotation;
        }
    }

    [HarmonyPatch(typeof(VRRig))]
    internal class AddNetworkPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start", MethodType.Normal)]
        private static void VRRigStart(VRRig __instance)
        {
            if (__instance.isOfflineVRRig) return;
            NetworkRotation networkRotation = __instance.gameObject.AddComponent<NetworkRotation>();
            networkRotation.NetworkedPlayer = __instance;
            networkRotation.PlayerPhotonView = __instance.photonView;
        }
    }
}