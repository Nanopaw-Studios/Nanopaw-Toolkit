// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;

namespace Nanodogs.UniversalScripts
{
    public class NanoPlayerTemplate : MonoBehaviour
    {
        public Transform cameraTransform;
        public Transform playerBodyTransform;

        protected void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}