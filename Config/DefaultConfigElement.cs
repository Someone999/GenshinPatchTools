using GenshinPatchTools.Config.Converters;

namespace GenshinPatchTools.Config
{
    /// <summary>
    /// <see cref="IConfigElement"/>的默认实现
    /// </summary>
    public class DefaultConfigElement : IConfigElement
    {
        private object _innerVal;

        public DefaultConfigElement(object val)
        {
            _innerVal = val;
        }

        /// <inheritdoc/>
        public object GetValue() => _innerVal;

        /// <summary>
        /// 使用Convert.ChangeType将内部存储的转换成所需的类型后，返回相应类型的对象
        /// </summary>
        /// <typeparam name="T">要转换成的类型</typeparam>
        /// <returns>相应类型的对象</returns>
        /// <exception cref="InvalidCastException">内部存储的对象无法转换成类型T</exception>

        public T? GetValue<T>()
        {
            if (typeof(T) == _innerVal.GetType())
            {
                return (T)_innerVal;
            }

            if (typeof(IConvertible).IsAssignableFrom(typeof(T)))
            {
                return (T)Convert.ChangeType(_innerVal, typeof(T));
            }

            return default;
            
        }
        
        
        /// <summary>
        /// 当内部存储对象的类型为Dictionary&lt;string, IConfigElement&gt;时获取或者设置
        /// </summary>
        /// <param name="key">要操作的键</param>
        /// <returns>如果内部对象的类型是Dictionary&lt;string, IConfigElement&gt;，返回或设置相应的值，否则get返回null，set不操作</returns>
        /// <exception cref="KeyNotFoundException">要查找的键不存在</exception>

        public IConfigElement this[string key]
        {
            get
            {
                if (_innerVal is Dictionary<string, IConfigElement> dict)
                {
                    return !dict.ContainsKey(key)
                        ? new DefaultConfigElement("")
                        : dict[key];
                }
                return new DefaultConfigElement("");
            }

            set
            {
                if (!(_innerVal is Dictionary<string, IConfigElement> dict))
                {
                    return;
                }
               
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, value);
                    return;
                }
                dict[key] = value;
            }
        }
        
        
        /// <inheritdoc/>
        
        public T ConvertTo<T>(IConfigConverter<T> converter)
        {
            return converter.Convert(_innerVal);
        }
        
        /// <summary>
        /// 确定配置项中是否包含指定的键
        /// </summary>
        /// <param name="key">要查询的键</param>
        /// <returns>如果内部对象的类型是字典，则返回判断结果，否则返回false</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool ContainsKey(string key)
        {
            if (_innerVal is Dictionary<string, IConfigElement> dict)
            {
                return dict.ContainsKey(key);
            }
            
            return false;
        }
    }
}