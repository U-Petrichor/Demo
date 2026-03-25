/*
    此脚本专门用于处理用户在设置菜单中点击绑定按键的操作
    1. 使用全局锁确保同一时间只有一个按钮在监听玩家输入，在StartListening中处理此逻辑
    2. 在OnGUI中按照此逻辑处理用户的绑定
        1. 是否是鼠标按键KeyDown，按下时是否有ctrl、shift、alt，监听到第一个主键就完成绑定，不监听KeyUp
        2. 是否是键盘按键KeyDown
            - 如果按下的是ctrl、shift、alt，则单纯更新按钮里面的文字，暂时不结束
            - 如果是其余键，则记录此时ctrl、shift、alt的状态，以及本次按下的键位，完成绑定
        3. 如果监听到KeyUp，则说明玩家一直没有按下主键，用IsModifierKey判断是否是修饰键，是则绑定
            - 但是这里有个问题，玩家无法绑定ctrl+shift这种没有主键的两个ModifierKey构成的组合键，但是考虑到一般不会有玩家这样使用
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.EventSystems; // 引入事件系统以处理焦点

public class KeybindUI : MonoBehaviour
{
    [Header("绑定设置")]
    public string actionName; 
    // 脚本需要挂在在按钮预制体的根节点，在每一个预制体的脚本处都要准确无误的填写这个按键的名称
    // 动作名称，例如 "MoveUp"，必须与 KeybindManager 里的名称对应，也可以查Excel表

    [Header("UI 组件引用")]
    public TextMeshProUGUI labelText;   // 左侧显示的文字，比如 "向前移动"
    public Button bindButton;           // 右侧供玩家点击的按钮
    public TextMeshProUGUI buttonText;  // 按钮上显示的当前按键，比如 "W" 或 "等待输入..."

    [Header("多语言文本配置")]
    public LocalizedString waitingInputString; // 多语言支持的 "等待输入..." 文本

    private bool isWaitingForInput = false; // 当前这个按钮是否正在监听玩家绑定新按键

    // 【全局互斥锁】static 意味着所有 KeybindUIItem 实例共享这一个变量
    // 用来确保同一时间绝对只有一个按钮处于 "等待输入" 状态
    private static KeybindUI currentActiveItem = null;

    private void Start()
    {
        // 游戏启动时，给按钮挂载点击事件
        bindButton.onClick.AddListener(StartListening);
        // 初始化按钮上显示的文字 (去 Manager 里查当前绑定的什么键)
        UpdateButtonText();
    }

    // 玩家点击了按钮，开始监听输入
    private void StartListening()
    {
        // 1. 互斥逻辑：如果有其他按钮正在等待输入，强行打断它，让它恢复原状
        if (currentActiveItem != null && currentActiveItem != this)
        {
            currentActiveItem.CancelListening();
        }

        // 2. 将全局锁交给自己，向全世界宣布：“现在轮到我接管输入了！”
        currentActiveItem = this;

        // 3. 剥夺当前 UI 焦点 (极其重要)
        // 防止玩家按下 Space(空格) 或 Enter(回车) 时，Unity 默认再次触发按钮点击事件
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // 4. 进入等待状态，并更新 UI 提示玩家
        isWaitingForInput = true;
        buttonText.text = waitingInputString.GetLocalizedString();
    }

    // 供其他按钮抢夺锁时调用（被迫停止监听）
    public void CancelListening()
    {
        isWaitingForInput = false;
        UpdateButtonText(); // 取消监听，把文字恢复成原来的按键名
    }

    // OnGUI 每一帧可能会执行多次，专门用来捕获底层的系统事件 (Event)
    private void OnGUI()
    {
        // 如果当前没在等待玩家输入，直接跳过，什么都不做
        if (!isWaitingForInput) return;

        Event e = Event.current; // 获取当前正在发生的事件（比如鼠标移动、键盘按下）

        // 【处理鼠标按键】
        // 如果事件是鼠标事件，且是按下(MouseDown)
        if (e.isMouse && e.type == EventType.MouseDown)
        {
            CustomKeyBind newBind = new CustomKeyBind
            {
                RequireCtrl = e.control, // 判断鼠标按下的同时，有没有按住 Ctrl
                RequireShift = e.shift,
                RequireAlt = e.alt,
                // 神奇的枚举加法：KeyCode.Mouse0 的底层数字是 323
                // e.button 返回 0(左键), 1(右键), 2(中键)
                // Mouse0 + 1 就变成了 KeyCode.Mouse1，完美映射！
                MainKey = KeyCode.Mouse0 + e.button 
            };

            FinishBinding(newBind); // 完成绑定
            e.Use();                // e.Use() 告诉系统：“这个事件我吃掉了，不要再往后传给游戏里的开枪或UI逻辑了”
            return;
        }

        // 【处理键盘按键】
        // isKey 代表是键盘事件，keyCode != None 排除掉一些无效的幽灵输入
        if (e.isKey && e.keyCode != KeyCode.None)
        {
            // 阶段 1：按键按下的瞬间 (KeyDown)
            if (e.type == EventType.KeyDown)
            {
                // 如果玩家按下的是修饰键 (比如只按下了 Ctrl)
                if (IsModifierKey(e.keyCode))
                {
                    string tempStr = "";
                    if (e.control) tempStr += "Ctrl + ";
                    if (e.shift) tempStr += "Shift + ";
                    if (e.alt) tempStr += "Alt + ";
                    
                    // 不结束绑定，而是实时更新 UI，让按钮显示 "Ctrl + "，提示玩家继续按下一个主键
                    buttonText.text = tempStr;
                    return; 
                }

                // 如果按下的不是修饰键（也就是按下了普通主键，比如 W, 空格, 回车）
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
            // 阶段 2：按键抬起的瞬间 (KeyUp) 
            // 专门为了处理玩家“仅仅想把动作绑定在单独的修饰键上”的情况 (比如我想按 Shift 翻滚，不按别的)
            else if (e.type == EventType.KeyUp)
            {
                // 如果玩家松开了一个修饰键 (说明他没有按其他主键就松手了)
                if (IsModifierKey(e.keyCode))
                {
                    CustomKeyBind newBind = new CustomKeyBind
                    {
                        // 因为修饰键本身变成了主键，所以不再需要组合键属性，全部设为 false
                        RequireCtrl = false, 
                        RequireShift = false,
                        RequireAlt = false,
                        MainKey = e.keyCode // 把 Shift 或 Ctrl 本身作为主键存进去
                    };

                    FinishBinding(newBind);
                    e.Use();
                }
            }
        }
    }

    // 完成绑定并善后
    private void FinishBinding(CustomKeyBind bind)
    {
        // 1. 将新的按键配置推送到 Manager 保存到硬盘
        KeybindManager.Instance.BindKey(actionName, bind);
        
        // 2. 退出监听状态
        isWaitingForInput = false;

        // 3. 释放全局锁 (如果锁还在自己手上的话)
        if (currentActiveItem == this)
        {
            currentActiveItem = null;
        }

        // 4. 更新按钮显示的文字
        UpdateButtonText();
    }

    // 更新按钮文字，向 Manager 索要当前动作对应的字符描述
    private void UpdateButtonText()
    {
        if (KeybindManager.Instance.Keybinds.ContainsKey(actionName))
        {
            buttonText.text = KeybindManager.Instance.Keybinds[actionName].ToString();
        }
    }

    // 工具方法：判断传入的按键是否是修饰键 (区分左右)
    private bool IsModifierKey(KeyCode code)
    {
        return code == KeyCode.LeftControl || code == KeyCode.RightControl ||
               code == KeyCode.LeftShift || code == KeyCode.RightShift ||
               code == KeyCode.LeftAlt || code == KeyCode.RightAlt ||
               code == KeyCode.LeftCommand || code == KeyCode.RightCommand; // Command 是 Mac 系统的按键
    }
}