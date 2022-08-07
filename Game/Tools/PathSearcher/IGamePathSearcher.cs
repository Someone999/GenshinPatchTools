using GenshinPatchTools.Launcher;

namespace GenshinPatchTools.Game.Tools.PathSearcher
{
    public interface IGamePathSearcher
    {
        LauncherInfo[] Search();
    }
}