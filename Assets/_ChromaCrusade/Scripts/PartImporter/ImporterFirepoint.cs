using UnityEngine;
using UnityEngine.EventSystems;

public class ImporterFirepoint : MonoBehaviour, IPointerDownHandler
{
    public FirePoint refFirepoint;

    public GameObject northArrow;
    public GameObject southArrow;
    public GameObject westArrow;
    public GameObject eastArrow;
    public GameObject northWestArrow;
    public GameObject northEastArrow;
    public GameObject southWestArrow;
    public GameObject southEastArrow;

    public void Init()
    {
        DisableArrows();
        northArrow.SetActive(true);
    }

    public void OnNorthArrowButton()
    {
        refFirepoint.fireDirection = FireDirection.North;
        DisableArrows();
        northArrow.SetActive(true);

    }
    public void OnSouthArrowButton()
    {
        refFirepoint.fireDirection = FireDirection.South;
        DisableArrows();
        southArrow.SetActive(true);

    }
    public void OnWestArrowButton()
    {
        refFirepoint.fireDirection = FireDirection.West;
        DisableArrows();
        westArrow.SetActive(true);

    }
    public void OnEastArrowButton()
    {
        refFirepoint.fireDirection = FireDirection.East;
        DisableArrows();
        eastArrow.SetActive(true);

    }
    public void OnNorthWestArrowButton()
    {
        refFirepoint.fireDirection = FireDirection.NorthWest;
        DisableArrows();
        northWestArrow.SetActive(true);
    }
    public void OnNorthEastArrowButton()
    {
        refFirepoint.fireDirection = FireDirection.NorthEast;
        DisableArrows();
        northEastArrow.SetActive(true);

    }
    public void OnSouthWestArrowButton()
    {
        refFirepoint.fireDirection = FireDirection.SouthWest;
        DisableArrows();
        southWestArrow.SetActive(true);
    }
    public void OnSouthEastArrowButton()
    {
        refFirepoint.fireDirection = FireDirection.SouthEast;
        DisableArrows();
        southEastArrow.SetActive(true);
    }

    void DisableArrows()
    {
        northArrow.SetActive(false);
        southArrow.SetActive(false);
        westArrow.SetActive(false);
        eastArrow.SetActive(false);
        northWestArrow.SetActive(false);
        northEastArrow.SetActive(false);
        southWestArrow.SetActive(false);
        southEastArrow.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            EventBus.Publish(new FirepointDeletedEvent
            {
                position = refFirepoint.position
            });

            Destroy(this.gameObject);
        }
    }
}

public struct FirepointDeletedEvent
{
    public Vector2Int position;
}
