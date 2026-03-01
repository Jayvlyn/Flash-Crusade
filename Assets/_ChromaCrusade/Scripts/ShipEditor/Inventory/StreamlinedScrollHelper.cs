using UnityEngine;

public class StreamlinedScrollHelper : MonoBehaviour
{
    [SerializeField] NavItem[] topRowItems;
    [SerializeField] NavItem[] bottomRowItems;
    [SerializeField] NavItem[] topRowTargets;
    [SerializeField] NavItem[] bottomRowTargets;

    [SerializeField] Pager pager;

    private void OnEnable()
    {
        EventBus.Subscribe<InventoryPageChangedEvent>(OnPageChange);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<InventoryPageChangedEvent>(OnPageChange);
    }

    private void OnPageChange(InventoryPageChangedEvent e)
    {
        if (pager == null) return;

        if(pager.CanPageDown())
        {
            for(int i = 0; i < bottomRowItems.Length; i++)
                bottomRowItems[i].navDown = bottomRowTargets[i];
        }
        else
        {
            for (int i = 0; i < bottomRowItems.Length; i++)
                bottomRowItems[i].navDown = bottomRowTargets[i];
        }


        if (pager.CanPageUp())
        {
            for (int i = 0; i < topRowItems.Length; i++)
                topRowItems[i].navUp = topRowTargets[i];
        }
        else
        {
            for (int i = 0; i < topRowItems.Length; i++)
                topRowItems[i].navUp = null;
        }
    }
}
