#ifndef __MRS_UTILITY_HPP__
#define __MRS_UTILITY_HPP__

#include <common.hpp>

#include <vector>

namespace mrs {
class Utility
{
public:
    static std::string ToHex( const void* data, uint32 data_len );
    
    static std::vector< std::string > Split( const char* separator, const char* value );
    
    static std::string Join( const char* separator, std::vector< std::string >& strings );
};
};

#endif
