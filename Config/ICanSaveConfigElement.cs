namespace GenshinPatchTools.Config
{
    public interface ICanSaveConfigElement : IConfigElement
    {
        /// <summary>
        /// 保存配置
        /// </summary>
        void Save();
    }
}