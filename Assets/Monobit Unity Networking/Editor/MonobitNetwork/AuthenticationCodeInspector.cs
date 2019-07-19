using System;
using UnityEngine;
using UnityEditor;

namespace MonobitEngine.Editor
{
    /**
     * AuthenticationCode のインスペクタ表示用クラス.
     */
    [CustomEditor(typeof(AuthenticationCode))]
    public class AuthenticationCodeInspector : UnityEditor.Editor
    {
		/** AuthenticationCode 本体. */
		private AuthenticationCode m_View = null;

		/**
         * Inspector上のGUI表示.
         */
		public override void OnInspectorGUI()
        {
			// 変数の初期化
			m_View = this.target as AuthenticationCode;
			if (m_View == null)
			{
				return;
			}

			// セーブデータの表示
			EditorGUILayout.LabelField("Save Data", EditorStyles.boldLabel);
			EditorGUI.indentLevel = 2;
			EditorGUILayout.LabelField("Authentication Code", MonobitNetworkSettings.Decrypt(m_View.saveAuthID));
			EditorGUI.indentLevel = 0;
		}
	}
}