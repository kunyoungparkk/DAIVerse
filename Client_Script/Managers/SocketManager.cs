using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

[Serializable]
public class Data
{
    public Data(string Query)
    {
        this.Query = Query;
        Bottype = "MyService";
    }
    public string Query;
    public string Bottype;
}
[Serializable]
public class ReceiveData
{
    public string Query;
    public string Intent;
    public float Probability;
}
public class StateObject
{
    public const int BufferSize = 256;
    public byte[] buffer = new byte[BufferSize];
    public StringBuilder sb = new StringBuilder();
    public Socket workSocket = null;
}
public class SocketManager : MonoBehaviour
{
    public GameObject loadingImage;

    private const int port = 8080;

    public static ManualResetEvent connectDone = new ManualResetEvent(false);
    public static ManualResetEvent sendDone = new ManualResetEvent(false);
    public static ManualResetEvent receiveDone = new ManualResetEvent(false);

    public static string response = String.Empty;

    public static SocketManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    static bool isFinished = false;
    private void Update()
    {
        if(isFinished)
        {
            isFinished = false;

            ReceiveData d = JsonUtility.FromJson<ReceiveData>(response);
            print(d.Query);
            print(d.Probability);
            print(d.Intent);
            NetworkManager.instance.Operate((VoiceCommandManager.OPERATION)(int.Parse(d.Intent)), int.Parse(DBManager.instance.myID));
            //로딩이미지 비활성화
            UIManager.instance.EndLoadSocket();

        }
    }
    private static void StartClient(string message)
    {
        connectDone.Reset();
        sendDone.Reset();
        receiveDone.Reset();

        try
        {
            //IPHostEntry ipHostEntry = Dns.GetHostEntry("ipAddress");
            IPAddress ipAddress = IPAddress.Parse("15.165.28.201");//ipHostEntry.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();

            Data data = new Data(message);
            message = JsonUtility.ToJson(data);
            var byteArray = Encoding.UTF8.GetBytes(message);
            //send
            client.Send(byteArray);
            //Send(client, message/*string.Format("{0} <EOF>", message)*/);
            //sendDone.WaitOne();

            Receive(client);
            receiveDone.WaitOne();

            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;

            client.EndConnect(ar);
            Debug.Log(string.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));

            connectDone.Set();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private static void Receive(Socket client)
    {
        try
        {
            StateObject state = new StateObject();
            state.workSocket = client;

            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer), 0, bytesRead);

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            //끝
            else
            {
                if (state.sb.Length > 1)
                    response = state.sb.ToString();

                isFinished = true;
                receiveDone.Set();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        //print(response);
        //Debug.LogFormat("Client Get Response: {0}", response);
    }

    private static void Send(Socket client, String data)
    {
        byte[] bytesData = Encoding.ASCII.GetBytes(data);

        client.BeginSend(bytesData, 0, bytesData.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;
            int bytesSent = client.EndSend(ar);

            Debug.Log(string.Format("Sent {0} bytes to server.", bytesSent));

            sendDone.Set();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    Thread thread;

    public void SendAndReceive(string message)
    {
        //로딩 이미지 활성화
        UIManager.instance.StartLoadSocket();
        thread = new Thread(new ThreadStart(() => StartClient(message)));
        thread.Start();
    }

}





/*using UnityEngine;
//using System.Net;
using System.Net.Sockets;
using System;
using System.Text;

[Serializable]
public class Data
{
    public Data(string Query)
    {
        this.Query = Query;
        Bottype = "MyService";
    }
    public string Query;
    public string Bottype;
}

[Serializable]
public class ReceiveData
{
    public string Query;
    public string Intent;
    public float Probability;
}
public class SocketManager : MonoBehaviour
{
    public static SocketManager instance;
    private void Awake()
    {
        if(instance == null)
            instance = this;
    }
    *//*public string debugMsg;
    private void Start()
    {
        print("결과: " + SendAndReceive(debugMsg));
    }*//*
    string ip = "15.165.28.201";
    int port = 8080;
    private Socket client;

    /// <summary> 
    /// Send data to port, receive data from port.
    /// </summary>
    /// <param name="dataOut">Data to send</param>
    /// <returns></returns>
    public int SendAndReceive(string dataOut)
    {
        //initialize socket
        string receivedString;
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(ip, port);
        if (!client.Connected)
        {
            Debug.LogError("Socket Connection Failed");
            return -1;
        }
        //string -> json -> byte -> send
        Data data = new Data(dataOut);
        dataOut = JsonUtility.ToJson(data);
        var byteArray = Encoding.UTF8.GetBytes(dataOut);
        //send
        client.Send(byteArray);

        //receive(byte) -> string -> json
        byte[] bytes = new byte[1024];
        client.Receive(bytes);
        receivedString = Encoding.UTF8.GetString(bytes, 0, 1000);
        
        //close
        client.Close();

        print(receivedString);
        ReceiveData d = JsonUtility.FromJson<ReceiveData>(receivedString);
        print(d.Probability);
        print(d.Query);
        print(d.Intent);
        
        NetworkManager.instance.Operate((VoiceCommandManager.OPERATION)(int.Parse(d.Intent)), int.Parse(DBManager.instance.myID));
        
        return 0;
    }
}*/