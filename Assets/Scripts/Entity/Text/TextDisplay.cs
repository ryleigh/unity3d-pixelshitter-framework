using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TextDisplayAlignment { LeftToRight, UpToDown };

public class TextDisplay : Entity
{
	protected string _text;
	public string Text
	{
		get { return _text; }
		set
		{
			_text = value;
			UpdateHitbox();
		}
	}
	
	protected int _letterWidth;
	public int letterWidth { get { return _letterWidth; } }
	protected int _letterHeight;
	public int letterHeight { get { return _letterHeight; } }
	
	protected string _fontName;
	
	protected int _spacing = 1;
	protected int _scale;

	protected TextDisplayAlignment _alignment;
//	public TextDisplayAlignment Alignment { get { return _alignment; } set { _alignment = value; }


	bool _outlined = true;

	public void SetColor(Color color)
	{
		// assumes text sprite data is colored with black
		sprite.AddOverriddenColor(new Color32(0, 0, 0, 0),
		                          (Color32)color);
	}

	public TextDisplay(string text, Scene scene, int x, int y, string fontName, TextDisplayAlignment alignment, int scale)
	{
		_text = text;
		scene = scene;
		
		_fontName = fontName;
		SetSprite(_fontName);
		
		collideable = false;
		
		PixelPoint size = sprite.GetAnimSize(_fontName, "a");
		_letterWidth = size.x;
		_letterHeight = size.y;
		
		pixelX = x;
		pixelY = y;
		
		depth = Globals.LAYER_TEXT;
		
		_scale = scale;

		_alignment = alignment;
		UpdateHitbox();
	}

	public TextDisplay(string text, Scene scene, int x, int y, string fontName)
	: this(text, scene, x, y, fontName, TextDisplayAlignment.LeftToRight, 1) { }

	void UpdateHitbox()
	{
		if(_alignment == TextDisplayAlignment.LeftToRight)
			hitbox = new PixelRect(0, 0, ((_letterWidth * _text.Length) + (_spacing * _text.Length - 1)) * _scale, _letterHeight * _scale);
		else
			hitbox = new PixelRect(0, 0, _letterWidth * _scale, (((_letterHeight + _spacing) * _text.Length) - 1) * _scale);
	}

	public override void UpdateEntity(float deltaTime)
	{
		base.UpdateEntity(deltaTime);
		
	}
	
	public override void Draw()
	{
		base.Draw();
		
		int currentX = pixelX;
		int currentY = pixelY;
		for(int i = 0; i < _text.Length; i++)
		{
			char c = _text[i];
			
			if(c == '\n')
			{
				if(_alignment == TextDisplayAlignment.LeftToRight)
				{
					currentY -= _letterHeight * _scale;
					currentX = pixelX;
				}
				else
				{
					currentX += _letterHeight * _scale;
					currentY = pixelY;
				}
				continue;
			}
			
			List<PixelData> pixelDataList = sprite.GetPixelDataList(_fontName, c.ToString());
			if(pixelDataList != null)
				sprite.DrawPixels(pixelDataList, currentX, currentY, _scale);

			if(_alignment == TextDisplayAlignment.LeftToRight)
				currentX += (_letterWidth + _spacing) * _scale;
			else if(_alignment == TextDisplayAlignment.UpToDown)
				currentY -= (_letterHeight + _spacing) * _scale;
		}
	}
}









