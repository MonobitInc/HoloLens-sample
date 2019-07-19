#ifndef __MRS_CONNECT_HPP__
#define __MRS_CONNECT_HPP__

#include <mrs.hpp>

namespace mrs {
class Connect
{
public:
    typedef struct {
        MrsConnectionType     connection_type;
        const char*           addr;
        uint16                port;
        uint32                timeout_msec;
    } Request;
    
    typedef void (*FallbackConnectCallback)( MrsConnection connection, const Request* request );
    
protected:
    std::vector< Request >  m_Requests;
    
    FallbackConnectCallback m_FallbackConnectCallback;
    
public:
    const Request* GetRequest(){ return ( 0 < m_Requests.size() ) ? &m_Requests[ 0 ] : NULL; }
    
    void AddRequest( const Request* request ){
        m_Requests.push_back( *request );
    }
    
    void SetFallbackConnectCallback( FallbackConnectCallback value ){ m_FallbackConnectCallback = value; }
    
public:
    Connect(){
        m_FallbackConnectCallback = NULL;
    }
    
    virtual ~Connect(){}
    
    virtual MrsConnection FallbackConnect( MrsConnection error_connection = NULL ){
        if ( NULL != error_connection ){
            mrs_close( error_connection );
            m_Requests.erase( m_Requests.begin() );
        }
        for ( std::vector< Request >::iterator itr = m_Requests.begin(); m_Requests.end() != itr; ){
            Request* request = &(*itr);
            MrsConnection connection = mrs_connect( request->connection_type, request->addr, request->port, request->timeout_msec );
            if ( NULL != connection ){
                if ( NULL != m_FallbackConnectCallback ) (*m_FallbackConnectCallback)( connection, request );
                return connection;
            }
            
            itr = m_Requests.erase( itr );
        }
        return NULL;
    }
};
};

#endif
