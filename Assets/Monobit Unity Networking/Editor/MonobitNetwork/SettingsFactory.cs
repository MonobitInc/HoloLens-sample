using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;

namespace MonobitEngine.Editor
{
	/**
	 * サーバー設定ファイルの作成クラス
	 */
	public class SettingsFactory : UnityEngine.MonoBehaviour
	{
		/**
		 * コンストラクタ
		 */
		public SettingsFactory()
		{
		}

		/**
		 * MonobitServerSettingsの作成メニュー
		 */
		[MenuItem("Assets/Create/MonobitServerSettings")]
		public static void OnCreateMonobitServerSettings()
		{
            MonobitBridge.Initialize();
		}
	}
}
