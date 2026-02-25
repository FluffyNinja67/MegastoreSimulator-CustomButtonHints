using BepInEx.Configuration;
using UnityEngine;

namespace CustomButtonHints
{
    public class Config
    {
        public ConfigEntry<KeyCode> kRefreshInputs;
        public Config(ConfigFile cfg)
        {
            kRefreshInputs = cfg.Bind("dummy", "dummy", KeyCode.F10, new ConfigDescription("dummy", null, new ConfigurationManagerAttributes { Order = 1, IsAdvanced = true }));
        }
    }
}
