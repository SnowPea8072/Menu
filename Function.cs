using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.IO;

namespace Menu
{
    internal class Function
    {
        /// <summary>
        /// 格式化 JSON 字符串
        /// </summary>
        /// <param name="str">需要格式化的字符串</param>
        /// <returns>格式化的 JSON 字符串</returns>
        public static string JsonFomat(string str)
        {
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            return str;
        }

        /// <summary>
        /// 判断玩家是否为OP
        /// </summary>
        /// <param name="xuid">玩家 xuid</param>
        /// <returns>玩家是否为OP</returns>
        public static bool Admin(string xuid) => JArray.Parse(File.ReadAllText(Menu.opFile)).First(l => l.Value<string>("xuid") == xuid).Value<string>("permission") == "operator" ? true : false;

        /// <summary>
        /// 获取玩家的 uuid
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <returns>玩家 uuid</returns>
        public static string getUuid(string name) => JArray.Parse(Menu.mcapi.getOnLinePlayers()).First(l => l.Value<string>("playername") == name).Value<string>("uuid");

        /// <summary>
        /// 获取玩家的 xuid
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <returns>玩家 xuid</returns>
        public static string getXuid(string name) => JArray.Parse(Menu.mcapi.getOnLinePlayers()).First(l => l.Value<string>("playername") == name).Value<string>("xuid");
    }
}
