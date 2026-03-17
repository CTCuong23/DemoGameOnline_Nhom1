using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class HostModeSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [Header("Player Prefab")]
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

    public void PlayerJoined(PlayerRef player)
    {
        if (!Runner.IsServer)
            return;

        var spawnPosition = new Vector3(Random.Range(-2f,2f),3f,Random.Range(-2f,2f));
        var networkObject = Runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

        _spawnedCharacters[player] = networkObject;
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!Runner.IsServer)
            return;
    
        if (!_spawnedCharacters.TryGetValue(player, out var networkObject))
            return;
    
        Runner.Despawn(networkObject);
        _spawnedCharacters.Remove(player);
    }
}
