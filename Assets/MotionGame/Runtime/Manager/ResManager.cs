﻿using System;
using MotionEngine;
using MotionEngine.Res;
using MotionEngine.Debug;
using UnityEngine;

namespace MotionGame
{
	/// <summary>
	/// 资源管理器
	/// </summary>
	public sealed class ResManager : IModule
	{
		public static readonly ResManager Instance = new ResManager();

		private ResManager()
		{
		}
		public void Awake()
		{
		}
		public void Start()
		{
		}
		public void Update()
		{
			AssetSystem.UpdatePoll();
		}
		public void LateUpdate()
		{
			AssetSystem.Release();
		}
		public void OnGUI()
		{
			int totalCount = AssetSystem.GetFileLoaderCount();
			int failedCount = AssetSystem.GetFileLoaderFailedCount();
			DebugConsole.GUILable($"[{nameof(ResManager)}] AssetLoadMode : {AssetSystem.AssetLoadMode}");
			DebugConsole.GUILable($"[{nameof(ResManager)}] Asset loader total count : {totalCount}");
			DebugConsole.GUILable($"[{nameof(ResManager)}] Asset loader failed count : {failedCount}");
		}

		/// <summary>
		/// 同步加载接口
		/// 注意：仅支持特殊的无依赖资源
		/// </summary>
		public T SyncLoad<T>(string resName) where T : UnityEngine.Object
		{
			UnityEngine.Object result = null;

			if (AssetSystem.AssetLoadMode == EAssetLoadMode.EditorMode)
			{
#if UNITY_EDITOR
				string loadPath = AssetSystem.GetDatabaseAssetPath(resName);
				result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(loadPath);
#else
				throw new Exception("AssetDatabaseLoader only support unity editor.");
#endif
			}
			else if (AssetSystem.AssetLoadMode == EAssetLoadMode.ResourceMode)
			{
				result = Resources.Load<T>(resName);
			}
			else if (AssetSystem.AssetLoadMode == EAssetLoadMode.BundleMode)
			{
				string fileName = System.IO.Path.GetFileNameWithoutExtension(resName);
				string manifestPath = AssetPathHelper.ConvertResourcePathToManifestPath(resName);
				string loadPath = AssetSystem.BundleMethod.GetAssetBundleLoadPath(manifestPath);
				AssetBundle bundle = AssetBundle.LoadFromFile(loadPath);
				result = bundle.LoadAsset<T>(fileName);
				bundle.Unload(false);
			}
			else
			{
				throw new NotImplementedException($"{AssetSystem.AssetLoadMode}");
			}

			return result as T;
		}
	}
}