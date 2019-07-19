#ifndef __MRS_TIME_HPP__
#define __MRS_TIME_HPP__

#include <common.hpp>

namespace mrs {
class Time
{
public:
    static mrs::Time Now();
    
protected:
    uint64 m_Sec;
    uint32 m_Usec;
    
public:
    uint64 GetSec() const { return m_Sec; }
    
    uint32 GetUsec() const { return m_Usec; }
    
public:
    Time( uint64 sec = 0, uint32 usec = 0 );
    
    virtual ~Time();
    
    virtual void Set();
    
    virtual void Set( uint64 sec, uint32 usec );
    
    virtual std::string ToString();
    
    mrs::Time operator -( const mrs::Time& time );
};
};

#endif
