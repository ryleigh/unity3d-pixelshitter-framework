using UnityEngine;
using System.Collections;

public enum SceneType { MainMenu, Game };

public class SceneManager : MonoBehaviour
{
	private static SceneManager _instance;
	public static SceneManager shared { get{ return _instance; } }
	
	private Hashtable _sceneList = new Hashtable();
	private Scene _currentScene;
	private SceneType _currentSceneType;
	
	public Scene GetCurrentScene() { return _currentScene; }
	public SceneType GetCurrentSceneType() { return _currentSceneType; }

	bool _started = false;
	
	void Awake()
	{
		_instance = this;
		
		_sceneList.Add(SceneType.MainMenu, gameObject.AddComponent<MainMenuScene>());
		_sceneList.Add(SceneType.Game, gameObject.AddComponent<GameScene>());
	}
	
	void Start()
	{

	}

	public void BeginStartingScene()
	{
		StartCoroutine(DelayStartingScene());
	}

	IEnumerator DelayStartingScene()
	{
		yield return new WaitForSeconds(0.1f);
		SetScene(SceneType.Game);
	}
	
	void Update()
	{
		if(_currentScene)
			_currentScene.UpdateScene();
	}
	
	public void SetScene(SceneType sceneType)
	{
		if((Scene)_sceneList[sceneType])
		{
			if(_currentScene)
				_currentScene.Deactivate();	
			
			_currentScene = (Scene)_sceneList[sceneType];	
			_currentSceneType = sceneType;
			_currentScene.Activate();
		}
		else
		{
			Debug.LogError("No scene of type " + sceneType + " exists!");
		}
	}
}
