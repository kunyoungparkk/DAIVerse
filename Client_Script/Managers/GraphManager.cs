using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;

public class GraphManager : MonoBehaviour
{
    public static GraphManager instance;
    public Transform[] positions;

    public GameObject barGraph;
    public GameObject dotGraph;
    public GameObject dotGraph_2d;

    public Material[] barMat;
    public Material dotMat;
    public Material dotMat_2D;

    int matIndex = 0;
    BarChart barChart;
    GraphChartBase dotChart;
    GraphChart dot2dChart;

    [SerializeField] GameObject dotPrefab;

    [SerializeField] GameObject GraphControlObject;

    [SerializeField] float pointSize = 0.1f;
    private void Awake()
    {
        instance = this;
        barChart = barGraph.GetComponent<BarChart>();
        dotChart = dotGraph.GetComponent<GraphChartBase>();
        dot2dChart = dotGraph_2d.GetComponent<GraphChart>();
    }
    private void Start()
    {
    }

    //함수가 오동작해서 bool 변수 변경으로 동작하게..
    public bool isDownloaded3DDot = false;
    public bool isDownloaded2DDot = false;
    public bool isDownloaded3DBar = false;
    private void Update()
    {
        if (isDownloaded3DDot)
        {
            isDownloaded3DDot = false;
            CreateDotGraph(UIManager.instance.GetPositionIndex());
        }
        else if(isDownloaded2DDot)
        {
            isDownloaded2DDot = false;
            CreateDotGraph_2D(0, 1);
        }
        else if(isDownloaded3DBar)
        {
            isDownloaded3DBar = false;
            CreateBarGraph(UIManager.instance.GetPositionIndex());
        }
    }
    public void CreateBarGraph(int positionIdx)
    {
        //초기화
        barChart.DataSource.ClearValues();
        barChart.DataSource.ClearGroups();
        barChart.DataSource.ClearCategories();

        //readdata
        DBManager.instance.BarGraphReadData();
        //위치 조정
        ChangeGraphPosition(positionIdx);
    }
    //read 후에 호출됨.
    public void CreateReadGraph(int scaler)
    {
        //scale을 크기에 맞춰 조정
        barGraph.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f) / scaler;
        barGraph.SetActive(true);
    }
    
    public void RemoveGraph(bool is3D)
    {
        if (is3D == true)
        {
            GraphControlObject.SetActive(false);
            barGraph.SetActive(false);
            dotGraph.SetActive(false);
        }
        else
        {
            dotGraph_2d.SetActive(false);
        }
    }
    public void UpdateGraphValue(string group, string category, float value)
    {
        barChart.DataSource.SetValue(category, group, value);
        //barChart.DataSource.SlideValue("Player 2", "Value 1", Random.value * 20, 40f);
    }
    //return: 카테고리 생성 성공 : true
    public bool AddCategory(string category)
    {
        if(barChart.DataSource.HasCategory(category) == true)
        {
            return false;
        }
        barChart.DataSource.AddCategory(category, barMat[++matIndex % barMat.Length]);
        return true;
    }
    //return: 그룹 생성 성공 : true
    public bool AddGroup(string group)
    {
        if (barChart.DataSource.HasGroup(group) == true)
        {
            return false;
        }
        barChart.DataSource.AddGroup(group);
        return true;
    }
    public void ChangeGraphPosition(int positionIdx)
    {
        barGraph.transform.position = positions[positionIdx].position;
        barGraph.transform.rotation = positions[positionIdx].rotation;
        dotGraph.transform.position = positions[positionIdx].position;
        dotGraph.transform.rotation = positions[positionIdx].rotation;
    }

    public void ClearDotChart()
    {
        dotChart.DataSource.Clear();
    }
    public void ClearAndActivate2DDotChart()
    {
        dot2dChart.DataSource.ClearCategory("0");
        dotGraph_2d.SetActive(true);
    }
    //dot
    public void CreateDotGraph(int positionIdx)
    {
        dotGraph.SetActive(true);
        GraphControlObject.SetActive(true);
        //초기화
        dotChart.DataSource.Clear();

        //readdata : 임시
        DBManager.instance.DotGraphReadData();

        //위치 조정 --> 
        ChangeGraphPosition(positionIdx);
    }
    public void SetViewSize(float maxValue)
    {
        dotChart.ScrollableData.HorizontalViewSize = maxValue;
        dotChart.ScrollableData.VerticalViewSize = maxValue;
    }
    public bool DotAddCategory(string category, float x, float y, float z, float maxValue)
    {
        Material mat;
        if (x + y + z > maxValue)
        {
            mat = dotMat;
        }
        else
        {
            mat = dotMat_2D;
        }

        dotChart.DataSource.AddCategory3DGraph(category, null, null, 0, new MaterialTiling(), null, null, false, dotPrefab,
            mat, pointSize, z, false, 10);
        dotChart.DataSource.AddPointToCategory(category, x, y);
        return true;
    }
    public void RemoveDotGraph()
    {
        dotGraph.SetActive(false);
    }

    //dot2d
    public void CreateDotGraph_2D(int firstIdx, int secondIdx)
    {
        dotGraph_2d.SetActive(true);

        //초기화
        dot2dChart.DataSource.ClearCategory("0");

        DBManager.instance.DotGraphReadData_2D(firstIdx, secondIdx);
    }
    public void SetViewSize_2D(float maxValue)
    {
        dot2dChart.ScrollableData.HorizontalViewSize = maxValue;
        dot2dChart.ScrollableData.VerticalViewSize = maxValue;
    }
    public bool DotAddCategory_2D(string category, float x, float y)
    {
        dot2dChart.DataSource.AddPointToCategory("0", x, y);
        return true;
    }
    public void RemoveDotGraph_2D()
    {
        dotGraph_2d.SetActive(false);
    }


}
