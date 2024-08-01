using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR

using UnityEditor;

#endif

public class EditorLeftPanel : EditorView
{
	[SerializeField]
	private Button levelButton;

	[SerializeField]
	private Button blocksButton;

	[SerializeField]
	private Button peopleButton;

	[SerializeField]
	private Button miscButton;

	[SerializeField]
	private Button saveButton;

	[SerializeField]
	private Button openButton;

	[SerializeField]
	private Button copyButton;

	[SerializeField]
	private Button pasteButton;

	[SerializeField]
	private Button playButton;

	[SerializeField]
	private Toggle debugModeToggle;

	protected override void Start()
	{
		base.Start();

		levelButton.onClick.AddListener(LevelButton_OnClick);
		blocksButton.onClick.AddListener(BlockButton_OnClick);
		peopleButton.onClick.AddListener(PeopleButton_OnClick);
		miscButton.onClick.AddListener(MiscButton_OnClick);
		saveButton.onClick.AddListener(SaveButton_OnClick);
		openButton.onClick.AddListener(OpenButton_OnClick);
		copyButton.onClick.AddListener(CopyButton_OnClick);
		pasteButton.onClick.AddListener(PasteButton_OnClick);
		playButton.onClick.AddListener(PlayButton_OnClick);
		debugModeToggle.onValueChanged.AddListener(DebugModeToggle_OnValueChanged);

		debugModeToggle.isOn = GlobalData.debugMode;

        UpdateLayerButtons();
    }

	void OnDestroy()
	{
		levelButton.onClick.RemoveListener(LevelButton_OnClick);
		blocksButton.onClick.RemoveListener(BlockButton_OnClick);
		peopleButton.onClick.RemoveListener(PeopleButton_OnClick);
		miscButton.onClick.RemoveListener(MiscButton_OnClick);
		saveButton.onClick.RemoveListener(CopyButton_OnClick);
		openButton.onClick.RemoveListener(PasteButton_OnClick);
		playButton.onClick.RemoveListener(PlayButton_OnClick);
		debugModeToggle.onValueChanged.RemoveListener(DebugModeToggle_OnValueChanged);
	}

	private string GetLocalPath(string fullPath)
	{
		return fullPath.Substring(fullPath.LastIndexOf("/Assets/") + 1);
	}

	private void LevelButton_OnClick()
	{
		editor.EditingLayer = LevelContainer.LayerType.level;
        UpdateLayerButtons();
	}
	private void BlockButton_OnClick()
	{
		editor.EditingLayer = LevelContainer.LayerType.blocks;
        UpdateLayerButtons();
    }
	private void PeopleButton_OnClick()
	{
		editor.EditingLayer = LevelContainer.LayerType.people;
        UpdateLayerButtons();
    }
	private void MiscButton_OnClick()
	{
		editor.EditingLayer = LevelContainer.LayerType.misc;
        UpdateLayerButtons();
    }
    private void UpdateLayerButtons()
    {
        levelButton.interactable = (editor.EditingLayer != LevelContainer.LayerType.level);
        blocksButton.interactable = (editor.EditingLayer != LevelContainer.LayerType.blocks);
        peopleButton.interactable = (editor.EditingLayer != LevelContainer.LayerType.people);
        miscButton.interactable = (editor.EditingLayer != LevelContainer.LayerType.misc);
    }

	private void SaveButton_OnClick()
	{
        /*#if UNITY_EDITOR

            var path = GetLocalPath(EditorUtility.SaveFilePanel("Save level", "Assets/Resources/Prefabs/Levels", "Level", "prefab"));

            if (path.Length != 0)
                editor.SaveLevel(path);

        #else*/

            editor.ShowDialogSave();

        //#endif
    }
	private void OpenButton_OnClick()
	{
        /*#if UNITY_EDITOR

            var path = GetLocalPath(EditorUtility.OpenFilePanel("Open level", "Assets/Resources/Prefabs/Levels", "prefab,txt"));

            if (path.Length != 0)
                editor.OpenLevel(path);

        #else*/

            editor.ShowDialogOpen();

        //#endif
    }

	private void CopyButton_OnClick()
	{
		editor.Copy();
	}
	private void PasteButton_OnClick()
	{
		editor.Paste();
	}

	private void PlayButton_OnClick()
	{
		editor.PlayMode = true;
	}

	private void DebugModeToggle_OnValueChanged(bool value)
	{
		GlobalData.debugMode = value;
	}
}