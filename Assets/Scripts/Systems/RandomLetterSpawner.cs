using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using NaughtyAttributes;
using Vector3 = System.Numerics.Vector3;

public class RandomLetterSpawner : MonoBehaviour
{
    public GameObject PickUpItemPrefb;
    public GameObject[] Spawnies;
    public int currentSpawnCount, maxSpawnCount;
    private void Start()
    {
        TurnOffMeshes();
        SpawnStarters();
    }

    private void TurnOffMeshes()
    {
        for (int i = 0; i < Spawnies.Length; i++)
        {
            Spawnies[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    void SpawnStarters()
    {
        for (int i = 0; i < maxSpawnCount; i++)
        {
            SpawnOneInstance();
        }
    }

    [Button("Add Letter")]
    public void SpawnOneInstance()
    {
        if (currentSpawnCount < Spawnies.Length)
        {
            GameObject Letter = GameObject.Instantiate(PickUpItemPrefb, Spawnies[currentSpawnCount].transform);
            Letter.transform.localPosition = UnityEngine.Vector3.zero;
            Letter.transform.localRotation = quaternion.identity;
            currentSpawnCount++;
        }   
    }
}

