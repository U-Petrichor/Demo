using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIInteract : MonoBehaviour
{
    private GameObject startSubmenu;

    private void Awake()
    {
        startSubmenu = transform.Find("StartSubmenu").gameObject;
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
        // 核心改变：直接呼叫独立的单例管理器！
        SettingsManager.Instance.OpenSettings();
    }

    public void OnExitGameClicked()
    {
        Debug.Log("执行退出游戏命令！");
        //Application.Quit();
    }

    public void OnCloseZoneClicked()
    {
        startSubmenu.SetActive(false);      
    }
}