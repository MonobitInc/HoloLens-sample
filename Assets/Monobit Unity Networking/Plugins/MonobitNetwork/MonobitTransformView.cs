using UnityEngine;
using System.Collections.Generic;

namespace MonobitEngine
{
    /**
     * オブジェクトの位置・姿勢・倍率の同期クラス.
     */
    [RequireComponent(typeof(MonobitView))]
    [AddComponentMenu("Monobit Networking/Monobit Transform View")]
    public class MonobitTransformView : MonoBehaviour
    {
        /** 同一オブジェクトにアタッチされている MonobitView. */
        private MonobitView m_MonobitView;

        /**
         * 同期パラメータ情報.
         */
        [System.Serializable]
        public class SynchronizedInfo
        {
            public bool m_EnableSync;           /**< 同期するかどうかのフラグ. */
            public bool m_EnableInterpolate;    /**< 補間させるかどうかのフラグ. */
            public float m_LerpRate;            /**< 線形補間係数. */
        };

        /* 同期パラメータ情報. */
        [SerializeField]
        public SynchronizedInfo m_SyncPosition = new SynchronizedInfo { m_EnableSync = true, m_EnableInterpolate = true, m_LerpRate = 10.0f };     /**< 位置の同期パラメータ情報. */
        [SerializeField]
        public SynchronizedInfo m_SyncRotation = new SynchronizedInfo { m_EnableSync = true, m_EnableInterpolate = false, m_LerpRate = 5.0f };     /**< 姿勢の同期パラメータ情報. */
        [SerializeField]
        public SynchronizedInfo m_SyncScale = new SynchronizedInfo { m_EnableSync = false, m_EnableInterpolate = false, m_LerpRate = 1.0f };     /**< 倍率の同期パラメータ情報. */

        /* 大幅に離れてしまった場合のワープ処理. */
        [SerializeField]
        public bool m_SnapEnabled = true;           /**< 一定距離以上離れてしまった場合ワープさせるかどうかのフラグ. */
        [SerializeField]
        public float m_SnapThreshold = 3.0f;        /**< ワープさせる場合の距離閾値. */

        /** ネットワークの更新情報を受信したかどうかのフラグ. */
        private bool m_UpdateNetwork = false;

        /* 受信した情報 */
        private Vector3 m_LastUpdatePosition = Vector3.zero;               /**< 受信した最新の位置情報. */
        private Quaternion m_LastUpdateRotation = Quaternion.identity;     /**< 受信した最新の姿勢情報. */
        private Vector3 m_LastUpdateScale = Vector3.one;                   /**< 受信した最新の倍率情報. */

        /**
         * 起動関数.
         */
        void Awake()
        {
            m_MonobitView = GetComponent<MonobitView>();
        }

        /**
         * 更新関数.
         */
        void Update()
        {
            if (! m_UpdateNetwork || m_MonobitView.isMine)
            {
                return;
            }

            UpdatePosition();
            UpdateRotation();
            UpdateScale();
        }

        /**
         * 位置情報の更新.
         */
        private void UpdatePosition()
        {
            if (m_SyncPosition.m_EnableSync)
            {
                if(!m_SyncPosition.m_EnableInterpolate)
                {
                    gameObject.transform.localPosition = m_LastUpdatePosition;
                }
                else
                {
                    gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, m_LastUpdatePosition, m_SyncPosition.m_LerpRate * Time.deltaTime);
                }
                if (m_SnapEnabled && (Vector3.Distance(gameObject.transform.localPosition, m_LastUpdatePosition) > m_SnapThreshold))
                {
                    gameObject.transform.localPosition = m_LastUpdatePosition;
                }
            }
        }

        /**
         * 姿勢情報の更新.
         */
        private void UpdateRotation()
        {
            if (m_SyncRotation.m_EnableSync)
            {
                if (!m_SyncRotation.m_EnableInterpolate)
                {
                    gameObject.transform.localRotation = m_LastUpdateRotation;
                }
                else
                {
                    gameObject.transform.localRotation = Quaternion.Lerp(gameObject.transform.localRotation, m_LastUpdateRotation, m_SyncRotation.m_LerpRate * Time.deltaTime);
                }
            }
        }

        /**
         * 倍率情報の更新.
         */
        private void UpdateScale()
        {
            if (m_SyncScale.m_EnableSync)
            {
                if (!m_SyncScale.m_EnableInterpolate)
                {
                    gameObject.transform.localScale = m_LastUpdateScale;
                }
                else
                {
                    gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, m_LastUpdateScale, m_SyncScale.m_LerpRate * Time.deltaTime);
                }
            }
        }

#if false
        /**
         * オブジェクトの位置・姿勢・倍率の同期処理関数.
         *
         * @param stream MonobitTransformViewの送信データ、または受信データのいずれかを提供するパラメータ
         * @param info 特定のメッセージやRPCの送受信、または更新に関する「送信者、対象オブジェクト、タイムスタンプ」などの情報を保有するパラメータ
         */
        public void OnMonobitSerializeView( MonobitStream stream, MonobitMessageInfo info )
        {
            if ( stream.isWriting ){
                OnMonobitSerializeViewWrite( stream, info );
            }else{
                OnMonobitSerializeViewRead( stream, info );
            }
        }
#endif
        
        /**
         * 書き込み専用オブジェクトの位置・姿勢・倍率の同期処理関数.
         *
         * @param stream MonobitTransformViewの送信データ、または受信データのいずれかを提供するパラメータ
         * @param info 特定のメッセージやRPCの送受信、または更新に関する「送信者、対象オブジェクト、タイムスタンプ」などの情報を保有するパラメータ
         */
        public override void OnMonobitSerializeViewWrite( MonobitStream stream, MonobitMessageInfo info )
        {
            // ストリームへの送信処理、および、自身の座標を最新のtransform情報に更新
            Transform transform = gameObject.transform; // gameObjectを参照するコストが重いので、1回にまとめる
            if ( m_SyncPosition.m_EnableSync )
            {
                stream.Enqueue(transform.localPosition);
            }
            if (m_SyncRotation.m_EnableSync)
            {
                stream.Enqueue(transform.localRotation);
            }
            if (m_SyncScale.m_EnableSync)
            {
                stream.Enqueue(transform.localScale);
            }
        }
        
        /**
         * 読み込み専用オブジェクトの位置・姿勢・倍率の同期処理関数.
         *
         * @param stream MonobitTransformViewの送信データ、または受信データのいずれかを提供するパラメータ
         * @param info 特定のメッセージやRPCの送受信、または更新に関する「送信者、対象オブジェクト、タイムスタンプ」などの情報を保有するパラメータ
         */
        public override void OnMonobitSerializeViewRead( MonobitStream stream, MonobitMessageInfo info )
        {
            // ネットワークからの更新を受信したことをフラグで検知
            m_UpdateNetwork = true;

            // ストリームからの受信処理
            if( m_SyncPosition.m_EnableSync )
            {
                m_LastUpdatePosition = (Vector3)stream.Dequeue();
            }
            if (m_SyncRotation.m_EnableSync)
            {
                m_LastUpdateRotation = (Quaternion)stream.Dequeue();
            }
            if (m_SyncScale.m_EnableSync)
            {
                m_LastUpdateScale = (Vector3)stream.Dequeue();
            }
        }
    }
}
