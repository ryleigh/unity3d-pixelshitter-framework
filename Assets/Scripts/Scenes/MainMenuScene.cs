using UnityEngine;
using System.Collections;

public class MainMenuScene : Scene
{
	public override void Awake()
	{
		base.Awake();
	}
	
	public override void Activate()
	{
		base.Activate();

		Debug.Log("MainMenuScene::Activate");
		PixelScreen.shared.Clear(Color.gray);
	}
	
	public override void Deactivate()
	{
		base.Deactivate();
	}
	
	public override void UpdateScene()
	{
		base.UpdateScene();
		
//		if(Input.GetKeyDown(KeyCode.Alpha2))
//			SceneManager.shared.SetScene(SceneType.Game);
	}
	
	public override void Pause()
	{
		base.Pause();
	}
	
	public override void Unpause()
	{
		base.Unpause();
	}
}





















