using System.Security.Cryptography;
using GenshinPatchTools.Tools;

namespace GenshinPatchTools.Game.Patch;

public class ScannedFileInfo
{
    public FileInfo FileInfo { get; }
    public string RelatePath { get; }
    public ScannedFileInfo(string fullPath, string absoluteDir)
    {
        FileInfo = new FileInfo(fullPath);
        RelatePath = fullPath.Replace(absoluteDir, ".");
    }
}

/// <summary>
/// 文件扫描器
/// </summary>
public class FileScanner
{
    static MD5 _md5 = MD5.Create();
    public ScannedFileInfo[] Files { get; private set; } = Array.Empty<ScannedFileInfo>();
    private string? _absoluteDirectory;

    /// <summary>
    /// 扫描文件，结果存储在<see cref="Files"/>中。每次扫描都会覆盖
    /// </summary>
    /// <param name="path">路径</param>
    public void Scan(string path)
    {
        _absoluteDirectory = path;
        if (_absoluteDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            _absoluteDirectory = _absoluteDirectory.Substring(0, _absoluteDirectory.Length - 1);
        }
        
        Files = (from f in 
            Directory.GetFiles(path, "*", SearchOption.AllDirectories) 
            select new ScannedFileInfo(f, _absoluteDirectory)).ToArray();
    }

    /// <summary>
    /// 比较两个目录下的文件是不是完全一致
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>判断结果</returns>
    public bool CompareWithPath(string path)
    {
        if (Files.Length == 0 || string.IsNullOrEmpty(_absoluteDirectory))
        {
            return false;
        }

        foreach (var fileInfo in Files)
        {
            string fileRelatePath = fileInfo.FileInfo.FullName.Replace(_absoluteDirectory!, ".");
            string targetFilePath = Path.Combine(path, fileRelatePath);
            FileInfo targetFileInfo = new FileInfo(targetFilePath);
            if (!fileInfo.FileInfo.Exists || !targetFileInfo.Exists)
            {
                return false;
            }

            string currentFileHash = _md5.ComputeHash(File.ReadAllBytes(fileInfo.FileInfo.FullName)).GetMd5String();
            string targetFileHash = _md5.ComputeHash(File.ReadAllBytes(targetFileInfo.FullName)).GetMd5String();

            if (currentFileHash != targetFileHash)
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// 将扫描到的文件复制到另一个目录
    /// </summary>
    /// <param name="folderPath">目标目录</param>

    public void CopyFilesTo(string folderPath)
    {
        if (Files.Length == 0 || string.IsNullOrEmpty(_absoluteDirectory))
        {
            return;
        }
        
        foreach (var fileInfo in Files)
        {
            string relatePath = fileInfo.FileInfo.FullName.Replace(_absoluteDirectory!, ".");
            string fullTargetPath = Path.Combine(folderPath, relatePath);
            fileInfo.FileInfo.CopyTo(fullTargetPath);
        }
    }
}