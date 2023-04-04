using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SerializeField]
public class UserClass
{
    //생성자
    public UserClass(string hashedPW)
    {
        this.password = hashedPW;
    }
    //회원 정보
    public string password;
    public string pdf;
    public string script;
    public string nickname;
}