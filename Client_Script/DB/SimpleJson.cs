using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//���� json
[SerializeField]
public class SimpleJson
{
    public SimpleJson(int userCount)
    {
        this.userCount = userCount;
    }
    public int userCount;
}
