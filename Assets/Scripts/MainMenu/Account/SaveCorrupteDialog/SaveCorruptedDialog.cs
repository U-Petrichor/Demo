using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SaveCorruptedDialog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI errorPathText; 
    [SerializeField] private Button newSlotBtn; 
    [SerializeField] private Button fixBtn;     

    public void ShowDialog(string errorPath, Action onConfirmOverwrite, Action onQuitGame)
    {
        gameObject.SetActive(true);

        if (errorPathText != null)
        {
            errorPathText.text = errorPath;
        }

        // 清理并重新绑定按钮事件
        if (newSlotBtn != null) newSlotBtn.onClick.RemoveAllListeners();
        if (fixBtn != null) fixBtn.onClick.RemoveAllListeners();

        if (newSlotBtn != null) newSlotBtn.onClick.AddListener(() => 
        {
            onConfirmOverwrite?.Invoke();
            gameObject.SetActive(false); 
        });

        if (fixBtn != null) fixBtn.onClick.AddListener(() => 
        {
            onQuitGame?.Invoke();
        });
    }

    private void OnDestroy()
    {
        if (newSlotBtn != null) newSlotBtn.onClick.RemoveAllListeners();
        if (fixBtn != null) fixBtn.onClick.RemoveAllListeners();
    }
}