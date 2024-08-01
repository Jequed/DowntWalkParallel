using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.UI;

#if UNITY_EDITOR

using UnityEditor;

#endif

public class Editor : MonoBehaviour
{
	public event Action<DWPObject, DWPObject> OnSelectionChanged;

	public event Action<DWPObject> OnSelectionMoved;

	public enum LevelTranslationMode
	{
		none,
		//background,
		phone
	}

	[SerializeField]
	private GameController gameController;

	[SerializeField]
	private EditorLeftPanel leftPanel;

	[SerializeField]
	private EditorRightPanel rightPanel;

    [SerializeField]
    private Toggle logo;

	[SerializeField]
	private GameObject selector;

    [SerializeField]
    private GameObject dialogBackground;
    [SerializeField]
    private EditorDialogMessage dialogMessage;
    [SerializeField]
    private EditorDialogSave dialogSave;
    [SerializeField]
    private EditorDialogOpen dialogOpen;

    private LevelContainer.LayerType layerMode;

	private DWPObject selectedItem;

	private bool dragItem = false;
	private bool isInitialClick = false;

	private ItemInfo.Type currentItemType = ItemInfo.Type.None;

	private Vector3 mouseWorldPosition;
	private Vector3 mousePrefabPosition;
	private Vector3 previousMouseWorldPosition;
	private Vector3 mousePanPositionStart;
    private float mousePinchZoomStart;

	private LineDrawer lineDrawer;

	private DWPObject copiedItem = null;
	
	private ItemInfo.Type currentMiscItemType = ItemInfo.Type.Cloner;

	private LevelTranslationMode leveltranslationMode = LevelTranslationMode.none;

	public ItemInfo.Type CurrentItemType
	{
		get
		{
			return currentItemType;
		}
		set
		{
			var info = GlobalData.GetInfo(value);
			if (info.Layer == LevelContainer.LayerType.misc)
				currentMiscItemType = value;

			currentItemType = value;
		}
	}

	public LevelContainer.LayerType EditingLayer
	{
		get
		{
			return layerMode;
		}
		set
		{
			layerMode = value;
			rightPanel.LayerMode = value;

			switch (value)
			{
				case LevelContainer.LayerType.blocks:
					currentItemType = ItemInfo.Type.Block;
					break;
				case LevelContainer.LayerType.people:
					currentItemType = ItemInfo.Type.NPC;
					break;
				case LevelContainer.LayerType.misc:
					currentItemType = currentMiscItemType;
					break;
				default:
					currentItemType = ItemInfo.Type.None;
					break;
			}
		}
	}

	public LevelContainer LevelContainer
	{
		get
		{
			return gameController.EditorLevelContainer;
		}
		set
		{
			gameController.EditorLevelContainer = value;
		}
	}

	public LevelTranslationMode LevelTranslateMode
	{
		get
		{
			return leveltranslationMode;
		}
		set
		{
			leveltranslationMode = value;
		}
	}

	public bool PlayMode
	{
		get
		{
			return GlobalData.playMode;
		}
		set
		{
			gameController.PlayMode = value;

			leftPanel.gameObject.SetActive(!value);
			rightPanel.gameObject.SetActive(!value);
			selector.gameObject.SetActive(!value);

			if (!value)
				UpdatePeoplePaths();
		}
	}

	public DWPObject SelectedItem
	{
		get
		{
			return selectedItem;
		}
		set
		{
			if (value != selectedItem)
			{
				var previousItem = selectedItem;
				selectedItem = value;
				if (OnSelectionChanged != null)
					OnSelectionChanged(previousItem, selectedItem);

                UpdatePeoplePaths();
			}
		}
	}

	void Start()
	{
		EditingLayer = LevelContainer.LayerType.level;

		lineDrawer = Camera.main.GetComponent<LineDrawer>();

        mousePinchZoomStart = Camera.main.orthographicSize;

        //LevelContainer.Background = "Dorm/DormRoom";
        //LevelContainer.Foreground = "Dorm/DormRoom_F";
    }

