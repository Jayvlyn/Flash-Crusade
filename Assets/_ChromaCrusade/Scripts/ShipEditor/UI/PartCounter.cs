using TMPro;
using UnityEngine;

public class PartCounter : MonoBehaviour
{
    public TMP_Text countText;
    public TMP_Text outlineText;

    public void SetCount(int value)
    {
        countText.text = value > 0 ? value.ToString() : "";
        if(outlineText != null) outlineText.text = countText.text;
    }
}
