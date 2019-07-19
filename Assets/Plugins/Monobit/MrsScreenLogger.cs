using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace mrs {
public class ScreenLogger : MonoBehaviour {
    protected List< String > m_LogList;
    protected String         m_Log;
    public GUIStyle          m_LogStyle;
    public Rect              m_LogRect;
    public int               m_MaxLogNum;
    public int               m_Depth;
    
    public GUIStyle LogStyle {
        get{ return m_LogStyle; }
        set{ m_LogStyle = value; }
    }
    
    public Rect LogRect {
        get{ return m_LogRect; }
        set{ m_LogRect = value; }
    }
    
    public int MaxLogNum {
        get{ return m_MaxLogNum; }
        set{ m_MaxLogNum = value; }
    }
    
    public int Depth {
        get{ return m_Depth; }
        set{ m_Depth = value; }
    }
    
    void Awake(){
        m_LogList = new List< String >();
        m_Log = "";
        m_LogStyle = new GUIStyle();
        m_LogStyle.richText = true;
        m_LogRect = new Rect( 0, 0, Screen.width, Screen.height );
        m_MaxLogNum = 30;
        m_Depth = -1;
    }
    
    void OnEnable(){
        Application.logMessageReceived += OnOutputLog;
    }
    
    void OnDisable(){
        Application.logMessageReceived -= OnOutputLog;
    }
    
    protected void OnOutputLog( String msg, String stack_trace, LogType type ){
        m_LogList.Add( Msg( msg, stack_trace, type ) );
        if ( m_MaxLogNum < m_LogList.Count ){
            m_LogList.RemoveAt( 0 );
        }
        m_Log = String.Join( "\n", m_LogList.ToArray() );
    }
    
    protected String Msg( String msg, String stack_trace, LogType type ){
        switch ( type ){
        case LogType.Log:{
            msg = String.Format( "<color=white>{0}</color>", msg );
        }break;
        
        case LogType.Warning:{
            msg = String.Format( "<color=orange>{0}</color>", msg );
        }break;
        
        default:{
            msg = String.Format( "<color=red>{0}</color>", msg );
        }break;
        }
        return msg;
    }
    
    void OnGUI(){
        GUI.depth = m_Depth;
        GUILayout.BeginArea( m_LogRect );
        GUILayout.Label( m_Log, m_LogStyle );
        GUILayout.EndArea();
    }
}
}
