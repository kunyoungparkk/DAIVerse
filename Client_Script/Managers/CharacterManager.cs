using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
//ĳ���� �����ÿ� �ڽ��� ĳ���Ϳ� ��Ī��Ŵ
public class CharacterManager : MonoBehaviourPunCallbacks
{
    public static CharacterManager instance;
    //�ڽ��� ��
    public GameObject cameraRig;
    float cameraRigHeight = 0.1f;
    public Transform centerEyeAnchor;
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    public OVRSkeleton leftSkeleton;
    public OVRSkeleton rightSkeleton;
    //������
    public Transform[] positions;
    //DebugCharacter
    public string debugCharacter;

    //���� ĳ���� ����
    GameObject myCharacter;
    VRRig myVRRig;
    NetworkManager networkManager;
    LobbyManager lobbyManager;
    //�̱���
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
    //ĳ���� ����
    public void CreateCharacter()
    {
        //�ڸ� ���ϱ� -> �����ڸ���(�ӽ�)
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
                print("���� �ڸ��� ����.");
                PhotonNetwork.Disconnect();
            }
        }
        UIManager.instance.ChangeUIPosition(positionIndex);
        //ĳ���� ����
        myCharacter = PhotonNetwork.Instantiate(debugCharacter, Vector3.zero, Quaternion.identity);
        //��ü �ű��
        ChangeCharacterLocation(positionIndex);
        //����
        VoiceNetworkManager.instance.RegisterSpeackerPrefab(myCharacter);
        //�ӽ÷� positionIndex�� hair , shirt�� ����. ���� �����ϱ�!!!!
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
    //���� ����
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
