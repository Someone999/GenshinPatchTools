namespace GenshinPatchTools.Game.Patch;

/// <summary>
/// 补丁状态
/// </summary>
[Flags]
public enum PatchResult
{
    Ok = 0,
    HasPatched = 1,
    NotPatched = 2,
    NotRestored = 4,
    UnknownError = 8,
    IoError = 16,
    PermissionDenied = 32,
    GameFileNotFound = 64,
    BackupFileNotFound = 128,
    PatchFileNotFound = 256,
    CanNotBackup = 512,
    UnknownClientType = 1024,
    Failed = 1 | 2 | 4 | 8 | 16 | 32 | 64 | 128 | 256 | 512 | 1024
}