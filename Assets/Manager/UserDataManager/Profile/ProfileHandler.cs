using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Manager.UserDataManager
{
    public enum ProfileLoadStatus
    {
        Success,              // 成功读取（或成功创建新档）
        RecoveredFromBackup,  // 主文件损坏，但成功从备份恢复
        Corrupted             // 主文件和备份均损坏，需要玩家介入
    }

    public class ProfileHandler : MonoBehaviour
    {
        public static ProfileHandler Instance { get; private set; }
        public ProfileData CurrentProfile { get; private set; }
        
        public string ProfilesDirectoryPath { get; private set; }

        // 【新增】账号初始化完成的全局广播事件
        public event Action OnProfileDataInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this) return;
            Instance = this;

            ProfilesDirectoryPath = Path.Combine(Application.persistentDataPath, "Profiles");
            if (!Directory.Exists(ProfilesDirectoryPath))
            {
                Directory.CreateDirectory(ProfilesDirectoryPath);
            }
        }

        public ProfileLoadStatus InitializeAccount(ulong steamId, out string errorFilePath)
        {
            errorFilePath = string.Empty;
            string filePath = Path.Combine(ProfilesDirectoryPath, $"{steamId}_Profile.json");
            string backupFilePath = filePath + ".bak";

            // 1. 如果完全没有存档，直接新建（首次游玩）
            if (!File.Exists(filePath))
            {
                CreateNewProfile(steamId);
                SaveCurrentProfile();
                OnProfileDataInitialized?.Invoke(); // 触发事件
                return ProfileLoadStatus.Success;
            }

            // 2. 尝试读取主文件
            if (TryLoadFromFile(filePath))
            {
                OnProfileDataInitialized?.Invoke(); // 触发事件
                return ProfileLoadStatus.Success;
            }

            // 3. 主文件失败，尝试读取备份文件
            Debug.LogWarning("[ProfileHandler] 主存档损坏，尝试从备份恢复...");
            if (File.Exists(backupFilePath) && TryLoadFromFile(backupFilePath))
            {
                File.Copy(backupFilePath, filePath, true);
                Debug.Log("[ProfileHandler] 存档已成功从备份恢复。");
                OnProfileDataInitialized?.Invoke(); // 触发事件
                return ProfileLoadStatus.RecoveredFromBackup;
            }

            // 4. 彻底损坏
            errorFilePath = filePath;
            CurrentProfile = null; 
            return ProfileLoadStatus.Corrupted;
        }

        private bool TryLoadFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var tempProfile = JsonConvert.DeserializeObject<ProfileData>(json);

                if (tempProfile == null) return false;
                
                tempProfile.SlotSummaries ??= new Dictionary<string, SlotSummary>();
                tempProfile.LastLoginDateTicks = DateTime.UtcNow.Ticks;
                
                CurrentProfile = tempProfile;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProfileHandler] 文件解析失败 ({path}): {e.Message}");
                return false;
            }
        }

        public void ForceCreateNewProfile(ulong steamId)
        {
            CreateNewProfile(steamId);
            SaveCurrentProfile();
            OnProfileDataInitialized?.Invoke(); // 强制覆盖后也触发事件
        }

        private void CreateNewProfile(ulong steamId)
        {
            CurrentProfile = new ProfileData(steamId);
        }

        public void SaveCurrentProfile()
        {
            if (CurrentProfile == null) return;
            
            string filePath = Path.Combine(ProfilesDirectoryPath, $"{CurrentProfile.SteamId}_Profile.json");
            string tempFilePath = filePath + ".tmp";
            string backupFilePath = filePath + ".bak";

            try
            {
                string json = JsonConvert.SerializeObject(CurrentProfile, Formatting.Indented);
                
                File.WriteAllText(tempFilePath, json);

                if (File.Exists(filePath))
                {
                    if (File.Exists(backupFilePath)) File.Delete(backupFilePath);
                    File.Move(filePath, backupFilePath);
                }

                File.Move(tempFilePath, filePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProfileHandler] 保存存档失败: {e.Message}");
            }
        }

        public List<SlotSummary> GetAllSlotSummaries()
        {
            if (CurrentProfile == null || CurrentProfile.SlotSummaries == null)
            {
                return new List<SlotSummary>();
            }
            return CurrentProfile.SlotSummaries.Values.OrderBy(s => s.SlotIndex).ToList();
        }

        public void UpdateOrAddSlotSummary(SlotSummary newSummary)
        {
            if (CurrentProfile == null) return;
            if (newSummary == null || string.IsNullOrEmpty(newSummary.SlotId)) return;

            CurrentProfile.SlotSummaries[newSummary.SlotId] = newSummary;
            SaveCurrentProfile();
        }

        public void DeleteSlot(string slotId)
        {
            if (CurrentProfile == null || !CurrentProfile.SlotSummaries.ContainsKey(slotId)) return;

            CurrentProfile.SlotSummaries.Remove(slotId);
            SaveCurrentProfile();
        }
    }
}