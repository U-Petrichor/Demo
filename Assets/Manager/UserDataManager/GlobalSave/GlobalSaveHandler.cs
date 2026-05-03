using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Manager.UserDataManager
{
    public class GlobalSaveHandler : MonoBehaviour
    {
        public static GlobalSaveHandler Instance { get; private set; }

        // 当前正在游玩的第二层全局数据（内存级）
        public GlobalSaveData CurrentGlobalData { get; private set; }

        // 运行时缓存：用于实验室和单局游戏高频查询物品是否解锁 (O(1) 复杂度)
        private HashSet<string> _runtimeCollectedItemsCache = new HashSet<string>();

        // 当前游玩槽位的物理索引 (对接第一层 ProfileHandler 需要)
        private int _currentSlotIndex;

        // 存档存放的根目录
        private string SavesDirectoryPath => Path.Combine(Application.persistentDataPath, "Saves");

        // --- 游戏全局配置 (应由你的配置表系统提供，这里写死仅作示例) ---
        private const int MAX_RUN_HISTORY_COUNT = 20; // 限制游玩记录上限防止存档膨胀
        private const int TOTAL_GAME_ACHIEVEMENTS = 50; // 初期调试试用，后期要删
        private const int TOTAL_GAME_ITEMS = 120; // 初期调试试用，后期要删

        private void Awake()
        {
            if (Instance != null && Instance != this) return;
            Instance = this;

            if (!Directory.Exists(SavesDirectoryPath))
            {
                Directory.CreateDirectory(SavesDirectoryPath);
            }
        }

        // ==========================================
        // 第一部分：生命周期与 I/O 控制
        // ==========================================

        /// <summary>
        /// 从主菜单选择存档槽位进入游戏时调用
        /// </summary>
        public bool LoadSlot(string slotId, int slotIndex)
        {
            _currentSlotIndex = slotIndex;
            string filePath = Path.Combine(SavesDirectoryPath, $"{slotId}_Global.json");
            string backupFilePath = filePath + ".bak";

            // 1. 如果没有存档，创建初始空存档
            if (!File.Exists(filePath))
            {
                CurrentGlobalData = new GlobalSaveData { SlotId = slotId };
                InitializeRuntimeCache();
                SaveToDisk(); // 立刻落盘并生成摘要
                return true;
            }

            // 2. 读取主文件
            if (TryLoadFromFile(filePath)) return true;

            // 3. 读取备份文件
            Debug.LogWarning($"[GlobalSaveHandler] {slotId} 主文件损坏，尝试读取备份...");
            if (File.Exists(backupFilePath) && TryLoadFromFile(backupFilePath))
            {
                File.Copy(backupFilePath, filePath, true);
                return true;
            }

            Debug.LogError($"[GlobalSaveHandler] {slotId} 存档彻底损坏！");
            return false;
        }

        private bool TryLoadFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                CurrentGlobalData = JsonConvert.DeserializeObject<GlobalSaveData>(json);
                if (CurrentGlobalData == null) return false;

                InitializeRuntimeCache();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GlobalSaveHandler] 读取失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 将内存数据构建的高性能缓存初始化
        /// </summary>
        private void InitializeRuntimeCache()
        {
            _runtimeCollectedItemsCache.Clear();
            if (CurrentGlobalData.CollectedItemIDs != null)
            {
                foreach (var id in CurrentGlobalData.CollectedItemIDs)
                {
                    _runtimeCollectedItemsCache.Add(id);
                }
            }
        }

        /// <summary>
        /// 将当前内存中的第二层数据物理写入磁盘，并通知第一层更新 UI 摘要
        /// 建议调用时机：单局游戏结束、解锁成就时
        /// </summary>
        public void SaveToDisk()
        {
            if (CurrentGlobalData == null) return;

            string filePath = Path.Combine(SavesDirectoryPath, $"{CurrentGlobalData.SlotId}_Global.json");
            string tempFilePath = filePath + ".tmp";
            string backupFilePath = filePath + ".bak";

            try
            {
                string json = JsonConvert.SerializeObject(CurrentGlobalData, Formatting.Indented);
                
                File.WriteAllText(tempFilePath, json);

                if (File.Exists(filePath))
                {
                    if (File.Exists(backupFilePath)) File.Delete(backupFilePath);
                    File.Move(filePath, backupFilePath);
                }

                File.Move(tempFilePath, filePath);

                // 核心对接：第二层保存成功后，立刻生成摘要推给第一层
                SyncWithProfileLayer();
            }
            catch (Exception e)
            {
                Debug.LogError($"[GlobalSaveHandler] 写入本地失败: {e.Message}");
            }
        }

        private void SyncWithProfileLayer()
        {
            SlotSummary summary = new SlotSummary
            {
                SlotId = CurrentGlobalData.SlotId,
                SlotIndex = _currentSlotIndex,
                TotalRuns = CurrentGlobalData.Stats.TotalRuns,
                ClearCount = CurrentGlobalData.Stats.ClearCount,
                LastSaveTimeTicks = DateTime.UtcNow.Ticks
            };

            int unlockedAchCount = CurrentGlobalData.Achievements.Count(a => a.IsUnlocked);
            summary.AchievementPercent = TOTAL_GAME_ACHIEVEMENTS > 0 ? (float)unlockedAchCount / TOTAL_GAME_ACHIEVEMENTS : 0f;
            summary.CollectionPercent = TOTAL_GAME_ITEMS > 0 ? (float)CurrentGlobalData.CollectedItemIDs.Count / TOTAL_GAME_ITEMS : 0f;

            // 调用你之前写好的第一层更新接口
            ProfileHandler.Instance.UpdateOrAddSlotSummary(summary);
        }

        // ==========================================
        // 第二部分：供游戏逻辑层调用的业务接口
        // ==========================================

        #region 收集品系统
        public void UnlockItem(string itemID)
        {
            if (CurrentGlobalData == null || string.IsNullOrEmpty(itemID)) return;

            // 利用 HashSet 实现 O(1) 查重，防止 List 重复添加
            if (!_runtimeCollectedItemsCache.Contains(itemID))
            {
                _runtimeCollectedItemsCache.Add(itemID);
                CurrentGlobalData.CollectedItemIDs.Add(itemID);
                
                // 视需求决定是否立刻触发保存。如果收集品极多，可考虑延迟保存。
                // SaveToDisk(); 
            }
        }

        public bool IsItemUnlocked(string itemID)
        {
            return _runtimeCollectedItemsCache.Contains(itemID);
        }
        #endregion

        #region 成就系统
        public void UnlockAchievement(string achievementID)
        {
            if (CurrentGlobalData == null || string.IsNullOrEmpty(achievementID)) return;

            var ach = CurrentGlobalData.Achievements.Find(a => a.AchievementID == achievementID);
            if (ach == null)
            {
                // 首次添加并解锁
                CurrentGlobalData.Achievements.Add(new AchievementData
                {
                    AchievementID = achievementID,
                    IsUnlocked = true,
                    UnlockTimestamp = DateTime.UtcNow.Ticks,
                    Progress = 1f
                });
                SaveToDisk(); // 成就解锁属于高价值行为，建议立刻落盘
                // 这里可触发 C# event 通知 UI 弹窗
            }
            else if (!ach.IsUnlocked)
            {
                // 更新状态
                ach.IsUnlocked = true;
                ach.UnlockTimestamp = DateTime.UtcNow.Ticks;
                ach.Progress = 1f;
                SaveToDisk();
            }
        }

        public void UpdateAchievementProgress(string achievementID, float progressToAdd)
        {
            if (CurrentGlobalData == null || string.IsNullOrEmpty(achievementID)) return;

            var ach = CurrentGlobalData.Achievements.Find(a => a.AchievementID == achievementID);
            if (ach == null)
            {
                ach = new AchievementData { AchievementID = achievementID, IsUnlocked = false, Progress = 0f };
                CurrentGlobalData.Achievements.Add(ach);
            }

            if (!ach.IsUnlocked)
            {
                ach.Progress += progressToAdd;
                // 这里不需要每加 1 点进度就 SaveToDisk，否则引发严重卡顿，应在单局结算时统一 Save
            }
        }
        #endregion

        #region 游玩记录与统计
        public void AddRunHistory(RunHistoryRecord newRecord, bool isClear)
        {
            if (CurrentGlobalData == null || newRecord == null) return;

            // 1. 更新记录墙
            CurrentGlobalData.RunHistories.Add(newRecord);
            // 剔除旧数据防止存档膨胀
            if (CurrentGlobalData.RunHistories.Count > MAX_RUN_HISTORY_COUNT)
            {
                CurrentGlobalData.RunHistories.RemoveAt(0); 
            }

            // 2. 更新基础统计
            CurrentGlobalData.Stats.TotalRuns++;
            if (isClear) CurrentGlobalData.Stats.ClearCount++;
            CurrentGlobalData.Stats.TotalFloorsClimbed += newRecord.FinalFloor;
            CurrentGlobalData.Stats.TotalPlayTimeSeconds += newRecord.DurationSeconds;

            SaveToDisk(); // 单局结束属于核心节点，强制落盘
        }

        public void UpdateAscensionLevel(int levelToUnlock)
        {
            if (CurrentGlobalData != null && levelToUnlock > CurrentGlobalData.MaxUnlockedAscensionLevel)
            {
                CurrentGlobalData.MaxUnlockedAscensionLevel = levelToUnlock;
                SaveToDisk();
            }
        }
        #endregion
    }
}