using UnityEngine;

public class PartOrganizer : MonoBehaviour
{
    public NavItem[] partSelectors; // usually 18 elements
                                    //partSelectors[0].onSelected.AddListener()

    public ShipPartData[] cabins;
    public ShipPartData[] cores;
    public ShipPartData[] wings;
    public ShipPartData[] weapons;
    public ShipPartData[] utilities;

    public RectTransform itemPanel;

    private int pageCount => partSelectors.Length;

    private void Awake()
    {
        cabins = PartDatabase.Instance.GetPartsOfType(PartType.Cabin);
        cores = PartDatabase.Instance.GetPartsOfType(PartType.Core);
        wings = PartDatabase.Instance.GetPartsOfType(PartType.Wing);
        weapons = PartDatabase.Instance.GetPartsOfType(PartType.Weapon);
        utilities = PartDatabase.Instance.GetPartsOfType(PartType.Utility);
    }

    #region Tab Event Responses

    public void OnCabinTabSelected()
    {
    }

    public void OnCoreTabSelected()
    {

    }

    public void OnWingTabSelected()
    {

    }

    public void OnWeaponTabSelected()
    {
        Debug.Log(weapons.Length);
        for (int i = 0; i < weapons.Length; i++)
        {
            GameObject obj = Instantiate(Assets.i.editorShipPartPrefab, itemPanel);
            EditorShipPart part = obj.GetComponent<EditorShipPart>();
            part.Init(weapons[i]);
            part.rtf.target = partSelectors[i].rect;
            part.rtf.enabled = true;
        }
    }

    public void OnUtilityTabSelected()
    {

    }

    #endregion
}
