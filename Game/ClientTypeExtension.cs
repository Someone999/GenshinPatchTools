namespace GenshinPatchTools.Game;

public static class ClientTypeExtension
{
    private static readonly ClientType[] ValidClientTypes = new[]
    {
        ClientType.Chinese,
        ClientType.Ocean
    };
    public static bool IsValid(this ClientType clientType)
    {
        return ValidClientTypes.Contains(clientType);
    }
}