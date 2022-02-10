using System;
using UnityEngine;
using UnityEngine.Rendering;


namespace Ambilight.Unity.Extension
{

	public static class LineUtil
	{
		private static Matrix4x4 s_project, s_cam2world;
		private static GameObject fovPrefab;
		private static float near = 0.1f;
		private static float far = 1.5f;


		/// <summary>
		/// 检测是否触摸到线条
		/// </summary>
		/// <param name="line"></param>
		/// <param name="detectCenter"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public static bool TouchedLine(this LineRenderer line, Vector3 detectCenter, float radius = 0.01f)
		{
			float radiusSqr = radius * radius;

			for (int i = 0; i < line.positionCount; ++i)
			{
				if ((line.GetPosition(i) - detectCenter).sqrMagnitude <= radiusSqr)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// OpenGL的剪裁空间取值范围是：近剪裁面-1，远剪裁面1
		/// DirectX的剪裁空间取值范围是：近剪裁面0，远剪裁面1
		/// Unity使用的是OpenGL模式
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="toWorld"></param>
		/// <param name="points"></param>
		/// <param name="color"></param>
		public static void DrawScreenPointsToWorld(Matrix4x4 proj, Matrix4x4 toWorld, Vector3[] points, Color color)
		{
			Vector3 posStart = toWorld.MultiplyPoint(Vector3.zero);
			for (int i = 0; i < points.Length; ++i)
			{
				Vector3 localPos = proj.inverse.MultiplyPoint(points[i]);
				localPos.z *= -1f;
				Vector3 posEnd = toWorld.MultiplyPoint(localPos);
				Vector3 dir = (posEnd - posStart).normalized;
				GetNewLine(new Vector3[] { posStart, posStart + dir * 2f }, color);

				//根据平台得到 符合平台的剪裁矩阵模式
				Matrix4x4 shaderProjMatrix = GL.GetGPUProjectionMatrix(proj, false);
				GL.LoadProjectionMatrix(shaderProjMatrix);


				CommandBuffer cmdBuf = new CommandBuffer();
				cmdBuf.SetProjectionMatrix(proj);
				Camera.main.AddCommandBuffer(CameraEvent.AfterDepthTexture, cmdBuf);
			}
		}

		public static void TRS(this Vector3 v, Vector3 move, Quaternion rotate, Vector3 scale)
		{
			v = Matrix4x4.TRS(move, rotate, scale).MultiplyVector(v);
		}

		public static void DrawScreenPointsToWorld(Matrix4x4 proj, Matrix4x4 toWorld, Vector3[] points, Color color, Vector3 rotate, Vector3 offset, Vector3 sign)
		{
			Vector3 posStart = toWorld.MultiplyPoint(Vector3.zero);
			Vector3 worldOffset = toWorld.MultiplyVector(offset);

			for (int i = 0; i < points.Length; ++i)
			{
				Vector3 localPos = proj.inverse.MultiplyPoint(points[i]);
				localPos.z *= -1f;
				Vector3 posEnd = toWorld.MultiplyPoint(localPos);

				Vector3 dir = (posEnd - posStart).normalized;
				//Vector3 sign = ModuleConfig.DebugConfig.RotateSign;
				dir = Quaternion.AngleAxis(rotate.x * sign.x, toWorld.MultiplyVector(Vector3.right)) * dir;
				dir = Quaternion.AngleAxis(rotate.y * sign.y, toWorld.MultiplyVector(Vector3.up)) * dir;
				dir = Quaternion.AngleAxis(rotate.z * sign.z, toWorld.MultiplyVector(Vector3.forward)) * dir;
				Vector3 start = posStart + worldOffset;

				GetNewLine(new Vector3[] { start, start + dir * 2f }, color);
			}
		}

		public static LineRenderer DrawRectangle(Matrix4x4 toWorld, Matrix4x4 proj, Vector2 scale, float depth, Color color, Vector3 rotate, Vector3 offset, Vector3 sign)
		{

			Vector3[] points = new Vector3[]
			{
			new Vector3(){x=-1f,y=1f,z=depth },
			new Vector3(){x=1f,y=1f,z=depth },
			new Vector3(){x=1f,y=-1f,z=depth },
			new Vector3(){x=-1f,y=-1f,z=depth },
			new Vector3(){x=-1f,y=1f,z=depth },
			new Vector3(){x=1f,y=-1f,z=depth },
			new Vector3(){x=1f,y=1f,z=depth },
			new Vector3(){x=-1f,y=-1f,z=depth },

			};

			Vector3 posStart = toWorld.MultiplyPoint(Vector3.zero);
			Vector3 worldOffset = toWorld.MultiplyVector(offset);
			//Vector3 sign = ModuleConfig.DebugConfig.RotateSign;
			for (int i = 0; i < points.Length; ++i)
			{
				Vector3 p = points[i];
				p.x *= scale.x;
				p.y *= scale.y;
				Vector3 localPos = proj.inverse.MultiplyPoint(p);
				localPos.z *= -1f;
				Vector3 posEnd = toWorld.MultiplyPoint(localPos);

				Vector3 dir = (posEnd - posStart).normalized;

				dir = Quaternion.AngleAxis(rotate.x * sign.x, toWorld.MultiplyVector(Vector3.right)) * dir;
				dir = Quaternion.AngleAxis(rotate.y * sign.y, toWorld.MultiplyVector(Vector3.up)) * dir;
				dir = Quaternion.AngleAxis(rotate.z * sign.z, toWorld.MultiplyVector(Vector3.forward)) * dir;

				points[i] = posStart + worldOffset + dir;

			}

			return GetNewLine(points, color);
		}


		/// <summary>
		/// 加入新的点
		/// </summary>
		/// <param name="line"></param>
		/// <param name="worldPoint"></param>
		/// <returns></returns>
		public static LineRenderer PushPoint(this LineRenderer line, Vector3 worldPoint)
		{
			line.positionCount = line.positionCount + 1;
			line.SetPosition(line.positionCount - 1, worldPoint);
			return line;
		}

		/// <summary>
		/// 加入新的点数组
		/// </summary>
		/// <param name="line"></param>
		/// <param name="worldPoints"></param>
		/// <returns></returns>
		public static LineRenderer PushPoints(this LineRenderer line, Vector3[] worldPoints)
		{
			Vector3[] orgPoints = new Vector3[line.positionCount];
			int orgNum = line.GetPositions(orgPoints);
			Vector3[] newPoints = new Vector3[orgNum + worldPoints.Length];
			Array.Copy(orgPoints, 0, newPoints, 0, orgNum);
			Array.Copy(worldPoints, 0, newPoints, orgNum, worldPoints.Length);
			line.SetPositions(newPoints);
			return line;
		}

		/// <summary>
		/// 成对绘制两个视椎体
		/// </summary>
		/// <param name="proj1"></param>
		/// <param name="proj2"></param>
		/// <param name="toW1"></param>
		/// <param name="toW2"></param>
		public static void DrawViewFramePair(Matrix4x4 proj1, Matrix4x4 proj2, Matrix4x4 toW1, Matrix4x4 toW2)
		{
			Vector3 center1, center2;
			DrawViewFrame(toW1, proj1, Color.red, out center1);
			LineRenderer line = DrawViewFrame(toW2, proj2, Color.blue, out center2);

			line.positionCount = line.positionCount + 1;
			line.SetPosition(line.positionCount - 1, center1);

			TextMesh text = new GameObject().AddComponent<TextMesh>();
			text.transform.position = center2;
			text.transform.localScale = Vector3.one * 0.005f;
			text.text = toW2.inverse.MultiplyVector(center1 - center2).ToString();

			float d1 = 1f, d2 = 2f, d3 = 10f;

			float depth1_1 = (proj1.inverse.MultiplyPoint(new Vector3(1f, 1f, d1)) - center1).magnitude;
			float depth1_2 = (proj1.inverse.MultiplyPoint(new Vector3(1f, 1f, d2)) - center1).magnitude;
			float depth1_3 = (proj1.inverse.MultiplyPoint(new Vector3(1f, 1f, d3)) - center1).magnitude;

			float depth2_1 = (proj2.inverse.MultiplyPoint(new Vector3(1f, 1f, d1)) - center2).magnitude;
			float depth2_2 = (proj2.inverse.MultiplyPoint(new Vector3(1f, 1f, d2)) - center2).magnitude;
			float depth2_3 = (proj2.inverse.MultiplyPoint(new Vector3(1f, 1f, d3)) - center2).magnitude;

			//string depthScale = new Vector3(depth1_1 / depth2_1, depth1_2 / depth2_2, depth1_3 / depth2_3).ToStr();
			//text.text += "\n DepthScale: " + depthScale;

			Vector3 up1 = toW1.MultiplyVector(Vector3.up);
			Vector3 fwd1 = toW1.MultiplyVector(Vector3.forward);
			//Vector3 right1 = toW1.MultiplyVector(Vector3.right);

			Vector3 up2 = toW2.MultiplyVector(Vector3.up);
			Vector3 fwd2 = toW2.MultiplyVector(Vector3.forward);
			//Vector3 right2 = toW2.MultiplyVector(Vector3.right);

			GameObject trans1 = new GameObject();
			GameObject trans2 = new GameObject();

			trans1.transform.rotation = Quaternion.LookRotation(fwd1, up1);
			trans2.transform.rotation = Quaternion.LookRotation(fwd2, up2);

			Vector3 eular1 = trans1.transform.eulerAngles;
			Vector3 eular2 = trans2.transform.eulerAngles;

			Vector3 deltaEular = eular1 - eular2;
			//text.text += "\n EularDelta:  " + deltaEular.ToStr("F8");//记录旋转偏移
		}


		/// <summary>
		/// 绘制Hololens和Unity相机的视椎体
		/// </summary>
		/// <param name="mainCam_proj"></param>
		/// <param name="mainCam_toWorld"></param>
		/// <param name="hole_proj"></param>
		/// <param name="holo_toWorld"></param>
		public static void DrawHoloeAndMainCam(Matrix4x4 mainCam_proj, Matrix4x4 mainCam_toWorld, Matrix4x4 hole_proj, Matrix4x4 holo_toWorld)
		{
			DrawViewFramePair(hole_proj, mainCam_proj, holo_toWorld, mainCam_toWorld);
		}


		/// <summary>
		/// 绘制相机的视椎体
		/// </summary>
		/// <param name="cam"></param>
		/// <param name="col"></param>
		/// <returns></returns>
		public static LineRenderer DrawCameraViewFrame(Camera cam, Color col)
		{
			return DrawViewFrame(cam.cameraToWorldMatrix, cam.projectionMatrix, Vector2.zero, col);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static LineRenderer DrawMainCamViewFrame(Color color)
		{
			return DrawViewFrame(Camera.main.cameraToWorldMatrix, Camera.main.projectionMatrix, Vector2.zero, color);
		}

		public static LineRenderer DrawViewFrame(Matrix4x4 cam2world, Matrix4x4 project, Color color, out Vector3 centerPos)
		{
			centerPos = cam2world.MultiplyPoint(Vector3.zero);
			return DrawViewFrame(cam2world, project, Vector2.zero, color);
		}






		public static LineRenderer DrawViewFrame(Matrix4x4 cam2world, Matrix4x4 project, Vector3 rotate, Vector3 offset, Vector2 screenOffset, Color color, LineRenderer line = null)
		{
			s_project = project;
			s_cam2world = cam2world;

			Vector2 ul = new Vector2(-1f, 1f) + screenOffset;//左上
			Vector2 ur = new Vector2(1f, 1f) + screenOffset;//右上
			Vector2 dl = new Vector2(-1f, -1f) + screenOffset;//左下
			Vector2 dr = new Vector2(1f, -1f) + screenOffset;//右下

			Vector2 dlm = new Vector2(-0.5f, -0.5f) + screenOffset;//左下中
			Vector2 drm = new Vector2(0.5f, -0.5f) + screenOffset;//右下中


			float distance = s_project.MultiplyPoint(Vector3.forward * far).z;
			//float near = 2f;
			//float far = 1f;

			Vector3 worldOffset = s_cam2world.MultiplyVector(offset);
			Vector3 camCenter = cam2world.MultiplyPoint(Vector3.zero);

			Vector3 dlm_2 = ToWorld(new Vector3(dlm.x, dlm.y, distance));
			Vector3 drm_2 = ToWorld(new Vector3(drm.x, drm.y, distance));

			//Vector3 ul_1 = ToWorld(new Vector3(ul.x, ul.y, near));
			Vector3 ul_2 = ToWorld(new Vector3(ul.x, ul.y, distance));

			//Vector3 ur_1 = ToWorld(new Vector3(ur.x, ur.y, near));
			Vector3 ur_2 = ToWorld(new Vector3(ur.x, ur.y, distance));

			//Vector3 dl_1 = ToWorld(new Vector3(dl.x, dl.y, near));
			Vector3 dl_2 = ToWorld(new Vector3(dl.x, dl.y, distance));

			//Vector3 dr_1 = ToWorld(new Vector3(dr.x, dr.y, near));
			Vector3 dr_2 = ToWorld(new Vector3(dr.x, dr.y, distance));


			dlm_2 = RotateAndOffset(s_cam2world, camCenter, dlm_2, rotate, worldOffset);
			drm_2 = RotateAndOffset(s_cam2world, camCenter, drm_2, rotate, worldOffset);

			ul_2 = RotateAndOffset(s_cam2world, camCenter, ul_2, rotate, worldOffset);

			ur_2 = RotateAndOffset(s_cam2world, camCenter, ur_2, rotate, worldOffset);

			dr_2 = RotateAndOffset(s_cam2world, camCenter, dr_2, rotate, worldOffset);

			dl_2 = RotateAndOffset(s_cam2world, camCenter, dl_2, rotate, worldOffset);

			camCenter += worldOffset;

			Vector3[] points = new Vector3[]
			{
			camCenter, ul_2, ur_2,
			camCenter, dl_2, dr_2,
			camCenter,dl_2,dr_2,ur_2,ul_2,dl_2,
			dlm_2,drm_2,dr_2,drm_2,ur_2
			};

			//BuiltinRenderTextureType
			//float height = (ul_2 - dl_2).magnitude;
			//float width = (ul_2 - ur_2).magnitude;

			//TextMesh text = new GameObject().AddComponent<TextMesh>();
			//text.transform.position = camCenter + Vector3.up * 0.01f;
			//text.transform.localScale = Vector3.one * 0.005f;
			//text.text = string.Format("[w:{0},h:{1},w/h:{2}]<r:{3}>", width.ToString("F8"), height.ToString("F8"), (width / height).ToString("F8"), cam2world.rotation.ToStr("F8"));
			//if (fovPrefab == null) fovPrefab = Resources.Load<GameObject>("Prefab/Fov_Prefab");
			//GameObject fov = GameObject.Instantiate(fovPrefab);
			//fov.transform.position = camCenter;
			//fov.transform.rotation = cam2world.rotation;
			if (line == null)
				return GetNewLine(points, color);
			else
			{
				line.positionCount = points.Length;
				line.SetPositions(points);
				line.material.color = color;
				line.startWidth = line.endWidth = 0.01f;
				return line;
			}
		}

		public static Vector3 RotateAndOffset(Matrix4x4 toWorld, Vector3 center, Vector3 end, Vector3 rotate, Vector3 worldOffset)
		{
			Vector3 orgDir = end - center;
			orgDir = Quaternion.AngleAxis(rotate.x, toWorld.MultiplyVector(Vector3.right)) * orgDir;
			orgDir = Quaternion.AngleAxis(rotate.y, toWorld.MultiplyVector(Vector3.up)) * orgDir;
			orgDir = Quaternion.AngleAxis(rotate.z, toWorld.MultiplyVector(Vector3.forward)) * orgDir;

			return center + worldOffset + orgDir;
		}


		public static LineRenderer DrawViewFrame(Matrix4x4 cam2world, Matrix4x4 project, Vector2 screenOffset, Color color, LineRenderer line = null)
		{
			return DrawViewFrame(cam2world, project, Vector3.zero, Vector3.zero, screenOffset, color, line);
		}


		/// <summary>
		/// 绘制相机的视椎体
		/// </summary>
		/// <param name="color"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		public static LineRenderer DrawCameraViewFrameWitchOneLine(Color color, LineRenderer line)
		{
			Matrix4x4 cam2world = Camera.main.cameraToWorldMatrix;
			Matrix4x4 project = Camera.main.projectionMatrix;
			if (line == null) line = GetEmptyLine();
			return DrawViewFrame(cam2world, project, Vector3.zero, Vector3.zero, Vector3.zero, color, line);
		}

		private static Vector3 ToWorld(Vector3 screenPos)
		{
			Vector3 localPos = s_project.inverse.MultiplyPoint(screenPos);
			localPos.z *= -1f;
			Vector3 worldPos = s_cam2world.MultiplyPoint(localPos);
			return worldPos;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="points"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public static LineRenderer GetNewLine(Vector3[] points, Color color, float width = 0.001f)
		{
			LineRenderer line = new GameObject().AddComponent<LineRenderer>();
			line.useWorldSpace = true;
			line.positionCount = points.Length;
			line.SetPositions(points);
			line.startWidth = line.endWidth = width;
			line.material = new Material(Shader.Find("GUI/Text Shader"));
			line.material.SetColor("_Color", color);
			return line;
		}


		public static LineRenderer GetEmptyLine()
		{
			LineRenderer line = new GameObject().AddComponent<LineRenderer>();
			line.useWorldSpace = true;
			line.shadowCastingMode = ShadowCastingMode.Off;
			line.receiveShadows = false;
			line.lightProbeUsage = LightProbeUsage.Off;
			line.startWidth = line.endWidth = 0.001f;
			line.material = new Material(Shader.Find("GUI/Text Shader"));

			return line;
		}

		public static LineRenderer SetupLine(this LineRenderer line, Vector3[] points, Color color, float width = 0.001f)
		{
			line.useWorldSpace = true;
			line.positionCount = points.Length;
			line.SetPositions(points);
			line.startWidth = line.endWidth = width;
			line.material = new Material(Shader.Find("GUI/Text Shader"));
			line.material.SetColor("_Color", color);
			return line;
		}













		/// <summary>
		/// 绘制坐标轴
		/// </summary>
		/// <param name="toWorldMatrix"></param>
		public static void DrawAxis(Matrix4x4 toWorldMatrix)
		{
			Vector3 start = toWorldMatrix.MultiplyPoint(Vector3.zero);
			DrawAxisInPos(toWorldMatrix, start);
		}

		/// <summary>
		/// 在制定位置绘制坐标轴
		/// </summary>
		/// <param name="toWorldMatrix"></param>
		/// <param name="center"></param>
		public static void DrawAxisInPos(Matrix4x4 toWorldMatrix, Vector3 center)
		{
			Vector3 start = center;
			Vector3 fwdEnd = start - toWorldMatrix.MultiplyVector(Vector3.forward).normalized * 0.025f;
			Vector3 upEnd = start + toWorldMatrix.MultiplyVector(Vector3.up).normalized * 0.025f;
			Vector3 rightEnd = start + toWorldMatrix.MultiplyVector(Vector3.right).normalized * 0.025f;

			GetNewLine(new Vector3[] { start, fwdEnd }, Color.blue);
			GetNewLine(new Vector3[] { start, upEnd }, Color.green);
			GetNewLine(new Vector3[] { start, rightEnd }, Color.red);
		}









		/// <summary>
		/// 对整个视角进行缩放和旋转（加入Hololens 虚拟相机的旋转和缩放）
		/// </summary>
		/// <param name="holo_cam2world"></param>
		/// <param name="unity_cam2world"></param>
		/// <param name="camLocalPoint"></param>
		/// <returns></returns>
		public static Vector3 GetHoloSpaceWorldPoint(Matrix4x4 unity_cam2world, Vector3 camLocalPoint, Vector3 holoScale)
		{
			//计算位置
			Vector3 pos = unity_cam2world.MultiplyPoint(Vector3.zero);//
																	  //计算朝向
			Vector3 right = unity_cam2world.MultiplyVector(Vector3.right);
			Vector3 fwd = Quaternion.AngleAxis(0.78f, right) * unity_cam2world.MultiplyVector(Vector3.forward);
			Vector3 up = unity_cam2world.MultiplyVector(Vector3.up);
			Quaternion roation = Quaternion.LookRotation(fwd, up);
			//计算一个转换矩阵，计算位置点
			Matrix4x4 matrix = new Matrix4x4();
			matrix.SetTRS(pos, roation, holoScale);
			return matrix.MultiplyPoint(camLocalPoint);
		}

	}


}