using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Camera))]
public class PixelScreen : MonoBehaviour
{
	private static PixelScreen _instance;
	public static PixelScreen shared { get{ return _instance; } }
	
	bool _initialized = false;
	
	GameObject _screenPlane;
	
	public int pixelWidth;
	public int pixelHeight;
	[HideInInspector] public int pixelSize;

	public int GetScreenWidth() { return pixelWidth; }
	public int GetScreenHeight() { return pixelHeight; }
	
	Texture2D _texture;
	Color32[] _pixels;
	Color32[] _clearCache;
	Color32 _clearColor;
	
	bool _dirtyPixels = true;
	
	public PixelPoint cameraPos;
	public int viewportXMin { get { return cameraPos.x; } }
	public int viewportXMax { get { return cameraPos.x + pixelWidth - 1; } }
	public int viewportYMin { get { return cameraPos.y; } }
	public int viewportYMax { get { return cameraPos.y + pixelHeight - 1; } }
	
	bool _drawInGUISpace = false;
	public bool DrawInGUISpace { get { return _drawInGUISpace; } set { _drawInGUISpace = value; } }
	
	int _shakeAmount;
	public int ShakeAmount { get { return _shakeAmount; } set { _shakeAmount = value; } }
	
	void Awake()
	{
		_instance = this;
		
	}
	
	void Start()
	{
		
	}
	
	public bool Init()
	{
		if(!_initialized)
		{
			// these seem to return 0 sometimes if we check them too early
			// so we won't consider the pixel screen initialized until we can get back a non-zero value for these
			if(Screen.width == 0 || Screen.height == 0)
				return false;

			pixelSize = Screen.width / pixelWidth;

			Camera camera = GetComponent<Camera>();
			camera.orthographic = true;
			camera.orthographicSize = 1;
			
			// Create remote plane
			_screenPlane = new GameObject();
			MeshRenderer meshRenderer = (MeshRenderer)_screenPlane.AddComponent(typeof(MeshRenderer));
			MeshFilter meshFilter = (MeshFilter)_screenPlane.AddComponent(typeof(MeshFilter));
			_screenPlane.transform.parent = transform;
			_screenPlane.transform.localPosition = Vector3.forward;
			float aspect = (float)Screen.width / (float)Screen.height;
			_screenPlane.transform.localScale = new Vector3(aspect, 1, 1);
			
			Mesh mesh = new Mesh();
			
			mesh.vertices = new Vector3[] {
				new Vector3(-1, 1, 0),
				new Vector3(1, 1, 0),
				new Vector3(-1, -1, 0),
				new Vector3(1, -1, 0)
			};
			
			mesh.triangles = new int[] {0, 1, 2, 1, 3, 2};
			
			mesh.uv = new Vector2[] {
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector2(0, 0),
				new Vector2(1, 0)
			};
			
			meshFilter.mesh = mesh;
			
			// create texture
			_texture = new Texture2D(pixelWidth, pixelHeight, TextureFormat.ARGB32, false);
			_texture.filterMode = FilterMode.Point;
			
			_pixels = new Color32[pixelWidth * pixelHeight];
			for(int i = 0; i < _pixels.Length; i++)
				_pixels[i] = new Color32(0, 0, 0, 255);
			
			_texture.SetPixels32(_pixels);
			_texture.Apply();
			
			// Create material
			Material material = (Material)Instantiate(Resources.Load("Materials/Unlit") as Material);
			GameManager.shared.AddDebugText("material: " + material);
			material.mainTexture = _texture;
			meshRenderer.material = material;
			
			GameManager.shared.AddDebugText("done PixelScreen::Awake");
			GameManager.shared.AddDebugText("texture: " + _texture + " dirtyPixels: " + _dirtyPixels);
			
			_initialized = true;
		}
		
		return _initialized;
	}
	
	void LateUpdate()
	{
		if(!_initialized)
			return;
		
		if(_shakeAmount > 0)
		{
			PixelScreen.shared.ShiftPixels(new PixelPoint(Random.Range(-_shakeAmount, _shakeAmount), Random.Range(-_shakeAmount, _shakeAmount)));
			_shakeAmount--;
			_dirtyPixels = true;
		}
		
		if(_dirtyPixels)
		{
			_texture.SetPixels32(_pixels);
			_texture.Apply();
			
			_dirtyPixels = false;
		}
	}
	
	public void Clear(Color color)
	{
		if(color != _clearColor || _clearCache.Length != _pixels.Length)
		{
			int length = _pixels.Length;
			
			if(_clearCache == null || _clearCache.Length != length)
				_clearCache = new Color32[length];
			
			for(int i = 0; i < length; i++)
				_clearCache[i] = (Color32)color;
			
			_clearColor = color;
		}
		
		_clearCache.CopyTo(_pixels, 0);
		_dirtyPixels = true;
	}
	
	public void AddColor(Color color)
	{
		for(int i = 0; i < _pixels.Length; i++)
			_pixels[i] = (Color32)((Color)_pixels[i] + color);
		
		_dirtyPixels = true;
	}
	
