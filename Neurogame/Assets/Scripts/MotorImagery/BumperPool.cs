using System.Collections.Generic;
using UnityEngine;

namespace MotorImagery
{
    /// <summary>
    /// Object pool for bumpers to reduce instantiation/destruction overhead.
    /// Discovers and reuses existing bumpers in the scene.
    /// </summary>
    public class BumperPool : Singleton<BumperPool>
    {
        [Header("Pool Settings")]
        [Min(1)]
        public int maxPoolSize = 50;

        [Header("Debug")]
        public bool poolDebugLogging = false;

        private Queue<Bumper> availableBumpers = new Queue<Bumper>();
        private HashSet<Bumper> activeBumpers = new HashSet<Bumper>();
        private bool initialized = false;

        public void Initialize()
        {
            // Guard: only initialize once
            if (initialized) return;
            initialized = true;

            if (poolDebugLogging)
                Debug.Log("=== BumperPool Initialize Starting ===");

            // Search for bumpers including inactive ones
            Bumper[] allBumpers = FindObjectsByType<Bumper>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            if (poolDebugLogging)
                Debug.Log("FindObjectsByType<Bumper>(FindObjectsInactive.Include) found: " + allBumpers.Length + " bumpers");

            int discoveredCount = 0;
            int activeCount = 0;
            int inactiveCount = 0;

            foreach (var bumper in allBumpers)
            {
                if (poolDebugLogging)
                    Debug.Log("Processing: " + bumper.gameObject.name + " - Active: " + bumper.gameObject.activeSelf + ", ActiveInHierarchy: " + bumper.gameObject.activeInHierarchy);

                // Track which are already active (placed in scene as active)
                if (bumper.gameObject.activeSelf)
                {
                    activeBumpers.Add(bumper);
                    activeCount++;
                    
                    if (poolDebugLogging)
                        Debug.Log("  -> Added to active: " + bumper.gameObject.name);
                }
                else
                {
                    // Ensure it's inactive and add to available pool
                    bumper.gameObject.SetActive(false);
                    availableBumpers.Enqueue(bumper);
                    inactiveCount++;
                    
                    if (poolDebugLogging)
                        Debug.Log("  -> Added to pool: " + bumper.gameObject.name);
                }

                discoveredCount++;
            }

            if (poolDebugLogging)
            {
                Debug.Log("\n=== BumperPool Initialization Complete ===");
                Debug.Log("Discovered: " + discoveredCount + " bumpers");
                Debug.Log("Active: " + activeCount);
                Debug.Log("Available (Pooled): " + inactiveCount);
                Debug.Log("Max Pool Size: " + maxPoolSize);
                Debug.Log("Available Queue Count: " + availableBumpers.Count);
                
                Debug.Log("\n=== Detailed Bumper List ===");
                Bumper[] finalCheck = FindObjectsByType<Bumper>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var b in finalCheck)
                {
                    string parentName = b.transform.parent != null ? b.transform.parent.name : "none";
                    Debug.Log("  " + b.gameObject.name + ": activeSelf=" + b.gameObject.activeSelf + ", activeInHierarchy=" + b.gameObject.activeInHierarchy + ", parent=" + parentName);
                }
                Debug.Log("=== End Bumper List ===\n");
            }
        }

        /// <summary>
        /// Request a bumper from the pool.
        /// </summary>
        public Bumper GetBumper(Vector3 position, Quaternion rotation, Transform parent)
        {
            Bumper bumper;

            if (availableBumpers.Count > 0)
            {
                bumper = availableBumpers.Dequeue();
                bumper.gameObject.SetActive(true);
            }
            else
            {
                if (poolDebugLogging)
                    Debug.LogWarning("BumperPool: No available bumpers in pool. Ensure enough bumpers are placed in scene.");
                
                return null;
            }

            // Configure the bumper
            bumper.transform.SetParent(parent);
            bumper.transform.SetPositionAndRotation(position, rotation);
            bumper.ResetBumper();

            activeBumpers.Add(bumper);

            if (poolDebugLogging)
                Debug.Log("BumperPool: Got bumper. Active: " + activeBumpers.Count + ", Available: " + availableBumpers.Count);

            return bumper;
        }

        /// <summary>
        /// Return a bumper to the pool.
        /// </summary>
        public void ReturnBumper(Bumper bumper)
        {
            if (bumper == null) return;

            activeBumpers.Remove(bumper);
            bumper.gameObject.SetActive(false);
            availableBumpers.Enqueue(bumper);

            if (poolDebugLogging)
                Debug.Log("BumperPool: Returned bumper. Active: " + activeBumpers.Count + ", Available: " + availableBumpers.Count);
        }

        public int GetActiveCount() => activeBumpers.Count;
        public int GetAvailableCount() => availableBumpers.Count;
    }
}