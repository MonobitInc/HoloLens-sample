#ifndef __MRS_COMMON_HPP__
#define __MRS_COMMON_HPP__

#if defined( _MSC_VER )
# include <Winsock2.h>
# include <windows.h>
# define MRS_WINDOWS

# if _MSC_VER < 1900
#  define snprintf( _buf, _buf_size, _format, ... ) _snprintf_s( _buf, _buf_size, _TRUNCATE, _format, ##__VA_ARGS__ )
# endif

# if defined( MRS_API_EXPORT )
#  define MRS_API __declspec(dllexport)
# elif defined( MRS_API_IMPORT )
#  define MRS_API __declspec(dllimport)
# else
#  define MRS_API
# endif

# ifndef MRS_API_CC
# define MRS_API_CC _cdecl
# endif
#else
# define MRS_API
# define MRS_API_CC
#endif

#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <stdlib.h>
#include <stdarg.h>
#include <time.h>
#include <string>
#include <errno.h>
#include <signal.h>

#ifdef MRS_WINDOWS
# include <sys/timeb.h>
#else
# include <sys/time.h>
#endif

typedef int8_t             int8;
typedef int16_t            int16;
typedef int32_t            int32;
typedef long long          int64;

typedef uint8_t            uint8;
typedef uint16_t           uint16;
typedef uint32_t           uint32;
typedef unsigned long long uint64;

#define MRS_MALLOC( ... )  malloc( __VA_ARGS__ )
#define MRS_REALLOC( ... ) realloc( __VA_ARGS__ )
#define MRS_FREE( _p ) { if ( NULL != _p ){ free( _p ); _p = NULL; } }

#define MRS_NEW( _type, ... ) new _type( __VA_ARGS__ )
#define MRS_DELETE( _p ) { if ( NULL != _p ){ delete _p; _p = NULL; } }

/**
 * @head2 関数の引数に使う各種の列挙体
 */

/**
 * @enum MrsLogLevel
 * @brief ログ出力レベルの種類を定義します
 * syslogが用いている、RFC 5424での定義に準拠しています。
 */
enum MrsLogLevel {
    MRS_LOG_LEVEL_EMERG   = 0, // @member システムが使用不可
    MRS_LOG_LEVEL_ALERT   = 1, // @member 直ちに行動を起こさなければならない
    MRS_LOG_LEVEL_CRIT    = 2, // @member 危険な状態
    MRS_LOG_LEVEL_ERR     = 3, // @member エラー
    MRS_LOG_LEVEL_WARNING = 4, // @member 警告
    MRS_LOG_LEVEL_NOTICE  = 5, // @member 通常だが重要な状態
    MRS_LOG_LEVEL_INFO    = 6, // @member 参考にすべきメッセージ
    MRS_LOG_LEVEL_DEBUG   = 7, // @member デバッグレベルのメッセージ
};

/**
 * @enum MrsConnectionType
 * @brief MRSで利用可能なトランスポート層プロトコルの種類を定義します
 */
enum MrsConnectionType {
    MRS_CONNECTION_TYPE_NONE = 0, // @member 未定義
    MRS_CONNECTION_TYPE_TCP  = 1, // @member TCP
    MRS_CONNECTION_TYPE_UDP  = 2, // @member UDP(RUDPを使う場合はこの値を選択します)
    MRS_CONNECTION_TYPE_WS   = 3, // @member WS(通常のWebSocketを使う場合はこの値を選択します)
    MRS_CONNECTION_TYPE_WSS  = 4, // @member WSS(通信経路が暗号化されたWebSocketを使う場合はこの値を選択します)
};

/**
 * @enum MrsCipherType
 * @brief 暗号化通信に使う鍵交換の方式を定義します
 */
enum MrsCipherType {
    MRS_CIPHER_TYPE_NONE = 0, // @member 未定義
    MRS_CIPHER_TYPE_ECDH = 1, // @member ECDH(楕円曲線ディフィーヘルマン)
};

