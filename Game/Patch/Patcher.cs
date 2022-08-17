using System.Security.Cryptography;
using GenshinPatchTools.Tools;

namespace GenshinPatchTools.Game.Patch
{
    /// <summary>
    /// 为游戏打补丁的工具
    /// </summary>
    public class Patcher
    {
        private GameInfo _gameInfo;
        private string _patchFolder;
        
        /// <summary>
        /// 使用<see cref="GameInfo"/>和补丁文件的根目录创建一个Patcher
        /// </summary>
        /// <param name="gameInfo">游戏信息</param>
        /// <param name="patchFolder">补丁文件根目录</param>
        public Patcher(GameInfo gameInfo, string patchFolder = "./")
        {
            _gameInfo = gameInfo;
            _patchFolder = patchFolder;
        }
        
        /// <summary>
        /// 获取补丁文件的路径
        /// </summary>
        /// <returns>一个元组，第一个元素是global-metadata.dat的路径，第二个元素是UserAssembly.dll的路径</returns>
        public (string, string) GetPatchPath()
        {
            string metadataPathFormat = Path.Combine(_patchFolder, "{1}_Data{0}Managed{0}Metadata{0}{2}");
            string userAssemblyFormat = Path.Combine(_patchFolder, "{1}_Data{0}Native{0}{2}");
            string? clientStr = _gameInfo.ClientType switch
            {
                ClientType.Chinese => "YuanShen",
                ClientType.Ocean => "GenshinImpact",
                _ => null
            };
            
            string? userAsmFileName = "UserAssembly.dll", metadataFileName = "global-metadata.dat";
            string metadataPath = string.Format(metadataPathFormat, Path.DirectorySeparatorChar, clientStr, metadataFileName);
            string userAssemblyPath = string.Format(userAssemblyFormat, Path.DirectorySeparatorChar, clientStr, userAsmFileName);
            return (metadataPath, userAssemblyPath);
        }
        
        /// <summary>
        /// 补丁文件是否存在
        /// </summary>
        /// <returns>存在为true，否则为false</returns>
        public PatchResult CheckPatchFiles()
        {
            if (_gameInfo.ClientType != ClientType.Ocean && _gameInfo.ClientType != ClientType.Chinese)
            {
                return PatchResult.UnknownClientType;
            }
            (string metadataPath, string userAssemblyPath) tuple = GetPatchPath();

            try
            {
                bool good = File.Exists(tuple.metadataPath) || File.Exists(tuple.userAssemblyPath);
                return good ? PatchResult.Ok : PatchResult.PatchFileNotFound;
            }
            catch (UnauthorizedAccessException)
            {
                return PatchResult.PermissionDenied;
            }
            catch (IOException)
            {
                return PatchResult.IoError;
            }
            catch (Exception)
            {
                return PatchResult.CanNotBackup;
            }
        }
        
        /// <summary>
        /// 备份文件
        /// </summary>
        /// <returns>备份成功为true，否则为false</returns>

        public PatchResult BackupFiles()
        {
            string? userAsmPath = _gameInfo.UserAssemblyPath;
            string? globalMetadataPath = _gameInfo.GlobalMetadataPath;
            if (string.IsNullOrEmpty(userAsmPath) || string.IsNullOrEmpty(globalMetadataPath))
            {
                return PatchResult.GameFileNotFound;
            }

            try
            {
                if (File.Exists(userAsmPath))
                {
                    File.Move(userAsmPath, userAsmPath + ".unpatched");
                }

                if (File.Exists(globalMetadataPath))
                {
                    File.Move(globalMetadataPath, globalMetadataPath + ".unpatched");
                }

                return PatchResult.Ok;
            }
            catch (UnauthorizedAccessException)
            {
                return PatchResult.PermissionDenied;
            }
            catch (IOException)
            {
                return PatchResult.IoError;
            }
            catch (Exception)
            {
                return PatchResult.CanNotBackup;
            }
        }

        private PatchResult ReplaceFiles()
        {
            try
            {
                var patchFileCheckResult = CheckPatchFiles();
                if (patchFileCheckResult.IsFailed())
                {
                    return patchFileCheckResult;
                }
                (string globalMetadata, string userAssembly) tuple = GetPatchPath();
                string? gameGlobalMetadataPath = _gameInfo.GlobalMetadataPath;
                string? gameUserAssemblyPath = _gameInfo.UserAssemblyPath;
                if (string.IsNullOrEmpty(gameGlobalMetadataPath) || string.IsNullOrEmpty(gameUserAssemblyPath))
                {
                    return PatchResult.GameFileNotFound;
                }

                if (BackupFiles() == PatchResult.CanNotBackup)
                {
                    return PatchResult.CanNotBackup;
                }

                if (File.Exists(tuple.globalMetadata))
                {
                    File.Copy(tuple.globalMetadata, gameGlobalMetadataPath);
                }

                if (File.Exists(tuple.userAssembly))
                {
                    File.Copy(tuple.userAssembly, gameUserAssemblyPath);
                }
                
                return PatchResult.Ok;
            }
            catch (UnauthorizedAccessException)
            {
                return PatchResult.PermissionDenied;
            }
            catch (IOException)
            {
                return PatchResult.IoError;
            }
            catch (Exception)
            {
                return PatchResult.UnknownError;
            }
        }

