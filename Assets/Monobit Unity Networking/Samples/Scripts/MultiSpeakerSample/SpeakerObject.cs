using UnityEngine;
using MonobitEngine;
using System.Collections.Generic;

public sealed class SpeakerObject : MonobitEngine.MonoBehaviour
{
	private Vector3 Position { get; set; }
	private Quaternion Rotation { get; set; }

	/// <summary>
	/// 
	/// </summary>
	public void Start()
	{
	}

	/// <summary>
	/// 
	/// </summary>
	public void Update()
	{
		transform.position = Position;
		transform.rotation = Rotation;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="info"></param>
	public void OnMonobitInstantiate(MonobitMessageInfo info)
	{
		Position = transform.position;
		Rotation = transform.rotation;

		foreach (SD_Unitychan_source_speaker speaker in GameObject.FindObjectsOfType<SD_Unitychan_source_speaker>())
		{
			if (speaker.monobitView.ownerId == info.sender.ID)
			{
				var voice = speaker.GetComponent<MultiSpeakerVoice>();
				if (voice == null) return;
				voice.SetSpeakerObject(gameObject, Position, Rotation);
				break;
			}
		}
	}
}
