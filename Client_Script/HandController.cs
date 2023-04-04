using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HandRig
{
    public Transform thumb_0;
    public Transform thumb_1;
    public Transform thumb_2;
    public Transform index_0;
    public Transform index_1;
    public Transform index_2;
    public Transform middle_0;
    public Transform middle_1;
    public Transform middle_2;
    public Transform pinky_0;
    public Transform pinky_1;
    public Transform pinky_2;
    public Transform ring_0;
    public Transform ring_1;
    public Transform ring_2;
}
public class HandController : MonoBehaviour
{
    //left:0, right:1
    public HandRig[] leftRightHandRig;
    public OVRSkeleton[] leftRightSkeleton;

    Vector3 leftHandRotationOffset = new Vector3(-90, 0, 90);
    Vector3 leftThumbRotationOffset = new Vector3(-180, 0, 90);
    Vector3 rightHandRotationOffset = new Vector3(90, 0, -90);
    Vector3 rightThumbRotationOffset = new Vector3(0, 0, -90);

    bool isThumb = false;
    Transform t;
    void FixedUpdate()
    {
        //leftHand에는 leftSkeleton을, rightHand에는 rightSkeleton의 뼈들의 회전값을 넣자.
        for (int idx = 0; idx < 2; idx++)
        {
            for (int i = 0; i < leftRightSkeleton[idx].Bones.Count; i++)
            {
                isThumb = false;
                switch (leftRightSkeleton[idx].Bones[i].Id)
                {
                    case OVRSkeleton.BoneId.Hand_Thumb1:
                        t = leftRightHandRig[idx].thumb_0;
                        isThumb = true;
                        break;
                    case OVRSkeleton.BoneId.Hand_Thumb2:
                        t = leftRightHandRig[idx].thumb_1;
                        isThumb = true;
                        break;
                    case OVRSkeleton.BoneId.Hand_Thumb3:
                        t = leftRightHandRig[idx].thumb_2;
                        isThumb = true;
                        break;
                    case OVRSkeleton.BoneId.Hand_Index1:
                        t = leftRightHandRig[idx].index_0;
                        break;
                    case OVRSkeleton.BoneId.Hand_Index2:
                        t = leftRightHandRig[idx].index_1;
                        break;
                    //중지 3번째이상함
                    case OVRSkeleton.BoneId.Hand_Index3:
                        t = leftRightHandRig[idx].index_2;
                        break;
                    case OVRSkeleton.BoneId.Hand_Middle1:
                        t = leftRightHandRig[idx].middle_0;
                        break;
                    case OVRSkeleton.BoneId.Hand_Middle2:
                        t = leftRightHandRig[idx].middle_1;
                        break;
                    case OVRSkeleton.BoneId.Hand_Middle3:
                        t = leftRightHandRig[idx].middle_2;
                        break;
                    case OVRSkeleton.BoneId.Hand_Pinky1:
                        t = leftRightHandRig[idx].pinky_0;
                        break;
                    case OVRSkeleton.BoneId.Hand_Pinky2:
                        t = leftRightHandRig[idx].pinky_1;
                        break;
                    case OVRSkeleton.BoneId.Hand_Pinky3:
                        t = leftRightHandRig[idx].pinky_2;
                        break;
                    case OVRSkeleton.BoneId.Hand_Ring1:
                        t = leftRightHandRig[idx].ring_0;
                        break;
                    case OVRSkeleton.BoneId.Hand_Ring2:
                        t = leftRightHandRig[idx].ring_1;
                        break;
                    case OVRSkeleton.BoneId.Hand_Ring3:
                        t = leftRightHandRig[idx].ring_2;
                        break;
                    default:
                        continue;
                }
                t.rotation = leftRightSkeleton[idx].Bones[i].Transform.rotation;
                if(idx == 0)
                {
                    if(isThumb)
                    {
                        t.Rotate(leftThumbRotationOffset);
                    }
                    else
                    {
                        t.Rotate(leftHandRotationOffset);
                    }
                }
                else if(idx == 1)
                {
                    if (isThumb)
                    {
                        t.Rotate(rightThumbRotationOffset);
                    }
                    else
                    {
                        t.Rotate(rightHandRotationOffset);
                    }
                }

            }
        }
    }
}
