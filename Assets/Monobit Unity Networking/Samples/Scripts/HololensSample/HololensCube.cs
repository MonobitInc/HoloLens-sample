using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonobitEngine;

public class HololensCube : MonobitEngine.MunMonoBehaviour
{
    [SerializeField] float clickTime = 0.5f;    //クリックか掴むかの判定時間
    [SerializeField] float power = 3f;          //爆発のPower
    [SerializeField] float radius = 1f;         //爆発の半径

    float startTime;
    Rigidbody rd;

    static List<GameObject> selfCubes = new List<GameObject>();

    public static List<GameObject> SelfCubes { get { return selfCubes; } }

    // Start is called before the first frame update
    void Start()
    {
        rd = GetComponent<Rigidbody>();

        //自分が生成したオブジェクトだけ保存
        if (monobitView.isMine)
        {
            selfCubes.Add(gameObject);
        }
    }

    public override void OnMonobitInstantiate(MonobitMessageInfo info)
    {
        //同期する座標はLocal座標なので、子Objectにすることで、親が合わせれば、同じ座標になる
        transform.parent = HololensSample.Instance.transform;
    }

    public void OnPress()
    {
        startTime = Time.realtimeSinceStartup;
    }

    public void OnRelease()
    {
        //クリック判定したら、爆発を起こる
        if (Time.realtimeSinceStartup - startTime < clickTime)
        {
            monobitView.RPC("Explosion", MonobitTargets.All);
        }
    }

    //周囲のオブジェクトを爆発する
    [MunRPC]
    void Explosion()
    {
        //HololensSample.Instance.SetStateText("Explosion");

        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(power, explosionPos, radius, 0.5F);
            }
        }
    }

    public static void ClearSelf()
    {
        //削除する際にも、自分のオブジェクトだけ削除
        selfCubes.ForEach(c =>
        {
            if (c && c.gameObject)
            {
                MonobitNetwork.Destroy(c.gameObject);
            }
        });
        selfCubes.Clear();
    }

    private void OnDestroy()
    {
        selfCubes.Remove(gameObject);
    }
}
