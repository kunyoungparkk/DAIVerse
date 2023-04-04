using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Animations.Rigging;
using Proyecto26;

//�ڽ��� ĳ���� ���ο� ���� Ȱ��ȭ / ��Ȱ��ȭ
//�ڽ��� ĳ������ �͵��� ���鿡�� ����ȭ
public class CharacterSync : MonoBehaviourPunCallbacks
{
    bool isMyCharacter = false;
    public GameObject VRConstraints;
    public GameObject[] hideObjects;
    //Ŀ���͸���¡
    public GameObject[] hairs;
    public SkinnedMeshRenderer eye;
    public SkinnedMeshRenderer head;
    public SkinnedMeshRenderer shirt;
    //�г���
    public UnityEngine.UI.Text nicknameText;
    public Transform nickNameCanvas;

    public void SyncCharacter(Transform headTarget, Transform leftHandTarget, Transform rightHandTarget, OVRSkeleton leftHandSkeleton, OVRSkeleton rightHandSkeleton,
        int hairIdx, int eyeIdx, int headIdx, int shirtIdx)
    {
        isMyCharacter = true;
        nickNameCanvas.gameObject.SetActive(false);
        //1. VR ���� ���� �ֱ�
        VRRig vrRig = GetComponent<VRRig>();
        RigBuilder rigBuilder = GetComponent<RigBuilder>();
        BoneRenderer boneRenderer = GetComponent<BoneRenderer>();
        HandController handController = GetComponent<HandController>();
        vrRig.head.vrTarget = headTarget;
        vrRig.leftHand.vrTarget = leftHandTarget;
        vrRig.rightHand.vrTarget = rightHandTarget;
        handController.leftRightSkeleton[0] = leftHandSkeleton;
        handController.leftRightSkeleton[1] = rightHandSkeleton;
        //2. Ȱ��ȭ
        VRConstraints.SetActive(true);
        vrRig.enabled = true;
        rigBuilder.enabled = true;
        boneRenderer.enabled = true;
        handController.enabled = true;
        //��Ÿ
        foreach (GameObject go in hideObjects)
        {
            go.layer = 3;
        }
        //3. Ŀ���͸���¡ ���� ����ȭ
        photonView.RPC("CustomizeCharacter", RpcTarget.AllBuffered, hairIdx, eyeIdx, headIdx, shirtIdx);
        //4. �г��� ����ȭ
        GetNickname();
    }
    public void GetNickname()
    {
        string nickname;
        string hashedId = DBManager.instance.SHA256Hash(DBManager.instance.myID);
        RestClient.Get(url: "https://daiverse-default-rtdb.firebaseio.com/users/" + hashedId + "/nickname.json").Then(response =>
         {
             string result = response.Text;
             if(result.Length == 2)
             {
                 nickname = "�͸�"+DBManager.instance.myID;
             }
             else
             {
                 nickname = result.Split('\"')[1];
             }
             photonView.RPC("ShowNickname", RpcTarget.AllBuffered, nickname);
         });
    }
    [PunRPC]
    public void ShowNickname(string nickname)
    {
        nicknameText.text = nickname;
    }
    [PunRPC]
    public void CustomizeCharacter(int hairIdx, int eyeIdx, int headIdx, int shirtIdx)
    {
        for (int i = 0; i < hairs.Length; i++)
        {
            if (i == hairIdx)
                hairs[i].SetActive(true);
            else
                hairs[i].SetActive(false);
        }
        eye.material = LobbyManager.instance.eyeMats[eyeIdx];
        head.material = LobbyManager.instance.headMats[headIdx];
        shirt.material = LobbyManager.instance.shirtMats[shirtIdx];
    }
    //�г����� ī�޶� �ٶ󺸰�
    private void Update()
    {
        if(isMyCharacter == false)
        {
            nickNameCanvas.LookAt(CharacterManager.instance.GetPlayerHeadPosition(), Vector3.up);
            nickNameCanvas.Rotate(new Vector3(0, 180, 0), Space.Self);
        }
    }
}
