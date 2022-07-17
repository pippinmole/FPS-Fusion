using System;
using Fusion;
using FusionFps.Core;
using UnityEngine;

public class ServerLobbyPlayer : LobbyPlayer {
    
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        base.OnPlayerJoined(runner, player);

        Debug.Log("Server player has joined");
        
        // switch ( MatchManager.Instance.GameState ) {
        //     case EGameState.GameInProgress:
        //         // Spawn them as a spectator
        //         throw new NotImplementedException();
        //     case EGameState.WaitingForPlayers: {
        //         if ( Runner.IsServer ) {
        //             // Spawn them as a player
        //             var spawnManager = FindObjectOfType<PlayerSpawnManager>();
        //             Spawn(spawnManager, player);
        //         }
        //
        //         break;
        //     }
        //     case EGameState.None:
        //         throw new NotImplementedException();
        //     case EGameState.LobbyConnected:
        //         throw new NotImplementedException();
        //     default:
        //         throw new ArgumentOutOfRangeException();
        // }
    }

    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        base.OnPlayerLeft(runner, player);

        if ( Runner.IsServer ) {
            var controller = GetPlayer(player);
            if ( controller != null ) {
                runner.Despawn(controller.Object);
            }
        }
    }

    // private static void SpawnAllPlayers(PlayerSpawnManager spawnManager) {
    //     foreach ( var player in Players ) {
    //         Debug.Log($"[Match Manager] Spawning {player}");
    //         player.Spawn(spawnManager, player.Object.InputAuthority);
    //     }
    // }
}
