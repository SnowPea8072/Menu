using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Essentials.Function;
// using MenuWindowsFormsApp;
using CSR;

namespace Menu
{
    internal class Menu
    {
        /// <summary>
        /// MC相关API方法
        /// </summary>
        public static MCCSAPI mcapi = null;
        private static JObject menu = null;
        private static JObject config = null;
        private static JObject Mdefault = null;
        private static readonly Dictionary<string, bool> player = new Dictionary<string, bool>();
        private static uint MenuX = 0;
        private static uint Modal = 0;
        private const int timeout = 5 * 1000;
        private static string NAME = null;
        public const string Version = "1.0.1";
        public const string Author = "SnowPea8072";
        public const string PluginName = "Menu";
        private const string BDSName = "plugins";
        private const string FolderName = "plugins/Menu";
        private const string MenuName = "plugins/Menu/Menu";
        public const string opFile = "permissions.json";
        private const string configFile = "plugins//Menu//config.json";
        private const string menuFile = "plugins//Menu//Menu//{0}.json";
        private const string defaultFile = "plugins//Menu//Menu//default.json";

        #region 初始化文件

        /// <summary>
        /// 初始化文件
        /// </summary>
        private static void initPlugin()
        {
            if (!Directory.Exists(BDSName))
                Directory.CreateDirectory(BDSName);
            if (!Directory.Exists(FolderName))
                Directory.CreateDirectory(FolderName);
            if (!Directory.Exists(MenuName))
                Directory.CreateDirectory(MenuName);
            if (!File.Exists(configFile))
                initConfig();
            if (!File.Exists(defaultFile))
                initDefault();
            try
            {
                config = JObject.Parse(File.ReadAllText(configFile));
                mcapi.setCommandDescribe("menu", "打开主菜单");
                foreach (JObject menu in config["menu"])
                    mcapi.setCommandDescribeEx((string)menu["command"], (string)menu["description"], (bool)config["admin"] ? MCCSAPI.CommandPermissionLevel.Admin : MCCSAPI.CommandPermissionLevel.Any, (byte)MCCSAPI.CommandCheatFlag.NotCheat, (byte)MCCSAPI.CommandVisibilityFlag.Visible);
            }
            catch
            {
                Console.WriteLine("Menu >> 配置文件 config.json 读取失败！");
                Console.WriteLine("Menu >> 为了服务器的安全，已卸载本插件的所有功能，请重启服务器确保插件正常运行！");
                mcapi.removeAfterActListener(EventKey.onUseItem, UseItem);
                mcapi.removeAfterActListener(EventKey.onLoadName, PlayerJoin);
                mcapi.removeAfterActListener(EventKey.onPlayerLeft, PlayerLeft);
                mcapi.removeAfterActListener(EventKey.onFormSelect, FormSelect);
                mcapi.removeBeforeActListener(EventKey.onServerCmd, ServerCmd);
                mcapi.removeBeforeActListener(EventKey.onInputCommand, InputCommand);
                return;
            }
            try { Mdefault = JObject.Parse(File.ReadAllText(defaultFile)); }
            catch { Console.WriteLine("Menu >> 菜单文件 default.json 读取失败！"); }
        }

        /// <summary>
        /// 初始化 config.json 文件
        /// </summary>
        private static void initConfig()
        {
            var config = new JObject(
                new JProperty("version", Version),
                new JProperty("open", 347),
                new JProperty("menu", new JArray())
            );
            File.WriteAllText(configFile, Function.JsonFomat(config.ToString()), Encoding.UTF8);
        }

