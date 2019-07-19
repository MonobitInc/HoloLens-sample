#ifndef __MRS_DATE_TIME_HPP__
#define __MRS_DATE_TIME_HPP__

#include <time.hpp>

namespace mrs {
class DateTime
{
public:
    static mrs::DateTime Now();
    
protected:
    uint16 m_Year;
    uint8  m_Month;
    uint8  m_Date;
    uint8  m_Hour;
    uint8  m_Min;
    uint8  m_Sec;
    uint32 m_Usec;
    
public:
    uint16 GetYear() const { return m_Year; }
    
    uint8 GetMonth() const { return m_Month; }
    
    uint8 GetDate() const { return m_Date; }
    
    uint8 GetHour() const { return m_Hour; }
    
    uint8 GetMin() const { return m_Min; }
    
    uint8 GetSec() const { return m_Sec; }
    
    uint32 GetUsec() const { return m_Usec; }
    
public:
    DateTime( uint16 year = 0, uint8 month = 0, uint8 date = 0, uint8 hour = 0, uint8 min = 0, uint8 sec = 0, uint32 usec = 0 );
    
    DateTime( const Time& time );
    
    virtual ~DateTime();
    
    virtual void Set();
    
    virtual void Set( const mrs::Time& time );
    
    virtual std::string ToString();
};
};

#endif
