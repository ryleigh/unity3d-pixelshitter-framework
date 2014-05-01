using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : Entity
{
	float _keyRepeatTimer;
	const float KEY_REPEAT_DELAY = 0.08f;

	public Player(int x, int y, Scene scene)
	{
		pixelPos = new PixelPoint(x, y);
		this.scene = scene;
		SetSprite("player");
		sprite.PlayAnimation("idle");
		tags.Add("player");

		layer = Globals.LAYER_PLAYER;
	}

	public override void UpdateEntity(float deltaTime)
	{
		base.UpdateEntity(deltaTime);

		int newPixelX = pixelX;
		int newPixelY = pixelY;

		if(Input.GetKeyDown(KeyCode.LeftArrow) ||
		   Input.GetKey(KeyCode.LeftArrow) && _keyRepeatTimer <= 0.0f)
		{
			newPixelX = pixelX - 1;
		}
		else if(Input.GetKeyDown(KeyCode.RightArrow) ||
		        Input.GetKey(KeyCode.RightArrow) && _keyRepeatTimer <= 0.0f)
		{
			newPixelX = pixelX + 1;
		}

		if(Input.GetKeyDown(KeyCode.UpArrow) ||
		   Input.GetKey(KeyCode.UpArrow) && _keyRepeatTimer <= 0.0f)
		{
			newPixelY = pixelY + 1;
		}
		else if(Input.GetKeyDown(KeyCode.DownArrow) ||
		        Input.GetKey(KeyCode.DownArrow) && _keyRepeatTimer <= 0.0f)
		{
			newPixelY = pixelY - 1;
		}


		// X-MOVEMENT
		if(newPixelX != pixelX && IsInBounds(newPixelX, pixelY))
		{
			bool valid = true;

			List<Entity> entities = CollideWithAll("box", newPixelX, pixelY);
			foreach(Entity entity in entities)
			{
				if(entity != null)
				{
					Box box = (Box)entity;
					PixelPoint movement = new PixelPoint(newPixelX - pixelX, 0);
					if(!box.Move(movement))
					{
						valid = false;
						break;
					}
				}
			}

			if(valid)
				pixelX = newPixelX;

			_keyRepeatTimer = KEY_REPEAT_DELAY;
		}

		// Y-MOVEMENT
		if(newPixelY != pixelY && IsInBounds(pixelX, newPixelY))
		{
			bool valid = true;
			
			List<Entity> entities = CollideWithAll("box", pixelX, newPixelY);
			foreach(Entity entity in entities)
			{
				if(entity != null)
				{
					Box box = (Box)entity;
					PixelPoint movement = new PixelPoint(0, newPixelY - pixelY);
					if(!box.Move(movement))
					{
						valid = false;
						break;
					}
				}
			}
			
			if(valid)
				pixelY = newPixelY;
			
			_keyRepeatTimer = KEY_REPEAT_DELAY;
		}

		if(_keyRepeatTimer > 0.0f)
			_keyRepeatTimer -= deltaTime;
	}
}