	public void SubtractColor(Color color)
	{
		for(int i = 0; i < _pixels.Length; i++)
			_pixels[i] = (Color32)((Color)_pixels[i] - color);
		
		_dirtyPixels = true;
	}
	
	public void SetPixel(int x, int y, Color color)
	{
		if(!_drawInGUISpace)
		{
			x -= cameraPos.x;
			y -= cameraPos.y;
		}
		
		if(x < 0 || x > pixelWidth - 1 ||
		   y < 0 || y > pixelHeight - 1)
			return;
		
		int index = y * pixelWidth + x;
		_pixels[index] = (Color32)color;
		_dirtyPixels = true;
	}

	public void SetPixel(PixelPoint point, Color color)
	{
		SetPixel(point.x, point.y, color);
	}
	
	public void AddPixel(int x, int y, Color newColor)
	{
		if(!_drawInGUISpace)
		{
			x -= cameraPos.x;
			y -= cameraPos.y;
		}
		
		if(x < 0 || x > pixelWidth - 1 ||
		   y < 0 || y > pixelHeight - 1)
			return;
		
		int index = y * pixelWidth + x;
		
		Color originalColor = (Color)_pixels[index];
		Color resultingColor = Utils.AddColors(originalColor, newColor);
		
		_pixels[index] = (Color32)resultingColor;
		
		_dirtyPixels = true;
	}

	public void AddPixel(PixelPoint point, Color newColor)
	{
		AddPixel(point.x, point.y, newColor);
	}
	
	public void AddPixels(Color newColor)
	{
		for(int i = 0; i < _pixels.Length; i++)
		{
			Color originalColor = (Color)_pixels[i];
			Color resultingColor = Utils.AddColors(originalColor, newColor);
			
			_pixels[i] = (Color32)resultingColor;
		}
		
		_dirtyPixels = true;
	}
	
	public void SubtractPixel(int x, int y, Color color)
	{
		if(!_drawInGUISpace)
		{
			x -= cameraPos.x;
			y -= cameraPos.y;
		}
		
		if(x < 0 || x > pixelWidth - 1 ||
		   y < 0 || y > pixelHeight - 1)
			return;
		
		int index = y * pixelWidth + x;
		_pixels[index] = (Color32)((Color)_pixels[index] - color);
		_dirtyPixels = true;
	}
	
	public PixelPoint GetMouseScreenPosition()
	{
		return new PixelPoint(Mathf.FloorToInt(Input.mousePosition.x / pixelSize), Mathf.FloorToInt(Input.mousePosition.y / pixelSize));
	}
	
	public PixelPoint GetMouseWorldPosition()
	{
		return new PixelPoint(Mathf.FloorToInt(Input.mousePosition.x / pixelSize), Mathf.FloorToInt(Input.mousePosition.y / pixelSize)) + cameraPos;
	}
	
	public PixelPoint GetPixelPositionForPos(Vector2 pos)
	{
		return new PixelPoint(Mathf.FloorToInt(pos.x / pixelSize), Mathf.FloorToInt(pos.y / pixelSize));
	}
	
	public void DrawLine(int x0, int y0, int x1, int y1, Color color)
	{
		bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
		if (steep) { Utils.Swap<int>(ref x0, ref y0); Utils.Swap<int>(ref x1, ref y1); }
		if (x0 > x1) { Utils.Swap<int>(ref x0, ref x1); Utils.Swap<int>(ref y0, ref y1); }
		int dX = (x1 - x0), dY = Mathf.Abs(y1 -y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;
		
		for (int x = x0; x <= x1; ++x)
		{
			if(steep)
				AddPixel(y, x, color);
			else
				AddPixel(x, y, color);
			
			err = err - dY;
			if (err < 0) { y += ystep;  err += dX; }
		}
	}

	public void DrawLine(PixelPoint a, PixelPoint b, Color color)
	{
		DrawLine(a.x, a.y, b.x, b.y, color);
	}
	
	public void ShiftPixels(PixelPoint offset)
	{
		offset = new PixelPoint(-offset.x, -offset.y);
		Color32[] tempPixels = new Color32[pixelWidth * pixelHeight];
		
		_pixels.CopyTo(tempPixels, 0);
		
		for(int x = 0; x < pixelWidth; x++)
		{
			for(int y = 0; y < pixelHeight; y++)
			{
				int currentIndex = y * pixelWidth + x;
				
				int newY = y + offset.y;
				while(newY > pixelHeight - 1)
					newY -= pixelHeight;
				while(newY < 0)
					newY += pixelHeight;
				
				int newX = x + offset.x;
				while(newX > pixelWidth - 1)
					newX -= pixelWidth;
				while(newX < 0)
					newX += pixelWidth;
				
				int copyIndex = newY * pixelWidth + newX;
				
				_pixels[currentIndex] = tempPixels[copyIndex];
				if(_pixels[currentIndex].a == 0)
					_pixels[currentIndex] = new Color32(0, 0, 0, 0);
			}
		}
	}
}





















