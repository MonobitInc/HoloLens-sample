using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
namespace MonobitEngine
{
    [InitializeOnLoad]
    public class MonobitBridge
    {
        static MonobitBridge()
        {
            Initialize();
        }

        public static void Initialize()
        {
            // MonobitServerSetting.asset ならびに MonobitAuthenticationCode.asset のディレクトリパスの指定
            MonobitNetworkSettings.SetServerSettingsDirectory("Assets/Monobit Unity Networking/Resources");

            if (MonobitNetworkSettings.MonobitServerSettings == null)
            {
                // サーバー設定ファイルがなかった場合
                CreateSettings();
            }

            if (MonobitNetworkSettings.MonobitAuthIdSettings == null)
            {
                // 認証ID設定ファイルがなかった場合
                CreateAuthenticateCode();
            }
        }

        /**
         * サーバー設定ファイルの作成.
         */
        public static void CreateSettings()
        {
            // ファイル有無の確認
            if (System.IO.File.Exists(MonobitNetworkSettings.GetServerSettingsPath()))
            {

                return;
            }

            // パスの確認
            string settingPath = Path.GetDirectoryName(MonobitNetworkSettings.GetServerSettingsPath());
            if (!Directory.Exists(settingPath))
            {
                Directory.CreateDirectory(settingPath);
                AssetDatabase.ImportAsset(settingPath);
            }

            // 設定ファイルの作成
            var item = ScriptableObject.CreateInstance<ServerSettings>();
            if (item != null)
            {
                AssetDatabase.CreateAsset(item, MonobitNetworkSettings.GetServerSettingsPath());
                AssetDatabase.SaveAssets();

                // インスタンスの設定
                MonobitNetworkSettings.MonobitServerSettings = item as ServerSettings;
                if (MonobitNetworkSettings.MonobitServerSettings == null) throw new Exception("Failed to create server settings.");
            }
        }

        /**
         * 認証ID設定ファイルの作成.
         */
        public static void CreateAuthenticateCode()
        {
            // ファイル有無の確認
            if (System.IO.File.Exists(MonobitNetworkSettings.GetAuthCodeSettingsPath()))
            {
                return;
            }

            // パスの確認
            string settingPath = Path.GetDirectoryName(MonobitNetworkSettings.GetAuthCodeSettingsPath());
            if (!Directory.Exists(settingPath))
            {
                Directory.CreateDirectory(settingPath);
                AssetDatabase.ImportAsset(settingPath);
            }

            // 設定ファイルの作成
            var item = ScriptableObject.CreateInstance<AuthenticationCode>();
            if (item != null)
            {
                // 認証コードの自動生成
                MonobitNetworkSettings.MonobitServerSettings.UpdateAuthID();
				item.saveAuthID = MonobitNetworkSettings.Encrypt(MonobitNetworkSettings.MonobitServerSettings.AuthID);
                EditorUtility.SetDirty(item);

                // 設定ファイルの作成
                AssetDatabase.CreateAsset(item, MonobitNetworkSettings.GetAuthCodeSettingsPath());
                AssetDatabase.SaveAssets();

                // インスタンスの設定
                MonobitNetworkSettings.MonobitAuthIdSettings = item as AuthenticationCode;
                if (MonobitNetworkSettings.MonobitAuthIdSettings == null) throw new Exception("Failed to save auth code.");
            }
        }
    }
}
#endif
