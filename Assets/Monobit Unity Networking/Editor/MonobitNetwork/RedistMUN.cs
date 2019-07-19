using System.IO;
using System.Collections.Generic;
using UnityEditor;

namespace MonobitEngine
{
    public class RedistMUN : AssetPostprocessor
    {
        /** MRS/MUN/VRVCライブラリが対応しているライブラリの種類. */
        enum MonobitEngineLibraryType
        {
            MonobitEngineLibraryType_for_AndroidARM64 = 0x01,
            MonobitEngineLibraryType_for_AndroidARMv7 = 0x02,
            MonobitEngineLibraryType_for_Androidx86   = 0x03,
            MonobitEngineLibraryType_for_AndroidAny   = 0x04,
            MonobitEngineLibraryType_for_iOS          = 0x05,
            MonobitEngineLibraryType_for_MacOSX       = 0x06,
            MonobitEngineLibraryType_for_Linux32      = 0x07,
            MonobitEngineLibraryType_for_Linux64      = 0x08,
            MonobitEngineLibraryType_for_Linux        = 0x09,
            MonobitEngineLibraryType_for_WebGL        = 0x0A,
            MonobitEngineLibraryType_for_Win32        = 0x0B,
            MonobitEngineLibraryType_for_Win64        = 0x0C,
            MonobitEngineLibraryType_for_Win          = 0x0D,
            MonobitEngineLibraryType_for_WSA          = 0x0E,
            MonobitEngineLibraryType_for_Any          = 0xFF,
        };

        /** MRS/MUN/VRVCライブラリのリスト. */
        static readonly Dictionary<string, MonobitEngineLibraryType> MonobitEngineLibraryList = new Dictionary<string, MonobitEngineLibraryType>()
        {
            { "Assets/Plugins/Android/mun.dll",                      MonobitEngineLibraryType.MonobitEngineLibraryType_for_AndroidAny   },
            { "Assets/Plugins/Android/mvc.dll",                      MonobitEngineLibraryType.MonobitEngineLibraryType_for_AndroidAny   },
            { "Assets/Plugins/Android/libs/arm64-v8a/libmrs.so",     MonobitEngineLibraryType.MonobitEngineLibraryType_for_AndroidARM64 },
            { "Assets/Plugins/Android/libs/arm64-v8a/libopus.so",    MonobitEngineLibraryType.MonobitEngineLibraryType_for_AndroidARM64 },
            { "Assets/Plugins/Android/libs/armeabi-v7a/libmrs.so",   MonobitEngineLibraryType.MonobitEngineLibraryType_for_AndroidARMv7 },
            { "Assets/Plugins/Android/libs/armeabi-v7a/libopus.so",  MonobitEngineLibraryType.MonobitEngineLibraryType_for_AndroidARMv7 },
            { "Assets/Plugins/Android/libs/x86/libmrs.so",           MonobitEngineLibraryType.MonobitEngineLibraryType_for_Androidx86   },
            { "Assets/Plugins/Android/libs/x86/libopus.so",          MonobitEngineLibraryType.MonobitEngineLibraryType_for_Androidx86   },
            { "Assets/Plugins/iOS/libopus.a",                        MonobitEngineLibraryType.MonobitEngineLibraryType_for_iOS          },
            { "Assets/Plugins/iOS/Monobit/mun.dll",                  MonobitEngineLibraryType.MonobitEngineLibraryType_for_iOS          },
            { "Assets/Plugins/iOS/Monobit/mvc.dll",                  MonobitEngineLibraryType.MonobitEngineLibraryType_for_iOS          },
            { "Assets/Plugins/iOS/Monobit/mrs/library/libcrypto.a",  MonobitEngineLibraryType.MonobitEngineLibraryType_for_iOS          },
            { "Assets/Plugins/iOS/Monobit/mrs/library/libenet.a",    MonobitEngineLibraryType.MonobitEngineLibraryType_for_iOS          },
            { "Assets/Plugins/iOS/Monobit/mrs/library/libmrs.a",     MonobitEngineLibraryType.MonobitEngineLibraryType_for_iOS          },
            { "Assets/Plugins/iOS/Monobit/mrs/library/libssl.a",     MonobitEngineLibraryType.MonobitEngineLibraryType_for_iOS          },
            { "Assets/Plugins/iOS/Monobit/mrs/library/libuv.a",      MonobitEngineLibraryType.MonobitEngineLibraryType_for_iOS          },
            { "Assets/Plugins/Mac/mrs.bundle",                       MonobitEngineLibraryType.MonobitEngineLibraryType_for_MacOSX       },
            { "Assets/Plugins/Mac/mun.dll",                          MonobitEngineLibraryType.MonobitEngineLibraryType_for_MacOSX       },
            { "Assets/Plugins/Mac/mvc.dll",                          MonobitEngineLibraryType.MonobitEngineLibraryType_for_MacOSX       },
            { "Assets/Plugins/Monobit/mrs.jslib",                    MonobitEngineLibraryType.MonobitEngineLibraryType_for_WebGL        },
            { "Assets/Plugins/Monobit/mun.dll",                      MonobitEngineLibraryType.MonobitEngineLibraryType_for_Win          },
            { "Assets/Plugins/Monobit/mvc.dll",                      MonobitEngineLibraryType.MonobitEngineLibraryType_for_Win          },
            { "Assets/Plugins/x86/mrs.dll",                          MonobitEngineLibraryType.MonobitEngineLibraryType_for_Win32        },
            { "Assets/Plugins/x86/opus.dll",                         MonobitEngineLibraryType.MonobitEngineLibraryType_for_Win32        },
            { "Assets/Plugins/x86_64/mrs.dll",                       MonobitEngineLibraryType.MonobitEngineLibraryType_for_Win64        },
            { "Assets/Plugins/x86_64/opus.bundle",                   MonobitEngineLibraryType.MonobitEngineLibraryType_for_MacOSX       },
            { "Assets/Plugins/x86_64/opus.dll",                      MonobitEngineLibraryType.MonobitEngineLibraryType_for_Win64        },
            { "Assets/Plugins/WSA/mun.dll",                          MonobitEngineLibraryType.MonobitEngineLibraryType_for_WSA          },
            { "Assets/Plugins/WSA/mvc.dll",                          MonobitEngineLibraryType.MonobitEngineLibraryType_for_WSA          },
        };
        
