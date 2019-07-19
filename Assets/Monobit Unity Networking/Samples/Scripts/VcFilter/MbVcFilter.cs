using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using MonobitEngine.VoiceChat;

[AddComponentMenu("Monobit Voice Chat/Monobit Voice Filter Sample")]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MonobitMicrophone))]
[RequireComponent(typeof(MonobitStreamingPlayer))]
public class MbVcFilter : MonobitEngine.VoiceChat.MonobitVoice
{
	/// <summary>
	/// コンストラクタ
	/// </summary>
	public MbVcFilter()
	{
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
		if (monobitView.ownerId != 1)
		{
			// 特定のオーナーIDの時にオーディオエフェクトを掛ける
			OnVoiceEffect(decodeVoice, channels);
		}
		return base.OnPreDecode(decodeVoice, channels, samplingRate);
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
		// ボイスデータの再生
		PlaybackVoiceData(parameters, voice, voice_size, ! ReliableMode);
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
	public override void Start()
	{
		base.Start();
		monobitView.SetReceiveVoiceEvent(OnRecievedVoiceWrapper);
	}

	/// <summary>
	/// 更新
	/// </summary>
	public override void Update()
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
	/// オーディオのフィルター関数
	/// </summary>
	/// <param name="data"></param>
	/// <param name="channels"></param>
	public void OnVoiceEffect(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i++)
		{
			data[i] *= 2;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="data"></param>
	/// <param name="channels"></param>
	private void OnAudioFilterRead(float[] data, int channels)
	{
	}
}
