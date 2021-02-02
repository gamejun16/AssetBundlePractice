using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    private List<Sprite> photos;
    public List<Sprite> Photos { get { return photos; } }

    [SerializeField]
    private List<AudioClip> albums;
    public List<AudioClip> Albums { get { return albums; } }
}
