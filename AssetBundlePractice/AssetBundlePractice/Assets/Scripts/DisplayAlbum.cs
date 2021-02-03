using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayAlbum : MonoBehaviour
{

    private string rootFolderPath;
    public Transform displayRoot;
    public GameObject displayPiece;

    void Start()
    {
        rootFolderPath = $"{Application.dataPath}/Resources/Albums/";
        Display();
    }

    private void OnEnable()
    {
        
    }

    

    private void Display()
    {
        // # 1. 전시할 앨범 개수 확인
        var v = DataContainer.instance.Albums;

        // # 2. 앨범 전시
        for (int i = 0; i < v.Count; i++)
        {
            Transform t = Instantiate(displayPiece, displayRoot).transform;
            t.GetChild(0).gameObject.SetActive(true);
            t.GetChild(0).GetComponent<MusicPlayer>().myClip = v[i];
            t.GetComponentInChildren<Text>().text = v[i].name;
        }
    }
}
