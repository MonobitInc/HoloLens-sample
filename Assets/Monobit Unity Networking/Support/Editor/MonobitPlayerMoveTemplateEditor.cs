using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;
using MonobitEngine;
using UnityEditor;
using MonobitEngine.Editor;

namespace Monobit.Support.Editor
{
	/**
     * MonobitPlayerMoveTemplate のInspector表示用クラス.
     */
	[CustomEditor(typeof(MonobitPlayerMoveTemplate))]
	public class MonobitPlayerMoveTemplateEditor : UnityEditor.Editor
	{
        /** MonobitPlayerMoveTemplate 本体. */
        MonobitPlayerMoveTemplate obj;
		
		/** MonobitView 本体. */
		MonobitView view;
		
		/** MonobitAnimatorView 本体. */
		MonobitAnimatorView animView;
		
		/**
         *
         */
		void AddMonobitObserverdComponent()
		{
			if( view == null )
			{
				view = obj.gameObject.GetComponent<MonobitView>();
			}
			if ( view != null )
			{
				if (view.InternalObservedComponents == null)
				{
					view.InternalObservedComponents = new List<Component>();
				}
				else
				{
					if ( view.InternalObservedComponents.FindAll(item => item != null && item.GetType() == typeof(MonobitTransformView)).Count == 0 )
					{
						view.InternalObservedComponents.Add(obj.gameObject.GetComponent<MonobitTransformView>());
					}
					if (view.InternalObservedComponents.FindAll(item => item != null && item.GetType() == typeof(MonobitAnimatorView)).Count == 0)
					{
						view.InternalObservedComponents.Add(obj.gameObject.GetComponent<MonobitAnimatorView>());
					}
				}
			}
		}

