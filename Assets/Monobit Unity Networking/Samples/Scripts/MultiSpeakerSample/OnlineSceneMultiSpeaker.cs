using UnityEngine;
using System.Collections.Generic;
using MonobitEngine;
using MonobitEngine.Definitions;
using MonobitEngine.VoiceChat;

public class OnlineSceneMultiSpeaker : MonobitEngine.MonoBehaviour
{
    // ゲームスタートフラグ
    private bool m_bGameStart = false;

    // 制限時間
    private int battleEndFrame = 60 * 60 * 60;

    // 自身のオブジェクトを生成したかどうかのフラグ
    private bool spawnMyChara = false;

    // 自身のオブジェクトの出現場所
    GameObject myObject = null;
    private Vector3 myPosition = new Vector3();
    private Quaternion myRotation = new Quaternion();

    // 意図しないタイミングで Disconnect されたかどうかのフラグ
    private bool bSafeDiscoonect = false;
    private bool bDisconnecting = false;
    private bool bReconnecting = false;

    // 再接続する際のルーム名
    private string reconnectRoomName = "";

	// 音声データ量
	private int m_bps = 0;

	// GUI処理
	void OnGUI()
    {
        // GUI用の解像度を調整する
        Vector2 guiScreenSize = new Vector2(800, 480);
        if (Screen.width > Screen.height)
        {
            // landscape
            GUIUtility.ScaleAroundPivot(new Vector2(Screen.width / guiScreenSize.x, Screen.height / guiScreenSize.y), Vector2.zero);
        }
        else
        {
            // portrait
            GUIUtility.ScaleAroundPivot(new Vector2(Screen.width / guiScreenSize.y, Screen.height / guiScreenSize.x), Vector2.zero);
        }

        // マイクデバイスの表示
        GUILayout.Label( "Microphone: "+ MonobitMicrophone.MicrophoneDeviceName );

        // マイクデバイスの切り替え
        for(int i = 0; i < Microphone.devices.Length; ++i)
		{
			string name = (Microphone.devices[i].Length > 0) ? Microphone.devices[i] : null;

			if (i == 0)
			{
				if ( null != MonobitMicrophone.MicrophoneDeviceName ){
					// i == 0はデフォルト。デフォルトに必ず戻せるようにしておく
					if ( GUILayout.Button( "Default" + ( ( null == name ) ? "" : " < " + name + " >" ), GUILayout.Width( 400 ) ) ){
						MonobitMicrophone.MicrophoneDeviceName = null;
					}
				}
				continue;
			}
			else if ( name == null)
			{
	            // Unity2018より前のバージョンだと、日本語を含むデバイス名が空文字列になり、選択できないので、無視
				continue;
			}
           
            // 選択済のデバイスは無視
            if ( name == MonobitMicrophone.MicrophoneDeviceName ) continue;
            
            if ( GUILayout.Button( name, GUILayout.Width( 400 ) ) ){
                MonobitMicrophone.MicrophoneDeviceName = name;
            }
        }
        
        // プレイヤーIDの表示
        if (MonobitNetwork.player != null)
        {
			GUILayout.Label(string.Format("My Player ID : {0}", MonobitNetwork.player.ID));
        }

        // ルーム情報の取得
        Room room = MonobitNetwork.room;
        if (room != null)
        {
			// ルーム名の表示
			GUILayout.Label("Voice bps : " + m_bps);

			// ルーム名の表示
			GUILayout.Label(string.Format("Room Name : {0}", room.name));

            // ルーム内に存在するプレイヤー数の表示
            GUILayout.Label(string.Format("PlayerCount : {0}", room.playerCount));

            // ルームがオープンかクローズか
            GUILayout.Label(string.Format("Room IsOpen : {0}", room.open));

            // 制限時間の表示
            if (m_bGameStart)
            {
                GUILayout.Label(string.Format("Rest Frame : {0}", this.battleEndFrame));
            }
        }
        // 部屋からの離脱
        if (GUILayout.Button("Leave Room", GUILayout.Width(100)))
        {
            // 安全なDisconnect
            bSafeDiscoonect = true;

            // 部屋から離脱する
            MonobitNetwork.DisconnectServer();

            // シーンをオフラインシーンへ
            MonobitNetwork.LoadLevel(OfflineSceneMultiSpeaker.SceneNameOffline);

            return;
        }

        // ホストの場合
        if (MonobitNetwork.isHost)
        {
            // ゲームスタート前にゲームスタートするかどうか
            if (!m_bGameStart && GUILayout.Button("Start Game", GUILayout.Width(100)))
            {
                // ゲームスタートフラグを立てる
                m_bGameStart = true;

                // バトルスタートを通知
                monobitView.RPC("OnGameStart", MonobitTargets.All, null);
            }

            // バトル終了
            if (battleEndFrame <= 0)
            {
                // 安全なDisconnect
                bSafeDiscoonect = true;

                // ルームを抜ける
                MonobitNetwork.DisconnectServer();

                // シーンをオフラインシーンへ
                MonobitNetwork.LoadLevel(OfflineSceneMultiSpeaker.SceneNameOffline);

                return;
            }
        }

        // 意図しないタイミングで切断されたとき
        if (!MonobitNetwork.isConnect && !bSafeDiscoonect)
        {
            GUILayout.Window(0, new Rect(Screen.width / 2 - 100, Screen.height / 2 - 40, 200, 80), WindowGUI, "Disconnect");
        }

    }

