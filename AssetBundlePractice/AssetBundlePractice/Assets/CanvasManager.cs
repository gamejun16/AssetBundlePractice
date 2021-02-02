using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private Transform photo;
    [SerializeField] private Transform album;


    private void Start()
    {
        photo.gameObject.SetActive(false);
        album.gameObject.SetActive(false);
    }

    public void On(int numb)
    {
        if(numb == 0) // photo
        {
            photo.gameObject.SetActive(!photo.gameObject.activeSelf);
        }

        if(numb == 1) // album
        {
            album.gameObject.SetActive(!album.gameObject.activeSelf);
            if (!album.gameObject.activeSelf)
                Camera.main.GetComponent<AudioSource>().Stop();
        }
    }
}
