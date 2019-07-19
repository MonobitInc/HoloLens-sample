using UnityEngine;
using System;
using System.Collections;
using MonobitEngine;
using MonobitEngine.Definitions;

public class SD_Unitychan_generic_PC : MonobitEngine.MonoBehaviour
{
    private Animator animator;                  // アニメータコントローラ
    private int animId = 0;                     // 再生中のアニメーションID
	private bool isMainCameraDisabled = false;	// メインカメラ復旧用フラグ

    private int serializeReadCount = 0;                 // シリアライズ読み込みカウンタ
    private byte[] serializeBytes = new byte[ 1 ]{ 0 }; // シリアライズ対象バイト配列

    void Awake()
    {
//        monobitView.compressedStream = MonobitEngineBase.CompressedStream.DeltaCompressed;
        if ( null == monobitView.instantiationData ){
            UnityEngine.Debug.Log( monobitView +" instantiationData is null" );
        }else{
            UnityEngine.Debug.Log( monobitView +" instantiaionData="+ monobitView.instantiationData[ 0 ] );
        }
    }

    // Use this for initialization
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        animId = Animator.StringToHash("animId");

		if (!monobitView.isMine)
        {
            gameObject.transform.Find("Camera").GetComponent<Camera>().enabled = false;
            gameObject.transform.Find("Camera").GetComponent<AudioListener>().enabled = false;
        }
        else
        {
            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = false;
            GameObject.Find("Main Camera").GetComponent<AudioListener>().enabled = false;
            isMainCameraDisabled = true;
		}
    }

	void OnDestroy()
	{
		if( isMainCameraDisabled )
		{
            GameObject go = GameObject.Find("Main Camera");
            if( go != null )
            {
                go.GetComponent<Camera>().enabled = true;
            }
        }
	}

    // Update is called once per frame
    public void Update()
    {
		if (monobitView.isOwner)
        {
            // キャラクタの移動＆アニメーション切り替え
            if (Input.GetKey("up"))
            {
                gameObject.transform.position += gameObject.transform.forward * 0.1f;
                animator.SetInteger(animId, 1);
            }
            else
            {
                animator.SetInteger(animId, 0);
            }
            if (Input.GetKey("right"))
            {
                gameObject.transform.Rotate(0, 2.0f, 0);
            }
            if (Input.GetKey("left"))
            {
                gameObject.transform.Rotate(0, -2.0f, 0);
            }
            if (Input.GetKeyDown("z"))
            {
                MonobitNetwork.Instantiate("Cube", transform.position, transform.rotation, 0);
            }
            if (Input.GetKeyDown("s"))
            {
                MonobitNetwork.Instantiate("Cube", transform.position, transform.rotation, 0, null, true, false, true);
            }
            if (Input.GetKeyDown("e"))
            {
                MonobitNetwork.Instantiate("Cube", transform.position, transform.rotation, 0, null, true, false, false);
            }
            if (Input.GetKeyDown("d"))
            {
                UnityEngine.Debug.Log( "Destroy Cube Start" );
                foreach ( GameObject go in FindObjectsOfType( typeof( GameObject ) ) ){
                    MonobitView view = go.GetComponent< MonobitView >();
                    if ( null == view ) continue;
                    if ( "Cube(Clone)" != go.name ) continue;
                    
                    MonobitNetwork.Destroy( go );
                    if ( ! view.enabled ) UnityEngine.Debug.Log( "Destroy Cube: "+ view );
                }
                UnityEngine.Debug.Log( "Destroy Cube End" );
            }
            if (Input.GetKeyDown("r"))
            {
                UnityEngine.Debug.Log( "RequestOwnership Cube Start" );
                foreach ( GameObject go in FindObjectsOfType( typeof( GameObject ) ) ){
                    MonobitView view = go.GetComponent< MonobitView >();
                    if ( null == view ) continue;
                    if ( "Cube(Clone)" != go.name ) continue;
                    
                    view.RequestOwnership();
                    UnityEngine.Debug.Log( "RequestOwnership Cube: "+ view );
                }
                UnityEngine.Debug.Log( "RequestOwnership Cube End" );
            }
            if (Input.GetKeyDown("t"))
            {
                UnityEngine.Debug.Log( "TransferOwnership Cube Start" );
                int playerId = MonobitNetwork.player.ID;
                foreach ( GameObject go in FindObjectsOfType( typeof( GameObject ) ) ){
                    MonobitView view = go.GetComponent< MonobitView >();
                    if ( null == view ) continue;
                    if ( "Cube(Clone)" != go.name ) continue;
                    
                    view.TransferOwnership( playerId );
                    UnityEngine.Debug.Log( "TransferOwnership Cube: "+ view );
                }
                UnityEngine.Debug.Log( "TransferOwnership Cube End" );
            }
            if (Input.GetKeyDown("i"))
            {
                UnityEngine.Debug.Log( "Change IsDontDestroyOnRoom Cube Start" );
                foreach ( GameObject go in FindObjectsOfType( typeof( GameObject ) ) ){
                    MonobitView view = go.GetComponent< MonobitView >();
                    if ( null == view ) continue;
                    if ( "Cube(Clone)" != go.name ) continue;
                    
                    bool isDontDestroyOnRoom = view.isDontDestroyOnRoom;
                    view.isDontDestroyOnRoom = ! isDontDestroyOnRoom;
                    if ( isDontDestroyOnRoom != view.isDontDestroyOnRoom ) UnityEngine.Debug.Log( "Change IsDontDestroyOnRoom Cube: "+ view +" "+ isDontDestroyOnRoom +" -> "+ view.isDontDestroyOnRoom );
                }
                UnityEngine.Debug.Log( "Change IsDontDestroyOnRoom Cube End" );
            }
            if ( Input.GetKeyDown( "0" ) )
            {
                serializeBytes[ 0 ] = 0;
            }
            if ( Input.GetKeyDown( "1" ) )
            {
                serializeBytes[ 0 ] = 1;
            }
        }
    }
    
    public void OnMonobitInstantiate(MonobitMessageInfo info)
    {
        UnityEngine.Debug.Log( "OnMonobitInstantiate sender="+ info.sender +" view="+ info.monobitView );
    }
    
    public void OnMonobitSerializeView(MonobitStream stream, MonobitMessageInfo info)
    {
        if ( stream.isWriting ){
            stream.Enqueue( serializeBytes );
        }else{
            serializeBytes = (byte[])stream.Dequeue();
            
            if ( MonobitEngineBase.CompressedStream.DeltaCompressed == monobitView.compressedStream ){
                ++serializeReadCount;
                UnityEngine.Debug.Log( "serializeBytes[0]="+ serializeBytes[0] +" serializeReadCount="+ serializeReadCount );
            }
        }
    }
}