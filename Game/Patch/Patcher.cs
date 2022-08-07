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
            string metadataPathFormat = Path.Combine(_patchFolder, "Genshin Impact Game{0}{1}_Data{0}Managed{0}Metadata{0}{2}");
            string userAssemblyFormat = Path.Combine(_patchFolder, "Genshin Impact Game{0}{1}_Data{0}Native{0}{2}");
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

            bool good = File.Exists(tuple.metadataPath) && File.Exists(tuple.userAssemblyPath);
            return good ? PatchResult.Ok : PatchResult.PatchFileNotFound;
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
            
                if (File.Exists(userAsmPath))
                {
                    File.Move(globalMetadataPath, globalMetadataPath + ".unpatched");
                }

                return PatchResult.Ok;
            }
            catch (Exception)
            {
                Console.WriteLine("备份文件失败");
                return PatchResult.CanNotBackup;
            }
        }

        private PatchResult ReplaceFiles()
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
            
            File.Move(tuple.globalMetadata, gameGlobalMetadataPath);
            File.Move(tuple.userAssembly, gameUserAssemblyPath);

            return PatchResult.Ok;
        }

        /// <summary>
        /// 根据文件的md5判断游戏是不是已经打了补丁了
        /// </summary>
        /// <returns>判断结果</returns>
        public PatchResult GetPatchState()
        {
            (string globalMetadataPath, string userAssemblyPath) tuple = GetPatchPath();
            
            string? gameGlobalMetadataPath = _gameInfo.GlobalMetadataPath;
            string? gameUserAssemblyPath = _gameInfo.UserAssemblyPath;
            
            if (string.IsNullOrEmpty(gameGlobalMetadataPath) || string.IsNullOrEmpty(gameUserAssemblyPath))
            {
                return PatchResult.GameFileNotFound;
            }

            byte[] gameGlobalMetadataFileBytes = File.ReadAllBytes(gameGlobalMetadataPath);
            byte[] gameUserAssemblyFileBytes = File.ReadAllBytes(gameUserAssemblyPath);
            
            byte[] patchGlobalMetadataFileBytes = File.ReadAllBytes(tuple.globalMetadataPath);
            byte[] patchUserAssemblyFileBytes = File.ReadAllBytes(tuple.userAssemblyPath);

            var md5Calculator = MD5.Create();

            string gameGlobalMetadataHash = md5Calculator.ComputeHash(gameGlobalMetadataFileBytes).GetMd5String();
            string gameUserAssemblyFileHash = md5Calculator.ComputeHash(gameUserAssemblyFileBytes).GetMd5String();
            string patchGlobalMetadataHash = md5Calculator.ComputeHash(patchGlobalMetadataFileBytes).GetMd5String();
            string patchUserAssemblyFileHash = md5Calculator.ComputeHash(patchUserAssemblyFileBytes).GetMd5String();
            bool patched = gameGlobalMetadataHash == patchGlobalMetadataHash && gameUserAssemblyFileHash == patchUserAssemblyFileHash;
            return patched ? PatchResult.HasPatched : PatchResult.NotPatched;
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

                if (GetPatchState() == PatchResult.NotPatched)
                {
                    return PatchResult.NotPatched;
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