        /**
         * @brief   プラットフォーム設定の実行.
         */
        public static void SettingPlatform()
        {
            // ファイル名照合
            foreach (KeyValuePair<string, MonobitEngineLibraryType> kvp in MonobitEngineLibraryList)
            {
                AssetImporter asset = AssetImporter.GetAtPath( kvp.Key );
                if ( null == asset ) continue;
                
                PluginImporter plugin = (PluginImporter)asset;
                switch (kvp.Value)
                {
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_AndroidARM64:
                        {
                            // Android ARM64-v8
#if UNITY_2017_4_16_OR_NEWER || UNITY_2018_2_OR_NEWER
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.Android }, "ARM64");
#endif
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_AndroidARMv7:
                        {
                            // Android ARMv7
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.Android }, "ARMv7");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_Androidx86:
                        {
                            // Android x86
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.Android }, "x86");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_AndroidAny:
                        {
                            // Android AnyCPU
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.Android }, "");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_iOS:
                        {
                            // iOS
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.iOS }, "");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_MacOSX:
                        {
                            // MacOSX
#if UNITY_2017_3_OR_NEWER
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.StandaloneOSX });
#else
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.StandaloneOSXIntel,
                                                                               BuildTarget.StandaloneOSXIntel64,
                                                                               BuildTarget.StandaloneOSXUniversal });
