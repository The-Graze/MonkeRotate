using System;
using BepInEx;
using Utilla;
using MonkeSwim.Patch;

namespace MonkeSwim
{
    [BepInPlugin("org.ahauntedarmy.plugins.monkeswim", "MonkeSwim" , "0.1.0.1")]
    [BepInProcess("Gorilla Tag.exe")]
    //[ForcePrivateLobby]
    public class MonkeSwim : BaseUnityPlugin
    {

        void Awake()
        {
            Patch.AirPatch.ApplyPatch();
        }
    }

}
