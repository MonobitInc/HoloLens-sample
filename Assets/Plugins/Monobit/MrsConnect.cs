using System;
using System.Collections.Generic;

using MrsConnection = System.IntPtr;
namespace mrs {
public class Connect {
    public class Request {
        public Mrs.MrsConnectionType     ConnectionType;
        public String                    Addr;
        public UInt16                    Port;
        public UInt32                    TimeoutMsec;
        
        public Request(){}
        
        public Request( Request request ){
            this.ConnectionType     = request.ConnectionType;
            this.Addr               = request.Addr;
            this.Port               = request.Port;
            this.TimeoutMsec        = request.TimeoutMsec;
        }
    }
    
    public delegate void FallbackConnectCallback( MrsConnection connection, Request request );
    
    protected List< Request >         m_Requests;
    
    protected FallbackConnectCallback m_FallbackConnectCallback;
    
    public Request GetRequest(){ return ( 0 < m_Requests.Count ) ? m_Requests[ 0 ] : null; }
    
    public void AddRequest( Request request ){
        m_Requests.Add( new Request( request ) );
    }
    
    public void SetFallbackConnectCallback( FallbackConnectCallback value ){ m_FallbackConnectCallback = value; }
    
    public Connect(){
        m_Requests = new List< Request >();
        m_FallbackConnectCallback = null;
    }
    
    public MrsConnection FallbackConnect(){
        return FallbackConnect( MrsConnection.Zero );
    }
    
    public MrsConnection FallbackConnect( MrsConnection error_connection ){
        if ( MrsConnection.Zero != error_connection ){
            Mrs.mrs_close( error_connection );
            m_Requests.RemoveAt( 0 );
        }
        while ( 0 < m_Requests.Count ){
            Request request = m_Requests[ 0 ];
            MrsConnection connection = Mrs.mrs_connect( request.ConnectionType, request.Addr, request.Port, request.TimeoutMsec );
            if ( MrsConnection.Zero != connection ){
                if ( null != m_FallbackConnectCallback ) m_FallbackConnectCallback( connection, new Request( request ) );
                return connection;
            }
            
            m_Requests.RemoveAt( 0 );
        }
        return MrsConnection.Zero;
    }
}
}
