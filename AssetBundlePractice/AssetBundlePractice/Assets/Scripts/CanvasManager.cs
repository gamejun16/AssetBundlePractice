using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private Transform album;
    [SerializeField] private Transform photo;
    [SerializeField] private Transform video;
    
    private void Start()
    {
        album.gameObject.SetActive(false);
        photo.gameObject.SetActive(false);
        video.gameObject.SetActive(false);
    }

    public void On(int numb)
    {
        if(numb == 0) // album
        {
            album.gameObject.SetActive(!album.gameObject.activeSelf);
            if (!album.gameObject.activeSelf)
                Camera.main.GetComponent<AudioSource>().Stop();
        }

        if(numb == 1) // photo
        {
            photo.gameObject.SetActive(!photo.gameObject.activeSelf);
        }

        if(numb == 2) // video
        {
            video.gameObject.SetActive(!video.gameObject.activeSelf);
        }

        if(numb == -1)
        {
            Application.Quit();
        }
    }
}
