using GenshinPatchTools.Config.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GenshinPatchTools.Config
{
    public class JsonConfig : IConfigElement, ICanSaveConfigElement
    {
        /// <summary>
        /// 创造一个空的配置对象
        /// </summary>
        /// <returns>空配置对象</returns>
        public static JsonConfig CreateEmpty()
        {
            JsonConfig config = new JsonConfig();
            return config;
        }
        
        private string _path;
        private Dictionary<string, object> _config;

        private JsonConfig()
        {
            _path = "";
            _config = new Dictionary<string, object>();
        }
        
        /// <summary>
        /// 使用指定的json文件创建配置对象
        /// </summary>
        /// <param name="path"></param>
        public JsonConfig(string path)
        {
            _config = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(_path = path)) ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// 获取配置字典
        /// </summary>
        /// <returns>配置字典</returns>

        public object GetValue() => _config;

        /// <summary>
        /// 获取配置字典
        /// </summary>
        /// <typeparam name="T">目标类型，只能为Dictionary&lt;string, object&gt;</typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">T不是Dictionary&lt;string, object&gt;</exception>
        public T GetValue<T>() => typeof(T) != typeof(JObject)
            ? throw new InvalidCastException()
            : (T)(object)_config;
       

        public IConfigElement this[string key]
        {
            get => new DefaultConfigElement(_config[key]);
            set => _config[key] = value.GetValue();

        }
        
        /// <summary>
        /// 此方法对于此类无效
        /// </summary>
        /// <param name="converter">转换器</param>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>此方法不会返回值</returns>
        /// <exception cref="NotSupportedException">尝试调用此方法</exception>

        public T ConvertTo<T>(IConfigConverter<T> converter)
        {
            throw new NotSupportedException();
        }
        
       ///<inheritdoc/>
        
        public bool ContainsKey(string key)
        {
            return _config.ContainsKey(key);
        }
       
       ///<inheritdoc/>
        public void Save()
        {
            string json = JsonConvert.SerializeObject(_config);
            File.WriteAllText(_path, json);
        }
    }
}