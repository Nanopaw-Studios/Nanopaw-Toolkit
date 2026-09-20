using UnityEngine;

namespace Nanodogs.Toolkit.Other
{
    [ExecuteAlways]
    /// <summary>
    /// Syncs the sun direction of the skybox material with the forward direction of the GameObject this script is attached to.
    /// </summary>
    public class SyncSunDirection : MonoBehaviour
    {
        public Material skyboxMaterial;

        void Update()
        {
            if (skyboxMaterial != null)
            {
                skyboxMaterial.SetVector("_SunDirection", transform.forward);
            }
        }
    }
}