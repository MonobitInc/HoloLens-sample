#ifndef __MRS_BUFFER_HPP__
#define __MRS_BUFFER_HPP__

#include <common.hpp>
#include <time.hpp>

namespace mrs {
class Buffer
{
protected:
    char*  m_Data;
    uint32 m_DataLen;
    uint32 m_WriteLen;
    uint32 m_ReadLen;
    
public:
    char* GetData() const { return ( NULL == m_Data ) ? NULL : &(m_Data[ m_ReadLen ]); }
    
    uint32 GetDataLen() const { return m_WriteLen - m_ReadLen; }
    
public:
    Buffer( uint32 data_len = 0 );
    
    Buffer( void* data, uint32 data_len );
    
    Buffer( const Buffer& buffer );
    
    virtual ~Buffer();
    
    virtual bool Write( const void* data, uint32 data_len );
    
    virtual bool WriteBool( bool data );
    
    virtual bool WriteUInt8( uint8 data );
    
    virtual bool WriteUInt16( uint16 data );
    
    virtual bool WriteUInt32( uint32 data );
    
    virtual bool WriteUInt64( uint64 data );
    
    virtual bool WriteInt8( int8 data );
    
    virtual bool WriteInt16( int16 data );
    
    virtual bool WriteInt32( int32 data );
    
    virtual bool WriteInt64( int64 data );
    
    virtual bool WriteFloat( float data );
    
    virtual bool WriteDouble( double data );
    
    virtual bool WriteBuffer( const void* data, uint32 data_len );
    
    virtual bool WriteBuffer( const mrs::Buffer* buffer );
    
    virtual bool WriteTime( uint64 sec, uint32 usec );
    
    virtual bool WriteTime( const mrs::Time* time );
    
    virtual bool Read( void* data, uint32 data_len );
    
    virtual bool ReadBool();
    
    virtual uint8 ReadUInt8();
    
    virtual uint16 ReadUInt16();
    
    virtual uint32 ReadUInt32();
    
    virtual uint64 ReadUInt64();
    
    virtual int8 ReadInt8();
    
    virtual int16 ReadInt16();
    
    virtual int32 ReadInt32();
    
    virtual int64 ReadInt64();
    
    virtual float ReadFloat();
    
    virtual double ReadDouble();
    
    virtual mrs::Buffer ReadBuffer();
    
    virtual mrs::Time ReadTime();
    
    virtual void Unwrite( uint32 data_len );
    
    virtual void Unread( uint32 data_len );
    
    virtual void Delete( uint32 data_len );
    
    virtual void Clear();
    
    Buffer& operator =( const Buffer& buffer );
};
};

#endif
