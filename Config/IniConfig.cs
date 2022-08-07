using System.Text;
using GenshinPatchTools.Config.Converters;

namespace GenshinPatchTools.Config
{
    /// <summary>
    /// 针对ini配置文件的<see cref="IConfigElement"/>实现
    /// </summary>
    public class IniConfig : IConfigElement, ICanSaveConfigElement
    {
        private Dictionary<string, IConfigElement> _configElements = new Dictionary<string, IConfigElement>();
        private string _path;
        public IniConfig(string path)
        {
            Parse(File.ReadAllLines(_path = path));
        }

        void Parse(string[] lines)
        {
            string? appKey = null;
            foreach (var line in lines)
            {
                //#代表注释
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                string trimmedLine = line.Trim();
                //如果是配置节
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    appKey = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    if (!_configElements.ContainsKey(appKey))
                    {
                        _configElements.Add(appKey, new DefaultConfigElement(new Dictionary<string, IConfigElement>()));
                    }
                }
                else if (trimmedLine.Contains("="))
                {
                    int firstEqualIndex = trimmedLine.IndexOf("=", StringComparison.Ordinal);
                    string propertyName = trimmedLine.Substring(0, firstEqualIndex).Trim();
                    string propertyVal = trimmedLine.Substring(firstEqualIndex + 1).Trim();
                    if (appKey == null || !(_configElements[appKey].GetValue() is Dictionary<string, IConfigElement> dict))
                    {
                        continue;
                    }
                    
                    dict.Add(propertyName, new DefaultConfigElement(propertyVal));
                }

            }
        }

        /// <summary>
        /// 以object的形式获取配置字典
        /// </summary>
        /// <returns>配置字典</returns>
        public object GetValue() => _configElements;
        
        /// <summary>
        /// 如果类型是Dictionary&lt;string, IConfigElement&gt;,以字典的形式返回配置字典,否则会引发异常
        /// </summary>
        /// <typeparam name="T">指定的类型，必须为Dictionary&lt;string, IConfigElement&gt;</typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">类型T不是Dictionary&lt;string, IConfigElement&gt;</exception>

        public T GetValue<T>() => (T)(object)(typeof(T) == typeof(Dictionary<object, IConfigElement>)
            ? _configElements
            : throw new InvalidCastException());


        /// <summary>
        /// 获取或设置配置
        /// </summary>
        /// <param name="key">键</param>
        public IConfigElement this[string key]
        {
            get => _configElements[key];
            set => _configElements[key] = value;
        }
        
        /// <summary>
        /// 此方法对于此类无效
        /// </summary>
        /// <param name="converter">转换器</param>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>这个方法不会返回值</returns>
        /// <exception cref="NotSupportedException">尝试调用这个方法</exception>

        public T ConvertTo<T>(IConfigConverter<T> converter)
        {
            throw new NotSupportedException("Method is invalid for this class.");
        }

        public bool ContainsKey(string key) => _configElements.ContainsKey(key);

        void Expend(StringBuilder builder, Dictionary<string, IConfigElement> configElements)
        {
            foreach (var element in _configElements)
            {
                if (element.Value.GetValue() is Dictionary<string, IConfigElement> dict)
                {
                    builder.AppendLine($"[{element.Key}]");
                    Expend(builder, dict);
                }
                else
                {
                    builder.AppendLine($"{element.Key} = {element.Value}");
                }
            }
        }

        public void Save()
        {
            StringBuilder builder = new StringBuilder();
            Expend(builder, _configElements);
            File.WriteAllText(_path, builder.ToString());
        }
    }
}