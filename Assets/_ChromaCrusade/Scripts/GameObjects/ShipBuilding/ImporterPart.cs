using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class ImporterPart : MonoBehaviour
{
    public enum PartType { Select, Core, Wing, Cabin, Weapon, Utility}
    #region Conditions
    private bool TypeSelected() => partType != PartType.Select;
    private bool IsCore() => partType == PartType.Core;
    private bool IsWing() => partType == PartType.Wing;
    private bool IsWeapon() => partType == PartType.Weapon;
    private bool IsCabin() => partType == PartType.Cabin;
    private bool IsUtility() => partType == PartType.Utility;
    private bool SpriteGiven() => partSprite != null;
    private bool IsValidDamage(int value) => value >= 0;
    #endregion
    [OnValueChanged("OnSpriteChangedCallback")]
    [ValidateInput(nameof(SpriteGiven), "Must assign sprite!")]
    public Sprite partSprite;
    [ValidateInput(nameof(TypeSelected), "Must select part type!")]
    public PartType partType = PartType.Select;
    //public float mass = 1;

    [ShowIf(nameof(IsWeapon)), Header("Weapon Attributes")]
    [ValidateInput(nameof(IsValidDamage), "Damage must be >= 0")]
    public int damage = 10;

    [ShowIf(nameof(IsCore)), Header("Energy Core Attributes")]
    public int energy = 100;

    [ShowIf(nameof(IsWing)), Header("Wing Attributes")]
    public int mobility = 1;

    [ShowIf(nameof(IsCabin)), Header("Pilot Cabin Attributes")]
    public int turning = 1;

    //[ShowIf(nameof(IsUtility)), Header("Utility Attributes")]
    //public int 

    [HideInInspector] public Image image;

    private void Start()
    {
        if (partSprite != null)
            image.sprite = partSprite;
        else
            image.enabled = false;
    }

    private void OnSpriteChangedCallback()
    {
        if (image == null) return;

        if (partSprite != null)
        {
            image.sprite = partSprite;
            image.enabled = true;
        }
        else
            image.enabled = false;
    }    

    [Button("Save Part")]
    private void SavePart()
    {
        // save all defined data to json
    }
}
