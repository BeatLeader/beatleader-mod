using System;
using System.Runtime.InteropServices;

namespace BeatLeader.Utils {
    internal static unsafe class OpenXRAcquirer {
        #region InteropStructures

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct XrInstance_T { }

        [Flags]
        private enum XrResult {
            XR_ERROR_CREATE_SPATIAL_ANCHOR_FAILED_MSFT = -1000039001,      // 0xC4649DA7
            XR_ERROR_ANDROID_THREAD_SETTINGS_FAILURE_KHR = -1000003001,    // 0xC4652A47
            XR_ERROR_ANDROID_THREAD_SETTINGS_ID_INVALID_KHR = -1000003000, // 0xC4652A48
            XR_ERROR_LOCALIZED_NAME_INVALID = -49,                         // 0xFFFFFFCF
            XR_ERROR_LOCALIZED_NAME_DUPLICATED = -48,                      // 0xFFFFFFD0
            XR_ERROR_ACTIONSETS_ALREADY_ATTACHED = -47,                    // 0xFFFFFFD1
            XR_ERROR_ACTIONSET_NOT_ATTACHED = -46,                         // 0xFFFFFFD2
            XR_ERROR_NAME_INVALID = -45,                                   // 0xFFFFFFD3
            XR_ERROR_NAME_DUPLICATED = -44,                                // 0xFFFFFFD4
            XR_ERROR_ENVIRONMENT_BLEND_MODE_UNSUPPORTED = -42,             // 0xFFFFFFD6
            XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED = -41,            // 0xFFFFFFD7
            XR_ERROR_INDEX_OUT_OF_RANGE = -40,                             // 0xFFFFFFD8
            XR_ERROR_POSE_INVALID = -39,                                   // 0xFFFFFFD9
            XR_ERROR_GRAPHICS_DEVICE_INVALID = -38,                        // 0xFFFFFFDA
            XR_ERROR_CALL_ORDER_INVALID = -37,                             // 0xFFFFFFDB
            XR_ERROR_API_LAYER_NOT_PRESENT = -36,                          // 0xFFFFFFDC
            XR_ERROR_FORM_FACTOR_UNAVAILABLE = -35,                        // 0xFFFFFFDD
            XR_ERROR_FORM_FACTOR_UNSUPPORTED = -34,                        // 0xFFFFFFDE
            XR_ERROR_FILE_CONTENTS_INVALID = -33,                          // 0xFFFFFFDF
            XR_ERROR_FILE_ACCESS_ERROR = -32,                              // 0xFFFFFFE0
            XR_ERROR_REFERENCE_SPACE_UNSUPPORTED = -31,                    // 0xFFFFFFE1
            XR_ERROR_TIME_INVALID = -30,                                   // 0xFFFFFFE2
            XR_ERROR_SESSION_NOT_STOPPING = -29,                           // 0xFFFFFFE3
            XR_ERROR_SESSION_NOT_READY = -28,                              // 0xFFFFFFE4
            XR_ERROR_ACTION_TYPE_MISMATCH = -27,                           // 0xFFFFFFE5
            XR_ERROR_SWAPCHAIN_FORMAT_UNSUPPORTED = -26,                   // 0xFFFFFFE6
            XR_ERROR_SWAPCHAIN_RECT_INVALID = -25,                         // 0xFFFFFFE7
            XR_ERROR_LAYER_LIMIT_EXCEEDED = -24,                           // 0xFFFFFFE8
            XR_ERROR_LAYER_INVALID = -23,                                  // 0xFFFFFFE9
            XR_ERROR_PATH_UNSUPPORTED = -22,                               // 0xFFFFFFEA
            XR_ERROR_PATH_FORMAT_INVALID = -21,                            // 0xFFFFFFEB
            XR_ERROR_PATH_COUNT_EXCEEDED = -20,                            // 0xFFFFFFEC
            XR_ERROR_PATH_INVALID = -19,                                   // 0xFFFFFFED
            XR_ERROR_SYSTEM_INVALID = -18,                                 // 0xFFFFFFEE
            XR_ERROR_SESSION_LOST = -17,                                   // 0xFFFFFFEF
            XR_ERROR_SESSION_NOT_RUNNING = -16,                            // 0xFFFFFFF0
            XR_ERROR_SESSION_RUNNING = -14,                                // 0xFFFFFFF2
            XR_ERROR_INSTANCE_LOST = -13,                                  // 0xFFFFFFF3
            XR_ERROR_HANDLE_INVALID = -12,                                 // 0xFFFFFFF4
            XR_ERROR_SIZE_INSUFFICIENT = -11,                              // 0xFFFFFFF5
            XR_ERROR_LIMIT_REACHED = -10,                                  // 0xFFFFFFF6
            XR_ERROR_EXTENSION_NOT_PRESENT = -9,                           // 0xFFFFFFF7
            XR_ERROR_FEATURE_UNSUPPORTED = -8,                             // 0xFFFFFFF8
            XR_ERROR_FUNCTION_UNSUPPORTED = -7,                            // 0xFFFFFFF9
            XR_ERROR_INITIALIZATION_FAILED = -6,                           // 0xFFFFFFFA
            XR_ERROR_API_VERSION_UNSUPPORTED = -4,                         // 0xFFFFFFFC
            XR_ERROR_OUT_OF_MEMORY = -3,                                   // 0xFFFFFFFD
            XR_ERROR_RUNTIME_FAILURE = -2,                                 // 0xFFFFFFFE
            XR_ERROR_VALIDATION_FAILURE = -1,                              // 0xFFFFFFFF
            XR_SUCCESS = 0,
            XR_TIMEOUT_EXPIRED = 1,
            XR_SESSION_LOSS_PENDING = 3,
            XR_EVENT_UNAVAILABLE = 4,
            XR_SPACE_BOUNDS_UNAVAILABLE = 7,
            XR_SESSION_NOT_FOCUSED = 8,
            XR_FRAME_DISCARDED = 9,
            XR_RESULT_MAX_ENUM = 2147483647, // 0x7FFFFFFF
        }

