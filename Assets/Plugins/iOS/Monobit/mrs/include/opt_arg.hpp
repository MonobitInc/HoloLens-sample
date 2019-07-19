#ifndef __MRS_OPT_ARG_HPP__
#define __MRS_OPT_ARG_HPP__

#include <mrs.hpp>

#include <map>
#include <vector>

namespace mrs {
class OptArg
{
public:
    typedef bool (*Callback)( int32 argi, const char* value, char short_name, const char* long_name, const char* default_value, const char* msg );
    
    struct Option {
        char        short_name;
        std::string long_name;
        bool        is_need_value;
        std::string default_value;
        std::string msg;
    };
    
protected:
    std::map< std::string, Option > m_Options;
    std::map< char, std::string >   m_OptionNames;
    std::vector< std::string >      m_OptionIndexes;
    std::vector< std::string >      m_OptionNeedValueIndexes;
    uint32                          m_LongNameMaxLen;
    uint32                          m_DefaultValueMaxLen;
    std::string                     m_ProcessName;
    std::map< int32, const char* >  m_Args;
    
public:
    std::string GetProcessName(){ return m_ProcessName; }
    
    std::map< int32, const char* >* GetArgs(){ return &m_Args; }
    
public:
    OptArg();
    
    virtual ~OptArg();
    
    virtual void Set( char short_name, const char* long_name, const char* default_value, const char* msg );
    
    virtual const Option* Get( char short_name );
    
    virtual const Option* Get( const char* long_name );
    
    virtual bool Parse( int32 argc, char** argv, Callback callback = NULL );
    
    virtual std::string HelpMsg( const char* prefix_command = "", const char* prefix_options = "    " );
    
public:
    virtual bool OnParse( int32 argi, const char* value, char short_name, const char* long_name, const char* default_value, const char* msg ){
        return false;
    }
};
};

#endif
