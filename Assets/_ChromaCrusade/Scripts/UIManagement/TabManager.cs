using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    public GameObject[] tabs;
    public Image[] tabButtons;
    public Sprite inactiveTab, activeTab;
    public Vector2 inactiveTabSize, activeTabSize;

    private void Start()
    {
        foreach (Image im in tabButtons)
        {
            im.sprite = inactiveTab;
            im.rectTransform.sizeDelta = inactiveTabSize;
        }
    }

    public void SwitchToTab(int tabId)
    {
        //foreach (GameObject go in tabs)
        //{
        //    go.SetActive(false);
        //}
        //tabs[tabId].SetActive(true);

        foreach (Image im in tabButtons)
        {
            im.sprite = inactiveTab;
            im.rectTransform.sizeDelta = inactiveTabSize;
        }
        tabButtons[tabId].sprite = activeTab;
        tabButtons[tabId].rectTransform.sizeDelta = activeTabSize;
    }
}
