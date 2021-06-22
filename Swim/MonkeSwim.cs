using System;
using BepInEx;
using Utilla;

using UnityEngine.XR;

namespace MonkeSwim
{
    [BepInPlugin("org.ahauntedarmy.plugins.monkeswim", "MonkeSwim" , "0.1.0.4")]
    [BepInProcess("Gorilla Tag.exe")]
    //[ForcePrivateLobby]
    public class MonkeSwim : BaseUnityPlugin
    {
        public void Awake()
        {
            Patch.AirPatch.ApplyPatch();
        }
    }

}
