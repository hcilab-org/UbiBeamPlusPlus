using UnityEngine;
using System.Collections;
using Meta;

///<summary> An example to use camera feed via code.</summary>
///
///<seealso cref="T:UnityEngine.MonoBehaviour"/>
public class CameraFeedExample : MonoBehaviour 
{

    public int sourceDevice = 1;  //  for color feed texture set value = 0, for depth set value = 1, for ir set value = 2;
    /*WARNING: the depthdata is converted to rgb space for display purposes. The values in the depth texture do not represent the actual depth value*/

    public MeshRenderer renderTarget;

    public Texture2D cameraTexture;

  
    void Start()
    {
        //sanity check. espcially if intended to use in Awake() or before that 
        if (DeviceTextureSource.Instance != null && MetaCore.Instance != null)
        {
            DeviceTextureSource.Instance.registerTextureDevice(sourceDevice);
        }
        //get the texture
        if (DeviceTextureSource.Instance.IsDeviceTextureRegistered(sourceDevice))
        {
            cameraTexture = DeviceTextureSource.Instance.GetDeviceTexture(sourceDevice);

            // if a rendering target is set. Display it
            if (renderTarget != null && renderTarget.material != null)
            {
                if (DeviceTextureSource.Instance != null && DeviceTextureSource.Instance.enabled)
                {
                    renderTarget.material.mainTexture = cameraTexture;
                }
            }
        }
        else
        {
            Debug.LogError("trying to access unregistered device texture");
        }
    }

    void OnDestroy()
    {
        //Sanity check. Espcially if intended to use in Awake() or before that 
        if (DeviceTextureSource.Instance != null && MetaCore.Instance != null)
        {
            DeviceTextureSource.Instance.unregisterTextureDevice(sourceDevice);
        }
    }

}
