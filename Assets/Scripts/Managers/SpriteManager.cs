using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ParseAnimState { Name, Hitbox, Frame, DifferencesMode, PixelColor, PixelYPos, PixelXPos, AnimTime, Loops, Size };

public class SpriteManager
{
	public static Dictionary<string, List<AnimationData>> animations = new Dictionary<string, List<AnimationData>>();

	public static void PlayOneShot(string spriteName, string animName, int x, int y, Scene scene, int layer)
	{
		Entity entity = new Entity();
		entity.posX = x;
		entity.posY = y;
		entity.scene = scene;
		scene.AddEntity(entity);
		entity.layer = layer;
		entity.SetSprite(spriteName);
		entity.sprite.PlayAnimation(animName);
		entity.sprite.SetAnimCompleteCallback(delegate {
			scene.RemoveEntity(entity);
		});
	}

	public static void ParseAnimationStrings()
	{
		Object[] animFiles = Resources.LoadAll("Animations", typeof(TextAsset));
		foreach(TextAsset animFile in animFiles)
		{
			TextAsset textAsset = (TextAsset)animFile;
			string[] lines = textAsset.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
			
			string currentType = "";
			for(int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				if(line.Length > 0)
				{
					if(line[0] != '*')
					{
						currentType = line;
					}
					else
					{
						if(!ParseAnimationString(line, currentType))
						{
							Debug.LogError("ERROR reading animation string... " + currentType + ": " + line);
						}
					}
				}
			}
		}
	}
	
