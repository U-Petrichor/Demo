/*
    此脚本专门用于处理用户自定义按键绑定：其逻辑为
    1. 在游戏加载时尝试用 SafeLoadKeybind 去加载每一个键位，在 Awake 中传入固定的默认键位
    2. 在每一个键位的设置中，尝试读取用户曾经的自定义键位，若出现了错误，则使用 Awake 中传入的默认键位，否则使用用户自定义键位，这样保证了游戏的高容错
    3. 对于组合键，通过 CustomKeyBind 结构体的 RequireCtrl, RequireShift, RequireAlt 字段来判断是否需要同时按下这些键，从而实现组合键的功能
    4. 对于UI中所需要呈现的内容，通过 CustomKeyBind 结构体的 ToString 方法来将其转化为字符串，例如 "Ctrl + Shift + W"
    5. 对于用户输入，通过 GetActionDown 严格判断是否按下了正确的键位
*/
using System.Collections.Generic;
using UnityEngine;

// 组合键数据结构
[System.Serializable]
public struct CustomKeyBind
{
    public KeyCode MainKey;      // 主键 (例如 W, Mouse0, Escape)
    public bool RequireCtrl;     // 是否需要同时按下 Ctrl 键
    public bool RequireShift;    // 是否需要同时按下 Shift 键
    public bool RequireAlt;      // 是否需要同时按下 Alt 键

    // 将结构体转化为 UI 显示的字符串 (用于在设置面板中展示给玩家看)
    public override string ToString()
    {
        string s = "";
        if (RequireCtrl) s += "Ctrl + ";
        if (RequireShift) s += "Shift + ";
        if (RequireAlt) s += "Alt + ";
        s += MainKey.ToString(); // 组合结果例如: "Ctrl + Shift + W"
        return s;
    }

    // 存入硬盘时的序列化格式 (去掉空格以节省空间，例如: "Ctrl+W", "Mouse0")
    public string Serialize()
    {
        string s = "";
        if (RequireCtrl) s += "Ctrl+";
        if (RequireShift) s += "Shift+";
        if (RequireAlt) s += "Alt+";
        s += MainKey.ToString();
        return s;
    }

    // 从硬盘读取时的反序列化解析器 (将 "Ctrl+W" 还原成 CustomKeyBind 结构体)
    public static CustomKeyBind Parse(string savedStr)
    {
        CustomKeyBind bind = new CustomKeyBind();

        // 1. 判断字符串中是否包含特定修饰键的标识
        bind.RequireCtrl = savedStr.Contains("Ctrl+");
        bind.RequireShift = savedStr.Contains("Shift+");
        bind.RequireAlt = savedStr.Contains("Alt+");

        // 2. 提取主键：通过 '+' 分割字符串，最后一部分必然是主键
        string[] parts = savedStr.Split('+');
        string keyStr = parts[parts.Length - 1];

        // 3. 安全地将字符串转换为 KeyCode 枚举类型
        // 如果转换成功，赋值给 MainKey；如果失败（比如存档被篡改），则赋值为 None
        /*
            1. System.Enum.TryParse：是 C# 自带的一个尝试转换函数。假设 keyStr 里的文本是 "W"，它会去 KeyCode 里找有没有叫 W 的按键
            2. out KeyCode parsedKey：这是 C# 的 out 语法。如果转换成功，它会当场创建一个名叫 parsedKey 的变量，并把转换后的结果（KeyCode.W）塞进去
            3. if 判断：TryParse 会返回一个布尔值。如果字符串是乱码（比如 "Hello"），找不到对应的按键，它就返回 false，这样就能防止游戏报错崩溃；如果找到了，返回 true，接着执行 bind.MainKey = parsedKey;
        */
        if (System.Enum.TryParse(keyStr, out KeyCode parsedKey))
            bind.MainKey = parsedKey;
        else
            bind.MainKey = KeyCode.None;

        return bind;
    }
}

