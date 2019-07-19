#ifndef __MRS_HPP__
#define __MRS_HPP__

/**
 *
 * @title  MRS APIリファレンス
 *
 * @head1 MRS APIリファレンス
 * 
 * 本文書はMRSライブラリが提供する全ての関数や型の解説をします。
 *
 */

#define MRS_VERSION_KEY "mrs"
#define MRS_VERSION     0x00010007

#include <common.hpp>
#include <buffer.hpp>
#include <utility.hpp>
#include <date_time.hpp>
#include <opt_arg.hpp>

#ifndef MRS_LOG_BUF_SIZE
# define MRS_LOG_BUF_SIZE 512
#endif
#define MRS_OUTPUT_LOG( _level, _format, ... ) {\
    if ( mrs_is_output_log_level( _level ) ){\
        char _msg[ MRS_LOG_BUF_SIZE ];\
        int _msg_len = snprintf( _msg, sizeof( _msg ), _format, ##__VA_ARGS__ );\
        if ( _msg_len < MRS_LOG_BUF_SIZE ){\
            mrs_output_log( _level, _msg );\
        }else{\
            int _new_msg_size = _msg_len + 1;\
            char* _new_msg = (char*)malloc( _new_msg_size );\
            snprintf( _new_msg, _new_msg_size, _format, ##__VA_ARGS__ );\
            mrs_output_log( _level, _new_msg );\
            free( _new_msg );\
        }\
    }\
}
#define MRS_LOG_EMERG( _format, ... )   MRS_OUTPUT_LOG( MRS_LOG_LEVEL_EMERG,   _format, ##__VA_ARGS__ )
#define MRS_LOG_ALERT( _format, ... )   MRS_OUTPUT_LOG( MRS_LOG_LEVEL_ALERT,   _format, ##__VA_ARGS__ )
#define MRS_LOG_CRIT( _format, ... )    MRS_OUTPUT_LOG( MRS_LOG_LEVEL_CRIT,    _format, ##__VA_ARGS__ )
#define MRS_LOG_ERR( _format, ... )     MRS_OUTPUT_LOG( MRS_LOG_LEVEL_ERR,     _format, ##__VA_ARGS__ )
#define MRS_LOG_WARNING( _format, ... ) MRS_OUTPUT_LOG( MRS_LOG_LEVEL_WARNING, _format, ##__VA_ARGS__ )
#define MRS_LOG_NOTICE( _format, ... )  MRS_OUTPUT_LOG( MRS_LOG_LEVEL_NOTICE,  _format, ##__VA_ARGS__ )
#define MRS_LOG_INFO( _format, ... )    MRS_OUTPUT_LOG( MRS_LOG_LEVEL_INFO,    _format, ##__VA_ARGS__ )
#define MRS_LOG_DEBUG( _format, ... )   MRS_OUTPUT_LOG( MRS_LOG_LEVEL_DEBUG,   _format, ##__VA_ARGS__ )

extern "C" {

    /**
     * @head2 MRSライブラリ全体を操作する関数
     * ライブラリ自体の初期化、状態更新、停止、削除などを行います。
     */
    /**
     * @fn mrs_initialize
     * @brief MRSのライブラリに必要なメモリを割り当てて初期化を行います
     * @return 成功したらtrue, メモリが足りないなどの場合にfalseを返します。
     */    
MRS_API bool MRS_API_CC mrs_initialize();
    /**
     * @fn mrs_update
     * @brief MRSライブラリの状態を更新します
     * 内部でOSに対してポーリングを行い、MRSが監視しているソケットにデータが到着している場合は
     * 必要なコールバックを呼び出し、また送信が可能な場合はソケットに対してデータ(レコード)を送信します。
     * OSに対する入出力はすべてここで行われます。
     */
MRS_API void MRS_API_CC mrs_update();
    /**
     * @fn mrs_update_keep_alive
     * @brief MRSライブラリの全コネクションのキープアライブ状態を更新します
     * キープアライブ対象のソケットに対してデータ(レコード)を送信します。
     */
MRS_API void MRS_API_CC mrs_update_keep_alive();
    /**
     * @fn mrs_run
     * @brief スリープしながらループし続けます
     * 動作中にmrs_stop_running() 関数を呼び出すことでループから抜けることができます。
     *     
     */
MRS_API void MRS_API_CC mrs_run( uint32 sleep_msec );
    /**
     * @fn mrs_stop_running
     * @brief mrs_runのループを停止します
     
     */
MRS_API void MRS_API_CC mrs_stop_running();
    /**
     * @fn mrs_finalize
     * @brief MRSライブラリが確保しているメモリをすべて解放します
     */
MRS_API void MRS_API_CC mrs_finalize();

    /**
     * @head2 接続数を操作するための関数
     * TCPまたはUDPクライアントの接続数を取得、設定するための関数群です。
     */

    /**
     * @fn mrs_get_connection_num_hard_limit
     * @brief MRSライブラリに埋め込まれている、同時接続数の絶対的な最大値(ハードリミット値)を返します
     */
MRS_API uint32 MRS_API_CC mrs_get_connection_num_hard_limit();
    /**
     * @fn mrs_get_connection_num_soft_limit
     * @brief アプリケーションがMRSライブラリに設定しているクライアントの最大接続数(ソフトリミット値)を返します。
     */
MRS_API uint32 MRS_API_CC mrs_get_connection_num_soft_limit();
    /**
     * @fn mrs_set_connection_num_soft_limit
     * @brief MRSライブラリ全体の最大接続数を設定します。ハードリミット値以下の値を設定できます。これをソフトリミットと呼びます。
     * @return 設定できたらtrue, 失敗したらfalse。
     * ```mrs_get_connection_num_hard_limit```関数が返す値より大きな値は設定できません。
     */
MRS_API bool MRS_API_CC mrs_set_connection_num_soft_limit( uint32 value );
    /**
     * @fn mrs_get_connection_num
     * @brief 全体の接続しているクライアント数を返します
     */
MRS_API uint32 MRS_API_CC mrs_get_connection_num();
    /**
     * @fn mrs_server_get_connection_num
     * @brief サーバーあたりの接続しているクライアント数を返します
     */
MRS_API uint32 MRS_API_CC mrs_server_get_connection_num( MrsServer server );

    /**
     * @head2 サーバを操作するための関数
     * TCPまたはUDPのサーバを作成、削除、設定するための関数群です。
     */

    /**
     * @fn mrs_server_create
     * @brief TCPまたはUDP(RUDPを含む)のサーバーを作成して返します
     * @param type 接続の種類。MRS_CONNECTION_TYPE_TCP, MRS_CONNECTION_TYPE_UDP,MRS_CONNECTION_TYPE_WS, MRS_CONNECTION_TYPE_WSS のいずれかを設定します
     * @param addr 受け入れるソケットのローカルアドレスをIPv4アドレスの数字表現で指定します。ドメイン名では指定できません。 インターネットの不特定多数向けのサービスの場合は "0.0.0.0"を指定し、ローカルマシンからのみに制限する場合は "127.0.0.1"を指定します。それ以外のアドレスも指定可能です。
     * @param port TCPまたはUDPのポート番号を指定します。0~65535の範囲で決定してください。
     * @param backlog TCPの場合はソケットのバックログの数(OSが保持している、acceptが呼び出される前のソケットの数)を指定します。短時間に大量の新しい接続を受け入れるようなサービスの場合にこれを大きくしてください。通常は10で問題ありません。UDPの場合は、1つのMrsServerあたりの最大の同時接続数を指定します。
     * この関数が返すMrsServerに対して、 ```mrs_set_error_callback```関数を使ってエラー検出用コールバック関数を定義すると、
     * OS資源が足りないなどの何らかの原因で新規接続を受け入れることができなかった場合のエラーを検出することができます。
     * 詳細は ```mrs_set_error_callback```関数の項を参照してください。
     */
MRS_API MrsServer MRS_API_CC mrs_server_create( MrsConnectionType type, const char* addr, uint16 port, int32 backlog );
    /**
     * @fn mrs_server_set_new_connection_callback
     * @brief  MRSサーバが新しい接続を受け入れたときに呼び出すコールバック関数を定義します
     * @param server 設定する対象となるサーバ
     * @param callback アプリケーションが定義するコールバック関数へのポインタ。コールバック関数のプロトタイプは次のようになっています。 <code>void new_conn_callback( MrsServer server, MrsConnection client )</code> このコールバックでは第1引数にサーバ、第2引数に新しいクライアントが渡されます。
     *  
     */
MRS_API void MRS_API_CC mrs_server_set_new_connection_callback( MrsServer server, MrsNewConnectionCallback callback );
    /**
     * @fn mrs_server_set_data
     * @brief  MrsServer に対して任意のポインタをひも付けます
     * @param server ...
     * @param server_data 任意のデータへのポインタ
     * @return 設定できたらtrue, 失敗したらfalse。有効なサーバーを指定する必要があります。
     * ひとつのプロセスで複数のサーバを起動し、同じ新規接続コールバック関数を登録した場合でも、
     * このポインタの値を使って、どのサーバに対する新規接続なのかを識別することができます。
     */
MRS_API bool MRS_API_CC mrs_server_set_data( MrsServer server, void* server_data );
    /**
     * @fn mrs_server_get_data
     * @brief  MrsServerに対してひも付けられているポインタ値を取得します
     * @param server ...
     * @return 設定されているポインタの現在の値
     */    
MRS_API void* MRS_API_CC mrs_server_get_data( MrsServer server );

    /**
     * @head2 クライアントを操作するための関数
     * MRSのサーバに接続して通信をするクライアントを作成、設定します。
     */
    /**
     * @fn mrs_connect
     * @brief 新しい接続(MrsConnection)をひとつ割り当てて初期化し、接続を開始します。
     * @param type 使用するトランスポート層プロトコルを指定します。サーバと同じ値を指定してください。
     * @param addr サーバが待ち受けているホストのIPv4アドレスを "192.168.1.232"のような数字の表現で指定します。
     * @param port ...
     * @param timeout_msec 接続失敗と判定されるまでのタイムアウト時間をミリ秒で指定します。
     * @return 新しく割り当てられた接続
     * mrs_connect関数は接続を開始するだけで、実際のトランスポート層パケットはこの関数を呼び出しても送信されません。
     * 実際のパケットは、後でmrs_update関数を呼び出すことによって送信されます。
     */    

MRS_API MrsConnection MRS_API_CC mrs_connect( MrsConnectionType type, const char* addr, uint16 port, uint32 timeout_msec );
    /**
     * @fn mrs_set_connect_callback
     * @brief サーバへの接続が完了したことを検出するためのコールバック関数を設定します
     * @param connection 設定する対象となる接続
     * @param callback コールバック関数
     * mrs_connect関数で作られた接続(MrsConnection)に対して、その接続が実際に接続が成功したことを検出するための
     * コールバック関数を設定します。コールバック関数は接続ごとに設定する必要があります。
     * 接続ごとに異なるコールバック関数を設定することも可能です。
     */        
MRS_API void MRS_API_CC mrs_set_connect_callback( MrsConnection connection, MrsConnectCallback callback );
    /**
     * @fn mrs_set_disconnect_callback
     * @brief 接続が切れたときに呼び出されるコールバック関数を設定します
     * @param connection ...
     * @param callback コールバック関数のポインタ
     * サーバーが起動していない場合やファイアウォールなど、何らかの原因によって接続に失敗した場合は、このコールバック関数は呼ばれず、 mrs_set_error_callback 関数で設定したコールバック関数が呼び出されます。
     */    
MRS_API void MRS_API_CC mrs_set_disconnect_callback( MrsConnection connection, MrsDisconnectCallback callback );
    /**
     * @head2 クライアントとサーバの両方で共通に使う関数
     * レコードの受信、送信、エラーハンドリングなどを行うための関数です。
     */
    /**
     * @fn mrs_set_error_callback
     * @brief 接続において何らかのエラーが発生したことを検出するためのコールバック関数を設定します
     * @param connection ...
     * @param callback ...
     */    
MRS_API void MRS_API_CC mrs_set_error_callback( MrsConnection connection, MrsErrorCallback callback );
    /**
     * @fn mrs_set_read_record_callback
     * @brief 接続において接続相手が送信したレコードを1個受信するごとに1回呼ばれるコールバック関数を設定します
     * @param connection ...
     * @param callback ... 
     */    
MRS_API void MRS_API_CC mrs_set_read_record_callback( MrsConnection connection, MrsReadRecordCallback callback );
    /**
     * @fn mrs_connection_set_data
     * @brief 接続に対して、アプリケーションが任意のポインタをひも付けます
     * @param connection ...
     * @param connection_data 任意のデータへのポインタ
     * @return 設定に成功したらtrue, 失敗したらfalse
     * 任意のポインタを使って、プレイヤーキャラクタなどのアプリケーションの内部データをMRSの接続に関連付けることが簡単にできます。
     */    
MRS_API bool MRS_API_CC mrs_connection_set_data( MrsConnection connection, void* connection_data );
    /**
     * @fn mrs_connection_get_data
     * @brief mrs_connection_set_data 関数で設定したポインタの値を取得します
     * @param connection ...
     * @return 設定されていたポインタの値
     */    
MRS_API void* MRS_API_CC mrs_connection_get_data( MrsConnection connection );
    /**
     * @fn mrs_connection_is_connected
     * @brief MrsConnectionがサーバへの接続を完了しているかどうかを調べます
     * @param connection ポインタを取り出す対象トなる接続
     * @return 接続できていて、データを送信可能な状態ならtrueを返します。
     */    
MRS_API bool MRS_API_CC mrs_connection_is_connected( MrsConnection connection );
    /**
     * @fn mrs_connection_set_readbuf_max_size
     * @brief MrsConnectionの読み込みバッファの最大サイズを設定します
     * @param connection 設定対象の接続
     * @param value      読み込みバッファの最大サイズ
     * @return 設定できたなら、trueを返します
     */    
MRS_API bool MRS_API_CC mrs_connection_set_readbuf_max_size( MrsConnection connection, uint32 value );
    /**
     * @fn mrs_connection_get_readbuf_max_size
     * @brief MrsConnectionの読み込みバッファの最大サイズを取得します
     * @param connection 設定対象の接続
     * @return 読み込みバッファの最大サイズ
     */    
MRS_API uint32 MRS_API_CC mrs_connection_get_readbuf_max_size( MrsConnection connection );
    /**
     * @fn mrs_connection_get_type
     * @brief MrsConnectionに設定されているプロトコルを返します
     * @param connection 対象となる接続
     * @return MrsConnectionに設定されているプロトコルを返します
     */    
MRS_API MrsConnectionType MRS_API_CC mrs_connection_get_type( MrsConnection connection );
    /**
     * @fn mrs_write_record
     * @brief 接続に対して、レコードを1個送信します
     * @param connection レコードを送信する対象となる接続
     * @param options レコードを送信するときのオプションを設定します。オプションはレコードごとに異なる値を設定できます。　設定可能な値は、MrsRecordOption列挙体で定義されている定数を、ORビット演算子でつないで複数同時に指定可能です。
     * @param payload_type 送信したいデータの種別をアプリケーションが自由に指定します。PAYLOAD_TYPE_BEGIN と MRS_PAYLOAD_TYPE_END の間の値を設定してください。 PAYLOAD_TYPE_BEGINは現在は0, MRS_PAYLOAD_TYPE_ENDは現在は0xffとなっていますが、将来は変更される可能性があります。
     * @param payload 送信したいデータの先頭アドレスです。データの内容はバイナリデータで、0を含んでいても問題なく送信できます。
     * @param payload_len 送信したいデータの長さ(バイト)です。
     * この関数を呼び出した時点では実際に送信されず、次の mrs_update 関数の呼び出し時に可能な限り送信しようとします。
     * ただし、OSやソケットの状態によっては、次の呼び出しで確実に送信されるとは限りません。
     * 送信の準備が成功したらtrue,メモリが足りないなどによって失敗したらfalseが返されます。
     *  MRSはレコードを送信するときに内部的に通し番号を付与します。 
     */    
MRS_API bool MRS_API_CC mrs_write_record( MrsConnection connection, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len );
    /**
     * @fn mrs_close
     * @brief 接続を閉じます
     * @param connection 操作対象の接続
     * この関数を呼んだ時点では、実際の接続終了の通信は行われず、次のmrs_update 関数の呼び出し時に実行されます。

     */    
MRS_API void MRS_API_CC mrs_close( MrsConnection connection );

    
    /**
     * @head2 暗号化通信を使うための関数
     * MRSでは、MrsCipherを作成し、それを接続ごとに登録することで暗号化通信を行います。
     * そのため接続ごとに暗号の種類を切り替えることが可能なように設計されています(現在はECDHのみが利用可能)。
     *
     * 送信するレコード単位で暗号の有効化をon/offすることもできます。
     * それにはレコードを送信する mrs_write_record  関数のオプションに暗号化利用モードを設定します。
     * ただし、暗号化オプションを有効化するためには、暗号に使う鍵をあらかじめ交換し終わっている必要があります。
     *
     * 鍵交換をするためには、 MrsCipherを mrs_cipher_create 関数を使って作成し、
     * それを mrs_set_cipher関数を使って接続に登録し、 mrs_key_exchange 関数を使って鍵交換の通信を起動します。　
     * それ以降の必要な通信や数値計算の処理は、MRSが内部的に自動で行います。
     * MRSは、鍵交換が無事に終了したことをアプリケーションに対してコールバック関数を使って通知します。
     */
     
    /**
     * @fn mrs_cipher_create
     * @brief  MrsCipherを作成します
     * @param type  鍵交換アルゴリズムの種類を指定します。 現在利用可能なのは MRS_CIPHER_TYPE_ECDH のみです。
     * @return 作成されたMrsCipher
     */    
MRS_API MrsCipher MRS_API_CC mrs_cipher_create( MrsCipherType type );
    /**
     * @fn mrs_set_cipher
     * @brief  接続に対してMrsCipherを登録します。
     * @param connection 登録対象となる接続
     * @param cipher 登録したいMrsCipher
     */
MRS_API void MRS_API_CC mrs_set_cipher( MrsConnection connection, MrsCipher cipher );
    /**
     * @fn mrs_key_exchange
     * @brief 鍵交換を開始するよう指示します
     * @param connection 鍵交換を開始する接続
     * @param callback  鍵交換が完了したときに呼び出されるコールバック関数
     * 鍵交換は、クライアントまたはサーバから、本関数を呼んだ時に「鍵交換リクエスト」を互いに送信し、
     * それに対して「鍵交換レスポンス」を互いに返すという動作をします。
     *  この関数を呼ぶと、鍵交換リクエストを送信します。　送信に成功したらtrue、失敗したらfalseを返します。
     */
MRS_API bool MRS_API_CC mrs_key_exchange( MrsConnection connection, MrsKeyExchangeCallback callback );

    /**
     * @head2 ログ出力をするための関数
     * MRSはライブラリ内部の動作ログを、 mrs_log_output関数を使って出力します。
     * この関数をアプリケーションでも使うことができます。MRSのログ出力は、
     * ログを出力するたびにコールバック関数を呼ぶことで実現されます。
     * デフォルトの状態で mrs_console_log関数が設定されていますが、
     * それを mrs_set_log_callback関数を用いてアプリケーションの独自のものに置き換えることができます。
     *
     * ログ出力レベルはプロセス全体でひとつで、設定した値以上(値自体を含む)のレベルのログが出力されます。
     * 出力レベルは、 EMERGが最高でDEBUGが最低です。詳細は MrsLogLevel 列挙体の定義を参照してください。
     */
    /**
     * @fn mrs_get_output_log_level
     * @brief 現在設定されているログ出力レベルの値を取得します
     * @return 現在のログ出力レベル
     */
MRS_API MrsLogLevel MRS_API_CC mrs_get_output_log_level();
    /**
     * @fn mrs_set_output_log_level
     * @brief MRSライブラリのログ出力レベルを設定します
     * @param level 設定するログ出力レベル
     */
MRS_API void MRS_API_CC mrs_set_output_log_level( MrsLogLevel level );
    /**
     * @fn mrs_is_output_log_level
     * @brief MRSライブラリのログ出力可能なレベルか判定します
     * @param level 判定するログ出力レベル
     * @return ログ出力可能なら、trueを返します
     */
MRS_API bool MRS_API_CC mrs_is_output_log_level( MrsLogLevel level );
    /**
     * @fn mrs_output_log
     * @brief ログレベルを指定してログを出力します
     * @param level 出力したいログのレベル
     * @param msg メッセージ
     */
MRS_API void MRS_API_CC mrs_output_log( MrsLogLevel level, const char* msg );
    /**
     * @fn mrs_get_log_callback
     * @brief ログを出力するために設定されているコールバック関数を取得します
     * @return 現在設定されているコールバック関数
     */    
MRS_API MrsLogOutputCallback MRS_API_CC mrs_get_log_callback();
    /**
     * @fn mrs_set_log_callback
     * @brief ログを出力するためのコールバック関数を設定します
     * @param callback コールバック関数
     */
MRS_API void MRS_API_CC mrs_set_log_callback( MrsLogOutputCallback callback );
    /**
     * @fn mrs_console_log
     * @brief MRSのログを標準出力に出力する関数
     * @param level mrs_output_log関数に指定されたログ出力レベルがそのまま渡されます
     * @param msg mrs_output_log関数に指定されたログメッセージがそのまま渡されます
     * mrs_console_log関数は、MRSライブラリのデフォルトのログ出力コールバック関数としてあらかじめ設定されています。
     * この関数は、 mrs_set_log_callback関数を使って置き換えることができます。     
     */
MRS_API void MRS_API_CC mrs_console_log( MrsLogLevel level, const char* msg );
    /**
     * @head2 エラーコードを扱うための関数
     * MRSライブラリ内部のエラー状態は、ライブラリでひとつの変数に格納されています。
     * mrs_get_last_error関数でその値を取得し、 mrs_get_error_string関数で文字列に変換できます。
     */
    /**
     * @fn mrs_get_last_error
     * @brief MRSの関数内部で起きたエラーの種類を返します
     * @return エラーコード。値の詳細は MrsErrorを参照してください
     */
MRS_API MrsError MRS_API_CC mrs_get_last_error();
    /**
     * @fn mrs_get_error_string
     * @brief エラー番号を文字列に変換します
     * @param error エラーコード。通常は mrs_get_last_errorの値を入力します
     * @return エラーを説明する文字列
     */    
MRS_API const char* MRS_API_CC mrs_get_error_string( MrsError error );
    /**
     * @fn mrs_get_connection_error_string
     * @brief 接続に関するエラー番号を文字列に変換します
     * @param error エラーコード。通常は MrsErrorCallbackのstatus値を入力します
     * @return エラーを説明する文字列
     */    
MRS_API const char* MRS_API_CC mrs_get_connection_error_string( MrsConnectionError error );
    /**
     * @head2 その他の便利な関数
     */
    /**
     * @fn mrs_sleep
     * @brief 指定した時間だけスレッドの実行を停止する
     * @param sleep_msec 現在のスレッドを停止するミリ秒数
     */
MRS_API void MRS_API_CC mrs_sleep( uint32 sleep_msec );
    /**
     * @fn mrs_set_ssl_certificate_data
     * @brief SSL証明書データの設定
     * @param data SSL証明書データ
     */
MRS_API void MRS_API_CC mrs_set_ssl_certificate_data( const char* data );
    /**
     * @fn mrs_set_ssl_private_key_data
     * @brief SSL秘密鍵データの設定
     * @param data SSL秘密鍵データ
     */
MRS_API void MRS_API_CC mrs_set_ssl_private_key_data( const char* data );
    /**
     * @fn mrs_set_keep_alive_update_msec
     * @brief キープアライブ更新時間の設定
     * @param update_msec キープアライブを更新するミリ秒数
     */
MRS_API void MRS_API_CC mrs_set_keep_alive_update_msec( uint32 update_msec );
    /**
     * @fn mrs_get_keep_alive_update_msec
     * @brief キープアライブ更新時間の取得
     * @return キープアライブを更新するミリ秒数
     */
MRS_API uint32 MRS_API_CC mrs_get_keep_alive_update_msec();
    /**
     * @fn mrs_set_version
     * @brief バージョンの設定
     * @param key 設定対象バージョンのキー
     * @param value 設定対象バージョンの値
     */
MRS_API void MRS_API_CC mrs_set_version( const char* key, uint32 value );
    /**
     * @fn mrs_get_version
     * @brief バージョンの取得
     * @param key 取得対象バージョンのキー
     * @return バージョン
     */
MRS_API uint32 MRS_API_CC mrs_get_version( const char* key );
    /**
     * @fn mrs_connection_get_remote_version
     * @brief 接続先のバージョンの取得
     * @param connection 取得対象の接続
     * @param key 取得対象バージョンのキー
     * @return 接続先のバージョン
     */
MRS_API uint32 MRS_API_CC mrs_connection_get_remote_version( MrsConnection connection, const char* key );
    /**
     * @fn mrs_udp_set_mtu
     * @brief UDPプロトコルのMTUの設定
     * @param value MTUの値
     */
MRS_API void MRS_API_CC mrs_udp_set_mtu( uint32 value );
    /**
     * @fn mrs_udp_get_mtu
     * @brief UDPプロトコルのMTUの取得
     * @return MTUの値
     */
MRS_API uint32 MRS_API_CC mrs_udp_get_mtu();
}

#endif