	void Update()
	{
        InputUtility.Update();

        #if UNITY_ANDROID
        if (InputUtility.LeftMouse() || InputUtility.DoubleTouch())
        #endif
        {
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(InputUtility.MousePosition); mouseWorldPosition.z = 0.0f;
            mousePrefabPosition = new Vector3(Mathf.Round(mouseWorldPosition.x), Mathf.Round(mouseWorldPosition.y));
        }

        #if UNITY_ANDROID
            if (InputUtility.LeftMouseDown())
                previousMouseWorldPosition = mouseWorldPosition;
        #endif

        if (GlobalData.error != "")
            ShowDialogMessage("Error: " + GlobalData.error);

        if (!PlayMode)
		{
            if (!InputUtility.DoubleTouch())
            {
                switch (layerMode)
                {
                    case LevelContainer.LayerType.level:
                        LevelOptionsControl();
                        break;
                    case LevelContainer.LayerType.blocks:
                    case LevelContainer.LayerType.people:
                    case LevelContainer.LayerType.misc:
                        ItemPlacementControl();
                        break;
                    default:
                        break;
                }
            }

			selector.SetActive(selectedItem);
			if (selectedItem)
				selector.transform.position = selectedItem.transform.position;

			if (Input.GetKeyDown(KeyCode.F2))
				PlayMode = true;
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				PlayMode = false;
		}

		if (!MouseInEditor())
		{
            var mousePanDelta = mousePanPositionStart - mouseWorldPosition;

            #if !UNITY_ANDROID
			    if (InputUtility.MiddleMouse() && mousePanDelta.magnitude < 5.0f)
				    Camera.main.transform.position += mousePanDelta;
			    else
				    mousePanPositionStart = mouseWorldPosition;

			    var mouseDelta = Input.mouseScrollDelta;
			    if (mouseDelta.magnitude > 0.1f)
				    Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - mouseDelta.y, 1.0f, 20.0f);
            #endif
            
            
            if (InputUtility.DoubleTouch())
            {
                if (mousePanDelta.magnitude < 5.0f)
				    Camera.main.transform.position += mousePanDelta;

                Camera.main.orthographicSize = mousePinchZoomStart / InputUtility.PinchDelta;
            }
            else
            {
                #if UNITY_ANDROID
                    mousePanPositionStart = mouseWorldPosition;
                    mousePinchZoomStart = Camera.main.orthographicSize;
                #endif
            }
        }

