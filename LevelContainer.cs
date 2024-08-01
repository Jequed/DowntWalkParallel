using UnityEngine;

public class LevelContainer : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer background;

	[SerializeField]
	private SpriteRenderer foreground;

	[SerializeField]
	private GameObject backgroundsContainer;

	[SerializeField]
	private GameObject blocksContainer;

	[SerializeField]
	private GameObject peopleContainer;

	[SerializeField]
	private GameObject miscContainer;

	[SerializeField]
	private GameObject phone;

	[SerializeField]
	private SpriteRenderer phoneScreen;

	[SerializeField]
	private new Camera camera;

	public enum LayerType
	{
		level,
		blocks,
		people,
		misc
	}

	public SpriteRenderer Background
	{
		get
		{
			return background;
		}
	}

	public SpriteRenderer Foreground
	{
		get
		{
			return foreground;
		}
	}

	public GameObject BackgroundsContainer
	{
		get
		{
			return backgroundsContainer;
		}
	}

	public GameObject BlocksContainer
	{
		get
		{
			return blocksContainer;
		}
	}
	public GameObject PeopleContainer
	{
		get
		{
			return peopleContainer;
		}
	}
	public GameObject MiscContainer
	{
		get
		{
			return miscContainer;
		}
	}

	public GameObject Phone
	{
		get
		{
			return phone;
		}
	}

	public SpriteRenderer PhoneScreen
	{
		get
		{
			return phoneScreen;
		}
	}

	public Camera Camera
	{
		get
		{
			return camera;
		}
	}

	public LevelController Controller
	{
		get
		{
			return GetComponent<LevelController>();
		}
	}
}