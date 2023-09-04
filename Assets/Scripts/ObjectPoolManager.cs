using System.Collections;
using System.Collections.Generic;
using Unity.BossRoom.Infrastructure;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    private void Start()
    {

        NetworkManager.Singleton.OnServerStarted += SpawnFoodStart;
    }

     void SpawnFoodStart()
    {

        NetworkManager.Singleton.OnServerStarted -= SpawnFoodStart;
       // NetworkObjectPool.Singleton.InitializePool();
        for(int i=0; i<30;++i)
        {
            SpawnFood();
        }
        StartCoroutine(SpawnFoodOverTime()); 
    }
    void SpawnFood()
    {
        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(prefab, GetRandomPositionOnMap(), Quaternion.identity);
        obj.GetComponent<Food>().prefab = prefab;
        if(!obj.IsSpawned)   obj.Spawn(true);
    }
    Vector3 GetRandomPositionOnMap()
    {
        return new Vector3(Random.Range(-9, 9f), Random.Range(-5, 5f), 0f);
    }
    IEnumerator SpawnFoodOverTime()
    {
        while(NetworkManager.Singleton.ConnectedClients.Count>0)
        {
            yield return new WaitForSeconds(2f);
            SpawnFood();
        }
    }
}
