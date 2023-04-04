using System;
using UnityEngine;
using UnityEngine.UI;
using FrostweepGames.Plugins.GoogleCloud.SpeechRecognition;
public class VoiceCommandManager : MonoBehaviour
{
    public static VoiceCommandManager instance;

    private GCSpeechRecognition _speechRecognition;

    private UIManager uiManager;
    private SocketManager socketManager;
    public float maxRecordTime;
    public float maxAdditionalTime;
    public float thresholdValue;

    public Image _voiceLevelImage;
    float recordTime = 0f;
    float additionalTime = 0f;

    public enum OPERATION
    {
        SHOW_GRAPH_2D_1 = 0,
        SHOW_GRAPH_2D_2 = 1,
        SHOW_GRAPH_2D_3 = 2,
        SHOW_GRAPH_2D_4 = 3,
        SHOW_GRAPH_2D_5 = 4,
        SHOW_GRAPH_2D_6 = 5,
        SHOW_GRAPH_2D_7 = 6,
        SHOW_GRAPH_2D_8 = 7,
        SHOW_GRAPH_2D_9 = 8,
        SHOW_GRAPH_2D_10 = 9,
        SHOW_GRAPH_3D_1 = 10,
        SHOW_GRAPH_3D_2 = 11,
        SHOW_GRAPH_3D_3 = 12,
        SHOW_GRAPH_3D_4 = 13,
        SHOW_GRAPH_3D_5 = 14,
        SHOW_GRAPH_3D_6 = 15,
        SHOW_GRAPH_3D_7 = 16,
        SHOW_GRAPH_3D_8 = 17,
        SHOW_GRAPH_3D_9 = 18,
        SHOW_GRAPH_3D_10 = 19,
        PREV_SLIDE = 20,
        NEXT_SLIDE = 21,
        POINT_ON = 22,
        POINT_OFF = 23,
        BGM_ON = 24,
        BGM_OFF = 25,
        GUEST_MUTE_ON = 26,
        GUEST_MUTE_OFF = 27,
        HIDE_GRAPH_2D = 28,
        HIDE_GRAPH_3D = 29
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        uiManager = UIManager.instance;
        socketManager = SocketManager.instance;

        _speechRecognition = GCSpeechRecognition.Instance;
        _speechRecognition.RecognizeSuccessEvent += RecognizeSuccessEventHandler;
        _speechRecognition.RecognizeFailedEvent += RecognizeFailedEventHandler;

        _speechRecognition.FinishedRecordEvent += FinishedRecordEventHandler;
        _speechRecognition.StartedRecordEvent += StartedRecordEventHandler;
        _speechRecognition.RecordFailedEvent += RecordFailedEventHandler;

        _speechRecognition.EndTalkigEvent += EndTalkigEventHandler;

        _speechRecognition.RequestMicrophonePermission(null);
        //Detect Threshold 하면 photonvoice recorder 꺼짐..
        //_speechRecognition.DetectThreshold();
        // select first microphone device -> 혹시 오류난다면 참고하기
        if (_speechRecognition.HasConnectedMicrophoneDevices())
        {
            _speechRecognition.SetMicrophoneDevice(_speechRecognition.GetMicrophoneDevices()[0]);
        }
    }
    private void Update()
    {
        if (_speechRecognition.IsRecording)
        {
            if (_speechRecognition.GetMaxFrame() > 0)
            {
                float max = (float)_speechRecognition.configs[_speechRecognition.currentConfigIndex].voiceDetectionThreshold;
                float current = _speechRecognition.GetLastFrame() / max;
                //print(current);
                if (current >= thresholdValue)
                {
                    _voiceLevelImage.fillAmount = Mathf.Lerp(_voiceLevelImage.fillAmount, Mathf.Clamp(current / thresholdValue, 0, 1f), 30 * Time.deltaTime);
                }
                else
                {
                    _voiceLevelImage.fillAmount = Mathf.Lerp(_voiceLevelImage.fillAmount, Mathf.Clamp(current / thresholdValue, 0, 0.5f), 30 * Time.deltaTime);
                }

                //_voiceLevelImage.color = current >= 1f ? Color.green : Color.yellow;


                //기본 3 + 추가 3 + 이후로는 current가 일정수치 미만으로 떨어진경우
                if (recordTime > maxRecordTime)
                {
                    if (current > thresholdValue)
                    {
                        additionalTime = 0;
                        return;
                    }
                    else
                        additionalTime += Time.deltaTime;

                    if (additionalTime > maxAdditionalTime)
                    {
                        StopRecordButtonOnClickHandler();
                    }
                }
                else
                {
                    recordTime += Time.deltaTime;
                }

            }
        }
    }

    //파괴시 이벤트 핸들러제거
    private void OnDestroy()
    {
        _speechRecognition.RecognizeSuccessEvent -= RecognizeSuccessEventHandler;
        _speechRecognition.RecognizeFailedEvent -= RecognizeFailedEventHandler;

        _speechRecognition.FinishedRecordEvent -= FinishedRecordEventHandler;
        _speechRecognition.StartedRecordEvent -= StartedRecordEventHandler;
        _speechRecognition.RecordFailedEvent -= RecordFailedEventHandler;

        _speechRecognition.EndTalkigEvent -= EndTalkigEventHandler;
    }

    //시작시
    public void StartRecordButtonOnClickHandler()
    {
        uiManager.OnAIStarted();
        _speechRecognition.StartRecord(false);
    }
    //끝날 때
    public void StopRecordButtonOnClickHandler()
    {
        _speechRecognition.StopRecord();
        uiManager.OnAIStopped();
        _voiceLevelImage.fillAmount = 0f;
        recordTime = 0f;
        additionalTime = 0f;
    }

    private void StartedRecordEventHandler()
    {
        uiManager.OnAIStarted();
    }

    private void RecordFailedEventHandler()
    {
        print("Start record Failed. Please check microphone device and try again.");
        uiManager.OnAIStopped();
        _voiceLevelImage.fillAmount = 0f;
        recordTime = 0f;
        additionalTime = 0f;
    }

    private void EndTalkigEventHandler(AudioClip clip, float[] raw)
    {
        FinishedRecordEventHandler(clip, raw);
    }

    //Finish했을 때 보내는 부분
    private void FinishedRecordEventHandler(AudioClip clip, float[] raw)
    {
        if (uiManager.isAIOn == true)
        {
            uiManager.OnAIStopped();
        }

        if (clip == null)
            return;

        RecognitionConfig config = RecognitionConfig.GetDefault();
        config.languageCode = Enumerators.LanguageCode.ko_KR.Parse();
        config.audioChannelCount = clip.channels;
        // configure other parameters of the config if need

        GeneralRecognitionRequest recognitionRequest = new GeneralRecognitionRequest()
        {
            audio = new RecognitionAudioContent()
            {
                content = raw.ToBase64()
            },
            config = config
        };

        _speechRecognition.Recognize(recognitionRequest);
    }

    //실패시 ?? 가긴하는건가..
    private void RecognizeFailedEventHandler(string error)
    {
        print("Recognize Failed: " + error);
    }
    //성공시 처리
    private void RecognizeSuccessEventHandler(RecognitionResponse recognitionResponse)
    {
        if (recognitionResponse.results.Length <= 0)
        {
            print("voice recognition failed");
            return;
        }
        string command = recognitionResponse.results[0].alternatives[0].transcript;

        print(command);
        //소켓
        socketManager.SendAndReceive(command);
    }
    
    
}
