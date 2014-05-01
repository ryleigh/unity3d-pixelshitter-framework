using UnityEngine;

public struct PixelPoint
{
	public int x;
	public int y;
	
	public PixelPoint(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public PixelPoint(Vector2 vector)
	{
		this.x = Mathf.RoundToInt(vector.x);
		this.y = Mathf.RoundToInt(vector.y);
	}
	
	public static PixelPoint operator +(PixelPoint a, PixelPoint b)
	{
		return new PixelPoint(a.x + b.x, a.y + b.y);
	}

	public static PixelPoint operator -(PixelPoint a, PixelPoint b)
	{
		return new PixelPoint(a.x - b.x, a.y - b.y);
	}

	public static PixelPoint operator -(PixelPoint a)
	{
		return new PixelPoint(-a.x, -a.y);
	}

	public static PixelPoint operator *(int d, PixelPoint a)
	{
		return new PixelPoint(a.x * d, a.y * d);
	}

	public static PixelPoint operator *(PixelPoint a, int d)
	{
		return new PixelPoint(a.x * d, a.y * d);
	}

	public static implicit operator PixelPoint(Vector2 v)
	{
		return new PixelPoint(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
	}

	public static implicit operator Vector2(PixelPoint v)
	{
		return new Vector2((float)v.x, (float)v.y);
	}

	public static int Distance(PixelPoint a, PixelPoint b)
	{
		return Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y);
	}

	public static bool operator ==(PixelPoint a, PixelPoint b)
	{
		return a.x == b.x && a.y == b.y;
	}
	
	public static bool operator !=(PixelPoint a, PixelPoint b)
	{
		return a.x != b.x || a.y != b.y;
	}

	public override bool Equals(object other)
	{
		if(!(other is PixelPoint))
		{
			return false;
		}
		PixelPoint pp = (PixelPoint)other;
		return this.x.Equals(pp.x) && this.y.Equals(pp.y);
	}

	public override int GetHashCode()
	{
		return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
	}

	public override string ToString()
	{
		return "(" + this.x.ToString() + ", " + this.y.ToString() + ")";
	}
}



























