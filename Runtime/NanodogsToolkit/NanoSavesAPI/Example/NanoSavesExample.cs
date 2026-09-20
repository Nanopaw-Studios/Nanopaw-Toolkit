using UnityEngine;

namespace Nanodogs.API.Nanosaves.Example
{
    /// <summary>
    /// Example MonoBehaviour demonstrating how to use the NanoSaves API.
    /// </summary>
    public class NanoSavesExample : MonoBehaviour
    {
        public void Start()
        {
            // Example usage of NanoSaves API
            string saveKey = "PlayerData";
            string playerData = "PlayerScore:1000";
            // Save data
            NanoSaves.SaveData(saveKey, playerData);
            Debug.Log("Data saved.");
            // Load data
            string loadedData = NanoSaves.LoadData(saveKey);
            Debug.Log("Loaded Data: " + loadedData);
        }
    }
}
