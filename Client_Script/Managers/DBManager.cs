using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Proyecto26;
using Firebase.Storage;

public class DBManager : MonoBehaviour
{
    public static DBManager instance;

    GraphManager graphManager;

    public string myID;
    string myPW;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        graphManager = GraphManager.instance;
        //�ʱ� �������� Ȯ��
        if (PlayerPrefs.HasKey("id") == false)
        {
            GenerateUserData();
        }
        //�ʱ� ���� �ƴϸ� �ٷ� ����â�� ǥ��
        else
        {
            myID = PlayerPrefs.GetString("id");
            UIManager.instance.DisplayUserInfo(myID, PlayerPrefs.GetString("pw"));
        }
    }

    //SHA256
    public string SHA256Hash(string data)
    {
        SHA256 sha = new SHA256Managed();
        byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte b in hash)
        {
            stringBuilder.AppendFormat("{0:x2}", b);
        }
        return stringBuilder.ToString();

    }
    //UserData DB�� ����
    private void GenerateUserData()
    {
        //id����
        RestClient.Get(url: "https://daiverse-default-rtdb.firebaseio.com/userCount/userCount.json").Then(response =>
        {
            string readData = response.Text;
            //����
            int newCount = int.Parse(readData) + 1;
            myID = newCount.ToString();
            myPW = (100000 + new System.Random(Time.renderedFrameCount).Next() % 900000).ToString(); //Random.Range(1000, 10000).ToString(); 
            RestClient.Put("https://daiverse-default-rtdb.firebaseio.com/userCount.json", new SimpleJson(newCount));
            //�ؽ�
            string hashedID = SHA256Hash(myID);
            string hashedPW = SHA256Hash(myPW);
            //DB�� UserData�� UserClass �ø���
            RestClient.Put("https://daiverse-default-rtdb.firebaseio.com/users/" + hashedID + ".json", new UserClass(hashedPW));
            StartCoroutine(WaitGenerate());

            //PlayerPrefs�� ����
            PlayerPrefs.SetString("id", myID);
            PlayerPrefs.SetString("pw", myPW);
            //����â�� ǥ��
            UIManager.instance.DisplayUserInfo(myID, myPW);
        });
    }
    //2�� �� 2d, 3d ����
    IEnumerator WaitGenerate()
    {
        yield return new WaitForSeconds(2);
        string hashedID = SHA256Hash(myID);
        UserData userData = new UserData();
        RestClient.Put("https://daiverse-default-rtdb.firebaseio.com/users/" + hashedID + "/2D.json", userData);
        RestClient.Put("https://daiverse-default-rtdb.firebaseio.com/users/" + hashedID + "/3D.json", userData);
    }
    //��, �� �� ū ���� return
    public void BarGraphReadData()
    {
        StreamReader sr = new StreamReader(Application.persistentDataPath + "/bargraph_3d.csv");
        bool endOfFile = false;
        int scaler;
        int groupCount = 0;
        int categoryCount = 0;

        if (null == sr.ReadLine())
        {
            endOfFile = true;
        }
        while (!endOfFile)
        {
            string data_String = sr.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            string[] objects = data_String.Split(',');

            if (graphManager.AddGroup(objects[0]) == true)
            {
                groupCount++;
            }
            if (graphManager.AddCategory(objects[1]) == true)
            {
                categoryCount++;
            }
            graphManager.UpdateGraphValue(objects[0], objects[1], float.Parse(objects[2]));

            //graphManager.DotAddCategory((categoryIdx++).ToString(), float.Parse(objects[0]), float.Parse(objects[1]), float.Parse(objects[2]) / maxValue * 5.0f);
        }
        sr.Close();

        scaler = (groupCount > categoryCount) ? groupCount : categoryCount;
        graphManager.CreateReadGraph(scaler);

    }
    public void DotGraphReadData(int firstIdx = -1, int secondIdx = -1)
    {
        StreamReader maxReader = new StreamReader(Application.persistentDataPath + "/dotgraph_3d.csv");

        bool endOfFile = false;
        int categoryIdx = 0;
        float maxValue = 0;

        int[] axis = new int[3] { 1, 2, 0 };

        //���� ��
        if (firstIdx != -1 && secondIdx != -1)
        {
            int temp;
            int axisChangeIdx_first = 0;
            int axisChangeIdx_second = 0;
            for (int i = 0; i < axis.Length; i++)
            {
                if(axis[i] == firstIdx)
                {
                    axisChangeIdx_first = i;
                }
                else if(axis[i] == secondIdx)
                {
                    axisChangeIdx_second = i;
                }
            }
            temp = axis[axisChangeIdx_first];
            axis[axisChangeIdx_first] = axis[axisChangeIdx_second];
            axis[axisChangeIdx_second] = temp;
        }

        //ù��
        if (null == maxReader.ReadLine())
        {
            endOfFile = true;
        }
        //�ִ� �� ���ϱ�
        while (!endOfFile)
        {
            string data_String = maxReader.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            string[] objects = data_String.Split(',');

            foreach (string s in objects)
            {
                if (float.Parse(s) > maxValue)
                {
                    maxValue = float.Parse(s);
                }
            }

        }
        maxReader.Close();

        endOfFile = false;
        //���� ������ �ֱ�
        StreamReader sr = new StreamReader(Application.persistentDataPath + "/dotgraph_3d.csv");
        //ù��
        if (null == sr.ReadLine())
        {
            endOfFile = true;
        }
        while (!endOfFile)
        {
            string data_String = sr.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            string[] objects = data_String.Split(',');


            graphManager.DotAddCategory((categoryIdx++).ToString(), float.Parse(objects[axis[0]]), float.Parse(objects[axis[1]]), 5.0f - float.Parse(objects[axis[2]]) / maxValue * 5.0f, maxValue);
        }
        sr.Close();
        graphManager.SetViewSize(maxValue);
    }
    //firstIdx, secondIdx�� x,y,z�� � ���� ���� ����. ��) x,z : 0,2
    public void DotGraphReadData_2D(int firstIdx, int secondIdx)
    {

        StreamReader maxReader = new StreamReader(Application.persistentDataPath + "/dotgraph_2d.csv");

        bool endOfFile = false;
        int categoryIdx = 0;
        float maxValue = 0;
        //ù��
        if (null == maxReader.ReadLine())
        {
            endOfFile = true;
        }
        //�ִ� �� ���ϱ�
        while (!endOfFile)
        {
            string data_String = maxReader.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            string[] objects = data_String.Split(',');

            if (float.Parse(objects[firstIdx]) > maxValue)
            {
                maxValue = float.Parse(objects[firstIdx]);
            }
            if (float.Parse(objects[secondIdx]) > maxValue)
            {
                maxValue = float.Parse(objects[secondIdx]);
            }
        }
        maxReader.Close();

        endOfFile = false;
        //���� ������ �ֱ�
        StreamReader sr = new StreamReader(Application.persistentDataPath + "/dotgraph_2d.csv");
        //ù��
        if (null == sr.ReadLine())
        {
            endOfFile = true;
        }
        while (!endOfFile)
        {
            string data_String = sr.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            string[] objects = data_String.Split(',');

            graphManager.DotAddCategory_2D((categoryIdx++).ToString(), float.Parse(objects[firstIdx]), float.Parse(objects[secondIdx]));
        }
        sr.Close();
        graphManager.SetViewSize_2D(maxValue);
    }

    //3D -> 2D ����
    //������ ���� DotGraphReadData_2D�� �����Ϸ�������.. ���������� ���� �Լ�����. ������ ���� ����.
    public void Convert3DTo2D(int firstIdx, int secondIdx)
    {
        StreamReader maxReader = new StreamReader(Application.persistentDataPath + "/dotgraph_3d.csv");

        bool endOfFile = false;
        int categoryIdx = 0;
        float maxValue = 0;

        int[] axis = new int[3] { 1, 2, 0 };
        int x = axis[firstIdx];
        int y = axis[secondIdx];

        //ù��
        if (null == maxReader.ReadLine())
        {
            endOfFile = true;
        }
        //�ִ� �� ���ϱ�
        while (!endOfFile)
        {
            string data_String = maxReader.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            string[] objects = data_String.Split(',');

            foreach (string s in objects)
            {
                if (float.Parse(s) > maxValue)
                {
                    maxValue = float.Parse(s);
                }
            }
        }
        maxReader.Close();

        endOfFile = false;
        //���� ������ �ֱ�
        StreamReader sr = new StreamReader(Application.persistentDataPath + "/dotgraph_3d.csv");
        //ù��
        if (null == sr.ReadLine())
        {
            endOfFile = true;
        }
        while (!endOfFile)
        {
            string data_String = sr.ReadLine();
            if (data_String == null)
            {
                endOfFile = true;
                break;
            }
            string[] objects = data_String.Split(',');

            graphManager.DotAddCategory_2D((categoryIdx++).ToString(), float.Parse(objects[secondIdx]), float.Parse(objects[firstIdx]));
        }
        sr.Close();
        graphManager.SetViewSize_2D(maxValue);
    }
    //Firebase DB
    private void Get3DBarGraph(string storage_path)
    {
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        StorageReference storage_ref = storage.GetReferenceFromUrl("gs://daiverse.appspot.com");
        StorageReference file_ref = storage_ref.Child(storage_path);

        string local_url = Application.persistentDataPath + "/bargraph_3d.csv";

        file_ref.GetFileAsync(local_url).ContinueWith(file_task =>
        {
            if (!file_task.IsFaulted && !file_task.IsCanceled)
            {
                print("���� �ٿ�ε�");
                graphManager.isDownloaded3DBar = true;
            }
            else
            {
                print("����");
                print(file_task.Exception.ToString());
            }
        });
    }
    private void Get3DDotGraph(string storage_path)
    {
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        StorageReference storage_ref = storage.GetReferenceFromUrl("gs://daiverse.appspot.com");
        StorageReference file_ref = storage_ref.Child(storage_path);

        string local_url = Application.persistentDataPath + "/dotgraph_3d.csv";

        file_ref.GetFileAsync(local_url).ContinueWith(file_task =>
        {
            if (!file_task.IsFaulted && !file_task.IsCanceled)
            {
                print("���� �ٿ�ε�");
                graphManager.isDownloaded3DDot = true;
            }
            else
            {
                print("����");
                print(file_task.Exception.ToString());
            }
        });
    }
    private void Get2DGraph(string storage_path)
    {
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        StorageReference storage_ref = storage.GetReferenceFromUrl("gs://daiverse.appspot.com");
        StorageReference file_ref = storage_ref.Child(storage_path);

        string local_url = Application.persistentDataPath + "/dotgraph_2d.csv";

        file_ref.GetFileAsync(local_url).ContinueWith(file_task =>
        {
            if (!file_task.IsFaulted && !file_task.IsCanceled)
            {
                print("���� �ٿ�ε�");
                graphManager.isDownloaded2DDot = true;
            }
            else
            {
                print("����");
                print(file_task.Exception.ToString());
            }
        });
    }
    private void GetPDF(string storage_path)
    {
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        StorageReference storage_ref = storage.GetReferenceFromUrl("gs://daiverse.appspot.com");
        StorageReference file_ref = storage_ref.Child(storage_path);

        string local_url = Application.persistentDataPath + "/file.pdf.bytes";

        file_ref.GetFileAsync(local_url).ContinueWith(file_task =>
        {
            if (!file_task.IsFaulted && !file_task.IsCanceled)
            {
                print("���� �ٿ�ε�");
                PDFManager.instance.isRender = true;
            }
            else
            {
                print("����");
                print(file_task.Exception.ToString());
            }
        });
    }

    public enum FileType
    {
        graph_2D,
        graph_3D,
        pdf
    }
    //realtime DB���� ��� �������
    public void GetFile(FileType fileType, int playerID, int dataNum = 1)
    {
        string hashedId = SHA256Hash(playerID.ToString());
        string fileURL = string.Empty;
        switch (fileType)
        {
            case FileType.graph_2D:
                fileURL = "https://daiverse-default-rtdb.firebaseio.com/users/" + hashedId + "/2D/_" + dataNum.ToString() + ".json";
                break;
            case FileType.graph_3D:
                fileURL = "https://daiverse-default-rtdb.firebaseio.com/users/" + hashedId + "/3D/_" + dataNum.ToString() + ".json";
                break;
            case FileType.pdf:
                fileURL = "https://daiverse-default-rtdb.firebaseio.com/users/" + hashedId + "/pdf.json";
                break;
        }

        RestClient.Get(url: fileURL).Then(response =>
        {
            string readData = response.Text;
            string path = readData.Substring(1, readData.Length - 2);
            string splitedPath = string.Copy(path);
            if (path.Length == 0)
            {
                print("���丮�� ��ΰ� ��ϵǾ����� �ʽ��ϴ�.");
                return;
            }
            switch (fileType)
            {
                case FileType.graph_2D:
                    Get2DGraph(path);
                    break;
                case FileType.graph_3D:
                    string[] splits = splitedPath.Split('_');
                    if (splits[splits.Length - 1].Contains("b"))
                    {
                        path = path.Substring(0, path.Length - 2);
                        Get3DBarGraph(path);
                    }
                    else if (splits[splits.Length - 1].Contains("d"))
                    {
                        path = path.Substring(0, path.Length - 2);
                        Get3DDotGraph(path);
                    }
                    break;
                case FileType.pdf:
                    GetPDF(path);
                    break;
            }
            print(path);
        });
    }
    public void GetGraphListToTextArr(UnityEngine.UI.Text[] texts, bool is3D)
    {
        string targetURL;
        if(is3D == true)
        {
            targetURL = "https://daiverse-default-rtdb.firebaseio.com/users/" + SHA256Hash(myID) + "/3D.json";
        }
        else
        {
            targetURL = "https://daiverse-default-rtdb.firebaseio.com/users/" + SHA256Hash(myID) + "/2D.json";
        }

        RestClient.Get(url: targetURL).Then(response =>
        {
            string readData = response.Text;
            UserData graphData = JsonUtility.FromJson<UserData>(readData);
            string[] graphDataArr;

            //���Ŀ��� �ݺ������� �ٲ���.
            graphDataArr = graphData._1.Split('/');
            texts[0].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];
            graphDataArr = graphData._2.Split('/');
            texts[1].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];
            graphDataArr = graphData._3.Split('/');
            texts[2].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];
            graphDataArr = graphData._4.Split('/');
            texts[3].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];
            graphDataArr = graphData._5.Split('/');
            texts[4].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];
            graphDataArr = graphData._6.Split('/');
            texts[5].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];
            graphDataArr = graphData._7.Split('/');
            texts[6].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];
            graphDataArr = graphData._8.Split('/');
            texts[7].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];
            graphDataArr = graphData._9.Split('/');
            texts[8].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];
            graphDataArr = graphData._10.Split('/');
            texts[9].text = graphDataArr[graphDataArr.Length - 1].Split('.')[0];

            UIManager.instance.OnLoadGraphInfo();
        });
    }
    public void GetScript()
    {
        string script;
        string targetURL = "https://daiverse-default-rtdb.firebaseio.com/users/" + SHA256Hash(myID) + "/script.json";
        RestClient.Get(url: targetURL).Then(response =>
        {
            string readData = response.Text;
            if(response.Text.Length == 2)
            {
                script = "�뺻�� �������� �ʽ��ϴ�.";
            }
            else
                script = readData.Substring(1, readData.Length - 2);
            
            UIManager.instance.OnLoadScript(script);
        });
    }
}
