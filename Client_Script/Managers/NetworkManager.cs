using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    public enum NETWORK_MSG
    {
        SHOW_GRAPH_2D = 0,
        SHOW_GRAPH_3D = 1,
        SHOW_PDF = 2,
        PREV_SLIDE = 3,//버퍼
        NEXT_SLIDE = 4,//버퍼
        GUEST_MUTE_ON = 5,//버퍼
        GUEST_MUTE_OFF = 6,//버퍼
        HIDE_GRAPH_2D = 7,
        HIDE_GRAPH_3D = 8,
        CHANGE_XYZ = 9,
        CONVERT_3D_2D = 10,
        CHANGE_XYZ_RESET =11
    }
    public int maxPlayerCount = 4;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // 임시
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        print("시작");
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("서버 입장");
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        LobbyManager.instance.RefreshRoomList(roomList);
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        print("로비 입장");
        LobbyManager.instance.OnJoinedLobby();
        //PC TEST
        //EnterRoom("2022 METAVERSE DEV");
    }
    string roomName;
    public void EnterRoom(string roomName)
    {
        this.roomName = roomName;
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("join room fail");
        RoomOptions options = new RoomOptions();
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        options.MaxPlayers = (byte)maxPlayerCount;
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("룸 입장, Actor ID: " + PhotonNetwork.LocalPlayer.ActorNumber);
        //캐릭터 생성
        CharacterManager.instance.CreateCharacter();
        SoundManager.instance.StartBGM();
        //pdf
        if (PhotonNetwork.IsMasterClient)
        {
            SendNetworkMessage(NETWORK_MSG.SHOW_PDF, int.Parse(DBManager.instance.myID), 0);
        }
        //그래프 띄우기
        else
        {
            StartCoroutine(InitialGraph(3.0f));
        }
        
    }
    IEnumerator InitialGraph(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (graph_3d_idx != 0)
        {
            UIManager.instance.ShowGraph(true, playerId_3d, graph_3d_idx);
        }
        if(graph_2d_idx != 0)
        {
            UIManager.instance.ShowGraph(false, playerId_2d, graph_2d_idx);
        }
        PDFManager.instance.GoToPage(pageIndex);
        //pdf
        yield return null;
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        //자리 빼기
        if (PhotonNetwork.IsMasterClient)
        {
            ChangeLocation(-1, (int)PhotonNetwork.CurrentRoom.CustomProperties[otherPlayer.ActorNumber.ToString()], otherPlayer.ActorNumber);
            //photonView.RPC("ChangeLocation", RpcTarget.AllBuffered, -1, PhotonNetwork.CurrentRoom.CustomProperties[otherPlayer.ActorNumber], otherPlayer.ActorNumber);
        }
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        /*foreach (DictionaryEntry d in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            print(d.Key + ": " + d.Value);
        }*/

    }
    //마스터 클라이언트가 바뀌었을 경우
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

    }
    //operation -> 네트워크 메시지로 변환
    public void Operate(VoiceCommandManager.OPERATION operation, int playerID)
    {
        NETWORK_MSG networkMsg = 0;
        int index = 0;
        //2d 보여주기
        if ((int)operation >= 0 && (int)operation < 10)
        {
            networkMsg = NETWORK_MSG.SHOW_GRAPH_2D;
            index = (int)operation + 1;
        }
        else if ((int)operation >= 10 && (int)operation < 20)
        {
            networkMsg = NETWORK_MSG.SHOW_GRAPH_3D;
            index = (int)operation - 9;
        }
        else
        {
            switch (operation)
            {
                case VoiceCommandManager.OPERATION.PREV_SLIDE:
                    networkMsg = NETWORK_MSG.PREV_SLIDE;
                    index = PDFManager.instance.GetPrevPage();
                    if (index == -1) return;
                    break;
                case VoiceCommandManager.OPERATION.NEXT_SLIDE:
                    networkMsg = NETWORK_MSG.NEXT_SLIDE;
                    index = PDFManager.instance.GetNextPage();
                    if (index == -1) return;
                    break;
                //포인터, BGM는 혼자
                case VoiceCommandManager.OPERATION.POINT_ON:
                    PointerManager.instance.PointerOnOff(true);
                    return;
                case VoiceCommandManager.OPERATION.POINT_OFF:
                    PointerManager.instance.PointerOnOff(false);
                    return;
                case VoiceCommandManager.OPERATION.BGM_ON:
                    SoundManager.instance.StartBGM();
                    return;
                case VoiceCommandManager.OPERATION.BGM_OFF:
                    SoundManager.instance.EndBGM();
                    return;
                case VoiceCommandManager.OPERATION.GUEST_MUTE_ON:
                    if (PhotonNetwork.IsMasterClient)
                        networkMsg = NETWORK_MSG.GUEST_MUTE_ON;
                    else
                        return;
                    break;
                case VoiceCommandManager.OPERATION.GUEST_MUTE_OFF:
                    if (PhotonNetwork.IsMasterClient)
                        networkMsg = NETWORK_MSG.GUEST_MUTE_OFF;
                    else
                        return;
                    break;
                case VoiceCommandManager.OPERATION.HIDE_GRAPH_2D:
                    networkMsg = NETWORK_MSG.HIDE_GRAPH_2D;
                    break;
                case VoiceCommandManager.OPERATION.HIDE_GRAPH_3D:
                    networkMsg = NETWORK_MSG.HIDE_GRAPH_3D;
                    break;
                //나머지는 빼기
                default:
                    return;
            }
        }

        SendNetworkMessage(networkMsg, playerID, index);
    }
    //index : showgraph의 경우 그거
    //분류,..
    public void SendNetworkMessage(NETWORK_MSG network_MSG, int playerID, int index)
    {
        //Buffer넣는 경우
        if((int)network_MSG >=2 && (int)network_MSG <=8)
        {
            photonView.RPC("ReceiveNetworkMessage", RpcTarget.AllBuffered, network_MSG, playerID, index);
        }
        //아닌 경우
        else
        {
            photonView.RPC("ReceiveNetworkMessage", RpcTarget.All, network_MSG, playerID, index);
        }

        switch(network_MSG)
        {
            case NETWORK_MSG.SHOW_GRAPH_2D:
                photonView.RPC("ChangeGraphState", RpcTarget.AllBuffered, false, index, playerID);
                break;
            case NETWORK_MSG.HIDE_GRAPH_2D:
                photonView.RPC("ChangeGraphState", RpcTarget.AllBuffered, false, 0, playerID);
                break;
            case NETWORK_MSG.SHOW_GRAPH_3D:
                photonView.RPC("ChangeGraphState", RpcTarget.AllBuffered, true, index, playerID);
                break;
            case NETWORK_MSG.HIDE_GRAPH_3D:
                photonView.RPC("ChangeGraphState", RpcTarget.AllBuffered, true, 0, playerID);
                break;
        }
    }

    //공유되는 변수 : 플레이어 들어오면 마스터클라이언트 아닌경우에 확인해서 그래프 생성
    //들어오고 3초뒤?
    //0: 없음
    public int graph_3d_idx = 0;
    public int playerId_3d = 0;
    public int graph_2d_idx = 0;
    public int playerId_2d = 0;
    public int pageIndex = 0;
    //0: 없음
    [PunRPC]
    public void ChangeGraphState(bool is3D, int index, int playerId)
    {
        if(is3D == true)
        {
            playerId_3d = playerId;
            graph_3d_idx = index;
        }
        else
        {
            playerId_2d = playerId;
            graph_2d_idx = index;
        }
    }
    [PunRPC]
    public void ReceiveNetworkMessage(NETWORK_MSG msg, int playerID, int index)
    {
        switch (msg)
        {
            case NETWORK_MSG.SHOW_GRAPH_2D:
                UIManager.instance.ShowGraph(false, playerID, index);
                break;
            case NETWORK_MSG.SHOW_GRAPH_3D:
                UIManager.instance.ShowGraph(true, playerID, index);
                break;
            case NETWORK_MSG.SHOW_PDF:
                DBManager.instance.GetFile(DBManager.FileType.pdf, playerID);
                break;
            case NETWORK_MSG.PREV_SLIDE:
                PDFManager.instance.GoToPage(index);
                pageIndex = index;
                break;
            case NETWORK_MSG.NEXT_SLIDE:
                PDFManager.instance.GoToPage(index);
                pageIndex = index;
                break;
            case NETWORK_MSG.GUEST_MUTE_ON:
                if(PhotonNetwork.IsMasterClient == false)
                    UIManager.instance.VoiceOnOff(false);
                break;
            case NETWORK_MSG.GUEST_MUTE_OFF:
                if (PhotonNetwork.IsMasterClient == false)
                    UIManager.instance.VoiceOnOff(true);
                break;
            case NETWORK_MSG.HIDE_GRAPH_2D:
                UIManager.instance.HideGraph(false);
                break;
            case NETWORK_MSG.HIDE_GRAPH_3D:
                UIManager.instance.HideGraph(true);
                break;
            case NETWORK_MSG.CHANGE_XYZ:
                GraphManager.instance.ClearDotChart();
                DBManager.instance.DotGraphReadData(playerID, index);
                break;
            case NETWORK_MSG.CONVERT_3D_2D:
                GraphManager.instance.ClearAndActivate2DDotChart();
                DBManager.instance.Convert3DTo2D(playerID, index);
                break;
            case NETWORK_MSG.CHANGE_XYZ_RESET:
                GraphManager.instance.ClearDotChart();
                DBManager.instance.DotGraphReadData();
                break;
        }

    }
    public void ChangeLocation(int index, int pastIndex, int actorNumber)
    {
        ExitGames.Client.Photon.Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
        //나간 플레이어라면
        if (index == -1)
        {
            //임시 -> 나중에 Remove로 변경하기
            table[actorNumber.ToString()] = index;
            //table.Remove(actorNumber.ToString());
        }
        //초기 플레이어라면
        else if (pastIndex == -1)
        {
            table.Add(actorNumber.ToString(), index);
        }
        //자리 변경
        else
        {
            table[actorNumber.ToString()] = index;
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }
}
