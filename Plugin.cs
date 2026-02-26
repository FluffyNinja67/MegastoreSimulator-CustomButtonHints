using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CustomButtonHints
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        public static Config _config;
        public static Harmony harmony;

        private void Awake()
        {
            harmony = new Harmony("FNK.CustomButtonHints.Megastoresimulator");
            _config = new Config(base.Config);
            Logger = base.Logger;
            harmony.PatchAll(typeof(ButtonHints));
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
        private void Update()
        {
            if(Input.GetKeyDown(_config.kRefreshInputs.Value))
            {
                ButtonHints.RefreshInputActions(null,  null);
            }
        }
    }
}
