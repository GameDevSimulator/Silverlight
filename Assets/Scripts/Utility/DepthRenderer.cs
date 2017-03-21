using UnityEngine;
using System.Collections;

/// <summary>
/// The purpose of this shader is to render scena objects (residing in a particular 
/// layer) in depth-only mode. The point is to restore the state of the depth buffer
/// before the scene is rendered with the camera which this script is attached to.
/// This is useful in case you need to perform rendering AFTER some screen-space image 
/// effects (such as Tonemapping or Contrast Enhance etc.) as those effects wipe out the 
/// contents of the depth buffer. Another example would be rendering scene in HDR and then 
/// rendering some LDR overlays.
/// USAGE: attach this script to a camera object with depth greater than that of a camera, 
/// which renders earlier and has image effects scripts attached to it.
/// </summary>
[RequireComponent(typeof(Camera))]
public class DepthRenderer : MonoBehaviour
{
    GameObject depthCameraObject = null;
    Shader replacementShader = null;

    // Use this for initialization
    void Start()
    {
        var camera = GetComponent<Camera>();
        //depthCameraObject = new GameObject { hideFlags = HideFlags.HideAndDontSave };
        depthCameraObject = new GameObject();


        var depthCamera = depthCameraObject.AddComponent<Camera>();
        depthCamera.depthTextureMode = DepthTextureMode.Depth;
        depthCamera.enabled = false;
        depthCamera.CopyFrom(camera);
        depthCamera.cullingMask = 1 << 0; // default layer for now
        depthCamera.clearFlags = CameraClearFlags.Depth;
        

        replacementShader = Shader.Find("RenderDepth");
        depthCamera.SetReplacementShader(replacementShader, "RenderType");
        if (replacementShader == null)
        {
            Debug.LogError("could not find 'RenderDepth' shader");
        }
    }

    // Update is called once per frame
    void OnPreRender()
    {
        if (replacementShader != null)
        {
            var camera = GetComponent<Camera>();
            var camCopy = depthCameraObject.GetComponent<Camera>();

            // copy position and location;
            camCopy.transform.position = camera.transform.position;
            camCopy.transform.rotation = camera.transform.rotation;
            camCopy.Render();
            //camCopy.RenderWithShader(replacementShader, "RenderType");
        }
    }
}