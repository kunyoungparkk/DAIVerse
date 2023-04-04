using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class UIManager : MonoBehaviourPunCallbacks
{
    public static UIManager instance;
    private void Awake()
    {
        instance = this;
    }
    public Transform uiTransform;
    public Transform[] positions;
    //voice on,off
    public Image voiceImage;
    public Sprite voiceOnImg;
    public Sprite voiceOffImg;
    bool isVoiceOn = true;
    VoiceNetworkManager voiceNetworkManager;
    //setting
    public Image settingImage;
    public GameObject settingPanel;
    Color activeMenuColor;
    Color passiveMenuColor;
    /*public Image heightChangeBtn;
    public Image idpwDisplayBtn;
    public GameObject heightChangeContent;
    public GameObject idpwDisplayContent;*/
    [SerializeField] Image[] menuBtnImgs;
    [SerializeField] GameObject[] menuBtnContents;
    public Text idpwText;
    //public text id;
    //public text pw;
    bool isSettingOn = false;
    //ai
    public Image aiImage;
    private VoiceCommandManager voiceCommandManager;
    public bool isAIOn = false;
    GraphManager graphManager;
    //change location
    bool isChangeLocationOn = false;
    public Image changeLocationImg;
    int myPositionIndex = -1;
    public GameObject[] changeLocationUIs;
    private Vector3[] changeLocationUIPositions;
    private CharacterManager characterManager;
    private NetworkManager networkManager;
    public GameObject portableCanvasModule;
    //mirror
    public Image mirrorImage;
    public GameObject mirror;
    bool isMirrorOn = false;
    private void Start()
    {
        voiceCommandManager = VoiceCommandManager.instance;
        voiceNetworkManager = VoiceNetworkManager.instance;
        characterManager = CharacterManager.instance;
        networkManager = NetworkManager.instance;
        graphManager = GraphManager.instance;

        //ui비활성화
        uiTransform.gameObject.SetActive(false);

        changeLocationUIPositions = new Vector3[changeLocationUIs.Length];
        for (int i = 0; i < changeLocationUIs.Length; i++)
        {
            changeLocationUIPositions[i] = changeLocationUIs[i].transform.localPosition;
        }
        //system menu
        activeMenuColor = menuBtnImgs[0].color;
        passiveMenuColor = menuBtnImgs[1].color;
    }
    public void OnClickVoice(bool isSelected)
    {
        if (!isSelected) return;
        //AI 켜져있으면 건들지못하게
        if (isAIOn)
            return;
        isVoiceOn = !isVoiceOn;
        //OFF->ON
        if (isVoiceOn)
        {
            voiceNetworkManager.Mute(false);
            voiceImage.sprite = voiceOnImg;
            voiceImage.color = new Color(0.764151f, 0.764151f, 0.764151f);
        }
        //ON->OFF
        else
        {
            voiceNetworkManager.Mute(true);
            voiceImage.sprite = voiceOffImg;
            voiceImage.color = new Color(0.764151f, 0, 0);
        }
    }
    public void VoiceOnOff(bool isOn)
    {
        //AI 켜져있으면 건들지못하게
        if (isAIOn)
            return;

        isVoiceOn = isOn;
        //OFF->ON
        if (isVoiceOn)
        {
            voiceNetworkManager.Mute(false);
            voiceImage.sprite = voiceOnImg;
            voiceImage.color = new Color(0.764151f, 0.764151f, 0.764151f);
        }
        //ON->OFF
        else
        {
            voiceNetworkManager.Mute(true);
            voiceImage.sprite = voiceOffImg;
            voiceImage.color = new Color(0.764151f, 0, 0);
        }
    }
    public void OnClickSetting(bool isSelected)
    {
        if (!isSelected) return;
        isSettingOn = !isSettingOn;
        if (isSettingOn)
        {
            settingImage.color = new Color(0.21f, 0.21f, 0.21f);
        }
        else
        {
            settingImage.color = new Color(0.764151f, 0.764151f, 0.764151f);
        }
        settingPanel.SetActive(isSettingOn);
    }
    public void OnClickMenuBtn(int index)
    {
        for(int i=0; i<menuBtnImgs.Length; i++)
        {
            if(i == index)
            {
                menuBtnImgs[i].color = activeMenuColor;
                menuBtnContents[i].SetActive(true);
            }
            else
            {
                menuBtnImgs[i].color = passiveMenuColor; ;
                menuBtnContents[i].SetActive(false);
            }
        }
        /*switch (index)
        {
            //height 조정
            case 0:
                //색상
                heightChangeBtn.color = activeMenuColor;
                idpwDisplayBtn.color = passiveMenuColor;
                //active
                heightChangeContent.SetActive(true);
                idpwDisplayContent.SetActive(false);
                break;
            //idpw
            case 1:
                heightChangeBtn.color = passiveMenuColor;
                idpwDisplayBtn.color = activeMenuColor;
                //active
                heightChangeContent.SetActive(false);
                idpwDisplayContent.SetActive(true);
                break;
            default:
                print("메뉴 버튼 에러");
                break;
        }*/
    }
    public void OnClickChangeHeight(bool isUp)
    {
        characterManager.ChangePlayerHeight(isUp);
    }
    //배경선택
    [Header("배경선택")]
    [SerializeField] MeshRenderer backgroundRenderer;
    [SerializeField] Material[] backgroundMats;
    public void OnClickChangeBackground(int index)
    {
        backgroundRenderer.material = backgroundMats[index];
    }
    public void OnClickQuit()
    {
        PhotonNetwork.Disconnect();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        uiTransform.gameObject.SetActive(true);
    }
    //ID / PW 출력
    public void DisplayUserInfo(string id, string pw)
    {
        idpwText.text = "ID:\t" + id + "\nPW:\t" + pw;
    }
    public void OnClickMirror(bool isSelected)
    {
        if (!isSelected) return;
        isMirrorOn = !isMirrorOn;
        if (isMirrorOn)
        {
            mirrorImage.color = new Color(0.21f, 0.21f, 0.21f);
        }
        else
        {
            mirrorImage.color = new Color(0.764151f, 0.764151f, 0.764151f);
        }
        mirror.SetActive(isMirrorOn);
    }

    //자리 이동 버튼 눌렀을 경우
    public void OnClickChangeLocation(bool isSelected)
    {
        if (!isSelected) return;
        //UI비활성화
        //StartCoroutine(SetActiveObject(uiTransform.gameObject, false));
        isChangeLocationOn = !isChangeLocationOn;
        if (isChangeLocationOn)
        {
            changeLocationImg.color = new Color(0.21f, 0.21f, 0.21f);
            //changeLocationUIs 활성화
            Vector3 playerHeadPosition = characterManager.GetPlayerHeadPosition();
            for (int i = 0; i < changeLocationUIs.Length; i++)
            {
                //이미 다른 사람이 앉은 자리는 제외
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(i))
                    continue;

                //플레이어 바라보기
                changeLocationUIs[i].transform.localPosition = changeLocationUIPositions[i];
                changeLocationUIs[i].transform.LookAt(playerHeadPosition, Vector3.up);
                changeLocationUIs[i].transform.Rotate(new Vector3(0, 180, 0), Space.Self);
                changeLocationUIs[i].transform.Translate(0, 0, -2, Space.Self);
                //활성화
                changeLocationUIs[i].SetActive(true);
            }
        }
        else
        {
            changeLocationImg.color = new Color(0.764151f, 0.764151f, 0.764151f);
            //changeLocationUIs 비활성화
            foreach (GameObject go in changeLocationUIs)
            {
                StartCoroutine(SetActiveObject(go, false));
            }
        }

    }
    //해당 index 자리로 이동
    public void ChangeLocation(int index)
    {
        //changeLocationUIs 비활성화
        /*foreach (GameObject go in changeLocationUIs)
        {
            StartCoroutine(SetActiveObject(go, false));
        }*/
        //자리 이동
        characterManager.ChangeCharacterLocation(index, myPositionIndex);
        ChangeUIPosition(index);

        //UI활성화
        //uiTransform.gameObject.SetActive(true);

    }

    public void OnClickAI(bool isSelected)
    {
        if (!isSelected) return;
        if (isAIOn)
        {
            voiceCommandManager.StopRecordButtonOnClickHandler();
        }
        else
        {
            voiceCommandManager.StartRecordButtonOnClickHandler();
        }
    }
    public void OnAIStopped()
    {
        if (isVoiceOn == false)
        {
            voiceNetworkManager.Mute(false);
            voiceImage.sprite = voiceOnImg;
            voiceImage.color = new Color(0.764151f, 0.764151f, 0.764151f);
            SoundManager.instance.OnAIEnd();
            isVoiceOn = true;
        }
        print("엔드");
        isAIOn = false;
        aiImage.color = new Color(0.764151f, 0.764151f, 0.764151f);
    }
    public void OnAIStarted()
    {
        if (isVoiceOn == true)
        {
            voiceNetworkManager.Mute(true);
            voiceImage.sprite = voiceOffImg;
            voiceImage.color = new Color(0.764151f, 0, 0);
            isVoiceOn = false;
        }
        print("스타트");
        isAIOn = true;
        aiImage.color = new Color(0.764151f, 0, 0);
    }
    public void ShowGraph(bool is3D, int playerID, int index)
    {
        DBManager.FileType fileType;
        if (is3D)
        {
            fileType = DBManager.FileType.graph_3D;
        }
        else
        {
            fileType = DBManager.FileType.graph_2D;
        }
        HideGraph(is3D);
        DBManager.instance.GetFile(fileType, playerID, index);
    }
    public void HideGraph(bool is3D)
    {
        graphManager.RemoveGraph(is3D);
    }
    public void ChangeUIPosition(int positionIndex)
    {
        myPositionIndex = positionIndex;
        graphManager.ChangeGraphPosition(myPositionIndex);
        uiTransform.position = positions[positionIndex].position;
        uiTransform.rotation = positions[positionIndex].rotation;
    }
    public int GetPositionIndex()
    {
        return myPositionIndex;
    }

    IEnumerator SetActiveObject(GameObject go, bool isActive)
    {
        yield return new WaitForEndOfFrame();
        go.SetActive(isActive);
    }

    [Header("그래프 - 번호 확인창")]
    [SerializeField] private GameObject graphInfoCanvas;
    [SerializeField] private Image graphInfoImage;
    [SerializeField] private Text[] graphTexts_2D;
    [SerializeField] private Text[] graphTexts_3D;

    [SerializeField] private GameObject graphInfo_LoadingPanel;
    [SerializeField] private GameObject graphInfo_ContentPanel;

    bool isGrpahInfoOn = false;
    int loadCount = 0;
    //창 열기
    public void OnClickGraphInfoBtn(bool isSelected)
    {
        if (!isSelected) return;
        isGrpahInfoOn = !isGrpahInfoOn;
        if (isGrpahInfoOn)
        {
            graphInfoImage.color = new Color(0.21f, 0.21f, 0.21f);
            graphInfoCanvas.SetActive(true);
            LoadGraphInfo();
        }
        else
        {
            graphInfoImage.color = new Color(0.764151f, 0.764151f, 0.764151f);
            graphInfoCanvas.SetActive(false);
        }
    }
    private void LoadGraphInfo()
    {
        loadCount = 0;
        graphInfo_LoadingPanel.SetActive(true);
        graphInfo_ContentPanel.SetActive(false);

        //받아와서 넣기
        DBManager.instance.GetGraphListToTextArr(graphTexts_2D, false);
        DBManager.instance.GetGraphListToTextArr(graphTexts_3D, true);
    }
    //로드 완료되면 띄우기
    public void OnLoadGraphInfo()
    {
        loadCount++;
        if (loadCount == 2)
        {
            graphInfo_LoadingPanel.SetActive(false);
            graphInfo_ContentPanel.SetActive(true);
        }
    }

    Coroutine currentCoroutine;
    public void StartLoadSocket()
    {
        currentCoroutine = StartCoroutine(SocketLoadingDelay(3.0f));
    }
    public void EndLoadSocket()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        loadingImage.fillAmount = 0.0f;
    }
    public Image loadingImage;
    IEnumerator SocketLoadingDelay(float delay)
    {
        float time = 0;
        while (time < delay)
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            loadingImage.fillAmount = time / delay;
        }
        yield return null;
    }
    bool isScriptOn = false;
    bool isScriptLoaded = false;
    [SerializeField] GameObject scriptObject;
    [SerializeField] Text scriptText;
    [SerializeField] ContentSizeFitter[] scriptSizeFiltters;
    public void OnClickScriptBtn()
    {
        isScriptOn = !isScriptOn;
        scriptObject.SetActive(isScriptOn);
        //scriptText.text = "로딩 중";
        //갖고오기
        if (isScriptLoaded == false)
            DBManager.instance.GetScript();
    }
    public void OnLoadScript(string script)
    {
        isScriptLoaded = true;
        script = script.Replace("\\n", "\n");
        script = script.Replace("\\r", "\r");
        Debug.Log(script);
        scriptText.text = script;
        foreach(ContentSizeFitter szf in scriptSizeFiltters)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)szf.transform);
        }
    }
    bool isSlideOn = false;
    [SerializeField] GameObject slideObject;
    [SerializeField] TMPro.TMP_Text slideText;
    [SerializeField] UIGraphControl graphControl;
    public void OnClickSlide()
    {
        isSlideOn = !isSlideOn;
        if(isSlideOn == true)
        {
            graphControl.TurnOffGraphControl();
        }
        slideObject.SetActive(isSlideOn);
    }
    public void TurnOffSlide()
    {
        if(isSlideOn == true)
        {
            OnClickSlide();
        }
    }
    public void OnClickPrevSlide()
    {
        int index = PDFManager.instance.GetPrevPage();
        if (index == -1) return;

        networkManager.SendNetworkMessage(NetworkManager.NETWORK_MSG.PREV_SLIDE, int.Parse(DBManager.instance.myID), index);
    }
    public void OnClickNextSlide()
    {
        int index = PDFManager.instance.GetNextPage();
        if (index == -1) return;

        networkManager.SendNetworkMessage(NetworkManager.NETWORK_MSG.NEXT_SLIDE, int.Parse(DBManager.instance.myID), index);
    }
    public void SetSlideText(string text)
    {
        slideText.text = text;
    }
}
