using System;
using UnityEngine;
using UnityEditor;
using MonobitEngine.VoiceChat;

namespace MonobitEngine.Editor
{
	[CustomEditor(typeof(MonobitStreamingPlayer), true)]
	[ExecuteInEditMode]
	public class MonobitStreamingPlayerEditor : UnityEditor.Editor
	{
		/// <summary>
		/// 
		/// </summary>
		private MonobitStreamingPlayer m_Player = null;

		/**
		 * @brief	Inspector に追加されたときの処理.
		 */
		public void Awake()
		{
			m_Player = target as MonobitStreamingPlayer;
			if (m_Player == null)
			{
				UnityEngine.Debug.LogErrorFormat("Player object is null.");
				return;
			}
		}

		/**
		 * @brief	Inspector から削除されたときの処理.
		 */
		public void OnDestroy()
		{
			m_Player = target as MonobitStreamingPlayer;
			if (m_Player != null)
			{
				return;
			}
		}

		/**
		 * @brief	Inspector上のGUI表示.
		 */
		public override void OnInspectorGUI()
		{
			// 変数取得
			m_Player = target as MonobitStreamingPlayer;
			if (m_Player == null)
			{
				return;
			}

			// ゲイン調整
			m_Player.CurrentGainDecibel = EditorGUILayout.IntSlider("Current Gain (dB)", m_Player.CurrentGainDecibel, -96, 96);

			// バッファリング数
			m_Player.BufferCount = EditorGUILayout.IntSlider("Buffer Count", m_Player.BufferCount, 0, 500);

			// 残バッファ数
			Rect rect = GUILayoutUtility.GetRect(20, 20);
			float remain_buffer = 0f;
			string remain_string = null;
			if (EditorApplication.isPlaying)
			{
				remain_buffer = m_Player.RemainTime > 1 ? 100f : (float)m_Player.RemainTime;
				remain_string = String.Format("Remain Buffer ({0:0.000}%)", remain_buffer);
			}
			else
			{
				remain_buffer = 0f;
				remain_string = "Remain Buffer (0.000%)";
			}
			EditorGUI.ProgressBar(rect, remain_buffer, remain_string);

			// 許容タイムラグ
			m_Player.AllowLagMs = EditorGUILayout.IntSlider("Allow time lag (ms)", m_Player.AllowLagMs, 100, 10000);

			// クリップ再生バッファ
			m_Player.ClipPlaybackSec = EditorGUILayout.IntSlider("Playback buffer (sec)", m_Player.ClipPlaybackSec, 3, 10);

			// セーブ
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(m_Player);
                MonobitEditor.MarkSceneDirty();
			}
		}
	}
}
