using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 操作GameObject的工具类
/// </summary>
public static class GoUtils
{
	/// <summary>
	/// 获取一个物体的层级路径
	/// </summary>
	/// <param name="go"></param>
	/// <returns></returns>
	public static string GetGoNamePath(Transform go)
	{
		StringBuilder sb = new StringBuilder(go.name);
		Transform currentTrans = go.parent;
		while (currentTrans != null)
		{
			sb.Insert(0, '/');
			sb.Insert(0, currentTrans.name);
			currentTrans = currentTrans.parent;
		}
		return sb.ToString();
	}

	public static string GetGoNamePath(GameObject go)
	{
		if (go == null)
			return string.Empty;

		return GetGoNamePath(go.transform);
	}

	/// <summary>
	/// 通过路径查找指定名字的物体
	/// </summary>
	/// <param name="root"></param>
	/// <param name="path"></param>
	/// <returns></returns>
	public static Transform FindGoByNamePath(Transform root, string path)
	{
		if (path == null || path == string.Empty) return null;
		if (root == null) return null;
		Transform currentParent = root;
		string[] gos = path.Split('/');
		if (gos.Length == 0) return null;
		//if (gos.Length == 1) return root;
		bool error = false;
		for (int i = 0; i < gos.Length; ++i)
		{
			Transform newParent = currentParent.Find(gos[i]);
			if (newParent != null) currentParent = newParent;
			else
			{
				error = true;
				break;
			}
		}
		if (error) return null;
		else return currentParent;
	}

	/// <summary>
	/// 依据名称路径来查找对应的物体
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public static Transform FindGoByNamePath(string path)
	{
		if (string.IsNullOrEmpty(path)) return null;
		string rootName = path;
		bool findChild = false;
		if (path.Contains("/"))
		{
			rootName = path.Substring(0, path.IndexOf('/'));
			findChild = true;
		}
		GameObject root = GameObject.Find(rootName);
		if (root == null)
		{
			Debug.LogErrorFormat("Can not find root gameobject:[{0}]", rootName);
			return null;
		}
		if (findChild) return FindGoByNamePath(root.transform, path.Substring(path.IndexOf('/') + 1));
		else return root.transform;
	}


	/// <summary>
	/// 获取树形结构的模型中心点
	/// 没有网格组件的节点不计算在内
	/// </summary>
	/// <param name="rootGo"></param>
	/// <returns></returns>
	public static Vector3 GetMeshesWorldCenter(Transform rootGo)
	{
		Vector3 center = Vector3.zero;
		int meshNum = 0;
		ForeachTreeNode(rootGo, (Transform node) =>
		{
			MeshRenderer renderer = node.GetComponent<MeshRenderer>();
			if (renderer != null)
			{
				++meshNum;
				center += renderer.bounds.center;
			}
		});
		if (meshNum == 0) return rootGo.position;
		center /= meshNum;
		return center;
	}


	/// <summary>
	/// 获取物体的模型中心点和包围盒半径
	/// </summary>
	/// <param name="rootGo"></param>
	/// <returns></returns>
	public static Vector4 GetMeshesWorldCenterAndSize(Transform rootGo)
	{
		Vector3 center = Vector3.zero;
		float size = 0;

		int meshNum = 0;
		ForeachTreeNode(rootGo, (Transform node) =>
		{
			MeshRenderer renderer = node.GetComponent<MeshRenderer>();
			if (renderer != null)
			{
				++meshNum;
				center += renderer.bounds.center;

				size = Mathf.Max(size, renderer.bounds.extents.magnitude);
			}
		});

		if (meshNum == 0) return rootGo.position;

		center /= meshNum;
		Vector4 result = center;
		result.w = size;//包围球的半径

		return result;
	}


	/// <summary>
	/// 获取物体的本地中心点
	/// （不是Transform.position）
	///  是所有子网格的中心点
	/// </summary>
	/// <param name="rootGo"></param>
	/// <returns></returns>
	public static Vector3 GetMeshesLocalCenter(Transform rootGo)
	{
		Vector3 center = Vector3.zero;
		int meshNum = 0;
		ForeachTreeNode(rootGo, (Transform node) =>
		{
			var filter = node.GetComponent<MeshFilter>();
			if (filter != null)
			{
				++meshNum;
				center += filter.mesh.bounds.center;
			}
		});
		if (meshNum == 0)
			return Vector3.zero;
		center /= meshNum;
		return center;
	}

	/// <summary>
	/// 计算一个物体的外包围盒，
	/// 子物体的MeshRenderer也会计算在内
	/// </summary>
	/// <param name="root"></param>
	/// <returns></returns>
	public static Bounds GetWorldBounds(Transform root)
	{
		if (root == null)
			return new Bounds() { center = root.position };

		var renders = root.GetComponentsInChildren<MeshRenderer>();
		if (renders == null || renders.Length <= 0)
			return new Bounds() { center = root.position };

		Bounds result = renders[0].bounds;
		for (int i = 1; i < renders.Length; ++i)
		{
			var rend = renders[i];
			result = CombineBounds(result, rend.bounds);
		}

		return result;
	}

	/// <summary>
	/// 将两个包围盒合并
	/// </summary>
	/// <param name="b1"></param>
	/// <param name="b2"></param>
	/// <returns></returns>
	public static Bounds CombineBounds(Bounds b1, Bounds b2)
	{
		Vector3 max1 = b1.center + b1.extents;
		Vector3 min1 = b1.center - b1.extents;

		Vector3 max2 = b2.center + b2.extents;
		Vector3 min2 = b2.center - b2.extents;

		Vector3 max = new Vector3
		(
			Mathf.Max(max1.x, max2.x),
			Mathf.Max(max1.y, max2.y),
			Mathf.Max(max1.z, max2.z)
		);

		Vector3 min = new Vector3
		(
			Mathf.Min(min1.x, min2.x),
			Mathf.Min(min1.y, min2.y),
			Mathf.Min(min1.z, min2.z)
		);

		Vector3 center = (min + max) * 0.5f;
		Vector3 extens = max - center;

		return new Bounds()
		{
			center = center,
			extents = extens
		};
	}


	/// <summary>
	/// 遍历每个树节点
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="nodeRoot"></param>
	/// <param name="nodeHandler"></param>
	public static void ForeachTreeNode<T>(T nodeRoot, UnityAction<T> nodeHandler) where T : IEnumerable
	{
		nodeHandler(nodeRoot);
		foreach (T child in nodeRoot)
		{
			ForeachTreeNode<T>(child, nodeHandler);
		}
	}
}