    // ウィンドウ表示
    void WindowGUI(int WindowID)
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        GUIStyleState stylestate = new GUIStyleState();
        stylestate.textColor = Color.white;
        style.normal = stylestate;
        if (bDisconnecting)
        {
            GUILayout.Label("MUN is disconnect.\nAre you reconnect?", style);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Yes", GUILayout.Width(50)))
            {
                // もう一度接続処理を実行する
                bDisconnecting = false;
                bReconnecting = true;
                m_bGameStart = false;
                spawnMyChara = false;
                MonobitNetwork.ConnectServer("MultiSpeakerSample_v1.0");
            }
            if (GUILayout.Button("No", GUILayout.Width(50)))
            {
                bDisconnecting = false;

                // シーンをオフラインシーンへ
                MonobitNetwork.LoadLevel(OfflineSceneMultiSpeaker.SceneNameOffline);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("reconnecting...", style);
        }
    }

    // 更新処理
    void Update()
    {
        // 自身のキャラクタ位置を退避する
        if (myObject != null)
        {
            myPosition = myObject.transform.position;
            myRotation = myObject.transform.rotation;
        }

        // ルーム名を退避する
        if (MonobitNetwork.room != null)
        {
            reconnectRoomName = MonobitNetwork.room.name;
        }

        // ゲームスタート後、まだ自分のキャラクタのspawnが終わってない状態であれば、自身のキャラクタをspawnする
        if (m_bGameStart && !spawnMyChara)
        {
            OnGameStart();
        }

        // ゲームスタート後、ホストなら
        if (m_bGameStart && MonobitNetwork.isHost)
        {
            // 制限時間の減少
            if (battleEndFrame > 0)
            {
                battleEndFrame--;
            }
            // 制限時間をRPCで送信
            object[] param = new object[]
            {
                battleEndFrame
            };

            monobitView.RPC("TickCount", MonobitTargets.Others, param);
        }
    }

    // 自オブジェクトをスポーン
    void SpawnMyObject()
    {
		// プレイヤーの配置（他クライアントにも同時にInstantiateする）
		myObject = MonobitNetwork.Instantiate("SD_unitychan_source_speaker", myPosition, myRotation, 0);
		if (myObject != null)
		{
			var wrapper = myObject.GetComponent<MultiSpeakerVoice>();
			wrapper.SetCallVoiceDataBps(VoiceDataBps);
			wrapper.SetMicrophoneErrorHandler(OnMicrophoneError);
			wrapper.SetMicrophoneRestartHandler(OnMicrophoneRestart);
		}

		// 出現させたことを確認
		spawnMyChara = true;
    }
    
