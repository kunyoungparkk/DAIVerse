using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SerializeField]
public class UserClass
{
    //������
    public UserClass(string hashedPW)
    {
        this.password = hashedPW;
    }
    //ȸ�� ����
    public string password;
    public string pdf;
    public string script;
    public string nickname;
}