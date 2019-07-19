var Mrs = {
    $Mrs: {
        VERSION_KEY: "mrs",
        VERSION:     0x00010007,
        
        s_Initialized: false,
        
        READ_BUFFER_MAX_SIZE_DEFAULT: 65535,
        
        RpcId: {
            BEGIN                            : 0xFF00,
            
            VERSION_CHECK                    : 0xFF00,
            
            KEEP_ALIVE_REQUEST               : 0xFFA1,
            KEEP_ALIVE_RESPONSE              : 0xFFA2,
            
            CONNECTION_CLOSE                 : 0xFFC0,
            CONNECTION_CLOSE_HARD_LIMIT_OVER : 0xFFC1,
            
            END                              : 0xFFFF,
        },
        
        LogLevel: {
            EMERG   : 0,
            ALERT   : 1,
            CRIT    : 2,
            ERR     : 3,
            WARNING : 4,
            NOTICE  : 5,
            INFO    : 6,
            DEBUG   : 7,
        },
        
        ConnectionType: {
            NONE : 0,
            TCP  : 1,
            UDP  : 2,
            WS   : 3,
            WSS  : 4,
        },
        
        Error: {
            NO_ERROR        : 0,
            
            ENOENT          : 2,
            ENOMEM          : 12,
            EACCES          : 13,
            EMFILE          : 24,
            EADDRINUSE      : 48,
            EADDRNOTAVAIL   : 49,
            ENETUNREACH     : 51,
            EHOSTUNREACH    : 65,
            
            ECONNECTIONTYPE : 0xF001,
            EBACKLOG        : 0xF002,
            ECONNECTIONNUM  : 0xF003,
        },
        
        ConnectionError: {
            CONNECT_ERROR                   : 1,
            CONNECT_TIMEOUT                 : 2,
            WRITE_ERROR                     : 3,
            KEY_EXCHANGE_REQUEST_ERROR      : 4,
            KEY_EXCHANGE_RESPONSE_ERROR     : 5,
            PEER_CONNECTION_HARD_LIMIT_OVER : 6,
            CONNECTION_READBUF_SIZE_OVER    : 7,
            KEEPALIVE_TIMEOUT               : 8,
            PROTOCOL_ERROR                  : 9,
            READ_INVALID_RECORD_ERROR       : 10,
            LISTEN_ERROR                    : 11,
            RESOLVE_ADDRESS_ERROR           : 12,
            RESOLVE_ADDRESS_TIMEOUT         : 13,
        },
        
        s_Error: 0,
        
        s_LogOutputCallback: 0,
        
        LOG_EMERG: function( msg ){
            Mrs.OutputLog( Mrs.LogLevel.EMERG, msg );
        },
        
        LOG_ALERT: function( msg ){
            Mrs.OutputLog( Mrs.LogLevel.ALERT, msg );
        },
        
        LOG_CRIT: function( msg ){
            Mrs.OutputLog( Mrs.LogLevel.CRIT, msg );
        },
        
        LOG_ERR: function( msg ){
            Mrs.OutputLog( Mrs.LogLevel.ERR, msg );
        },
        
        LOG_WARNING: function( msg ){
            Mrs.OutputLog( Mrs.LogLevel.WARNING, msg );
        },
        
        LOG_NOTICE: function( msg ){
            Mrs.OutputLog( Mrs.LogLevel.NOTICE, msg );
        },
        
        LOG_INFO: function( msg ){
            Mrs.OutputLog( Mrs.LogLevel.INFO, msg );
        },
        
        LOG_DEBUG: function( msg ){
            Mrs.OutputLog( Mrs.LogLevel.DEBUG, msg );
        },
        
        OutputLog: function( level, msg ){
            if ( typeof msg !== "string" ) msg = Pointer_stringify( msg );
            if ( 0 !== Mrs.s_LogOutputCallback ) Runtime.dynCall( "vii", Mrs.s_LogOutputCallback, [ level, allocate( intArrayFromString( msg ), "i8", ALLOC_STACK ) ] );
        },
        
        String: {
            ToBytes: function( data ){
                return ( new Uint8Array( [].map.call( data, function( c ){ return c.charCodeAt( 0 ); } ) ) );
            },
            
            FromBytes: function( bytes ){
                return String.fromCharCode.apply( null, bytes );
            },
        },
        
        Int64: {
            ToInt32Array: function( data ){
                var hex_string = "0000000000000000"+ data.toString( 16 );
                return [ parseInt( hex_string.slice( 0, -8 ) ), data & 0xFFFFFFFF ];
            },
            
            FromInt32Array: function( array ){
                var hex_string = array[ 0 ].toString( 16 ) + "00000000";
                return parseInt( hex_string ) | array[ 1 ];
            },
        },
        
        Memory: {
            Get: function( input, input_index, data_len ){
                return input.subarray( input_index, input_index + data_len );
            },
            
            GetUInt8: function( input, input_index ){
                return input[ input_index ];
            },
            
            GetUInt16: function( input, input_index ){
                var bytes = input.subarray( input_index, input_index + 2 );
                // Little Endian Only
                var data = bytes[ 0 ];
                data |= ( bytes[ 1 ] << 8 );
                return data;
            },
            
            GetUInt32: function( input, input_index ){
                var bytes = input.subarray( input_index, input_index + 4 );
                // Little Endian Only
                var data = bytes[ 0 ];
                data |= ( bytes[ 1 ] <<  8 );
                data |= ( bytes[ 2 ] << 16 );
                data |= ( bytes[ 3 ] << 24 );
                return data >>> 0;
            },
            
            GetInt64: function( input, input_index ){
                var bytes = input.subarray( input_index, input_index + 8 );
                // Little Endian Only
                var hex_strings = [];
                for ( var i = 0; i < 8; ++i ){
                    var string = bytes[ i ].toString( 16 );
                    if ( 1 == string.length ) string = "0"+ string;
                    hex_strings.push( string );
                }
                hex_strings = hex_strings.reverse();
                return parseInt( hex_strings.join( "" ), 16 );
            },
            
            Set: function( output, output_index, data ){
                output.set( data, output_index );
            },
            
            SetUInt8: function( output, output_index, data ){
                output[ output_index ] = data & 0xFF;
            },
            
            SetUInt16: function( output, output_index, data ){
                // Little Endian Only
                output[ output_index++ ] = data & 0xFF;
                output[ output_index++ ] = ( data >> 8 ) & 0xFF;
            },
            
            SetUInt32: function( output, output_index, data ){
                // Little Endian Only
                output[ output_index++ ] = data & 0xFF;
                output[ output_index++ ] = ( data >> 8 ) & 0xFF;
                output[ output_index++ ] = ( data >> 16 ) & 0xFF;
                output[ output_index++ ] = ( data >> 24 ) & 0xFF;
            },
            
            SetInt64: function( output, output_index, data ){
                // Little Endian Only
                var hex_string = "0000000000000000"+ data.toString( 16 );
                for ( var i = 0; i < 8; ++i ){
                    output[ output_index++ ] = parseInt( hex_string.substr( -2 * ( i + 1 ), 2 ), 16 );
                }
            },
        },
        
        Buffer: function(){
            var self = this;
            
            self.m_Data = new Uint8Array( 0 );
            self.m_WriteLen = 0;
            self.m_ReadLen = 0;
            self.m_AlignLen = 1024;
            
            self.GetData = function(){
                return self.m_Data.subarray( self.m_ReadLen, self.m_WriteLen );
            };
            
            self.GetDataLen = function(){
                return self.m_WriteLen - self.m_ReadLen;
            };
            
            self.WriteCheck = function( data_len ){
                var max_len = self.m_WriteLen + data_len;
                if ( self.m_Data.length < max_len ){
                    if ( 1 < self.m_AlignLen ){
                        var block_num = max_len / self.m_AlignLen;
                        if ( 0 < max_len % self.m_AlignLen ) ++block_num;
                        max_len = block_num * self.m_AlignLen;
                    }
                    var data = new Uint8Array( max_len );
                    data.set( self.m_Data );
                    self.m_Data = data;
                }
            };
            
            self.Write = function( data ){
                self.WriteCheck( data.length );
                self.m_Data.set( data, self.m_WriteLen );
                self.m_WriteLen += data.length;
                return true;
            };
            
            self.WriteUInt8 = function( data ){
                var bytes = new Uint8Array( 1 );
                Mrs.Memory.SetUInt8( bytes, 0, data );
                return self.Write( bytes );
            };
            
            self.WriteUInt16 = function( data ){
                var bytes = new Uint8Array( 2 );
                Mrs.Memory.SetUInt16( bytes, 0, data );
                return self.Write( bytes );
            };
            
            self.WriteUInt32 = function( data ){
                var bytes = new Uint8Array( 4 );
                Mrs.Memory.SetUInt32( bytes, 0, data );
                return self.Write( bytes );
            };
            
            self.WriteInt8 = function( data ){
                return self.WriteUInt8( data ) | 0;
            };
            
            self.WriteInt16 = function( data ){
                return self.WriteUInt16( data ) | 0;
            };
            
            self.WriteInt32 = function( data ){
                return self.WriteUInt32( data ) | 0;
            };
            
            self.WriteInt64 = function( data ){
                var bytes = new Uint8Array( 8 );
                Mrs.Memory.SetInt64( bytes, 0, data );
                return self.Write( bytes );
            };
            
            self.Read = function( data_len ){
                if ( self.m_WriteLen < self.m_ReadLen + data_len ) return null;
                
                var data = self.m_Data.subarray( self.m_ReadLen, self.m_ReadLen + data_len );
                self.m_ReadLen += data_len;
                return data;
            };
            
            self.ReadUInt8 = function(){
                var bytes = self.Read( 1 );
                if ( null == bytes ) return 0;
                
                return Mrs.Memory.GetUInt8( bytes, 0 );
            };
            
            self.ReadUInt16 = function(){
                var bytes = self.Read( 2 );
                if ( null == bytes ) return 0;
                
                return Mrs.Memory.GetUInt16( bytes, 0 );
            };
            
            self.ReadUInt32 = function(){
                var bytes = self.Read( 4 );
                if ( null == bytes ) return 0;
                
                return Mrs.Memory.GetUInt32( bytes, 0 );
            };
            
            self.ReadInt8 = function(){
                return ( self.ReadUInt8() << 8 ) >> 8;
            };
            
            self.ReadInt16 = function(){
                return ( self.ReadUInt16() << 16 ) >> 16;
            };
            
            self.ReadInt32 = function(){
                return self.ReadUInt32() | 0;
            };
            
            self.ReadInt64 = function(){
                var bytes = self.Read( 8 );
                if ( null == bytes ) return 0;
                
                return Mrs.Memory.GetInt64( bytes, 0 );
            };
            
            self.Unwrite = function( data_len ){
                if ( self.m_WriteLen < data_len ) data_len = self.m_WriteLen;
                self.m_WriteLen -= data_len;
            };
            
            self.Unread = function( data_len ){
                if ( self.m_ReadLen < data_len ) data_len = self.m_ReadLen;
                self.m_ReadLen -= data_len;
            };
            
            self.Delete = function( data_len ){
                if ( 0 == data_len ) return;
                
                if ( self.m_WriteLen < data_len ) data_len = self.m_WriteLen;
                var move_len = self.m_WriteLen - data_len;
                if ( 0 < move_len ){
                    self.m_Data = self.m_Data.subarray( data_len, self.m_WriteLen );
                }else{
                    self.m_Data = new Uint8Array( 0 );
                }
                self.m_WriteLen -= data_len;
                self.m_ReadLen = ( data_len < self.m_ReadLen ) ? self.m_ReadLen - data_len : 0;
            };
        },
        
        Record: function( options, payload_type, payload, payload_len ){
            var self = this;
            
            self.m_Seqnum      = 0;
            self.m_Options     = options;
            self.m_PayloadType = payload_type;
            self.m_Payload     = payload;
            self.m_PayloadLen  = payload_len;
            
            self.GetSeqnum = function(){
                return self.m_Seqnum;
            };
            self.SetSeqnum = function( value ){
                self.m_Seqnum = ( value >>> 0 );
            };
            
            self.GetOptions = function(){
                return self.m_Options;
            };
            
            self.GetPayloadType = function(){
                return self.m_PayloadType;
            };
            
            self.GetPayload = function(){
                return self.m_Payload;
            };
            
            self.GetPayloadLen = function(){
                return self.m_PayloadLen;
            };
            
            self.Read = function( input, input_len ){
                if ( input_len < 4 ) return false;
                
                var input_index = 0;
                var len = Mrs.Memory.GetUInt32( input, input_index );
                if ( len < self.GetMinLen() - 4 ) return false;
                input_index += 4;
                if ( input_len < 4 + len ) return false;
                
                self.m_Seqnum = Mrs.Memory.GetUInt32( input, input_index );
                input_index += 4;
                
                self.m_Options = Mrs.Memory.GetUInt16( input, input_index );
                input_index += 2;
                
                self.m_PayloadType = Mrs.Memory.GetUInt16( input, input_index );
                input_index += 2;
                
                self.m_PayloadLen = len - 4/* m_Seqnum */ - 2/* m_Options */ - 2/* m_PayloadType */;
                self.m_Payload = ( 0 < self.m_PayloadLen ) ? input.subarray( input_index, input_index + self.m_PayloadLen ) : null;
                return true;
            };
            
            self.GetMinLen = function(){
                return 4 + 4/* m_Seqnum */ + 2/* m_Options */ + 2/* m_PayloadType */;
            };
            
            self.GetMaxLen = function(){
                return self.GetMinLen() + self.m_PayloadLen;
            };
            
            self.ToString = function(){
                return "seqnum="+ self.m_Seqnum +" options=0x"+ self.m_Options.toString( 16 ).toUpperCase() +" payload=0x"+ self.m_PayloadType.toString( 16 ).toUpperCase() +"/"+ self.m_PayloadLen;
            };
        },
        
        Connection: function( type, addr, port, timeout_msec ){
            var self = this;
            
            self.m_Socket = null;
            self.GetSocket = function(){
                return self.m_Socket;
            };
            
            self.m_ConnectionId = 0;
            self.SetConnectionId = function( value ){
                self.m_ConnectionId = ( value >>> 0 );
            };
            
            self.m_ConnectionType = type;
            self.GetConnectionType = function(){
                return self.m_ConnectionType;
            };
            
            self.m_Data = 0;
            self.SetData = function( value ){
                self.m_Data = value;
            };
            self.GetData = function(){
                return self.m_Data;
            };
            
            self.m_ConnectTimerId = 0;
            self.StopConnectTimer = function(){
                if ( 0 !== self.m_ConnectTimerId ){
                    clearTimeout( self.m_ConnectTimerId );
                    self.m_ConnectTimerId = 0;
                }
            };
            
            self.m_IsConnected = false;
            self.IsConnected = function(){
                return self.m_IsConnected;
            };
            
            self.m_ReadBufferMaxSize = Mrs.READ_BUFFER_MAX_SIZE_DEFAULT;
            self.SetReadBufferMaxSize = function( value ){
                self.m_ReadBufferMaxSize = ( value >>> 0 );
            };
            self.GetReadBufferMaxSize = function(){
                return self.m_ReadBufferMaxSize;
            };
            
            self.m_ReadBuffer = new Mrs.Buffer();
            self.ReadRecord = function( record ){
                var payload = record.GetPayload();
                var payload_type = record.GetPayloadType();
                switch ( payload_type ){
                case Mrs.RpcId.VERSION_CHECK:{
                    var buffer = new Mrs.Buffer();
                    buffer.Write( payload );
                    var count = buffer.ReadUInt32();
                    for ( var i = 0; i < count; ++i ){
                        var key_len = buffer.ReadUInt16();
                        var key = Mrs.String.FromBytes( buffer.Read( key_len ) );
                        var value = buffer.ReadUInt32();
                        
                        self.SetRemoteVersion( key, value );
                    }
                    self.OnConnect();
                }break;
                
                case Mrs.RpcId.KEEP_ALIVE_REQUEST:{
                    record = new Mrs.Record( 0, Mrs.RpcId.KEEP_ALIVE_RESPONSE, null, 0 );
                    self.Write( record );
                }break;
                
                case Mrs.RpcId.KEEP_ALIVE_RESPONSE:{}break;
                
                case Mrs.RpcId.CONNECTION_CLOSE:{
                    Mrs.LOG_WARNING( "Unused: CONNECTION_CLOSE" );
                }break;
                
                case Mrs.RpcId.CONNECTION_CLOSE_HARD_LIMIT_OVER:{
                    var buffer = new Mrs.Buffer();
                    buffer.Write( payload );
                    var hard_limit = buffer.ReadUInt32();
                    Mrs.LOG_WARNING( "Peer connection num hard limit over: "+ hard_limit );
                    
                    self.OnError( Mrs.ConnectionError.PEER_CONNECTION_HARD_LIMIT_OVER );
                    self.OnDisconnect();
                }break;
                
                default:{
                    if ( payload_type < Mrs.RpcId.BEGIN ){
                        if ( self.m_IsConnected ) self.OnReadRecord( record.GetSeqnum(), record.GetOptions(), payload_type, payload, record.GetPayloadLen() );
                    }else{
                        Mrs.LOG_WARNING( "Mrs protocol error: MRS_PROTOCOL_VERSION="+ Mrs.PROTOCOL_VERSION +" payload_type="+ payload_type );
                        
                        self.OnError( Mrs.ConnectionError.PROTOCOL_ERROR );
                        self.OnDisconnect();
                    }
                }break;
                }
            };
            self.Read = function( bytes ){
                self.SetKeepAliveStatus( self.KeepAliveStatus.LIVING );
                var read_size = bytes.length;
                var read_buffer_max_size = self.GetReadBufferMaxSize();
                var status = Mrs.ConnectionError.CONNECTION_READBUF_SIZE_OVER;
                do{
                    if ( read_buffer_max_size < read_size ) break;
                    
                    self.m_ReadBuffer.Write( bytes );
                    read_size = self.m_ReadBuffer.GetDataLen();
                    if ( read_buffer_max_size < read_size ) break;
                    
                    var unread_size = read_size;
                    var record = new Mrs.Record( 0, 0, null, 0 );
                    var p = self.m_ReadBuffer.GetData();
                    while ( record.Read( p, unread_size ) ){
                        var len = record.GetMaxLen();
                        p = p.subarray( len, unread_size );
                        unread_size -= len;
                        self.ReadRecord( record );
                    }
                    if ( 4 <= unread_size ){
                        if ( Mrs.Memory.GetUInt32( p, 0 ) < record.GetMinLen() - 4 ){
                            status = Mrs.ConnectionError.READ_INVALID_RECORD_ERROR;
                            break;
                        }
                    }
                    
                    self.m_ReadBuffer.Delete( read_size - unread_size );
                    return;
                }while ( false );
                
                if ( Mrs.ConnectionError.CONNECTION_READBUF_SIZE_OVER == status ){
                    Mrs.LOG_DEBUG( "Read buffer max size over: "+ read_size +"/"+ read_buffer_max_size );
                }
                self.OnError( status );
                self.OnDisconnect();
                return;
            };
            
            self.m_Seqnum = 0;
            self.UpdateSeqnum = function(){
                var seqnum = ++self.m_Seqnum;
                if ( 0xFFFFFFFF == seqnum ) self.m_Seqnum = 0;
                return seqnum;
            };
            
            self.Write = function( record ){
                if ( null == self.m_Socket ) return false;
                
                record.SetSeqnum( self.UpdateSeqnum() );
                
                var write_bytes = new Uint8Array( record.GetMaxLen() );
                var write_index = 0;
                
                Mrs.Memory.SetUInt32( write_bytes, write_index, write_bytes.length - 4 );
                write_index += 4;
                
                Mrs.Memory.SetUInt32( write_bytes, write_index, record.GetSeqnum() );
                write_index += 4;
                
                Mrs.Memory.SetUInt16( write_bytes, write_index, record.GetOptions() );
                write_index += 2;
                
                Mrs.Memory.SetUInt16( write_bytes, write_index, record.GetPayloadType() );
                write_index += 2;
                
                if ( 0 < record.GetPayloadLen() ){
                    Mrs.Memory.Set( write_bytes, write_index, record.GetPayload() );
                }
                
                self.m_Socket.send( write_bytes.buffer );
                return true;
            };
            
            self.m_ConnectCallback = 0;
            self.SetConnectCallback = function( value ){
                self.m_ConnectCallback = value;
            };
            self.OnConnect = function(){
                self.m_IsConnected = true;
                self.SetKeepAliveStatus( self.KeepAliveStatus.LIVING );
                if ( 0 !== self.m_ConnectCallback ) Runtime.dynCall( "vii", self.m_ConnectCallback, [ self.m_ConnectionId, self.m_Data ] );
            };
            
            self.m_DisconnectCallback = 0;
            self.SetDisconnectCallback = function( value ){
                self.m_DisconnectCallback = value;
            };
            self.OnDisconnect = function(){
                if ( self.m_IsConnected ){
                    self.m_IsConnected = false;
                    self.SetKeepAliveStatus( self.KeepAliveStatus.NONE );
                    if ( 0 !== self.m_DisconnectCallback ) Runtime.dynCall( "vii", self.m_DisconnectCallback, [ self.m_ConnectionId, self.m_Data ] );
                }
                if ( null != self.m_Socket ) Mrs.ConnectionManager.CloseConnection( self.m_ConnectionId );
            };
            
            self.m_ReadRecordCallback = 0;
            self.SetReadRecordCallback = function( value ){
                self.m_ReadRecordCallback = value;
            };
            self.OnReadRecord = function( seqnum, options, payload_type, payload, payload_len ){
                if ( 0 !== self.m_ReadRecordCallback ) Runtime.dynCall( "viiiiiii", self.m_ReadRecordCallback, [ self.m_ConnectionId, self.m_Data, seqnum, options, payload_type, allocate( payload, "i8", ALLOC_STACK ), payload_len ] );
            };
            
            self.m_ErrorCallback = 0;
            self.SetErrorCallback = function( value ){
                self.m_ErrorCallback = value;
            };
            self.OnError = function( status ){
                if ( 0 !== self.m_ErrorCallback ) Runtime.dynCall( "viii", self.m_ErrorCallback, [ self.m_ConnectionId, self.m_Data, status ] );
            };
            
            self.m_IsClosed = false;
            self.IsClosed = function( value ){
                if ( undefined === value ) return self.m_IsClosed;
                
                self.m_IsClosed = value;
            };
            
            self.Close = function(){
                self.StopConnectTimer();
                if ( null != self.m_Socket ){
                    self.m_Socket.close();
                    self.m_Socket = null;
                    Mrs.ConnectionManager.AddConnectionNum( -1 );
                }
            };
            
            self.m_RemoteVersions = {};
            self.SetRemoteVersion = function( key, value ){
                self.m_RemoteVersions[ key ] = ( value >>> 0 );
            };
            self.GetRemoteVersion = function( key ){
                return ( key in self.m_RemoteVersions ) ? self.m_RemoteVersions[ key ] : 0;
            };
            
            self.KeepAliveStatus = {
                NONE     : 0,
                IDLE     : 1,
                CHECKING : 2,
                LIVING   : 3,
            };
            
            self.m_KeepAliveStatus = self.KeepAliveStatus.NONE;
            self.SetKeepAliveStatus = function( value ){
                self.m_KeepAliveStatus = value;
            };
            self.GetKeepAliveStatus = function(){
                return self.m_KeepAliveStatus;
            };
            
            switch ( type ){
            case Mrs.ConnectionType.WS:{
                self.m_Socket = new WebSocket( "ws://"+ addr +":"+ port, [] );
            }break;
            
            case Mrs.ConnectionType.WSS:{
                self.m_Socket = new WebSocket( "wss://"+ addr +":"+ port, [] );
            }break;
            
            default:{
                Mrs.s_Error = Mrs.Error.ECONNECTIONTYPE;
            }break;
            }
            
            if ( null != self.m_Socket ){
                Mrs.ConnectionManager.AddConnectionNum( 1 );
                self.m_Socket.onopen = function(){
                    self.StopConnectTimer();
                    
                    var buffer = Mrs.Version.GetBuffer();
                    var record = new Mrs.Record( 0, Mrs.RpcId.VERSION_CHECK, buffer.GetData(), buffer.GetDataLen() );
                    self.Write( record );
                };
                
                self.m_Socket.onerror = function( evt ){
                    if ( null == self.m_Socket ) return;
                    
                    if ( ( 0 !== self.m_ConnectTimerId ) && ! self.m_IsConnected ) self.OnError( Mrs.ConnectionError.CONNECT_ERROR );
                    self.OnDisconnect();
                };
                
                self.m_Socket.onmessage = function( evt ){
                    if ( evt.data instanceof Blob ){
                        var reader = new FileReader();
                        reader.onload = function(){
                            if ( null != self.m_Socket ) self.Read( new Uint8Array( reader.result ) );
                        };
                        reader.readAsArrayBuffer( evt.data );
                    }
                };
                
                self.m_Socket.onclose = function( evt ){
                    switch ( evt.code ){
                    case 1000:
                    case 1006:{}break;
                    
                    default:{
                        Mrs.LOG_DEBUG( "WebSocket.onclose code="+ evt.code );
                    }break;
                    }
                    self.OnDisconnect();
                };
                
                self.m_ConnectTimerId = setTimeout( function(){
                    if ( null == self.m_Socket ) return;
                    
                    self.m_ConnectTimerId = 0;
                    self.OnError( Mrs.ConnectionError.CONNECT_TIMEOUT );
                    self.OnDisconnect();
                }, timeout_msec );
            }
        },
        
        ConnectionManager: {
            s_AutoConnectionId: 0,
            s_ConnectionIds: {},
            s_ConnectionNumHardLimit: 100,
            s_ConnectionNumSoftLimit: 100,
            s_ConnectionNum: 0,
            s_KeepAliveUpdateMsec: 10000,
            s_KeepAliveUpdateTimerId: 0,
            
            Initialize: function(){
                Mrs.ConnectionManager.Clear();
            },
            
            Finalize: function(){
                if ( 0 !== Mrs.ConnectionManager.s_KeepAliveUpdateTimerId ){
                    clearTimeout( Mrs.ConnectionManager.s_KeepAliveUpdateTimerId );
                    Mrs.ConnectionManager.s_KeepAliveUpdateTimerId = 0;
                }
                Mrs.ConnectionManager.Clear();
            },
            
            GenerateConnectionId: function(){
                var start = Mrs.ConnectionManager.s_AutoConnectionId;
                do{
                    var value = ++Mrs.ConnectionManager.s_AutoConnectionId;
                    if ( 0xFFFFFFFF === Mrs.ConnectionManager.s_AutoConnectionId ){
                        Mrs.ConnectionManager.s_AutoConnectionId = 0;
                    }
                    
                    if ( ! ( value in Mrs.ConnectionManager.s_ConnectionIds ) ) return value;
                }while ( start !== value );
                return 0;
            },
            
            AddConnection: function( connection ){
                if ( null == connection.GetSocket() ) return 0;
                
                var connection_id = Mrs.ConnectionManager.GenerateConnectionId();
                if ( 0 === connection_id ){
                    Mrs.s_Error = Mrs.Error.ENOMEM;
                    return 0;
                }
                
                if ( Mrs.ConnectionManager.s_ConnectionNumSoftLimit < Mrs.ConnectionManager.s_ConnectionNum ){
                    Mrs.LOG_INFO( "Connection num soft limit over: "+ Mrs.ConnectionManager.s_ConnectionNum +"/"+ Mrs.ConnectionManager.s_ConnectionNumSoftLimit );
                }
                
                connection.SetConnectionId( connection_id );
                Mrs.ConnectionManager.s_ConnectionIds[ connection_id ] = connection;
                return connection_id;
            },
            
            GetConnection: function( connection_id ){
                return ( connection_id in Mrs.ConnectionManager.s_ConnectionIds ) ? Mrs.ConnectionManager.s_ConnectionIds[ connection_id ] : null;
            },
            
            CloseConnection: function( connection_id ){
                var connection = Mrs.ConnectionManager.GetConnection( connection_id );
                if ( null == connection ) return;
                if ( connection.IsClosed() ) return;
                
                connection.IsClosed( true );
                connection.OnDisconnect();
                
                delete Mrs.ConnectionManager.s_ConnectionIds[ connection_id ];
                connection.Close();
            },
            
            Clear: function(){
                Mrs.ConnectionManager.s_AutoConnectionId = 0;
                for ( var connection_id in Mrs.ConnectionManager.s_ConnectionIds ){
                    var connection = Mrs.ConnectionManager.GetConnection( connection_id );
                    connection.Close();
                }
                Mrs.ConnectionManager.s_ConnectionIds = {};
            },
            
            GetConnectionNumHardLimit: function(){
                return Mrs.ConnectionManager.s_ConnectionNumHardLimit;
            },
            
            GetConnectionNumSoftLimit: function(){
                return Mrs.ConnectionManager.s_ConnectionNumSoftLimit;
            },
            
            SetConnectionNumSoftLimit: function( value ){
                Mrs.ConnectionManager.s_ConnectionNumSoftLimit = ( value >>> 0 );
            },
            
            GetConnectionNum: function(){
                return Mrs.ConnectionManager.s_ConnectionNum;
            },
            
            AddConnectionNum: function( value ){
                Mrs.ConnectionManager.s_ConnectionNum += value;
            },
            
            SetKeepAliveUpdateMsec: function( value ){
                Mrs.ConnectionManager.s_KeepAliveUpdateMsec = ( value >>> 0 );
            },
            
            GetKeepAliveUpdateMsec: function(){
                return Mrs.ConnectionManager.s_KeepAliveUpdateMsec;
            },
            
            Update: function(){
                if ( 0 !== Mrs.ConnectionManager.s_KeepAliveUpdateTimerId ) return;
                
                Mrs.ConnectionManager.s_KeepAliveUpdateTimerId = setTimeout( function(){
                    Mrs.ConnectionManager.s_KeepAliveUpdateTimerId = 0;
                    
                    var connection_ids = Object.keys( Mrs.ConnectionManager.s_ConnectionIds );
                    for ( var i = 0; i < connection_ids.length; ++i ){
                        var connection_id = connection_ids[ i ];
                        var connection = Mrs.ConnectionManager.GetConnection( connection_id );
                        if ( null == connection ) continue;
                        
                        switch ( connection.GetKeepAliveStatus() ){
                        case connection.KeepAliveStatus.IDLE:{
                            connection.SetKeepAliveStatus( connection.KeepAliveStatus.CHECKING );
                            var record = new Mrs.Record( 0, Mrs.RpcId.KEEP_ALIVE_REQUEST, null, 0 );
                            connection.Write( record );
                        }break;
                        
                        case connection.KeepAliveStatus.CHECKING:{
                            connection.OnError( Mrs.ConnectionError.KEEPALIVE_TIMEOUT );
                            connection.OnDisconnect();
                        }break;
                        
                        case connection.KeepAliveStatus.LIVING:{
                            connection.SetKeepAliveStatus( connection.KeepAliveStatus.IDLE );
                        }break;
                        
                        default:{}break;
                        }
                    }
                }, Mrs.ConnectionManager.GetKeepAliveUpdateMsec() );
            },
            
            UpdateKeepAlive: function(){
                var connection_ids = Object.keys( Mrs.ConnectionManager.s_ConnectionIds );
                for ( var i = 0; i < connection_ids.length; ++i ){
                    var connection_id = connection_ids[ i ];
                    var connection = Mrs.ConnectionManager.GetConnection( connection_id );
                    if ( null == connection ) continue;
                    
                    switch ( connection.GetKeepAliveStatus() ){
                    case connection.KeepAliveStatus.IDLE:
                    case connection.KeepAliveStatus.CHECKING:
                    case connection.KeepAliveStatus.LIVING:{
                        connection.SetKeepAliveStatus( connection.KeepAliveStatus.CHECKING );
                        var record = new Mrs.Record( 0, Mrs.RpcId.KEEP_ALIVE_REQUEST, null, 0 );
                        connection.Write( record );
                    }break;
                    
                    default:{}break;
                    }
                }
            },
        },
        
        Version: {
            s_Versions: {},
            s_VersionsBuffer: null,
            
            GetBuffer: function(){
                return Mrs.Version.s_VersionsBuffer;
            },
            
            Set: function( key, value ){
                Mrs.Version.s_Versions[ key ] = value;
                
                var buffer = new Mrs.Buffer();
                buffer.WriteUInt32( Object.keys( Mrs.Version.s_Versions ).length );
                for ( var key in Mrs.Version.s_Versions ){
                    var key_buffer = new Mrs.Buffer();
                    key_buffer.Write( Mrs.String.ToBytes( key ) );
                    buffer.WriteUInt16( key_buffer.GetDataLen() );
                    buffer.Write( key_buffer.GetData() );
                    buffer.WriteUInt32( Mrs.Version.s_Versions[ key ] );
                }
                Mrs.Version.s_VersionsBuffer = buffer;
            },
            
            Get: function( key ){
                return ( key in Mrs.Version.s_Versions ) ? Mrs.Version.s_Versions[ key ] : 0;
            },
        },
        
        mrs_initialize: function(){
            if ( Mrs.s_Initialized ) return true;
            
            Mrs.s_Error = Mrs.Error.NO_ERROR;
            Mrs.ConnectionManager.Initialize();
            Mrs.s_Initialized = true;
            return true;
        },
        
        mrs_update: function(){
            Mrs.ConnectionManager.Update();
        },
        
        mrs_update_keep_alive: function(){
            Mrs.ConnectionManager.UpdateKeepAlive();
        },
        
        mrs_finalize: function(){
            if ( ! Mrs.s_Initialized ) return;
            
            Mrs.ConnectionManager.Finalize();
            Mrs.s_Initialized = false;
        },
        
        mrs_get_connection_num_hard_limit: function(){
            return Mrs.ConnectionManager.GetConnectionNumHardLimit();
        },
        
        mrs_get_connection_num_soft_limit: function(){
            return Mrs.ConnectionManager.GetConnectionNumSoftLimit();
        },
        
        mrs_set_connection_num_soft_limit: function( value ){
            Mrs.ConnectionManager.SetConnectionNumSoftLimit( value );
        },
        
        mrs_get_connection_num: function(){
            return Mrs.ConnectionManager.GetConnectionNum();
        },
        
        mrs_connect: function( type, addr, port, timeout_msec ){
            if ( typeof addr !== "string" ) addr = Pointer_stringify( addr );
            port = port & 0xFFFF;
            
            var connection = new Mrs.Connection( type, addr, port, timeout_msec );
            var connection_id = Mrs.ConnectionManager.AddConnection( connection );
            if ( 0 == connection_id ) connection.Close();
            return connection_id;
        },
        
        mrs_set_connect_callback: function( connection, _callback ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            if ( null != connection ) connection.SetConnectCallback( _callback );
        },
        
        mrs_set_disconnect_callback: function( connection, _callback ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            if ( null != connection ) connection.SetDisconnectCallback( _callback );
        },
        
        mrs_set_error_callback: function( connection, _callback ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            if ( null != connection ) connection.SetErrorCallback( _callback );
        },
        
        mrs_set_read_record_callback: function( connection, _callback ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            if ( null != connection ) connection.SetReadRecordCallback( _callback );
        },
        
        mrs_connection_set_data: function( connection, connection_data ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            if ( null == connection ) return false;
            
            connection.SetData( connection_data );
            return true;
        },
        
        mrs_connection_get_data: function( connection ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            return ( null == connection ) ? 0 : connection.GetData();
        },
        
        mrs_connection_is_connected: function( connection ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            return ( null == connection ) ? false : connection.IsConnected();
        },
        
        mrs_connection_set_readbuf_max_size: function( connection, value ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            if ( null == connection ) return false;
            
            connection.SetReadBufferMaxSize( value );
            return true;
        },
        
        mrs_connection_get_readbuf_max_size: function( connection ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            return ( null == connection ) ? 0 : connection.GetReadBufferMaxSize();
        },
        
        mrs_connection_get_type: function( connection ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            return ( null == connection ) ? Mrs.ConnectionType.NONE : connection.GetConnectionType();
        },
        
        mrs_write_record: function( connection, options, payload_type, payload, payload_len ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            if ( null == connection ) return false;
            
            if ( ! ( payload instanceof Uint8Array ) ) payload = new Uint8Array( HEAPU8.buffer, payload, payload_len );
            var record = new Mrs.Record( options, payload_type, payload, payload_len );
            return connection.Write( record );
        },
        
        mrs_close: function( connection ){
            Mrs.ConnectionManager.CloseConnection( connection );
        },
        
        mrs_set_log_callback: function( _callback ){
            Mrs.s_LogOutputCallback = _callback;
        },
        
        mrs_get_last_error: function(){
            return Mrs.s_Error;
        },
        
        mrs_get_error_string: function( error ){
            var msg = "";
            switch ( error ){
            case Mrs.Error.NO_ERROR:        { msg = "MRS_NO_ERROR: No error"; }break;
            case Mrs.Error.ENOMEM:          { msg = "MRS_ENOMEM: Out of memory"; }break;
            case Mrs.Error.EACCES:          { msg = "MRS_EACCES: Permission denied"; }break;
            case Mrs.Error.EMFILE:          { msg = "MRS_EMFILE: Too many open files"; }break;
            case Mrs.Error.EADDRINUSE:      { msg = "MRS_EADDRINUSE: Address already in use"; }break;
            case Mrs.Error.EADDRNOTAVAIL:   { msg = "MRS_EADDRNOTAVAIL: Cannot assign requested address"; }break;
            case Mrs.Error.ENETUNREACH:     { msg = "MRS_ENETUNREACH: Network is unreachable"; }break;
            case Mrs.Error.EHOSTUNREACH:    { msg = "MRS_EHOSTUNREACH: No route to host"; }break;
            case Mrs.Error.ECONNECTIONTYPE: { msg = "MRS_ECONNECTIONTYPE: Invalid connection type"; }break;
            case Mrs.Error.EBACKLOG:        { msg = "MRS_EBACKLOG: Invalid backlog"; }break;
            case Mrs.Error.ECONNECTIONNUM:  { msg = "MRS_ECONNECTIONNUM: Limit over connection num"; }break;
            
            default:                        { msg = "Undefined error: "+ error; }break;
            }
            return allocate( intArrayFromString( msg ), "i8", ALLOC_NORMAL );
        },
        
        mrs_get_connection_error_string: function( error ){
            var msg = "";
            switch ( error ){
            case Mrs.ConnectionError.CONNECT_ERROR:                   { msg = "MRS_CONNECT_ERROR: Connect error"; }break;
            case Mrs.ConnectionError.CONNECT_TIMEOUT:                 { msg = "MRS_CONNECT_TIMEOUT: Connect timeout"; }break;
            case Mrs.ConnectionError.WRITE_ERROR:                     { msg = "MRS_WRITE_ERROR: Write error"; }break;
            case Mrs.ConnectionError.KEY_EXCHANGE_REQUEST_ERROR:      { msg = "MRS_KEY_EXCHANGE_REQUEST_ERROR: Key exchange request error"; }break;
            case Mrs.ConnectionError.KEY_EXCHANGE_RESPONSE_ERROR:     { msg = "MRS_KEY_EXCHANGE_RESPONSE_ERROR: Key exchange response error"; }break;
            case Mrs.ConnectionError.PEER_CONNECTION_HARD_LIMIT_OVER: { msg = "MRS_PEER_CONNECTION_HARD_LIMIT_OVER: Peer connection hard limit over"; }break;
            case Mrs.ConnectionError.CONNECTION_READBUF_SIZE_OVER:    { msg = "MRS_CONNECTION_READBUF_SIZE_OVER: Connection readbuf size over"; }break;
            case Mrs.ConnectionError.KEEPALIVE_TIMEOUT:               { msg = "MRS_KEEPALIVE_TIMEOUT: Keepalive timeout"; }break;
            case Mrs.ConnectionError.PROTOCOL_ERROR:                  { msg = "MRS_PROTOCOL_ERROR: Protocol error"; }break;
            case Mrs.ConnectionError.READ_INVALID_RECORD_ERROR:       { msg = "MRS_READ_INVALID_RECORD_ERROR: Read invalid record error"; }break;
            case Mrs.ConnectionError.LISTEN_ERROR:                    { msg = "MRS_LISTEN_ERROR: Listen error"; }break;
            case Mrs.ConnectionError.RESOLVE_ADDRESS_ERROR:           { msg = "MRS_RESOLVE_ADDRESS_ERROR: Resolve address error"; }break;
            case Mrs.ConnectionError.RESOLVE_ADDRESS_TIMEOUT:         { msg = "MRS_RESOLVE_ADDRESS_TIMEOUT: Resolve address timeout"; }break;
            
            default:                                                  { msg = "Undefined connection error: "+ error; }break;
            }
            return allocate( intArrayFromString( msg ), "i8", ALLOC_NORMAL );
        },
        
        mrs_set_keep_alive_update_msec: function( update_msec ){
            Mrs.ConnectionManager.SetKeepAliveUpdateMsec( update_msec );
        },
        
        mrs_get_keep_alive_update_msec: function(){
            return Mrs.ConnectionManager.GetKeepAliveUpdateMsec();
        },
        
        mrs_set_version: function( key, value ){
            if ( typeof key !== "string" ) key = Pointer_stringify( key );
            Mrs.Version.Set( key, value );
        },
        
        mrs_get_version: function( key ){
            if ( typeof key !== "string" ) key = Pointer_stringify( key );
            return Mrs.Version.Get( key );
        },
        
        mrs_connection_get_remote_version: function( connection, key ){
            var connection_id = connection;
            connection = Mrs.ConnectionManager.GetConnection( connection_id );
            return ( null != connection ) ? connection.GetRemoteVersion( Pointer_stringify( key ) ) : 0;
        },
    },
    
    mrs_initialize: function(){
        return Mrs.mrs_initialize.apply( this, arguments );
    },
    
    mrs_update: function(){
        Mrs.mrs_update.apply( this, arguments );
    },
    
    mrs_update_keep_alive: function(){
        Mrs.mrs_update_keep_alive.apply( this, arguments );
    },
    
    mrs_finalize: function(){
        Mrs.mrs_finalize.apply( this, arguments );
    },
    
    mrs_get_connection_num_hard_limit: function(){
        return Mrs.mrs_get_connection_num_hard_limit.apply( this, arguments );
    },
    
    mrs_get_connection_num_soft_limit: function(){
        return Mrs.mrs_get_connection_num_soft_limit.apply( this, arguments );
    },
    
    mrs_set_connection_num_soft_limit: function(){
        Mrs.mrs_set_connection_num_soft_limit.apply( this, arguments );
    },
    
    mrs_get_connection_num: function(){
        return Mrs.mrs_get_connection_num.apply( this, arguments );
    },
    
    mrs_connect: function(){
        return Mrs.mrs_connect.apply( this, arguments );
    },
    
    mrs_set_connect_callback: function(){
        Mrs.mrs_set_connect_callback.apply( this, arguments );
    },
    
    mrs_set_disconnect_callback: function(){
        Mrs.mrs_set_disconnect_callback.apply( this, arguments );
    },
    
    mrs_set_error_callback: function(){
        Mrs.mrs_set_error_callback.apply( this, arguments );
    },
    
    mrs_set_read_record_callback: function(){
        Mrs.mrs_set_read_record_callback.apply( this, arguments );
    },
    
    mrs_connection_set_data: function(){
        return Mrs.mrs_connection_set_data.apply( this, arguments );
    },
    
    mrs_connection_get_data: function(){
        return Mrs.mrs_connection_get_data.apply( this, arguments );
    },
    
    mrs_connection_is_connected: function(){
        return Mrs.mrs_connection_is_connected.apply( this, arguments );
    },
    
    mrs_connection_set_readbuf_max_size: function(){
        return Mrs.mrs_connection_set_readbuf_max_size.apply( this, arguments );
    },
    
    mrs_connection_get_readbuf_max_size: function(){
        return Mrs.mrs_connection_get_readbuf_max_size.apply( this, arguments );
    },
    
    mrs_connection_get_type: function(){
        return Mrs.mrs_connection_get_type.apply( this, arguments );
    },
    
    mrs_write_record: function(){
        return Mrs.mrs_write_record.apply( this, arguments );
    },
    
    mrs_close: function(){
        Mrs.mrs_close.apply( this, arguments );
    },
    
    mrs_set_log_callback: function(){
        Mrs.mrs_set_log_callback.apply( this, arguments );
    },
    
    mrs_get_last_error: function(){
        return Mrs.mrs_get_last_error.apply( this, arguments );
    },
    
    mrs_get_error_string: function(){
        return Mrs.mrs_get_error_string.apply( this, arguments );
    },
    
    mrs_get_connection_error_string: function(){
        return Mrs.mrs_get_connection_error_string.apply( this, arguments );
    },
    
    mrs_set_keep_alive_update_msec: function(){
        Mrs.mrs_set_keep_alive_update_msec.apply( this, arguments );
    },
    
    mrs_get_keep_alive_update_msec: function(){
        return Mrs.mrs_get_keep_alive_update_msec.apply( this, arguments );
    },
    
    mrs_set_version: function(){
        Mrs.mrs_set_version.apply( this, arguments );
    },
    
    mrs_get_version: function(){
        return Mrs.mrs_get_version.apply( this, arguments );
    },
    
    mrs_connection_get_remote_version: function(){
        return Mrs.mrs_connection_get_remote_version.apply( this, arguments );
    },
    
// [webgl only functions]
    __mrs_setup__: function(){
        Mrs.mrs_set_version( Mrs.VERSION_KEY, Mrs.VERSION );
    },
    
// [unsupported functions]
    mrs_run: function( sleep_msec ){
        Mrs.LOG_ERR( "Unsupported function: mrs_run" );
    },
    
    mrs_stop_running: function(){
        Mrs.LOG_ERR( "Unsupported function: mrs_stop_running" );
    },
    
    mrs_server_get_connection_num: function(){
        Mrs.LOG_ERR( "Unsupported function: mrs_server_get_connection_num" );
        return 0;
    },
    
    mrs_server_create: function( type, addr, port, backlog ){
        Mrs.LOG_ERR( "Unsupported function: mrs_server_create" );
        return 0;
    },
    
    mrs_server_set_new_connection_callback: function( server, _callback ){
        Mrs.LOG_ERR( "Unsupported function: mrs_server_set_new_connection_callback" );
    },
    
    mrs_server_set_data: function( server, server_data ){
        Mrs.LOG_ERR( "Unsupported function: mrs_server_set_data" );
        return false;
    },
    
    mrs_server_get_data: function( server ){
        Mrs.LOG_ERR( "Unsupported function: mrs_server_get_data" );
        return 0;
    },
    
    mrs_cipher_create: function( type ){
        Mrs.LOG_ERR( "Unsupported function: mrs_cipher_create" );
        return 0;
    },
    
    mrs_set_cipher: function( connection, cipher ){
        Mrs.LOG_ERR( "Unsupported function: mrs_set_cipher" );
    },
    
    mrs_key_exchange: function( connection, _callback ){
        Mrs.LOG_ERR( "Unsupported function: mrs_key_exchange" );
        return false;
    },
    
    mrs_sleep: function( sleep_msec ){
        Mrs.LOG_ERR( "Unsupported function: mrs_sleep" );
    },
    
    mrs_set_ssl_certificate_data: function( data ){
        Mrs.LOG_ERR( "Unsupported function: mrs_set_ssl_certificate_data" );
    },
    
    mrs_set_ssl_private_key_data: function( data ){
        Mrs.LOG_ERR( "Unsupported function: mrs_set_ssl_private_key_data" );
    },
    
    mrs_udp_set_mtu: function( value ){
        Mrs.LOG_ERR( "Unsupported function: mrs_udp_set_mtu" );
    },
    
    mrs_udp_get_mtu: function(){
        Mrs.LOG_ERR( "Unsupported function: mrs_udp_get_mtu" );
        return 0;
    },
};
autoAddDeps( Mrs, '$Mrs' );
mergeInto( LibraryManager.library, Mrs );
