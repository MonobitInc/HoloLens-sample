using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonobitEngine;
using MonobitEngine.VoiceChat;

[AddComponentMenu("Monobit Voice Chat/Multi Speaker Voice")]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MonobitMicrophone))]
[RequireComponent(typeof(MonobitStreamingPlayer))]
public class MultiSpeakerVoice : MonobitEngine.VoiceChat.MonobitVoice
{
	/// <summary>
	/// MonobitVoiceのリスト
	/// </summary>
	private List<SpeakerObjectInfo> SpeakerObjectInfos = new List<SpeakerObjectInfo>();
	
	/// <summary>
	/// コンストラクタ
	/// </summary>
	public MultiSpeakerVoice()
	{
	}

	/// <summary>
	/// エンコード前処理
	/// </summary>
	/// <returns>trueなら成功</returns>
	/// <remarks>音声のエンコードが始まる前に呼び出される</remarks>
	public override bool OnBeginEncode(int channels, int samplingRate)
	{
		return base.OnBeginEncode(channels, samplingRate);
	}

	/// <summary>
	/// 音声データ処理
	/// </summary>
	/// <param name="voice">音声データ</param>
	/// <returns>trueなら成功</returns>
	/// <remarks>音声データを加工する場合、この関数で加工してください</remarks>
	public override bool OnPreEncode(float[] voice, int channels, int samplingRate)
	{
		return base.OnPreEncode(voice, channels, samplingRate);
	}

	/// <summary>
	/// ボイスデータ後処理
	/// </summary>
	public override void OnEndEncode()
	{
		base.OnEndEncode();
	}

	/// <summary>
	/// デコード前処理
	/// </summary>
	/// <param name="channels">チャンネル数</param>
	/// <param name="samplingRate">サンプリングレート</param>
	/// <returns>trueなら成功</returns>
	public override bool OnBeginDecode(int channels, int samplingRate)
	{
		return base.OnBeginDecode(channels, samplingRate);
	}

	/// <summary>
	/// ボイスデータ再生前処理
	/// </summary>
	/// <param name="decodeVoice">Codecでデコードされたボイスデータ</param>
	/// <param name="channels">チャンネル数</param>
	/// <param name="samplingRate">サンプリングレート</param>
	/// <returns>trueなら成功</returns>
	public override bool OnPreDecode(float[] decodeVoice, int channels, int samplingRate)
	{
		return base.OnPreDecode(decodeVoice, channels, samplingRate);
	}

	/// <summary>
	/// ボイスデータ再生後処理
	/// </summary>
	public override void OnEndDecode(bool success)
	{
		base.OnEndDecode(success);
	}

	/// <summary>
	/// エンコードボイスデータ送信デリゲート
	/// </summary>
	/// <param name="debugMode">trueの場合、送信する音声が自分にも返ってくる</param>
	/// <param name="header">ヘッダー</param>
	/// <param name="voice">音声データ</param>
	public override void OnSendVoice(bool debugMode, object[] header, byte[] voice, int voice_size)
	{
        switch (SendStreamType)
        {
            case StreamType.BROADCAST:
                {
                    // 送信タイプの指定
                    var target = (debugMode == true) ? MonobitTargets.All : MonobitTargets.Others;

                    // ボイスチャットデータの送信処理
                    MonobitNetwork.SendVoice(monobitView, target, new int[0], ReliableMode, Encrypt, header, voice, voice_size);
                }
                break;
            case StreamType.MULTICAST:
                {
                    // 送信対象リストの取得
                    List<Int32> multicastPlayerList = GetMulticastTarget();

                    // 送信対象リストの中に自分自身が含まれている場合、デバッグモードを有効にする
                    if (multicastPlayerList.Contains(MonobitNetwork.player.ID))
                    {
                        m_DebugMode = true;
                    }

                    // デバッグモードが有効の状態で、かつ送信対象リストの中に自分自身が含まれていない場合、リストに追加する
                    if (debugMode && !multicastPlayerList.Contains(MonobitNetwork.player.ID))
                    {
                        multicastPlayerList.Add(MonobitNetwork.player.ID);
                    }

                    // 送信対象リストの中に自分自身が含まれていない場合、デバッグモードを無効にする
                    if (!multicastPlayerList.Contains(MonobitNetwork.player.ID))
                    {
                        m_DebugMode = false;
                    }

                    // ボイスチャットデータの送信処理
                    if (multicastPlayerList.Count > 0)
                    {
                        MonobitEngineBase.MonobitNetwork.SendVoice(monobitView, MonobitTargets.LimitedPlayer, multicastPlayerList.ToArray(), ReliableMode, Encrypt, header, voice, voice_size);
                    }
                }
                break;
        }
    }

