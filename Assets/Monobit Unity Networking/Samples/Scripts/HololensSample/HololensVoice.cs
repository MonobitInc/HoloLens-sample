using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonobitEngine;

public class HololensVoice : MonobitEngine.MunMonoBehaviour
{
    //プレイヤーアバター（Hololens）保存
    static HololensVoice self;
    public static HololensVoice Self { get { return self; } }

    private void Start()
    {
        if (monobitView.isMine)
        {
            self = this;
        }
    }

    public override void OnMonobitInstantiate(MonobitMessageInfo info)
    {
        //同期する座標はLocal座標なので、子Objectにすることで、親が合わせれば、同じ座標になる
        transform.parent = HololensSample.Instance.transform;
    }
}
