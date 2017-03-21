using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Darkness
{
    [RequireComponent(typeof(Camera))]
    public class DarknessEffect : MonoBehaviour
    {
        public Material CombineMaterial;
        public Shader DepthRendererShader;

        private Material _combineMaterial;
        private Camera _camera;

        private GameObject _depthCameraObj;
        private Camera _depthCamera;

        private GameObject _darknessCameraObj;
        private Camera _darknessCamera;

        private RenderTexture _target;

        void Start ()
        {
            _camera = Camera.main;

            // Half sized render texture with 16bit depth buffer
            _target = new RenderTexture(
                _camera.pixelWidth,
                _camera.pixelHeight, 1,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default)
            {
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Clamp,
                name = "DARKNESS TARGET",
                antiAliasing = 1,
                anisoLevel = 2,
                depth = 16,
            };
            _target.Create();

            _depthCameraObj = new GameObject { name = "Depth camera" };
            _depthCameraObj.transform.SetParent(transform, false);
            _depthCamera = _depthCameraObj.AddComponent<Camera>();
            _depthCamera.enabled = false;
            _depthCamera.aspect = _camera.aspect;
            _depthCamera.projectionMatrix = _camera.projectionMatrix;
            _depthCamera.targetTexture = _target;
            _depthCamera.cullingMask = _camera.cullingMask; // default layer for now
            _depthCamera.clearFlags = CameraClearFlags.Nothing;
            _depthCamera.SetReplacementShader(DepthRendererShader, "RenderType");
            

            _darknessCameraObj = new GameObject { name = "Darkness camera" };
            _darknessCameraObj.transform.SetParent(transform, false);
            _darknessCamera = _darknessCameraObj.AddComponent<Camera>();
            _darknessCamera.enabled = false;
            _darknessCamera.aspect = _camera.aspect;
            _darknessCamera.projectionMatrix = _camera.projectionMatrix;
            _darknessCamera.targetTexture = _target;
            _darknessCamera.cullingMask = LayerMask.GetMask(WellKnown.LayerNames.Darkness, WellKnown.LayerNames.DrawableDarkness);
            _darknessCamera.clearFlags = CameraClearFlags.Nothing;
            _darknessCamera.backgroundColor = Color.clear;

            //_combineMaterial = Instantiate(CombineMaterial);
            _combineMaterial = CombineMaterial;
            _combineMaterial.SetTexture("_StateTex", _target);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            var last = RenderTexture.active;
            RenderTexture.active = _target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = last;

            _depthCamera.Render();
            _darknessCamera.Render();
            
            Graphics.Blit(src, dest, _combineMaterial);
        }
    }
}