    // 自オブジェクトをリスポーン
    void RespawnMyObject()
    {
        UnityEngine.Debug.Log( "RespawnMyObject myObject="+ myObject );
		List<MultiSpeakerVoice.SpeakerObjectInfo> infos = null;

		if ( null != myObject )
		{
			// スピーカーオブジェクトの破棄
			var wrapper = myObject.GetComponent<MultiSpeakerVoice>();
			wrapper.DestroySpeakerObject();
			infos = wrapper.GetSpeakerObjectInfosAsClone();

			// 自オブジェクトを破棄
			MonobitNetwork.Destroy( myObject );
		}

		// 自オブジェクトをスポーン
        SpawnMyObject();
		if (null != myObject)
		{
			// スピーカーオブジェクトのスポーン
			var wrapper = myObject.GetComponent<MultiSpeakerVoice>();
			wrapper.SpawnSpeakerObject(infos);
		}
	}

	// ゲームスタートを受信(RPC)
	[MunRPC]
    void OnGameStart()
    {
        // ゲームスタートフラグを立てる
        m_bGameStart = true;

        if (!bReconnecting)
        {
            // ある程度ランダムな場所にプレイヤーを配置する
            myPosition = Vector3.zero;
            myPosition.x = Random.Range(-10.0f, 10.0f);
            myPosition.z = Random.Range(-10.0f, 10.0f);

            myRotation = Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f), Vector3.up);
        }

        // 自オブジェクトをスポーン
        SpawnMyObject();

        // 再接続処理完了
        bReconnecting = false;

#if false
        // RPCのOthersBufferedの確認用
        monobitView.RPC( "BufferedPlayerId", MonobitTargets.OthersBuffered, MonobitNetwork.player.ID );
#endif
    }

    // 制限時間を受信(RPC)
    [MunRPC]
    void TickCount(int frame)
    {
        // ゲームスタートフラグを立てる（途中参加者のための処置）
        m_bGameStart = true;

        // 制限時間を同期する
        this.battleEndFrame = frame;
    }

	// bpsデータを取得
	void VoiceDataBps(int bps)
	{
		m_bps = bps;
	}

	/// <summary>
	/// マイクのエラーハンドリング用デリゲート
	/// </summary>
	/// <returns>
	/// true : 内部にてStopCaptureを実行しループを抜けます。
	/// false: StopCaptureを実行せずにループを抜けます。
	/// </returns>
	public bool OnMicrophoneError()
	{
		UnityEngine.Debug.LogError("Error: Microphone Error !!!");
		return true;
	}

	/// <summary>
	/// マイクのリスタート用デリゲート
	/// </summary>
	/// <remarks>
	/// 呼び出された時点ではすでにStopCaptureされています。
	/// </remarks>
	public void OnMicrophoneRestart()
	{
		UnityEngine.Debug.LogWarning("Info: Microphone Restart !!!");
		
		// マイクを挿し直した後で、
		// マイク入力を正常に機能させるためには、
		// 自オブジェクトをリスポーンするのが簡単
		RespawnMyObject();
	}

#if false
    // RPCのOthersBufferedの確認用(RPC)
    [MunRPC]
    void BufferedPlayerId( int playerId )
    {
        Debug.Log( System.String.Format( "BufferedPlayerId playerId={0}", playerId ) );
    }
#endif

	// 接続が切断されたときの処理
	public void OnDisconnectedFromServer()
    {
        Debug.Log("Disconnected from Monobit");
        bDisconnecting = true;
    }

    // 接続失敗時の処理
    public void OnConnectToServerFailed(object parameters)
    {
        Debug.Log("OnConnectToServerFailed : StatusCode = " + parameters + ", ServerAddress = " + MonobitNetwork.ServerAddress);
        bDisconnecting = true;
    }

    // 接続が確立した時の処理
    public void OnJoinedLobby()
    {
        // 現在残っているルーム情報から再接続を実行する
        MonobitNetwork.JoinRoom(reconnectRoomName);
    }

    // ルームが存在しなかった場合
    // 指定ルーム入室失敗時の処理
    public void OnJoinRoomFailed(object[] parameters)
    {
        Debug.Log("OnJoinRoomFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);

        // オフラインシーンに戻す
        MonobitNetwork.LoadLevel(OfflineSceneMultiSpeaker.SceneNameOffline);
    }
}
