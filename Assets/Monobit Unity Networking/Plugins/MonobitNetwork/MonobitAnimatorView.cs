using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif // UNITY_EDITOR

namespace MonobitEngine
{
    /// <summary>
    /// アニメーション同期制御クラス
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(MonobitView))]
    [AddComponentMenu("Monobit Networking/Monobit Animator View")]
    public class MonobitAnimatorView : MonoBehaviour
    {
        /// <summary>
        /// Mecanimのアニメーションデータ本体
        /// </summary>
        public Animator m_Animator = null;

#if UNITY_EDITOR
        /// <summary>
        /// Mecanimのアニメーションコントローラ本体
        /// </summary>
        public AnimatorController m_Controller = null;
#endif // UNITY_EDITOR

        /// <summary>
        /// アニメーションコントローラのパラメータ型定数.
        /// </summary>
        public enum AnimatorControllerParameterType
        {
            /// <summary>
            /// Float型.
            /// </summary>
            Float = 1,

            /// <summary>
            /// Int型.
            /// </summary>
            Int = 3,

            /// <summary>
            /// Bool型.
            /// </summary>
            Bool,

            /// <summary>
            /// Trigger型.
            /// </summary>
            Trigger = 9
        }

        /// <summary>
        /// 同期するアニメーションレイヤー情報
        /// </summary>
        [System.Serializable]
        public class AnimLayerInfo
        {
            /// <summary>
            /// アニメーションレイヤーのインデックス.
            /// </summary>
            public int m_Index;

            /// <summary>
            /// アニメーションレイヤー名.
            /// </summary>
            public string m_Name;

            /// <summary>
            /// 同期するかどうかのフラグ.
            /// </summary>
            public bool m_EnableSync;
        }

        /// <summary>
        /// 同期するアニメーションパラメータ情報. 
        /// </summary>
        [System.Serializable]
        public class AnimParamInfo
        {
            public AnimatorControllerParameterType m_Type;  /**< 同期するアニメーションパラメータの型. */
            public string m_Name;                           /**< アニメーションパラメータ名. */
            public bool m_EnableSync;                       /**< 同期するかどうかのフラグ. */
        }

        /* 同期データリスト. */

        /// <summary>
        /// 送受信対象となりうるアニメーションレイヤーのリスト.
        /// </summary>
        [SerializeField]
        private List<AnimLayerInfo> m_SyncAnimLayers = new List<AnimLayerInfo>();

        /// <summary>
        /// 送受信対象となりうるアニメーションパラメータのリスト.
        /// </summary>
        [SerializeField]
        private List<AnimParamInfo> m_SyncAnimParams = new List<AnimParamInfo>();

        /// <summary>
        /// 送受信対象となりうるアニメーションレイヤーのリストを取得します.
        /// </summary>
        public List<AnimLayerInfo> SyncAnimLayers
        {
            get
            {
                return m_SyncAnimLayers;
            }
        }

        /// <summary>
        /// 送受信対象となりうるアニメーションパラメータのリストを取得します.
        /// </summary>
        public List<AnimParamInfo> SyncAnimParams
        {
            get
            {
                return m_SyncAnimParams;
            }
        }

        /// <summary>
        /// MonobitAnimatorViewの同期データの読み書きを実行する
        /// </summary>
        /// <param name="stream">MonobitAnimatorViewの送信データ、または受信データのいずれかを提供するパラメータ</param>
        /// <param name="info">特定のメッセージやRPCの送受信、または更新に関する「送信者、対象オブジェクト、タイムスタンプ」などの情報を保有するパラメータ</param>
        public void OnMonobitSerializeView(MonobitStream stream, MonobitMessageInfo info)
        {
            if (this.m_Animator == null)
            {
                return;
            }

            if (stream.isWriting == true)
            {
                this.Serialize(stream);
            }
            else
            {
                this.Deserialize(stream);
            }
        }

        /// <summary>
        /// アニメーションレイヤー＆パラメータを送信データにシリアライズ
        /// </summary>
        /// <param name="stream">送信ストリーム情報</param>
        private void Serialize(MonobitStream stream)
        {
            // アニメーションレイヤーを送信データにシリアライズ
            foreach (AnimLayerInfo layerInfo in this.m_SyncAnimLayers)
            {
                if (layerInfo.m_EnableSync)
                {
                    stream.Enqueue(this.m_Animator.GetLayerWeight(layerInfo.m_Index));
                }
            }

            // アニメーションパラメータを送信データにシリアライズ
            foreach (AnimParamInfo paramInfo in this.m_SyncAnimParams)
            {
                if (paramInfo.m_EnableSync && !m_Animator.IsParameterControlledByCurve(paramInfo.m_Name))
                {
                    switch (paramInfo.m_Type)
                    {
                        case AnimatorControllerParameterType.Bool:
                            stream.Enqueue(this.m_Animator.GetBool(paramInfo.m_Name));
                            break;
                        case AnimatorControllerParameterType.Float:
                            stream.Enqueue(this.m_Animator.GetFloat(paramInfo.m_Name));
                            break;
                        case AnimatorControllerParameterType.Int:
                            stream.Enqueue(this.m_Animator.GetInteger(paramInfo.m_Name));
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// アニメーションレイヤー＆パラメータを受信データからデシリアライズ
        /// </summary>
        /// <param name="stream">アニメーションレイヤー＆パラメータを受信データからデシリアライズ</param>
        private void Deserialize(MonobitStream stream)
        {
            // アニメーションレイヤーを受信データからデシリアライズ
            foreach (AnimLayerInfo layerInfo in this.m_SyncAnimLayers)
            {
                if (layerInfo.m_EnableSync)
                {
                    this.m_Animator.SetLayerWeight(layerInfo.m_Index, (float)stream.Dequeue());
                }
            }

            // アニメーションパラメータを受信データからデシリアライズ
            foreach (AnimParamInfo paramInfo in this.m_SyncAnimParams)
            {
                if (paramInfo.m_EnableSync && !m_Animator.IsParameterControlledByCurve(paramInfo.m_Name))
                {
                    switch (paramInfo.m_Type)
                    {
                        case AnimatorControllerParameterType.Bool:
                            if (stream.Peek() is bool)
                            {
                                this.m_Animator.SetBool(paramInfo.m_Name, (bool)stream.Dequeue());
                            }
                            break;
                        case AnimatorControllerParameterType.Float:
                            if (stream.Peek() is float)
                            {
                                this.m_Animator.SetFloat(paramInfo.m_Name, (float)stream.Dequeue());
                            }
                            break;
                        case AnimatorControllerParameterType.Int:
                            if (stream.Peek() is int)
                            {
                                this.m_Animator.SetInteger(paramInfo.m_Name, (int)stream.Dequeue());
                            }
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            break;
                    }
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// アニメーションコントローラの取得
        /// </summary>
        /// <returns>アニメーションコントローラ</returns>
        public AnimatorController GetAnimController()
        {
            if (m_Animator == null)
            {
                return null;
            }
            RuntimeAnimatorController animController = m_Animator.runtimeAnimatorController;
            AnimatorOverrideController animOverrideController = animController as AnimatorOverrideController;
            if (animOverrideController != null)
            {
                animController = animOverrideController.runtimeAnimatorController;
            }

            return animController as AnimatorController;
        }

        /// <summary>
        /// アニメーションレイヤー情報を取得し、リストを更新する.
        /// </summary>
        public void UpdateAnimLayer()
        {
            // アニメーションレイヤーリストが現在のアニメーションレイヤーの数と一致しなければ、その項目を削除する
            for (int i = this.m_SyncAnimLayers.Count - 1; i >= 0; --i)
            {
                if (!this.IsExistAnimLayerName(this.m_SyncAnimLayers[i].m_Name))
                {
                    this.m_SyncAnimLayers.RemoveAt(i);
                }
            }

            // アニメーションレイヤーに対する追加と更新
            for (int i = 0; i < this.GetAnimLayerCount(); ++i)
            {
                string layerName = this.GetAnimLayerName(i);

                // アニメーションレイヤーに対する追加処理
                if (this.m_SyncAnimLayers.FindIndex(item => item.m_Name == layerName) == -1)
                {
                    this.m_SyncAnimLayers.Add(new AnimLayerInfo { m_Index = i, m_Name = this.GetAnimLayerName(i), m_EnableSync = true });
                }
                // アニメーションレイヤー情報の更新
                else
                {
                    this.m_SyncAnimLayers[i].m_Index = i;
                }
            }
        }

        /// <summary>
        /// アニメーションパラメータ情報を取得し、リストを更新する.
        /// </summary>
        public void UpdateAnimParameter()
        {
            // アニメーションパラメータリストが現在のアニメーションパラメータの数と一致しなければ、その項目を削除する
            for (int i = this.m_SyncAnimParams.Count - 1; i >= 0; --i)
            {
                if (!this.IsExistAnimParamName(this.m_SyncAnimParams[i].m_Name))
                {
                    this.m_SyncAnimParams.RemoveAt(i);
                }
            }

            // アニメーションパラメータに対する追加と更新
            for (int i = 0; i < this.GetAnimParamCount(); ++i)
            {
                AnimatorControllerParameter param = this.GetAnimParam(i);

                // アニメーションレイヤーに対する追加処理
                if (this.m_SyncAnimParams.FindIndex(item => item.m_Name == param.name) == -1)
                {
                    this.m_SyncAnimParams.Add(new AnimParamInfo { m_Type = (MonobitAnimatorView.AnimatorControllerParameterType)param.type, m_Name = param.name, m_EnableSync = true });
                }
                // アニメーションレイヤー情報の更新
                else
                {
                    this.m_SyncAnimParams[i].m_Type = (MonobitAnimatorView.AnimatorControllerParameterType)param.type;
                }
            }
        }

        /// <summary>
        /// アニメーションレイヤーの個数を取得する.
        /// </summary>
        /// <returns>アニメーションレイヤーの個数</returns>
        private int GetAnimLayerCount()
        {
            return (this.m_Controller != null) ? this.m_Controller.layers.Length : 0;
        }

        /// <summary>
        /// アニメーションレイヤーの名前を取得する
        /// </summary>
        /// <param name="index">アニメーションレイヤーのインデックス</param>
        /// <returns>アニメーションレイヤーの名前</returns>
        private string GetAnimLayerName(int index)
        {
            return (this.m_Controller != null) ? this.m_Controller.layers[index].name : null;
        }

        /// <summary>
        /// 指定した名前を持つアニメーションレイヤーが存在するかどうかを調べる
        /// </summary>
        /// <param name="name">アニメーションレイヤーの名前</param>
        /// <returns>そのアニメーションレイヤーが存在するならtrueを返す</returns>
        private bool IsExistAnimLayerName(string name)
        {
            for (int i = 0; i < this.GetAnimLayerCount(); ++i)
            {
                if (this.GetAnimLayerName(i) == name)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// アニメーションパラメータの個数を取得する.
        /// </summary>
        /// <returns>アニメーションパラメータの個数</returns>
        private int GetAnimParamCount()
        {
            return (this.m_Controller != null) ? this.m_Controller.parameters.Length : 0;
        }

        /// <summary>
        /// アニメーションパラメータの詳細情報を取得する
        /// </summary>
        /// <param name="paramIndex">アニメーションパラメータのインデックス</param>
        /// <returns>アニメーションパラメータ情報</returns>
        private AnimatorControllerParameter GetAnimParam(int paramIndex)
        {
            return this.m_Controller.parameters[paramIndex];
        }

        /// <summary>
        /// 指定した名前を持つアニメーションパラメータが存在するかどうかを調べる
        /// </summary>
        /// <param name="name">アニメーションパラメータの名前</param>
        /// <returns>そのアニメーションパラメータが存在するなら true を返す</returns>
        private bool IsExistAnimParamName(string name)
        {
            for (int i = 0; i < this.GetAnimParamCount(); ++i)
            {
                if (this.GetAnimParam(i).name == name)
                {
                    return true;
                }
            }

            return false;
        }
#endif // UNITY_EDITOR
    }
}
