using UnityEngine;

public struct PixelRect
{
	public int left;
	public int bottom;
	public int width;
	public int height;

	public PixelRect(int left, int bottom, int width, int height)
	{
		this.left = left;
		this.bottom = bottom;
		this.width = width;
		this.height = height;
	}

	public int xMin { get { return left; } }
	public int xMax { get { return left + width - 1; } }
	public int yMin { get { return bottom; } }
	public int yMax { get { return bottom + height - 1; } }

	public bool Contains(PixelPoint point)
	{
		return point.x >= left && point.x < (left + width) && point.y >= bottom && point.y < (bottom + height);
	}

	public static PixelRect operator +(PixelRect rect, PixelPoint offset)
	{
		return new PixelRect(rect.left + offset.x, rect.bottom + offset.y, rect.width, rect.height);
	}

	public static bool operator ==(PixelRect a, PixelRect b)
	{
		return a.left == b.left && a.bottom == b.bottom && a.width == b.width && a.height == b.height;
	}
	
	public static bool operator !=(PixelRect a, PixelRect b)
	{
		return a.left != b.left || a.bottom != b.bottom || a.width != b.width || a.height != b.height;
	}

	public override bool Equals(object other)
	{
		if(!(other is PixelRect))
		{
			return false;
		}
		PixelRect rect = (PixelRect)other;
		return this.left.Equals(rect.left) && this.bottom.Equals(rect.bottom) && this.width.Equals(rect.width) && this.height.Equals(rect.height);
	}
	
	public override int GetHashCode()
	{
		return this.left.GetHashCode() ^ this.width.GetHashCode() << 2 ^ this.bottom.GetHashCode() >> 2 ^ this.height.GetHashCode() >> 1;
	}

	public bool Overlaps(PixelRect other)
	{
//		Debug.Log("OVERLAPS: this.xMin: " + xMin + " this.xMax: " + xMax + " this.yMin: " + yMin + " this.yMax: " + yMax +
//		          " other.xMin: " + other.xMin + " other.xMax: " + other.xMax + " other.yMin: " + other.yMin + " other.yMax: " + other.yMax);
		return other.xMax >= this.xMin && other.xMin <= this.xMax && other.yMax >= this.yMin && other.yMin <= this.yMax;
	}

	public string ToString()
	{
		return "(" + left.ToString() + ", " + bottom.ToString() + ", " + width.ToString() + ", " + height.ToString() + ")";
	}
}




















