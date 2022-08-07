namespace GenshinPatchTools.Game.Patch;

public static class PatchResultExtension
{
    public static bool IsFailed(this PatchResult result)
    {
        return (result & PatchResult.Failed) != 0;
    }
    
}