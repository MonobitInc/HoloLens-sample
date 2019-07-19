#ifndef __MRS_FILE_HPP__
#define __MRS_FILE_HPP__

#include <mrs.hpp>
#include <sys/stat.h>

#ifndef MRS_WINDOWS
# include <unistd.h>
# define fopen( _file, _name, _mode ) _file = ::fopen( _name, _mode )
# define localtime( _sec, _tm ) localtime_r( &(_sec), &(_tm) )
#else
# define fopen( _file, _name, _mode ) fopen_s( &(_file), _name, _mode )
# define localtime( _sec, _tm ) localtime_s( &(_tm), &(_sec) )
#endif

namespace mrs {
class File
{
public:
    static bool Remove( const char* path ){
        return ( 0 == remove( path ) );
    }
    
    static bool IsExist( const char* path ){
        struct stat info;
        return ( 0 == stat( path, &info ) );
    }
    
    static std::string ReadToString( const char* path ){
        File file;
        file.Open( path, "rb" );
        mrs::Buffer buffer;
        char data[ 10240 ];
        do{
            uint32 read_len = file.Read( data, sizeof( data ) );
            if ( 0 == read_len ) break;
            
            buffer.Write( data, read_len );
        }while ( true );
        file.Close();
        buffer.WriteInt8( '\0' );
        buffer.Unwrite( 1 );
        return std::string( buffer.GetData(), buffer.GetDataLen() );
    }
    
    static FILE* AppendOpenFile( const char* path ){
        FILE* file = NULL;
        fopen( file, path, "r+" );
        if ( NULL != file ){
            fseek( file, 0L, SEEK_END );
        }else{
            fopen( file, path, "w" );
        }
        return file;
    }
    
    static bool Rotate( const char* src_path, const char* dst_path ){
        struct stat st;
        ::stat( src_path, &st );
        if ( 0 == st.st_size ) return true;
        
        bool result = false;
        FILE* src_file = NULL;
        FILE* dst_file = NULL;
        fopen( src_file, src_path, "r+" );
        dst_file = AppendOpenFile( dst_path );
        do{
            if ( NULL == src_file ) break;
            if ( NULL == dst_file ) break;
            
            char buf[ 65535 ];
            do{
                size_t read_size = ::fread( buf, 1, sizeof( buf ), src_file );
                if ( 0 == read_size ) break;
                
                if ( read_size != ::fwrite( buf, 1, read_size, dst_file ) ) break;
            }while ( true );
            
            if ( 0 == ::feof( src_file ) ) break;
            
            ::fclose( src_file );
            fopen( src_file, src_path, "w" );
            if ( NULL == src_file ) break;
            
            result = true;
        }while ( false );
        if ( NULL != src_file ) ::fclose( src_file );
        if ( NULL != dst_file ){
            ::fclose( dst_file );
            if ( ! result ) ::remove( dst_path );
        }
        return result;
    }
    
protected:
    FILE*       m_File;
    std::string m_FilePath;
    
    struct tm   m_Updated;
    
public:
    std::string GetFilePath(){ return m_FilePath; }
    
public:
    File(){
        m_File = NULL;
    }
    
    virtual ~File(){
        Close();
    }
    
    virtual bool Open( const char* path, const char* mode ){
        if ( NULL != m_File ) Close();
        
        if ( 'a' == mode[ 0 ] ){
            m_File = AppendOpenFile( path );
        }else{
            fopen( m_File, path, mode );
        }
        if ( NULL != m_File ){
            m_FilePath = path;
            
            ::memset( &m_Updated, 0, sizeof( m_Updated ) );
            struct stat st;
            ::stat( path, &st );
#if defined( MRS_WINDOWS )
            localtime( st.st_mtime, m_Updated );
#elif defined( MRS_MAC )
            localtime( st.st_mtimespec.tv_sec, m_Updated );
#else
            localtime( st.st_mtim.tv_sec, m_Updated );
#endif
        }
        return ( NULL != m_File );
    }
    
    virtual void Close(){
        if ( NULL != m_File ){
            fclose( m_File );
            m_File = NULL;
        }
    }
    
    virtual void Reopen(){
        std::string file_path = m_FilePath;
        Open( file_path.c_str(), "a" );
    }
    
    virtual void Printf( const char* format, va_list list ){
        if ( NULL != m_File ) vfprintf( m_File, format, list );
    }
    
    virtual void Printf( const char* format, ... ){
        va_list list;
        va_start( list, format );
        Printf( format, list );
        va_end( list );
    }
    
    virtual void Flush(){
        if ( NULL != m_File ) fflush( m_File );
    }
    
    virtual uint32 Write( const void* data, uint32 data_len ){
        return ( NULL == m_File ) ? 0 : (uint32)fwrite( data, 1, data_len, m_File );
    }
    
    virtual uint32 Read( void* data, uint32 data_len ){
        return ( NULL == m_File ) ? 0 : (uint32)fread( data, 1, data_len, m_File );
    }
    
    virtual void DailyRotate(){
        time_t sec = time( NULL );
        struct tm local_time;
        localtime( sec, local_time );
        if ( ( local_time.tm_mday != m_Updated.tm_mday ) || ( local_time.tm_mon != m_Updated.tm_mon ) || ( local_time.tm_year != m_Updated.tm_year ) ){
            ::fclose( m_File );
            m_File = NULL;
            
            char new_file_name[ FILENAME_MAX ];
            ::snprintf( new_file_name, sizeof( new_file_name ), "%s.%04d%02d%02d", m_FilePath.c_str(), m_Updated.tm_year + 1900, m_Updated.tm_mon + 1, m_Updated.tm_mday );
            Rotate( m_FilePath.c_str(), new_file_name );
            
            m_File = AppendOpenFile( m_FilePath.c_str() );
            m_Updated = local_time;
        }
    }
};
};

#endif
