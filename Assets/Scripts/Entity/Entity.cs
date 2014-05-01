using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void ScheduledDelegate();
public struct ScheduledDelegateData
{
	public ScheduledDelegate callback;
	public float delay;
	
	public ScheduledDelegateData(ScheduledDelegate callback, float delay)
	{
		this.callback = callback;
		this.delay = delay;
	}
}

public class Entity
{
	static public implicit operator bool(Entity val) { return val != null; }

	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	// DRAW LAYERS
	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	public int layer;
	public int depth;

	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	// FLAGS
	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	public bool active = true; // whether this should be updated
	public bool visible = true; // whether we should be drawn
	public bool collideable = true; // whether it is checked when other objects test collision

	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	// UPDATE ORDER
	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	// lower number means it gets updated earlier
	public int updateOrder;

	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	// POSITIONING
	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	public Vector2 pos;
	public PixelPoint pixelPos { get { return new PixelPoint(pos); } set { pos = new Vector2(value.x, value.y); } }

	public float posX
	{
		get { return pos.x; }
		set { pos = new Vector2(value, pos.y); }
	}
	
	public float posY
	{
		get { return pos.y; }
		set { pos = new Vector2(pos.x, value); }
	}
	
	public int pixelX
	{
		get { return Mathf.RoundToInt(pos.x); }
		set { pos = new Vector2(value, pos.y); }
	}
	
	public int pixelY
	{
		get { return Mathf.RoundToInt(pos.y); }
		set { pos = new Vector2(pos.x, value); }
	}

	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	// VELOCITY
	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------
	public Vector2 velocity;
	public float velX { get { return velocity.x; } set { velocity = new Vector2(value, velocity.y); } }
	public float velY { get { return velocity.y; } set { velocity = new Vector2(velocity.x, value); } }

	// the center-x position on the pixel grid
	public PixelPoint centerPixelPos
	{
		get
		{
			if(sprite != null && sprite.currentAnimation != null)
				return new PixelPoint(pixelX + Mathf.RoundToInt((sprite.currentAnimation.animSize.x - 1) / 2),
				                      pixelY + Mathf.RoundToInt((sprite.currentAnimation.animSize.y - 1) / 2));
			else
				return pixelPos;
		}
	}

	// the center-y position on the pixel grid
	public Vector2 centerPos
	{ 
		get
		{ 
			if(sprite != null && sprite.currentAnimation != null)
				return new Vector2(posX + (float)sprite.currentAnimation.animSize.x * 0.5f,
				                   posY + (float)sprite.currentAnimation.animSize.y * 0.5f);
			else
				return pos;
		}
	}

	public SpriteComponent sprite;
	public List<string> tags = new List<string>();

	public PixelRect hitbox;
	public PixelRect offsetHitbox { get { return hitbox + pixelPos; } }
	public int offsetHitboxXMin { get { return offsetHitbox.xMin; } set { pixelX += (value - offsetHitbox.xMin); } }
	public int offsetHitboxXMax { get { return offsetHitbox.xMax; } set { pixelX += (value - offsetHitbox.xMax); } }
	public int offsetHitboxYMin { get { return offsetHitbox.yMin; } set { pixelY += (value - offsetHitbox.yMin); } }
	public int offsetHitboxYMax { get { return offsetHitbox.yMax; } set { pixelY += (value - offsetHitbox.yMax); } }

	public PixelRect GetOffsetHitbox(PixelPoint position) { return hitbox + position; }
	public PixelRect GetOffsetHitbox(int x, int y) { return hitbox + new PixelPoint(x, y); }
	
	public Scene scene;
	
	List<ScheduledDelegateData> _scheduledCallbacks = new List<ScheduledDelegateData>();
	public void AddScheduledCallback(float delay, ScheduledDelegate callback) { _scheduledCallbacks.Add(new ScheduledDelegateData(callback, delay)); }

	public void SetSprite(string spriteName)
	{
		if(sprite)
			sprite.spriteName = spriteName;
		else
			sprite = new SpriteComponent(spriteName, this);
	}

	public virtual void UpdateEntity(float deltaTime)
	{
		if(_scheduledCallbacks.Count > 0)
			HandleScheduledCallbacks(deltaTime);

		if(sprite)
			sprite.UpdateComponent(deltaTime);
	}
	
	void HandleScheduledCallbacks(float deltaTime)
	{
		for(int i = _scheduledCallbacks.Count - 1; i >= 0; i--)
		{
			ScheduledDelegateData scheduledDelegateData = _scheduledCallbacks[i];
			ScheduledDelegate callback = scheduledDelegateData.callback;
			float newTime = scheduledDelegateData.delay - deltaTime;
			if(newTime <= 0.0f)
				callback();
			else
				_scheduledCallbacks.Add(new ScheduledDelegateData(callback, newTime));
			
			_scheduledCallbacks.RemoveAt(i);
		}
	}
	
	public virtual void Draw()
	{
		if(sprite)
			sprite.Draw();

		#if UNITY_EDITOR
		DrawHitboxes();
		#endif
	}
	
	public void DrawHitboxes()
	{
		if(Globals.shared.DEBUG_HITBOXES)
		{
			PixelRect hitbox = offsetHitbox;
			
			for(int y = hitbox.bottom; y < hitbox.bottom + hitbox.height; y++)
			{
				for(int x = hitbox.left; x < hitbox.left + hitbox.width; x++)
				{
					PixelScreen.shared.AddPixel(x, y, Globals.shared.DebugHitboxColor);
				}
			}
		}
	}
	
	protected Entity Collide(string type, int xPixel, int yPixel)
	{
		return scene.Collide(this, type, xPixel, yPixel);
	}

