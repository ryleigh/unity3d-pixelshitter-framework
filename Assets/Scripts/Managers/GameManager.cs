using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance;
	public static GameManager shared { get{ return _instance; } }

	bool _started = false;
	GUIText _debugText;

	void Awake()
	{
		_instance = this;

		// i usually just use this GUIText debug if a webplayer is having an unknown issue
		_debugText = gameObject.AddComponent<GUIText>();
		_debugText.transform.position = new Vector3(0, 1, 0);
		_debugText.anchor = TextAnchor.UpperLeft;
		_debugText.alignment = TextAlignment.Left;
		_debugText.text = "";
	}
	
	void Update()
	{
		if(!_started)
		{
			// try to initialize the pixel screen
			// sometimes Screen.width/Screen.height seems to return 0 for the first frame in webplayer
			// - when this happens the screen will be unable to initialize
			if(PixelScreen.shared.Init())
			{
				SpriteManager.ParseAnimationStrings();
				SceneManager.shared.BeginStartingScene();
				_started = true;
			}
		}
	}

	public void AddDebugText(string text)
	{
		if(Globals.shared.DEBUG_MESSAGES)
			_debugText.text += text + "\n";
	}

	public void ClearDebugText()
	{
		if(Globals.shared.DEBUG_MESSAGES)
			_debugText.text = "";
	}
}
