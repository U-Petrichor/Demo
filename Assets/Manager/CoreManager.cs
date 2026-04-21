using System.Collections;
using UnityEngine;

// 【修复 Bug 1 & 2】：必须引入 Steamworks
using Steamworks; 

// 【修复 Bug 3】：只需要 using 这个统一的命名空间，
// 就能直接访问里面的 ProfileManager、GlobalSaveManager 和 RunSaveManager
using Manager.SaveManager;

namespace Manager
{
    public class CoreManager : MonoBehaviour
    {
        public static CoreManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            StartCoroutine(InitializationSaveManager());
        }

        private IEnumerator InitializationSaveManager()
        {
            Debug.Log("[CoreManager] 阶段 1：等待 Steam SDK 就绪...");
            
            // 确保 SteamManager 已经在场景中其他地方被正确挂载和初始化
            yield return new WaitUntil(() => SteamManager.Initialized);

            // 这里如果依然报错，说明你的工程可能没有正确导入 Steamworks.NET，或者缺少 Assembly Definition 引用
            string steamId = SteamUser.GetSteamID().ToString();
            string steamName = SteamFriends.GetPersonaName();
            Debug.Log($"[CoreManager] Steam 已就绪。玩家: {steamName}");

            Debug.Log("[CoreManager] 阶段 2：初始化账号偏好系统...");
            
            // 【修改点】：因为命名空间已经清理干净，这里直接调用 ProfileManager 即可，不会再报不存在的错误
            ProfileManager.Instance.InitializeAccount(steamId, steamName);

            Debug.Log("[CoreManager] 核心系统初始化完毕。");
        }
    }
}