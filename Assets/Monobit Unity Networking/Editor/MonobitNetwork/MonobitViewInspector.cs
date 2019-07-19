using System;
using UnityEngine;
using UnityEditor;

namespace MonobitEngine.Editor
{
    /**
     * MonobitView のInspector表示用クラス.
     */
    [CustomEditor(typeof(MonobitView))]
    [ExecuteInEditMode]
    public class MonobitViewInspector : UnityEditor.Editor
    {
        /** MonobitViewオブジェクト. */
        private MonobitView m_View = null;

        /** MonobitViewが保持するゲームオブジェクト. */
        private GameObject m_GameObject = null;

        /** MonobitViewのID. */
        private Int32 m_ViewID = 0;

        /** IDが他と重複するかどうかのフラグ. */
        private bool m_ViewIdOverlapped = false;

        /**
         * @brief   Inspector に追加されたときの処理.
         */
        public void Awake()
        {
            // 変数取得
            m_View = target as MonobitView;
            if (m_View == null)
            {
                return;
            }
            m_GameObject = m_View.gameObject;

            // シーン内のオブジェクトでIDが重複している場合、警告を促す
            m_ViewIdOverlapped = MonobitView.OverlappedSceneViewID(m_View.viewID);
        }

        /**
         * @brief   Inspector から削除されたときの処理.
         */
        public void OnDestroy()
        {
            // 変数取得
            m_View = target as MonobitView;
            if (m_View != null)
            {
                return;
            }

            if (m_GameObject != null)
            {
                bool bPrefab = EditorUtility.IsPersistent(m_GameObject);
                if (!bPrefab && m_ViewID != 0)
                {
                    // 0 以外が入っていた場合には MonobitView.RemoveSceneViewID() で自動削除
                    MonobitView.RemoveSceneViewID(m_ViewID);
                }
            }
        }

        /**
         * @brief   Inspector上のGUI表示.
         */
        public override void OnInspectorGUI()
        {
            // 変数取得
            m_View = target as MonobitView;
            if (m_View == null)
            {
                return;
            }

            bool bPrefab = EditorUtility.IsPersistent(m_View.gameObject);

            // 監視コンポーネントリストの初期化
            InitializeObservedComponentList();

            // 統括設定
            GeneralSettings(bPrefab);

            // 監視コンポーネントリストの設定
            try
            {
                ObservedComponentListSettings();
            }
            catch (Exception)
            {
            }

            // セーブ
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(m_View);
                if ( ! bPrefab ) MonobitEditor.MarkSceneDirty();
            }
        }

        /**
         * 監視コンポーネントリストの初期化.
         */
        private void InitializeObservedComponentList()
        {
            // 監視コンポーネントリストの生成
            if (m_View.ObservedComponents == null)
            {
                m_View.ObservedComponents = new System.Collections.Generic.List<Component>();
            }
        }

        /**
         * 統括設定.
         *
         * @param bPrefab プレハブかどうかのフラグ
         */
        private void GeneralSettings(bool bPrefab)
        {
            GUILayout.Space(5);

            // 標題の表示
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel = 2;

            // MonobitView ID の表示
            if (bPrefab)
            {
                EditorGUILayout.LabelField("MonobitView ID", "Decide at runtime");
                if ( 0 != m_View.viewID ) m_View.viewID = 0;
            }
            else if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("MonobitView ID", m_View.viewID.ToString());
            }
            else
            {
                // MonobitView ID の手動登録
                int viewID = (int)EditorGUILayout.IntField("MonobitView ID", m_View.viewID);
                if (viewID != m_View.viewID)
                {
                    MonobitView.RemoveSceneViewID(m_View.viewID);
                    m_View.viewID = MonobitView.AssignedSceneViewID(viewID);
                    m_ViewIdOverlapped = MonobitView.OverlappedSceneViewID(viewID);
                }

                // MonobitView ID の退避
                m_ViewID = m_View.viewID;

                // シーン内のオブジェクトでIDが重複している場合、警告を促す
                if (m_ViewIdOverlapped)
                {
                    EditorGUILayout.HelpBox("This \"MonobitView ID\" is stored in another object.", MessageType.Warning, true);
                }

                // 登録されたIDが不正である場合、エラーを出力する
                if (m_View.viewID < 0 || m_View.viewID >= MonobitNetwork.PERSONAL_VIEW_ID_MAX)
                {
                    EditorGUILayout.HelpBox("This \"MonobitView ID\" doesn't allowed as this value (must between 0 to 999).", MessageType.Error, true);
                }
            }

