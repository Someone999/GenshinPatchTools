using System.Text;
using GenshinPatchTools.Launcher;
using Microsoft.Win32;

namespace GenshinPatchTools.Game.Tools.PathSearcher
{
    
    /// <summary>
    /// 搜索注册表的原神目录搜索器
    /// </summary>
    public class RegistryGamePathSearcher : IGamePathSearcher
    {
        /// <summary>
        /// 在注册表中搜索启动器地址
        /// </summary>
        /// <returns>启动器的地址</returns>
        public static string GetLauncherPath()
        {
            RegistryKey key = Registry.LocalMachine;//打开指定注册表根
            //获取官方启动器路径
            string? launcherPath = "";
            try
            {
                var regKeyOcean = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Genshin Impact");

                launcherPath = regKeyOcean?.GetValue("InstallPath")?.ToString();

            }
            catch (Exception)
            {
                try
                {
                    var regKeyChinese = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\原神");

                    launcherPath = regKeyChinese?.GetValue("InstallPath")?.ToString();
                }
                catch (Exception)
                {
                    return "";
                }


            }

            launcherPath = launcherPath ?? "";
            byte[] bytePath = Encoding.UTF8.GetBytes(launcherPath);//编码转换
            string path = Encoding.UTF8.GetString(bytePath);
            return path;
        }
        
        
        public LauncherInfo[] Search()
        {
            string path = GetLauncherPath();
            return Directory.Exists(path)
                ? new[] {new LauncherInfo(path)}
                : Array.Empty<LauncherInfo>();
        }
    }
}