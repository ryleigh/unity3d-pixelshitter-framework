using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public delegate void FadeDelegate();

public class Scene : MonoBehaviour
{
	protected List<Entity> _entities = new List<Entity>();
	
	protected List<Entity> _entitesToAdd = new List<Entity>();
	protected List<Entity> _entitesToRemove = new List<Entity>();
	
	protected bool _switchingScene = false;
	SceneType _switchingSceneType;

	// game freezes for a frame or more
	protected int _frameSkips = 0;
	public int FrameSkips { get { return _frameSkips; } set { _frameSkips = value; } }
	
	protected bool _fadingOut = false;
	public void FadeOut(float time) { _fadingOut = true; _fadeOutTimer = 0.0f; _fadeOutTime = time; }
	float _fadeOutTimer;
	float _fadeOutTime;
	FadeDelegate _fadeOutCallback = null;
	public void SetFadeOutCallback(FadeDelegate callback) { _fadeOutCallback = callback; }
	
	protected bool _fadingIn = false;
	public void FadeIn(float time) { _fadingIn = true; _fadeInTimer = 0.0f; _fadeInTime = time; }
	float _fadeInTimer;
	float _fadeInTime;
	FadeDelegate _fadeInCallback = null;
	public void SetFadeInCallback(FadeDelegate callback) { _fadeInCallback = callback; }
	
	public virtual void Awake()
	{
		
	}
	
	public virtual void Activate()
	{
		
	}
	
	public virtual void Deactivate()
	{
		_entities.Clear();
		_entitesToAdd.Clear();
		_entitesToRemove.Clear();
		_frameSkips = 0;
	}
	
	public virtual void UpdateScene()
	{
		float deltaTime = Time.deltaTime;

		// HANDLE THE ADDING AND REMOVING OF ENTITIES
		foreach(Entity e in _entitesToAdd)
			_entities.Add(e);
		_entitesToAdd.Clear();
		
		foreach(Entity e in _entitesToRemove)
			_entities.Remove(e);
		_entitesToRemove.Clear();

		// FREEZE FOR A FEW FRAMES, IF WE SHOULD
		if(_frameSkips > 0)
		{
			_frameSkips--;
		}
		else
		{
			// SORT ENTITIES BASED ON UPDATE ORDER
			_entities.Sort((a, b) => a.updateOrder.CompareTo(b.updateOrder));

			foreach(Entity entity in _entities)
			{
				if(entity.active)
					entity.UpdateEntity(deltaTime);
			}
		}
		
		if(_switchingScene)
		{
			SceneManager.shared.SetScene(_switchingSceneType);
			_switchingScene = false;
		}
		else
		{
			// SORT ENTITIES BASED ON LAYER & DEPTH SO THEY ARE DRAWN IN THE CORRECT ORDER
			_entities.Sort(delegate(Entity a, Entity b) {
				if(a.layer > b.layer) 		{ return -1; }
				else if(b.layer > a.layer)	{ return 1; }
				else 						{ return ((a.depth > b.depth) ? -1 : 1); }
			});

			foreach(Entity entity in _entities)
			{
				if(entity.visible)
					entity.Draw();
			}
		}
		
		if(_fadingIn)
		{
			float opacity = Utils.Map(_fadeInTimer, 0.0f, _fadeInTime, 1.0f, 0.0f, true, EASING_TYPE.SINE_EASE_OUT);
			PixelScreen.shared.AddPixels(new Color(0, 0, 0, opacity));
			
			_fadeInTimer += Time.deltaTime;
			if(_fadeInTimer > _fadeInTime)
			{
				_fadingIn = false;
				if(_fadeInCallback != null)
					_fadeInCallback();
			}


		}
		else if(_fadingOut)
		{
			_fadeOutTimer += Time.deltaTime;
			float opacity = Utils.Map(_fadeOutTimer, 0.0f, _fadeOutTime, 0.0f, 1.0f, true, EASING_TYPE.SINE_EASE_OUT);
			PixelScreen.shared.AddPixels(new Color(0, 0, 0, opacity));
			
			if(_fadeOutTimer > _fadeOutTime)
			{
				_fadingOut = false;
				if(_fadeOutCallback != null)
					_fadeOutCallback();
			}
		}
	}
	
	public Entity Collide(Entity entity, string tag, int x, int y)
	{
		foreach(Entity other in _entities)
		{
			if(!other.tags.Contains(tag) || !other.collideable || other == entity)
				continue;
			
			if(entity.GetOffsetHitbox(x, y).Overlaps(other.offsetHitbox))
			{
				return other;
			}
		}
		
		return null;
	}

	public Entity Collide(Entity entity, string tag, PixelRect rect)
	{
		foreach(Entity other in _entities)
		{
			if(!other.tags.Contains(tag) || !other.collideable || other == entity)
				continue;

			if(rect.Overlaps(other.offsetHitbox))
			{
				return other;
			}
		}
		
		return null;
	}
	
	public List<Entity> CollideWithAll(Entity entity, string tag, int x, int y)
	{
		List<Entity> entities = new List<Entity>();
		
		foreach(Entity other in _entities)
		{
			if(!other.tags.Contains(tag) || !other.collideable || other == entity)
				continue;
			
			if(entity.GetOffsetHitbox(x, y).Overlaps(other.offsetHitbox))
			{
				entities.Add(other);
			}
		}
		
		return entities;
	}

	// doesn't return the specified entity
	public List<Entity> GetEntities(Entity entity, string tag)
	{
		List<Entity> entities = new List<Entity>();
		
		foreach(Entity other in _entities)
		{
			if(!other.tags.Contains(tag) || other == entity)
				continue;
			
			entities.Add(other);
		}
		
		return entities;
	}
	
	public bool IsInBounds(Entity entity, int x, int y)
	{
		PixelRect hitbox = entity.GetOffsetHitbox(x, y);
		
		if(hitbox.xMax >= PixelScreen.shared.GetScreenWidth() ||
		   hitbox.xMin < 0 ||
		   hitbox.yMax >= PixelScreen.shared.GetScreenHeight() ||
		   hitbox.yMin < 0)
		{
			return false;
		}
		
		return true;
	}
	
	public void AddEntity(Entity e)
	{
		_entitesToAdd.Add(e);
	}
	
	public void RemoveEntity(Entity e)
	{
		_entitesToRemove.Add(e);
	}
	
	public virtual void Pause()
	{
		
	}
	
	public virtual void Unpause()
	{
		
	}
	
	public void SwitchScene(SceneType sceneType)
	{
		_switchingScene = true;
		_switchingSceneType = sceneType;
	}
}





















