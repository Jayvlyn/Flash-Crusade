using UnityEngine;

//[ExecuteAlways]
public class RectTransformFollower : MonoBehaviour
{
    [SerializeField] private RectTransform target;

    private RectTransform self;

    private void Awake()
    {
        self = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (!target || !self) return;

        self.anchorMin = target.anchorMin;
        self.anchorMax = target.anchorMax;

        self.pivot = target.pivot;

        self.anchoredPosition = target.anchoredPosition;

        self.sizeDelta = target.sizeDelta;

        self.localRotation = target.localRotation;
        self.localScale = target.localScale;
    }
}