#endif
                            SettingPlatformWithEditor(plugin, "OSX");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_Linux32:
                        {
                            // Linux 32bit
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.StandaloneLinux }, "x86");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_Linux64:
                        {
                            // Linux 64bit
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.StandaloneLinux64 }, "x86_64");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_Linux:
                        {
                            // Linux AnyCPU
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.StandaloneLinux,
                                                                               BuildTarget.StandaloneLinux64,
                                                                               BuildTarget.StandaloneLinuxUniversal });
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_WebGL:
                        {
                            // WebGL
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.WebGL }, "");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_Win32:
                        {
                            // Windows 32bit
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.StandaloneWindows,
                                                                               BuildTarget.WSAPlayer }, "x86");
                            SettingPlatformWithEditor(plugin, "Windows", "x86");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_Win64:
                        {
                            // Windows 64bit
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.StandaloneWindows64,
                                                                               BuildTarget.WSAPlayer }, "x86_64");
                            SettingPlatformWithEditor(plugin, "Windows", "x86_64");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_Win:
                        {
                            // Windows AnyCPU
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.StandaloneWindows,
                                                                               BuildTarget.StandaloneWindows64 });
                            SettingPlatformWithEditor(plugin, "Windows");
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_WSA:
                        {
                            // Windows Store Apps
                            SettingPlatform(plugin, false, new BuildTarget[] { BuildTarget.WSAPlayer });
                        }
                        break;
                    case MonobitEngineLibraryType.MonobitEngineLibraryType_for_Any:
                        {
                            // Any Platform
                            SettingPlatform(plugin, true);
                            SettingPlatformWithEditor(plugin);
                        }
                        break;
                }
            }
        }

        /**
         * @brief   プラグインのターゲットプラットフォーム別設定の実行.
         * @param   plugin                  対象となるプラグイン.
         * @param   isEnableAnyPlatform     すべてのプラットフォームに対し有効にするかどうかのフラグ.
         * @param   buildTarget             対象となるプラグインのプラットフォーム.
         * @param   cpuType                 対象となるプラグインのサポートCPUタイプ.
         */
        static void SettingPlatform(PluginImporter plugin, bool isEnableAnyPlatform, BuildTarget[] buildTargets = null, string cpuType = "AnyCPU")
        {
            // 一旦すべてのプラットフォーム依存を解除
            plugin.SetCompatibleWithPlatform(BuildTarget.Android,                  false);
            plugin.SetCompatibleWithPlatform(BuildTarget.iOS,                      false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux,          false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64,        false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#if UNITY_2017_3_OR_NEWER
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX,            false);
#else
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel,       false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64,     false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal,   false);
#endif
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows,        false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64,      false);
            plugin.SetCompatibleWithPlatform(BuildTarget.WebGL,                    false);
            plugin.SetCompatibleWithPlatform(BuildTarget.WSAPlayer,                false);
            plugin.SetCompatibleWithEditor(false);
            plugin.SetCompatibleWithAnyPlatform(false);

            if (isEnableAnyPlatform)
            {
                // 全プラットフォームに対する有効化
                plugin.SetCompatibleWithAnyPlatform(isEnableAnyPlatform);
            }
            else if (buildTargets != null)
            {
                // 個別プラットフォーム向け設定
                foreach (BuildTarget buildTarget in buildTargets)
                {
                    plugin.SetCompatibleWithPlatform(buildTarget, true);
                    switch (buildTarget)
                    {
                        case BuildTarget.Android:
                            {
                                if (!cpuType.Equals("AnyCPU") && cpuType.Length > 0)
                                {
                                    plugin.SetPlatformData(buildTarget, "CPU", cpuType);
                                }
                            }
                            break;
                        case BuildTarget.StandaloneLinux:
                        case BuildTarget.StandaloneLinux64:
                        case BuildTarget.StandaloneLinuxUniversal:
                            {
                                if (cpuType == "x86")
                                {
                                    plugin.SetPlatformData(BuildTarget.StandaloneLinux, "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneLinux64, "CPU", "None");
                                    plugin.SetPlatformData(BuildTarget.StandaloneLinuxUniversal, "CPU", "AnyCPU");
                                }
                                else if (cpuType == "x86_64")
                                {
                                    plugin.SetPlatformData(BuildTarget.StandaloneLinux, "CPU", "None");
                                    plugin.SetPlatformData(BuildTarget.StandaloneLinux64, "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneLinuxUniversal, "CPU", "AnyCPU");
                                }
                                else
                                {
                                    plugin.SetPlatformData(BuildTarget.StandaloneLinux, "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneLinux64, "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneLinuxUniversal, "CPU", "AnyCPU");
                                }
                            }
                            break;
#if UNITY_2017_3_OR_NEWER
                        case BuildTarget.StandaloneOSX:
                            {
                                plugin.SetPlatformData(buildTarget, "CPU", cpuType);
                            }
                            break;
#else
                        case BuildTarget.StandaloneOSXIntel:
                        case BuildTarget.StandaloneOSXIntel64:
                        case BuildTarget.StandaloneOSXUniversal:
                            {
                                if (cpuType == "x86")
                                {
                                    plugin.SetPlatformData(BuildTarget.StandaloneOSXIntel,     "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneOSXIntel64,   "CPU", "None");
                                    plugin.SetPlatformData(BuildTarget.StandaloneOSXUniversal, "CPU", "AnyCPU");
                                }
                                else if (cpuType == "x86_64")
                                {
                                    plugin.SetPlatformData(BuildTarget.StandaloneOSXIntel,     "CPU", "None");
                                    plugin.SetPlatformData(BuildTarget.StandaloneOSXIntel64,   "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneOSXUniversal, "CPU", "AnyCPU");
                                }
                                else
                                {
                                    plugin.SetPlatformData(BuildTarget.StandaloneOSXIntel,     "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneOSXIntel64,   "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneOSXUniversal, "CPU", "AnyCPU");
                                }
                            }
                            break;
#endif
                        case BuildTarget.StandaloneWindows:
                        case BuildTarget.StandaloneWindows64:
                            {
                                if (cpuType == "x86")
                                {
                                    plugin.SetPlatformData(BuildTarget.StandaloneWindows, "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneWindows64, "CPU", "None");
                                }
                                else if (cpuType == "x86_64")
                                {
                                    plugin.SetPlatformData(BuildTarget.StandaloneWindows, "CPU", "None");
                                    plugin.SetPlatformData(BuildTarget.StandaloneWindows64, "CPU", "AnyCPU");
                                }
                                else
                                {
                                    plugin.SetPlatformData(BuildTarget.StandaloneWindows, "CPU", "AnyCPU");
                                    plugin.SetPlatformData(BuildTarget.StandaloneWindows64, "CPU", "AnyCPU");
                                }
                            }
                            break;
                        case BuildTarget.WSAPlayer:
                            {
                                if (cpuType == "x86")
                                {
                                    plugin.SetPlatformData(BuildTarget.WSAPlayer, "CPU", "x86");
                                }
                                else if (cpuType == "x86_64")
                                {
                                    plugin.SetPlatformData(BuildTarget.WSAPlayer, "CPU", "x64");
                                }
                                else
                                {
                                    plugin.SetPlatformData(BuildTarget.WSAPlayer, "CPU", "AnyCPU");
                                }
                            }
                            break;
                    }
                }
            }
        }

        /**
         * @brief   プラグインのUnityEditor上でのターゲットプラットフォーム別設定の実行.
         * @param   plugin                  対象となるプラグイン.
         * @param   cpuType                 対象となるプラグインのサポートCPUタイプ（"AnyCPU", "X86", "X86_64" のうちのどれか）.
         * @param   osType                  対象となるプラグインのサポートOSタイプ（"AnyOS", "OSX", "Windows", "Linux" のうちのどれか）.
         */
        static void SettingPlatformWithEditor(PluginImporter plugin, string osType = "AnyOS", string cpuType = "AnyCPU")
        {
            // UnityEditorでの有効化設定
            plugin.SetCompatibleWithEditor(true);

            // CPUタイプの設定
            if (cpuType.Length > 0)
            {
                plugin.SetEditorData("CPU", cpuType);
                plugin.SetPlatformData("Editor", "CPU", cpuType);
            }

            // OSタイプの設定
            if (osType.Length > 0)
            {
                plugin.SetEditorData("OS", osType);
                plugin.SetPlatformData("Editor", "OS", osType);
            }
        }
        
        private static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
        {
            SettingPlatform();
            
#if UNITY_WSA
            PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.InternetClient, true);
            PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.Microphone, true);
#endif
        }
    }
}
