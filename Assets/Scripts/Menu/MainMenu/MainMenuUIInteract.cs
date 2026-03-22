using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIInteract : MonoBehaviour
{
    private GameObject startSubmenu;
    private GameObject SettingsSubmenu;

    // Canvas->SettingsSubmenu->SettingsPanel->Settings下的四个Panel
    private GameObject LanguagePanel;
    private GameObject ControlPanel;
    private GameObject AudioPanel;
    private GameObject GraphicPanel;
    private void Awake()
    {
        startSubmenu = transform.Find("StartSubmenu").gameObject;
        SettingsSubmenu = transform.Find("SettingsSubmenu").gameObject;
        // 找到并绑定Settings下的四个Panel
        Transform Settings = SettingsSubmenu.transform.Find("SettingsPanel/Settings"); // 因为SettingsSubmenu和各个面板相隔一个层级，所以要先找到SettingsPane
        LanguagePanel = Settings.Find("LanguagePanel").gameObject;
        ControlPanel = Settings.Find("ControlPanel").gameObject;
        AudioPanel = Settings.Find("AudioPanel").gameObject;
        GraphicPanel = Settings.Find("GraphicPanel").gameObject;
        // 找到并绑定SettingsPanel下的Bar下的四个Button
        Transform Bar = SettingsSubmenu.transform.Find("SettingsPanel/Bar");
        Toggle ToggleLanguage = Bar.Find("Content/Language").GetComponent<Toggle>();
        Toggle ToggleControl = Bar.Find("Content/Control").GetComponent<Toggle>();
        Toggle ToggleAudio = Bar.Find("Content/Audio").GetComponent<Toggle>();
        Toggle ToggleGraphic = Bar.Find("Content/Graphic").GetComponent<Toggle>();

        ToggleLanguage.onValueChanged.AddListener((value) => SwitchSettingsPage(LanguagePanel));
        ToggleControl.onValueChanged.AddListener((value) => SwitchSettingsPage(ControlPanel));
        ToggleAudio.onValueChanged.AddListener((value) => SwitchSettingsPage(AudioPanel));
        ToggleGraphic.onValueChanged.AddListener((value) => SwitchSettingsPage(GraphicPanel));

        ToggleLanguage.isOn = true;
        SwitchSettingsPage(LanguagePanel);
    }
    public void OnStartGameClicked()
    {
        if (startSubmenu != null)
        {
            startSubmenu.SetActive(true);
        }
    }
    public void OnSinglePlayerClicked()
    {
        SceneManager.LoadScene("Game");
    }
    public void OnMultiplayerClicked()
    {
        SceneManager.LoadScene("TeamLobby");
    }
    public void OnTurorialClicked()
    {
        SceneManager.LoadScene("Tutorial");
    }
    public void OnLaboratoryClicked()
    {
        SceneManager.LoadScene("Laboratory");
    }
    public void OnSettingsClicked()
    {
        if (SettingsSubmenu != null)
        {
            SettingsSubmenu.SetActive(true);
        }
    }

    public void OnExitGameClicked()
    {
        Debug.Log("执行退出游戏命令！(提示：在 Unity 编辑器中点击只会打印这行字，打包成 exe 后才会真正关闭窗口)");
        // 缺 弹窗询问是否真的关闭
        Application.Quit();
    }
    public void OnCloseZoneClicked()
    {
        Debug.Log("点了");
        startSubmenu.SetActive(false);      
    }
    public void SwitchSettingsPage(GameObject page)
    {
        // 关闭所有Panel
        LanguagePanel.SetActive(false);
        ControlPanel.SetActive(false);
        AudioPanel.SetActive(false);
        GraphicPanel.SetActive(false);
        // 打开选中的Panel
        page.SetActive(true);
    }
}