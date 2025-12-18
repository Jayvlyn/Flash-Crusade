using UnityEngine;

//[ExecuteAlways]
public class RectTransformFollower : MonoBehaviour
{
    public bool startOnly = false;
    public bool stretch = false;

    public RectTransform target;

    private RectTransform self;


    private void Awake()
    {
        self = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Follow();
    }

    private void LateUpdate()
    {
        if(!startOnly) Follow();
    }

    public void Follow()
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


        self.localEulerAngles = target.localEulerAngles;
    }
}
