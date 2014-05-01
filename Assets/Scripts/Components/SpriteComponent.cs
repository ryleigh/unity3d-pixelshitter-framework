using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct PixelData
{
	public PixelPoint position;
	public Color32 color;
	
	public PixelData(PixelPoint position, Color32 color)
	{
		this.position = position;
		this.color = color;
	}
}

public struct FrameData
{
	public List<PixelData> pixels;
	public float animTime;
	
	public FrameData(List<PixelData> pixels, float animTime = 0.0f)
	{
		this.pixels = pixels;
		this.animTime = animTime;
	}
	
	public List<PixelData> GetPixelsFlippedX(int width)
	{
		List<PixelData> flippedPixels = new List<PixelData>();
		
		foreach(PixelData pixelData in pixels)
			flippedPixels.Add(new PixelData(new PixelPoint(width - 1 - pixelData.position.x, pixelData.position.y), pixelData.color));
		
		return flippedPixels;
	}
	
	public List<PixelData> GetPixelsFlippedY(int height)
	{
		List<PixelData> flippedPixels = new List<PixelData>();
		
		foreach(PixelData pixelData in pixels)
			flippedPixels.Add(new PixelData(new PixelPoint(pixelData.position.x, height - 1 - pixelData.position.y), pixelData.color));
		
		return flippedPixels;
	}
	
	public List<PixelData> GetPixelsFlippedXAndY(int width, int height)
	{
		List<PixelData> flippedPixels = new List<PixelData>();
		
		foreach(PixelData pixelData in pixels)
			flippedPixels.Add(new PixelData(new PixelPoint(width - 1 - pixelData.position.x, height - 1 - pixelData.position.y), pixelData.color));
		
		return flippedPixels;
	}
}

public class AnimationData
{
	public string name;
	public List<FrameData> frames;
	public PixelPoint animSize;
	public PixelRect hitbox;
	public LoopMode loopMode;
	
	public AnimationData(string name, List<FrameData> frames, PixelPoint animSize, PixelRect hitbox, LoopMode loopMode)
	{
		this.name = name;
		this.frames = frames;
		this.animSize = animSize;
		this.hitbox = hitbox;
		this.loopMode = loopMode;
	}
}

public enum LoopMode { Loops, PlayOnce, PingPong, RandomFrame };
public delegate void AnimationCompleteDelegate();
public delegate void SetFrameDelegate();

public class SpriteComponent : EntityComponent
{
	static public implicit operator bool(SpriteComponent val) { return val != null; }
	
	public string spriteName;
	
	public AnimationData currentAnimation;
	public int currentFrameNumber;
	protected float _currentFrameTime;
	public FrameData currentFrameData;
	protected bool _pingPongForward;
	protected float _animationTimeScale = 1.0f; // affects all anim frame times - higher means faster anims
	public float AnimTimeScale { get { return _animationTimeScale; } set { _animationTimeScale = value; } }
	bool _overrideLoopMode = false;
	LoopMode _overriddenLoopMode;
	
	AnimationCompleteDelegate _animCompleteCallback;
	public void SetAnimCompleteCallback(AnimationCompleteDelegate callback) { _animCompleteCallback = callback; }
	bool _playOnceAnimFinished;
	
	Dictionary<int, SetFrameDelegate> _setFrameDelegates = new Dictionary<int, SetFrameDelegate>();
	public void AddSetFrameCallback(int frameNumber, SetFrameDelegate callback) { _setFrameDelegates.Add(frameNumber, callback); }
	public void ClearSetFrameCallbacks() { _setFrameDelegates.Clear(); }
	
	public bool flipX;
	public bool flipY;
	
	Dictionary<Color32, Color32> _overriddenColors = new Dictionary<Color32, Color32>();
	public void AddOverriddenColor(Color32 a, Color32 b) { _overriddenColors.Add(a, b); }
	public void RemoveOverriddenColor(Color32 a) { if(_overriddenColors.ContainsKey(a)) _overriddenColors.Remove(a); }
	public void ClearOverriddenColors() { _overriddenColors.Clear(); }
	
