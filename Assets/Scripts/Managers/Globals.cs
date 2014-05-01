using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction { None, Left, Right, Up, Down };

public class Globals : MonoBehaviour
{
	private static Globals _instance;
	public static Globals shared { get{ return _instance; } }
	
	public bool DEBUG_MESSAGES = false;
	public bool DEBUG_HITBOXES = false;
	public Color DebugHitboxColor = new Color(1.0f, 0.0f, 0.0f, 0.3f);

	public static int LAYER_BOX = -2;
	public static int LAYER_PLAYER = -1;
	public static int LAYER_TEXT = 0;
	
	void Awake()
	{
		_instance = this;
	}
}



















