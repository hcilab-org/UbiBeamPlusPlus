using UnityEngine;
using System.Collections;

namespace Meta.Apps.MetaSDKGuide
{

    /// <summary>
    /// Controls the HUDCube in the Meta SDK Guide scene
    /// </summary>
    public class HudCube : MonoBehaviour
    {

        [SerializeField]
        private GameObject _hudCube;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Sets the cube's position to the centre of the hud so that it is not cropped off
        /// </summary>
        public void SetHudCubePos()
        {
            if (MetaCore.Instance.transform.rotation != Quaternion.identity)
            {
                _hudCube.transform.position = Camera.main.transform.position;
                _hudCube.transform.Translate(Camera.main.transform.forward * 0.4f, relativeTo: Space.World);
            }
        }
    }

}
