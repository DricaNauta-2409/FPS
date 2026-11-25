using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip muisic;
    [SerializeField] private float musicVolume;

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PlayMusic(muisic, musicVolume);
    }

 
}
