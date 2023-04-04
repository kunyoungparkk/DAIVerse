using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
//�κ� ���� ��� ���� �����Ѵ�.
//1. Ŀ���͸���¡
//2. �� ���� �� ����

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    public GameObject player;
    public Transform lobbyPlayerPosition;
    public GameObject[] enableObjsInLobby;
    public Text inputStringText;

    //ùȭ��, �ι�°ȭ��
    public GameObject firstUI;
    public GameObject secondUI;

    //Ŀ���͸���¡
    public GameObject[] hairs;
    public Material[] eyeMats;
    public Material[] headMats;
    public Material[] shirtMats;

    public SkinnedMeshRenderer lobbyEye;
    public SkinnedMeshRenderer lobbyHead;
    public SkinnedMeshRenderer lobbyShirt;

    //effect
    [SerializeField] GameObject loadingObj;
    public GameObject finishCustomizeEffect;
    public GameObject enterRoomEffect;
    int hairIdx = 0;
    int eyeIdx = 0;
    int headIdx = 0;
    int shirtIdx = 0;

    string inputString = "";

    [Header("�� ����Ʈ")]
    [SerializeField] GameObject roomListObj;
    [SerializeField] GameObject createRoomObj;
    List<RoomInfo> roomList = new List<RoomInfo>();
    [SerializeField] Toggle[] roomToggles;
    [SerializeField] TMP_Text[] roomNames;
    [SerializeField] TMP_Text[] roomPersonCount;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    private void Start()
    {
        loadingObj.SetActive(true);

        roomListObj.SetActive(true);
        createRoomObj.SetActive(false);
    }
    public void RefreshRoomList(List<RoomInfo> refreshRooms)
    {
        //������Ʈ �� ���
        foreach (RoomInfo newRoom in refreshRooms)
        {
            //bool isExist = false;
            if (roomList.Contains(newRoom))
            {
                //�����ؾ� �� ���̶��
                if (newRoom.RemovedFromList)
                {
                    roomList.Remove(newRoom);
                    //isExist = true;
                }
                else
                {
                    roomList[roomList.IndexOf(newRoom)] = newRoom;
                }
            }
            //���ο� ��
            else if (!newRoom.RemovedFromList)
            {
                roomList.Add(newRoom);
            }

        }
        //�� �����ڰ� 0���̸� ����
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount == 0)
            {
                roomList.Remove(roomList[i]);
                //������ ����� ��� ó�� ���� �κ� ������ ����
                i--;
            }
        }
        //��ư�� ���ΰ�ħ
        RefreshButtons();
    }
    void RefreshButtons()
    {
        //roomList.count < roomButtons.Length ��� �����Ͽ�
        for (int i = 0; i < roomList.Count; i++)
        {
            roomToggles[i].gameObject.SetActive(true);
            roomNames[i].text = roomList[i].Name;
            roomPersonCount[i].text = roomList[i].PlayerCount + "/4";
        }
        for (int i = roomList.Count; i < roomToggles.Length; i++)
        {
            roomToggles[i].gameObject.SetActive(false);
        }
    }
    public void OnClickEnterRoom()
    {
        bool isExist = false;
        for(int i=0; i<roomList.Count; i++)
        {
            if(roomToggles[i].isOn == true)
            {
                inputString = roomNames[i].text;
                isExist = true;
                break;
            }
        }
        if(isExist)
            EnterRoom();
    }
    public void BackToRoomList()
    {
        roomListObj.SetActive(true);
        createRoomObj.SetActive(false);
    }
    public void OnClickCreateRoom()
    {
        roomListObj.SetActive(false);
        createRoomObj.SetActive(true);
    }
    public void InputKeyboard(string key)
    {
        //���ڼ����� �������
        if (inputString.Length > 20)
            return;

        inputString += key;
        inputStringText.color = Color.white;
        inputStringText.text = inputString;
    }
    public void DeleteNumber()
    {
        if (inputString.Length == 0)
        {
            return;
        }
        inputString = inputString.Substring(0, inputString.Length - 1);

        if (inputString.Length == 0)
        {
            inputStringText.text = "�� �̸��� �Է��ϼ���";
            inputStringText.color = Color.gray;
        }
        else
        {
            inputStringText.text = inputString;
        }
    }
    //OnJoinnedLobby
    public void OnJoinedLobby()
    {
        foreach (GameObject go in enableObjsInLobby)
        {
            go.SetActive(true);
        }
        loadingObj.SetActive(false);
        player.transform.position = lobbyPlayerPosition.position;
        player.transform.rotation = lobbyPlayerPosition.rotation;
        firstUI.SetActive(true);
        secondUI.SetActive(false);
    }
    public void EnterRoom()
    {
        if (inputString.Length == 0)
        {
            return;
        }
        foreach (GameObject go in enableObjsInLobby)
        {
            go.SetActive(false);
        }
        NetworkManager.instance.EnterRoom(inputString);
        enterRoomEffect.SetActive(true);
        StartCoroutine(ActiveWait(enterRoomEffect, 5));
    }

    //Ŀ���͸���¡
    public void ChangeHair(int addVal)
    {
        hairs[hairIdx].SetActive(false);
        hairIdx = (hairIdx + hairs.Length + addVal) % hairs.Length;
        hairs[hairIdx].SetActive(true);
    }
    public void ChangeEye(int addVal)
    {
        eyeIdx = (eyeIdx + eyeMats.Length + addVal) % eyeMats.Length;
        lobbyEye.material = eyeMats[eyeIdx];
    }
    public void ChangeHead(int addVal)
    {
        headIdx = (headIdx + headMats.Length + addVal) % headMats.Length;
        lobbyHead.material = headMats[headIdx];
    }
    public void ChangeShirt(int addVal)
    {
        shirtIdx = (shirtIdx + shirtMats.Length + addVal) % shirtMats.Length;
        lobbyShirt.material = shirtMats[shirtIdx];
    }

    public void BackToCustomizing()
    {
        firstUI.SetActive(true);
        secondUI.SetActive(false);
    }
    //���� UI�� (�� ����)
    public void SaveCustomizing()
    {
        firstUI.SetActive(false);
        secondUI.SetActive(true);
        finishCustomizeEffect.SetActive(true);
        StartCoroutine(ActiveWait(finishCustomizeEffect, 5));
    }
    //����
    public void OnClickQuit()
    {
#if PLATFORM_ANDROID
        Application.Quit();
#endif
    }

    //�ٸ� ������ Ŀ���͸���¡ ���� GET
    public int GetHairIdx() { return hairIdx; }
    public int GetHeadIdx() { return headIdx; }
    public int GetEyeIdx() { return eyeIdx; }
    public int GetShirtIdx() { return shirtIdx; }

    IEnumerator ActiveWait(GameObject go, int waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        go.SetActive(false);
    }
}
