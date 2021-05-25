using UnityEngine;

namespace MonkeSwim.Config
{
    [System.Serializable]
    public class MonkeSwimConfig : MonoBehaviour
    {
        [Tooltip("if this is true the mod will be enabled accross the entire map \n" +
                 "globalsettings will be used as the base map settings")]
        public bool EntireMap = false;

        [Header("Global Settings")]
        [Tooltip("reference for settings to be used as global")]
        [SerializeReference]
        public MonkeSwimSettings GlobalSwimSettings;
    };

}