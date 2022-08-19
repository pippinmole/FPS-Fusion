using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class TeamColor : NetworkBehaviour {
    
    [Serializable]
    private struct TeamColorData {
        public ETeam Team;
        public Material Material;
    }
    
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private List<TeamColorData> _materials = new();

    public override void Spawned() {
        base.Spawned();

        var lobbyPlayer = LobbyPlayer.GetPlayer(Object.InputAuthority);
        var team = lobbyPlayer.Team;
        var data = _materials.First(x => x.Team == team);

        _renderer.material = data.Material;
    }
}