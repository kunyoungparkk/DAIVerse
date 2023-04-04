using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioClip aiFinish;
    public AudioSource bgmAudio;

    AudioSource myAudio;
    private void Awake()
    {
        if(instance == null)
            instance = this;
    }
    private void Start()
    {
        myAudio = GetComponent<AudioSource>();
    }
    public void OnAIEnd()
    {
        myAudio.clip = aiFinish;
        myAudio.Play();
    }

    public void StartBGM()
    {
        bgmAudio.Play();
    }
    public void EndBGM()
    {
        bgmAudio.Stop();
    }
}