	// ** name
	// !! anim size
	// <> anim hitbox
	// {} frame
	// [] color
	// (y-x,x,x,x,etc) pixels
	// ## frame time
	// && anim loop mode
	static bool ParseAnimationString(string animString, string type)
	{
		Stack<ParseAnimState> animStates = new Stack<ParseAnimState>();
		string currentString = "";
		
		// ANIMATION DATA
		string name = "";
		List<FrameData> frames = new List<FrameData>();
		LoopMode loopMode = LoopMode.Loops;
		
		// CURRENT FRAME DATA
		Color32 currentColor = new Color32(0, 0, 0, 0);
		List<PixelData> currentPixels = new List<PixelData>();
		List<PixelData> previousFramePixels = new List<PixelData>();
		PixelPoint animSize = new PixelPoint(0, 0);
		PixelRect hitbox = new PixelRect(0, 0, 0, 0);
		float currentAnimTime = 0.0f;
		int currentPixelYPos = 0;
		
		bool differencesMode = false;
		
		foreach(char c in animString)
		{
			if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.DifferencesMode)
			{
				if(c == '^')
					differencesMode = true;
				else
					differencesMode = false;
				
				animStates.Pop();
				animStates.Push(ParseAnimState.Frame);
			}
			else
			{
				if(c == '*')
				{
					if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.Name)
					{
						name = currentString;
						currentString = "";
						animStates.Pop();
					}
					else
					{
						animStates.Push(ParseAnimState.Name);
						currentString = "";
					}
				}
				else if(c == '!')
				{
					if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.Size)
					{
						// parse current string for animation rect and save it
						string[] vals = currentString.Split(',');
						if(vals.Length == 2)
						{
							int xSize = 0;
							int ySize = 0;
							if(!(int.TryParse(vals[0], out xSize) && int.TryParse(vals[1], out ySize)))
								return false;
							
							animSize = new PixelPoint(xSize, ySize);
						}
						else
						{
							Debug.LogError(currentString + " is not properly formatted anim size data!");
							return false;
						}
						
						currentString = "";
						animStates.Pop();
					}
					else
					{
						animStates.Push(ParseAnimState.Size);
					}
				}
				else if(c == '<')
				{
					animStates.Push(ParseAnimState.Hitbox);
				}
				else if(c == '>')
				{
					if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.Hitbox)
					{
						// parse current string for hitbox info and save it
						string[] vals = currentString.Split(',');
						if(vals.Length == 4)
						{
							int left = 0;
							int bottom = 0;
							int width = 0;
							int height = 0;
							if(!(int.TryParse(vals[0], out left) &&
							     int.TryParse(vals[1], out bottom) &&
							     int.TryParse(vals[2], out width) &&
							     int.TryParse(vals[3], out height)))
								return false;
							
							hitbox = new PixelRect(left, bottom, width, height);
						}
						else
						{
							Debug.LogError(currentString + " is not properly formatted hitbox data!");
							return false;
						}
						
						currentString = "";
						animStates.Pop();
					}
				}
				else if(c == '{')
				{
					animStates.Push(ParseAnimState.DifferencesMode);
				}
				else if(c == '}')
				{
					if(differencesMode)
					{
						// currentPixels holds pixels that are DIFFERENT than the previous frame
						List<PixelData> pixels = new List<PixelData>();
						foreach(PixelData prevPixelData in previousFramePixels)
						{
							// only add the previous frame pixels that AREN'T being overriden by the new frame data
							bool allowPixel = true;
							foreach(PixelData pixel in currentPixels)
							{
								if(pixel.position == prevPixelData.position)
								{
									allowPixel = false;
									break;
								}
							}
							
							if(allowPixel)
								pixels.Add(new PixelData(prevPixelData.position, prevPixelData.color));
						}
						
						// add in the new, different pixels
						foreach(PixelData newPixel in currentPixels)
						{
							if(newPixel.color.a > 0.0f)
								pixels.Add(newPixel);
						}
						
						currentPixels.Clear();
						
						frames.Add(new FrameData(pixels, currentAnimTime));
						
						previousFramePixels.Clear();
						previousFramePixels.AddRange(pixels);
					}
					else
					{
						// save current frame data
						List<PixelData> pixels = new List<PixelData>();
						pixels.AddRange(currentPixels);
						currentPixels.Clear();
						
						frames.Add(new FrameData(pixels, currentAnimTime));
						
						previousFramePixels.Clear();
						previousFramePixels.AddRange(pixels);
					}
					
					if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.Frame)
						animStates.Pop();
				}
				else if(c == '[')
				{
					animStates.Push(ParseAnimState.PixelColor);
				}
				else if(c == ']')
				{
					if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.PixelColor)
					{
						// parse current string for pixel color
						if(currentString.Length == 0)
						{
							currentColor = new Color32(0, 0, 0, 0);
						}
						else
						{
							string[] vals = currentString.Split(',');
							if(vals.Length == 1)
							{
								byte grey = 0;
								if(!byte.TryParse(vals[0], out grey))
									return false;
								currentColor = new Color32(grey, grey, grey, 255);
							}
							else if(vals.Length == 2)
							{
								byte grey = 0;
								byte opacity = 0;
								if(!(byte.TryParse(vals[0], out grey) && byte.TryParse(vals[1], out opacity)))
									return false;
								currentColor = new Color32(grey, grey, grey, opacity);
							}
							else if(vals.Length == 3)
							{
								byte red = 0;
								byte green = 0;
								byte blue = 0;
								if(!(byte.TryParse(vals[0], out red) && byte.TryParse(vals[1], out green) && byte.TryParse(vals[2], out blue)))
									return false;
								currentColor = new Color32(red, green, blue, 255);
							}
							else if(vals.Length == 4)
							{
								byte red = 0;
								byte green = 0;
								byte blue = 0;
								byte opacity = 0;
								if(!(byte.TryParse(vals[0], out red) && byte.TryParse(vals[1], out green) && byte.TryParse(vals[2], out blue) && byte.TryParse(vals[3], out opacity)))
									return false;
								currentColor = new Color32(red, green, blue, opacity);
							}
							else
							{
								Debug.LogError(currentString + " is not properly formatted color data!");
								return false;
							}
						}
						
						currentString = "";
						animStates.Pop();
					}
				}
				else if(c == '(')
				{
					animStates.Push(ParseAnimState.PixelYPos);
				}
				else if(c == '-')
				{
					if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.PixelYPos)
					{
						if(!int.TryParse(currentString, out currentPixelYPos))
							return false;
						currentString = "";
						animStates.Pop();
						
						animStates.Push(ParseAnimState.PixelXPos);
					}
				}
				else if(c == ')')
				{
					if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.PixelXPos)
					{
						// parse current string for x pixel positions (for the current pixel y pos)
						// add a pixel of current color to a list
						string[] vals = currentString.Split(',');
						for(int i = 0; i < vals.Length; i++)
						{
							int xPos = 0;
							if(!int.TryParse(vals[i], out xPos))
								return false;
							
							currentPixels.Add(new PixelData(new PixelPoint(xPos, currentPixelYPos), currentColor));
						}
						
						animStates.Pop();
					}
					currentString = "";
				}
				else if(c == '#')
				{
					if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.AnimTime)
					{
						// parse current string for animation time and save it
						if(!float.TryParse(currentString, out currentAnimTime))
							return false;
						
						currentString = "";
						animStates.Pop();
					}
					else
					{
						animStates.Push(ParseAnimState.AnimTime);
					}
				}
				else if(c == '&')
				{
					if(animStates.Count > 0 && animStates.Peek() == ParseAnimState.Loops)
					{
						// parse current string for loop mode and save it
						int loopModeInt = 0;
						if(!int.TryParse(currentString, out loopModeInt))
							return false;
						
						loopMode = (LoopMode)loopModeInt;
						currentString = "";
						animStates.Pop();
					}
					else
					{
						animStates.Push(ParseAnimState.Loops);
					}
				}
				else
				{
					currentString += c;
				}
			}
		}
		
		AnimationData animData = new AnimationData(name, frames, animSize, hitbox, loopMode);
		if(animations.ContainsKey(type))
			animations[type].Add(animData);
		else
			animations.Add(type, new List<AnimationData>() { animData });
		
		return true;
	}
}
