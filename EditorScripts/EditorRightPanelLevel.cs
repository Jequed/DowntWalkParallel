using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorRightPanelLevel : EditorView
{
	[SerializeField]
	private SliderAndInputField screenSize;

	[SerializeField]
	private ToggleGroup levelTranslationType;

	private List<Toggle> toggles;

	protected override void Start()
	{
		base.Start();

		ScreenSize_OnValueChanged(screenSize.Value);

		screenSize.OnValueChanged += ScreenSize_OnValueChanged;

		toggles = new List<Toggle>(levelTranslationType.GetComponentsInChildren<Toggle>());
		foreach (var toggle in toggles)
			toggle.onValueChanged.AddListener(LevelTranslationType_OnValueChanged);
	}

	void OnDestroy()
	{
		screenSize.OnValueChanged -= ScreenSize_OnValueChanged;

		foreach (var toggle in toggles)
			toggle.onValueChanged.RemoveListener(LevelTranslationType_OnValueChanged);
	}

	private void ScreenSize_OnValueChanged(float value)
	{
		const float screenHeight = 7.18f; //718 pixels high

		editor.LevelContainer.Phone.transform.localScale = new Vector3(value / screenHeight, value / screenHeight, 1.0f);
	}

	private void LevelTranslationType_OnValueChanged(bool value)
	{
		for (int i = 0; i < toggles.Count; i++)
		{
			if (toggles[i].isOn)
			{
				editor.LevelTranslateMode = (Editor.LevelTranslationMode)i;
				break;
			}
		}
	}

	private Sprite LoadSprite(string path)
	{
		Sprite sprite = Resources.Load<Sprite>("Images/Backgrounds/" + path);

		return sprite;
	}
}