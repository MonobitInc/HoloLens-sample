using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using MonobitEngine;

namespace Monobit.Support
{
	[Serializable]
    [RequireComponent(typeof(MonobitView))]
    [RequireComponent(typeof(MonobitTransformView))]
    [RequireComponent(typeof(MonobitAnimatorView))]
	[AddComponentMenu("Monobit Networking Support/Monobit Player Move Template")]
	public class MonobitPlayerMoveTemplate : MonobitEngine.MonoBehaviour
    {
		/** MonobitAnimatorView 本体. */
        [SerializeField]
        public MonobitAnimatorView animView = null;

        public enum KeyCode
        {
            Everytime,
            Horizontal,
            Vertical,
            Fire1,
            Fire2,
            Fire3,
            Jump,
        };

        public enum AxisAction
        {
            Positive,
            Negative,
            Zero,
        };

        public enum ButtonAction
        {
            Press,
            Up,
            Down,
        };

        public enum ActionType
        {
            Move,
            Rotate,
            ChangeAnimLayerWeight,
            ChangeAnimParam,
            Instantiate,
        }

        public enum InstantiateType
        {
            Absolute,
            Relative,
            RandomAbsolute,
        }

		[System.Serializable]
		public class AnimLayerInfo
        {
            [SerializeField]
            public int m_Index;         /**< アニメーションレイヤーのインデックス. */
            [SerializeField]
            public string m_Name;       /**< アニメーションレイヤー名. */
            [SerializeField]
            public float m_animWeight;  /**< アニメーションウェイト値. */
        };

		[System.Serializable]
		public class AnimParamInfo
        {
            [SerializeField]
            public int m_Index;                             /**< アニメーションレイヤーのインデックス. */
			[SerializeField]
			public MonobitAnimatorView.AnimatorControllerParameterType m_Type;  /**< 同期するアニメーションパラメータの型. */
			[SerializeField]
            public string m_Name;                           /**< アニメーションパラメータ名. */
            [SerializeField]
            public float m_floatValue;                      /**< アニメーションパラメータ値（float）. */
            [SerializeField]
            public int m_intValue;                          /**< アニメーションパラメータ値（Int）. */
            [SerializeField]
            public bool m_boolValue;                        /**< アニメーションパラメータ値（bool）. */
        }

		[System.Serializable]
		public class MonobitKeySettings
        {
            [SerializeField]
            public KeyCode keyCode;
            [SerializeField]
            public AxisAction axisAction;
            [SerializeField]
            public ButtonAction buttonAction;
            [SerializeField]
            public ActionType actionType;
            [SerializeField]
            public Vector3 Position;
            [SerializeField]
            public Vector3 Rotation;
            [SerializeField]
            public int SelectLayer;
            [SerializeField]
            public List<AnimLayerInfo> layerInfo = new List<AnimLayerInfo>();
            [SerializeField]
            public int SelectParam;
            [SerializeField]
            public List<AnimParamInfo> paramInfo = new List<AnimParamInfo>();
            [SerializeField]
            public GameObject instantiatePrefab = null;
            [SerializeField]
            public InstantiateType instantiateType;
            [SerializeField]
            public Vector3 PositionMin;
            [SerializeField]
            public Vector3 PositionMax;
            [SerializeField]
            public Vector3 RotationMin;
            [SerializeField]
            public Vector3 RotationMax;
        };

        [SerializeField]
        public List<MonobitKeySettings> KeyAndAnimSettings = new List<MonobitKeySettings>();

        void Update()
        {
            if( animView == null )
            {
                animView = gameObject.GetComponent<MonobitAnimatorView>();
            }
            if (!monobitView.isMine)
            {
                return;
            }

            foreach (var settings in KeyAndAnimSettings)
            {
                DoAction(settings, GetKeyActiveValue(settings));
            }
        }

