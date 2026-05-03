using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; 
using Manager.UserDataManager;

public class AccountUIController : MonoBehaviour
{
    [Header("UI 面板引用")]
    [SerializeField] private GameObject slotSelectionPanel;
    
    [Header("动态生成配置")]
    [Tooltip("已有存档的 Prefab")]
    [SerializeField] private GameObject slotItemPrefab;
    [Tooltip("用于新建存档的按钮 Prefab")]
    [SerializeField] private GameObject newSlotPrefab;
    [Tooltip("带有 Vertical Layout Group 的容器")]
    [SerializeField] private Transform slotContainer;

    [Header("文本组件引用")]
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI slotNameText;

    private void OnEnable()
    {
        // 订阅初始化事件：一旦底层数据准备完毕，自动刷新 UI
        if (ProfileHandler.Instance != null)
        {
            ProfileHandler.Instance.OnProfileDataInitialized += RefreshAccountDisplay;
        }
    }

    private void OnDisable()
    {
        if (ProfileHandler.Instance != null)
        {
            ProfileHandler.Instance.OnProfileDataInitialized -= RefreshAccountDisplay;
        }
    }

    private void Start()
    {
        CloseSlotSelectionPanel();

        // 首次启动时，如果数据还没异步加载完，显示等待状态
        if (ProfileHandler.Instance == null || ProfileHandler.Instance.CurrentProfile == null)
        {
            userNameText.text = "正在同步 Steam...";
            slotNameText.text = "---";
        }
        else
        {
            RefreshAccountDisplay();
        }
    }

    public void OpenSlotSelectionPanel()
    {
        if (slotSelectionPanel == null) return;
        
        slotSelectionPanel.SetActive(true);
        GenerateSlotList();
    }

    public void CloseSlotSelectionPanel()
    {
        if (slotSelectionPanel != null)
        {
            slotSelectionPanel.SetActive(false);
        }
    }

    public void RefreshAccountDisplay()
    {
        if (ProfileHandler.Instance != null && ProfileHandler.Instance.CurrentProfile != null)
        {
            userNameText.text = ProfileHandler.Instance.CurrentProfile.SteamId.ToString();
        }
        else
        {
            userNameText.text = "未知账号";
        }

        if (GlobalSaveHandler.Instance != null && GlobalSaveHandler.Instance.CurrentGlobalData != null)
        {
            slotNameText.text = GlobalSaveHandler.Instance.CurrentGlobalData.SlotId;
        }
        else
        {
            slotNameText.text = "未选择存档";
        }
    }

    private void GenerateSlotList()
    {
        if (slotItemPrefab == null || newSlotPrefab == null || slotContainer == null)
        {
            Debug.LogError("[AccountUIController] 预制体或容器未分配！");
            return;
        }

        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        List<SlotSummary> summaries = ProfileHandler.Instance.GetAllSlotSummaries();

        foreach (var summary in summaries)
        {
            // 注意 Instantiate 加上 false，防止 UI 缩放变形
            GameObject slotObj = Instantiate(slotItemPrefab, slotContainer, false);
            SlotItemUI slotUI = slotObj.GetComponent<SlotItemUI>();
            if (slotUI != null)
            {
                slotUI.Initialize(summary, OnSlotSelected);
            }
        }

        GameObject newSlotObj = Instantiate(newSlotPrefab, slotContainer, false);
        Button newSlotBtn = newSlotObj.GetComponent<Button>();
        if (newSlotBtn != null)
        {
            newSlotBtn.onClick.RemoveAllListeners();
            newSlotBtn.onClick.AddListener(OnNewSlotClicked);
        }
    }

    private void OnSlotSelected(string clickedSlotId, int clickedSlotIndex)
    {
        bool success = GlobalSaveHandler.Instance.LoadSlot(clickedSlotId, clickedSlotIndex);

        if (success)
        {
            RefreshAccountDisplay();
            CloseSlotSelectionPanel();
        }
        else
        {
            Debug.LogError("[AccountUIController] 已有存档读取失败！");
        }
    }

    private void OnNewSlotClicked()
    {
        List<SlotSummary> summaries = ProfileHandler.Instance.GetAllSlotSummaries();
        
        int newIndex = 0;
        if (summaries != null && summaries.Count > 0)
        {
            newIndex = summaries.Max(s => s.SlotIndex) + 1;
        }

        string newSlotId = $"SaveSlot_{newIndex}";

        bool success = GlobalSaveHandler.Instance.LoadSlot(newSlotId, newIndex);

        if (success)
        {
            RefreshAccountDisplay();
            CloseSlotSelectionPanel();
        }
        else
        {
            Debug.LogError("[AccountUIController] 新建存档失败！");
        }
    }
}