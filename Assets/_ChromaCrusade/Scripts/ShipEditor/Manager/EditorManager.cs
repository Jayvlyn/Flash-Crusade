using System.Collections;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class EditorManager : MonoBehaviour, ICommandContext
{
    [SerializeField] NavItem buildWindow;
    [SerializeField] NavItem exitItem;
    [SerializeField] BuildArea buildArea;
    [SerializeField] ShipNameValidator nameValidator;
    [SerializeField] TMP_Text validationResponseText;
    [SerializeField] RectTransform presetMenu;
    [SerializeField] NavItem openPresetsButton;
    [SerializeField] NavItem savePresetButton;
    [SerializeField] NavItem clearButton;
    [SerializeField] NavItem completeButton;
    [SerializeField] ShipPresetManager presetManager;

    [SerializeField] EditorNavVisualizer visualizer;
    [SerializeField] EditorUINavigator uiNav;
    [SerializeField] EditorGridNavigator gridNav;
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] PartDestroyer partDestroyer;
    [SerializeField] PartPlacer partPlacer;
    [SerializeField] PartGrabber partGrabber;
    [SerializeField] PartTransformer partTransformer;

    #region Lifecycle

    void OnEnable()
    {
        SubscribeToInputEvents();

        EventBus.Subscribe<EnterInputFieldEvent>(OnEnterInputField);
        EventBus.Subscribe<InventoryPartGrabbedEvent>(OnInventoryPartGrabbedEvent);
        EventBus.Subscribe<PresetSelectedEvent>(OnPresetSelected);
        EventBus.Subscribe<SavePresetEvent>(OnSaveBuildAsPreset);
#if UNITY_EDITOR
        EventBus.Subscribe<SaveDevPresetEvent>(SaveBuildAsDevPreset);
#endif
    }

    void OnDisable()
    {
        UnsubscribeFromInputEvents();

        EventBus.Unsubscribe<EnterInputFieldEvent>(OnEnterInputField);
        EventBus.Unsubscribe<InventoryPartGrabbedEvent>(OnInventoryPartGrabbedEvent);
        EventBus.Unsubscribe<PresetSelectedEvent>(OnPresetSelected);
        EventBus.Unsubscribe<SavePresetEvent>(OnSaveBuildAsPreset);
#if UNITY_EDITOR
        EventBus.Unsubscribe<SaveDevPresetEvent>(SaveBuildAsDevPreset);
#endif
    }

    void Awake()
    {
        EditorState.Init();

        uiNav.gridNav = (IGridNavigator)gridNav;

        gridNav.uiNav = (IUINavigator)uiNav;

        partDestroyer.grabber = (IPartGrabber) partGrabber;
        partDestroyer.transformer = (IPartTransformer) partTransformer;
        partDestroyer.placer = (IPartPlacer) partPlacer;
        partDestroyer.visualizer = (IEditorNavVisualizer) visualizer;
        partDestroyer.gridNav = (IGridNavigator) gridNav;
        partDestroyer.inventory = (IInventoryManager) inventoryManager;

        partPlacer.buildArea = buildArea;

        partGrabber.buildArea = buildArea;
        partGrabber.uiNav = (IUINavigator)uiNav;
        partGrabber.visualizer = (IEditorNavVisualizer)visualizer;

        partTransformer.visualizer = (IEditorNavVisualizer)visualizer;

        visualizer.gameObject.SetActive(true);
        gridNav.ResetGridPosition();
        uiNav.Init();
    }

    #endregion

    #region IGridNavigator

    public void InitGridMode() => gridNav.InitGridMode();

    public void TriggerGridNav(Vector2 dir) => gridNav.TriggerGridNav(dir);

    public Vector2Int GetCurrentGridCell() => gridNav.GetCurrentGridCell();

    public void SwitchToItemMode() => gridNav.SwitchToItemMode();

    public void NavToCell(Vector2Int cell) => gridNav.NavToCell(cell);

    public void ResetGridPosition() => gridNav.ResetGridPosition();

    #endregion

    #region IUINavigator

    public void Init() => uiNav.Init();

    public void TriggerItemNav(Vector2 dir) => uiNav.TriggerItemNav(dir);

    public void SwitchOff() => uiNav.SwitchOff();
    
    void GoBack()
    {
        if(NavState.inPopupScreen)
        {
            EventBus.Publish(new CloseConfirmScreenEvent());
            EventBus.Publish(new CloseMessageScreenEvent());
        }
        else if(EditorState.inPresetMenu)
        {
            ClosePresetMenu();
        }
        else if (NavState.inInputField)
        {
            NavState.inInputField = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (EditorState.navMode == NavMode.Item)
        {
            if (NavState.HoveredItem == exitItem) exitItem.OnSelected();
            else NavToItem(exitItem);
        }
        else if (EditorState.navMode == NavMode.Grid)
        {
            NavState.LastHoveredItem = null;
            CommandHistory.Execute(new ExitGridModeCommand(this, EditorState.heldPart));
        }
    }

    void ToggleNavMode()
    {
        if (EditorState.inPresetMenu || NavState.inPopupScreen) return;

        if (EditorState.navMode == NavMode.Item) CommandHistory.Execute(new EnterGridModeCommand(this));
        else if (EditorState.navMode == NavMode.Grid) CommandHistory.Execute(new ExitGridModeCommand(this, EditorState.heldPart));
    }

    public void NavToItem(NavItem item) => uiNav.NavToItem(item);

    public void HighlightItem(NavItem newItem) => visualizer.HighlightItem(newItem);

    public void HighlightItemImmediate(NavItem newItem) => visualizer.HighlightItemImmediate(newItem);

    public void HighlightItemLerp(NavItem newItem) => visualizer.HighlightItemLerp(newItem);

    #endregion

    #region IPartDestroyer

    public void DestroyPart(EditorShipPart part) => partDestroyer.DestroyPart(part);

    public void HandleUndoRoutine(bool wasPlaced, ShipPartData partData, Vector2Int partPosition, Vector2Int startCell, float rotation, bool xFlipped = false, bool yFlipped = false) =>
        partDestroyer.HandleUndoRoutine(wasPlaced, partData, partPosition, startCell, rotation, xFlipped, yFlipped);

    #endregion

    #region IPartPlacer

    public EditorShipPart GetHeldPart() => partPlacer.GetHeldPart();

    public void PlacePart(EditorShipPart part, Vector2Int cell) => partPlacer.PlacePart(part, cell);

    void TryPlacePart()
    {
        if (buildArea.CanPlacePart(EditorState.heldPart, gridNav.GetCurrentGridCell()))
        {
            CommandHistory.Execute(new PlaceCommand(this, gridNav.GetCurrentGridCell()));
        }
    }

    bool placeQueued;
    IEnumerator TryPlacePartDelayed()
    {
        if (placeQueued) yield break; // prevents spam stacking
        placeQueued = true;

        while (NavVisualizer.IsLerping || visualizer.IsRotateLerping || visualizer.IsFlipLerping)
            yield return null;

        TryPlacePart(); // safe now
        placeQueued = false;
    }

    #endregion

    #region IPartGrabber

    public EditorShipPart GrabFromGrid(Vector2Int cell) => partGrabber.GrabFromGrid(cell);

    public void GrabImmediate(EditorShipPart part, bool fromInv) => partGrabber.GrabImmediate(part, fromInv);

    public void GrabFrameLate(EditorShipPart part, bool fromInv) => partGrabber.GrabFrameLate(part, fromInv);

    public void GrabWithLerp(EditorShipPart part, bool fromInv) => partGrabber.GrabWithLerp(part, fromInv);

    void TryGrabPart()
    {
        EditorShipPart part = buildArea.GetPartAtCell(gridNav.GetCurrentGridCell());
        if (part) CommandHistory.Execute(new GrabCommand(this, part.position, gridNav.GetCurrentGridCell()));
    }

    #endregion

    #region IInventoryManager

    public bool TryTakePart(ShipPartData data, out EditorShipPart part) => inventoryManager.TryTakePart(data, out part);

    public void AddPart(ShipPartData data) => inventoryManager.AddPart(data);

    public void SetPartToDefaultStart(EditorShipPart part) => inventoryManager.SetPartToDefaultStart(part);

    #endregion

    #region IPartTransformer

    public void RotatePart(float angle) => partTransformer.RotatePart(angle);

    public void FlipPart(FlipAxis axis) => partTransformer.FlipPart(axis);

    public void RestoreHeldPartTransformations(float rotation, bool xFlipped = false, bool yFlipped = false) =>
        partTransformer.RestoreHeldPartTransformations(rotation, xFlipped, yFlipped);

    public void RestorePartTransformations(EditorShipPart part, float rotation, bool xFlipped = false, bool yFlipped = false) =>
        partTransformer.RestorePartTransformations(part, rotation, xFlipped, yFlipped);

    #endregion

    #region IVisualizer

    public void SetExpanded(bool expanded) => visualizer.SetExpanded(expanded);

    public void HighlightCellImmediate(Vector2Int cell) => visualizer.HighlightCellImmediate(cell);

    public void UpdateWithRectImmediate(RectTransform rect) => visualizer.UpdateWithRectImmediate(rect);

    public void MatchRectScale(RectTransform rect) => visualizer.MatchRectScale(rect);

    public void ResetScale() => visualizer.ResetScale();

    public RectTransform GetRect() => visualizer.GetRect();

    public Coroutine LerpWithRect(RectTransform rt) => visualizer.LerpWithRect(rt);

    public void Flip(FlipAxis axis) => visualizer.Flip(axis);

    public void FlipImmediate(FlipAxis axis) => visualizer.FlipImmediate(axis);

    public void Rotate(float angle) => visualizer.Rotate(angle);

    public void RotateImmediate(float angle) => visualizer.RotateImmediate(angle);

    #endregion

    #region Event Handling

    void SubscribeToInputEvents()
    {
        EventBus.Subscribe<SubmitInputEvent>(OnSubmitInputEvent);
        EventBus.Subscribe<CancelInputEvent>(OnCancelInputEvent);
        EventBus.Subscribe<ModeInputEvent>(OnModeInputEvent);
        EventBus.Subscribe<UndoInputEvent>(OnUndoInputEvent);
        EventBus.Subscribe<RedoInputEvent>(OnRedoInputEvent);
        EventBus.Subscribe<DeleteInputEvent>(OnDeleteInputEvent);
        EventBus.Subscribe<ResetInputEvent>(OnResetInputEvent);
        EventBus.Subscribe<NavigateInputEvent>(OnNavigateInputEvent);
        EventBus.Subscribe<ModifyInputEvent>(OnModifyInputEvent);
        EventBus.Subscribe<FlipInputEvent>(OnFlipInputEvent);
        EventBus.Subscribe<RotateInputEvent>(OnRotateInputEvent);
    }

    void UnsubscribeFromInputEvents()
    {
        EventBus.Unsubscribe<SubmitInputEvent>(OnSubmitInputEvent);
        EventBus.Unsubscribe<CancelInputEvent>(OnCancelInputEvent);
        EventBus.Unsubscribe<ModeInputEvent>(OnModeInputEvent);
        EventBus.Unsubscribe<UndoInputEvent>(OnUndoInputEvent);
        EventBus.Unsubscribe<RedoInputEvent>(OnRedoInputEvent);
        EventBus.Unsubscribe<DeleteInputEvent>(OnDeleteInputEvent);
        EventBus.Unsubscribe<ResetInputEvent>(OnResetInputEvent);
        EventBus.Unsubscribe<NavigateInputEvent>(OnNavigateInputEvent);
        EventBus.Unsubscribe<ModifyInputEvent>(OnModifyInputEvent);
        EventBus.Unsubscribe<FlipInputEvent>(OnFlipInputEvent);
        EventBus.Unsubscribe<RotateInputEvent>(OnRotateInputEvent);
    }

    void OnSubmitInputEvent(SubmitInputEvent e)
    {
        if (EditorState.navMode == NavMode.Item)
        {
            if (NavState.HoveredItem != null)
            {
                if (NavState.HoveredItem == buildWindow) CommandHistory.Execute(new EnterGridModeCommand(this));
                else NavState.HoveredItem.OnSelected();
            }
        }
        else if (EditorState.navMode == NavMode.Grid)
        {
            if (EditorState.midGrab) return;
            if (EditorState.heldPart != null)
            {
                if (UIManager.Smoothing)
                    StartCoroutine(TryPlacePartDelayed());
                else
                    TryPlacePart();
            }
            else
            {
                TryGrabPart();
            }
        }
    }

    void OnCancelInputEvent(CancelInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || NavVisualizer.IsLerping || EditorState.midUndoDelete)
            return;
        
        GoBack();
    }

    void OnModeInputEvent(ModeInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || NavVisualizer.IsLerping || EditorState.midUndoDelete)
            return;

        ToggleNavMode();
    }

    void OnUndoInputEvent(UndoInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || NavVisualizer.IsLerping || EditorState.midUndoDelete)
            return;

        CommandHistory.Undo();
    }

    void OnRedoInputEvent(RedoInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || NavVisualizer.IsLerping || EditorState.midUndoDelete)
            return;

        CommandHistory.Redo();
    }

    void OnDeleteInputEvent(DeleteInputEvent e)
    {
        if (NavState.inPopupScreen) return;

        if (EditorState.inPresetMenu)
        {
            if (presetManager.HoveredPreset == null) return;

            string presetName = presetManager.HoveredPreset.ShipName;
            
            if(presetManager.DevPresetNameExists(presetName))
            {
                EventBus.Publish(new OpenMessageScreenEvent
                {
                    message = $"\"{presetName}\" is a built-in preset that can't be deleted."
                });
            }
            else
            {
                presetToDelete = presetName;
                EventBus.Publish(new OpenConfirmScreenEvent
                {
                    message = $"Are you sure you want to permanently delete the preset \"{presetName}\"?",
                    action = DeletePreset
                });
            }
        }
        else if (EditorState.navMode == NavMode.Grid)
        {
            EditorShipPart part = buildArea.GetPartAtCell(gridNav.GetCurrentGridCell());
            if (EditorState.heldPart == null && part == null) return;
            CommandHistory.Execute(new DeleteCommand(this, gridNav.GetCurrentGridCell()));
        }
    }

    void OnResetInputEvent(ResetInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || NavVisualizer.IsLerping || EditorState.midUndoDelete)
            return;

        if (EditorState.navMode == NavMode.Item)
        {
            NavState.HoveredItem = null;
            NavState.LastHoveredItem = null;
            ResetGridPosition();
            uiNav.Init();
        }
        else if (EditorState.navMode == NavMode.Grid)
        {
            CommandHistory.Execute(new ResetCommand(this));
        }
    }

    void OnNavigateInputEvent(NavigateInputEvent e)
    {
        if (EditorState.midGrab || ZoomController.MidZoom || NavState.Scrolling) return;

        Vector2 dir = e.dir;

        dir.x = Mathf.RoundToInt(dir.x);
        dir.y = Mathf.RoundToInt(dir.y);
        if (modifyHeld)
        {
            dir.x *= 3;
            dir.y *= 3;
        }

        if (EditorState.navMode == NavMode.Grid) CommandHistory.Execute(new NavigateCommand(this, dir));
        else
        {
            uiNav.TriggerItemNav(dir);
            if (modifyHeld) uiNav.TriggerItemNav(dir); // double trigger when modify held (dir mag doesnt matter for item mode)
        }
    }

    bool modifyHeld;
    void OnModifyInputEvent(ModifyInputEvent e) => modifyHeld = e.held;

    void OnFlipInputEvent(FlipInputEvent e)
    {
        if (EditorState.heldPart == null) return;
        if (EditorState.navMode != NavMode.Grid) return;
        if (visualizer.IsFlipLerping) return;

        CommandHistory.Execute(new FlipCommand(this, e.flipAxis));
    }

    void OnRotateInputEvent(RotateInputEvent e)
    {
        if (EditorState.heldPart == null) return;
        if (EditorState.navMode != NavMode.Grid) return;
        if (visualizer.IsRotateLerping) return;

        float angle = 0;
        if (e.rotationDirection == RotationDirection.Clockwise) angle = 90;
        else angle = -90;

        if (modifyHeld) angle *= 1.999f; // comes out to ~179.9 so that lerp happens in correct direction, will snap to int later
        CommandHistory.Execute(new RotateCommand(this, angle));
    }

    void OnEnterInputField(EnterInputFieldEvent e) => NavState.inInputField = true;

    void OnInventoryPartGrabbedEvent(InventoryPartGrabbedEvent e) => CommandHistory.Execute(new InventoryGrabCommand(this, e.part));

    public void OnExitButtonSelected()
    {
        EventBus.Publish(new OpenConfirmScreenEvent
        {
            message = "Are you sure you want to exit the editor? The current build will be discarded.",
            action = ExitEditor
        });
    }

    public void OnCompleteButtonSelected()
    {
        string validationResult = ValidateBuild();

        if (validationResult.Equals("Valid"))
        {
            if (EditorState.context == EditorContext.Creative)
            {
                OpenPresetMenu(); // replace this with a confirmation to save as preset
            }
            else
            {
                EventBus.Publish(new OpenConfirmScreenEvent
                {
                    message = $"{nameValidator.GetText()} is complete! Finalize the build?",
                    action = SaveBuildAsPreset
                });
            }
        }
        else
        {
            SetResponseText(validationResult);
            SetResponseColor(Assets.i.uiRed);
        }
    }

    void OnPresetSelected(PresetSelectedEvent e)
    {
        presetToLoad = e.presetName;

        EventBus.Publish(new OpenConfirmScreenEvent
        {
            message = $"Load {presetToLoad}? Anything in the build area will be replaced",
            action = LoadPresetAfterConfirmation,
            yesNavItem = buildWindow,
            noNavItem = NavState.PrevScreenItem
        });
    }

    void OnSaveBuildAsPreset(SavePresetEvent e) => SaveBuildAsPresetAfterValidation();

    #endregion

    #region Validation Response

    void SetResponseText(string content, float duration = 5)
    {
        validationResponseText.text = content;
        if(validationTextClearerCoroutine != null) StopCoroutine(validationTextClearerCoroutine);
        validationTextClearerCoroutine = StartCoroutine(ValidationResponseTextClearer(duration));
    }

    void SetResponseColor(Color color)
    {
        validationResponseText.color = color;
    }

    Coroutine validationTextClearerCoroutine;
    IEnumerator ValidationResponseTextClearer(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        validationResponseText.text = "";
    }

    #endregion

    #region Clearing
    public void OnClearButtonPressed()
    {
        EventBus.Publish(new OpenConfirmScreenEvent {
            message = "Clear the build area?",
            action = ClearBuildArea
        });
    }

    void ClearBuildArea()
    {
        nameValidator.SetText(string.Empty);

        EditorShipPart[] parts = buildArea.ClearParts();
        foreach (EditorShipPart part in parts)
            inventoryManager.AddPart(part.partData);

        SetResponseText("Build area cleared!");
        SetResponseColor(Assets.i.uiGreen);

        CommandHistory.ClearStacks();
    }
    #endregion

    void ExitEditor()
    {
        ClearBuildArea();
        SceneManager.LoadScene("Scene_MainMenu");
    }

    public string ValidateBuild()
    {
        ShipBuildValidator validator = new ShipBuildValidator(buildArea, nameValidator);
        return validator.ValidateCurrentBuild();
    }

    public Texture2D GetShipTexture()
    {
        PartSpriteCombiner spriteCombiner = new PartSpriteCombiner(buildArea.Parts);
        return spriteCombiner.CreateCombinedTexture();
    }

    public UIShipData GetUIShipData()
    {
        Texture2D texture = GetShipTexture();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        return new UIShipData(sprite, nameValidator.GetText());
    }

    public void OpenPresetMenu()
    {
        EditorState.inPresetMenu = true;
        presetMenu.gameObject.SetActive(true);
        NavToItem(savePresetButton);

        string valitation = ValidateBuild();
        if (valitation.Equals("Valid"))
            presetManager.DisplayPresets(GetUIShipData());
        else
            presetManager.DisplayPresets();
    }

    public void ClosePresetMenu()
    {
        EditorState.inPresetMenu = false;
        presetMenu.gameObject.SetActive(false);
        NavToItem(openPresetsButton);
    }

    public void SaveBuildAsPresetAfterValidation()
    {
        string validation = ValidateBuild();
        if (!validation.Equals("Valid")) return;

        string presetName = nameValidator.GetText();

        if(presetManager.DevPresetNameExists(presetName))
        {
            EventBus.Publish(new OpenMessageScreenEvent
            {
                message = $"\"{presetName}\" is a built-in preset that can't be overwritten.\nRename your build to save it as a preset"
            });
        }
        else
        {
            string confirmMessage;
            if (presetManager.PlayerPresetNameExists(presetName))
                confirmMessage = $"The preset \"{presetName}\" already exists, overwrite it?";
            else
                confirmMessage = $"Save {presetName} as a preset? You can delete it later.";

            EventBus.Publish(new OpenConfirmScreenEvent
            {
                message = confirmMessage,
                action = SaveBuildAsPreset
            });
        }
    }

    public void SaveBuildAsPreset()
    {
        ShipSaveLoader ShipSL = new ShipSaveLoader();
        ShipSL.SaveBuildAsPreset(GetUIShipData(), buildArea.Parts);
        presetManager.DisplayPresets();
    }

