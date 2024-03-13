﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using BeatLeader.SteamVR;
using JetBrains.Annotations;

namespace BeatLeader.Utils {
    internal static unsafe class OpenXRAcquirer {
        #region InteropStructures

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        // ReSharper disable once InconsistentNaming
        private struct XrInstance_T { }

        [Flags, UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private enum XrResult {
            XR_SUCCESS = 0,
            XR_ERROR_VALIDATION_FAILURE = -1,
            XR_ERROR_RUNTIME_FAILURE = -2,
            XR_ERROR_OUT_OF_MEMORY = -3,
            XR_ERROR_HANDLE_INVALID = -12,
            XR_ERROR_INSTANCE_LOST = -13,
            XR_ERROR_SESSION_NOT_RUNNING = -16,
            XR_ERROR_SYSTEM_INVALID = -18,
            XR_ERROR_FORM_FACTOR_UNSUPPORTED = -34,
            XR_ERROR_FORM_FACTOR_UNAVAILABLE = -35,
        }

        public enum XrStructureType {
            XR_TYPE_SYSTEM_GET_INFO = 4,
            XR_TYPE_SYSTEM_PROPERTIES = 5,
        }

        private enum XrFormFactor {
            XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY = 1,
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private struct XrSystemGetInfo {
            public XrStructureType type;
            public void* next;
            public XrFormFactor formFactor;
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public struct XrSystemProperties {
            public XrStructureType type;
            public void* next;
            public ulong systemId;
            public uint vendorId;
            public fixed sbyte systemName[256];
            public XrSystemGraphicsProperties graphicsProperties;
            public XrSystemTrackingProperties trackingProperties;
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public struct XrSystemGraphicsProperties {
            public uint maxSwapchainImageHeight;
            public uint maxSwapchainImageWidth;
            public uint maxLayerCount;
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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
        private const long DLLGetLoaderInstanceFuncPtr = 0x00039480;

        public static XrSystemProperties? SystemProperties { get; private set; }
        public static string? SystemName { get; private set; }

        public static void Init(VRPlatformSDK vRPlatformSDK) {
            try {
                var res = InitInternal();
                if (vRPlatformSDK is VRPlatformSDK.OpenXR && SystemName?.ToLower().Contains("pico") != true) {
                    SteamVRSettings.UpdateAsync();
                }
                if (res == XrResult.XR_SUCCESS) {
                    return;
                }
                if (res is XrResult.XR_ERROR_SESSION_NOT_RUNNING) {
                    Plugin.Log.Warn("OpenXR session is not running, info won't be available!");
                } else {
                    Plugin.Log.Error($"Failed to acquire OpenXR data: {res}");
                }
            } catch (Exception ex) {
                Plugin.Log.Critical(ex);
            }
        }

        private static XrResult InitInternal() {
            var xrInstance = AcquireXrInstancePtr();
            if (xrInstance is null) return XrResult.XR_ERROR_SESSION_NOT_RUNNING;

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
            SystemName = new string(systemProperties.systemName);
            return XrResult.XR_SUCCESS;
        }

        private static XrInstance_T* AcquireXrInstancePtr() {
            var moduleHandlePtr = GetModuleHandle(DLLName);
            if (moduleHandlePtr == IntPtr.Zero) return null;

            var funcAddress = ApplyOffset(moduleHandlePtr, DLLGetLoaderInstanceFuncPtr);
            int byteCount = 8;
            byte[] buffer = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                buffer[i] = Marshal.ReadByte(funcAddress, i);
            }
            string result = Encoding.ASCII.GetString(buffer);
            if (result != "H??(eH?") { return null; }

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