using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DisplayPhoto : MonoBehaviour
{
    private string rootFolderPath;
    public Transform displayRoot;
    public GameObject displayPiece;

    void Start()
    {
        rootFolderPath = $"{Application.dataPath}/Resources/Photos/";
        Display();
    }

    private void OnEnable()
    {
        
    }

    private void Display()
    {
        // # 1. 전시할 이미지 개수 확인
        var v = DataContainer.instance.Photos;
        
        // # 2. 이미지 개수만큼 전시 공간 확보
        for(int i=0;i<v.Count; i++)
            Instantiate(displayPiece, displayRoot).GetComponent<Image>().sprite = v[i];
    }
}