        public enum XrStructureType {
            XR_TYPE_UNKNOWN = 0,
            XR_TYPE_API_LAYER_PROPERTIES = 1,
            XR_TYPE_EXTENSION_PROPERTIES = 2,
            XR_TYPE_INSTANCE_CREATE_INFO = 3,
            XR_TYPE_SYSTEM_GET_INFO = 4,
            XR_TYPE_SYSTEM_PROPERTIES = 5,
            XR_TYPE_VIEW_LOCATE_INFO = 6,
            XR_TYPE_VIEW = 7,
            XR_TYPE_SESSION_CREATE_INFO = 8,
            XR_TYPE_SWAPCHAIN_CREATE_INFO = 9,
            XR_TYPE_SESSION_BEGIN_INFO = 10,                                   // 0x0000000A
            XR_TYPE_VIEW_STATE = 11,                                           // 0x0000000B
            XR_TYPE_FRAME_END_INFO = 12,                                       // 0x0000000C
            XR_TYPE_HAPTIC_VIBRATION = 13,                                     // 0x0000000D
            XR_TYPE_EVENT_DATA_BUFFER = 16,                                    // 0x00000010
            XR_TYPE_EVENT_DATA_INSTANCE_LOSS_PENDING = 17,                     // 0x00000011
            XR_TYPE_EVENT_DATA_SESSION_STATE_CHANGED = 18,                     // 0x00000012
            XR_TYPE_ACTION_STATE_BOOLEAN = 23,                                 // 0x00000017
            XR_TYPE_ACTION_STATE_FLOAT = 24,                                   // 0x00000018
            XR_TYPE_ACTION_STATE_VECTOR2F = 25,                                // 0x00000019
            XR_TYPE_ACTION_STATE_POSE = 27,                                    // 0x0000001B
            XR_TYPE_ACTION_SET_CREATE_INFO = 28,                               // 0x0000001C
            XR_TYPE_ACTION_CREATE_INFO = 29,                                   // 0x0000001D
            XR_TYPE_INSTANCE_PROPERTIES = 32,                                  // 0x00000020
            XR_TYPE_FRAME_WAIT_INFO = 33,                                      // 0x00000021
            XR_TYPE_COMPOSITION_LAYER_PROJECTION = 35,                         // 0x00000023
            XR_TYPE_COMPOSITION_LAYER_QUAD = 36,                               // 0x00000024
            XR_TYPE_REFERENCE_SPACE_CREATE_INFO = 37,                          // 0x00000025
            XR_TYPE_ACTION_SPACE_CREATE_INFO = 38,                             // 0x00000026
            XR_TYPE_EVENT_DATA_REFERENCE_SPACE_CHANGE_PENDING = 40,            // 0x00000028
            XR_TYPE_VIEW_CONFIGURATION_VIEW = 41,                              // 0x00000029
            XR_TYPE_SPACE_LOCATION = 42,                                       // 0x0000002A
            XR_TYPE_SPACE_VELOCITY = 43,                                       // 0x0000002B
            XR_TYPE_FRAME_STATE = 44,                                          // 0x0000002C
            XR_TYPE_VIEW_CONFIGURATION_PROPERTIES = 45,                        // 0x0000002D
            XR_TYPE_FRAME_BEGIN_INFO = 46,                                     // 0x0000002E
            XR_TYPE_COMPOSITION_LAYER_PROJECTION_VIEW = 48,                    // 0x00000030
            XR_TYPE_EVENT_DATA_EVENTS_LOST = 49,                               // 0x00000031
            XR_TYPE_INTERACTION_PROFILE_SUGGESTED_BINDING = 51,                // 0x00000033
            XR_TYPE_EVENT_DATA_INTERACTION_PROFILE_CHANGED = 52,               // 0x00000034
            XR_TYPE_INTERACTION_PROFILE_STATE = 53,                            // 0x00000035
            XR_TYPE_SWAPCHAIN_IMAGE_ACQUIRE_INFO = 55,                         // 0x00000037
            XR_TYPE_SWAPCHAIN_IMAGE_WAIT_INFO = 56,                            // 0x00000038
            XR_TYPE_SWAPCHAIN_IMAGE_RELEASE_INFO = 57,                         // 0x00000039
            XR_TYPE_ACTION_STATE_GET_INFO = 58,                                // 0x0000003A
            XR_TYPE_HAPTIC_ACTION_INFO = 59,                                   // 0x0000003B
            XR_TYPE_SESSION_ACTION_SETS_ATTACH_INFO = 60,                      // 0x0000003C
            XR_TYPE_ACTIONS_SYNC_INFO = 61,                                    // 0x0000003D
            XR_TYPE_BOUND_SOURCES_FOR_ACTION_ENUMERATE_INFO = 62,              // 0x0000003E
            XR_TYPE_INPUT_SOURCE_LOCALIZED_NAME_GET_INFO = 63,                 // 0x0000003F
            XR_TYPE_COMPOSITION_LAYER_CUBE_KHR = 1000006000,                   // 0x3B9AE170
            XR_TYPE_INSTANCE_CREATE_INFO_ANDROID_KHR = 1000008000,             // 0x3B9AE940
            XR_TYPE_COMPOSITION_LAYER_DEPTH_INFO_KHR = 1000010000,             // 0x3B9AF110
            XR_TYPE_VULKAN_SWAPCHAIN_FORMAT_LIST_CREATE_INFO_KHR = 1000014000, // 0x3B9B00B0
            XR_TYPE_EVENT_DATA_PERF_SETTINGS_EXT = 1000015000,                 // 0x3B9B0498
            XR_TYPE_COMPOSITION_LAYER_CYLINDER_KHR = 1000017000,               // 0x3B9B0C68
            XR_TYPE_COMPOSITION_LAYER_EQUIRECT_KHR = 1000018000,               // 0x3B9B1050
            XR_TYPE_DEBUG_UTILS_OBJECT_NAME_INFO_EXT = 1000019000,             // 0x3B9B1438
            XR_TYPE_DEBUG_UTILS_MESSENGER_CALLBACK_DATA_EXT = 1000019001,      // 0x3B9B1439
            XR_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT = 1000019002,        // 0x3B9B143A
            XR_TYPE_DEBUG_UTILS_LABEL_EXT = 1000019003,                        // 0x3B9B143B
            XR_TYPE_GRAPHICS_BINDING_OPENGL_WIN32_KHR = 1000023000,            // 0x3B9B23D8
            XR_TYPE_GRAPHICS_BINDING_OPENGL_XLIB_KHR = 1000023001,             // 0x3B9B23D9
            XR_TYPE_GRAPHICS_BINDING_OPENGL_XCB_KHR = 1000023002,              // 0x3B9B23DA
            XR_TYPE_GRAPHICS_BINDING_OPENGL_WAYLAND_KHR = 1000023003,          // 0x3B9B23DB
            XR_TYPE_SWAPCHAIN_IMAGE_OPENGL_KHR = 1000023004,                   // 0x3B9B23DC
            XR_TYPE_GRAPHICS_REQUIREMENTS_OPENGL_KHR = 1000023005,             // 0x3B9B23DD
            XR_TYPE_GRAPHICS_BINDING_OPENGL_ES_ANDROID_KHR = 1000024001,       // 0x3B9B27C1
            XR_TYPE_SWAPCHAIN_IMAGE_OPENGL_ES_KHR = 1000024002,                // 0x3B9B27C2
            XR_TYPE_GRAPHICS_REQUIREMENTS_OPENGL_ES_KHR = 1000024003,          // 0x3B9B27C3
            XR_TYPE_GRAPHICS_BINDING_VULKAN_KHR = 1000025000,                  // 0x3B9B2BA8
            XR_TYPE_SWAPCHAIN_IMAGE_VULKAN_KHR = 1000025001,                   // 0x3B9B2BA9
            XR_TYPE_GRAPHICS_REQUIREMENTS_VULKAN_KHR = 1000025002,             // 0x3B9B2BAA
            XR_TYPE_GRAPHICS_BINDING_D3D11_KHR = 1000027000,                   // 0x3B9B3378
            XR_TYPE_SWAPCHAIN_IMAGE_D3D11_KHR = 1000027001,                    // 0x3B9B3379
            XR_TYPE_GRAPHICS_REQUIREMENTS_D3D11_KHR = 1000027002,              // 0x3B9B337A
            XR_TYPE_GRAPHICS_BINDING_D3D12_KHR = 1000028000,                   // 0x3B9B3760
            XR_TYPE_SWAPCHAIN_IMAGE_D3D12_KHR = 1000028001,                    // 0x3B9B3761
            XR_TYPE_GRAPHICS_REQUIREMENTS_D3D12_KHR = 1000028002,              // 0x3B9B3762
            XR_TYPE_VISIBILITY_MASK_KHR = 1000031000,                          // 0x3B9B4318
            XR_TYPE_EVENT_DATA_VISIBILITY_MASK_CHANGED_KHR = 1000031001,       // 0x3B9B4319
            XR_TYPE_SPATIAL_ANCHOR_CREATE_INFO_MSFT = 1000039000,              // 0x3B9B6258
            XR_TYPE_SPATIAL_ANCHOR_SPACE_CREATE_INFO_MSFT = 1000039001,        // 0x3B9B6259
            XR_TYPE_VIEW_CONFIGURATION_DEPTH_RANGE_EXT = 1000046000,           // 0x3B9B7DB0
            XR_STRUCTURE_TYPE_MAX_ENUM = 2147483647,                           // 0x7FFFFFFF
        }

