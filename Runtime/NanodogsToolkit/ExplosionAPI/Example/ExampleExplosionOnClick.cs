// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;

namespace Nanodogs.API.Explosion
{
    /// <summary>
    /// Example script to demonstrate how to use the ExplosionAPI to create an explosion on mouse click.
    /// </summary>
    public class ExampleExplosionOnClick : MonoBehaviour
    {
        public ExplosionSettings settings;

        void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    ExplosionAPI.Explosion(hit.point, settings);
                }
            }
        }
    }
}
