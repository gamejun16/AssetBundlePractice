using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> bgms, effs;

    public void PlayBGM(int numb)
    {
        audioSource.clip = bgms[numb];
        audioSource.Play();
    }
    
    public void PlayEFF(int numb)
    {
        audioSource.clip = effs[numb];
        audioSource.Play();
    }
}