		/**
         * Inspector上のGUI表示.
         */
		public override void OnInspectorGUI()
		{
			// 本体の取得
			obj = target as MonobitPlayerMoveTemplate;
			if (!EditorApplication.isPlaying)
			{
				// MonobitObservedComponentにMonobitTransformViewとMonobitAnimatorViewを自動追加
				AddMonobitObserverdComponent();
				
				// キー操作とアニメーションパラメータの登録
				obj.EntryKeyAndAnim();
			}
			
			GUILayout.Space(5);

			// 標題と追加の表示
			EditorGUILayout.LabelField("Key And Anim Settings List", EditorStyles.boldLabel);
			
			GUI.enabled = !EditorApplication.isPlaying;
			EditorGUI.indentLevel = 2;
			
			// 各リスト項目と削除ボタンの表示
			for (int i = 0; i < obj.KeyAndAnimSettings.Count; ++i)
			{
				obj.KeyAndAnimSettings[i].keyCode = (MonobitPlayerMoveTemplate.KeyCode)EditorGUILayout.EnumPopup("Key Assign", obj.KeyAndAnimSettings[i].keyCode);
				if (obj.KeyAndAnimSettings[i].keyCode == MonobitPlayerMoveTemplate.KeyCode.Horizontal || obj.KeyAndAnimSettings[i].keyCode == MonobitPlayerMoveTemplate.KeyCode.Vertical)
				{
					obj.KeyAndAnimSettings[i].axisAction = (MonobitPlayerMoveTemplate.AxisAction)EditorGUILayout.EnumPopup("Axis Action", obj.KeyAndAnimSettings[i].axisAction);
				}
				else
				{
					obj.KeyAndAnimSettings[i].buttonAction = (MonobitPlayerMoveTemplate.ButtonAction)EditorGUILayout.EnumPopup("Button Action", obj.KeyAndAnimSettings[i].buttonAction);
				}
				
				obj.KeyAndAnimSettings[i].actionType = (MonobitPlayerMoveTemplate.ActionType)EditorGUILayout.EnumPopup("Action Type", obj.KeyAndAnimSettings[i].actionType);
				switch (obj.KeyAndAnimSettings[i].actionType)
				{
				case MonobitPlayerMoveTemplate.ActionType.Move:
				{
					obj.KeyAndAnimSettings[i].Position = EditorGUILayout.Vector3Field("Position Increase", obj.KeyAndAnimSettings[i].Position);
				}
					break;
				case MonobitPlayerMoveTemplate.ActionType.Rotate:
				{
					obj.KeyAndAnimSettings[i].Rotation = EditorGUILayout.Vector3Field("Rotation Increase", obj.KeyAndAnimSettings[i].Rotation);
				}
					break;
				case MonobitPlayerMoveTemplate.ActionType.ChangeAnimLayerWeight:
				{
					List<string> name = new List<string>();
					foreach (var layer in obj.KeyAndAnimSettings[i].layerInfo)
					{
						name.Add(layer.m_Name);
					}
					obj.KeyAndAnimSettings[i].SelectLayer = EditorGUILayout.Popup("Select Anim Layer", obj.KeyAndAnimSettings[i].SelectLayer, name.ToArray());
					var selected = obj.KeyAndAnimSettings[i].layerInfo[obj.KeyAndAnimSettings[i].SelectLayer];
					selected.m_animWeight = EditorGUILayout.FloatField("Anim Weight[" + selected.m_Name + "]", selected.m_animWeight);
				}
					break;
				case MonobitPlayerMoveTemplate.ActionType.ChangeAnimParam:
				{
					List<string> name = new List<string>();
					foreach (var param in obj.KeyAndAnimSettings[i].paramInfo)
					{
						name.Add(param.m_Name);
					}
					obj.KeyAndAnimSettings[i].SelectParam = EditorGUILayout.Popup("Select Anim Param", obj.KeyAndAnimSettings[i].SelectParam, name.ToArray());
					
					var selected = obj.KeyAndAnimSettings[i].paramInfo[obj.KeyAndAnimSettings[i].SelectParam];
					switch (selected.m_Type)
					{
					case MonobitAnimatorView.AnimatorControllerParameterType.Bool:
						selected.m_boolValue = EditorGUILayout.Toggle("Anim Flag[" + selected.m_Name + "]", selected.m_boolValue);
						break;
					case MonobitAnimatorView.AnimatorControllerParameterType.Float:
						selected.m_floatValue = EditorGUILayout.FloatField("Anim Value[" + selected.m_Name + "]", selected.m_floatValue);
						break;
					case MonobitAnimatorView.AnimatorControllerParameterType.Int:
						selected.m_intValue = EditorGUILayout.IntField("Anim Value[" + selected.m_Name + "]", selected.m_intValue);
						break;
					case MonobitAnimatorView.AnimatorControllerParameterType.Trigger:
						break;
					}
				}
					break;
				case MonobitPlayerMoveTemplate.ActionType.Instantiate:
				{
					obj.KeyAndAnimSettings[i].instantiatePrefab = EditorGUILayout.ObjectField("Prefab", obj.KeyAndAnimSettings[i].instantiatePrefab, typeof(GameObject), false) as GameObject;
					
					// 登録したプレハブが Resources 内に存在するかどうかを調べる
					if (obj.KeyAndAnimSettings[i].instantiatePrefab != null)
					{
						GameObject tmp = Resources.Load(obj.KeyAndAnimSettings[i].instantiatePrefab.name, typeof(GameObject)) as GameObject;
						if (tmp == null)
						{
							EditorGUILayout.HelpBox("This Prefab is not included in the 'Resources' folder .", MessageType.Warning, true);
						}
					}
					
					obj.KeyAndAnimSettings[i].instantiateType = (MonobitPlayerMoveTemplate.InstantiateType)EditorGUILayout.EnumPopup("Instantiate Type", obj.KeyAndAnimSettings[i].instantiateType);
					switch (obj.KeyAndAnimSettings[i].instantiateType)
					{
					case MonobitPlayerMoveTemplate.InstantiateType.Absolute:
					{
						obj.KeyAndAnimSettings[i].Position = EditorGUILayout.Vector3Field("Absolute Position", obj.KeyAndAnimSettings[i].Position);
						obj.KeyAndAnimSettings[i].Rotation = EditorGUILayout.Vector3Field("Absolute Rotation", obj.KeyAndAnimSettings[i].Rotation);
					}
						break;
					case MonobitPlayerMoveTemplate.InstantiateType.Relative:
					{
						obj.KeyAndAnimSettings[i].Position = EditorGUILayout.Vector3Field("Relative Position", obj.KeyAndAnimSettings[i].Position);
						obj.KeyAndAnimSettings[i].Rotation = EditorGUILayout.Vector3Field("Relative Rotation", obj.KeyAndAnimSettings[i].Rotation);
					}
						break;
					case MonobitPlayerMoveTemplate.InstantiateType.RandomAbsolute:
					{
						obj.KeyAndAnimSettings[i].PositionMin = EditorGUILayout.Vector3Field("Min Position", obj.KeyAndAnimSettings[i].PositionMin);
						obj.KeyAndAnimSettings[i].PositionMax = EditorGUILayout.Vector3Field("Max Position", obj.KeyAndAnimSettings[i].PositionMax);
						obj.KeyAndAnimSettings[i].RotationMin = EditorGUILayout.Vector3Field("Min Rotation", obj.KeyAndAnimSettings[i].RotationMin);
						obj.KeyAndAnimSettings[i].RotationMax = EditorGUILayout.Vector3Field("Max Rotation", obj.KeyAndAnimSettings[i].RotationMax);
					}
						break;
					}
				}
					break;
				}
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Remove", GUILayout.Width(75.0f)))
				{
					obj.KeyAndAnimSettings.RemoveAt(i);
				}
				GUILayout.EndHorizontal();
			}
			
			// 追加ボタンの表示
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			if (GUILayout.Button("Add Key And Anim Settings List Column"))
			{
				obj.KeyAndAnimSettings.Add(new MonobitPlayerMoveTemplate.MonobitKeySettings());
			}
			GUILayout.EndHorizontal();
			
			GUI.enabled = true;
			
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
