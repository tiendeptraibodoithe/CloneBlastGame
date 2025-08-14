using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingPanel;
    public GameObject selectLevelPanel; // Panel chọn level

    [Header("Âm thanh")]
    public Slider volumeSlider;
    public AudioMixer audioMixer;

    void Start()
    {
        // Gán sự kiện thay đổi volume
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);

            float defaultVolume = PlayerPrefs.GetFloat("volume", 0.5f);
            volumeSlider.value = defaultVolume;
            SetVolume(defaultVolume);
        }

        // Ẩn panel level lúc khởi động
        if (selectLevelPanel != null) selectLevelPanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);
    }

    // Khi bấm nút Play
    public void PlayGame()
    {
        mainMenuPanel.SetActive(false);
        selectLevelPanel.SetActive(true);
    }

    // Chọn Level bất kỳ
    public void SelectLevel(int levelNumber)
    {
        string sceneName = "Level" + levelNumber;
        SceneManager.LoadScene(sceneName);
    }

    // Quay lại từ SelectLevelPanel
    public void BackFromSelectLevel()
    {
        if (selectLevelPanel != null) selectLevelPanel.SetActive(false); // Ẩn panel chọn level
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);       // Hiện panel menu chính
    }


    // Hiện menu cài đặt
    public void OpenSetting()
    {
        mainMenuPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    // Quay lại menu chính từ Setting
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
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Hàm chỉnh âm lượng
    public void SetVolume(float volume)
    {
        float volumeDB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("Volume", volumeDB);
        PlayerPrefs.SetFloat("volume", volume);
    }
}
