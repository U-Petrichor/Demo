using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
// 为了使用Localization实现Dropdown在不同语言下呈现不同内容，需要引用
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class GraphicsUI : MonoBehaviour
{
    private TMP_Dropdown _fullscreenDropdown;
    private TMP_Dropdown _resolutionDropdown;
    private TMP_Dropdown _fpsDropdown;
    private Toggle _vSyncToggle;

    private List<Vector2Int> _uniqueResolutions = new List<Vector2Int>();
    private readonly string _tableName = "Settings_Menu"; // Localization表

    private readonly int[] _fpsOptions = new int[] { -1, 360, 240, 165, 144, 120, 90, 75, 60, 30 };

    private bool _isInitializing = false;
    private bool _isLanguageDirty = false; // 下拉表是否过期

    private void Awake()
    {
        Transform contentObj = transform.Find("Content");

        if (contentObj != null)
        {
            _fullscreenDropdown = contentObj.Find("FullScreen/Wrapper/Dropdown").GetComponent<TMP_Dropdown>();
            _resolutionDropdown = contentObj.Find("Resolution/Wrapper/Dropdown").GetComponent<TMP_Dropdown>();
            _fpsDropdown = contentObj.Find("FPS/Wrapper/Dropdown").GetComponent<TMP_Dropdown>();
            _vSyncToggle = contentObj.Find("V-Sync/Toggle").GetComponent<Toggle>();
        }
        else
        {
            Debug.LogError("[GraphicsUI] 找不到 Content 节点！");
        }
    }

    private void Start()
    {
        if (_fullscreenDropdown == null) return;

        _isInitializing = true;

        InitFullscreenDropdown(); // 窗口模式下拉菜单
        InitResolutionDropdown(); // 分辨率下拉菜单
        InitFPSDropdown(); // 帧率下拉菜单
        InitVSyncToggle(); // 垂直同步 开关

        _isInitializing = false;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged; // 在Start里面订阅，保证能一直跟踪语言切换事件
    }
    private void OnDestroy() => LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged; // 组件销毁时取消订阅
    private void OnLocaleChanged(Locale newLocale) => _isLanguageDirty = true; // 标记脏，需刷新
    private void OnEnable()
    {
        // 当点击Graphics按钮时才会考虑更新UI，从而节省性能
        if (_isLanguageDirty && _fullscreenDropdown != null)
        {
            _isInitializing = true;
            InitFullscreenDropdown();
            InitFPSDropdown();
            _isInitializing = false;
            _isLanguageDirty = false;
            Debug.Log("[GraphicsUI] 发现脏标记，已懒加载重绘 UI！");
        }
    }
    // ================= 窗口模式 =================
    private void InitFullscreenDropdown()
    {
        _fullscreenDropdown.ClearOptions();
        List<string> options = new List<string>
        {
            // LocalizationSettings.StringDatabase.GetLocalizedString(_tableName, "KeyName"),
            LocalizationSettings.StringDatabase.GetLocalizedString(_tableName, "GraphicsPanel_FullScreen_Dropdown_FS_0"),
            LocalizationSettings.StringDatabase.GetLocalizedString(_tableName, "GraphicsPanel_FullScreen_Dropdown_FS_1"),
            LocalizationSettings.StringDatabase.GetLocalizedString(_tableName, "GraphicsPanel_FullScreen_Dropdown_FS_2")
        };
        _fullscreenDropdown.AddOptions(options);

        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) _fullscreenDropdown.value = 0;
        else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) _fullscreenDropdown.value = 1;
        else _fullscreenDropdown.value = 2;

        _fullscreenDropdown.RefreshShownValue();
        _resolutionDropdown.interactable = (_fullscreenDropdown.value != 1);
        _fullscreenDropdown.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void OnFullscreenChanged(int dropdownIndex)
    {
        if (_isInitializing) return;
        FullScreenMode mode = FullScreenMode.Windowed;
        switch (dropdownIndex)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: mode = FullScreenMode.FullScreenWindow; break;
            case 2: mode = FullScreenMode.Windowed; break;
        }
        GraphicsManager.Instance.SetAndSaveFullScreenMode(mode);
        _resolutionDropdown.interactable = (dropdownIndex != 1);
        Debug.Log("OnFullscreenChanged");
    }
    // ================= 分辨率 =================
    private void InitResolutionDropdown()
    {
        _resolutionDropdown.ClearOptions();
        _uniqueResolutions.Clear();

        Resolution[] allResolutions = GraphicsManager.Instance.SupportedResolutions;
        List<string> options = new List<string>();
        int currentResIndex = 0;

        // 将所有分辨率选项从高到低排列
        for (int i = allResolutions.Length - 1; i >= 0; i--)
        {
            Vector2Int resSize = new Vector2Int(allResolutions[i].width, allResolutions[i].height);
            if (!_uniqueResolutions.Contains(resSize))
            {
                _uniqueResolutions.Add(resSize);
                options.Add($"{resSize.x} x {resSize.y}");
                if (resSize.x == Screen.width && resSize.y == Screen.height)
                {
                    currentResIndex = _uniqueResolutions.Count - 1;
                }
            }
        }
        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResIndex;
        _resolutionDropdown.RefreshShownValue();
        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void OnResolutionChanged(int resIndex)
    {
        if (_isInitializing) return;
        Vector2Int selectedRes = _uniqueResolutions[resIndex];
        GraphicsManager.Instance.SetAndSaveResolution(selectedRes.x, selectedRes.y);
    }

    // ================= 帧率限制 (FPS) =================
    private void InitFPSDropdown()
    {
        _fpsDropdown.ClearOptions();

        string strUnlimited = LocalizationSettings.StringDatabase.GetLocalizedString(_tableName, "GraphicsPanel_FullScreen_Dropdown_FPS_Unlimited");
        if (string.IsNullOrEmpty(strUnlimited))
        {
            strUnlimited = "无限制"; // 兜底
            Debug.LogWarning($"[GraphicsUI] 找不到本地化键值 'GraphicsPanel_FullScreen_Dropdown_FPS_Unlimited' ！请检查 Localization Table。");
        }
        string fpsFormat = LocalizationSettings.StringDatabase.GetLocalizedString(_tableName, "GraphicsPanel_FullScreen_Dropdown_FPS_0");
        if (string.IsNullOrEmpty(fpsFormat))
        {
            fpsFormat = "{0} FPS";  // 兜底
            Debug.LogWarning($"[GraphicsUI] 找不到本地化键值 'GraphicsPanel_FullScreen_Dropdown_FPS_0' ！请检查 Localization Table。");
        }
        List<string> options = new List<string>();
        for (int i = 0; i < _fpsOptions.Length; i++)
        {
            if (_fpsOptions[i] == -1) options.Add(strUnlimited);
            else options.Add(string.Format(fpsFormat, _fpsOptions[i]));
        }
        _fpsDropdown.AddOptions(options);

        int currentFPS = Application.targetFrameRate;

        // 按照用户需求，帧率从高到低排列
        int targetIndex = 0;

        for (int i = 0; i < _fpsOptions.Length; i++)
        {
            if (currentFPS == _fpsOptions[i])
            {
                targetIndex = i;
                break;
            }
        }
        _fpsDropdown.value = targetIndex;
        _fpsDropdown.RefreshShownValue();
        _fpsDropdown.onValueChanged.AddListener(OnFPSChanged);
    }

    private void OnFPSChanged(int index)
    {
        if (_isInitializing) return;
        int selectedFPS = _fpsOptions[index];
        GraphicsManager.Instance.SetAndSaveTargetFPS(selectedFPS);
    }

    // ================= 4. 垂直同步 (V-Sync) =================
    private void InitVSyncToggle()
    {
        _vSyncToggle.isOn = (QualitySettings.vSyncCount > 0);
        _vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
    }

    private void OnVSyncChanged(bool isOn)
    {
        if (_isInitializing) return;
        GraphicsManager.Instance.SetAndSaveVSync(isOn);
    }

}