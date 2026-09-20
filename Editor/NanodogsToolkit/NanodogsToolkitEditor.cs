using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Net.Security;
using UnityEditor.SearchService;
using UnityEditor.SceneManagement;

namespace Nanodogs.Toolkit
{
    public class NanodogsToolkitToolsEditor : MonoBehaviour
    {
        #region Tools
        #region Add GameObject

        [MenuItem("Nanodogs/Tools/Add GameObject/Empty GameObject", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject go = new GameObject("GameObject");
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("Nanodogs/Tools/Add GameObject/Cube")]
        static void Cube()
        {
            GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        [MenuItem("Nanodogs/Tools/Add GameObject/Sphere")]
        static void Sphere()
        {
            GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        [MenuItem("Nanodogs/Tools/Add GameObject/Capsule")]
        static void Capsule()
        {
            GameObject.CreatePrimitive(PrimitiveType.Capsule);
        }

        [MenuItem("Nanodogs/Tools/Add GameObject/Cylinder")]
        static void Cylinder()
        {
            GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        }

        [MenuItem("Nanodogs/Tools/Add GameObject/Plane")]
        static void Plane()
        {
            GameObject.CreatePrimitive(PrimitiveType.Plane);
        }
        [MenuItem("Nanodogs/Tools/Add GameObject/Sprites/Square")]
        static void Square()
        {
            GameObject Square = new GameObject("Square");
            Square.AddComponent<SpriteRenderer>();
            Debug.LogWarning("You must change the Sprite field to a square!");
        }

        [MenuItem("Nanodogs/Tools/Add GameObject/Sprites/Circle")]
        static void Circle()
        {
            GameObject Circle = new GameObject("Circle");
            Circle.AddComponent<SpriteRenderer>();
            Debug.LogWarning("You must change the Sprite field to a circle!");
        }

        [MenuItem("Nanodogs/Tools/Add GameObject/Quad")]
        static void Quad()
        {
            GameObject.CreatePrimitive(PrimitiveType.Quad);
        }

        [MenuItem("Nanodogs/Tools/Add GameObject/Terrain")]
        static void Terrain()
        {
            GameObject Terrain = new GameObject("Terrain");
            Terrain.AddComponent<Terrain>();
            Debug.LogWarning("You must create new terrain data and assign it to the Terrain gameobject!");
        }
        #endregion

        [MenuItem("Nanodogs/Tools/Properties/Double Mass")]
        static void DoubleMass(MenuCommand command)
        {
            Rigidbody body = (Rigidbody)command.context;
            body.mass = body.mass * 2;
            Debug.Log("Doubled Rigidbody's Mass to " + body.mass + " from Context Menu.");
        }

        [MenuItem("Nanodogs/Tools/Properties/Add Rigidbody to selected GameObject")]
        static void AddRBtoGO()
        {
            Selection.activeTransform.gameObject.AddComponent<Rigidbody>();
        }

        [MenuItem("Nanodogs/Tools/Properties/Disable Selected")]
        static void DisableSelectedGO()
        {
            Selection.activeTransform.gameObject.SetActive(false);
        }
        [MenuItem("Nanodogs/Tools/Properties/Enable Selected")]
        static void EnableSelectedGO()
        {
            Selection.activeTransform.gameObject.SetActive(true);
        }

        #region Materials

        [MenuItem("Nanodogs/Tools/Material/Create Material (Built-in)")]
        static void CreateNewMat()
        {
            Material mat = new Material(Shader.Find("Standard/Lit"));
        }

        [MenuItem("Nanodogs/Tools/Material/Create Material (URP)")]
        static void CreateNewMatURP()
        {
            Material mat = new Material(Shader.Find("URP/Lit"));
        }

        [MenuItem("Nanodogs/Tools/Material/Create Material (HDRP)")]
        static void CreateNewMatHDRP()
        {
            Material mat = new Material(Shader.Find("HDRP/Lit"));
        }

        #endregion

        #region Lights

        [MenuItem("Nanodogs/Tools/Lighting/Add Light/Create Light (Spot)")]
        static void CreateLightspot(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject lightGOs = new GameObject("Light");
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(lightGOs, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(lightGOs, "Create " + lightGOs.name);
            Selection.activeObject = lightGOs;
            lightGOs.AddComponent<Light>();
            lightGOs.GetComponent<Light>().type = LightType.Spot;
        }
        [MenuItem("Nanodogs/Tools/Lighting/Add Light/Create Light (Area)")]
        static void CreateLight(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject lightGOa = new GameObject("Light");
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(lightGOa, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(lightGOa, "Create " + lightGOa.name);
            Selection.activeObject = lightGOa;
            lightGOa.AddComponent<Light>();
            lightGOa.GetComponent<Light>().type = LightType.Rectangle;
        }

        [MenuItem("Nanodogs/Tools/Lighting/Add Light/Create Light (Directional)")]
        static void CreateLightDirectional(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject lightGOd = new GameObject("Light");
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(lightGOd, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(lightGOd, "Create " + lightGOd.name);
            Selection.activeObject = lightGOd;
            lightGOd.AddComponent<Light>();
            lightGOd.GetComponent<Light>().type = LightType.Directional;
        }

        [MenuItem("Nanodogs/Tools/Lighting/Add Light/Create Light (Point)")]
        static void CreateLightPoint(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject lightGOp = new GameObject("Light");
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(lightGOp, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(lightGOp, "Create " + lightGOp.name);
            Selection.activeObject = lightGOp;
            lightGOp.AddComponent<Light>();
            lightGOp.GetComponent<Light>().type = LightType.Point;
        }

        [MenuItem("Nanodogs/Tools/Lighting/Add Light/Create Light (Rect)")]
        static void CreateLightRect(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject lightGOr = new GameObject("Light");
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(lightGOr, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(lightGOr, "Create " + lightGOr.name);
            Selection.activeObject = lightGOr;
            lightGOr.AddComponent<Light>();
            lightGOr.GetComponent<Light>().type = LightType.Rectangle;
        }

        #endregion

        #endregion
    }
}