        private enum XrFormFactor {
            XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY = 1,
            XR_FORM_FACTOR_HANDHELD_DISPLAY = 2,
            XR_FORM_FACTOR_MAX_ENUM = 2147483647, // 0x7FFFFFFF
        }

        private struct XrSystemGetInfo {
            public XrStructureType type;
            public void* next;
            public XrFormFactor formFactor;
        }

        public struct XrSystemProperties {
            public XrStructureType type;
            public void* next;
            public ulong systemId;
            public uint vendorId;
            public fixed sbyte systemName[256];
            public XrSystemGraphicsProperties graphicsProperties;
            public XrSystemTrackingProperties trackingProperties;
        }

        public struct XrSystemGraphicsProperties {
            public uint maxSwapchainImageHeight;
            public uint maxSwapchainImageWidth;
            public uint maxLayerCount;
        }

        public struct XrSystemTrackingProperties {
            public uint orientationTracking;
            public uint positionTracking;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr XrInstanceFuncDelegate();

        #endregion

        private class InvalidPointerException : Exception {
            public InvalidPointerException(IntPtr ptr) : base("Unable to access data at " + ptr) { }
        }

        private const string DLLName = "openxr_loader";
        private const long DLLGetLoaderInstanceFuncPtr = 0x00038980;

