using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;

namespace MonobitEngine.Editor
{
    /**
     * MonobitAnimatorView のインスペクタ表示用クラス.
     */
    [CustomEditor(typeof(MonobitAnimatorView))]
    public class MonobitAnimatorViewEditor : UnityEditor.Editor
    {
        /** MonobitAnimatorView 本体. */
        private MonobitAnimatorView m_View = null;

        /**
         * Inspector上のGUI表示.
         */
        public override void OnInspectorGUI()
        {
            // 変数の初期化
            this.m_View = this.target as MonobitAnimatorView;
            this.m_View.m_Animator = m_View.GetComponent<Animator>();
            this.m_View.m_Controller = m_View.GetAnimController();
            if (this.m_View == null)
            {
                return;
            }
            if (this.m_View.m_Animator == null)
            {
                EditorGUILayout.HelpBox("It doesn't have an Animator Component.", MessageType.Warning, true);
                return;
            }
            if (this.m_View.m_Controller == null)
            {
                EditorGUILayout.HelpBox("It doesn't have an Animator Controller in Animator Component.", MessageType.Warning, true);
                return;
            }

            // アニメーションレイヤー情報の更新
            m_View.UpdateAnimLayer();

            // アニメーションパラメータ情報の更新
            m_View.UpdateAnimParameter();

            // アニメーションレイヤーの設定
            AnimLayerSetting();

            // アニメーションパラメータの設定
            AnimParamSetting();

            // データの更新
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(m_View);
                MonobitEditor.MarkSceneDirty();
            }
        }

        /**
         * アニメーションレイヤーの設定.
         */
        private void AnimLayerSetting()
        {
            GUILayout.Space(5);
            GUI.enabled = !EditorApplication.isPlaying;

            // 標題の表示
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Animation Layer Configure", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Enable Sync");
            EditorGUILayout.EndHorizontal();


            // アニメーションレイヤーの各項目に対する表示
            foreach (MonobitAnimatorView.AnimLayerInfo layerInfo in m_View.SyncAnimLayers)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel = 2;
                EditorGUILayout.LabelField(layerInfo.m_Name);
                EditorGUI.indentLevel = 0;
                layerInfo.m_EnableSync = EditorGUILayout.Toggle(layerInfo.m_EnableSync);
                EditorGUILayout.EndHorizontal();
            }

            GUI.enabled = true;
            GUILayout.Space(5);
        }

        /**
         * アニメーションパラメータに関する情報を表示.
         */
        private void AnimParamSetting()
        {
            GUILayout.Space(5);
            GUI.enabled = !EditorApplication.isPlaying;

            // 標題の表示
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Animation Parameter Configure", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Enable Sync");
            EditorGUILayout.EndHorizontal();

            // アニメーションパラメータの各項目に対する表示
            foreach (MonobitAnimatorView.AnimParamInfo paramInfo in m_View.SyncAnimParams)
            {
                EditorGUILayout.BeginHorizontal();
                switch (paramInfo.m_Type)
                {
                    case MonobitAnimatorView.AnimatorControllerParameterType.Bool:
                        EditorGUI.indentLevel = 2;
                        EditorGUILayout.LabelField("[Bool] " + paramInfo.m_Name);
                        EditorGUI.indentLevel = 0;
                        paramInfo.m_EnableSync = EditorGUILayout.Toggle(paramInfo.m_EnableSync);
                        break;
                    case MonobitAnimatorView.AnimatorControllerParameterType.Float:
                        EditorGUI.indentLevel = 2;
                        EditorGUILayout.LabelField("[Float] " + paramInfo.m_Name);
                        EditorGUI.indentLevel = 0;
                        paramInfo.m_EnableSync = EditorGUILayout.Toggle(paramInfo.m_EnableSync);
                        break;
                    case MonobitAnimatorView.AnimatorControllerParameterType.Int:
                        EditorGUI.indentLevel = 2;
                        EditorGUILayout.LabelField("[Int] " + paramInfo.m_Name);
                        EditorGUI.indentLevel = 0;
                        paramInfo.m_EnableSync = EditorGUILayout.Toggle(paramInfo.m_EnableSync);
                        break;
                    case MonobitAnimatorView.AnimatorControllerParameterType.Trigger:
                        EditorGUI.indentLevel = 2;
                        EditorGUILayout.LabelField("[Trigger] " + paramInfo.m_Name);
                        EditorGUI.indentLevel = 0;
                        EditorGUILayout.LabelField("(not supported)");
                        paramInfo.m_EnableSync = false;
                        break;
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.enabled = true;
            GUILayout.Space(5);
        }
    }
}
