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

        // Match anchor setup
        self.anchorMin = target.anchorMin;
        self.anchorMax = target.anchorMax;

        // Match pivot
        self.pivot = target.pivot;

        // Match anchored position
        self.anchoredPosition = target.anchoredPosition;

        // Match size
        self.sizeDelta = target.sizeDelta;

        // Match rotation & scale just in case it's not uniform
        self.localRotation = target.localRotation;
        self.localScale = target.localScale;
    }
}