        /// <summary>
        /// 根据文件的md5判断游戏是不是已经打了补丁了
        /// </summary>
        /// <returns>判断结果</returns>
        public PatchResult GetPatchState()
        {
            try
            {
                (string globalMetadataPath, string userAssemblyPath) tuple = GetPatchPath();
            
                string? gameGlobalMetadataPath = _gameInfo.GlobalMetadataPath;
                string? gameUserAssemblyPath = _gameInfo.UserAssemblyPath;
            
                if (string.IsNullOrEmpty(gameGlobalMetadataPath) || string.IsNullOrEmpty(gameUserAssemblyPath))
                {
                    return PatchResult.GameFileNotFound;
                }

                bool metadataMatched = false, userAsmMatched = false;
                bool metadataExists = false, userAsmExists = false;
                var md5Calculator = MD5.Create();
                if (File.Exists(tuple.globalMetadataPath))
                {
                    metadataExists = true;
                    byte[] gameGlobalMetadataFileBytes = File.ReadAllBytes(gameGlobalMetadataPath);
                    byte[] patchGlobalMetadataFileBytes = File.ReadAllBytes(tuple.globalMetadataPath);
                    string gameGlobalMetadataHash = md5Calculator.ComputeHash(gameGlobalMetadataFileBytes).GetMd5String();
                    string patchGlobalMetadataHash = md5Calculator.ComputeHash(patchGlobalMetadataFileBytes).GetMd5String();
                    metadataMatched = gameGlobalMetadataHash == patchGlobalMetadataHash;
                }
                
                if (File.Exists(tuple.userAssemblyPath))
                {
                    userAsmExists = true;
                    byte[] gameUserAssemblyFileBytes = File.ReadAllBytes(gameUserAssemblyPath);
                    byte[] patchUserAssemblyFileBytes = File.ReadAllBytes(tuple.userAssemblyPath);
                    string gameUserAssemblyFileHash = md5Calculator.ComputeHash(gameUserAssemblyFileBytes).GetMd5String();
                    string patchUserAssemblyFileHash = md5Calculator.ComputeHash(patchUserAssemblyFileBytes).GetMd5String();
                    userAsmMatched = gameUserAssemblyFileHash == patchUserAssemblyFileHash;

                }

                metadataMatched = !metadataExists || metadataMatched;
                userAsmMatched = !userAsmExists || userAsmMatched;
                bool patched = metadataMatched && userAsmMatched;
                return patched ? PatchResult.HasPatched : PatchResult.NotPatched;
            }
            catch (UnauthorizedAccessException)
            {
                return PatchResult.PermissionDenied;
            }
            catch (IOException)
            {
                return PatchResult.IoError;
            }
            catch (Exception)
            {
                return PatchResult.UnknownError;
            }
            
        }

        /// <summary>
        /// 给游戏打补丁
        /// </summary>
        /// <returns>成功为true，否则为false</returns>
        public PatchResult Patch()
        {
            try
            {
                return GetPatchState() == PatchResult.NotPatched
                    ? ReplaceFiles()
                    : PatchResult.HasPatched;
            }
            catch (IOException)
            {
                return PatchResult.IoError;
            }
            catch (UnauthorizedAccessException)
            {
                
                return PatchResult.PermissionDenied;
            }
            catch (Exception)
            {
                return PatchResult.UnknownError;
            }
            
        }

        public PatchResult UnPatch()
        {
            try
            {
                string? gameGlobalMetadataPath = _gameInfo.GlobalMetadataPath;
                string? gameUserAssemblyPath = _gameInfo.UserAssemblyPath;
            
                if (string.IsNullOrEmpty(gameGlobalMetadataPath) || string.IsNullOrEmpty(gameUserAssemblyPath))
                {
                    return PatchResult.GameFileNotFound;
                }

                if (!File.Exists(gameGlobalMetadataPath + ".unpatched") || !File.Exists(gameUserAssemblyPath + ".unpatched"))
                {
                    return PatchResult.BackupFileNotFound;
                }

                if (GetPatchState().IsFailed())
                {
                    return GetPatchState();
                }
            
                File.Move(gameGlobalMetadataPath, gameGlobalMetadataPath + ".patched");
                File.Move(gameGlobalMetadataPath + ".unpatched", gameGlobalMetadataPath);
                File.Move(gameUserAssemblyPath, gameUserAssemblyPath + ".patched");
                File.Move(gameUserAssemblyPath + ".unpatched", gameUserAssemblyPath);
            

                return GetPatchState() == PatchResult.NotPatched ? PatchResult.Ok : PatchResult.NotRestored;
            }
            catch (IOException)
            {
                return PatchResult.IoError;
            }
            catch (UnauthorizedAccessException)
            {
                return PatchResult.PermissionDenied;
            }
            catch (Exception)
            {
                return PatchResult.UnknownError;
            }
        }
        
    }
}
