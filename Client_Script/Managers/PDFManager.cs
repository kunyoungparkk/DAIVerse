using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paroxe.PdfRenderer;
public class PDFManager : MonoBehaviour
{
    [SerializeField] PDFViewer viewer;
    public static PDFManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    void RenderPDF()
    {
        viewer.LoadDocument();
        UIManager.instance.SetSlideText("1/" + viewer.GetPageCount());
    }

    public void GoToPage(int pageNum)
    {
        if(false == viewer.IsLoaded)
        {
            return;
        }
        if (pageNum < 0)
        {
            return;
        }
        viewer.GoToPage(pageNum);
        UIManager.instance.SetSlideText(pageNum+1 + "/" + viewer.GetPageCount());
        print("page : " + pageNum);
    }
    public int GetNextPage()
    {
        if (false == viewer.IsLoaded)
        {
            return -1;
        }
        if (viewer.CurrentPageIndex + 1 < viewer.GetPageCount())
            return viewer.CurrentPageIndex + 1;
        else
            return viewer.CurrentPageIndex;
    }
    public int GetPrevPage()
    {
        if (false == viewer.IsLoaded)
        {
            return -1;
        }
        if (viewer.CurrentPageIndex - 1 >= 0)
            return viewer.CurrentPageIndex - 1;
        else
            return viewer.CurrentPageIndex;
    }

    public bool isRender = false;
    private void Update()
    {
        if (isRender == true)
        {
            isRender = false;
            RenderPDF();
        }
    }
}
