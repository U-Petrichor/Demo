using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIInteract : MonoBehaviour
{
    public void OnStartGameClicked()
    {
        SceneManager.LoadScene("Game");
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
        Debug.Log("打开设置面板...");
        // 缺 弹出设置面板
    }

    public void OnExitGameClicked()
    {
        Debug.Log("执行退出游戏命令！(提示：在 Unity 编辑器中点击只会打印这行字，打包成 exe 后才会真正关闭窗口)");
        // 缺 弹窗询问是否真的关闭
        Application.Quit();
    }
}