        void DoAction(MonobitKeySettings settings, float keyActiveValue)
        {
            switch (settings.actionType)
            {
                case ActionType.Move:
                    gameObject.transform.position += gameObject.transform.right * settings.Position.x * keyActiveValue;
                    gameObject.transform.position += gameObject.transform.up * settings.Position.y * keyActiveValue;
                    gameObject.transform.position += gameObject.transform.forward * settings.Position.z * keyActiveValue;
                    break;
                case ActionType.Rotate:
                    gameObject.transform.Rotate(settings.Rotation * keyActiveValue);
                    break;
                case ActionType.ChangeAnimLayerWeight:
                    if (animView != null && animView.m_Animator != null)
                    {
                        AnimLayerInfo info = settings.layerInfo[settings.SelectLayer];
                        animView.m_Animator.SetLayerWeight(info.m_Index, info.m_animWeight * keyActiveValue);
                    }
                    break;
                case ActionType.ChangeAnimParam:
                    if (keyActiveValue > 0.0f)
                    {
                        if (animView != null && animView.m_Animator != null)
                        {
                            AnimParamInfo info = settings.paramInfo[settings.SelectParam];
                            switch (info.m_Type)
                            {
                                case MonobitAnimatorView.AnimatorControllerParameterType.Bool:
                                    animView.m_Animator.SetBool(info.m_Name, info.m_boolValue);
                                    break;
                                case MonobitAnimatorView.AnimatorControllerParameterType.Float:
                                    animView.m_Animator.SetFloat(info.m_Name, info.m_floatValue * keyActiveValue);
                                    break;
                                case MonobitAnimatorView.AnimatorControllerParameterType.Int:
                                    animView.m_Animator.SetInteger(info.m_Name, (int)(info.m_intValue * keyActiveValue));
                                    break;
                                case MonobitAnimatorView.AnimatorControllerParameterType.Trigger:
                                    break;
                            }
                        }
                    }
                    break;
                case ActionType.Instantiate:
                    if (keyActiveValue >= 1.0f)
                    {
                        switch (settings.instantiateType)
                        {
                            case InstantiateType.Absolute:
                                {
                                    Quaternion rotation = Quaternion.Euler(settings.Rotation);
                                    MonobitNetwork.Instantiate(settings.instantiatePrefab.name, settings.Position, rotation, 0);
                                }
                                break;
                            case InstantiateType.Relative:
                                {
                                    Vector3 position = gameObject.transform.position;
                                    position += gameObject.transform.right * settings.Position.x;
                                    position += gameObject.transform.up * settings.Position.y;
                                    position += gameObject.transform.forward * settings.Position.z;
                                    Quaternion rotation = gameObject.transform.rotation * Quaternion.Euler(settings.Rotation);
                                    MonobitNetwork.Instantiate(settings.instantiatePrefab.name, position, rotation, 0);
                                }
                                break;
                            case InstantiateType.RandomAbsolute:
                                {
                                    Vector3 position = new Vector3(UnityEngine.Random.Range(settings.PositionMin.x, settings.PositionMax.x),
                                                                   UnityEngine.Random.Range(settings.PositionMin.y, settings.PositionMax.y),
                                                                   UnityEngine.Random.Range(settings.PositionMin.z, settings.PositionMax.z));
                                    Quaternion rotation = Quaternion.Euler(UnityEngine.Random.Range(settings.RotationMin.x, settings.RotationMax.x),
                                                                           UnityEngine.Random.Range(settings.RotationMin.y, settings.RotationMax.y),
                                                                           UnityEngine.Random.Range(settings.RotationMin.z, settings.RotationMax.z));
                                    MonobitNetwork.Instantiate(settings.instantiatePrefab.name, position, rotation, 0);
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private float GetKeyActiveValue(MonobitKeySettings settings)
        {
            if (settings.keyCode == KeyCode.Horizontal && settings.axisAction == AxisAction.Positive)
            {
                if (Input.GetAxis("Horizontal") > 0.0f)
                {
                    return Math.Abs(Input.GetAxis("Horizontal"));
                }
                return 0.0f;
            }
            else if (settings.keyCode == KeyCode.Horizontal && settings.axisAction == AxisAction.Negative)
            {
                if (Input.GetAxis("Horizontal") < 0.0f)
                {
                    return Math.Abs(Input.GetAxis("Horizontal"));
                }
                return 0.0f;
            }
            else if (settings.keyCode == KeyCode.Horizontal && settings.axisAction == AxisAction.Zero)
            {
                if (Input.GetAxis("Horizontal") == 0.0f)
                {
                    return 1.0f;
                }
                return 0.0f;
            }
            else if (settings.keyCode == KeyCode.Vertical && settings.axisAction == AxisAction.Positive)
            {
                if (Input.GetAxis("Vertical") > 0.0f)
                {
                    return Math.Abs(Input.GetAxis("Vertical"));
                }
                return 0.0f;
            }
            else if (settings.keyCode == KeyCode.Vertical && settings.axisAction == AxisAction.Negative)
            {
                if (Input.GetAxis("Vertical") < 0.0f)
                {
                    return Math.Abs(Input.GetAxis("Vertical"));
                }
                return 0.0f;
            }
            else if (settings.keyCode == KeyCode.Vertical && settings.axisAction == AxisAction.Zero)
            {
                if (Input.GetAxis("Vertical") == 0.0f)
                {
                    return 1.0f;
                }
                return 0.0f;
            }
            else if (settings.keyCode == KeyCode.Fire1)
            {
                return IsKeyAction("Fire1", settings.buttonAction) ? 1.0f : 0.0f;
            }
            else if (settings.keyCode == KeyCode.Fire2)
            {
                return IsKeyAction("Fire2", settings.buttonAction) ? 1.0f : 0.0f;
            }
            else if (settings.keyCode == KeyCode.Fire3)
            {
                return IsKeyAction("Fire3", settings.buttonAction) ? 1.0f : 0.0f;
            }
            else if (settings.keyCode == KeyCode.Jump)
            {
                return IsKeyAction("Jump", settings.buttonAction) ? 1.0f : 0.0f;
            }
            else if (settings.keyCode == KeyCode.Everytime)
            {
                return 1.0f;
            }
            return 0.0f;
        }

        bool IsKeyAction(string buttonName, ButtonAction action)
        {
            switch (action)
            {
                case ButtonAction.Press:
                    return Input.GetButton(buttonName);
                case ButtonAction.Down:
                    return Input.GetButtonDown(buttonName);
                case ButtonAction.Up:
                    return Input.GetButtonUp(buttonName);
            }
            return false;
        }

		/**
		 * 
		 */
		public void EntryKeyAndAnim()
		{
			if (animView == null)
			{
				animView = gameObject.GetComponent<MonobitAnimatorView>();
			}
			if (animView != null && this.KeyAndAnimSettings != null)
			{
				List<MonobitAnimatorView.AnimLayerInfo> animLayer = animView.SyncAnimLayers;
				List<MonobitAnimatorView.AnimParamInfo> animParam = animView.SyncAnimParams;
				
				foreach (MonobitPlayerMoveTemplate.MonobitKeySettings settings in this.KeyAndAnimSettings)
				{
					for (int i = settings.layerInfo.Count - 1; i >= 0; --i)
					{
						bool bFound = false;
						foreach (var layer in animLayer)
						{
							if (layer.m_Name == settings.layerInfo[i].m_Name)
							{
								bFound = true;
								break;
							}
						}
						if (bFound == false)
						{
							settings.layerInfo.RemoveAt(i);
						}
					}
					for (int i = settings.paramInfo.Count - 1; i >= 0; --i)
					{
						bool bFound = false;
						foreach (var param in animParam)
						{
							if (param.m_Name == settings.paramInfo[i].m_Name)
							{
								bFound = true;
								break;
							}
						}
						if (bFound == false)
						{
							settings.paramInfo.RemoveAt(i);
						}
					}
				}
				
				foreach (MonobitPlayerMoveTemplate.MonobitKeySettings settings in this.KeyAndAnimSettings)
				{
					foreach (MonobitAnimatorView.AnimLayerInfo layer in animLayer)
					{
						bool bFound = false;
						if( settings.layerInfo != null )
						{
							foreach (MonobitPlayerMoveTemplate.AnimLayerInfo layerInfo in settings.layerInfo)
							{
								if (layer.m_Name == layerInfo.m_Name)
								{
									bFound = true;
									break;
								}
							}
							if (bFound == false)
							{
								settings.layerInfo.Add(new MonobitPlayerMoveTemplate.AnimLayerInfo
								                       {
									m_Index = layer.m_Index,
									m_Name = layer.m_Name,
									m_animWeight = (animView.m_Animator.isActiveAndEnabled) ? animView.m_Animator.GetLayerWeight(layer.m_Index) : 0.0f
								});
							}
						}
					}
					foreach (MonobitAnimatorView.AnimParamInfo param in animParam)
					{
						bool bFound = false;
						if( settings.paramInfo != null )
						{
							foreach (MonobitPlayerMoveTemplate.AnimParamInfo paramInfo in settings.paramInfo)
							{
								if (param.m_Name == paramInfo.m_Name)
								{
									bFound = true;
									break;
								}
							}
							if (bFound == false)
							{
								switch (param.m_Type)
								{
								case MonobitAnimatorView.AnimatorControllerParameterType.Bool:
									settings.paramInfo.Add(new MonobitPlayerMoveTemplate.AnimParamInfo
									                       {
										m_Type = param.m_Type,
										m_Name = param.m_Name,
										m_boolValue = (animView.m_Animator.isActiveAndEnabled) ? animView.m_Animator.GetBool(param.m_Name): false
									});
									break;
								case MonobitAnimatorView.AnimatorControllerParameterType.Float:
									settings.paramInfo.Add(new MonobitPlayerMoveTemplate.AnimParamInfo
									                       {
										m_Type = param.m_Type,
										m_Name = param.m_Name,
										m_floatValue = (animView.m_Animator.isActiveAndEnabled) ? animView.m_Animator.GetFloat(param.m_Name): 0.0f
									});
									break;
								case MonobitAnimatorView.AnimatorControllerParameterType.Int:
									settings.paramInfo.Add(new MonobitPlayerMoveTemplate.AnimParamInfo
									{
										m_Type = param.m_Type,
										m_Name = param.m_Name,
										m_intValue = (animView.m_Animator.isActiveAndEnabled) ? animView.m_Animator.GetInteger(param.m_Name): 0
									});
									break;
								case MonobitAnimatorView.AnimatorControllerParameterType.Trigger:
									break;
								}
							}
						}
					}
				}
			}
		}
	}
}
