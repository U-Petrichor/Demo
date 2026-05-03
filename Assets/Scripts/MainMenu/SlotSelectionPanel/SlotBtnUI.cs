using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Manager.UserDataManager;

[RequireComponent(typeof(Button))]
public class SlotItemUI : MonoBehaviour
{
    [Header("自身组件")]
    [Tooltip("挂载在 SlotBtn 自身的 Button 组件")]
    public Button slotBtn;

    [Header("Line1 数据")]
    public TextMeshProUGUI slotId;
    public TextMeshProUGUI lastSaveTimeTicks;

    [Header("Line2 数据")]
    public TextMeshProUGUI achievementPercent;
    public TextMeshProUGUI collectionPercent;

    [Header("Line3 数据")]
    public TextMeshProUGUI totalRun;
    public TextMeshProUGUI clearCount;

    private string _slotId;
    private int _slotIndex;
    private Action<string, int> _onClickCallback;

    private void Awake()
    {
        // 自动获取自身的 Button 组件防呆
        if (slotBtn == null)
        {
            slotBtn = GetComponent<Button>();
        }
    }

    /// <summary>
    /// 由 AccountUIController 在生成此预制体时调用
    /// </summary>
    public void Initialize(SlotSummary summary, Action<string, int> onClickAction)
    {
        _slotId = summary.SlotId;
        _slotIndex = summary.SlotIndex;
        _onClickCallback = onClickAction;

        // --- 数据填充 ---
        
        // Line 1
        slotId.text = summary.SlotId;
        
        // 转换 Ticks 为本地时间，并格式化为明确的 年月日
        DateTime time = new DateTime(summary.LastSaveTimeTicks, DateTimeKind.Utc).ToLocalTime();
        lastSaveTimeTicks.text = time.ToString("yyyy年MM月dd日");

        // Line 2 (格式化为无小数点的百分比)
        achievementPercent.text = $"成就: {(summary.AchievementPercent * 100):F0}%";
        collectionPercent.text = $"收集: {(summary.CollectionPercent * 100):F0}%";

        // Line 3
        totalRun.text = $"局数: {summary.TotalRuns}";
        clearCount.text = $"通关: {summary.ClearCount}";

        // --- 事件绑定 ---
        slotBtn.onClick.RemoveAllListeners(); 
        slotBtn.onClick.AddListener(OnSlotClicked);
    }

    private void OnSlotClicked()
    {
        _onClickCallback?.Invoke(_slotId, _slotIndex);
    }
}