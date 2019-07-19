using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using MonobitEngine.Definitions;

namespace MonobitEngine.Editor
{
    /**
     * ServerSettings のインスペクタ表示用クラス.
     */
    [CustomEditor(typeof(ServerSettings))]
    public class ServerSettingsInspector : UnityEditor.Editor
    {
        /** ServerSettins 本体. */
        ServerSettings m_Settings = null;

        /** 初期化したかどうかのフラグ. */
        bool m_Initialize = false;

        /**
         * Inspector上のGUI表示.
         */
        public override void OnInspectorGUI()
        {
#if UNITY_IPHONE
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
            m_Settings = target as ServerSettings;
            if (m_Settings == null) return;

            // 初期設定
            Init();

            // アプリケーション設定
            ApplicationSettings();

            // サーバ設定
            ServerSettings();

            // 認証サーバ設定
            CustomAuthServerSettings();

            // 送受信バイト数上限設定
            MaxLimitTrafficBytesSettings();

            // 時間設定
            TimeSettings();
            
            // バージョン表示
            Versions();

            // データの更新
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(m_Settings);
            }
        }

        /**
         * 初期設定.
         */
        private void Init()
        {
            if (!m_Initialize)
            {
                m_Initialize = true;
                AuthenticationCode m_SaveData = AssetDatabase.LoadAssetAtPath(MonobitNetworkSettings.GetAuthCodeSettingsPath(), (typeof(AuthenticationCode))) as AuthenticationCode;
                if (m_SaveData != null)
                {
                    MonobitNetworkSettings.MonobitServerSettings.AuthID = MonobitNetworkSettings.Decrypt(m_SaveData.saveAuthID);
                }
                AssetDatabase.Refresh();
            }
        }

