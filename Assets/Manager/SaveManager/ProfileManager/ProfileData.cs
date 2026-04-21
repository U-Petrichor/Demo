using System;
using System.Collections.Generic;

// 命名空间对齐你的根目录
namespace Manager.SaveManager     
{
    [Serializable]
    public class ProfileData
    {
        public string SteamId;   
        public string SteamName;  
        public string CreationDate;
        public string LastLoginDate; 

        public float MasterVolume = 1.0f;
        public float BgmVolume = 1.0f;
        public float SfxVolume = 1.0f;
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;
        public bool IsFullScreen = true;

        public string LastPlayedSlot = "";

        // 统一修改为大驼峰
        public Dictionary<string, SlotSummary> SlotSummaries = new Dictionary<string, SlotSummary>();

        public ProfileData() { }

        public ProfileData(string steamId, string steamName)
        {
            this.SteamId = steamId;
            this.SteamName = steamName;
            this.CreationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.LastLoginDate = this.CreationDate;
            this.SlotSummaries = new Dictionary<string, SlotSummary>();
        }
    }

    [Serializable]
    public class SlotSummary
    {
        public string SlotID;           
        public int TotalRuns;           
        public int ClearCount;          
        public float AchievementPercent;
        public float CollectionPercent; 
        public string LastSaveTime;     
    }
}