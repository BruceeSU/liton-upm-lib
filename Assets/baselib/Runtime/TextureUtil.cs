using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class TextureUtil
{
	public static Texture2D GetTexture(Color color, int width, int height, bool bInitPixel)
	{
		Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		if (bInitPixel == true)
		{
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; ++j)
					tex.SetPixel(i, j, color);
			}
		}

		return tex;
	}


	public static Texture GetSpriteRectTexture(Sprite sprite)
	{
		if (sprite == null)
			return null;

		if (sprite.texture.isReadable == false)
		{
			Debug.LogError("图片不可读");
			return null;
		}

		Rect rect = sprite.rect;

		Color[] cols = sprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
		Texture2D tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
		tex.SetPixels(cols);
		tex.Apply();
		return tex;
	}

	public static Texture2D CreateGroupIcon(List<Texture> icons, int size = 512)
	{
		if (icons == null || icons.Count <= 0)
		{
			Debug.LogError("没有传入图片列表");
			return GetTexture(Color.cyan, size, size, true);//默认是个青色图片
		}

		DynamicGrid grid = new DynamicGrid(size, icons.Count);
		int cellSize = grid.CellSize;//计算好的子方格大小

		Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
		Color defaultCol = new Color(1f, 1f, 1f, 1f);//默认是全透明
		for (int i = 0; i < size; ++i)
			for (int j = 0; j < size; ++j)
				tex.SetPixel(i, j, defaultCol);

		RenderTexture cache = RenderTexture.GetTemporary(cellSize, cellSize);
		Texture2D cell = new Texture2D(cellSize, cellSize);
		RenderTexture orgRT = RenderTexture.active;

		for (int i = 0; i < icons.Count; ++i)
		{
			Texture icon = icons[i];
			RenderTexture.active = cache;
			Graphics.Blit(icon, cache);//将图片分辨率缩小 
			Rect rect = grid.GetGridPos(i);
			tex.ReadPixels(new Rect(0, 0, cellSize, cellSize), (int)rect.x, (int)rect.y);
		}

		tex.Apply();
		RenderTexture.active = orgRT;
		RenderTexture.ReleaseTemporary(cache);
		Object.Destroy(cell);
		return tex;
	}

	/// <summary>
	/// 将纹理缩放成对应尺寸
	/// </summary>
	/// <param name="source"></param>
	/// <param name="toWidth"></param>
	/// <param name="toHeight"></param>
	/// <returns></returns>
	public static Texture2D GetScaledTexture(Texture2D source, int toWidth, int toHeight)
	{
		Texture2D newTex = GetTexture(Color.white, toWidth, toHeight, false);
		for (int i = 0; i < toHeight; i++)
		{
			for (int j = 0; j < toWidth; j++)
			{
				Color newColor = source.GetPixelBilinear((float)j / toWidth, (float)i / toHeight);
				newTex.SetPixel(j, i, newColor);
			}
		}

		newTex.Apply();
		return newTex;
	}

	public static void Blit(Texture2D source, Texture2D dest)
	{
		Vector2 rate = new Vector2(source.width / (float)dest.width, source.height / (float)dest.height);

		for (int i = 0; i < dest.width; i++)
		{
			for (int j = 0; j < dest.height; j++)
			{
				Color newColor = source.GetPixel(Mathf.RoundToInt(i * rate.x), Mathf.RoundToInt(j * rate.y));
				dest.SetPixel(i,j, newColor);
			}
		}
		dest.Apply();
	}

	/// <summary>
	/// 取中央的矩形纹理
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static Texture2D GetMiddleRectTexture(Texture2D source)
	{
		if (source.width > source.height)
		{
			int width = source.height;
			int delta = (source.width - source.height) / 2;
			Texture2D newTex = GetTexture(Color.white, width, width, false);
			for (int i = 0; i < width; i++) //y
			{
				for (int j = 0; j < width; j++) //x
				{
					Color color = source.GetPixel(j + delta, i);
					newTex.SetPixel(j, i, color);
				}
			}
			return newTex;
		}
		else if (source.width < source.height)
		{
			int width = source.width;
			int delta = (source.height - source.width) / 2;
			Texture2D newTex = GetTexture(Color.white, width, width, false);
			for (int i = 0; i < width; i++) //y
			{
				for (int j = 0; j < width; j++) //x
				{
					Color color = source.GetPixel(j, i + delta);
					newTex.SetPixel(j, i, color);
				}
			}

			return newTex;
		}
		else
		{
			return source;
		}
	}

	/// <summary>
	/// 改变图片分辨率
	/// </summary>
	/// <param name="tex"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <returns></returns>
	public static Texture2D ChangeResolution(Texture texture, int width, int height)
	{
		Texture2D tex = texture as Texture2D;
		if (tex == null) return null;
		if (tex.width == width && tex.height == height)
			return tex;

		Texture2D newTex = new Texture2D(width, height);
		for (int i = 0; i < width; ++i)
		{
			for (int j = 0; j < height; ++j)
			{
				float xRate = i / (float)width;
				float yRate = j / (float)height;
				int x = Mathf.RoundToInt(xRate * tex.width);
				int y = Mathf.RoundToInt(yRate * tex.height);
				newTex.SetPixel(i, j, tex.GetPixel(x, y));
			}
		}
		newTex.Apply();
		return newTex;
	}


	/// <summary>
	/// 改变图片尺寸
	/// </summary>
	/// <param name="texture"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <returns></returns>
	public static Texture2D ChangeSize(Texture2D texture, int width, int height)
	{
		if (texture == null) return null;
		if (texture.width == width && texture.height == height)//尺寸没变
			return texture;

		if (width > texture.width || height > texture.height)
		{
			Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
			tex.SetPixels(0, 0, texture.width, texture.height, texture.GetPixels());
			tex.Apply();
			return tex;
		}

		return null;
	}
}


