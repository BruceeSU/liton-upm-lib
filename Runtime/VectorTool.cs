using UnityEngine;

namespace Ambilight.Unity.Extension

{
	public static class VectorTool
	{
		/// <summary>
		/// 绕一个轴旋转一定角度
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static Vector3 Rotate(this Vector3 vec, Vector3 axis, float angle)
		{
			return Quaternion.AngleAxis(angle, axis) * vec;
		}
	}
}