	bool _preserveAlphaWhenOverridingColor = false;
	public void SetPreserveAlpha(bool preserveAlpha) { _preserveAlphaWhenOverridingColor = preserveAlpha; }
	
	public SpriteComponent(string spriteName, Entity entity)
	{
		this.spriteName = spriteName;
		_entity = entity;
	}
	
	public override void UpdateComponent(float deltaTime)
	{
		if(currentAnimation != null)
			HandleAnimation(deltaTime);
	}
	
	void HandleAnimation(float deltaTime)
	{
		LoopMode loopMode;
		if(_overrideLoopMode)
			loopMode = _overriddenLoopMode;
		else
			loopMode = currentAnimation.loopMode;
		
		int numFrames = currentAnimation.frames.Count;
		
		_currentFrameTime += (deltaTime * _animationTimeScale);
		if(_currentFrameTime > currentAnimation.frames[currentFrameNumber].animTime)
		{
			// ----------------------------------------------------------------------
			// LOOP
			// ----------------------------------------------------------------------
			if(loopMode == LoopMode.Loops && numFrames > 1)
			{
				if(currentFrameNumber == numFrames - 1)
					SetAnimationFrame(0);
				else
					SetAnimationFrame(currentFrameNumber + 1);
			}
			// ----------------------------------------------------------------------
			// PLAY ONCE
			// ----------------------------------------------------------------------
			else if(loopMode == LoopMode.PlayOnce && !_playOnceAnimFinished)
			{
				if(currentFrameNumber < numFrames - 1)
				{
					SetAnimationFrame(currentFrameNumber + 1);
				}
				else if(currentFrameNumber == numFrames - 1)
				{
					_playOnceAnimFinished = true;
					if(_animCompleteCallback != null)
					{
						_animCompleteCallback();
						_animCompleteCallback = null;
					}
				}
			}
			// ----------------------------------------------------------------------
			// PING PONG
			// ----------------------------------------------------------------------
			else if(loopMode == LoopMode.PingPong && numFrames > 1)
			{
				// ----------------------------------------------------------------------
				// FORWARD
				// ----------------------------------------------------------------------
				if(_pingPongForward)
				{
					if(currentFrameNumber < numFrames - 1)
					{
						SetAnimationFrame(currentFrameNumber + 1);
					}
					else
					{
						SetAnimationFrame(currentFrameNumber - 1);
						_pingPongForward = false;
					}
				}
				// ----------------------------------------------------------------------
				// REVERSE
				// ----------------------------------------------------------------------
				else
				{
					if(currentFrameNumber > 0)
					{
						SetAnimationFrame(currentFrameNumber - 1);
					}
					else
					{
						SetAnimationFrame(currentFrameNumber + 1);
						_pingPongForward = true;
					}
				}
			}
			// ----------------------------------------------------------------------
			// RANDOM FRAME
			// ----------------------------------------------------------------------
			else if(loopMode == LoopMode.RandomFrame && numFrames > 1)
			{
				int newFrame = Random.Range(0, currentAnimation.frames.Count);
				while(newFrame == currentFrameNumber)
					newFrame = Random.Range(0, currentAnimation.frames.Count);
				
				SetAnimationFrame(newFrame);
			}
			
			_currentFrameTime = 0.0f;
		}
	}
	
	void SetAnimationFrame(int frameNumber)
	{
		foreach(KeyValuePair<int, SetFrameDelegate> pair in _setFrameDelegates)
		{
			if(pair.Key == frameNumber && pair.Value != null)
				pair.Value();
		}
		
		currentFrameNumber = frameNumber;
		currentFrameData = currentAnimation.frames[currentFrameNumber];
	}
	
