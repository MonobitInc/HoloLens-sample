#ifndef __MRS_OBJECT_HPP__
#define __MRS_OBJECT_HPP__

#include <mrs.hpp>

namespace mrs {
class Object
{
public:
    enum Type {
        MRS_OBJECT_TYPE_NULL    = 0,
        
        MRS_OBJECT_TYPE_BOOL    = 1,
        
        MRS_OBJECT_TYPE_UINT8   = 11,
        MRS_OBJECT_TYPE_UINT16  = 12,
        MRS_OBJECT_TYPE_UINT32  = 13,
        MRS_OBJECT_TYPE_UINT64  = 14,
        
        MRS_OBJECT_TYPE_INT8    = 21,
        MRS_OBJECT_TYPE_INT16   = 22,
        MRS_OBJECT_TYPE_INT32   = 23,
        MRS_OBJECT_TYPE_INT64   = 24,
        
        MRS_OBJECT_TYPE_FLOAT   = 31,
        MRS_OBJECT_TYPE_DOUBLE  = 32,
        
        MRS_OBJECT_TYPE_STRING  = 41,
        
        MRS_OBJECT_TYPE_UNKNOWN = 0xFF,
    };
    
protected:
    uint8       m_Type;
    
    mrs::Buffer m_DataBuffer;
    
public:
    uint8 GetType(){ return m_Type; }
    
    mrs::Buffer* GetDataBuffer(){ return &m_DataBuffer; }
    
public:
    Object(){
        m_Type = 0;
    }
    
    Object( mrs::Buffer* buffer ){
        Unpack( buffer );
    }
    
    virtual ~Object(){}
    
    virtual bool Set( uint8 type, void* data, uint32 data_len ){
        m_Type = type;
        m_DataBuffer.Unread( 0xFFFFFFFF );
        m_DataBuffer.Unwrite( 0xFFFFFFFF );
        return m_DataBuffer.Write( data, data_len );
    }
    
    virtual bool SetBool( bool data ){ return Set( MRS_OBJECT_TYPE_BOOL, &data, sizeof( data ) ); }
    
    virtual bool SetUInt8( uint8 data ){ return Set( MRS_OBJECT_TYPE_UINT8, &data, sizeof( data ) ); }
    virtual bool SetUInt16( uint16 data ){ return Set( MRS_OBJECT_TYPE_UINT16, &data, sizeof( data ) ); }
    virtual bool SetUInt32( uint32 data ){ return Set( MRS_OBJECT_TYPE_UINT32, &data, sizeof( data ) ); }
    virtual bool SetUInt64( uint64 data ){ return Set( MRS_OBJECT_TYPE_UINT64, &data, sizeof( data ) ); }
    
    virtual bool SetInt8( int8 data ){ return Set( MRS_OBJECT_TYPE_INT8, &data, sizeof( data ) ); }
    virtual bool SetInt16( int16 data ){ return Set( MRS_OBJECT_TYPE_INT16, &data, sizeof( data ) ); }
    virtual bool SetInt32( int32 data ){ return Set( MRS_OBJECT_TYPE_INT32, &data, sizeof( data ) ); }
    virtual bool SetInt64( int64 data ){ return Set( MRS_OBJECT_TYPE_INT64, &data, sizeof( data ) ); }
    
    virtual bool SetFloat( float data ){ return Set( MRS_OBJECT_TYPE_FLOAT, &data, sizeof( data ) ); }
    virtual bool SetDouble( double data ){ return Set( MRS_OBJECT_TYPE_DOUBLE, &data, sizeof( data ) ); }
    
    virtual bool SetString( const char* data, uint32 data_len ){
        m_Type = MRS_OBJECT_TYPE_STRING;
        m_DataBuffer.Unread( 0xFFFFFFFF );
        m_DataBuffer.Unwrite( 0xFFFFFFFF );
        return ( 0 == data_len ) ? true : m_DataBuffer.Write( data, data_len );
    }
    virtual bool SetString( const char* data ){
        return SetString( data, strlen( data ) );
    }
    
    virtual bool Get( void* data, uint32 data_len ){
        m_DataBuffer.Unread( 0xFFFFFFFF );
        return m_DataBuffer.Read( data, data_len );
    }
    
    virtual bool GetBool(){
        bool data = false;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual uint8 GetUInt8(){
        uint8 data = 0;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual uint16 GetUInt16(){
        uint16 data = 0;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual uint32 GetUInt32(){
        uint32 data = 0;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual uint64 GetUInt64(){
        uint64 data = 0;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual int8 GetInt8(){
        int8 data = 0;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual int16 GetInt16(){
        int16 data = 0;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual int32 GetInt32(){
        int32 data = 0;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual int64 GetInt64(){
        int64 data = 0;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual float GetFloat(){
        float data = 0.0f;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual double GetDouble(){
        double data = 0.0f;
        Get( &data, sizeof( data ) );
        return data;
    }
    
    virtual std::string GetString(){
        m_DataBuffer.Unread( 0xFFFFFFFF );
        return std::string( m_DataBuffer.GetData(), m_DataBuffer.GetDataLen() );
    }
    
    virtual bool Pack( mrs::Buffer* buffer ){
        do{
            if ( ! buffer->WriteUInt8( m_Type ) ) break;
            
            m_DataBuffer.Unread( 0xFFFFFFFF );
            uint32 data_len = m_DataBuffer.GetDataLen();
            if ( ! buffer->WriteUInt32( data_len ) ) break;
            if ( 0 < data_len ){
                if ( ! buffer->Write( m_DataBuffer.GetData(), data_len ) ) break;
            }
            return true;
        }while ( false );
        return false;
    }
    
    virtual bool Unpack( mrs::Buffer* buffer ){
        m_Type = buffer->ReadUInt8();
        uint32 data_len = buffer->ReadUInt32();
        do{
            m_DataBuffer.Unread( 0xFFFFFFFF );
            m_DataBuffer.Unwrite( 0xFFFFFFFF );
            if ( 0 < data_len ){
                if ( ! m_DataBuffer.Write( buffer->GetData(), data_len ) ) break;
                buffer->Read( NULL, data_len );
            }
            return true;
        }while ( false );
        return false;
    }
    
    Object& operator =( const Object& object ){
        m_Type = object.m_Type;
        m_DataBuffer = object.m_DataBuffer;
        return *this;
    }
};
};

#endif
