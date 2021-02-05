using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DataContainer : MonoBehaviour
{
    #region singleton
    public static DataContainer instance;
    
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [SerializeField]
    private List<AudioClip> albums;
    public List<AudioClip> Albums { get { return albums; } }

    [SerializeField]
    private List<Sprite> photos;
    public List<Sprite> Photos { get { return photos; } }

    [SerializeField]
    private List<VideoClip> videos;
    public List<VideoClip> Videos { get { return videos; } }
    
    public void Init(Object[] albumAllAssets, Object[] photoAllAssets, Object[] videoAllAssets)
    {
        foreach (Object obj in albumAllAssets)
        {
            if (obj is AudioClip)
                albums.Add(obj as AudioClip);
        }

        foreach (Object obj in photoAllAssets)
        {
            if (obj is Sprite)
                photos.Add(obj as Sprite);
        }

        foreach(Object obj in videoAllAssets)
        {
            if (obj is VideoClip)
                videos.Add(obj as VideoClip);
        }
    }
    
}
