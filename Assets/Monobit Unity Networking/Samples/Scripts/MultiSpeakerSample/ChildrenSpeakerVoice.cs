using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using MonobitEngine.VoiceChat;

[AddComponentMenu("Monobit Voice Chat/Children Speaker Sample")]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MonobitMicrophone))]
[RequireComponent(typeof(MonobitStreamingPlayer))]
public class ChildrenSpeakerVoice : MonobitEngine.VoiceChat.MonobitVoice
{
	/// <summary>
	/// コンストラクタ
	/// </summary>
	public ChildrenSpeakerVoice() { }

	/// <summary>
	/// 更新
	/// </summary>
	public override void Update ()
	{
	}
}
