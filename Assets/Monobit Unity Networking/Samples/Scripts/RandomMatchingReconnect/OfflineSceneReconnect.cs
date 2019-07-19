using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using MonobitEngine.Definitions;

public class OfflineSceneReconnect : MonobitEngine.MonoBehaviour
{
	// シーンファイル名
	public static readonly string SceneNameOffline = "OfflineSceneReconnect";
	public static readonly string SceneNameOnline = "OnlineSceneReconnect";

	// マッチングルームの最大人数
	private byte maxPlayers = 10;

	// ルーム名
	private string roomName = "roomName";

	// ルームリスト
	RoomData[] m_RoomData = null;

	// ホストプレイヤーが必要かどうかのフラグ
	private bool isNeedHostPlayer = true;

	// 開始関数
	public void Awake()
	{
		// デフォルトロビーへの強制入室をONにする。
		MonobitNetwork.autoJoinLobby = true;

		// ホストのシーンと同じシーンを部屋に入室した人もロードする。
		MonobitNetwork.autoSyncScene = true;

		// まだ未接続の場合、MonobitNetworkに接続する。
		if (! MonobitNetwork.isConnect)
        {
			MonobitNetwork.ConnectServer("RandomMatchingReconnect_v1.0");
		}
	}

	// GUIまわりの記述
	public void OnGUI()
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

		if ( MonobitNetwork.isConnect )
		{
			// ルーム一覧を取得
			m_RoomData = MonobitNetwork.GetRoomData();

			// ルーム一覧からボタン選択
			if (m_RoomData != null)
			{
				for (int i = 0; i < m_RoomData.Length; i++)
				{
					if (GUILayout.Button(m_RoomData[i].name + "(" + m_RoomData[i].playerCount + ")", GUILayout.Width(100)))
					{
						MonobitNetwork.JoinRoom(m_RoomData[i].name);
					}
				}
			}

			// ルーム名の入力
			this.roomName = GUILayout.TextField(this.roomName);

			// ルームの作成
			if (GUILayout.Button("Create Room", GUILayout.Width(100)))
			{
				MonobitNetwork.CreateRoom(this.roomName, new RoomSettings() { isVisible = true, isOpen = true, maxPlayers = this.maxPlayers, isNeedHostPlayer = this.isNeedHostPlayer }, null);
			}

			// ルームへの入室（ランダム）
			if (GUILayout.Button("Join Room", GUILayout.Width(100)))
			{
				MonobitNetwork.JoinRandomRoom();
			}
		}
	}

	// ルーム作成時の処理
	public void OnCreatedRoom()
	{
		Debug.Log("OnCreatedRoom player="+ MonobitNetwork.player);

		// シーンをオンラインシーンに
		MonobitNetwork.LoadLevel(SceneNameOnline);
	}

	// ルーム作成失敗時の処理
	public void OnCreateRoomFailed(object[] parameters)
	{
		Debug.Log("OnCreateRoomFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);
	}

	// ルーム入室時の処理
	public void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom player="+ MonobitNetwork.player);

		// シーンをオンラインシーンに
		MonobitNetwork.LoadLevel(SceneNameOnline);
	}

	// ランダムルーム入室失敗時の処理
	public void OnMonobitRandomJoinFailed(object[] parameters)
	{
		Debug.Log("OnMonobitRandomJoinFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);
	}

	// 指定ルーム入室失敗時の処理
	public void OnJoinRoomFailed(object[] parameters)
	{
		Debug.Log("OnJoinRoomFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);
	}

	// 接続が切断されたときの処理
	public void OnDisconnectedFromServer()
	{
		Debug.Log("OnDisconnectedFromServer");
	}

	// 接続失敗時の処理
	public void OnConnectToServerFailed(object parameters)
	{
		Debug.Log("OnConnectToServerFailed : StatusCode = " + parameters + ", ServerAddress = " + MonobitNetwork.ServerAddress);
	}

	// ロビー接続時の処理
	public void OnJoinedLobby()
	{
		Debug.Log("OnJoinedLobby");
	}
}