        /// <summary>
        /// 初始化 default.json 文件
        /// </summary>
        private static void initDefault()
        {
            var defaultX = new JObject(
                new JProperty("type", "form"),
                new JProperty("title", "主菜单"),
                new JProperty("content", "按钮如下："),
                new JProperty("buttons", new JArray(
                    new JObject(
                        new JProperty("imageX", false),
                        new JProperty("moneyX", false),
                        new JProperty("text", "你好"),
                        new JProperty("cmd", new JArray(
                            new JObject(
                                new JProperty("command", "me HelloWorld！"),
                                new JProperty("type", "default")
                            )
                        ))
                    ),
                    new JObject(
                        new JProperty("imageX", false),
                        new JProperty("moneyX", false),
                        new JProperty("text", "生存"),
                        new JProperty("cmd", new JArray(
                            new JObject(
                                new JProperty("command", "gamemode 0"),
                                new JProperty("type", "temporary")
                            )
                        ))
                    ),
                    new JObject(
                        new JProperty("imageX", false),
                        new JProperty("moneyX", false),
                        new JProperty("text", "给所有人说悄悄话"),
                        new JProperty("cmd", new JArray(
                            new JObject(
                                new JProperty("command", "tell @a qwq"),
                                new JProperty("type", "operator")
                            ),
                            new JObject(
                                new JProperty("command", "tellraw @s {\"rawtext\":[{\"text\":\"只有OP才可以执行哟！\"}]}"),
                                new JProperty("type", "cmd")
                            )
                        ))
                    ),
                    new JObject(
                        new JProperty("imageX", true),
                        new JProperty("moneyX", false),
                        new JProperty("image", "textures/items/apple"),
                        new JProperty("text", "苹果"),
                        new JProperty("cmd", new JArray(
                            new JObject(
                                new JProperty("command", "give @s apple 1"),
                                new JProperty("type", "cmd")
                            ),
                            new JObject(
                                new JProperty("command", "tellraw @s {\"rawtext\":[{\"text\":\"你还是太嫩了！\"}]}"),
                                new JProperty("type", "cmd")
                            )
                        )),
                        new JProperty("money", 0)
                    )
                ))
            );
            File.WriteAllText(defaultFile, Function.JsonFomat(defaultX.ToString()), Encoding.UTF8);
        }

        #endregion

        #region 菜单功能实现

        /// <summary>
        /// 给玩家发送表单
        /// </summary>
        /// <param name="name">玩家名称</param>
        public static void Menu_Open(string name)
        {
            string uuid = Function.getUuid(name);
            if ((string)menu["type"] == "form")
            {
                var buttons = new JArray();
                foreach (JObject menuX in menu["buttons"])
                {
                    var button = new JObject();
                    if ((bool)menuX["imageX"])
                    {
                        button.Add("image", new JObject(
                            new JProperty("type", "path"),
                            new JProperty("data", menuX["image"])
                        ));
                    }
                    button.Add("text", menuX["text"]);
                    buttons.Add(button);
                }
                var menu_ = new JObject(
                    new JProperty("type", menu["type"]),
                    new JProperty("title", menu["title"]),
                    new JProperty("content", menu["content"]),
                    new JProperty("buttons", buttons)
                );
                MenuX = mcapi.sendCustomForm(uuid, JsonConvert.SerializeObject(menu_));
            }
            else if ((string)menu["type"] == "modal")
                Modal = mcapi.sendModalForm(uuid, (string)menu["title"], (string)menu["content"], (string)menu["events"]["button1"]["text"], (string)menu["events"]["button2"]["text"]);
        }

