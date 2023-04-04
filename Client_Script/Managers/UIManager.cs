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

        //ui��Ȱ��ȭ
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
        //AI ���������� �ǵ������ϰ�
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
        //AI ���������� �ǵ������ϰ�
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
            //height ����
            case 0:
                //����
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
                print("�޴� ��ư ����");
                break;
        }*/
    }
    public void OnClickChangeHeight(bool isUp)
    {
        characterManager.ChangePlayerHeight(isUp);
    }
    //��漱��
    [Header("��漱��")]
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
    //ID / PW ���
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

    //�ڸ� �̵� ��ư ������ ���
    public void OnClickChangeLocation(bool isSelected)
    {
        if (!isSelected) return;
        //UI��Ȱ��ȭ
        //StartCoroutine(SetActiveObject(uiTransform.gameObject, false));
        isChangeLocationOn = !isChangeLocationOn;
        if (isChangeLocationOn)
        {
            changeLocationImg.color = new Color(0.21f, 0.21f, 0.21f);
            //changeLocationUIs Ȱ��ȭ
            Vector3 playerHeadPosition = characterManager.GetPlayerHeadPosition();
            for (int i = 0; i < changeLocationUIs.Length; i++)
            {
                //�̹� �ٸ� ����� ���� �ڸ��� ����
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(i))
                    continue;

                //�÷��̾� �ٶ󺸱�
                changeLocationUIs[i].transform.localPosition = changeLocationUIPositions[i];
                changeLocationUIs[i].transform.LookAt(playerHeadPosition, Vector3.up);
                changeLocationUIs[i].transform.Rotate(new Vector3(0, 180, 0), Space.Self);
                changeLocationUIs[i].transform.Translate(0, 0, -2, Space.Self);
                //Ȱ��ȭ
                changeLocationUIs[i].SetActive(true);
            }
        }
        else
        {
            changeLocationImg.color = new Color(0.764151f, 0.764151f, 0.764151f);
            //changeLocationUIs ��Ȱ��ȭ
            foreach (GameObject go in changeLocationUIs)
            {
                StartCoroutine(SetActiveObject(go, false));
            }
        }

    }
    //�ش� index �ڸ��� �̵�
    public void ChangeLocation(int index)
    {
        //changeLocationUIs ��Ȱ��ȭ
        /*foreach (GameObject go in changeLocationUIs)
        {
            StartCoroutine(SetActiveObject(go, false));
        }*/
        //�ڸ� �̵�
        characterManager.ChangeCharacterLocation(index, myPositionIndex);
        ChangeUIPosition(index);

        //UIȰ��ȭ
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
        print("����");
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
        print("��ŸƮ");
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

    [Header("�׷��� - ��ȣ Ȯ��â")]
    [SerializeField] private GameObject graphInfoCanvas;
    [SerializeField] private Image graphInfoImage;
    [SerializeField] private Text[] graphTexts_2D;
    [SerializeField] private Text[] graphTexts_3D;

    [SerializeField] private GameObject graphInfo_LoadingPanel;
    [SerializeField] private GameObject graphInfo_ContentPanel;

    bool isGrpahInfoOn = false;
    int loadCount = 0;
    //â ����
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

        //�޾ƿͼ� �ֱ�
        DBManager.instance.GetGraphListToTextArr(graphTexts_2D, false);
        DBManager.instance.GetGraphListToTextArr(graphTexts_3D, true);
    }
    //�ε� �Ϸ�Ǹ� ����
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
        //scriptText.text = "�ε� ��";
        //�������
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
