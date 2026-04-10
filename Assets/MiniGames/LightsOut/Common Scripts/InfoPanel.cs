using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    public GameObject panelRoot;
    public GameObject[] pages;

    int currentPage = 0;

    void Start()
    {
        panelRoot.SetActive(false);
        ShowPage(0);
    }

    public void Open()
    {
        panelRoot.SetActive(true);
        // Game is NOT paused anymore
    }

    public void Close()
    {
        panelRoot.SetActive(false);
    }

    public void NextPage()
    {
        if (pages.Length <= 1) return;

        currentPage = (currentPage + 1) % pages.Length;
        ShowPage(currentPage);
    }

    public void PrevPage()
    {
        if (pages.Length <= 1) return;

        currentPage = (currentPage - 1 + pages.Length) % pages.Length;
        ShowPage(currentPage);
    }

    void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);
    }
}
