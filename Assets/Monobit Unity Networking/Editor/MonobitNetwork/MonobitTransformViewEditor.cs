using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MonobitEngine.Editor
{
    /**
     * MonobitTransformView のインスペクタ表示用クラス.
     */
    [CustomEditor(typeof(MonobitTransformView))]
    public class MonobitTransformViewEditor : UnityEditor.Editor
    {
        /** MonobitTransformView本体. */
        private MonobitTransformView m_View = null;

        /**
         * Inspector上のGUI表示.
         */
        public override void OnInspectorGUI()
        {
            // 変数の初期化
            m_View = this.target as MonobitTransformView;
            if( m_View == null )
            {
                return;
            }

            // Transformの同期に関する設定の表示
            SyncSetting("Position", ref m_View.m_SyncPosition);
            SyncSetting("Rotation", ref m_View.m_SyncRotation);
            SyncSetting("Scale",    ref m_View.m_SyncScale);

            // ワープに関する設定の表示
            SnapSetting();

            // データの更新
            if ( GUI.changed )
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(m_View);
                MonobitEditor.MarkSceneDirty();
            }
        }

        /**
         * Transformの同期に関する設定の表示.
         *
         * @param title     標題に表示させる項目名
         * @param syncInfo  同期に関するパラメータ情報
         */
        private void SyncSetting(string title, ref MonobitTransformView.SynchronizedInfo syncInfo)
        {
            GUILayout.Space(5);
            GUI.enabled = !EditorApplication.isPlaying;

            // 標題の表示
            EditorGUILayout.LabelField(title + " Configure", EditorStyles.boldLabel);

            EditorGUI.indentLevel = 2;

            // 位置同期をさせるかどうかのフラグの設定
            syncInfo.m_EnableSync = EditorGUILayout.Toggle("Enable Sync", syncInfo.m_EnableSync);

            // 補間処理をするかどうかのフラグ設定
            if (syncInfo.m_EnableSync)
            {
                syncInfo.m_EnableInterpolate = EditorGUILayout.Toggle("Enable Interpolate", syncInfo.m_EnableInterpolate);
            }
            else
            {
                EditorGUILayout.LabelField("Enable Interpolate", "Disable");
            }

            // 同期時の線形補間係数の設定
            if (syncInfo.m_EnableInterpolate)
            {
                syncInfo.m_LerpRate = EditorGUILayout.FloatField("Lerp Rate [bigger than 0]", syncInfo.m_LerpRate);
            }
            else
            {
                EditorGUILayout.LabelField("Lerp Rate [bigger than 0]", "Disable");
            }

            EditorGUI.indentLevel = 0;
            GUI.enabled = true;
            GUILayout.Space(5);
        }

        /**
         * ワープに関する設定の表示.
         */
        private void SnapSetting()
        {
            GUILayout.Space(5);
            GUI.enabled = !EditorApplication.isPlaying;

            // 標題の表示
            EditorGUILayout.LabelField("Movement Snap(Warp)", EditorStyles.boldLabel);

            EditorGUI.indentLevel = 2;

            // ワープさせるかどうかのフラグの設定
            m_View.m_SnapEnabled = EditorGUILayout.Toggle("Enable Snap(Warp)", m_View.m_SnapEnabled);

            // 補間処理をするかどうかのフラグ設定
            if (m_View.m_SnapEnabled)
            {
                m_View.m_SnapThreshold = EditorGUILayout.FloatField("Snap(Warp) Threshold", m_View.m_SnapThreshold);
            }
            else
            {
                EditorGUILayout.LabelField("Snap(Warp) Threshold", "Disable");
            }

            EditorGUI.indentLevel = 0;
            GUI.enabled = true;
            GUILayout.Space(5);
        }
    }
}
