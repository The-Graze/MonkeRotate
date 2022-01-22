using System.Collections;

using BepInEx;

using UnityEngine;
using Cinemachine;

namespace MonkeSwim
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("Gorilla Tag.exe")]
    public class MonkeSwim : BaseUnityPlugin
    {
        public void Awake()
        {
            Patch.RotationPatch.Init();
            Patch.CineMachinePatch.Init();
            // StartCoroutine(AddCameraTransformOverride());
            // Patch.MonkeSwimPatch.ApplyPatch();
        }

        /*
        IEnumerator AddCameraTransformOverride()
        {
            GameObject camera = null;
            GameObject turnParent = null;

            Debug.Log("waiting untill camera is found");
            while(camera == null) {
                yield return new WaitForSeconds(0.2f);
                camera = GameObject.Find("Third Person Camera/Shoulder Camera");
            }
            Debug.Log("camera found");

            Debug.Log("waiting untill player turnparent is found");
            while(turnParent == null) {
                yield return new WaitForSeconds(0.2f);
                turnParent = GameObject.Find("Player/GorillaPlayer/TurnParent");
            }
            Debug.Log("player turnparent found");

            Debug.Log("attach turn parent as world up override");
            CinemachineBrain brain = camera.GetComponent<CinemachineBrain>();
            brain.m_WorldUpOverride = turnParent.transform;
        }
        */
    }

}
