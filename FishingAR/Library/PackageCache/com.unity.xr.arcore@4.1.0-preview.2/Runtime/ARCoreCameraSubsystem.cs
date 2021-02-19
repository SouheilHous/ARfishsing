using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// The camera subsystem implementation for ARCore.
    /// </summary>
    [Preserve]
    public sealed class ARCoreCameraSubsystem : XRCameraSubsystem
    {
        /// <summary>
        /// The identifying name for the camera-providing implementation.
        /// </summary>
        /// <value>
        /// The identifying name for the camera-providing implementation.
        /// </value>
        const string k_SubsystemId = "ARCore-Camera";

        /// <summary>
        /// The name for the shader for rendering the camera texture.
        /// </summary>
        /// <value>
        /// The name for the shader for rendering the camera texture.
        /// </value>
        const string k_DefaultBackgroundShaderName = "Unlit/ARCoreBackground";

        enum CameraConfigurationResult
        {
            /// <summary>
            /// Setting the camera configuration was successful.
            /// </summary>
            Success = 0,

            /// <summary>
            /// The given camera configuration was not valid to be set by the provider.
            /// </summary>
            InvalidCameraConfiguration = 1,

            /// <summary>
            /// The provider session was invalid.
            /// </summary>
            InvalidSession = 2,

            /// <summary>
            /// An error occurred because the user did not dispose of all <c>XRCpuImage</c> and did not allow all
            /// asynchronous conversion jobs complete before changing the camera configuration.
            /// </summary>
            ErrorImagesNotDisposed = 3,
        }

        /// <summary>
        /// The name for the background shader based on the current render pipeline.
        /// </summary>
        /// <value>
        /// The name for the background shader based on the current render pipeline. Or, <c>null</c> if the current
        /// render pipeline is incompatible with the set of shaders.
        /// </value>
        /// <remarks>
        /// The value for the <c>GraphicsSettings.renderPipelineAsset</c> is not expected to change within the lifetime
        /// of the application.
        /// </remarks>
        public static string backgroundShaderName => k_DefaultBackgroundShaderName;

        /// <summary>
        /// Create and register the camera subsystem descriptor to advertise a providing implementation for camera
        /// functionality.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            if (!Api.Android)
                return;

            var cameraSubsystemCinfo = new XRCameraSubsystemCinfo
            {
                id = k_SubsystemId,
#if UNITY_2020_2_OR_NEWER
                providerType = typeof(ARCoreCameraSubsystem.ARCoreProvider),
                subsystemTypeOverride = typeof(ARCoreCameraSubsystem),
#else
                implementationType = typeof(ARCoreCameraSubsystem),
#endif
                supportsAverageBrightness = true,
                supportsAverageColorTemperature = false,
                supportsColorCorrection = true,
                supportsDisplayMatrix = true,
                supportsProjectionMatrix = true,
                supportsTimestamp = true,
                supportsCameraConfigurations = true,
                supportsCameraImage = true,
                supportsAverageIntensityInLumens = false,
                supportsFocusModes = true,
                supportsFaceTrackingAmbientIntensityLightEstimation = true,
                supportsFaceTrackingHDRLightEstimation = false,
                supportsWorldTrackingAmbientIntensityLightEstimation = true,
                supportsWorldTrackingHDRLightEstimation = true,
                supportsCameraGrain = false,
            };

            if (!XRCameraSubsystem.Register(cameraSubsystemCinfo))
            {
                Debug.LogError($"Failed to register the {k_SubsystemId} subsystem.");
            }
        }

#if !UNITY_2020_2_OR_NEWER
        /// <summary>
        /// Create the ARCore camera functionality provider for the camera subsystem.
        /// </summary>
        /// <returns>
        /// The ARCore camera functionality provider for the camera subsystem.
        /// </returns>
        protected override Provider CreateProvider() => new ARCoreProvider();