	public virtual void PlayAnimation(string animName)
	{
		if(currentAnimation != null && currentAnimation.name == animName)
			return;
		
		if(!SpriteManager.animations.ContainsKey(spriteName))
		{
			Debug.LogError("No entity type called <" + spriteName + ">!");
			return;
		}
		
		bool success = false;
		foreach(AnimationData animData in SpriteManager.animations[spriteName])
		{
			if(animData.name == animName)
			{
				currentAnimation = animData;
				_entity.hitbox = currentAnimation.hitbox;
				SetAnimationFrame(0);
				_currentFrameTime = 0.0f;
				_overrideLoopMode = false;
				success = true;
				_playOnceAnimFinished = false;
				break;
			}
		}
		
		if(!success)
			Debug.LogError("No animation called <" + animName + "> for <" + spriteName + ">!");
	}
	
	public virtual void PlayAnimation(string animName, LoopMode overriddenLoopMode)
	{
		PlayAnimation(animName);
		_overrideLoopMode = true;
		_overriddenLoopMode = overriddenLoopMode;
	}
	
	public virtual void Draw()
	{
		if(currentAnimation != null)
		{
			// check if any of the entity is in the view
			if(!IsVisibleInScreenBounds() && !PixelScreen.shared.DrawInGUISpace)
				return;
			
			List<PixelData> pixels = currentFrameData.pixels;
			if(flipX && flipY)
				pixels = currentFrameData.GetPixelsFlippedXAndY(currentAnimation.animSize.x, currentAnimation.animSize.y);
			else if(flipX)
				pixels = currentFrameData.GetPixelsFlippedX(currentAnimation.animSize.x);
			else if(flipY)
				pixels = currentFrameData.GetPixelsFlippedY(currentAnimation.animSize.y);
			
			foreach(PixelData pixel in pixels)
			{
				Color32 color = pixel.color;
				
				foreach(KeyValuePair<Color32, Color32> pair in _overriddenColors)
				{
					if(color.r == pair.Key.r &&
					   color.g == pair.Key.g &&
					   color.b == pair.Key.b)
					{
						if(_preserveAlphaWhenOverridingColor)
							color = new Color32(pair.Value.r, pair.Value.g, pair.Value.b, ((Color32)color).a);
						else
							color = pair.Value;
						
						break;
					}
				}
				
				if(color.a == 255)
					PixelScreen.shared.SetPixel(_entity.pixelX + pixel.position.x, _entity.pixelY + pixel.position.y, color);
				else
					PixelScreen.shared.AddPixel(_entity.pixelX + pixel.position.x, _entity.pixelY + pixel.position.y, color);
			}
		}
	}
	
	public bool IsVisibleInScreenBounds()
	{
		if(currentAnimation != null)
		{
			int width = currentAnimation.animSize.x;
			int height = currentAnimation.animSize.y;
			
			if(_entity.pixelX < PixelScreen.shared.cameraPos.x - width)
				return false;
			if(_entity.pixelX > PixelScreen.shared.GetScreenWidth() + PixelScreen.shared.cameraPos.x)
				return false;
			if(_entity.pixelY < PixelScreen.shared.cameraPos.y - height)
				return false;
			if(_entity.pixelY > PixelScreen.shared.GetScreenHeight() + PixelScreen.shared.cameraPos.y)
				return false;
			
			return true;
		}
		
		return false;
	}
	
	public virtual void DrawPixels(List<PixelData> pixelDataList)
	{
		DrawPixels(pixelDataList, _entity.pixelPos.x, _entity.pixelPos.y);
	}
	
	public virtual void DrawPixels(List<PixelData> pixelDataList, int x, int y, int scale = 1)
	{
		foreach(PixelData pixel in pixelDataList)
		{
			Color32 color = pixel.color;
			
			foreach(KeyValuePair<Color32, Color32> pair in _overriddenColors)
			{
				if(color.r == pair.Key.r &&
				   color.g == pair.Key.g &&
				   color.b == pair.Key.b)
				{
					if(_preserveAlphaWhenOverridingColor)
						color = new Color32(pair.Value.r, pair.Value.g, pair.Value.b, ((Color32)color).a);
					else
						color = pair.Value;
					
					break;
				}
			}
			
			for(int xOffset = 0; xOffset < scale; xOffset++)
			{
				for(int yOffset = 0; yOffset < scale; yOffset++)
				{
					int xPos = x + (pixel.position.x * scale) + xOffset;
					int yPos = y + (pixel.position.y * scale) + yOffset;
					
					if(color.a == 255)
						PixelScreen.shared.SetPixel(xPos, yPos, color);
					else
						PixelScreen.shared.AddPixel(xPos, yPos, color);
				}
			}
		}
	}
	
