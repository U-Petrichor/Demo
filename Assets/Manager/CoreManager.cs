using System.Collections;
using UnityEngine;
using Steamworks;
using Manager.SaveManager;

namespace Manager
{
    public class CoreManager : MonoBehaviour
    {
        public static CoreManager Instance { get; private set; }
        [SerializeField] private SaveCorruptedDialog corruptedDialogUI;
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
            // 确保 SteamManager 已经初始化
            yield return new WaitUntil(() => SteamManager.Initialized);
            ulong steamId = Steamworks.SteamUser.GetSteamID().m_SteamID;
            ProfileLoadStatus status = ProfileManager.Instance.InitializeAccount(steamId, out string errorPath);
            // 可以改成!=测试
            if (status == ProfileLoadStatus.Corrupted)
            {
                corruptedDialogUI.ShowDialog(
                    errorPath,
                    () => ProfileManager.Instance.ForceCreateNewProfile(steamId),
                    () => Application.Quit()    
                );
            }
            else
            {
                // 成功或备份恢复，正常走完流程
                Debug.Log("[CoreManager] 核心系统初始化完毕。");
            }
        }
    }
}