using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Box : Entity
{
	public Box(int x, int y, Scene scene)
	{
		pixelPos = new PixelPoint(x, y);
		this.scene = scene;
		SetSprite("box");
		sprite.PlayAnimation("idle");
		tags.Add("box");

		layer = Globals.LAYER_BOX;
	}
	
	public override void UpdateEntity(float deltaTime)
	{
		base.UpdateEntity(deltaTime);
		
	}

	// returns true if was successfully pushed
	public bool Move(PixelPoint movement)
	{
		PixelPoint newPos = pixelPos + movement;
		if(IsInBounds(newPos) &&
		   !Collide("box", newPos))
		{
			pixelPos = newPos;
			return true;
		}

		return false;
	}

	public override void Draw()
	{
		// draw the default sprite
		base.Draw();

		for(int x = 1; x <= 3; x++)
		{
			for(int y = 1; y <= 3; y++)
			{
				PixelScreen.shared.SetPixel(pixelPos + new PixelPoint(x, y), GetRandomColor());
			}
		}

		List<Entity> otherBoxes = scene.GetEntities(this, "box");
		foreach(Entity entity in otherBoxes)
		{
			PixelScreen.shared.DrawLine(new PixelPoint(pixelPos.x + 2, pixelPos.y + 2),
			                            new PixelPoint(entity.pixelPos.x + 2, entity.pixelPos.y + 2),
			                            GetRandomColor());
		}
	}

	Color GetRandomColor()
	{
		float COLOR_RANGE_MIN = 0.3f;
		float COLOR_RANGE_MAX = 1.0f;
		
		Color color = new Color(Random.Range(COLOR_RANGE_MIN, COLOR_RANGE_MAX),
		                        Random.Range(COLOR_RANGE_MIN, COLOR_RANGE_MAX),
		                        Random.Range(COLOR_RANGE_MIN, COLOR_RANGE_MAX));

		return color;
	}
}

