/**
 * @enum MrsError
 * @brief MRS APIのエラーコードを定義します
 */
enum MrsError {
    MRS_NO_ERROR        = 0,  // @member エラーなし
    
    MRS_ENOENT          = 2,  // @member ファイルやディレクトリが存在しない
    MRS_ENOMEM          = 12, // @member メモリが不足している
    MRS_EACCES          = 13, // @member アクセス権限が不足している(1024以下のポート番号など)
    MRS_EMFILE          = 24, // @member これ以上ファイル(ソケット)を開けない(ulimitの設定が必要です)
    MRS_EADDRINUSE      = 48, // @member ポート番号がすでに使われている
    MRS_EADDRNOTAVAIL   = 49, // @member クライアント側で必要なポート番号を使い切っている
    MRS_ENETUNREACH     = 51, // @member 到達できないネットワークである
    MRS_EHOSTUNREACH    = 65, // @member 到達できないホストである
    
    MRS_ECONNECTIONTYPE = 0xF001, // @member 不正な接続の種別コード (MrsConnectionの値が不正)
    MRS_EBACKLOG        = 0xF002, // @member mrs_server_create 関数のbacklogの値が不正(大きすぎる)
    MRS_ECONNECTIONNUM  = 0xF003, // @member mrs_get_connection_num_hard_limit 関数の値より多いクライアントを生成した
};

/**
 * @enum MrsConnectionError
 * @brief MrsConnectionに設定したエラーコールバック関数に渡されるエラー種別コードを定義します
 */
enum MrsConnectionError {
    MRS_CONNECT_ERROR                   = 1,  // @member MrsConnectionにおいてTCPのconnect()に失敗した(サーバがポートを開いていないなど)
    MRS_CONNECT_TIMEOUT                 = 2,  // @member MrsConnectionにおいてTCPのconnect()の時間切れ(サーバマシン自体が起動していないか、アドレスを間違えているなど)
    MRS_WRITE_ERROR                     = 3,  // @member 接続に対して書き込みができなかった
    MRS_KEY_EXCHANGE_REQUEST_ERROR      = 4,  // @member 鍵交換の開始要求を送信できなかった
    MRS_KEY_EXCHANGE_RESPONSE_ERROR     = 5,  // @member 鍵交換の返信を送信できなかった
    MRS_PEER_CONNECTION_HARD_LIMIT_OVER = 6,  // @member 接続相手がコネクション上限に達している
    MRS_CONNECTION_READBUF_SIZE_OVER    = 7,  // @member 読み込みバッファを越えたデータを受信した
    MRS_KEEPALIVE_TIMEOUT               = 8,  // @member 一定時間無通信状態によるタイムアウトを検出した
    MRS_PROTOCOL_ERROR                  = 9,  // @member 接続相手のプロトコルが不正
    MRS_READ_INVALID_RECORD_ERROR       = 10, // @member 読み込んだレコードが不正
    MRS_LISTEN_ERROR                    = 11, // @member ソケットの待ち受け処理に失敗した(既に同じポートが使われているなど)
    MRS_RESOLVE_ADDRESS_ERROR           = 12, // @member アドレス解決処理に失敗した
    MRS_RESOLVE_ADDRESS_TIMEOUT         = 13, // @member アドレス解決処理の時間切れ
};

/**
 * @enum MrsRecordOption
 * @brief レコードの送信オプション
 * mrs_write_record関数のoptionsに与える送信オプションは、この列挙体で定義している値を OR'|'演算子で並べて設定します。
 * 例: ( MRS_RECORD_OPTION_ON_CRYPT | MRS_RECORD_OPTION_UDP_UNRELIABLE )
 * オプションは任意の組み合わせが可能です。
 */
