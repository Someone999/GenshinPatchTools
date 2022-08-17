using GenshinPatchTools.Config;
using GenshinPatchTools.Game;

namespace GenshinPatchTools.Launcher
{
    /// <summary>
    /// 存储启动器信息的类
    /// </summary>
    public class LauncherInfo
    {
        /// <summary>
        /// 使用启动器路径创建一个LauncherInfo
        /// </summary>
        /// <param name="launcherDirectory">启动器路径</param>
        public LauncherInfo(string launcherDirectory)
        {
            bool folderNotExists = !Directory.Exists(launcherDirectory);
            bool noLauncher = !File.Exists(Path.Combine(launcherDirectory, "launcher.exe"));
            bool noLauncherConfig = !File.Exists(Path.Combine(launcherDirectory, "config.ini"));

            if (folderNotExists || noLauncher || noLauncherConfig)
            {
                ExecutableFile = null;
                ConfigFile = null;
                Config = null;
                return;
            }

            ExecutableFile = Path.Combine(launcherDirectory, "launcher.exe");
            ConfigFile = Path.Combine(launcherDirectory, "config.ini");
            Config = new IniConfig(ConfigFile);
            IsValid = true;
        }
        /// <summary>
        /// 是否获取到了一个有效的启动器路径
        /// </summary>
        public bool IsValid { get; }
        
        /// <summary>
        /// 启动器的全路径
        /// </summary>
        public string? ExecutableFile { get; }
        
        /// <summary>
        /// 启动器配置文件的全路径
        /// </summary>
        public string? ConfigFile { get; }
        
        /// <summary>
        /// 启动器的配置
        /// </summary>
        public IConfigElement? Config { get; }
        
        /// <summary>
        /// 通过游戏的可执行文件创建一个LauncherInfo
        /// </summary>
        /// <param name="gameExecutorPath">游戏的可执行文件</param>
        /// <returns>得到的LauncherInfo</returns>
        
        public static LauncherInfo GetByGameExecutable(string gameExecutorPath)
        {
            string[] split = gameExecutorPath.Split(Path.DirectorySeparatorChar);
            split = split.Take(split.Length - 2).ToArray();
            return new LauncherInfo(string.Join(Path.DirectorySeparatorChar.ToString(), split));
        }
        
        /// <summary>
        /// 使用启动器目录创建一个<see cref="GameInfo"/>
        /// </summary>
        /// <returns>如果读取到了有效的启动器，则为GameInfo，否则为null</returns>
        public GameInfo? GetGameInfo()
        {
            if (ConfigFile != null && Config != null && Config.ContainsKey("General"))
            {
                string? gamePath0 = Config["General"]["game_install_path"].GetValue<string>();
                return string.IsNullOrEmpty(gamePath0)
                    ? null
                    : new GameInfo(gamePath0!);
            }
            
            if (ExecutableFile == null)
            {
                return null;
            }
            var path = Path.Combine(Path.GetDirectoryName(ExecutableFile) ?? ".\\", "Genshin Impact Game");
            return new GameInfo(path);

        }
    }
}