// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;

namespace Nanodogs.UniversalScripts
{
    public class NanoPlayerSpawnpoint : MonoBehaviour
    {
        public NanoPlayerTemplate playerTemplate;
        private static GameObject player; // ✅ static so it persists across scenes
        public static NanoPlayerSpawnpoint Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }


        public static GameObject GetPlayer()
        {
            return player;
        }

        private void Start()
        {
            // ✅ If a player already exists (DontDestroyOnLoad), don’t spawn another
            if (player != null)
                return;

            player = Instantiate(playerTemplate.gameObject, transform.position, transform.rotation);
            player.tag = "Player";
            DontDestroyOnLoad(player);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.right * 0.5f);
        }

        [ContextMenu("Zero Spawnpoint")]
        public void Zero()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
    }
}
