using System;
using UnityEngine;
using MonobitEngine;

namespace Monobit.Support
{
    [Serializable]
    [RequireComponent(typeof(MonobitView))]
    [AddComponentMenu("Monobit Networking Support/Monobit Auto Login Template")]
    public class MonobitAutoLoginTemplate : MonobitEngine.MonoBehaviour
    {
        [SerializeField]
        public GameObject InstantiatePrefab = null;

        [SerializeField]
        public Vector3 camPosition = new Vector3(1, 1, -3);

        [SerializeField]
        public Quaternion camRotation = Quaternion.identity;

		private GameObject go = null;

		private bool bStart = false;
		private bool bSelectMenu = false;
        private bool bReloadScene = false;

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

            if (bSelectMenu == false)
            {
                if (!MonobitNetwork.isConnect)
                {
                    if (GUILayout.Button("Connect", GUILayout.Width(150)))
                    {
                        bSelectMenu = true;
                        MonobitNetwork.autoJoinLobby = true;
                        MonobitNetwork.ConnectServer("MonobitAutoLoginTemplate_v0.1");
                    }
                }
                else if (MonobitNetwork.inRoom)
				{
					if (!bStart)
					{
						if (GUILayout.Button("GameStart", GUILayout.Width(150)))
						{
							bSelectMenu = true;
							monobitView.RPC("GameStart", MonobitTargets.All, null);
						}
					}
					else
					{
						if (GUILayout.Button("Disconnect", GUILayout.Width(150)))
						{
							MonobitNetwork.DisconnectServer();
						}
					}
                }
            }
        }

        void Update()
        {
            if (bReloadScene)
            {
                // 全てのオブジェクトを消すため、シーンを再ロードする。
                mrs.Utility.LoadScene(mrs.Utility.GetSceneIndex());

                bReloadScene = false;
            }
        }

        void OnConnectToServerFailed(DisconnectCause cause)
        {
            bSelectMenu = false;
            Debug.Log("OnConnectToServerFailed cause="+cause);
        }
        void OnDisconnectedFromServer()
        {
            bSelectMenu = false;
            bReloadScene = true;
        }
        void OnJoinedLobby()
        {
			bSelectMenu = false;
            Debug.Log("OnJoinedLobby");
            MonobitNetwork.JoinOrCreateRoom("AutoLoginRoom", new RoomSettings(), LobbyInfo.Default);
        }
		void OnJoinRoomFailed()
        {
            Debug.Log("OnJoinRoomFailed");
        }
        void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
		}

		[MunRPC]
		void GameStart()
		{
			bStart = true;
			bSelectMenu = false;
			if (InstantiatePrefab == null || go != null)
            {
                return;
            }
			go = MonobitNetwork.Instantiate(InstantiatePrefab.name, Vector3.zero, Quaternion.identity, 0) as GameObject;
			if (go != null)
            {
                Camera mainCamera = GameObject.FindObjectOfType<Camera>();
                mainCamera.GetComponent<Camera>().enabled = false;

				Camera camera = go.GetComponentInChildren<Camera>();
                if (camera == null)
                {
                    GameObject camObj = new GameObject();
                    camObj.name = "Camera";
                    camera = camObj.AddComponent<Camera>();
					camera.transform.parent = go.transform;
                }
                camera.transform.position = camPosition;
                camera.transform.rotation = camRotation;
            }
        }
    }
}

