using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using MonobitEngine;

public class HololensSample : MonobitEngine.MonoBehaviour
{
    [SerializeField] TextMesh State = null;                     //状態UI
    [SerializeField] GameObject ARCamera = null;                //自分のCameraオブジェクト
    [SerializeField] Transform CreatePosition = null;           //Cubeを生成する位置
    [SerializeField] Vector3 customG = Vector3.zero;            //カスタマイズ重力
    [SerializeField] TextMesh NetworkText = null;               //接続ボタンの文字

    public const string PLAYER_COLOR_R = "PLAYER_COLOR_R";
    public const string PLAYER_COLOR_G = "PLAYER_COLOR_G";
    public const string PLAYER_COLOR_B = "PLAYER_COLOR_B";

    public enum ButtonType
    {
        None,
        CreateObject,       //Cube生成
        ClearObject,        //Cube削除
        Network,            //ネットワーク接続/切断
    }

    public static HololensSample Instance;

    private void Awake()
    {
        Instance = this;
        //カスタマイズ重力で、Cubeをふわふわする
        Physics.gravity = customG;
    }

    // Start is called before the first frame update
    void Start()
    {
        //ネットワーク送信レート　一秒5回
        MonobitNetwork.sendRate = 5;

        //各自プレイヤーの色はプレイヤーパラメータで記録する
        Hashtable table = new Hashtable();
        table.Add(HololensSample.PLAYER_COLOR_R, Random.Range(0f, 1f));
        table.Add(HololensSample.PLAYER_COLOR_G, Random.Range(0f, 1f));
        table.Add(HololensSample.PLAYER_COLOR_B, Random.Range(0f, 1f));
        MonobitNetwork.SetPlayerCustomParameters(table);
    }

    //状態UI表示
    public void SetStateText(string str)
    {
        if (State)
        {
            State.text = str;
            State.gameObject.SetActive(true);
        }
    }

    //状態UI隠す
    public void HideState()
    {
        if (MonobitNetwork.isConnect == false)
        {
            return;
        }

        if (State)
        {
            State.gameObject.SetActive(false);
        }
    }

    //サーバーに接続
    public void Connect()
    {
        SetStateText("Connecting Server...");
        MonobitNetwork.autoJoinLobby = true;
        MonobitNetwork.ConnectServer("Monobit Hololens Server");
    }

    //サーバーに切断
    public void Disconnect()
    {
        MonobitNetwork.DisconnectServer();
    }

    //ボタンAction
    public void ButtonPressed(int btn)
    {
        switch ((ButtonType)btn)
        {
            case ButtonType.CreateObject:
                MonobitNetwork.Instantiate("HololensCube", CreatePosition.position + Random.insideUnitSphere * 0.5f, Random.rotation, 0);
                break;
            case ButtonType.ClearObject:
                //全員に通知、各自は自分が生成したものを削除
                monobitView.RPC("ClearAll", MonobitTargets.All);
                break;
            case ButtonType.Network:
                if (MonobitNetwork.isConnect)
                {
                    Disconnect();
                }
                else
                {
                    Connect();
                }
                break;
        }
    }

    //Cube削除
    [MunRPC]
    void ClearAll()
    {
        Debug.Log("ClearAll");
        HololensCube.ClearSelf();
    }

    //同期の原点を変更
    public void SetBasePosition(Transform t)
    {
        HololensCube.SelfCubes.ForEach(c =>
        {
            c.transform.parent = null;
        });

        transform.position = t.position;
        transform.rotation = t.rotation;

        HololensCube.SelfCubes.ForEach(c =>
        {
            c.transform.parent = transform;
        });
    }

    // Update is called once per frame
    void Update()
    {
        #region Debug Input
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ButtonPressed((int)ButtonType.CreateObject);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ButtonPressed((int)ButtonType.ClearObject);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ButtonPressed((int)ButtonType.Network);
        }
        #endregion

        if (MonobitNetwork.inRoom == false)
        {
            return;
        }

        //ARCameraカメラの位置は変更しない、同期するオブジェクトをARCameraに合わせる
        if (HololensVoice.Self)
        {
            HololensVoice.Self.transform.position = ARCamera.transform.position;
            HololensVoice.Self.transform.rotation = ARCamera.transform.rotation;
        }
    }

    //サーバーに接続成功
    void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        SetStateText("Joining Room...");
        MonobitNetwork.JoinOrCreateRoom("Monobit Hololens Room", new RoomSettings(), LobbyInfo.Default);
    }

    //ルームに入ることが失敗
    void OnJoinRoomFailed()
    {
        Debug.Log("OnJoinRoomFailed");
    }

    //ルームに入った
    void OnJoinedRoom()
    {
        NetworkText.text = "接続中";
        NetworkText.color = Color.green;
        Debug.Log("OnJoinedRoom Player: " + MonobitNetwork.player.ID);

        HideState();

        //自分のアバターを生成
        MonobitNetwork.Instantiate("HololensVoice", Vector3.zero, Quaternion.identity, 0);
    }

    //他のプレイヤーがルームに入った
    public void OnOtherPlayerConnected(MonobitPlayer newPlayer)
    {
        Debug.Log("OnOtherPlayerConnected Player: " + newPlayer.ID);
    }

    //他のプレイヤーが接続切断
    public virtual void OnOtherPlayerDisconnected(MonobitPlayer otherPlayer)
    {
        Debug.Log("OnOtherPlayerDisconnected Player: " + otherPlayer.ID);
    }

    //サーバーに接続失敗
    void OnConnectToServerFailed(DisconnectCause cause)
    {        
        Debug.Log("OnConnectToServerFailed cause=" + cause);
    }

    //サーバーに切断
    void OnDisconnectedFromServer()
    {
        NetworkText.text = "接続切れ";
        NetworkText.color = Color.red;
        Debug.Log("OnDisconnectedFromServer");
    }
}