        #if UNITY_ANDROID
        if (InputUtility.LeftMouse() || InputUtility.DoubleTouch())
        #endif
            previousMouseWorldPosition = mouseWorldPosition;
	}

	private void LevelOptionsControl()
	{
		if (!MouseInEditor() && InputUtility.LeftMouse())
		{
			var delta = mouseWorldPosition - previousMouseWorldPosition;

			switch (leveltranslationMode)
			{
				default:
				case LevelTranslationMode.none:
					break;
				//case LevelTranslationMode.background:
				//	LevelContainer.BackgroundsContainer.transform.position += delta;
				//	break;
				case LevelTranslationMode.phone:
					LevelContainer.Phone.transform.position += delta;
					break;
			}
		}
	}

	private void ItemPlacementControl()
	{
        DWPObject hoverItem = null;
        if (InputUtility.LeftMouse() || InputUtility.RightMouse())
        {
            var ray = Camera.main.ScreenPointToRay(InputUtility.MousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, float.PositiveInfinity, Camera.main.cullingMask);
            
            foreach (var hit in hits)
            {
                var item = hit.transform.GetComponentInParent<DWPObject>();

                if (item != null && item.Info.Layer == layerMode)
                    hoverItem = item;
            }
        }

        if (InputUtility.LeftMouse() && !logo.isOn)
        {
            if (!MouseInEditor())
            {
                if (!dragItem)
                {
                    if (hoverItem == null)
                    {
                        if (currentItemType != ItemInfo.Type.None)
                        {
                            var item = Instantiate(Resources.Load<DWPObject>(GlobalData.GetInfo(currentItemType).PrefabName));

                            GameObject container = null;
                            switch (item.Layer)
                            {
                                case LevelContainer.LayerType.blocks:
                                    container = LevelContainer.BlocksContainer;
                                    break;
                                case LevelContainer.LayerType.people:
                                    container = LevelContainer.PeopleContainer;
                                    (item as Person).RandomizeAppearance();
                                    break;
                                default:
                                case LevelContainer.LayerType.misc:
                                    container = LevelContainer.MiscContainer;
                                    break;
                            }

                            item.transform.parent = container.transform;
                            item.transform.localPosition = mousePrefabPosition;
                        }
                    }
                    else
                    {
                        SelectedItem = hoverItem;

                        if (!isInitialClick)
                            dragItem = true;
                    }
                }
                else
                {
                    if (SelectedItem != null)
                    {
                        selectedItem.transform.localPosition = mousePrefabPosition;
                        if (OnSelectionMoved != null)
                            OnSelectionMoved(selectedItem);
                    }
                    else
                    {
                        dragItem = false;
                    }
                }
            }

            isInitialClick = true;
        }

        if (InputUtility.RightMouse() || (InputUtility.LeftMouse() && logo.isOn))
        {
            if (!MouseInEditor())
            {
                if (hoverItem != null && !(hoverItem is Player))
                    DestroyItem(hoverItem);
                else
                    SelectedItem = null;
            }
		}

		if (!InputUtility.LeftMouse())
		{
			dragItem = false;
			isInitialClick = false;
		}
	}
    
	private bool MouseInEditor()
	{
        //var pointerInputModel = EventSystem.current.currentInputModule as EditorStandaloneInputModule;

        //if (pointerInputModel != null)
        //{
        //    var data = pointerInputModel.GetLastPointerEventData();

        //    if (data != null)
        //        return data.pointerEnter != null;
        //}

        //return false;

        return (InputUtility.MousePosition.x < leftPanel.GetComponent<RectTransform>().sizeDelta.x || InputUtility.MousePosition.x > Screen.width - rightPanel.GetComponent<RectTransform>().sizeDelta.x || dialogBackground.gameObject.activeSelf);
	}

	private void SetBlockVisibility(bool visible)
	{
		var items = LevelContainer.BlocksContainer.GetComponentsInChildren<DWPObject>();

		foreach (var item in items)
		{
			if (item.Info.Layer == LevelContainer.LayerType.blocks)
				SetItemVisibility(item, visible);
		}
	}

	private void SetItemVisibility(DWPObject item, bool visible)
	{
		item.GetComponent<SpriteRenderer>().enabled = visible;
	}

	private void DestroyItem(DWPObject item)
	{
		if (SelectedItem == item)
		{
			var previousSelectedItem = SelectedItem;
			SelectedItem = null;
			OnSelectionChanged(previousSelectedItem, null);
		}

		bool updatePeoplePaths = (item as NPC) != null;

		Destroy(item.gameObject);

		if (updatePeoplePaths)
			UpdatePeoplePaths();
	}

	public void Copy()
	{
		copiedItem = selectedItem;
	}
	public void Paste()
	{
		if (copiedItem)
		{
			var newItem = Instantiate(copiedItem);
			newItem.transform.parent = copiedItem.transform.parent;

			Vector3 currentPosition = newItem.transform.position;
			RaycastHit[] hits;
			while (true)
			{
				var ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(currentPosition));
				hits = Physics.RaycastAll(ray, float.PositiveInfinity, Camera.main.cullingMask);

				bool found = false;
				foreach (var hit in hits)
				{
					var item = hit.transform.GetComponentInParent<DWPObject>();
					if (item != null && item.Info.Layer == newItem.Info.Layer)
					{
						found = true;
						break;
					}
				}

				if (!found)
					break;

				currentPosition += Vector3.right;
			}

			newItem.transform.position = currentPosition;
			SelectedItem = newItem;

			if (newItem.Info.Layer == LevelContainer.LayerType.people)
				UpdatePeoplePaths();
		}
	}

	public void UpdatePeoplePaths()
	{
        lineDrawer.Clear();

		var people = LevelContainer.GetComponentsInChildren<NPC>();

        for (int i = 0; i < 2; i++)
        {
            foreach (var person in people)
            {
                if ((i == 0 && SelectedItem == person) || (i == 1 && SelectedItem != person))
                    continue;

                var lineColor = (i == 0) ? Color.red : Color.blue;

                if (person.enabled && person.Actions.Count > 0)
                {
                    Vector3 lastPosition = person.transform.position;

                    foreach (var action in person.Actions)
                    {
                        if (action.direction != ObjectAction.MovementDirection.None)
                        {
                            var nextPosition = lastPosition + action.DirectionVector * action.distance;

                            lineDrawer.AddPoint(lastPosition, lineColor);
                            lineDrawer.AddPoint(nextPosition, lineColor);

                            const float arrowDistanceAlongLine = 0.6f;
                            const float arrowSize = 0.1f;

                            Vector3 lineVec = (nextPosition - lastPosition).normalized;
                            Vector3 lineNormal = new Vector3(lineVec.y, lineVec.x, lineVec.z);
                            Vector3 arrowCenter = lastPosition + lineVec * arrowDistanceAlongLine * (nextPosition - lastPosition).magnitude;
                            Vector3 leftArrowPosition = arrowCenter - (lineVec + lineNormal) * arrowSize;
                            Vector3 rightArrowPosition = arrowCenter - (lineVec - lineNormal) * arrowSize;

                            lineDrawer.AddPoint(arrowCenter, lineColor);
                            lineDrawer.AddPoint(leftArrowPosition, lineColor);
                            lineDrawer.AddPoint(arrowCenter, lineColor);
                            lineDrawer.AddPoint(rightArrowPosition, lineColor);

                            lastPosition = nextPosition;
                        }
                    }
                }
            }
        }
	}

    public void ShowDialogMessage(string message)
    {
        dialogMessage.Text.text = message;
        dialogMessage.Editor = this;

        dialogBackground.gameObject.SetActive(true);
        dialogMessage.gameObject.SetActive(true);
        dialogSave.gameObject.SetActive(false);
        dialogOpen.gameObject.SetActive(false);
    }
    public void ShowDialogSave()
    {
        dialogSave.transform.parent = dialogBackground.transform;
        dialogSave.Editor = this;

        dialogBackground.gameObject.SetActive(true);
        dialogMessage.gameObject.SetActive(false);
        dialogSave.gameObject.SetActive(true);
        dialogOpen.gameObject.SetActive(false);
    }
    public void ShowDialogOpen()
    {
        dialogOpen.transform.parent = dialogBackground.transform;
        dialogOpen.Editor = this;

        dialogBackground.gameObject.SetActive(true);
        dialogMessage.gameObject.SetActive(false);
        dialogSave.gameObject.SetActive(false);
        dialogOpen.gameObject.SetActive(true);
    }
    public void HideDialog()
    {
        dialogBackground.gameObject.SetActive(false);
        dialogMessage.gameObject.SetActive(false);
        dialogSave.gameObject.SetActive(false);
        dialogOpen.gameObject.SetActive(false);
    }

	public void SaveLevel(string path)
	{
        /*#if UNITY_EDITOR

            var prefab = PrefabUtility.CreateEmptyPrefab(path);

            PrefabUtility.ReplacePrefab(LevelContainer.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);

        #else*/

            MobileIOUtility.SaveLevel(path, LevelContainer);

        //#endif

        ShowDialogMessage("PATH: " + path);
    }
	public void OpenLevel(string path)
	{
        SelectedItem = null;

        /*#if UNITY_EDITOR

            if (path.Contains(".prefab")) 
            {
                Destroy(LevelContainer.gameObject);

                const string resources = "/Resources/";
                const string dotPrefab = ".prefab";
                string resourcePath = path.Substring(path.LastIndexOf(resources) + resources.Length);
                var prefab = Resources.Load<LevelContainer>(resourcePath.Substring(0, resourcePath.Length - dotPrefab.Length));

                LevelContainer = Instantiate(prefab);
            }
            else if (path.Contains(".txt"))
            {
                MobileIOUtility.OpenLevel(path, LevelContainer);
            }

        #else*/

            MobileIOUtility.OpenLevel(path, LevelContainer);

        //#endif

        EditingLayer = layerMode;

        UpdatePeoplePaths();
    }
}