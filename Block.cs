using UnityEngine;

public class Block : SolidObject
{
	[SerializeField]
	private SpriteRenderer editorSpriteRenderer;

    [SerializeField]
    private MeshRenderer gameMeshRenderer;

	public bool CanSeeOver
	{
		get
		{
			return canSeeOver;
		}
		set
		{
			if (value != canSeeOver)
			{
				canSeeOver = value;
				UpdateColor();
			}
		}
	}

	protected override void Start()
	{
		base.Start();

		UpdateColor();
	}

	private void UpdateColor()
	{
        if (CanSeeOver)
        {
            editorSpriteRenderer.color = Color.yellow;
            gameMeshRenderer.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else
        {
            editorSpriteRenderer.color = Color.red;
            gameMeshRenderer.transform.localScale = Vector3.one;
        }
	}
}