using GenshinPatchTools.Config.Converters;

namespace GenshinPatchTools.Config
{
    /// <summary>
    /// 存放配置的容器
    /// </summary>
    public interface IConfigElement
    {
        
        /// <summary>
        /// 以object的形式获取值
        /// </summary>
        /// <returns>内部存储的值</returns>
        object GetValue();
        
        /// <summary>
        /// 返回转换成类型T的值
        /// </summary>
        /// <typeparam name="T">要转换成的类型</typeparam>
        /// <returns>相应类型的对象</returns>

        T? GetValue<T>();
        IConfigElement this[string key] { get; set; }
        
        /// <summary>
        /// 使用指定类型的<see cref="IConfigConverter{T}"/>返回转换后的值
        /// </summary>
        /// <param name="converter">转换器</param>
        /// <typeparam name="T">要转换成的类型</typeparam>
        /// <returns>转换后的结果</returns>
        T ConvertTo<T>(IConfigConverter<T> converter);
        
        /// <summary>
        /// 确定配置项中是否包含指定的键
        /// </summary>
        /// <param name="key">要查询的键</param>
        /// <returns>判断结果</returns>
        
        bool ContainsKey(string key);
    }

}