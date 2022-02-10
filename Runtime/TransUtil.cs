using UnityEngine;
namespace Ambilight.Unity.Extension
{
	public static partial class TransUtil
	{
        #region 设置物体相对摄像机位置--------------------------------

        /// <summary>
        /// 摆放到摄像机前方
        /// 并且面向摄像机
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="fixedDistance"></param>
        /// <param name="distance"></param>
        public static void ShowAtCameraFront(this Transform trans, bool fixedDistance, float distance = 0.1f)
		{
			if (Camera.main == null) return;
			if (fixedDistance == false)
			{
				RaycastHit info;
				Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				if (Physics.Raycast(ray, out info, float.MaxValue))
					distance = Mathf.Min(distance, info.distance * 0.8f);
			}

			trans.position = Camera.main.transform.position + Camera.main.transform.forward * distance;

			trans.rotation = Quaternion.LookRotation(trans.position - Camera.main.transform.position, Vector3.up);
		}

		/// <summary>
		/// 垂直摆在摄像机前方
		/// 不考虑摄像机的垂直朝向问题
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="fixedDistance"></param>
		/// <param name="distance"></param>
		public static void ShowAtCameraVerticalFront(this Transform trans, bool fixedDistance, float distance = 0.1f, float verticalOffset = 0f)
		{
			if (Camera.main == null) return;
			if (fixedDistance == false)
			{
				RaycastHit info;
				Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				if (Physics.Raycast(ray, out info, float.MaxValue))
					distance = Mathf.Min(distance, info.distance * 0.8f);
			}

			Vector3 camFwd = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;

			trans.position = Camera.main.transform.position + camFwd * distance + Vector3.up * verticalOffset;
			trans.rotation = Quaternion.LookRotation(camFwd, Vector3.up);
		}

		/// <summary>
		/// 将物体摆放在摄像机指定的一个方向
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="localDir"></param>
		/// <param name="distance"></param>
		public static void ShowAtCameraSide(this Transform trans, Vector3 localDir, float distance = 0.2f)
		{
			Vector3 dir = Camera.main.transform.localToWorldMatrix.MultiplyVector(localDir.normalized).normalized;

			trans.position = Camera.main.transform.position + dir * distance;

			trans.rotation = Quaternion.LookRotation(trans.position - Camera.main.transform.position, Vector3.up);

			trans.localScale = Vector3.one * 0.5f;
		}


		/// <summary>
		/// 将物体垂直摆放到摄像机的某个方向上
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="dir"></param>
		/// <param name="distance"></param>
		public static void ShowAtCameraVerticalSide(this Transform trans, Vector3 dir, float distance = 0.4f)
		{
			Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
			Vector3 camRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
			dir = (camForward * dir.z + camRight * dir.x).normalized;
			trans.position = Camera.main.transform.position + dir * distance;

			trans.rotation = Quaternion.LookRotation(trans.position - Camera.main.transform.position, Vector3.up);
		}


		/// <summary>
		/// 将物体摆放到摄像机的某个角度上
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="angle"></param>
		/// <param name="distance"></param>
		public static void ShowAtCameraSideInAngle(this Transform trans, float angle, float distance = 0.8f)
		{
			Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
			camForward = Quaternion.AngleAxis(angle, Vector3.up) * camForward;

			trans.position = Camera.main.transform.position + camForward * distance;
			trans.rotation = Quaternion.LookRotation(trans.position - Camera.main.transform.position, Vector3.up);
		}



		public static void ShowAtCameraSideInAngle(this Transform trans, Axis axis, float angle, float distance = 0.8f)
		{
			Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
			Vector3 axisDir =
			camForward = Quaternion.AngleAxis(angle, Vector3.up) * camForward;

			trans.position = Camera.main.transform.position + camForward * distance;
			trans.rotation = Quaternion.LookRotation(trans.position - Camera.main.transform.position, Vector3.up);
		}


        #endregion


        public static void ResetTrans(this Transform trans)
		{
			trans.localScale = Vector3.one;
			trans.localPosition = Vector3.zero;
			trans.localRotation = Quaternion.identity;
		}

	}

}

