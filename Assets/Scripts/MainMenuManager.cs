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

    [Header("Âm thanh")]
    public Slider volumeSlider;
    public AudioMixer audioMixer;

    void Start()
    {
        // Gán sự kiện thay đổi volume
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);

            // Đặt giá trị khởi đầu (ví dụ: 0.5)
            float defaultVolume = PlayerPrefs.GetFloat("volume", 0.5f);
            volumeSlider.value = defaultVolume;
            SetVolume(defaultVolume);
        }
    }

    // Hàm bắt đầu game
    public void PlayGame()
    {
        SceneManager.LoadScene("Level1");
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
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Hàm chỉnh âm lượng
    public void SetVolume(float volume)
    {
        // Tránh lỗi Log10(0)
        float volumeDB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("Volume", volumeDB);
        PlayerPrefs.SetFloat("volume", volume); // Lưu lại nếu cần
    }
}
