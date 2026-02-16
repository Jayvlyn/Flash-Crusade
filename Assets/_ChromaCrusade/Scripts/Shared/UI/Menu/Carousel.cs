using UnityEngine;
using System.Collections.Generic;

public class Carousel : MonoBehaviour
{
    List<RectTransform> items;
    List<CarouselSlot> slots;

    int selectedIndex;
    void UpdateLayout()
    {
        int count = items.Count;

        for (int i = 0; i < count; i++)
        {
            int rel = WrapDistance(i - selectedIndex, count);
            int abs = Mathf.Abs(rel);

            RectTransform rt = items[i];

            if (abs > 2)
            {
                rt.gameObject.SetActive(false);
                continue;
            }

            rt.gameObject.SetActive(true);

            CarouselSlot slot = slots[abs];

            float dir = Mathf.Sign(rel);

            rt.anchoredPosition = new Vector2(
                slot.xOffset * dir,
                0f
            );

            rt.localScale = Vector3.one * slot.scale;

            rt.SetSiblingIndex(slot.siblingOrder);
        }
    }

    int WrapDistance(int delta, int count)
    {
        delta %= count;

        if (delta > count / 2) delta -= count;
        if (delta < -count / 2) delta += count;

        return delta;
    }
}

[System.Serializable]
public struct CarouselSlot
{
    public float xOffset;
    public float scale;
    public int siblingOrder;
}

