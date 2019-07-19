using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vuforia;

public class CalibrationControl : DefaultTrackableEventHandler
{
    [SerializeField] GameObject ARCamera = null;        //VuforiaのCamera Object
    [SerializeField] GameObject Board = null;           //看板Object
    [SerializeField] float CalibrationTime = 3f;        //Calibrationする時間

    bool found;             //画像認識見つかったかどうか
    bool undone = true;     //Calibration未完成かどうか
    float startTime;        //Calibration開始時間

    void Update()
    {
        //Vuforiaで認識したObjectの座標に看板Objectを移動させ、そこに原点にする
        if (undone && found)
        {
            HololensSample.Instance.SetStateText("Calibrating...");

#if UNITY_EDITOR
            //Editorの場合、MRTKのSpatialMappingない為、直接設定
            Board.transform.position = transform.position;
            Board.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
#else
            //Vuforiaの結果はただ方向を使います。その方向にMRTKのSpatialMappingで構築した壁に、Collision判定で画像の位置を確定する
            RaycastHit hit;
            if (Physics.Raycast(ARCamera.transform.position, transform.position - ARCamera.transform.position, out hit))
            {
                Board.transform.position = hit.point;
                Board.transform.forward = new Vector3(-hit.normal.x, 0f, -hit.normal.z);
            }
#endif
            //時間経ったら、Calibration完成
            if (Time.realtimeSinceStartup - startTime > CalibrationTime)
            {
                undone = false;
                Debug.Log("Calibration Done.");
                HololensSample.Instance.HideState();
                //看板Objectの位置を原点にする
                HololensSample.Instance.SetBasePosition(Board.transform);
            }
        }
    }

    //Calibration開始
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();

        Debug.Log("OnTrackingFound");
        found = true;
        startTime = Time.realtimeSinceStartup;
    }

    //画像認識Lost、Calibration失敗或いはReset
    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();

        Debug.Log("OnTrackingLost");
        found = false;
        undone = true;
        HololensSample.Instance.HideState();
    }
}
