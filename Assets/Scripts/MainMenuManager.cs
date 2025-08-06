using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Thêm dòng này

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingPanel;

    // Hàm bắt đầu game
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); 
    }

    // Hiện menu cài đặt
    public void OpenSetting()
    {
        mainMenuPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    // Quay lại menu chính
    public void BackToMainMenu()
    {
        settingPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Thoát game
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Dành cho khi test trong Unity Editor
#endif
    }
}