	protected Entity Collide(string type, PixelPoint point)
	{
		return scene.Collide(this, type, point.x, point.y);
	}
	
	protected Entity Collide(string type)
	{
		return scene.Collide(this, type, pixelX, pixelY);
	}

	protected Entity Collide(string type, PixelRect rect)
	{
		return scene.Collide(this, type, rect);
	}
	
	protected List<Entity> CollideWithAll(string type, int xPixel, int yPixel)
	{
		return scene.CollideWithAll(this, type, xPixel, yPixel);
	}

	protected List<Entity> CollideWithAll(string type, PixelPoint point)
	{
		return scene.CollideWithAll(this, type, point.x, point.y);
	}

	protected List<Entity> CollideWithAll(string type)
	{
		return scene.CollideWithAll(this, type, pixelX, pixelY);
	}
	
	protected bool IsInBounds(int xPixel, int yPixel)
	{
		return scene.IsInBounds(this, xPixel, yPixel);
	}

	protected bool IsInBounds(PixelPoint point)
	{
		return scene.IsInBounds(this, point.x, point.y);
	}

	// takes into account the camera position
	public bool IsInScreenBounds(int x, int y)
	{
		PixelRect rect = GetOffsetHitbox(x, y);
		if((rect.xMin < PixelScreen.shared.cameraPos.x) ||
		   (rect.xMax > PixelScreen.shared.cameraPos.x + PixelScreen.shared.GetScreenWidth() - 1) ||
		   (rect.yMin < PixelScreen.shared.cameraPos.y) ||
		   (rect.yMax > PixelScreen.shared.cameraPos.y + PixelScreen.shared.GetScreenHeight() - 1))
			return false;

		return true;
	}

	public bool IsInScreenBounds(PixelRect rect)
	{
		if((rect.xMin < PixelScreen.shared.cameraPos.x) ||
		   (rect.xMax > PixelScreen.shared.cameraPos.x + PixelScreen.shared.GetScreenWidth() - 1) ||
		   (rect.yMin < PixelScreen.shared.cameraPos.y) ||
		   (rect.yMax > PixelScreen.shared.cameraPos.y + PixelScreen.shared.GetScreenHeight() - 1))
			return false;
		
		return true;
	}

	public bool ClampToScreenBounds()
	{
		if(offsetHitbox.xMin < PixelScreen.shared.viewportXMin)
		{
			offsetHitboxXMin = PixelScreen.shared.viewportXMin;
			return true;
		}

		if(offsetHitbox.xMax > PixelScreen.shared.viewportXMax)
		{
			offsetHitboxXMax = PixelScreen.shared.viewportXMax;
			return true;
		}

		if(offsetHitbox.yMin < PixelScreen.shared.viewportYMin)
		{
			offsetHitboxYMin = PixelScreen.shared.viewportYMin;
			return true;
		}


		if(offsetHitbox.yMax > PixelScreen.shared.viewportYMax)
		{
			offsetHitboxYMax = PixelScreen.shared.viewportYMax;
			return true;
		}
		
		return false;
	}
	
	public virtual void Unpenetrate(Entity other)
	{
		if(!offsetHitbox.Overlaps(other.offsetHitbox))
			return;
		
		PixelRect myHitbox = offsetHitbox;
		PixelRect otherHitbox = other.offsetHitbox;
		
		int leftPenetration = myHitbox.xMax - otherHitbox.xMin + 1; // the amount we'd have to move left to unpenetrate
		int rightPenetration = otherHitbox.xMax - myHitbox.xMin + 1;
		int upPenetration = otherHitbox.yMax - myHitbox.yMin + 1;
		int downPenetration = myHitbox.yMax - otherHitbox.yMin + 1;

		Direction unpenetrateDirection = Direction.None;
		int currentUnpenetrateAmount = 0;

		int rand = Random.Range(0, 4);
		if(rand == 0) 		{ unpenetrateDirection = Direction.Up; 		currentUnpenetrateAmount = upPenetration; }
		else if(rand == 1) 	{ unpenetrateDirection = Direction.Down; 	currentUnpenetrateAmount = downPenetration; }
		else if(rand == 2) 	{ unpenetrateDirection = Direction.Left; 	currentUnpenetrateAmount = leftPenetration; }
		else if(rand == 3) 	{ unpenetrateDirection = Direction.Right;	currentUnpenetrateAmount = rightPenetration; }

		if(upPenetration < currentUnpenetrateAmount) 	{ unpenetrateDirection = Direction.Up; 		currentUnpenetrateAmount = upPenetration; }
		if(downPenetration < currentUnpenetrateAmount) 	{ unpenetrateDirection = Direction.Down;	currentUnpenetrateAmount = downPenetration; }
		if(leftPenetration < currentUnpenetrateAmount) 	{ unpenetrateDirection = Direction.Left; 	currentUnpenetrateAmount = leftPenetration; }
		if(rightPenetration < currentUnpenetrateAmount) { unpenetrateDirection = Direction.Right; 	currentUnpenetrateAmount = rightPenetration; }
		
		// *** also check if a direction would make them penetrate into something else
		// and if it would make them penetrate out of bounds
		
		switch(unpenetrateDirection)
		{
		case Direction.Up: 		pixelY += currentUnpenetrateAmount;		break;
		case Direction.Down: 	pixelY -= currentUnpenetrateAmount; 	break;
		case Direction.Left: 	pixelX -= currentUnpenetrateAmount; 	break;
		case Direction.Right:	pixelX += currentUnpenetrateAmount;		break;
		}
	}
}






