        /**
         * アプリケーション設定.
         */
        private void ApplicationSettings()
        {
            // 標題の表示
            EditorGUILayout.LabelField("Application Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel = 2;
            GUILayout.Space(5);

            // 認証コードの表示
            if (m_Settings.AuthID == "") SaveAuthID();
            EditorGUILayout.LabelField("Authentication Code", m_Settings.AuthID);
            
            // ボタン表示）
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Code"))
            {
                SaveAuthID();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = 0;
            GUILayout.Space(5);
        }

        private void SaveAuthID()
        {
            m_Settings.UpdateAuthID();
            AssetDatabase.DeleteAsset(MonobitNetworkSettings.GetAuthCodeSettingsPath());
            AuthenticationCode m_SaveData = ScriptableObject.CreateInstance<AuthenticationCode>();
            m_SaveData.saveAuthID = MonobitNetworkSettings.Encrypt(m_Settings.AuthID);
            EditorUtility.SetDirty(m_SaveData);
            AssetDatabase.CreateAsset(m_SaveData, MonobitNetworkSettings.GetAuthCodeSettingsPath());
            AssetDatabase.SaveAssets();
        }

        /**
         * サーバ設定.
         */
        private void ServerSettings()
        {
            // 標題の表示
            EditorGUILayout.LabelField("Server Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel = 2;
            GUILayout.Space(5);

            GUI.enabled = !EditorApplication.isPlaying;

            // ホストタイプの設定
            m_Settings.HostType = (ServerSettings.MunHostingOption)EditorGUILayout.EnumPopup("Host Type", m_Settings.HostType);

            // ポート番号＆アドレスの設定
            switch (m_Settings.HostType)
            {
                case MonobitEngine.ServerSettings.MunHostingOption.None:
                    {
                    }
                    break;
                case MonobitEngine.ServerSettings.MunHostingOption.MunTestServer:
                    {
                        // 接続プロトコルを選択する
                        m_Settings.HostProtocol = (ConnectionProtocol)EditorGUILayout.EnumPopup("Protocol", m_Settings.HostProtocol);
                    }
                    break;
                case MonobitEngine.ServerSettings.MunHostingOption.SelfServer:
                    {
                        // 接続プロトコルを選択する
                        m_Settings.HostProtocol = (ConnectionProtocol)EditorGUILayout.EnumPopup("Protocol", m_Settings.HostProtocol);

                        // アドレス・ポート番号ともに自分で設定する
                        m_Settings.SelfServerAddress = EditorGUILayout.TextField("IP Address", m_Settings.SelfServerAddress).Trim();
                        m_Settings.SelfServerPortString = EditorGUILayout.TextField("Port", m_Settings.SelfServerPortString).Trim();
                    }
                    break;
                case MonobitEngine.ServerSettings.MunHostingOption.OfflineMode:
                    {
                    }
                    break;
				case MonobitEngine.ServerSettings.MunHostingOption.MBECloud:
					{
						// 接続プロトコルを選択する
						m_Settings.HostProtocol = (ConnectionProtocol)EditorGUILayout.EnumPopup("Protocol", m_Settings.HostProtocol);

						// アドレス・ポート番号ともに自分で設定する
						m_Settings.MunCloudEndpointAddress = EditorGUILayout.TextField("Endpoint Address", m_Settings.MunCloudEndpointAddress).Trim();
						m_Settings.MunCloudAppID = EditorGUILayout.TextField("AppID", m_Settings.MunCloudAppID).Trim();                        
                        m_Settings.MunCloudConnectionTimeOut = uint.Parse(EditorGUILayout.TextField("MBE Cloud Connection TimeOut (msec)"
                                                                    , m_Settings.MunCloudConnectionTimeOut.ToString()).Trim());
                    }
                    break;
			}

			GUI.enabled = true;

            EditorGUI.indentLevel = 0;
            GUILayout.Space(5);
        }

        /**
         * カスタム認証サーバ設定.
         */
        private void CustomAuthServerSettings()
        {
            // 標題の表示
            EditorGUILayout.LabelField("Custom Authentication Server Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel = 2;
            GUILayout.Space(5);

            GUI.enabled = !EditorApplication.isPlaying;

            // カスタム認証サーバの利用タイプの設定
            m_Settings.CustomAuthType = (ServerSettings.CustomAuthenticationType)EditorGUILayout.EnumPopup("Type", m_Settings.CustomAuthType);

            // アドレスの設定
            switch (m_Settings.CustomAuthType)
            {
                case MonobitEngine.ServerSettings.CustomAuthenticationType.None:
                    {
                        // 設定しない
                    }
                    break;
                case MonobitEngine.ServerSettings.CustomAuthenticationType.WebServer_AppointClient:
                    {
                        // アドレスを自分で設定する
                        m_Settings.CustomAuthServerAddress = EditorGUILayout.TextField("Address", m_Settings.CustomAuthServerAddress).Trim();

                        // エラーを無視するかどうかのフラグの設定
                        m_Settings.CustomAuth_IgnoreError = EditorGUILayout.ToggleLeft(" Ignore AuthServer Error", m_Settings.CustomAuth_IgnoreError);
                    }
                    break;
                case MonobitEngine.ServerSettings.CustomAuthenticationType.WebServer_AppointServer:
                    {
                        // エラーを無視するかどうかのフラグの設定
                        m_Settings.CustomAuth_IgnoreError = EditorGUILayout.ToggleLeft(" Ignore AuthServer Error", m_Settings.CustomAuth_IgnoreError);
                    }
                    break;
            }

            GUI.enabled = true;

            EditorGUI.indentLevel = 0;
            GUILayout.Space(5);
        }

        /**
         * 送受信上限バイト数設定.
         */
        private void MaxLimitTrafficBytesSettings()
        {
            EditorGUILayout.LabelField("Max-Limit Traffic Bytes Settings (Bytes/Frame)", EditorStyles.boldLabel);

            EditorGUI.indentLevel = 2;
            GUILayout.Space(5);
            GUI.enabled = !EditorApplication.isPlaying;

            string oldObjectStreamSendLimitBytesPerFrame = m_Settings.ObjectStreamSendLimitBytesPerFrame.ToString();
            string newObjectStreamSendLimitBytesPerFrame = EditorGUILayout.TextField("Send (Object Stream)", oldObjectStreamSendLimitBytesPerFrame);
            if (oldObjectStreamSendLimitBytesPerFrame != newObjectStreamSendLimitBytesPerFrame)
            {
                try
                {
                    int value = int.Parse(newObjectStreamSendLimitBytesPerFrame);
                    if (value >= 0)
                    {
                        m_Settings.ObjectStreamSendLimitBytesPerFrame = (uint)value;
                    }
                    else
                    {
                        Debug.LogError(String.Format("OverflowException : {0} is between {1} and {2}", newObjectStreamSendLimitBytesPerFrame, 0, int.MaxValue));
                    }
                }
                catch (OverflowException)
                {
                    Debug.LogError(String.Format("OverflowException : {0} is between {1} and {2}", newObjectStreamSendLimitBytesPerFrame, 0, int.MaxValue));
                }
            }

            string oldRpcSendLimitBytesPerFrame = m_Settings.RpcSendLimitBytesPerFrame.ToString();
            string newRpcSendLimitBytesPerFrame = EditorGUILayout.TextField("Send (RPC)", oldRpcSendLimitBytesPerFrame);
            if (oldRpcSendLimitBytesPerFrame != newRpcSendLimitBytesPerFrame)
            {
                try
                {
                    int value = int.Parse(newRpcSendLimitBytesPerFrame);
                    if (value >= 0)
                    {
                        m_Settings.RpcSendLimitBytesPerFrame = (uint)value;
                    }
                    else
                    {
                        Debug.LogError(String.Format("OverflowException : {0} is between {1} and {2}", newRpcSendLimitBytesPerFrame, 0, int.MaxValue));
                    }
                }
                catch (OverflowException)
                {
                    Debug.LogError(String.Format("OverflowException : {0} is between {1} and {2}", newRpcSendLimitBytesPerFrame, 0, int.MaxValue));
                }
            }

            string oldReceiveLimitBytesPerFrame = m_Settings.ReceiveLimitBytesPerFrame.ToString();
            string newReceiveLimitBytesPerFrame = EditorGUILayout.TextField("Receive (All)", oldReceiveLimitBytesPerFrame);
            if (oldReceiveLimitBytesPerFrame != newReceiveLimitBytesPerFrame)
            {
                try
                {
                    m_Settings.ReceiveLimitBytesPerFrame = uint.Parse(newReceiveLimitBytesPerFrame);
                }
                catch (OverflowException)
                {
                    Debug.LogError(String.Format("OverflowException : {0} is between {1} and {2}", newReceiveLimitBytesPerFrame, uint.MinValue, uint.MaxValue));
                }
            }

            GUI.enabled = true;
            EditorGUI.indentLevel = 0;
            GUILayout.Space(5);
        }

        /**
         * 時間設定
         */
        private void TimeSettings()
        {
            EditorGUILayout.LabelField("Time Settings (ms)", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel = 2;
            GUILayout.Space(5);
            GUI.enabled = ! EditorApplication.isPlaying;
            
            string oldServerConnectWaitTime = m_Settings.ServerConnectWaitTime.ToString();
            string newServerConnectWaitTime = EditorGUILayout.TextField( "Server Connect", oldServerConnectWaitTime );
            if ( oldServerConnectWaitTime != newServerConnectWaitTime ){
                try{
                    m_Settings.ServerConnectWaitTime = uint.Parse( newServerConnectWaitTime );
                }catch{}
            }
            
            string oldKeepAliveUpdateTime = m_Settings.KeepAliveUpdateTime.ToString();
            string newKeepAliveUpdateTime = EditorGUILayout.TextField( "Keep Alive", oldKeepAliveUpdateTime );
            if ( oldKeepAliveUpdateTime != newKeepAliveUpdateTime ){
                try{
                    m_Settings.KeepAliveUpdateTime = uint.Parse( newKeepAliveUpdateTime );
                }catch{}
            }
            
            GUI.enabled = true;
            EditorGUI.indentLevel = 0;
            GUILayout.Space(5);
        }
        
        /**
         * バージョン表示
         */
        private void Versions(){
            EditorGUILayout.LabelField("Versions", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel = 2;
            GUILayout.Space(5);
            
            UInt32 hard_limit = Mrs.mrs_get_connection_num_hard_limit();
            EditorGUILayout.LabelField( "Hard Limit", String.Format( "{0}(0x{1:X})", hard_limit, hard_limit ) );
            
            UInt32 mrs_version = MunVersion.GetVersion( MunVersion.MRS_VERSION_KEY );
            EditorGUILayout.LabelField( "Mrs", String.Format( "0x{0:X}", mrs_version ) );
            
            UInt32 mun_version = MunVersion.GetVersion( MunVersion.MUN_VERSION_KEY );
            EditorGUILayout.LabelField( "Mun", String.Format( "0x{0:X}", mun_version ) );
            
            EditorGUI.indentLevel = 0;
            GUILayout.Space(5);
        }
    }
}
