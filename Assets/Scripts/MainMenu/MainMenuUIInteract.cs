using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUIInteract : MonoBehaviour
{
    [Header("Submenus")]
    [SerializeField] private GameObject startSubmenu;

    [Header("Controllers")]
    [Tooltip("拖入挂载了 AccountUIController 的物体")]
    [SerializeField] private AccountUIController accountUIController;

    [Header("Buttons")]
    // 将所有需要交互的按钮在这里声明
    [SerializeField] private Button startGameBtn;
    [SerializeField] private Button singlePlayerBtn;
    [SerializeField] private Button multiPlayerBtn;
    [SerializeField] private Button tutorialBtn;
    [SerializeField] private Button laboratoryBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Button closeZoneBtn;
    [SerializeField] private Button AccountBtn;

    private void Awake()
    {
        // 在代码中集中进行事件绑定
        BindEvents();
    }

    private void BindEvents()
    {
        // 使用 Lambda 表达式或直接传递方法名进行绑定
        if (startGameBtn != null) startGameBtn.onClick.AddListener(() => {
            if (startSubmenu != null) startSubmenu.SetActive(true);
        });

        if (singlePlayerBtn != null) singlePlayerBtn.onClick.AddListener(() => SceneManager.LoadScene("Game"));
        if (multiPlayerBtn != null) multiPlayerBtn.onClick.AddListener(() => SceneManager.LoadScene("TeamLobby"));
        if (tutorialBtn != null) tutorialBtn.onClick.AddListener(() => SceneManager.LoadScene("Tutorial"));
        if (laboratoryBtn != null) laboratoryBtn.onClick.AddListener(() => SceneManager.LoadScene("Laboratory"));
        if (settingsBtn != null) settingsBtn.onClick.AddListener(() => SettingsManager.Instance.OpenSettings());
        
        if (exitBtn != null) exitBtn.onClick.AddListener(() => {
            Debug.Log("执行退出游戏命令！");
            // Application.Quit();
        });

        if (closeZoneBtn != null) closeZoneBtn.onClick.AddListener(() => {
            if (startSubmenu != null) startSubmenu.SetActive(false);
        });
        if (AccountBtn != null) AccountBtn.onClick.AddListener(() => accountUIController.OpenSlotSelectionPanel());

    }

    private void OnDestroy()
    {
        // 良好的工程习惯：在对象销毁时注销事件，防止内存泄漏或空引用
        if (startGameBtn != null) startGameBtn.onClick.RemoveAllListeners();
        if (singlePlayerBtn != null) singlePlayerBtn.onClick.RemoveAllListeners();
        // ... (其他按钮同样处理)
    }
}