using System.Collections.Generic;

namespace Manager.SaveManager
{
    [System.Serializable]
    public class GlobalSaveData
    {
        public string SlotId;

        // 1. 进阶难度限制 (类似杀戮尖塔的进阶等级)
        public int MaxUnlockedAscensionLevel = 0; 

        // 2. 统计数据看板 (严格限定为四项)
        public GlobalStatistics Stats = new();

        // 3. 收集品/图鉴系统
        public List<string> CollectedItemIDs = new List<string>();

        // 4. 成就系统数据
        public List<AchievementData> Achievements = new List<AchievementData>();
        // 5. 游玩记录墙
        public List<RunHistoryRecord> RunHistories = new();
    }

    // --- 附带的子数据结构 ---

    [System.Serializable]
public class GlobalStatistics
{
    public int TotalRuns;          // 总游玩局数
    public int ClearCount;         // 成功通关次数
    public int TotalFloorsClimbed; // 总爬层数
    public int TotalKills;         // 总击杀数
    public int TotalBossesDefeated;// 总击杀Boss数
    public double TotalPlayTimeSeconds; 
}

    [System.Serializable]
    public class AchievementData
    {
        public string AchievementID; // 直接把 ID 作为字段写在数据内部
        public bool IsUnlocked;
        public long UnlockTimestamp;
        public float Progress; // 进度型成就
    }

    [System.Serializable]
    public class RunHistoryRecord
    {
        public int FinalFloor;
        public float DurationSeconds;
        
        // 记录装备与饰品的 ID 即可，读取时通过配置表还原名称或图标
        public List<string> EquippedArmorAndWeaponIDs = new();
        public List<string> EquippedAccessoryIDs = new();

        // 按层数顺序记录的关卡信息摘要
        public List<FloorSummary> FloorRecords = new();
    }

    [System.Serializable]
    public class FloorSummary
    {
        public int FloorNumber;
        public string FloorType; // 后续会给每一层弄随即元素，到时候在命名
        public string EncounteredBossName; // 如果不是 Boss 层，则留空
    }
}