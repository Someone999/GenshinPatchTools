namespace GenshinPatchTools.Config
{
    public static class Configs
    {
        internal static void Init()
        {
            string[] configFiles = new[] {"./Config/cfg.json"};
            foreach (var configFile in configFiles)
            {
                if (File.Exists(configFile))
                {
                    continue;
                }
                
                if (!Directory.Exists(Path.GetDirectoryName(configFile)))
                {
                    string? dirName = Path.GetDirectoryName(configFile);
                    if (dirName == null)
                    {
                        continue;
                    }
                    
                    Directory.CreateDirectory(dirName);
                }

                File.Create(configFile).Close();

            }
            LauncherConfig = new JsonConfig("./Config/cfg.json");
        }
        
        public static ICanSaveConfigElement LauncherConfig { get; private set; } = JsonConfig.CreateEmpty();
    }
}