public class DynamicGrid
{
	public int totalSize;
	private int cellSize;
	private int pushedNum;
	public int CellSize => cellSize;


	/// <summary>
	/// 每个子方格引用一个锚点位置的索引，从锚点位置做偏移
	/// 偏移量就是像素数量
	/// </summary>
	private List<Vector3Int> grids;
	private List<Vector2Int> anchors;

	private DynamicGrid(int size)
	{
		totalSize = size;//总方格尺寸
		cellSize = totalSize / 3;//子方格尺寸
		pushedNum = 0;//当前子方格的数量
		grids = new List<Vector3Int>();
		anchors = new List<Vector2Int>();
	}

	public DynamicGrid(int size, int gridNum)
		: this(size)
	{
		int num = Mathf.Min(gridNum, 9);
		if (num <= 4) cellSize = size / 2;

		for (int i = 0; i < num; ++i)//构建布局
			Push();
	}


	/// <summary>
	/// 往里压入一个格子
	/// 調用这个方法会造成其他所有的格子位置变化
	/// </summary>
	public void Push()
	{
		if (pushedNum == 9)//最多只能放置9个格子
			return;

		++pushedNum;
		int refedAnchorIndex = 0;
		int startPos;

		switch (pushedNum)
		{
			case 1:
				refedAnchorIndex = 0;
				startPos = totalSize / 2 - cellSize / 2;
				anchors.Add(new Vector2Int(startPos, startPos));
				grids.Add(new Vector3Int(refedAnchorIndex, 0, 0));
				break;
			case 2:
				refedAnchorIndex = 0;
				anchors[0] -= new Vector2Int(cellSize / 2, 0);//锚点被挤到左边
				grids.Add(new Vector3Int(refedAnchorIndex, cellSize, 0));
				break;
			case 3:
				refedAnchorIndex = 1;//
				anchors[0] -= new Vector2Int(0, cellSize / 2);//锚点被挤到下面
				startPos = totalSize / 2 - cellSize / 2;
				anchors.Add(new Vector2Int(startPos, totalSize / 2));
				grids.Add(new Vector3Int(refedAnchorIndex, 0, 0));
				break;
			case 4:
				refedAnchorIndex = 1;//
				anchors[1] = new Vector2Int(totalSize / 2 - cellSize, totalSize / 2);//锚点被挤到左边
				grids.Add(new Vector3Int(refedAnchorIndex, cellSize, 0));
				break;
			case 5:
				refedAnchorIndex = 2;//
				anchors[0] = new Vector2Int(totalSize / 2 - cellSize, 0);//锚点被挤到下面
				anchors[1] = new Vector2Int(totalSize / 2 - cellSize, cellSize);//锚点被挤到下面
				anchors.Add(new Vector2Int(totalSize / 2 - cellSize / 2, cellSize * 2));
				grids.Add(new Vector3Int(refedAnchorIndex, 0, 0));
				break;
			case 6:
				refedAnchorIndex = 2;//
				anchors[2] -= new Vector2Int(cellSize / 2, 0);//锚点被挤到下面
				grids.Add(new Vector3Int(refedAnchorIndex, cellSize, 0));
				break;
			case 7:
				grids[2] = new Vector3Int(0, cellSize * 2, 0);
				grids[3] = new Vector3Int(1, 0, 0);
				grids[4] = new Vector3Int(1, cellSize, 0);
				grids[5] = new Vector3Int(1, cellSize * 2, 0);
				grids.Add(new Vector3Int(2, 0, 0));
				anchors[0] = Vector2Int.zero;
				anchors[1] = new Vector2Int(0, cellSize);
				anchors[2] = new Vector2Int(0, cellSize * 2);
				break;
			case 8:
				refedAnchorIndex = 2;//
				anchors[2] = new Vector2Int(totalSize / 2 - cellSize, totalSize - cellSize);//锚点被挤到下面
				grids.Add(new Vector3Int(refedAnchorIndex, cellSize, 0));
				break;
			case 9:
				anchors[2] = new Vector2Int(0, totalSize - cellSize);//锚点被挤到下面
				grids.Add(new Vector3Int(2, cellSize * 2, 0));
				break;
		}
	}


	public Rect GetGridPos(int index)
	{
		if (index > 8)
			return new Rect(totalSize, totalSize, 0f, 0f);

		Rect rect = new Rect();
		Vector3Int refer = grids[index];
		Vector2Int pos = anchors[refer.x];//找到引用的那个锚点的位置
		pos += new Vector2Int(refer.y, refer.z);//用锚点位置加上偏移量
		rect.position = pos;
		rect.size = new Vector2(cellSize, cellSize);
		return rect;
	}

}



