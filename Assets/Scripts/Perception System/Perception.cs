using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class should be used to collect and store information things that are sensed
/// </summary>
public class Perception : MonoBehaviour
{
    /// <summary>
    /// Data from this map is never be removed. This gives the AI a "memory" of things it's sensed
    /// Up to you to decide how far into the past the AI remembers things
    /// </summary>
    public Dictionary<GameObject, MemoryRecord> memoryMap = new Dictionary<GameObject, MemoryRecord>();

    // For debugging
    public GameObject[] sensedObjects;
    public MemoryRecord[] sensedRecord;

    MemoryRecord record;

    /// <summary>
    /// Clears all the current FOVs
    /// </summary>
    public void ClearFoV()
    {
        foreach (KeyValuePair<GameObject, MemoryRecord> memory in memoryMap)
        {
            memory.Value.withinFoV = false;
        }
    }

    public void AddMemory(GameObject target)
    {
        // Create a new memory record
        record = new MemoryRecord(DateTime.Now, target.transform.position, true);

        // Check if we already have a previous memory record for this target
        if (memoryMap.ContainsKey(target))
        {
            // Overwrite the previous record instead of adding a new one
            memoryMap[target] = record;
        }
        else
        {
            // Otherwise add the new record
            memoryMap.Add(target, record);
        }
    }

    public void RemoveMemory(GameObject target)
    {
        if (memoryMap.ContainsKey(target))
        {
            memoryMap.Remove(target);
        }
    }

    /// <summary>
    /// Can remove this whole update - just for debugging
    /// </summary>
    private void Update()
    {
        // Just expose the values to inspector so we can see if it's working
        sensedObjects = new GameObject[memoryMap.Keys.Count];
        sensedRecord = new MemoryRecord[memoryMap.Values.Count];
        memoryMap.Keys.CopyTo(sensedObjects, 0);
        memoryMap.Values.CopyTo(sensedRecord, 0);

        for (int i = 0; i < sensedRecord.Length; i++)
        {
            if (sensedRecord[i].timeLimitReached)
            {
                RemoveMemory(sensedObjects[i]);
            }
        }
    }
}

[Serializable]
public class MemoryRecord
{
    /// <summary>
    /// The time the target was last sensed
    /// </summary>
    [SerializeField]
    public DateTime timeLastSensed;

    /// <summary>
    /// The position the target was last sensed
    /// </summary>
    [SerializeField]
    public Vector3 lastSensedPosition;

    /// <summary>
    /// Whether the target is currently within the FoV
    /// </summary>
    [SerializeField]
    public bool withinFoV;

    /// <summary>
    /// To help with debugging, we convert DateTime to string from so can serialize in inspector
    /// </summary>
    [SerializeField]
    public string timeLastSensedStr;

    /// <summary>
    /// If the time limit has been reached, the target gets removed from memory
    /// </summary>
    [SerializeField]
    public bool timeLimitReached;

    public MemoryRecord()
    {
        timeLastSensed = DateTime.MinValue;
        lastSensedPosition = Vector3.zero;
        withinFoV = false;

        timeLimitReached = false;
    }

    public MemoryRecord(DateTime theTime, Vector3 thePos, bool theFoV)
    {
        timeLastSensed = theTime;
        lastSensedPosition = thePos;
        withinFoV = theFoV;
        timeLastSensedStr = theTime.ToLongTimeString();

        timeLimitReached = false;
    }
}
