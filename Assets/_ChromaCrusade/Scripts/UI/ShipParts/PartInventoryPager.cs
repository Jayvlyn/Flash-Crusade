using UnityEngine;

public class PartInventoryPager
{
    public int CurrentPage { get; private set; } = 1;
    public int PageCount { get; private set; } = 1;

    public void Reset()
    {
        CurrentPage = 1;
        PageCount = 1;
    }

    public void Recalculate(int totalItems, int itemsPerPage)
    {
        PageCount = Mathf.Max(1, Mathf.CeilToInt((float)totalItems / itemsPerPage));

        CurrentPage = Mathf.Clamp(CurrentPage, 1, PageCount);
    }

    public (int start, int end) GetRange(int totalItems, int itemsPerPage)
    {
        int start = (CurrentPage - 1) * itemsPerPage;
        int end = Mathf.Min(start + itemsPerPage, totalItems);
        return (start, end);
    }

    public bool CanPageUp() => CurrentPage > 1;
    public bool CanPageDown() => CurrentPage < PageCount;

    public void PageUp()
    {
        if (CanPageUp())
            CurrentPage--;
    }

    public void PageDown()
    {
        if (CanPageDown())
            CurrentPage++;
    }
}
