using System.Collections;
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

    private void LateUpdate()
    {
        Follow();
    }

    private void Start()
    {
        if(startOnly)
        {
            StartCoroutine(Deactivator());
        }
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

    IEnumerator Deactivator()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        this.enabled = false;
    }
}