        public static XrSystemProperties? SystemProperties { get; private set; }

        public static void Init() {
            try {
                if (InitInternal() is var res and not XrResult.XR_SUCCESS) 
                    throw new InvalidOperationException($"Failed to acquire OpenXR data: {res}");
            } catch (Exception ex) {
                Plugin.Log.Critical(ex);
            }
        }
        
        private static XrResult InitInternal() {
            var xrInstance = AcquireXrInstancePtr();

            var info = new XrSystemGetInfo {
                type = XrStructureType.XR_TYPE_SYSTEM_GET_INFO,
                formFactor = XrFormFactor.XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY
            };
            var systemId = default(ulong);
            if (GetSystem(xrInstance, &info, &systemId) is
                var getSysRes and not XrResult.XR_SUCCESS) return getSysRes; 

            var systemProperties = new XrSystemProperties {
                type = XrStructureType.XR_TYPE_SYSTEM_PROPERTIES
            };
            if (GetSystemProperties(xrInstance, systemId, &systemProperties) is
                var getSysPropRes and not XrResult.XR_SUCCESS) return getSysPropRes;

            SystemProperties = systemProperties;
            return XrResult.XR_SUCCESS;
        }

        private static XrInstance_T* AcquireXrInstancePtr() {
            var moduleHandlePtr = GetModuleHandle(DLLName);
            if (moduleHandlePtr == IntPtr.Zero) throw new InvalidPointerException(moduleHandlePtr);

            var funcAddress = ApplyOffset(moduleHandlePtr, DLLGetLoaderInstanceFuncPtr);
            var getLoaderInstanceFunc = Marshal.GetDelegateForFunctionPointer<XrInstanceFuncDelegate>(funcAddress);
            if (getLoaderInstanceFunc == null) throw new InvalidPointerException(funcAddress);

            var loaderInstancePtr = new IntPtr(*(long*)getLoaderInstanceFunc());
            if (loaderInstancePtr == IntPtr.Zero) throw new InvalidPointerException(loaderInstancePtr);

            var xrInstancePtr = *(long*)ApplyOffset(loaderInstancePtr, 8);
            return (XrInstance_T*)xrInstancePtr;
        }
        
        private static IntPtr ApplyOffset(IntPtr ptr, long offset) => new((byte*)ptr.ToPointer() + offset);

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(
            string lpModuleName);

        [DllImport(DLLName, EntryPoint = "xrGetSystemProperties", CallingConvention = CallingConvention.Cdecl)]
        private static extern XrResult GetSystemProperties(
            XrInstance_T* instance,
            ulong systemId,
            XrSystemProperties* properties);

        [DllImport(DLLName, EntryPoint = "xrGetSystem", CallingConvention = CallingConvention.Cdecl)]
        private static extern XrResult GetSystem(
            XrInstance_T* instance,
            XrSystemGetInfo* getInfo,
            ulong* systemId);
    }
}