using UnityEditor;
using UnityEngine;
using System.Collections;
using MonobitEngine.Editor;

namespace Monobit.Support.Editor
{
	/**
     * MonobitAutoLoginTemplate のInspector表示用クラス.
     */
	[CustomEditor(typeof(MonobitAutoLoginTemplate))]
	public class MonobitAutoLoginTemplateEditor : UnityEditor.Editor
	{
		/** MonobitAutoLoginTemplate 本体. */
		MonobitAutoLoginTemplate obj;
		
		/**
         * Inspector上のGUI表示.
         */
		public override void OnInspectorGUI()
		{
			GUILayout.Space(5);
			
			// 本体の取得
			obj = target as MonobitAutoLoginTemplate;
			
			// 標題の表示
			EditorGUILayout.LabelField("Instantiate Player", EditorStyles.boldLabel);
			
			EditorGUI.indentLevel = 2;
			
			// プレハブの登録
			obj.InstantiatePrefab = EditorGUILayout.ObjectField("Prefab", obj.InstantiatePrefab, typeof(GameObject), false) as GameObject;
			
			// 登録したプレハブが Resources 内に存在するかどうかを調べる
			if (obj.InstantiatePrefab != null)
			{
				GameObject tmp = Resources.Load(obj.InstantiatePrefab.name, typeof(GameObject)) as GameObject;
				if (tmp == null)
				{
					EditorGUILayout.HelpBox("This Prefab is not included in the 'Resources' folder .", MessageType.Warning, true);
				}
			}
			
			// 座標・回転量を入力
			obj.camPosition = EditorGUILayout.Vector3Field("cam position", obj.camPosition);
			Vector3 camRotation = obj.camRotation.eulerAngles;
			camRotation = EditorGUILayout.Vector3Field("cam rotation", camRotation);
			obj.camRotation.eulerAngles = camRotation;
			
			EditorGUI.indentLevel = 0;
			GUILayout.Space(5);
			
			// データの更新
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(obj);
                MonobitEditor.MarkSceneDirty();
			}
		}
	}
}
