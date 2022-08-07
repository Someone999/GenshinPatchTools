using GenshinPatchTools.Launcher;

namespace GenshinPatchTools.Game.Tools.PathSearcher
{
   /// <summary>
   /// 遍历Program Files目录的原神搜索器
   /// </summary>
    public class DirectoryGamePathSearcher : IGamePathSearcher
    {
        /// <summary>
        /// 通过遍历各个驱动器下的Program Files目录以寻找原神启动器路径
        /// </summary>
        /// <returns>所有搜索到的启动器的地址</returns>
        /// <exception cref="UnauthorizedAccessException">在没有权限访问文件夹时触发</exception>
        string[] GetLauncherPaths()
        {
            DriveInfo[] driveInfos = DriveInfo.GetDrives();
            string programFile = "Program Files";
            string template = "Genshin Impact Game{0}{1}";
            string chineseServerPath = string.Format(template, Path.DirectorySeparatorChar, "YuanShen_Data");
            string oceanServerPath = String.Format(template, Path.DirectorySeparatorChar, "GenshinImpact_Data");
            List<string> possibleLauncherPath = new List<string>();
            string[] genshinDirs = {chineseServerPath, oceanServerPath};
            foreach (var driveInfo in driveInfos)
            {
                string programFileFolder = Path.Combine(driveInfo.Name, programFile);
                if (!Directory.Exists(programFileFolder))
                {
                    continue;
                }
                string[] programFolders = Directory.GetDirectories(programFileFolder, "*", SearchOption.TopDirectoryOnly);
                foreach (var programFolder in programFolders)
                {

                    foreach (var dir in genshinDirs)
                    {
                        if (!Directory.Exists(Path.Combine(programFolder, dir)))
                        {
                            continue;
                        }
                        possibleLauncherPath.Add(programFolder);
                        break;
                    }


                }
                
                
            }

            return possibleLauncherPath.ToArray();
        }
        
        /// <summary>
        /// 寻找所有的启动器
        /// </summary>
        /// <returns>找到的启动器</returns>
        /// <exception cref="UnauthorizedAccessException">在没有权限访问文件夹时触发</exception>
        public LauncherInfo[] Search()
        {
            return GetLauncherPaths().Select(path => new LauncherInfo(path)).Where(info => info.GetGameInfo() != null).ToArray();
        }
    }
}