// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nanodogs.UniversalScripts
{
    public class LevelLoadTrigger : NanoTrigger
    {
        public string levelName;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Nanoloader.LoadLevel(levelName);
                Destroy(gameObject);
            }
        }
    }
}