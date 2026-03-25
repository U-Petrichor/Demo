/*
    此脚本用于解决GraphicsPanel下各个控件的存储与游戏初始化时加载
    - 分辨率
    - 窗口模式
    - FPS
    - 垂直同步
*/

using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    public static GraphicsManager Instance { get; private set; }

    public Resolution[] SupportedResolutions { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SupportedResolutions = Screen.resolutions;
        SafeLoadGraphicsSettings();
    }

    /// <summary>
    /// 安全加载图形设置，确保在不支持的分辨率下使用默认值
    /// </summary>
    private void SafeLoadGraphicsSettings()
    {
        // 1. 获取当前分辨率，并且设置为默认值
        Resolution nativeRes = Screen.currentResolution;
        int defaultWidth = nativeRes.width;
        int defaultHeight = nativeRes.height;

        // 2. 尝试获取上次保存的分辨率，若没有就使用默认值
        int savedWidth = PlayerPrefs.GetInt("ResWidth", defaultWidth); 
        int savedHeight = PlayerPrefs.GetInt("ResHeight", defaultHeight);

        // 3. 尝试获取上次保存的“窗口模式”、“目标FPS”、“垂直同步”，若没有就取默认值
        FullScreenMode savedMode = (FullScreenMode)PlayerPrefs.GetInt("FullScreenMode", 1); 
        int savedTargetFPS = PlayerPrefs.GetInt("TargetFPS", -1); // 获取保存的FPS值
        int savedVSync = PlayerPrefs.GetInt("VSync", 1);  // 获取垂直同步的设置

        bool isSupported = false;
        
        // 4. 开始遍历当前显示器支持的分辨率，上次的保存值是否存在于本显示器支持的分辨率中（防止更换显示器导致的错误）
        foreach (Resolution res in SupportedResolutions)
        {
            if (res.width == savedWidth && res.height == savedHeight)
            {
                isSupported = true;
                break;
            }
        }
        if (!isSupported)
        {
            savedWidth = defaultWidth;
            savedHeight = defaultHeight;
            savedMode = FullScreenMode.FullScreenWindow; 
        }

        // 5. 获取最高帧率，防止提高分辨率导致的最高帧率降低
        RefreshRate bestRefreshRate = GetHighestRefreshRate(savedWidth, savedHeight); 
        
        Screen.SetResolution(savedWidth, savedHeight, savedMode, bestRefreshRate);
        Application.targetFrameRate = savedTargetFPS; 
        QualitySettings.vSyncCount = savedVSync; 
    }
    #region 提供给GraphicsUI调用的用于修改的方法
    public void SetAndSaveResolution(int width, int height)
    {
        RefreshRate bestRefreshRate = GetHighestRefreshRate(width, height); // 同一台设备在不同分辨率下支持的最大刷新率不同，所以要重新获取最大刷新率，再设置
        Screen.SetResolution(width, height, Screen.fullScreenMode, bestRefreshRate);
        
        PlayerPrefs.SetInt("ResWidth", width);
        PlayerPrefs.SetInt("ResHeight", height);
        PlayerPrefs.Save();
    }

    public void SetAndSaveFullScreenMode(FullScreenMode mode)
    {
        Screen.fullScreenMode = mode;
        PlayerPrefs.SetInt("FullScreenMode", (int)mode);
        PlayerPrefs.Save();
    }

    public void SetAndSaveTargetFPS(int targetFPS)
    {
        Application.targetFrameRate = targetFPS;
        PlayerPrefs.SetInt("TargetFPS", targetFPS);
        PlayerPrefs.Save();
    }

    public void SetAndSaveVSync(bool isOn)
    {
        int vSyncValue = isOn ? 1 : 0;
        QualitySettings.vSyncCount = vSyncValue;
        
        PlayerPrefs.SetInt("VSync", vSyncValue);
        PlayerPrefs.Save();
    }
    #endregion
    private RefreshRate GetHighestRefreshRate(int width, int height)
    {
        RefreshRate highestRate = new RefreshRate() { numerator = 0, denominator = 1 };
        double maxRateValue = 0;

        foreach (Resolution res in SupportedResolutions)
        {
            if (res.width == width && res.height == height) // 先找到我们需要的分辨率
            {
                if (res.refreshRateRatio.value > maxRateValue) // 再找到最大帧率
                {
                    maxRateValue = res.refreshRateRatio.value;
                    highestRate = res.refreshRateRatio;
                }
            }
        }
        return maxRateValue == 0 ? Screen.currentResolution.refreshRateRatio : highestRate;
    }
}