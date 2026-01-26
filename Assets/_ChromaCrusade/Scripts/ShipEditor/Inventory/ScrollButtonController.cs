using UnityEngine;

public class ScrollButtonController : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public NavItem scrollUpButton;
    public NavItem scrollDownButton;

    private PartInventoryPager pager;

    private void OnEnable()
    {
        EventBus.Subscribe<InventoryPageChangedEvent>(OnInventoryPageChangedEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<InventoryPageChangedEvent>(OnInventoryPageChangedEvent);
    }

    private void Awake()
    {
        pager = inventoryManager.GetPager();
    }

    private void OnInventoryPageChangedEvent(InventoryPageChangedEvent e)
    {
        ValidateButtons();
    }

    public void ValidateButtons()
    {
        if (pager == null) return;
        if (pager.CanPageUp()) scrollUpButton.Disabled = false; 
        else scrollUpButton.Disabled = true;

        if (pager.CanPageDown()) scrollDownButton.Disabled = false;
        else scrollDownButton.Disabled = true;
    }
}
