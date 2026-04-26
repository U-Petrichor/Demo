using System;
using System.Collections.Generic;

// 命名空间对齐你的根目录
namespace Manager.SaveManager
{
    [Serializable]
    public class ProfileData
    {
        public ulong SteamId; // SteamID64 通常是 ulong 类型
        public long CreationDateTicks; // 创建时期
        public long LastLoginDateTicks;  // 上次游玩时期
        public string LastPlayedSlot = ""; // 上次游玩的存档类型

        public Dictionary<string, SlotSummary> SlotSummaries = new Dictionary<string, SlotSummary>();

        public ProfileData() { }

        public ProfileData(ulong steamId)
        {
            this.SteamId = steamId;
            this.CreationDateTicks = DateTime.UtcNow.Ticks;
            this.LastLoginDateTicks = this.CreationDateTicks;
            this.SlotSummaries = new Dictionary<string, SlotSummary>();
        }
    }

    [Serializable]
    public class SlotSummary
    {
        public string SlotId;       
        public int SlotIndex;
        public int TotalRuns;
        public int ClearCount;
        public float AchievementPercent;
        public float CollectionPercent;
        public long LastSaveTimeTicks;
    }

}