using GenshinPatchTools.Config;
using GenshinPatchTools.Launcher;

namespace GenshinPatchTools.Game
{
    public class GameInfo
    {
        /// <summary>
        /// 使用启动器路径创建一个GameInfo
        /// </summary>
        /// <param name="gameDirectory">启动器路径</param>
        public GameInfo(string gameDirectory)
        {

            #region 判断参数是否有效
            
            string chineseServer = "YuanShen_Data";
            string oceanServer = "GenshinImpact_Data";
            bool noChineseGame = !Directory.Exists(Path.Combine(gameDirectory, chineseServer));
            bool noOceanGame = !Directory.Exists(Path.Combine(gameDirectory, oceanServer));
            if (noChineseGame && noOceanGame)
            {
                ExecutableFile = null;
                ConfigFile = null;
                GlobalMetadataPath = null;
                UserAssemblyPath = null;
            }
            #endregion

            #region 尝试通过客户端类型获取游戏路径
            ClientType = noChineseGame
                ? noOceanGame
                    ? ClientType.None
                    : ClientType.Ocean
                : ClientType.Chinese;

            switch (ClientType)
            {
                case ClientType.None:
                    ExecutableFile = null;
                    ConfigFile = null;
                    GlobalMetadataPath = null;
                    UserAssemblyPath = null;
                    GamePath = null;
                    break;
                
                case ClientType.Chinese:
                    ExecutableFile = Path.Combine(gameDirectory, "YuanShen.exe");
                    GlobalMetadataPath = Path.Combine(gameDirectory, "YuanShen_Data", "Managed", "Metadata", "global-metadata.dat");
                    UserAssemblyPath = Path.Combine(gameDirectory, "YuanShen_Data", "Native", "UserAssembly.dll");
                    ConfigFile = null;
                    GamePath = Path.Combine(gameDirectory);
                    IsValid = true;
                    break;
                
                case ClientType.Ocean:
                    ExecutableFile = Path.Combine(gameDirectory, "GenshinImpact.exe");
                    GlobalMetadataPath = Path.Combine(gameDirectory, "GenshinImpact_Data", "Managed", "Metadata", "global-metadata.dat");
                    UserAssemblyPath = Path.Combine(gameDirectory, "Native", "UserAssembly.dll");
                    ConfigFile = null;
                    GamePath = Path.Combine(gameDirectory);
                    IsValid = true;
                    break;
                
                case ClientType.NotSupported:
                    ClientType = ClientType.NotSupported;
                    ExecutableFile = null;
                    ConfigFile = null;
                    GlobalMetadataPath = null;
                    UserAssemblyPath = null;
                    GamePath = null;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            string exceptedConfigFile = Path.Combine(gameDirectory, "config.ini");
            if (File.Exists(exceptedConfigFile))
            {
                Config = new IniConfig(ConfigFile!);
                HasConfig = true;
            }

            
            _gameDir = gameDirectory;
            
            
            if (File.Exists(ExecutableFile) && ClientType != ClientType.NotSupported && ClientType != ClientType.None)
            {
                return;
            }

            #endregion
            
            //如果通过搜索文件夹找到的路径不存在，则使用启动器配置的游戏路径继续搜索

            #region 尝试使用启动器的配置搜索游戏路径

            LauncherInfo launcherInfo = GetLauncherInfo();
            if (!launcherInfo.IsValid || launcherInfo.Config == null || launcherInfo.Config.ContainsKey("General"))
            {
                IsValid = false;
                ClientType = ClientType.NotSupported;
                ExecutableFile = null;
                ConfigFile = null;
                GlobalMetadataPath = null;
                UserAssemblyPath = null;
                GamePath = null;
                return;
            }

            var gamePath = launcherInfo.Config["General"]["game_install_path"].GetValue<string>();
            if (gamePath == null)
            {
                return;
            }
            GamePath = gamePath;

            switch (ClientType)
            {
                case ClientType.None:
                    ExecutableFile = null;
                    ConfigFile = null;
                    GlobalMetadataPath = null;
                    UserAssemblyPath = null;
                    break;
                
                case ClientType.Chinese:
                    ExecutableFile = Path.Combine(gamePath, "YuanShen.exe");
                    GlobalMetadataPath = Path.Combine(gamePath, "YuanShen_Data", "Managed", "Metadata", "global-metadata.dat");
                    UserAssemblyPath = Path.Combine(gamePath, "YuanShen_Data", "Native", "UserAssembly.dll");
                    ConfigFile = null;
                    IsValid = true;
                    break;
                
                case ClientType.Ocean:
                    ExecutableFile = Path.Combine(gamePath, "GenshinImpact.exe");
                    GlobalMetadataPath = Path.Combine(gamePath, "GenshinImpact_Data", "Managed", "Metadata", "global-metadata.dat");
                    UserAssemblyPath = Path.Combine(gamePath, "GenshinImpact_Data", "Native", "UserAssembly.dll");
                    ConfigFile = null;
                    IsValid = true;
                    break;
                
                case ClientType.NotSupported:
                    ClientType = ClientType.NotSupported;
                    ExecutableFile = null;
                    ConfigFile = null;
                    GlobalMetadataPath = null;
                    UserAssemblyPath = null;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            exceptedConfigFile = Path.Combine(gameDirectory, "config.ini");
            if (!File.Exists(exceptedConfigFile))
            {
                return;
            }
            Config = new IniConfig(ConfigFile!);
            HasConfig = true;
            #endregion
            
           
            
        }

        private readonly string? _gameDir;
        
        /// <summary>
        /// 是否获取到了一个有效的游戏路径
        /// </summary>
        public bool IsValid { get; }
        
        /// <summary>
        /// 游戏的客户端类型
        /// </summary>
        public ClientType ClientType { get; }
        
        /// <summary>
        /// 游戏可执行文件的全路径
        /// </summary>
        public string? ExecutableFile { get; }
        
        /// <summary>
        /// 游戏配置文件的全路径
        /// </summary>
        public string? ConfigFile { get; }
        
        /// <summary>
        /// 游戏配置
        /// </summary>
        public IConfigElement? Config { get; }
        
        /// <summary>
        /// 游戏有没有配置文件
        /// </summary>
        public bool HasConfig { get; }
        
        /// <summary>
        /// 存放游戏的文件夹的全路径
        /// </summary>
        public string? GamePath { get; }
        
        /// <summary>
        /// 游戏UserAssembly.dll的路径
        /// </summary>
        public string? UserAssemblyPath { get; }
        
        /// <summary>
        /// 游戏GlobalMetadata的路径
        /// </summary>
        public string? GlobalMetadataPath { get; }
        
        /// <summary>
        /// 通过游戏的可执行文件创建一个GameInfo
        /// </summary>
        /// <param name="gameExecutorPath">游戏的可执行文件</param>
        /// <returns>得到的GameInfo</returns>
        
        public static GameInfo GetByGameExecutable(string gameExecutorPath)
        {
            string[] split = gameExecutorPath.Split(Path.DirectorySeparatorChar);
            split = split.Take(split.Length - 1).ToArray();
            return new GameInfo(string.Join(Path.DirectorySeparatorChar.ToString(), split));
        }
        
        /// <summary>
        /// 根据启动器路径获取<see cref="LauncherInfo"/>
        /// </summary>
        /// <returns>得到的<see cref="LauncherInfo"/></returns>
        public LauncherInfo GetLauncherInfo() => new LauncherInfo(_gameDir ?? "");

    }
}