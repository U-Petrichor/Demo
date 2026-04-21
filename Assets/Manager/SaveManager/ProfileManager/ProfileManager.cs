using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Steamworks;
using Newtonsoft.Json; // 必须引入此库替代 JsonUtility

namespace Manager.SaveManager
{
    public class ProfileManager : MonoBehaviour
    {
        public static ProfileManager Instance { get; private set; }
        public ProfileData CurrentProfile { get; private set; }

        private string profilesDirectoryPath;

        private void Awake()
        {
            if (Instance != null && Instance != this) return;
            Instance = this;

            profilesDirectoryPath = Path.Combine(Application.persistentDataPath, "Profiles");
            if (!Directory.Exists(profilesDirectoryPath))
            {
                Directory.CreateDirectory(profilesDirectoryPath);
            }
        }

        public void InitializeAccount(string steamId, string steamName)
        {
            string filePath = Path.Combine(profilesDirectoryPath, $"{steamId}_Settings.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    CurrentProfile = JsonConvert.DeserializeObject<ProfileData>(json);

                    if (CurrentProfile == null) throw new Exception("反序列化结果为空");
                    if (CurrentProfile.SlotSummaries == null) CurrentProfile.SlotSummaries = new Dictionary<string, SlotSummary>();

                    // 每次登录刷新最新的 Steam 昵称和登录时间
                    CurrentProfile.SteamName = steamName;
                    CurrentProfile.LastLoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ProfileManager] 账号数据异常，执行重置: {e.Message}");
                    CreateNewProfile(steamId, steamName);
                }
            }
            else
            {
                CreateNewProfile(steamId, steamName);
            }

            SaveCurrentProfile();
        }

        private void CreateNewProfile(string steamId, string steamName)
        {
            CurrentProfile = new ProfileData(steamId, steamName);
            Debug.Log($"[ProfileManager] 已为新玩家 {steamName} 创建本地配置。");
        }

        public void SaveCurrentProfile()
        {
            if (CurrentProfile == null) return;
            
            string filePath = Path.Combine(profilesDirectoryPath, $"{CurrentProfile.SteamId}_Settings.json");
            string json = JsonConvert.SerializeObject(CurrentProfile, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}