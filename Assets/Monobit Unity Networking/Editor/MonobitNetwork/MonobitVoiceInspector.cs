using System;
using UnityEngine;
using UnityEditor;
using MonobitEngine.VoiceChat;

namespace MonobitEngine.Editor
{
	[CustomEditor(typeof(MonobitVoice), true)]
	[ExecuteInEditMode]
	public class MonobitVoiceInspector : UnityEditor.Editor
	{
		/** MonobitVoice オブジェクト. */
		private MonobitVoice m_Voice = null;

		/** MonobitViewが保持するゲームオブジェクト. */
		private bool m_bVADFoldout = true;

		/** MonobitViewが保持するゲームオブジェクト. */
		private bool m_bOpusCodecFoldout = true;

		/** サンプリングレートのプリセット用文字列. */
		private static readonly string[] m_presetTypes = { "48000Hz", "24000Hz", "16000Hz", "12000Hz", "8000Hz" };

		/**
		 * @brief	Inspector に追加されたときの処理.
		 */
		public void Awake()
		{
			// MonobitVoiceの取得
			m_Voice = target as MonobitVoice;
			if (m_Voice == null)
			{
				UnityEngine.Debug.LogErrorFormat("Voice object is null.");
				return;
			}
		}

		/**
		 * @brief	Inspector から削除されたときの処理.
		 */
		public void OnDestroy()
		{
			m_Voice = target as MonobitVoice;
			if (m_Voice != null)
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
			m_Voice = target as MonobitVoice;
			if (m_Voice == null)
			{
				return;
			}

			// Version
			EditorGUILayout.BeginHorizontal();
			GUI.enabled = false;
			EditorGUILayout.TextField("MonobitVoice Version", string.Format("0x{0:x8}",m_Voice.Version), EditorStyles.boldLabel);
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

			// SendStreamType
			m_Voice.SendStreamType = (StreamType)EditorGUILayout.EnumPopup("Send Stream Type", m_Voice.SendStreamType);

			// ReliableMode
			m_Voice.ReliableMode = EditorGUILayout.Toggle("Reliable Mode", m_Voice.ReliableMode);

            // Encrypt
            m_Voice.Encrypt = EditorGUILayout.Toggle("Encrypt", m_Voice.Encrypt);

            // Surround 3D
            m_Voice.Surround3D = EditorGUILayout.Toggle("Surround 3D", m_Voice.Surround3D);

			// Call Bps Callback
			m_Voice.ShowVoiceDataBps = EditorGUILayout.Toggle("Call bps callback", m_Voice.ShowVoiceDataBps);

			// PlaybackVoice Local Check
			m_Voice.PlaybackVoiceLocalNoCheck = EditorGUILayout.Toggle("Playback Voice Local NoCheck", m_Voice.PlaybackVoiceLocalNoCheck);

			// Debug Mode
			m_Voice.DebugMode = EditorGUILayout.Toggle("Debug Mode", m_Voice.DebugMode);

			// VAD
			m_bVADFoldout = EditorGUILayout.Foldout(m_bVADFoldout, "Voice Activity Detector (VAD)");
			if (m_bVADFoldout)
			{
				EditorGUI.indentLevel = 1;
				SetupVAD();
				EditorGUI.indentLevel = 0;
			}

			// Opus Codecの設定
			m_bOpusCodecFoldout = EditorGUILayout.Foldout(m_bOpusCodecFoldout, "Opus Codec Settings");
			if (m_bOpusCodecFoldout)
			{
				EditorGUI.indentLevel = 1;
				SetupOpusCodec();
				EditorGUI.indentLevel = 0;
			}

			// セーブ
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(m_Voice);
                MonobitEditor.MarkSceneDirty();
			}
		}

		/**
		 * @brief	Inspector上のGUI表示.
		 */
		private void SetupVAD()
		{
			// VADの有効・無効化
			m_Voice.VAD = EditorGUILayout.Toggle("VAD Enabled", m_Voice.VAD);

			// VADが有効な時のみ無音検知閾値を設定できるようにする
			GUI.enabled = m_Voice.VAD;
			m_Voice.TalkingThreshold = EditorGUILayout.IntField("Talking Threshold", m_Voice.TalkingThreshold);
			m_Voice.VADLatitude = EditorGUILayout.IntSlider("VAD Latitude", m_Voice.VADLatitude, 1, 10);
			GUI.enabled = true;
		}

