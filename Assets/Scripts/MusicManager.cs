using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource audioSource;

    [Header("Âm lượng")]
    public float menuVolume = 0.5f;   // Volume ở menu
    public float levelVolume = 0.2f;  // Volume ở level

    void Awake()
    {
        // Nếu đã có 1 MusicManager rồi thì xóa cái mới tạo
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        // Phát nhạc nếu chưa phát
        if (!audioSource.isPlaying)
        {
            audioSource.loop = true;
            audioSource.Play();
        }

        // Lắng nghe sự kiện đổi scene
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    // Hàm gọi khi đổi scene
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        // Nếu là scene menu
        if (newScene.name == "MainMenu")
        {
            audioSource.volume = menuVolume;
        }
        else // Các scene còn lại (Level1, Level2,...)
        {
            audioSource.volume = levelVolume;
        }
    }
}