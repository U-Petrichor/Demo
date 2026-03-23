using System.Collections.Generic;
using UnityEngine;

// 【新增】工业级组合键数据结构
[System.Serializable]
public struct CustomKeyBind
{
    public KeyCode MainKey;
    public bool RequireCtrl;
    public bool RequireShift;
    public bool RequireAlt;

    // 将结构体转化为 UI 显示的字符串
    public override string ToString()
    {
        string s = "";
        if (RequireCtrl) s += "Ctrl + ";
        if (RequireShift) s += "Shift + ";
        if (RequireAlt) s += "Alt + ";
        s += MainKey.ToString();
        return s;
    }

    // 存入硬盘时的序列化格式 (例如: "Ctrl+W", "Mouse0", "Escape")
    public string Serialize()
    {
        string s = "";
        if (RequireCtrl) s += "Ctrl+";
        if (RequireShift) s += "Shift+";
        if (RequireAlt) s += "Alt+";
        s += MainKey.ToString();
        return s;
    }

    // 从硬盘读取时的反序列化解析器
    public static CustomKeyBind Parse(string savedStr)
    {
        CustomKeyBind bind = new CustomKeyBind();
        bind.RequireCtrl = savedStr.Contains("Ctrl+");
        bind.RequireShift = savedStr.Contains("Shift+");
        bind.RequireAlt = savedStr.Contains("Alt+");

        string[] parts = savedStr.Split('+');
        string keyStr = parts[parts.Length - 1]; // 最后一个永远是主键

        if (System.Enum.TryParse(keyStr, out KeyCode parsedKey))
            bind.MainKey = parsedKey;
        else
            bind.MainKey = KeyCode.None;

        return bind;
    }
}

public class KeybindManager : MonoBehaviour
{
    public static KeybindManager Instance { get; private set; }

    public Dictionary<string, CustomKeyBind> Keybinds { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad 在 SettingsManager 里做过了

        Keybinds = new Dictionary<string, CustomKeyBind>();
        LoadKeybinds();
    }

    private void LoadKeybinds()
    {
        SafeLoadKeybind("MoveUp", "W");
        SafeLoadKeybind("MoveDown", "S");
        SafeLoadKeybind("MoveLeft", "A");
        SafeLoadKeybind("MoveRight", "D");
        SafeLoadKeybind("Attack", "Mouse0");        // 鼠标左键
        SafeLoadKeybind("Skill", "Ctrl+Mouse1");    // Ctrl+鼠标右键
        SafeLoadKeybind("Menu", "Escape");          // 绑定ESC
    }

    private void SafeLoadKeybind(string actionName, string defaultKeyString)
    {
        string savedKeyString = PlayerPrefs.GetString(actionName, defaultKeyString);
        CustomKeyBind bind = CustomKeyBind.Parse(savedKeyString);

        // 容错：如果解析出来连主键都没有（存档被玩家改烂了），强行重置
        if (bind.MainKey == KeyCode.None)
        {
            Debug.LogWarning($"[{actionName}] 存档已损坏，重置为: {defaultKeyString}");
            bind = CustomKeyBind.Parse(defaultKeyString);
        }

        Keybinds[actionName] = bind;
    }

    public void BindKey(string actionName, CustomKeyBind newBind)
    {
        Keybinds[actionName] = newBind;
        PlayerPrefs.SetString(actionName, newBind.Serialize());
        PlayerPrefs.Save();
    }

    public bool GetActionDown(string actionName)
    {
        if (!Keybinds.TryGetValue(actionName, out CustomKeyBind bind)) return false;

        // 检查修饰键状态是否全部吻合
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        if (bind.RequireCtrl != ctrlHeld) return false;
        if (bind.RequireShift != shiftHeld) return false;
        if (bind.RequireAlt != altHeld) return false;

        // 最后检查主按键是否按下
        return Input.GetKeyDown(bind.MainKey);
    }

    public bool GetAction(string actionName)
    {
        if (!Keybinds.TryGetValue(actionName, out CustomKeyBind bind)) return false;

        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        if (bind.RequireCtrl != ctrlHeld) return false;
        if (bind.RequireShift != shiftHeld) return false;
        if (bind.RequireAlt != altHeld) return false;

        return Input.GetKey(bind.MainKey);
    }
}