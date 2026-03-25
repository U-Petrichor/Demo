using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    // 【架构核心】单例模式，方便全宇宙的脚本呼唤它
    public static SettingsManager Instance { get; private set; }
    private GameObject _settingsPanel;
    private GameObject _languagePanel;
    private GameObject _controlsPanel;
    private GameObject _audioPanel;
    private GameObject _graphicsPanel;

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

        _settingsPanel = transform.Find("SettingsPanel").gameObject;

        Transform Settings = _settingsPanel.transform.Find("Settings");
        _languagePanel = Settings.Find("LanguagePanel").gameObject;
        _controlsPanel = Settings.Find("ControlsPanel").gameObject;
        _audioPanel = Settings.Find("AudioPanel").gameObject;
        _graphicsPanel = Settings.Find("GraphicsPanel").gameObject;

        Transform Bar = _settingsPanel.transform.Find("Bar");
        Toggle ToggleLanguage = Bar.Find("Content/Language").GetComponent<Toggle>();
        Toggle ToggleControls = Bar.Find("Content/Controls").GetComponent<Toggle>();
        Toggle ToggleAudio = Bar.Find("Content/Audio").GetComponent<Toggle>();
        Toggle ToggleGraphics = Bar.Find("Content/Graphics").GetComponent<Toggle>();

        ToggleLanguage.onValueChanged.AddListener((isOn) => { if (isOn) SwitchSettingsPage(_languagePanel); });
        ToggleControls.onValueChanged.AddListener((isOn) => { if (isOn) SwitchSettingsPage(_controlsPanel); });
        ToggleAudio.onValueChanged.AddListener((isOn) => { if (isOn) SwitchSettingsPage(_audioPanel); });
        ToggleGraphics.onValueChanged.AddListener((isOn) => { if (isOn) SwitchSettingsPage(_graphicsPanel); });

        // 默认初始化到第一页
        ToggleLanguage.isOn = true;
        SwitchSettingsPage(_languagePanel);

        // 4. 绑定具体设置项 (语言下拉菜单)
        languageDropdown = _languagePanel.transform.Find("Content/Language/Wrapper/Dropdown").GetComponent<TMP_Dropdown>();
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
        _languagePanel.SetActive(false);
        _controlsPanel.SetActive(false);
        _audioPanel.SetActive(false);
        _graphicsPanel.SetActive(false);
        
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