		/**
		 * @brief	Inspector上のGUI表示.
		 */
		private void SetupOpusCodec()
		{
			// アプリケーション設定
			m_Voice.Application = (VoiceChat.Codec.Opus.Application)EditorGUILayout.EnumPopup("Application", m_Voice.Application);

			// シグナル設定
			m_Voice.OpusSignal = (VoiceChat.Codec.Opus.OpusSignal)EditorGUILayout.EnumPopup("Signal", m_Voice.OpusSignal);

			// エンコードモード
			m_Voice.EncodeMode = (VoiceChat.Codec.Opus.EncodeMode)EditorGUILayout.EnumPopup("Encode Mode", m_Voice.EncodeMode);

			// 圧縮ビットレートのプリセット
			m_Voice.CompressedBitRatePreset = (VoiceChat.Codec.Opus.CompressedBitRatePreset)EditorGUILayout.EnumPopup("Compressed Bit Rate (Preset)", m_Voice.CompressedBitRatePreset);
			if (m_Voice.CompressedBitRatePreset == VoiceChat.Codec.Opus.CompressedBitRatePreset.Free)
			{
				GUI.enabled = true;
				m_Voice.CompressedBitRate = EditorGUILayout.IntField("Compressed Bit Rate (bps)", m_Voice.CompressedBitRate);
			}
			else
			{
				GUI.enabled = false;
				int compressedBitRate = m_Voice.OpusCodec.GetCompressedBitRate(m_Voice.CompressedBitRatePreset);
				m_Voice.CompressedBitRate = EditorGUILayout.IntField("Compressed Bit Rate (bps)", compressedBitRate);
				GUI.enabled = true;
			}

			// 帯域の設定
			m_Voice.OpusBandwidth = (VoiceChat.Codec.Opus.OpusBandwidth)EditorGUILayout.EnumPopup("Band Width", m_Voice.OpusBandwidth);

			// サンプリングレートのプリセット
			m_Voice.SampligRatePreset = (VoiceChat.Codec.Opus.SampligRatePreset)EditorGUILayout.EnumPopup("Sampling Rate (Preset)", m_Voice.SampligRatePreset);
			if (m_Voice.SampligRatePreset == VoiceChat.Codec.Opus.SampligRatePreset.Free)
			{
				GUI.enabled = true;
				m_Voice.EncodeSamplingRate = EditorGUILayout.IntField("Encode Sampling Rate (Hz)", m_Voice.EncodeSamplingRate);
				m_Voice.DecodeSamplingRatePreset = (VoiceChat.Codec.Opus.DecodeSamplingRatePreset)EditorGUILayout.Popup("Decode Sampling Rate (Hz)", (int)m_Voice.DecodeSamplingRatePreset, m_presetTypes);
			}
			else
			{
				GUI.enabled = false;
				var info = m_Voice.OpusCodec.GetSamplingRate(m_Voice.SampligRatePreset);
				m_Voice.EncodeSamplingRate = EditorGUILayout.IntField("Encode Sampling Rate (Hz)", info.encodeSamplingRate);
				m_Voice.DecodeSamplingRatePreset = (VoiceChat.Codec.Opus.DecodeSamplingRatePreset)EditorGUILayout.Popup("Decode Sampling Rate (Hz)", (int)info.decodeSamplingRate, m_presetTypes);
				GUI.enabled = true;
			}

			// エンコード品質
			m_Voice.Complexity = EditorGUILayout.IntSlider("Encode Complexity (Low <==> High)", m_Voice.Complexity, 0, 10);

			// frame size
			m_Voice.FrameSizeMs = (FrameSizeMs)EditorGUILayout.EnumPopup("Frame Size (ms)", m_Voice.FrameSizeMs);

			// デフォルト設定ボタン
			if (GUILayout.Button("Default Codec Settings", GUILayout.Width(150)))
			{
				SetDefaultCodecSettings();
			}
		}

		/**
		 * @brief	Inspector上のGUI表示.
		 */
		private void SetDefaultCodecSettings()
		{
			// アプリケーション設定
			m_Voice.Application = VoiceChat.Codec.Opus.Application.VoIP;
			// シグナル設定
			m_Voice.OpusSignal = VoiceChat.Codec.Opus.OpusSignal.Voice;
			// エンコードモード
			m_Voice.EncodeMode = VoiceChat.Codec.Opus.EncodeMode.VBR;
			// 圧縮ビットレートのプリセット
			m_Voice.CompressedBitRatePreset = VoiceChat.Codec.Opus.CompressedBitRatePreset.VoipLow;
			// 圧縮ビットレート
			m_Voice.CompressedBitRate = 32000;
			// 帯域幅
			m_Voice.OpusBandwidth = VoiceChat.Codec.Opus.OpusBandwidth.WideBand;
			// サンプリングレートのプリセット
			m_Voice.SampligRatePreset = VoiceChat.Codec.Opus.SampligRatePreset.Medium;
			// 品質
			m_Voice.Complexity = 1;
			// フレームサイズ
			m_Voice.FrameSizeMs = FrameSizeMs.FrameSize40ms;
		}
	}
}
