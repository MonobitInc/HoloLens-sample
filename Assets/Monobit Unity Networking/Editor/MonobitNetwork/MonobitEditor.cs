using System;
using UnityEngine;
using UnityEditor;
using MonobitEngine.Definitions;

namespace MonobitEngine.Editor
{
	[InitializeOnLoad]
	public class MonobitEditor : EditorWindow
	{
		/**
		 * ロード時に一度だけ呼び出される
		 */
		static MonobitEditor()
		{
            // プラットフォーム依存設定をリセット
            RedistMUN.SettingPlatform();
        }

        /**
		 * Windowごと
		 */
        public MonobitEditor()
		{

		}

		/**
		 * 
		 */
		[MenuItem("Window/Monobit Unity Networking/Pick Up Settings %#&m", false, 1)]
		protected static void OnMenuItemHighlightServerSettings()
		{
			DoHighlightServerSettings();
		}

		/**
		 * MUNで内部的に生成している秘匿オブジェクトのクリーンアップ.
		 */
		[MenuItem("Window/Monobit Unity Networking/Cleanup MUN internal HideInHierarchy object %#&c", false, 2)]
		static void CleanupMunHideInHierarchyObject()
		{
			MonobitNetwork.CleanupMunHideInHierarchyObject();
		}

        /**
         * @brief   MUN関連ライブラリとして頒布しているライブラリ一式について、プラットフォーム依存設定をリセットする.
         */
        [MenuItem("Window/Monobit Unity Networking/Reimport Redistribution Library %#&m", false, 3)]
        static void ReimportRedistributionLibrary()
        {
            RedistMUN.SettingPlatform();
        }

        /**
		 * 
		 */
        protected static void DoHighlightServerSettings()
		{
			Selection.objects = new UnityEngine.Object[] { MonobitNetworkSettings.MonobitServerSettings };
			EditorGUIUtility.PingObject(MonobitNetworkSettings.MonobitServerSettings);
		}
		
		/**
		 * @brief   現在開いているシーンの変更フラグを立てる
		 */
		public static void MarkSceneDirty()
		{
			if ( EditorApplication.isPlaying ) return;
#if UNITY_5_3_OR_NEWER || UNITY_5_3
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEngine.SceneManagement.SceneManager.GetActiveScene() );
#else
			UnityEditor.EditorApplication.MarkSceneDirty();
#endif
		}
    }
}
