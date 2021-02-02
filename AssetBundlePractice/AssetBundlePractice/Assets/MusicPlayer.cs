using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip myClip;
    private AudioSource audioSourceOnCamera;

    private void Start()
    {
        audioSourceOnCamera = Camera.main.GetComponent<AudioSource>();
    }

    public void PlayMusic()
    {
        audioSourceOnCamera.clip = myClip;
        audioSourceOnCamera.Play();
    }
}