	/// 音声データの受信と再生処理
	/// </summary>
	/// <param name="parameters">音声データの設定パラメータ</param>
	/// <param name="voice">音声データ</param>
	void OnRecievedVoiceWrapper(object[] parameters, byte[] voice, int voice_size)
	{
		for(int i = 0; i < SpeakerObjectInfos.Count; ++i)
		{
			var vo = SpeakerObjectInfos[i].voice;
			if (vo != null) vo.PlaybackVoiceData(parameters, voice, voice_size);
		}

		// ボイスデータの再生
		//PlaybackVoiceData(parameters, voice, voice_size);
	}

	/// <summary>
	/// インスタンスの開始
	/// </summary>
	public override void Awake()
	{
		base.Awake();
	}

	/// <summary>
	/// コンポーネントの開始 
	/// </summary>
	public override void Start ()
	{
		base.Start();
        monobitView.SetReceiveVoiceEvent(OnRecievedVoiceWrapper);
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Update ()
	{
		base.Update();
	}

	/// <summary>
	/// 破棄
	/// </summary>
	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	/// <summary>
	/// スピーカーオブジェクトの設定
	/// </summary>
	/// <param name="speakerObject"></param>
	public void SetSpeakerObject(GameObject speakerObject, Vector3 vector3, Quaternion quaternion)
	{
		if (speakerObject == null) return;
		if (SpeakerObjectInfos == null) SpeakerObjectInfos = new List<SpeakerObjectInfo>();
		SpeakerObjectInfos.Add(new SpeakerObjectInfo(speakerObject, vector3, quaternion));
	}

	/// <summary>
	/// スピーカーオブジェクトの廃棄
	/// </summary>
	public void DestroySpeakerObject()
	{
		for(int i = 0; i < SpeakerObjectInfos.Count; ++i)
		{
			MonobitNetwork.Destroy(SpeakerObjectInfos[i].speakerObject);
			SpeakerObjectInfos[i].speakerObject = null;
			SpeakerObjectInfos[i].voice = null;
		}
	}

	/// <summary>
	/// スピーカーオブジェクトの再作成
	/// </summary>
	public void SpawnSpeakerObject(List<SpeakerObjectInfo> infos)
	{
		if (SpeakerObjectInfos != null)
		{
			SpeakerObjectInfos.Clear();
			SpeakerObjectInfos = null;
		}

		for (int i = 0; i < infos.Count; ++i)
		{
			var info = infos[i];
			MonobitNetwork.Instantiate("SpeakerObject", info.position, info.rotation, 0);
		}
	}

	/// <summary>
	/// スピーカーオブジェクト情報
	/// </summary>
	public sealed class SpeakerObjectInfo
	{
		public GameObject speakerObject;
		public MonobitVoice voice;
		public Vector3 position;
		public Quaternion rotation;

		public SpeakerObjectInfo(GameObject go, Vector3 vector3, Quaternion quaternion)
		{
			System.Diagnostics.Debug.Assert(go != null);
			voice = go.GetComponent<MonobitVoice>();
			position = vector3;
			rotation = quaternion;
		}
	}

	/// <summary>
	/// スピーカーオブジェクトのクローンを取得する
	/// </summary>
	/// <returns></returns>
	public List<SpeakerObjectInfo> GetSpeakerObjectInfosAsClone()
	{
		if (SpeakerObjectInfos == null) return new List<SpeakerObjectInfo>();
		return SpeakerObjectInfos.ToList();
	}
}
