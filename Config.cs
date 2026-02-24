using BepInEx.Configuration;

namespace CustomButtonHints
{
    public class Config
    {
        public ConfigEntry<bool> dummy;
        public Config(ConfigFile cfg)
        {
            dummy = cfg.Bind<bool>("dummy", "dummy", false, new ConfigDescription("dummy", null, new ConfigurationManagerAttributes { Order = 1, IsAdvanced = true }));
        }
    }
}
