using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityComponent
{
	protected Entity _entity;
	public Entity entity { get { return _entity; } set { _entity = value; } }

	public virtual void UpdateComponent(float deltaTime) { }
	public virtual void Remove() { }
}
