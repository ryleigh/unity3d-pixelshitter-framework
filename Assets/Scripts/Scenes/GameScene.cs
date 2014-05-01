using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameScene : Scene
{
	Player _player;

	public override void Awake()
	{
		base.Awake();
	}

	public override void Activate()
	{
		base.Activate();

		Debug.Log("GameScene::Activate");

		_player = new Player(1, 1, this);
		AddEntity(_player);

		AddEntity(new Box(22, 7, this));
		AddEntity(new Box(10, 12, this));
		AddEntity(new Box(6, 25, this));

		TextDisplay textDisplay = new TextDisplay("32x32", this, 0, 0, "text");
		textDisplay.SetColor(new Color(0.4f, 0.4f, 0.45f));
		textDisplay.pixelX = PixelScreen.shared.pixelWidth - textDisplay.hitbox.width - 1;
		textDisplay.pixelY = PixelScreen.shared.pixelHeight - textDisplay.hitbox.height - 1;
		AddEntity(textDisplay);
	}

	public override void Deactivate()
	{
		base.Deactivate();

		// entities are automatically deleted, but you may need to clean up other stuff here
	}

	public override void UpdateScene()
	{
		PixelScreen.shared.Clear(Color.gray);

		base.UpdateScene();
		
//		if(Input.GetKeyDown(KeyCode.Alpha1))
//			SceneManager.shared.SetScene(SceneType.MainMenu);
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





















