using System.Text;

namespace GenshinPatchTools.Tools
{
    public static class Md5Tools
    {
        /// <summary>
        /// 将md5从字节形式转换成字符串形式
        /// </summary>
        /// <param name="bts">md5字节</param>
        /// <returns>转换结果</returns>
        public static string GetMd5String(this byte[] bts)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var bt in bts)
            {
                builder.Append($"{bt:x2}");
            }
            return builder.ToString();
        }
    }
}