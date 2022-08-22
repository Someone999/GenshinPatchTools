namespace GenshinPatchTools.Game.Patch;

public static class PatchResultExtension
{
    /// <summary>
    /// 补丁结果是不是失败的状态
    /// </summary>
    /// <param name="result">补丁结果</param>
    /// <returns></returns>
    public static bool IsFailed(this PatchResult result)
    {
        return (result & PatchResult.Failed) != 0;
    }
    
}