using UnityEngine;
namespace Ambilight.Unity.Extension

{
	public enum Axis
	{
		Up, Right, Forward
	}


	public static class MatrixUtil
	{

		public static Vector3 TransformVector_Local2World(Matrix4x4 cam2world, Vector3 localVec)
		{
			return cam2world.MultiplyVector(localVec);
		}



		public static Vector3 GetCamWorldPos(Matrix4x4 cam2World)
		{
			return cam2World.MultiplyPoint(Vector3.zero);
		}




		public static Vector3 GetScreenWorldPos(Matrix4x4 cam2world, Matrix4x4 camProj, Vector2 screenPos)
		{
			Vector4 camSpacePos = camProj.inverse.MultiplyPoint(screenPos);
			Vector4 worldPos = cam2world.MultiplyPoint(camSpacePos);
			return worldPos;
		}



		public static Vector3 GetScreenWorldPosWithDepth(Matrix4x4 cam2world, Matrix4x4 camProj, Vector2 screenPos, float depth)
		{
			Vector3 camSpacePos = camProj.inverse.MultiplyPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(depth)));
			return cam2world.MultiplyPoint(camSpacePos);
		}



		/// <summary>
		/// 基于一个点的世界坐标作参考，反算屏幕坐标的世界坐标
		/// </summary>
		/// <param name="cam2world"></param>
		/// <param name="camProj"></param>
		/// <param name="basePos"></param>
		/// <param name="screenPos"></param>
		/// <returns></returns>
		public static Vector3 GetScreenWorldPosWithBasePos(Matrix4x4 cam2world, Matrix4x4 camProj, Vector3 basePos, Vector2 screenPos)
		{
			float depth = GetProjectDepth(cam2world, camProj, basePos);
			Vector4 camSpacePos = camProj.inverse.MultiplyPoint(new Vector3(screenPos.x, screenPos.y, depth));
			return cam2world.MultiplyPoint(camSpacePos);
		}



		/// <summary>
		/// 通过屏幕空间坐标值算出在相机坐标系的坐标
		/// </summary>
		/// <param name="camProj">相机投影矩阵</param>
		/// <param name="screenPos">屏幕位置坐标（原点在屏幕中心，xy范围是-1~1）</param>
		/// <param name="camViewDepth">想把位置放在的深度（0~1），0表示近剪裁面，1表示远剪裁面，但是距离和这个值不是线性关系</param>
		/// <returns></returns>
		public static Vector3 GetCameraSpacePos(Matrix4x4 camProj, Vector2 screenPos, float depth)
		{

			//Z值为1，可以达到远剪裁面，为0则近似为近剪裁面
			Vector3 camViewPos = camProj.inverse.MultiplyPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(depth)));
			return camViewPos;
		}



		/// <summary>
		/// 计算一个世界坐标系下的点的投影坐标的深度
		/// </summary>
		/// <param name="cam2world"></param>
		/// <param name="project"></param>
		/// <param name="worldPos"></param>
		/// <returns></returns>
		public static float GetProjectDepth(Matrix4x4 cam2world, Matrix4x4 project, Vector3 worldPos)
		{
			Vector3 camSpaceCenterPos = cam2world.inverse.MultiplyPoint(worldPos);
			return project.MultiplyPoint(camSpaceCenterPos).z;
		}



		public static Vector3 GetCameraAxis(Axis dir, Matrix4x4 cam2world)
		{
			Vector3 result = Vector3.zero;
			Vector3 camWorldPos = cam2world.MultiplyPoint(Vector3.zero);
			switch (dir)
			{
				case Axis.Forward:
					result = cam2world.MultiplyVector(Vector3.forward);//Unity中摄像机的Z轴是反的
					break;
				case Axis.Right:
					result = cam2world.MultiplyVector(Vector3.right);
					break;
				case Axis.Up:
					result = cam2world.MultiplyVector(Vector3.up);
					break;
			}
			result -= camWorldPos;
			return result.normalized;
		}


		public static Vector3 GetCamAxis(Axis axisType)
		{
			Vector3 axis = Vector3.forward;
			switch (axisType)
			{
				case Axis.Forward:
					axis = Camera.main.transform.forward;
					break;
				case Axis.Right:
					axis = Camera.main.transform.right;
					break;
				case Axis.Up:
					axis = Camera.main.transform.up;
					break;

			}

			return axis;
		}


		public static Vector3 GetCamAxisProj(Axis axisType, Vector3 planeNormal)
		{
			Vector3 axis = Vector3.forward;

			switch (axisType)
			{
				case Axis.Forward:
					axis = Camera.main.transform.forward;
					break;
				case Axis.Right:
					axis = Camera.main.transform.right;
					break;
				case Axis.Up:
					axis = Camera.main.transform.up;
					break;
			}
			return Vector3.ProjectOnPlane(axis, planeNormal);
		}

	}
}