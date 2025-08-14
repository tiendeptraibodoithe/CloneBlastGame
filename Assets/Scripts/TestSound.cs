using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSound : MonoBehaviour
{
    public AudioClip testClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
        audioSource.loop = false;

        // Phát thử khi bắt đầu game
        if (testClip != null)
        {
            audioSource.PlayOneShot(testClip);
            Debug.Log("Phát âm thanh test!");
        }
        else
        {
            Debug.LogError("Chưa gán testClip!");
        }
    }
}