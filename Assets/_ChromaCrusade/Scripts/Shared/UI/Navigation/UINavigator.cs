using System.Collections;
using UnityEngine;

public class UINavigator : Navigator, IUINavigator
{
    [SerializeField] Transform visualizerParent;
    [SerializeField] NavItem initialHoveredItem;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<ItemNavEvent>(OnNavEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ItemNavEvent>(OnNavEvent);
    }

    public override void Init()
    {
        base.Init();

        visualizer.transform.SetParent(visualizerParent);
        visualizer.transform.localScale = Vector3.one;
        NavItem targetItem = null;
        if (NavState.LastHoveredItem != null) targetItem = NavState.LastHoveredItem;
        else if (initialHoveredItem != null) targetItem = initialHoveredItem;
        else targetItem = GetComponentInChildren<NavItem>();
        NavToItem(targetItem);

        visualizer.ResetScale();
    }

    void OnNavEvent(ItemNavEvent e) => NavToItem(e.target);

    public void NavToItem(NavItem item)
    {
        if (item == null) return;
        NavState.hoveringSelector = item.CompareTag("EditorInventorySelector");
        NavState.HoveredItem = item;
        NavState.HoveredItem.OnHighlighted();
        NavState.currentItem = item;
        if (item.visualize) 
            visualizer.HighlightItem(NavState.HoveredItem);
    }

    public virtual void SwitchOff() {}

    public void TriggerItemNav(Vector2 dir)
    {
        if (NavState.HoveredItem == null)
            return;

        NavItem next = null;

        if (dir.y > 0.5f) next = NavState.HoveredItem.navUp;
        else if (dir.y < -0.5f) next = NavState.HoveredItem.navDown;
        else if (dir.x < -0.5f) next = NavState.HoveredItem.navLeft;
        else if (dir.x > 0.5f) next = NavState.HoveredItem.navRight;

        if (next == null)
            return;

        NavToItem(next);
    }

    public void DoDelayedNav(NavItem target)
    {
        if (delayedNavRoutine != null) StopCoroutine(delayedNavRoutine);
        delayedNavRoutine = StartCoroutine(DelayedNavigate(target));
    }

    Coroutine delayedNavRoutine;
    IEnumerator DelayedNavigate(NavItem target)
    {
        while (NavState.Scrolling)
            yield return null;

        NavToItem(target);
    }
}
