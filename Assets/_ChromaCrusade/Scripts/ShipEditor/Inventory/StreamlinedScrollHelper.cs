using UnityEngine;

public class StreamlinedScrollHelper : MonoBehaviour
{
    [SerializeField] NavItem leftUpItem;
    [SerializeField] NavItem midUpItem;
    [SerializeField] NavItem rightUpItem;

    [SerializeField] NavItem leftDownItem;
    [SerializeField] NavItem midDownItem;
    [SerializeField] NavItem rightDownItem;



    [SerializeField] NavItem leftUpItemTarget;
    [SerializeField] NavItem midUpItemTarget;
    [SerializeField] NavItem rightUpItemTarget;

    [SerializeField] NavItem leftDownItemTarget;
    [SerializeField] NavItem midDownItemTarget;
    [SerializeField] NavItem rightDownItemTarget;


    [SerializeField] InventoryManager inventoryManager;
    //PartInventoryPager pager;

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
        if (inventoryManager == null || inventoryManager.GetPager() == null) return;

        PartInventoryPager pager = inventoryManager.GetPager();
        if(pager.CanPageDown())
        {
            leftDownItem.navDown = leftDownItemTarget;
            midDownItem.navDown = midDownItemTarget;
            rightDownItem.navDown = rightDownItemTarget;
        }
        else
        {
            leftDownItem.navDown = null;
            midDownItem.navDown = null;
            rightDownItem.navDown = null;
        }


        if (pager.CanPageUp())
        {
            leftUpItem.navUp = leftUpItemTarget;
            midUpItem.navUp = midUpItemTarget;
            rightUpItem.navUp = rightUpItemTarget;
        }
        else
        {
            leftUpItem.navUp = null;
            midUpItem.navUp = null;
            rightUpItem.navUp = null;
        }
    }
}
