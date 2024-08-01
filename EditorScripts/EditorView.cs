using UnityEngine;

public class EditorView : MonoBehaviour
{
	protected Editor editor;

	protected LevelContainer levelContainer;

	protected virtual void Start()
	{
		editor = GetComponentInParent<Editor>();

		levelContainer = editor.LevelContainer;
	}
}