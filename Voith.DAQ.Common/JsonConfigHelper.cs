using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Voith.DAQ.Common
{
    public class JsonConfigHelper
    {
        private JObject _jObject;
        private readonly string _path;


        /// <summary>
        /// 根据键获取Json键值对字符串的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                string str = "";
                if (_jObject != null)
                {
                    str = _jObject[key]?.ToString();
                }
                return str;
            }
            set
            {
                if (_jObject != null)
                {
                    if (_jObject[key] == null)
                    {
                        _jObject.Add(key,value);
                    }
                    else
                    {
                        _jObject[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// 实例化Json帮助类，传入Json文件路径
        /// </summary>
        /// <param name="path"></param>
        public JsonConfigHelper(string path)
        {
            _jObject = new JObject();
            _path = path;
            using (StreamReader file = new StreamReader(path,Encoding.Default))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    _jObject = JObject.Load(reader);
                }
            }
        }

        /// <summary>
        /// 根据键读取Json对应值并转换为指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetValue<T>(string key) where T : class
        {
            return JsonConvert.DeserializeObject<T>(_jObject.SelectToken(key).ToString());
        }

        /// <summary>
        /// 根据键读取Json对应值的字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            return Regex.Replace((_jObject.SelectToken(key).ToString()), @"\s", "");
        }

        /// <summary>
        /// 如果对Json文件作修改，需要调用此方法保存
        /// </summary>
        public void Save()
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(_jObject),Encoding.Default);
        }
    }
}
