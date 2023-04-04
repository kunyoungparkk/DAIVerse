using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
public class VoiceNetworkManager : MonoBehaviour
{
    public static VoiceNetworkManager instance;

    PhotonVoiceNetwork photonVoiceNetwork;
    Recorder recorder;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        photonVoiceNetwork = GetComponent<PhotonVoiceNetwork>();
        recorder = GetComponent<Recorder>();
    }
    public void RegisterSpeackerPrefab(GameObject prefab)
    {
        photonVoiceNetwork.SpeakerPrefab = prefab;
        //recorder.StartRecording();
    }
    public void Mute(bool isMute)
    {
        if (isMute == true)
            recorder.StopRecording();
        else
            recorder.StartRecording();
        //recorder.IsRecording = !isMute;
    }
}
