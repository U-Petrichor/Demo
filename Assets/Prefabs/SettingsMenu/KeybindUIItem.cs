using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.EventSystems; // 引入事件系统以处理焦点

public class KeybindUIItem : MonoBehaviour
{
    [Header("绑定设置")]
    public string actionName;

    [Header("UI 组件引用")]
    public TextMeshProUGUI labelText;
    public Button bindButton;
    public TextMeshProUGUI buttonText;

    [Header("多语言文本配置")]
    public LocalizedString waitingInputString;

    private bool isWaitingForInput = false;

    // 【全局互斥锁】确保同一时间只有一个按钮在等待输入
    private static KeybindUIItem currentActiveItem = null;

    private void Start()
    {
        bindButton.onClick.AddListener(StartListening);
        UpdateButtonText();
    }

    private void StartListening()
    {
        // 1. 如果有其他按钮正在等待，强行打断它
        if (currentActiveItem != null && currentActiveItem != this)
        {
            currentActiveItem.CancelListening();
        }

        // 2. 将全局锁交给自己
        currentActiveItem = this;

        // 3. 剥夺当前 UI 焦点
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        isWaitingForInput = true;
        buttonText.text = waitingInputString.GetLocalizedString();
    }

    // 供其他按钮抢夺锁时调用
    public void CancelListening()
    {
        isWaitingForInput = false;
        UpdateButtonText();
    }

    private void OnGUI()
    {
        if (!isWaitingForInput) return;

        Event e = Event.current;

        // 【处理鼠标按键】
        if (e.isMouse && e.type == EventType.MouseDown)
        {
            CustomKeyBind newBind = new CustomKeyBind
            {
                RequireCtrl = e.control,
                RequireShift = e.shift,
                RequireAlt = e.alt,
                MainKey = KeyCode.Mouse0 + e.button 
            };

            FinishBinding(newBind);
            e.Use(); 
            return;
        }

        // 【处理键盘按键】
        if (e.isKey && e.keyCode != KeyCode.None)
        {
            // 阶段 1：按键按下的瞬间 (KeyDown)
            if (e.type == EventType.KeyDown)
            {
                if (IsModifierKey(e.keyCode))
                {
                    string tempStr = "";
                    if (e.control) tempStr += "Ctrl + ";
                    if (e.shift) tempStr += "Shift + ";
                    if (e.alt) tempStr += "Alt + ";
                    
                    // 只显示 "Ctrl + " 等
                    buttonText.text = tempStr;
                    return; 
                }

                CustomKeyBind newBind = new CustomKeyBind
                {
                    RequireCtrl = e.control,
                    RequireShift = e.shift,
                    RequireAlt = e.alt,
                    MainKey = e.keyCode
                };

                FinishBinding(newBind);
                e.Use();
            }
            // 阶段 2：按键抬起的瞬间 (KeyUp) - 专门用于绑定修饰键本身
            else if (e.type == EventType.KeyUp)
            {
                if (IsModifierKey(e.keyCode))
                {
                    CustomKeyBind newBind = new CustomKeyBind
                    {
                        RequireCtrl = false, 
                        RequireShift = false,
                        RequireAlt = false,
                        MainKey = e.keyCode 
                    };

                    FinishBinding(newBind);
                    e.Use();
                }
            }
        }
    }

    private void FinishBinding(CustomKeyBind bind)
    {
        KeybindManager.Instance.BindKey(actionName, bind);
        isWaitingForInput = false;

        // 任务完成，释放全局锁
        if (currentActiveItem == this)
        {
            currentActiveItem = null;
        }

        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (KeybindManager.Instance.Keybinds.ContainsKey(actionName))
        {
            buttonText.text = KeybindManager.Instance.Keybinds[actionName].ToString();
        }
    }

    private bool IsModifierKey(KeyCode code)
    {
        return code == KeyCode.LeftControl || code == KeyCode.RightControl ||
               code == KeyCode.LeftShift || code == KeyCode.RightShift ||
               code == KeyCode.LeftAlt || code == KeyCode.RightAlt ||
               code == KeyCode.LeftCommand || code == KeyCode.RightCommand;
    }
}