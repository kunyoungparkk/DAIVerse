using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class UIGraphControl : MonoBehaviour
{
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject inputUI;

    [SerializeField] private InteractableColorVisual[] axisInputs;
    [SerializeField] private GameObject[] axisObjects;
    public Color chooseColor;
    public Color originColor;

    bool isChangeXYZ;

    private void OnEnable()
    {
        isShow = false;
        Init();
    }

    void ChangeAxisColor(int axisNum, bool isChoose)
    {
        InteractableColorVisual.ColorState colorState = new InteractableColorVisual.ColorState();
        if (isChoose)
            colorState.Color = chooseColor;
        else
            colorState.Color = originColor;

        axisInputs[axisNum].InjectOptionalNormalColorState(colorState);
    }

    void Init()
    {
        firstAxis = -1;
        secondAxis = -1;
        mainUI.SetActive(false);
        inputUI.SetActive(false);
        for(int i = 0; i < axisInputs.Length; i++)
        {
            ChangeAxisColor(i, false);
        }
    }

    bool isShow = false;
    public void OnOffGraphControl()
    {
        Init();
        isShow = !isShow;
        if(isShow == true)
        {
            UIManager.instance.TurnOffSlide();
        }
        mainUI.SetActive(isShow);
    }
    public void TurnOffGraphControl()
    {
        if(isShow == true)
        {
            OnOffGraphControl();
        }    
    }
    public void GoToDetail(bool isChangeXYZ)
    {
        this.isChangeXYZ = isChangeXYZ;
        mainUI.SetActive(false);
        inputUI.SetActive(true);
    }
    public void OnClickOK()
    {
        //�ΰ� �Ȱ����
        if(firstAxis == -1 || secondAxis == -1)
        {
            if(isChangeXYZ == true)
            {
                NetworkManager.instance.SendNetworkMessage(NetworkManager.NETWORK_MSG.CHANGE_XYZ_RESET, 0, 0);
            }
            else
            {
                return;
            }
        }
        else if(isChangeXYZ == true)
        {
            NetworkManager.instance.SendNetworkMessage(NetworkManager.NETWORK_MSG.CHANGE_XYZ, firstAxis, secondAxis);
        }
        else
        {
            NetworkManager.instance.SendNetworkMessage(NetworkManager.NETWORK_MSG.CONVERT_3D_2D, firstAxis, secondAxis);
        }
        Init();
        mainUI.SetActive(true);
    }
    //x:0, y:1, z:2
    public int firstAxis;
    public int secondAxis;
    public void OnClickAxis(int axisNum)
    {
        //�̹� Ŭ���ȰŸ�..
        if(firstAxis == axisNum || secondAxis == axisNum)
        {
            return;
        }

        //�ƿ� ó�� ���� ���
        if(firstAxis == -1)
        {
            firstAxis = axisNum;
        }
        //ù��°�� �ִ� ���
        else if(secondAxis == -1)
        {
            secondAxis = firstAxis;
            firstAxis = axisNum;
        }
        //�̹� �Ѵ� �ִ� ���
        else
        {
            //���� ���� ����
            ChangeAxisColor(secondAxis, false);
            axisObjects[secondAxis].SetActive(false);
            axisObjects[secondAxis].SetActive(true);
            secondAxis = firstAxis;
            firstAxis = axisNum;
        }
        //axisNum�ش��ϴ� �Ϳ� ���� �ֱ�.
        ChangeAxisColor(axisNum, true);
    }

}
