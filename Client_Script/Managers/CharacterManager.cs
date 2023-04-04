using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
//캐릭터 생성시에 자신의 캐릭터와 매칭시킴
public class CharacterManager : MonoBehaviourPunCallbacks
{
    public static CharacterManager instance;
    //자신의 것
    public GameObject cameraRig;
    float cameraRigHeight = 0.1f;
    public Transform centerEyeAnchor;
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    public OVRSkeleton leftSkeleton;
    public OVRSkeleton rightSkeleton;
    //포지션
    public Transform[] positions;
    //DebugCharacter
    public string debugCharacter;

    //나의 캐릭터 저장
    GameObject myCharacter;
    VRRig myVRRig;
    NetworkManager networkManager;
    LobbyManager lobbyManager;
    //싱글톤
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    private void Start()
    {
        networkManager = NetworkManager.instance;
        lobbyManager = LobbyManager.instance;
    }
    //캐릭터 생성
    public void CreateCharacter()
    {
        //자리 정하기 -> 남는자리에(임시)
        int positionIndex = 0;
        for (int i = 0; i < networkManager.maxPlayerCount; i++)
        {
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(i))
            {
                positionIndex = i;
                break;
            }
            else if (i == networkManager.maxPlayerCount - 1)
            {
                print("남는 자리가 없음.");
                PhotonNetwork.Disconnect();
            }
        }
        UIManager.instance.ChangeUIPosition(positionIndex);
        //캐릭터 생성
        myCharacter = PhotonNetwork.Instantiate(debugCharacter, Vector3.zero, Quaternion.identity);
        //본체 옮기기
        ChangeCharacterLocation(positionIndex);
        //전달
        VoiceNetworkManager.instance.RegisterSpeackerPrefab(myCharacter);
        //임시로 positionIndex를 hair , shirt에 넣음. 추후 수정하기!!!!
        myCharacter.GetComponent<CharacterSync>().SyncCharacter(centerEyeAnchor, leftHandAnchor, rightHandAnchor, leftSkeleton, rightSkeleton, 
            lobbyManager.GetHairIdx(), lobbyManager.GetEyeIdx(),lobbyManager.GetHeadIdx(), lobbyManager.GetShirtIdx());
        myVRRig = myCharacter.GetComponent<VRRig>();
    }
    //
    public void ChangeCharacterLocation(int index, int pastIndex = -1)
    {
        //networkManager.photonView.RPC("ChangeLocation", RpcTarget.AllBuffered, index, pastIndex, PhotonNetwork.LocalPlayer.ActorNumber);
        networkManager.ChangeLocation(index, pastIndex, PhotonNetwork.LocalPlayer.ActorNumber);
        cameraRig.transform.position = positions[index].position;
        cameraRig.transform.rotation = positions[index].rotation;
        ApplyPlayerHeight();
    }
    public GameObject GetMyCharacter()
    {
        return myCharacter;
    }
    public Vector3 GetPlayerHeadPosition()
    {
        return myVRRig.head.vrTarget.position;
    }
    //높이 적용
    public void ApplyPlayerHeight()
    {
        cameraRig.transform.position = new Vector3(cameraRig.transform.position.x, cameraRigHeight, cameraRig.transform.position.z);
    }
    public void ChangePlayerHeight(bool isUp)
    {
        if (isUp)
        {
            cameraRigHeight += 0.01f;
        }
        else
        {
            cameraRigHeight -= 0.01f;
        }
        ApplyPlayerHeight();
    }
}
