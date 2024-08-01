using UnityEngine;
using UnityEngine.UI;

public class EditorRightPanelBlocks : EditorView
{
	[SerializeField]
	private Toggle seeOverToggle;

	[SerializeField]
	private GameObject selectedItemSection;

	private bool lockListeners = false;

	protected override void Start()
	{
		base.Start();

		selectedItemSection.SetActive(false);

		seeOverToggle.onValueChanged.AddListener(SeeOverToggle_OnValueChanged);

		editor.OnSelectionChanged += Editor_OnSelectionChanged;
	}

	void OnDestroy()
	{
		seeOverToggle.onValueChanged.AddListener(SeeOverToggle_OnValueChanged);

		if (editor != null)
			editor.OnSelectionChanged -= Editor_OnSelectionChanged;
	}

	private void Editor_OnSelectionChanged(DWPObject previuosItem, DWPObject newItem)
	{
		if (lockListeners)
			return;

		var block = newItem as Block;

		selectedItemSection.SetActive(block != null);

		if (block != null)
		{
			lockListeners = true;
			seeOverToggle.isOn = block.CanSeeOver;
			lockListeners = false;
		}
	}

	private void DisplayBlocksToggle_OnValueChanged(bool value)
	{
		if (lockListeners)
			return;
	}

	private void SeeOverToggle_OnValueChanged(bool value)
	{
		if (lockListeners)
			return;

		var block = editor.SelectedItem as Block;

		if (block != null)
			block.CanSeeOver = value;
	}
}