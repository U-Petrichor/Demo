using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager.SaveManager
{
    public class RunSaveManager : MonoBehaviour
    {
        public static RunSaveManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) return;
            Instance = this;
        }

        public void InitializeReadyState()
        {
            Debug.Log("[RunSaveManager] 处于待命状态。");
        }

        // 由主菜单“进入地牢”按钮触发
        public void GenerateOrLoadRun(string slotId)
        {
            // 生成新地牢或读取本地断点快照 (.tmp)
        }
    }
}