            // Owner の表示
            if (bPrefab)
            {
                EditorGUILayout.LabelField("Owner", "Decide at runtime");
            }
            else if (m_View.isSceneView)
            {
                EditorGUILayout.LabelField("Owner", "Scene(HostClient)");
            }
            else
            {
                MonobitPlayer player = MonobitPlayer.Find(m_View.ownerId);
                string playerInfo = (player != null) ? player.name : "<MonobitPlayer is not found>";

                if (string.IsNullOrEmpty(playerInfo))
                {
                    playerInfo = "<playername is not set>";
                }

                EditorGUILayout.LabelField("Owner", "[" + m_View.ownerId + "]" + playerInfo);
            }

            // Ownerの所有権委譲の設定
            GUI.enabled = !EditorApplication.isPlaying;
            m_View.ownershipTransfer = (MonobitEngineBase.OwnershipOption)EditorGUILayout.EnumPopup("Ownership Transfer", m_View.ownershipTransfer);
            GUI.enabled = true;

            // 制御権の表示
            if (EditorApplication.isPlaying && MonobitNetwork.player != null)
            {
                EditorGUILayout.Toggle("Enable Control" + (MonobitNetwork.isHost ? " <master>" : ""), m_View.isMine);
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.Toggle("Enable Control", true);
                GUI.enabled = true;
            }

            // 暗号化の有効/無効フラグ
            m_View.Encrypt = EditorGUILayout.Toggle("Enable Sync Encrypt Network", m_View.Encrypt);

            // 所有者がルーム退室時にオブジェクトを削除するかどうか
            if ( m_View.isInstantiated ){
                GUI.enabled = false;
                EditorGUILayout.Toggle("Is Dont Destroy On Room", m_View.isDontDestroyOnRoom);
                GUI.enabled = true;
            }
            
            EditorGUI.indentLevel = 0;

            GUILayout.Space(5);
        }

        /**
         * 監視コンポーネントリストの設定.
         */
        private void ObservedComponentListSettings()
        {
            GUILayout.Space(5);

            // 内部監視コンポーネントリストの破棄
            if(!EditorApplication.isPlaying)
            {
                if (m_View.InternalObservedComponents != null && m_View.gameObject.GetComponent<Monobit.Support.MonobitPlayerMoveTemplate>() == null)
                {
                    m_View.InternalObservedComponents.Clear();
                }
            }

            // コンポーネントリストのプロパティの取得
            SerializedProperty property = serializedObject.FindProperty("ObservedComponents");

            // 標題と追加の表示
            EditorGUILayout.LabelField("Observed Component Registration List", EditorStyles.boldLabel);

            GUI.enabled = !EditorApplication.isPlaying;
            EditorGUI.indentLevel = 2;

            // 追加ボタンの表示
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            if (GUILayout.Button("Add Observed Component List Column"))
            {
                property.InsertArrayElementAtIndex(m_View.ObservedComponents.Count);
            }
            GUILayout.EndHorizontal();

            // 各リスト項目と削除ボタンの表示
            for (int i = 0; i < property.arraySize; ++i)
            {
                GUILayout.BeginHorizontal();
                Rect rect = EditorGUILayout.GetControlRect(false, 18);
                EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(i), GUIContent.none);
                if (GUILayout.Button("Remove", GUILayout.Width(75.0f)))
                {
                    property.DeleteArrayElementAtIndex(i);
                }
                GUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = 0;
            GUI.enabled = true;

            GUILayout.Space(5);
        }
    }
}

