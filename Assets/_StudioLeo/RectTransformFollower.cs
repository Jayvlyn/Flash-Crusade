using UnityEngine;

//[ExecuteAlways]
public class RectTransformFollower : MonoBehaviour
{
    public bool stretch = false;

    public RectTransform target;

    private RectTransform self;


    private void Awake()
    {
        self = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (!target || !self) return;

        if (stretch)
        {
            self.anchorMin = target.anchorMin;
            self.anchorMax = target.anchorMax;
            self.sizeDelta = target.sizeDelta;
            self.localScale = target.localScale;
        }

        self.pivot = target.pivot;
        self.anchoredPosition = target.anchoredPosition;


        self.localRotation = target.localRotation;
    }
}
