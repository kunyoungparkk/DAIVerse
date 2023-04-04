using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTwistCorrection : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    };
    public Axis axis;
    [Range(0f, 1f)]
    public float weight;
    public Transform source;
    public Transform target;
    Vector3 offset;
    private void Start()
    {
        offset = target.eulerAngles - source.eulerAngles;
    }
    // Update is called once per frame
    void Update()
    {
        switch (axis)
        {
            case Axis.X:
                target.Rotate((source.localRotation.eulerAngles.x - target.localRotation.eulerAngles.x) * weight, 0, 0);
                break;
            case Axis.Y:
                target.localEulerAngles = Vector3.zero;
                //target.Rotate(0, (source.localEulerAngles.y - target.localEulerAngles.y + offset.y) * weight, 0);
                //target.eulerAngles = new Vector3(target.eulerAngles.x, source.eulerAngles.y + offset.y, target.eulerAngles.z);
                //target.Rotate(0, (source.localRotation.eulerAngles.y - target.localRotation.eulerAngles.y) * weight, 0);
                break;
            case Axis.Z:
                target.Rotate(0, 0, (source.localRotation.eulerAngles.z - target.localRotation.eulerAngles.z) * weight);
                break;
            default:
                break;
        }
    }
}