public class KeybindManager : MonoBehaviour
{
    // 单例模式，方便全局随时调用 (例如 KeybindManager.Instance.GetActionDown("Attack"))
    public static KeybindManager Instance { get; private set; }

    // 核心字典：使用动作名称 (如 "MoveUp") 作为 Key，对应的按键配置作为 Value，存在：游戏数据Excel->Keybind 中
    public Dictionary<string, CustomKeyBind> Keybinds { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }
        Instance = this;

        Keybinds = new Dictionary<string, CustomKeyBind>();
        LoadKeybinds(); // 游戏启动时加载所有按键配置
    }

    // 注册并加载游戏中所有的默认操作键位
    private void LoadKeybinds()
    {
        SafeLoadKeybind("MoveUp", "W");
        SafeLoadKeybind("MoveDown", "S");
        SafeLoadKeybind("MoveLeft", "A");
        SafeLoadKeybind("MoveRight", "D");
        SafeLoadKeybind("Attack", "Mouse0");        // 鼠标左键
        SafeLoadKeybind("Skill", "Ctrl+Mouse1");    // Ctrl + 鼠标右键
        SafeLoadKeybind("Menu", "Escape");          // 绑定ESC
    }

    // 安全加载机制：负责从 PlayerPrefs 读取存档，并处理存档损坏的情况
    private void SafeLoadKeybind(string actionName, string defaultKeyString)
    {
        // 使用Unity引擎自带的PlayerPrefs去查查是否有存过的数据，尝试获取玩家自定义的按键字符串，如果没有，则使用传入的默认值 (defaultKeyString)
        string savedKeyString = PlayerPrefs.GetString(actionName, defaultKeyString);
        CustomKeyBind bind = CustomKeyBind.Parse(savedKeyString);

        // 容错处理：如果解析出来连主键都没有（例如玩家修改注册表填了个乱码），强行重置为默认键位
        if (bind.MainKey == KeyCode.None)
        {
            Debug.LogWarning($"[{actionName}] 存档已损坏，重置为: {defaultKeyString}");
            bind = CustomKeyBind.Parse(defaultKeyString);
        }

        // 将最终合法的按键存入字典
        Keybinds[actionName] = bind;
    }

    // 供 UI 设置面板调用的方法：当玩家在设置里修改了按键后，调用此方法更新并保存
    public void BindKey(string actionName, CustomKeyBind newBind)
    {
        Keybinds[actionName] = newBind;                                // 更新内存字典
        PlayerPrefs.SetString(actionName, newBind.Serialize());        // 存入硬盘缓存
        PlayerPrefs.Save();                                            // 强制写入硬盘
    }

    // 获取某个动作是否在【当前帧被按下】 (相当于 Input.GetKeyDown)
    public bool GetActionDown(string actionName)
    {
        // 如果字典里找不到这个动作，直接返回 false
        if (!Keybinds.TryGetValue(actionName, out CustomKeyBind bind)) return false;

        // 1. 获取当前玩家实际上按下了哪些修饰键 (区分左右键)
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        // 2. 严格校验修饰键状态 (互斥校验)
        // 例如：设置里要求按 Ctrl，但玩家没按，或者设置里没要求按 Ctrl，但玩家按了，都不算触发。
        if (bind.RequireCtrl != ctrlHeld) return false;
        if (bind.RequireShift != shiftHeld) return false;
        if (bind.RequireAlt != altHeld) return false;

        // 3. 最后检查主按键是否在当前帧被按下
        return Input.GetKeyDown(bind.MainKey);
    }

    // 获取某个动作是否【处于持续被按住的状态】 (相当于 Input.GetKey)
    public bool GetAction(string actionName)
    {
        if (!Keybinds.TryGetValue(actionName, out CustomKeyBind bind)) return false;

        // 修饰键校验逻辑同上
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        if (bind.RequireCtrl != ctrlHeld) return false;
        if (bind.RequireShift != shiftHeld) return false;
        if (bind.RequireAlt != altHeld) return false;

        // 检查主键是否被按住
        return Input.GetKey(bind.MainKey);
    }
}