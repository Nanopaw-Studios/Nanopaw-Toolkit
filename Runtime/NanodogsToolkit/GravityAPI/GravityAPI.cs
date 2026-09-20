// © 2025 Nanodogs Studios. All rights reserved.

using UnityEngine;

namespace Nanodogs.API.Gravity
{
    /// <summary>
    /// GravityAPI is a class for gravity-related functionalities.
    /// </summary>
    public class GravityAPI
    {
        public static void ChangeGravity(Vector3 newGravity)
        {
            Physics.gravity = newGravity;
            Debug.Log($"Gravity changed to: {newGravity}");
        }

        public static void ChangeXGravity(float newGravity)
        {
            Vector3 orginal = Physics.gravity;
            Physics.gravity = new Vector3(newGravity, orginal.y, orginal.z);
            Debug.Log($"Gravity X changed to: {newGravity}");
        }

        public static void ChangeYGravity(float newGravity)
        {
            Vector3 orginal = Physics.gravity;
            Physics.gravity = new Vector3(orginal.x, newGravity, orginal.z);
            Debug.Log($"Gravity Y changed to: {newGravity}");
        }

        public static void ChangeZGravity(float newGravity)
        {
            Vector3 orginal = Physics.gravity;
            Physics.gravity = new Vector3(orginal.x, orginal.y, newGravity);
            Debug.Log($"Gravity Z changed to: {newGravity}");
        }

        public static Vector3 GetGravity()
        {
            return Physics.gravity;
        }

        public static float GetXGravity()
        {
            return Physics.gravity.x;
        }
        public static float GetYGravity()
        {
            return Physics.gravity.y;
        }
        public static float GetZGravity()
        {
            return Physics.gravity.z;
        }
    }
}
