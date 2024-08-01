using UnityEngine;

public class EditorRightPanel : MonoBehaviour
{
	[SerializeField]
	private GameObject levelPanel;

	[SerializeField]
	private GameObject blockPanel;

	[SerializeField]
	private GameObject peoplePanel;

	[SerializeField]
	private GameObject miscPanel;

	public LevelContainer.LayerType LayerMode
	{
		set
		{
			levelPanel.SetActive(value == LevelContainer.LayerType.level);
			blockPanel.SetActive(value == LevelContainer.LayerType.blocks);
			peoplePanel.SetActive(value == LevelContainer.LayerType.people);
			miscPanel.SetActive(value == LevelContainer.LayerType.misc);
		}
	}
}