#endif

        /// <summary>
        /// Provides the camera functionality for the ARCore implementation.
        /// </summary>
        class ARCoreProvider : Provider
        {
            /// <summary>
            /// The shader property name for the main texture of the camera video frame.
            /// </summary>
            /// <value>
            /// The shader property name for the main texture of the camera video frame.
            /// </value>
            const string k_MainTexPropertyName = "_MainTex";

            /// <summary>
            /// The name of the camera permission for Android.
            /// </summary>
            /// <value>
            /// The name of the camera permission for Android.
            /// </value>
            const string k_CameraPermissionName = "android.permission.CAMERA";

            /// <summary>
            /// The shader property name identifier for the main texture of the camera video frame.
            /// </summary>
            /// <value>
            /// The shader property name identifier for the main texture of the camera video frame.
            /// </value>
            static readonly int k_MainTexPropertyNameId = Shader.PropertyToID(k_MainTexPropertyName);

            /// <summary>
            /// Get the material used by <c>XRCameraSubsystem</c> to render the camera texture.
            /// </summary>
            /// <returns>
            /// The material to render the camera texture.
            /// </returns>
            public override Material cameraMaterial => m_CameraMaterial;
            Material m_CameraMaterial;

            /// <summary>
            /// Determine whether camera permission has been granted.
            /// </summary>
            /// <returns>
            /// <c>true</c> if camera permission has been granted for this app. Otherwise, <c>false</c>.
            /// </returns>
            public override bool permissionGranted => ARCorePermissionManager.IsPermissionGranted(k_CameraPermissionName);

            public override bool invertCulling => NativeApi.UnityARCore_Camera_ShouldInvertCulling();

            public override XRCpuImage.Api cpuImageApi => ARCoreCpuImageApi.instance;

            /// <summary>
            /// Construct the camera functionality provider for ARCore.
            /// </summary>
            public ARCoreProvider()
            {
                NativeApi.UnityARCore_Camera_Construct(k_MainTexPropertyNameId);
                m_CameraMaterial = CreateCameraMaterial(ARCoreCameraSubsystem.backgroundShaderName);
            }

            /// <summary>
            /// Get the camera facing direction.
            /// </summary>
            public override Feature currentCamera => NativeApi.GetCurrentFacingDirection();

            /// <summary>
            /// Get the currently active camera or set the requested camera
            /// </summary>
            public override Feature requestedCamera
            {
                get => Api.GetRequestedFeatures();
                set
                {
                    Api.SetFeatureRequested(Feature.AnyCamera, false);
                    Api.SetFeatureRequested(value, true);
                }
            }

            /// <summary>
            /// Start the camera functionality.
            /// </summary>
            public override void Start() => NativeApi.UnityARCore_Camera_Start();

            /// <summary>
            /// Stop the camera functionality.
            /// </summary>
            public override void Stop() => NativeApi.UnityARCore_Camera_Stop();

            /// <summary>
            /// Destroy any resources required for the camera functionality.
            /// </summary>
            public override void Destroy() => NativeApi.UnityARCore_Camera_Destruct();

            /// <summary>
            /// Get the camera frame for the subsystem.
            /// </summary>
            /// <param name="cameraParams">The current Unity <c>Camera</c> parameters.</param>
            /// <param name="cameraFrame">The current camera frame returned by the method.</param>
            /// <returns>
            /// <c>true</c> if the method successfully got a frame. Otherwise, <c>false</c>.
            /// </returns>
            public override bool TryGetFrame(XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
            {
                return NativeApi.UnityARCore_Camera_TryGetFrame(cameraParams, out cameraFrame);
            }

            /// <summary>
            /// Get or set the focus mode for the camera.
            /// </summary>
            public override bool autoFocusRequested
            {
                get => Api.GetRequestedFeatures().All(Feature.AutoFocus);
                set => Api.SetFeatureRequested(Feature.AutoFocus, value);
            }

            /// <summary>
            /// Get the actual auto focus state
            /// </summary>
            public override bool autoFocusEnabled => NativeApi.GetAutoFocusEnabled();

            /// <summary>
            /// Called on the render thread by background rendering code immediately before the background
            /// is rendered.
            /// For ARCore this is required in order to submit the GL commands for waiting on the fence
            /// created on the main thread after calling ArPresto_Update().
            /// </summary>
            /// <param name="id">Platform-specific data.</param>
            public override void OnBeforeBackgroundRender(int id)
            {
                NativeApi.UnityARCore_Camera_GetFenceWaitHandler(id);
            }

            /// <summary>
            /// Get or set the light estimation mode.
            /// </summary>
            public override Feature requestedLightEstimation
            {
                get => Api.GetRequestedFeatures();
                set
                {
                    Api.SetFeatureRequested(Feature.AnyLightEstimation, false);
                    Api.SetFeatureRequested(value, true);
                }
            }

            /// <summary>
            /// Get the light estimation features currently enabled
            /// </summary>
            public override Feature currentLightEstimation => NativeApi.GetCurrentLightEstimation();

            /// <summary>
            /// Get the camera intrinisics information.
            /// </summary>
            /// <param name="cameraIntrinsics">The camera intrinsics information returned from the method.</param>
            /// <returns>
            /// <c>true</c> if the method successfully gets the camera intrinsics information. Otherwise, <c>false</c>.
            /// </returns>
            public override bool TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics) => NativeApi.UnityARCore_Camera_TryGetIntrinsics(out cameraIntrinsics);

            /// <summary>
            /// Queries the supported camera configurations.
            /// </summary>
            /// <param name="defaultCameraConfiguration">A default value used to fill the returned array before copying
            /// in real values. This ensures future additions to this struct are backwards compatible.</param>
            /// <param name="allocator">The allocation strategy to use for the returned data.</param>
            /// <returns>
            /// The supported camera configurations.
            /// </returns>
            public override NativeArray<XRCameraConfiguration> GetConfigurations(XRCameraConfiguration defaultCameraConfiguration,
                                                                                 Allocator allocator)
            {
                IntPtr configurations = NativeApi.UnityARCore_Camera_AcquireConfigurations(out int configurationsCount,
                                                                                           out int configurationSize);
                try
                {
                    unsafe
                    {
                        return NativeCopyUtility.PtrToNativeArrayWithDefault(defaultCameraConfiguration,
                                                                             (void*)configurations,
                                                                             configurationSize, configurationsCount,
                                                                             allocator);
                    }
                }
                finally
                {
                    NativeApi.UnityARCore_Camera_ReleaseConfigurations(configurations);
                }
            }

            /// <summary>
            /// The current camera configuration.
            /// </summary>
            /// <value>
            /// The current camera configuration if it exists. Otherise, <c>null</c>.
            /// </value>
            /// <exception cref="System.ArgumentException">Thrown when setting the current configuration if the given
            /// configuration is not a valid, supported camera configuration.</exception>
            /// <exception cref="System.InvalidOperationException">Thrown when setting the current configuration if the
            /// implementation is unable to set the current camera configuration for various reasons such as:
            /// <list type="bullet">
            /// <item><description>ARCore session is invalid</description></item>
            /// <item><description>Captured <c>XRCpuImage</c> have not been disposed</description></item>
            /// </list>
            /// </exception>
            /// <seealso cref="TryAcquireLatestCpuImage"/>
            public override XRCameraConfiguration? currentConfiguration
            {
                get
                {
                    if (TryGetCurrentConfiguration(out XRCameraConfiguration cameraConfiguration))
                    {
                        return cameraConfiguration;
                    }

                    return null;
                }
                set
                {
                    // Assert that the camera configuration is not null.
                    // The XRCameraSubsystem should have already checked this.
                    Debug.Assert(value != null, "Cannot set the current camera configuration to null");

                    switch (NativeApi.UnityARCore_Camera_TrySetCurrentConfiguration((XRCameraConfiguration)value))
                    {
                        case CameraConfigurationResult.Success:
                            break;
                        case CameraConfigurationResult.InvalidCameraConfiguration:
                            throw new ArgumentException("Camera configuration does not exist in the available "
                                                        + "configurations", "value");
                        case CameraConfigurationResult.InvalidSession:
                            throw new InvalidOperationException("Cannot set camera configuration because the ARCore "
                                                                + "session is not valid");
                        case CameraConfigurationResult.ErrorImagesNotDisposed:
                            throw new InvalidOperationException("Cannot set camera configuration because you have not "
                                                                + "disposed of all XRCpuImage and allowed all "
                                                                + "asynchronous conversion jobs to complete");
                        default:
                            throw new InvalidOperationException("cannot set camera configuration for ARCore");
                    }
                }
            }

            /// <summary>
            /// Gets the texture descriptors associated with the camera image.
            /// </summary>
            /// <returns>The texture descriptors.</returns>
            /// <param name="defaultDescriptor">Default descriptor.</param>
            /// <param name="allocator">Allocator.</param>
            public unsafe override NativeArray<XRTextureDescriptor> GetTextureDescriptors(
                XRTextureDescriptor defaultDescriptor,
                Allocator allocator)
            {
                int length, elementSize;
                var textureDescriptors = NativeApi.UnityARCore_Camera_AcquireTextureDescriptors(
                    out length, out elementSize);

                try
                {
                    return NativeCopyUtility.PtrToNativeArrayWithDefault(
                        defaultDescriptor,
                        textureDescriptors, elementSize, length, allocator);
                }
                finally
                {
                    NativeApi.UnityARCore_Camera_ReleaseTextureDescriptors(textureDescriptors);
                }
            }

            /// <summary>
            /// Query for the latest native camera image.
            /// </summary>
            /// <param name="cameraImageCinfo">The metadata required to construct a <see cref="XRCpuImage"/></param>
            /// <returns>
            /// <c>true</c> if the camera image is acquired. Otherwise, <c>false</c>.
            /// </returns>
            public override bool TryAcquireLatestCpuImage(out XRCpuImage.Cinfo cameraImageCinfo)
                => ARCoreCpuImageApi.TryAcquireLatestImage(ARCoreCpuImageApi.ImageType.Camera, out cameraImageCinfo);
        }

        internal static bool TryGetCurrentConfiguration(out XRCameraConfiguration configuration) => NativeApi.UnityARCore_Camera_TryGetCurrentConfiguration(out configuration);

        /// <summary>
        /// Container to wrap the native ARCore camera APIs.
        /// </summary>
        static class NativeApi
        {
            [DllImport("UnityARCore")]
            public static extern void UnityARCore_Camera_Construct(int mainTexPropertyNameId);

            [DllImport("UnityARCore")]
            public static extern void UnityARCore_Camera_Destruct();

            [DllImport("UnityARCore")]
            public static extern void UnityARCore_Camera_Start();

            [DllImport("UnityARCore")]
            public static extern void UnityARCore_Camera_Stop();

            [DllImport("UnityARCore")]
            public static extern bool UnityARCore_Camera_TryGetFrame(XRCameraParams cameraParams,
                                                                    out XRCameraFrame cameraFrame);

            [DllImport("UnityARCore", EntryPoint="UnityARCore_Camera_GetAutoFocusEnabled")]
            public static extern bool GetAutoFocusEnabled();

            [DllImport("UnityARCore", EntryPoint="UnityARCore_Camera_GetCurrentLightEstimation")]
            public static extern Feature GetCurrentLightEstimation();

            [DllImport("UnityARCore")]
            public static extern bool UnityARCore_Camera_TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics);

            [DllImport("UnityARCore")]
            public static extern IntPtr UnityARCore_Camera_AcquireConfigurations(out int configurationsCount,
                                                                                 out int configurationSize);

            [DllImport("UnityARCore")]
            public static extern void UnityARCore_Camera_ReleaseConfigurations(IntPtr configurations);

            [DllImport("UnityARCore")]
            public static extern bool UnityARCore_Camera_TryGetCurrentConfiguration(out XRCameraConfiguration cameraConfiguration);

            [DllImport("UnityARCore")]
            public static extern CameraConfigurationResult UnityARCore_Camera_TrySetCurrentConfiguration(XRCameraConfiguration cameraConfiguration);

            [DllImport("UnityARCore")]
            public static extern unsafe void* UnityARCore_Camera_AcquireTextureDescriptors(
                out int length, out int elementSize);

            [DllImport("UnityARCore")]
            public static extern unsafe void UnityARCore_Camera_ReleaseTextureDescriptors(
                void* descriptors);

            [DllImport("UnityARCore")]
            public static extern bool UnityARCore_Camera_ShouldInvertCulling();

            [DllImport("UnityARCore", EntryPoint="UnityARCore_Camera_GetCurrentFacingDirection")]
            public static extern Feature GetCurrentFacingDirection();

            [DllImport("UnityARCore")]
            public static extern void UnityARCore_Camera_GetFenceWaitHandler(int unused);
        }
    }
}
