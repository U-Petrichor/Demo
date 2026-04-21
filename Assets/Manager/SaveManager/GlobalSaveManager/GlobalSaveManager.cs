using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager.SaveManager
{
    public class GlobalSaveManager : MonoBehaviour
    {
        public static GlobalSaveManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) return;
            Instance = this;
        }

        // 由 CoreManager 调用的待命状态初始化
        public void InitializeReadyState()
        {
            Debug.Log("[GlobalSaveManager] 处于待命状态。等待玩家在 UI 中选择具体存档槽位。");
        }

        // 这个方法不归 CoreManager 管，而是归 UI 按钮的 OnClick 事件管
        public void LoadSlotData(string slotId)
        {
            // 读取具体存档逻辑...
        }
    }
}