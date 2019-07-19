using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MonobitEngine
{
	/// <summary>
	/// MonobitNetworkプラグインで使うためのメインクラスです
	/// </summary>
	public sealed class MonobitNetwork : MonobitEngineBase.MonobitNetwork
    {
        [MunInitialize]
        public static void MonobitNetworkInitialize(){
            // オフラインモード
            MonobitEngine.MonobitNetwork.offline = ( MonobitEngine.MonobitNetworkSettings.MonobitServerSettings.HostType == ServerSettings.MunHostingOption.OfflineMode );
            
            // プレハブ生成処理ルーチンの登録
            MonobitEngineBase.MonobitNetwork.SetInstantiateWithResourcesHandle( MonobitNetwork.OnInstantiateWithResources );
            MonobitEngineBase.MonobitNetwork.SetInstantiateWithAssetBundleHandle( MonobitNetwork.OnInstantiateWithAssetBundle );
        }
        
        /// <summary>
        /// Monobitサーバーに接続します。
        /// </summary>
        /// <param name="gameVersion">クライアントのバージョン番号</param>
        /// <returns>true:接続成功、false:接続失敗</returns>
        /// <remarks>ユーザーはゲームバージョンで個々に分断される</remarks>
        public static bool ConnectServer(string gameVersion, Hashtable customAuthData = null)
        {
            // Monobitサーバーへの接続処理
            return MonobitEngineBase.MonobitNetwork.ConnectServerBase(gameVersion, customAuthData);
        }

		/// <summary>
		/// ルーム内の全クライアントが、ホストと同じゲームシーンをロードすべきかを決めます。
		/// </summary>
		/// <remarks>
		/// 読み込むゲームシーンを同期するためには、ホストはMonobitNetwork.LoadLevelを使っている必要があります。
		/// そうであれば、全てのクライアントは、更新や入室の際に新しいシーンを読み込むことになります。
		/// </remarks>
		public static bool autoSyncScene
        {
            get { return GetAutomaticallySyncScene(); }
            set
            {
                MonobitNetwork.SetAutomaticallySyncScene(value, ActiveSceneBuildIndex, ActiveSceneName);
            }
        }

		/// <summary>
		/// 現在動作しているシーンのファイル名を取得します。
		/// </summary>
		public static string ActiveSceneName
        {
            get
            {
                return mrs.Utility.GetSceneName();
            }
        }

		/// <summary>
		/// 現在動作しているシーンのインデックスを取得します。
		/// </summary>
		public static int ActiveSceneBuildIndex
        {
            get
            {
                return mrs.Utility.GetSceneIndex();
            }
        }

        /// <summary>
        /// Resources.Load() を実行してプレハブを生成します。
        /// </summary>
        /// <param name="prefabName">プレハブ名</param>
        /// <returns>新しく生成されたインスタンスを返します。</returns>
        public static GameObject OnInstantiateWithResources(string prefabName)
        {
            return (GameObject)Resources.Load(prefabName, typeof(GameObject));
        }

        /// <summary>
        /// AssetBundle.LoadAsset() を実行してプレハブを生成します。
        /// </summary>
        /// <param name="assetBundlePath">アセットバンドルのパス名</param>
        /// <param name="assetBundleName">アセットバンドル名</param>
        /// <returns>新しく生成されたインスタンスを返します。</returns>
        public static GameObject OnInstantiateWithAssetBundle(string assetBundlePath, string assetBundleName)
        {
            GameObject go = null;

            try
            {
                // アセットバンドルに含まれるリストの取得
#if UNITY_5_3_OR_NEWER || UNITY_5_3
                AssetBundle bundle = AssetBundle.LoadFromFile(assetBundlePath + "/" + assetBundleName);
#else
                AssetBundle bundle = AssetBundle.CreateFromFile(assetBundlePath + "/" + assetBundleName);
#endif
                string[] bundleNameArray = bundle.GetAllAssetNames();
                if ( 0 < bundleNameArray.Length ){
                    go = bundle.LoadAsset<GameObject>(bundleNameArray[0]);
                }
                bundle.Unload(false);
            }
            catch (System.Exception)
            {
                Debug.LogError("AssetBundle.LoadAsset() failed.");
                return null;
            }

            return go;
        }
    }
}
