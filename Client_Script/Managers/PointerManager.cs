using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PointerManager : MonoBehaviour
{
    public static PointerManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    GameObject myPointer;
    public Transform rightHandIndexTransform;
    [SerializeField] LineRenderer lineRenderer;

    bool isPointerOn = false;

    public void PointerOnOff(bool isOn)
    {
        //이미 켜져있으면 return
        if (isPointerOn && isOn)
        {
            return;
        }
        isPointerOn = isOn;
        lineRenderer.enabled = isOn;
        if (isOn == true)
        {
            myPointer = PhotonNetwork.Instantiate("PointerCanvas", Vector3.zero, Quaternion.identity);
        }
        else
        {
            if(myPointer != null)
            {
                PhotonNetwork.Destroy(myPointer);
                myPointer = null;
            }
        }
    }
    private RaycastHit hit;
    // Update is called once per frame
    void Update()
    {
        //ray
        if (isPointerOn == true && myPointer != null)
        {
            if (Physics.Raycast(rightHandIndexTransform.position, rightHandIndexTransform.right, out hit, 100, LayerMask.GetMask("PointerWall")))
            {
                lineRenderer.SetPosition(0, rightHandIndexTransform.transform.position);
                lineRenderer.SetPosition(1, hit.point);
                myPointer.transform.forward = hit.transform.forward;
                myPointer.transform.position = hit.point;
            }
        }
    }
}
