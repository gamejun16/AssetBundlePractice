using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayController : MonoBehaviour
{
    public VideoClip myClip;
    private VideoPlayer screen;

    private void Start()
    {
        screen = GameObject.FindObjectOfType<DisplayVideo>().transform.GetChild(0).GetComponent<VideoPlayer>();
    }

    public void PlayVideo()
    {
        screen.clip = myClip;
        screen.Play();
    }
}
