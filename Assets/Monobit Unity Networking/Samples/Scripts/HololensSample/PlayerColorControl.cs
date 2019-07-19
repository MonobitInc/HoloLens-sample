using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonobitEngine;

public class PlayerColorControl : MonobitEngine.MunMonoBehaviour
{
    public override void OnMonobitInstantiate(MonobitMessageInfo info)
    {
        //プレイヤーパラメータの情報でオブジェクトの色を設定する
        float r = (float)info.sender.customParameters[HololensSample.PLAYER_COLOR_R];
        float g = (float)info.sender.customParameters[HololensSample.PLAYER_COLOR_G];
        float b = (float)info.sender.customParameters[HololensSample.PLAYER_COLOR_B];

        Color c = new Color(r, g, b);

        MeshRenderer[] rs = GetComponentsInChildren<MeshRenderer>();
        System.Array.ForEach(rs, _ =>
        {
            _.material.color = c;
        });

        //Debug.Log("OnMonobitInstantiate: " + info.sender.ID + " c: " + c);
    }
}
