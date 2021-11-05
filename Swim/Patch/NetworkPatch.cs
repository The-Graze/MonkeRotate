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
            set { if (!(value == null || value == playerPhotonView)) playerPhotonView = value; }
        }

        void OnEnable()
        {
            playerPhotonView?.ObservedComponents.Add(this);
        }

        void OnDisable()
        {
            playerPhotonView?.ObservedComponents.Remove(this);
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting) {
                stream.SendNext(Mathf.RoundToInt(networkedPlayer.transform.rotation.eulerAngles.x));
                stream.SendNext(Mathf.RoundToInt(networkedPlayer.transform.rotation.eulerAngles.z));

                return;
            }

            Vector3 eulerRotation = Vector3.zero;
            eulerRotation.y = networkedPlayer.syncRotation.eulerAngles.y;

            if ((System.Type)stream.PeekNext() == typeof(int)) {
                eulerRotation.x = (int)stream.ReceiveNext();
            }

            if ((System.Type)stream.PeekNext() == typeof(int)) {
                eulerRotation.z = (int)stream.ReceiveNext();
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