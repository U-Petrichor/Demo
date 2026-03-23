using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    // 【架构核心】单例模式，方便全宇宙的脚本呼唤它
    public static SettingsManager Instance { get; private set; }
    private GameObject settingsPanel;
    private GameObject LanguagePanel;
    private GameObject ControlPanel;
    private GameObject AudioPanel;
    private GameObject GraphicPanel;

    private TMP_Dropdown languageDropdown;

    private void Awake()
    {
        // 1. 初始化单例，确保跨场景不被销毁
        if (Instance != null && Instance != this)
        {
            // 存在其他实例时，销毁当前实例
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        settingsPanel = transform.Find("SettingsPanel").gameObject;

        Transform Settings = settingsPanel.transform.Find("Settings");
        LanguagePanel = Settings.Find("LanguagePanel").gameObject;
        ControlPanel = Settings.Find("ControlPanel").gameObject;
        AudioPanel = Settings.Find("AudioPanel").gameObject;
        GraphicPanel = Settings.Find("GraphicPanel").gameObject;

        Transform Bar = settingsPanel.transform.Find("Bar");
        Toggle ToggleLanguage = Bar.Find("Content/Language").GetComponent<Toggle>();
        Toggle ToggleControl = Bar.Find("Content/Control").GetComponent<Toggle>();
        Toggle ToggleAudio = Bar.Find("Content/Audio").GetComponent<Toggle>();
        Toggle ToggleGraphic = Bar.Find("Content/Graphic").GetComponent<Toggle>();

        ToggleLanguage.onValueChanged.AddListener((isOn) => { if (isOn) SwitchSettingsPage(LanguagePanel); });
        ToggleControl.onValueChanged.AddListener((isOn) => { if (isOn) SwitchSettingsPage(ControlPanel); });
        ToggleAudio.onValueChanged.AddListener((isOn) => { if (isOn) SwitchSettingsPage(AudioPanel); });
        ToggleGraphic.onValueChanged.AddListener((isOn) => { if (isOn) SwitchSettingsPage(GraphicPanel); });

        // 默认初始化到第一页
        ToggleLanguage.isOn = true;
        SwitchSettingsPage(LanguagePanel);

        // 4. 绑定具体设置项 (语言下拉菜单)
        languageDropdown = LanguagePanel.transform.Find("Content/Language/Wrapper/Dropdown").GetComponent<TMP_Dropdown>();
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

        // 游戏启动时，默认隐藏整个设置菜单
        CloseSettings();
    }

    // --- 供外部调用的核心方法 ---

    /// <summary>
    /// 打开设置界面
    /// </summary>
    public void OpenSettings()
    {
        foreach (Transform child in transform)
        {
            // 选择激活每一个子物体，否则关闭Canvas_Settings_Menu之后无法再次打开
            child.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 关闭设置界面
    /// </summary>
    public void CloseSettings()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // --- 内部 UI 逻辑 ---

    private void SwitchSettingsPage(GameObject page)
    {
        LanguagePanel.SetActive(false);
        ControlPanel.SetActive(false);
        AudioPanel.SetActive(false);
        GraphicPanel.SetActive(false);
        
        page.SetActive(true);
    }

    private void OnLanguageChanged(int index)
    {
        if (LocalizationSettings.InitializationOperation.IsDone)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }
    }
}