enum MrsRecordOption {
    MRS_RECORD_OPTION_NONE            = 0x00, // @member オプションを設定しない
    MRS_RECORD_OPTION_ON_CRYPT        = 0x01, // @member レコードを暗号化して送信する
    MRS_RECORD_OPTION_UDP_UNRELIABLE  = 0x02, // @member UDPにおいて信頼性保証(再送処理)をせずにレコードを送信する
    MRS_RECORD_OPTION_UDP_UNSEQUENCED = 0x04, // @member UDPにおいて到着順序保証をせずにレコードを送信する
};

/**
 * @enum MrsPayloadType
 * @brief アプリケーションが利用可能なペイロードの種別コードの範囲を定義します
 */
enum MrsPayloadType {
    MRS_PAYLOAD_TYPE_BEGIN = 0x00, // @member 0x00. アプリケーションが利用可能な最小の値
    MRS_PAYLOAD_TYPE_END   = 0xFF, // @member 0xff. アプリケーションが利用可能な最大の値
};

typedef void* MrsServer;
typedef void* MrsConnection;
typedef void* MrsCipher;

/**
 * @head2 関数の引数に与えるコールバック関数
 */

/**
 * @cbfn MrsLogOutputCallback
 * @brief ログが1回出力されるごとに呼び出されるコールバック関数
 * @param level mrs_output_log 関数の呼び出し時に指定されたログ出力レベルの値
 * @param msg 出力されたログメッセージ
 */
typedef void (MRS_API_CC *MrsLogOutputCallback)( MrsLogLevel level, const char* msg );
/**
 * @cbfn MrsNewConnectionCallback
 * @brief 新しい接続を受け入れたときに呼び出されるコールバック関数
 * @param server どのサーバに対する接続要求かをあらわすMrsServer
 * @param server_data mrs_server_set_data 関数で設定されたアプリケーションの任意のデータポインタ
 * @param client 新しく受け入れた接続
 */
typedef void (MRS_API_CC *MrsNewConnectionCallback)( MrsServer server, void* server_data, MrsConnection client );
/**
 * @cbfn MrsConnectCallback
 * @brief サーバへの接続に成功した時に呼び出されるコールバック関数
 * @param connection サーバへの接続処理に成功した接続
 * @param connection_data 接続に対してアプリケーションが設定した任意のデータポインタ
 */
typedef void (MRS_API_CC *MrsConnectCallback)( MrsConnection connection, void* connection_data );
/**
 * @cbfn MrsDisconnectCallback
 * @brief 接続が切断したときに呼び出されるコールバック関数
 * @param connection 切断が発生した接続
 * @param connection_data ...
 */
typedef void (MRS_API_CC *MrsDisconnectCallback)( MrsConnection connection, void* connection_data );
/**
 * @cbfn MrsErrorCallback
 * @brief 接続でエラーが発生したときに呼び出されるコールバック関数
 * @param connection エラーが発生した接続
 * @param connection_data ...
 * @param status エラー種別コード
 */
typedef void (MRS_API_CC *MrsErrorCallback)( MrsConnection connection, void* connection_data, MrsConnectionError status );
/**
 * @cbfn MrsReadRecordCallback
 * @brief レコードを1個受信したときに呼び出されるコールバック関数
 * @param connection レコードを受信した接続
 * @param connection_data ...
 * @param seqnum 送信側が付与した通し番号
 * @param options 送信側が設定した送信オプション
 * @param payload_type 送信側が設定したペイロード種別
 * @param payload 送信されたデータ
 * @param payload_len 送信されたデータの長さ(バイト数)
 */
typedef void (MRS_API_CC *MrsReadRecordCallback)( MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len );
/**
 * @cbfn MrsKeyExchangeCallback
 * @brief 鍵交換が完了したときに呼ばれるコールバック関数
 * @param connection 鍵交換が完了した接続
 * @param connection_data ...
 */
typedef void (MRS_API_CC *MrsKeyExchangeCallback)( MrsConnection connection, void* connection_data );

#endif
