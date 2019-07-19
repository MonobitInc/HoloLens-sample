#ifndef __MRS_SIGNAL_HPP__
#define __MRS_SIGNAL_HPP__

#include <mrs.hpp>

#ifndef MRS_WINDOWS
namespace mrs {
class Signal
{
public:
    typedef void (*Callback)( int value );
    
public:
    static void SetCallback( int value, Callback callback ){
        struct sigaction new_act;
        memset( &new_act, 0, sizeof( new_act ) );
        new_act.sa_handler = callback;
        sigaction( (int)value, &new_act, NULL );
    }
    
    static void Ignore( int value ){
        SetCallback( value, SIG_IGN );
    }
};
};
#endif

#endif
