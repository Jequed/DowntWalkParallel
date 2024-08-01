using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField]
	private LevelContainer editorLevelContainer;

	[SerializeField]
	private Camera globalCamera;

	private LevelContainer playModeLevelContainer;

	public LevelContainer PlayModeLevelContainer
	{
		get
		{
			return playModeLevelContainer;
		}
		set
		{
			playModeLevelContainer = value;
		}
	}

	public LevelContainer EditorLevelContainer
	{
		get
		{
			return editorLevelContainer;
		}
		set
		{
			editorLevelContainer = value;
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
			GlobalData.playMode = value;

			if (value)
			{
                StartLevel();

                EditorLevelContainer.gameObject.SetActive(false);
            }
			else
			{
                EndLevel();

				EditorLevelContainer.gameObject.SetActive(true);
			}

            GlobalData.random = new System.Random(12345);
		}
	}

	private void Controller_OnPhobiaMaxed(Phobia phobia)
	{
		#if UNITY_EDITOR
			Debug.Log(phobia.Message);
		#endif

		Reset();
	}

    private void StartLevel()
    {
        PlayModeLevelContainer = Instantiate(EditorLevelContainer);
        PlayModeLevelContainer.gameObject.SetActive(true);
        PlayModeLevelContainer.Controller.OnPhobiaMaxed += Controller_OnPhobiaMaxed;

        if (!GlobalData.debugMode)
        {
            globalCamera.cullingMask = GlobalData.phoneCameraMask;
            playModeLevelContainer.Camera.gameObject.SetActive(true);
            playModeLevelContainer.PhoneScreen.gameObject.SetActive(true);
        }
    }
    private void EndLevel()
    {
        globalCamera.cullingMask = GlobalData.defaultCameraMask;
        playModeLevelContainer.Camera.gameObject.SetActive(false);
        playModeLevelContainer.PhoneScreen.gameObject.SetActive(false);

        PlayModeLevelContainer.Controller.OnPhobiaMaxed -= Controller_OnPhobiaMaxed;
        Destroy(PlayModeLevelContainer.gameObject);

        PlayModeLevelContainer = null;
    }

	private void Reset()
	{
        EndLevel();
        StartLevel();
	}
}