	public List<PixelData> GetPixelDataList()
	{
		if(currentAnimation == null)
			return null;
		
		if(flipX && flipY)
		{
			return currentAnimation.frames[currentFrameNumber].GetPixelsFlippedXAndY(currentAnimation.animSize.x, currentAnimation.animSize.y);
		}
		else if(flipX)
		{
			return currentAnimation.frames[currentFrameNumber].GetPixelsFlippedX(currentAnimation.animSize.x);
		}
		else if(flipY)
		{
			return currentAnimation.frames[currentFrameNumber].GetPixelsFlippedY(currentAnimation.animSize.y);
		}
		else
		{
			List<PixelData> frames = new List<PixelData>();
			frames.AddRange(currentAnimation.frames[currentFrameNumber].pixels);
			return frames;
		}
	}
	
	public List<PixelData> GetPixelDataList(string type, string animName, int frameNum = 0, bool flippedX = false, bool flippedY = false)
	{
		if(!SpriteManager.animations.ContainsKey(type))
		{
			Debug.LogError("No entity type called <" + type + ">!");
			return null;
		}
		
		AnimationData anim = null;
		foreach(AnimationData animData in SpriteManager.animations[type])
		{
			if(animData.name == animName)
			{
				anim = animData; 
				break;
			}
		}
		
		if(anim == null)
			return null;
		
		if(frameNum > anim.frames.Count - 1)
		{
			return null;
		}
		else
		{
			if(flippedX && flippedY)
			{
				return anim.frames[frameNum].GetPixelsFlippedXAndY(currentAnimation.animSize.x, currentAnimation.animSize.y);
			}
			else if(flippedX)
			{
				return anim.frames[frameNum].GetPixelsFlippedX(currentAnimation.animSize.x);
			}
			else if(flippedY)
			{
				return anim.frames[frameNum].GetPixelsFlippedY(currentAnimation.animSize.y);
			}
			else
			{
				List<PixelData> frames = new List<PixelData>();
				frames.AddRange(anim.frames[frameNum].pixels);
				return frames;
			}
		}
	}
	
	public PixelPoint GetAnimSize(string type, string animName)
	{
		if(!SpriteManager.animations.ContainsKey(type))
		{
			Debug.LogError("No entity type called <" + type + ">!");
			return new PixelPoint(0, 0);
		}
		
		AnimationData anim = null;
		foreach(AnimationData animData in SpriteManager.animations[type])
		{
			if(animData.name == animName)
			{
				anim = animData; 
				break;
			}
		}
		
		if(anim == null)
			return new PixelPoint(0, 0);
		
		return anim.animSize;
	}
	
	public PixelRect GetAnimHitbox(string type, string animName)
	{
		if(!SpriteManager.animations.ContainsKey(type))
		{
			Debug.LogError("No entity type called <" + type + ">!");
			return new PixelRect(0, 0, 0, 0);
		}
		
		AnimationData anim = null;
		foreach(AnimationData animData in SpriteManager.animations[type])
		{
			if(animData.name == animName)
			{
				anim = animData; 
				break;
			}
		}
		
		if(anim == null)
			return new PixelRect(0, 0, 0, 0);
		
		return anim.hitbox;
	}
	
	public AnimationData GetAnimationData(string type, string animName)
	{
		AnimationData anim = null;
		foreach(AnimationData animData in SpriteManager.animations[type])
		{
			if(animData.name == animName)
			{
				anim = animData; 
				break;
			}
		}
		
		return anim;
	}
}