#if UNITY_EDITOR
    public void SaveBuildAsDevPreset(SaveDevPresetEvent e)
    {
        ShipSaveLoader ShipSL = new ShipSaveLoader();
        ShipSL.SaveBuildAsPreset(GetUIShipData(), buildArea.Parts, true);
        presetManager.DisplayPresets();
    }
#endif

    // loads a player save ship without taking from inventory
    public void LoadBuild(string shipName)
    {
        ShipSaveLoader ShipSL = new ShipSaveLoader();
        //ShipSL.GetShipBuild(shipName, **activeSave??**);
    }

    // loads preset. In creative for free, otherwise requires parts
    string presetToLoad;
    public void LoadPresetAfterConfirmation() => LoadBuildPreset(presetToLoad);

    public void LoadBuildPreset(string shipName)
    {
        ClearBuildArea();

        ShipSaveLoader ShipSL = new ShipSaveLoader();

        bool dev = presetManager.DevPresetNameExists(shipName);

        ShipSave save = ShipSL.GetShipPreset(shipName, dev);

        nameValidator.SetText(shipName);

        int partCount = 0;
        int successCount = 0;

        foreach (PartStruct part in save.partList)
        {
            partCount++;

            if (RestorePart(part))
                successCount++;
            
        }

        if(successCount == 0) // couldnt load ANY parts
        {
            SetResponseText($"Preset \"{shipName}\" could not be loaded, no necessary parts were found in inventory.");
            SetResponseColor(Assets.i.uiRed);
        }
        else if(partCount > successCount) // couldnt load all parts
        {
            SetResponseText($"Preset \"{shipName}\" partially loaded, not enough parts in inventory to finish.");
            SetResponseColor(Assets.i.uiRed);
        }
        else
        {
            SetResponseText($"Preset \"{shipName}\" loaded!");
            SetResponseColor(Assets.i.uiGreen);
        }

        ClosePresetMenu();
        CommandHistory.ClearStacks();
    }

    bool RestorePart(PartStruct partStruct)
    {
        ShipPartData partData = PartDatabase.Instance.Get(partStruct.partName);
        bool success = inventoryManager.TryTakePart(partData, out EditorShipPart part);

        if (!success) return false;

        partTransformer.RestorePartTransformations(part, partStruct.rotation, partStruct.xFlipped, partStruct.yFlipped);

        part.transform.SetParent(buildArea.transform);
        part.rect.sizeDelta = gridNav.centerGridCell.sizeDelta*3;
        part.transform.localPosition = new Vector2(partStruct.xPos * gridNav.centerGridCell.sizeDelta.x, partStruct.yPos * gridNav.centerGridCell.sizeDelta.y);

        part.rect.pivot = new Vector2(0.5f, 0.5f);
        part.rect.localEulerAngles = new Vector3(0, 0, part.rect.localEulerAngles.z - part.Rotation);

        part.transform.localScale = new Vector3(
            part.xFlipped ? -1 : 1,
            part.yFlipped ? -1 : 1,
            1);

        partPlacer.PlacePart(part, new Vector2Int(partStruct.xPos, partStruct.yPos));

        return true;
    }

    string presetToDelete;
    void DeletePreset()
    {
        if (presetToDelete == null) return;
        presetManager.DeletePreset(presetToDelete);
        presetToDelete = null;
    }
}