        /// <summary>
        /// 玩家选择按钮后执行的事件
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <param name="menuX">按钮相关信息</param>
        private static void Menu_Do(string name, JObject menuX)
        {
            string uuid = Function.getUuid(name),
                command = (string)menuX["command"], path = string.Format(menuFile, command), type = (string)menuX["type"];
            bool isX = Function.Admin(Function.getXuid(name)),
                defaultX = false, temporary = false, op = false,
                cmd = false, MENU = false, AMenu = false;
            if (type == "default")
                defaultX = true;
            if (type == "temporary")
            {
                temporary = true;
                if (!isX)
                    mcapi.runcmd($"op \"{name}\"");
            }
            if (type == "operator")
            {
                if (!isX)
                {
                    uint failed = mcapi.sendModalForm(uuid, "菜单管理", "你没有权限执行该命令! ", "确定", "取消");
                    Thread.Sleep(timeout);
                    mcapi.releaseForm(failed);
                    return;
                }
                op = true;
            }
            if (type == "cmd")
            {
                cmd = true;
                command = command.Replace("@s", $"\"{name}\"").Replace("@p", $"\"{name}\"");
            }
            if (type == "menu")
            {
                if (File.Exists(path))
                {
                    try { menu = JObject.Parse(File.ReadAllText(path)); }
                    catch
                    {
                        Console.WriteLine("Menu >> 配置文件 {0}.json 读取失败！", command);
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Menu >> 未找到配置文件 {0}.json，请自行生成！", command);
                    return;
                }
                MENU = true;
            }
            if (type == "AMenu")
            {
                if (!isX)
                {
                    uint failed = mcapi.sendModalForm(uuid, "菜单管理", "你没有权限打开该菜单! ", "确定", "取消");
                    Thread.Sleep(timeout);
                    mcapi.releaseForm(failed);
                    return;
                }
                if (File.Exists(path))
                {
                    try { menu = JObject.Parse(File.ReadAllText(path)); }
                    catch
                    {
                        Console.WriteLine("Menu >> 配置文件 {0}.json 读取失败！", command);
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Menu >> 未找到配置文件 {0}.json，请自行生成！", command);
                    return;
                }
                AMenu = true;
            }
            if ((defaultX || temporary || op) && !cmd && !MENU && !AMenu)
            {
                Thread.Sleep(200);
                mcapi.runcmdAs(uuid, $"/{command}");
                if (temporary && !isX)
                {
                    Thread.Sleep(500);
                    mcapi.runcmd($"deop \"{name}\"");
                }
            }
            else if (cmd)
                mcapi.runcmd(command);
            else if (MENU || AMenu)
                Menu_Open(name);
        }

        #endregion

        #region 点地菜单操作

        /// <summary>
        /// 设置玩家加入游戏监听
        /// </summary>
        private static bool PlayerJoin(Events x)
        {
            var json = BaseEvent.getFrom(x) as LoadNameEvent;
            player.Add(json.playername, true);
            return true;
        }

        /// <summary>
        /// 设置玩家退出游戏监听
        /// </summary>
        private static bool PlayerLeft(Events x)
        {
            var json = BaseEvent.getFrom(x) as PlayerLeftEvent;
            player.Remove(json.playername);
            return true;
        }

        /// <summary>
        /// 设置玩家使用物品监听
        /// </summary>
        private static bool UseItem(Events x)
        {
            var json = BaseEvent.getFrom(x) as UseItemEvent;
            if (json.itemid == (int)config["open"] && (bool)player[json.playername])
            {
                player[json.playername] = false;
                menu = Mdefault;
                Menu_Open(json.playername);
                Thread.Sleep(300);
                player[json.playername] = true;
                return false;
            }
            return true;
        }

        #endregion

        #region 后台命令

        /// <summary>
        /// 后台指令监听
        /// </summary>
        private static bool ServerCmd(Events x)
        {
            var json = BaseEvent.getFrom(x) as ServerCmdEvent;
            switch (json.cmd)
            {
                case "menu reload":
                    if (File.Exists(configFile))
                    {
                        try
                        {
                            config = JObject.Parse(File.ReadAllText(configFile));
                            Console.WriteLine("Menu >> 配置文件 config.json 读取成功！");
                        }
                        catch { Console.WriteLine("Menu >> 配置文件 config.json 读取失败！"); }
                    }
                    else
                    {
                        initConfig();
                        Console.WriteLine("Menu >> 未找到配置文件 config.json，正在为您生成！");
                    }
                    if (File.Exists(defaultFile))
                    {
                        try
                        {
                            Mdefault = JObject.Parse(File.ReadAllText(defaultFile));
                            Console.WriteLine("Menu >> 菜单文件 default.json 读取成功！");
                        }
                        catch { Console.WriteLine("Menu >> 菜单文件 default.json 读取失败！"); }
                    }
                    else
                    {
                        initDefault();
                        Console.WriteLine("Menu >> 未找到菜单文件 default.json，正在为您生成！");
                    }
                    return false;
                /* case "menu":
                    Program.Open();
                    return false; */
                default:
                    break;
            }
            return true;
        }

        #endregion

        #region 玩家命令

        /// <summary>
        /// 设置玩家命令监听
        /// </summary>
        private static bool InputCommand(Events x)
        {
            var json = BaseEvent.getFrom(x) as InputCommandEvent;
            if (json.cmd == "/menu")
            {
                menu = Mdefault;
                Menu_Open(json.playername);
                return false;
            }
            else if (json.cmd != null)
                foreach (JObject configX in config["menu"])
                {
                    string path = string.Format(menuFile, configX["name"]);
                    string command = string.Format("/{0}", configX["command"]);
                    if (json.cmd == command)
                    {
                        if (File.Exists(path))
                        {
                            try
                            {
                                menu = JObject.Parse(File.ReadAllText(path));
                                Menu_Open(json.playername);
                                return false;
                            }
                            catch { Console.WriteLine("Menu >> 菜单文件 {0}.json 读取失败！", configX["name"]); }
                        }
                        else
                            Console.WriteLine("Menu >> 未找到菜单文件 {0}.json，请自行生成！", configX["name"]);
                    }
                }
            return true;
        }

        #endregion

        #region 玩家菜单选择信息

        private static void Failed()
        {
            uint failed = mcapi.sendModalForm(Function.getUuid(NAME), "经济系统", "余额不足！", "确定", "取消");
            Thread.Sleep(timeout);
            mcapi.releaseForm(failed);
        }

        /// <summary>
        /// 设置玩家表单监听
        /// </summary>
        private static bool FormSelect(Events x)
        {
            var json = BaseEvent.getFrom(x) as FormSelectEvent;
            if (json.selected != "null")
            {
                if (json.formid == MenuX)
                {
                    var selected = int.Parse(json.selected);
                    var money = true;
                    foreach (JObject menuX in menu["buttons"][selected]["cmd"])
                    {
                        if ((string)menuX["type"] != "none")
                        {
                            var MENU = new JObject(
                               new JProperty("command", menuX["command"]),
                               new JProperty("type", menuX["type"])
                            );
                            if ((bool)menu["buttons"][selected]["moneyX"] && money)
                            {
                                money = false;
                                if (!ChangeMoney(json.playername, "Deduct", (int)menu["buttons"][selected]["money"]))
                                {
                                    var failed = new Thread(new ThreadStart(Failed));
                                    failed.Name = "余额不足";
                                    NAME = json.playername;
                                    failed.Start();
                                    break;
                                }
                            }
                            Menu_Do(json.playername, MENU);
                        }
                    }
                    return false;
                }
                if (json.formid == Modal)
                {
                    string selected = bool.Parse(json.selected) ? "button1" : "button2";
                    foreach (JObject menuX in menu["events"][selected]["cmd"])
                        if ((string)menuX["type"] != "none")
                        {
                            var MENU = new JObject(
                               new JProperty("moneyX", false),
                               new JProperty("command", menuX["command"]),
                               new JProperty("type", menuX["type"])
                            );
                            Menu_Do(json.playername, MENU);
                        }
                    return false;
                }
            }
            return true;
        }

        #endregion

        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="api">MC相关API方法</param>
        internal static void init(MCCSAPI api)
        {
            
            mcapi = api;

            initPlugin();

            #region 设置事件监听器

            api.addAfterActListener(EventKey.onUseItem, UseItem);
            api.addAfterActListener(EventKey.onLoadName, PlayerJoin);
            api.addAfterActListener(EventKey.onPlayerLeft, PlayerLeft);
            api.addAfterActListener(EventKey.onFormSelect, FormSelect);
            api.addBeforeActListener(EventKey.onServerCmd, ServerCmd);
            api.addBeforeActListener(EventKey.onInputCommand, InputCommand);

            #endregion
        }
    }
}



namespace CSR
{
    partial class Plugin
    {
        /// <summary>
        /// 静态api对象
        /// </summary>
        public static MCCSAPI api { get; private set; } = null;

        /// <summary>
        /// 插件装载时的事件
        /// </summary>
        public static int onServerStart(string pathandversion)
        {
            string[] pav = pathandversion.Split(',');
            if (pav.Length > 1)
            {
                string path = pav[0];
                string version = pav[1];
                bool commercial = (pav[pav.Length - 1] == "1");
                api = new MCCSAPI(path, version, commercial);
                if (api != null)
                {
                    onStart(api);
                    GC.KeepAlive(api);
                    return 0;
                }
            }
            Console.WriteLine("Load failed.");
            return -1;
        }

        /// <summary>
        /// 通用调用接口
        /// </summary>
        /// <param name="api">MC相关调用方法</param>
        public static void onStart(MCCSAPI api)
        {
            // TODO 此接口为必要实现
            Menu.Menu.init(api);
            Console.WriteLine("[{0}]{1}插件加载成功！ By：{2}", Menu.Menu.Version, Menu.Menu.PluginName, Menu.Menu.Author);
        }
    }
}