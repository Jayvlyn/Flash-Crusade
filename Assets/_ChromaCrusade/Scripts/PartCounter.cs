using TMPro;
using UnityEngine;

public class PartCounter : MonoBehaviour
{
    public TMP_Text countText;

    public void SetCount(int value)
    {
        countText.text = value > 0 ? value.ToString() : "";
    }
}
