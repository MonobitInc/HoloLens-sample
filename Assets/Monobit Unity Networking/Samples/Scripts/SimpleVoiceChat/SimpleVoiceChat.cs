using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MonobitEngine;
using MonobitEngine.VoiceChat;

namespace MonobitEngine.Sample
{
    public class SimpleVoiceChat : MonobitEngine.MonoBehaviour
    {
        /** ルーム名. */
        private string roomName = "";

        /** ルーム内のプレイヤーに対するボイスチャット送信可否設定. */
        private Dictionary<MonobitPlayer, Int32> vcPlayerInfo = new Dictionary<MonobitPlayer, int>();

        /** 自身が所有するボイスアクターのMonobitViewコンポーネント. */
        private MonobitVoice myVoice = null;

        /** ボイスチャット送信可否設定の定数. */
        private enum EnableVC
        {
            ENABLE = 0,         /**< 有効. */
            DISABLE = 1,        /**< 無効. */
        }

        /**
         * GUI制御.
         */
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

            // MUNサーバに接続している場合
            if (MonobitNetwork.isConnect)
            {
                // ルームに入室している場合
                if (MonobitNetwork.inRoom)
                {
                    // ルーム内のプレイヤー一覧の表示
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("PlayerList : ");
                    foreach (MonobitPlayer player in MonobitNetwork.playerList)
                    {
                        GUILayout.Label(player.name + " ");
                    }
                    GUILayout.EndHorizontal();

                    // ルームからの退室
                    if (GUILayout.Button("Leave Room", GUILayout.Width(150)))
                    {
                        MonobitNetwork.LeaveRoom();
                    }

                    if(myVoice != null)
                    {
                        // 送信タイプの設定
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("VoiceChat Send Type : ");
                        Int32 streamType = myVoice.SendStreamType == StreamType.BROADCAST ? 0 : 1;
                        myVoice.SendStreamType = (StreamType)GUILayout.Toolbar(streamType, new string[] { "broadcast", "multicast" });
                        GUILayout.EndHorizontal();

                        // マルチキャスト送信の場合の、ボイスチャットの送信可否設定
                        if (myVoice.SendStreamType == StreamType.MULTICAST)
                        {
                            List<MonobitPlayer> playerList = new List<MonobitPlayer>(vcPlayerInfo.Keys);
                            List<MonobitPlayer> vcTargets = new List<MonobitPlayer>();
                            foreach (MonobitPlayer player in playerList)
                            {
                                // GUI による送信可否の切替
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("PlayerName : " + player.name + " ");
                                GUILayout.Label("Send Permission: ");
                                vcPlayerInfo[player] = GUILayout.Toolbar(vcPlayerInfo[player], new string[] { "Allow", "Deny" });
                                GUILayout.EndHorizontal();
                                // ボイスチャットの送信可のプレイヤー情報を登録する
                                if (vcPlayerInfo[player] == (Int32)EnableVC.ENABLE)
                                {
                                    vcTargets.Add(player);
                                }
                            }

                            // ボイスチャットの送信可否設定を反映させる
                            myVoice.SetMulticastTarget(vcTargets.ToArray());
                        }
                    }
                }
                // ルームに入室していない場合
                else
                {
                    // ルーム名の入力
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("RoomName : ");
                    roomName = GUILayout.TextField(roomName, GUILayout.Width(200));
                    GUILayout.EndHorizontal();

                    // ルームを作成して入室する
                    if (GUILayout.Button("Create Room", GUILayout.Width(150)))
                    {
                        MonobitNetwork.CreateRoom(roomName);
                    }

                    // ルーム一覧を検索
                    foreach (RoomData room in MonobitNetwork.GetRoomData())
                    {
                        // ルームを選択して入室する
                        if (GUILayout.Button("Enter Room : " + room.name + "(" + room.playerCount + "/" + ((room.maxPlayers == 0) ? "-" : room.maxPlayers.ToString()) + ")"))
                        {
                            MonobitNetwork.JoinRoom(room.name);
                        }
                    }
                }
            }
            // MUNサーバに接続していない場合
            else
            {
                // プレイヤー名の入力
                GUILayout.BeginHorizontal();
                GUILayout.Label("PlayerName : ");
                MonobitNetwork.playerName = GUILayout.TextField((MonobitNetwork.playerName == null) ? "" : MonobitNetwork.playerName, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                // デフォルトロビーへの自動入室を許可する
                MonobitNetwork.autoJoinLobby = true;

                // MUNサーバに接続する
                if (GUILayout.Button("Connect Server", GUILayout.Width(150)))
                {
                    MonobitNetwork.ConnectServer("SimpleVoiceChat_v1.0");
                }
            }
        }

        // 自身がルーム入室に成功したときの処理
        public void OnJoinedRoom()
        {
            vcPlayerInfo.Clear();
            vcPlayerInfo.Add(MonobitNetwork.player, (Int32)EnableVC.DISABLE);

            foreach (MonobitPlayer player in MonobitNetwork.otherPlayersList)
            {
                vcPlayerInfo.Add(player, (Int32)EnableVC.ENABLE);
            }

            GameObject go = MonobitNetwork.Instantiate("VoiceActor", Vector3.zero, Quaternion.identity, 0);
            myVoice = go.GetComponent<MonobitVoice>();
			if (myVoice != null)
			{
	            myVoice.SetMicrophoneErrorHandler(OnMicrophoneError);
	            myVoice.SetMicrophoneRestartHandler(OnMicrophoneRestart);
			}
        }

        // 誰かがルームにログインしたときの処理
        public void OnOtherPlayerConnected(MonobitPlayer newPlayer)
        {
            if (!vcPlayerInfo.ContainsKey(newPlayer))
            {
                vcPlayerInfo.Add(newPlayer, (Int32)EnableVC.ENABLE);
            }
        }

        // 誰かがルームからログアウトしたときの処理
        public virtual void OnOtherPlayerDisconnected(MonobitPlayer otherPlayer)
        {
            if (vcPlayerInfo.ContainsKey(otherPlayer))
            {
                vcPlayerInfo.Remove(otherPlayer);
            }
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
		}
    }
}
