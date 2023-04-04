using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
//로비에 관한 모든 것을 관리한다.
//1. 커스터마이징
//2. 방 생성 및 입장

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    public GameObject player;
    public Transform lobbyPlayerPosition;
    public GameObject[] enableObjsInLobby;
    public Text inputStringText;

    //첫화면, 두번째화면
    public GameObject firstUI;
    public GameObject secondUI;

    //커스터마이징
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

    [Header("룸 리스트")]
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
        //업데이트 될 방들
        foreach (RoomInfo newRoom in refreshRooms)
        {
            //bool isExist = false;
            if (roomList.Contains(newRoom))
            {
                //제거해야 할 방이라면
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
            //새로운 방
            else if (!newRoom.RemovedFromList)
            {
                roomList.Add(newRoom);
            }

        }
        //방 접속자가 0명이면 삭제
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount == 0)
            {
                roomList.Remove(roomList[i]);
                //앞으로 당겨질 경우 처리 못한 부분 방지를 위해
                i--;
            }
        }
        //버튼들 새로고침
        RefreshButtons();
    }
    void RefreshButtons()
    {
        //roomList.count < roomButtons.Length 라는 전제하에
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
        //글자수제한 넘은경우
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
            inputStringText.text = "방 이름을 입력하세요";
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

    //커스터마이징
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
    //다음 UI로 (룸 선택)
    public void SaveCustomizing()
    {
        firstUI.SetActive(false);
        secondUI.SetActive(true);
        finishCustomizeEffect.SetActive(true);
        StartCoroutine(ActiveWait(finishCustomizeEffect, 5));
    }
    //종료
    public void OnClickQuit()
    {
#if PLATFORM_ANDROID
        Application.Quit();
#endif
    }

    //다른 곳에서 커스터마이징 정보 GET
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
