using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class DisplayVideo : MonoBehaviour
{

    private string rootFolderPath;
    public Transform displayRoot;
    public GameObject displayPiece;

    void Start()
    {
        rootFolderPath = $"{Application.dataPath}/Resources/Videos/";
        Display();
    }


    private void Display()
    {
        // # 1. 전시할 비디오 개수 확인
        var v = DataContainer.instance.Videos;

        print($"v Count : {v.Count}");

        // # 2. 비디오
        for (int i = 0; i < v.Count; i++)
        {
            Transform t = Instantiate(displayPiece, displayRoot).transform;
            t.GetChild(1).gameObject.SetActive(true);
            t.GetChild(1).GetComponent<VideoPlayController>().myClip = v[i];
            t.GetChild(1).GetChild(0).GetComponent<Text>().text = v[i].name;
        }
    }
}
