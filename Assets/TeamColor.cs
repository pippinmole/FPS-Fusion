using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class TeamColor : NetworkBehaviour {
    [SerializeField] private MeshRenderer _renderer;

    private static readonly Dictionary<ETeam, Color> TeamColors = new() {
        { ETeam.Blue, new Color(0, 15, 154) },
        { ETeam.Red, new Color(169, 0, 8) }
    };

    public override void Spawned() {
        base.Spawned();

        var lobbyPlayer = LobbyPlayer.GetPlayer(Object.InputAuthority);
        var team = lobbyPlayer.Team;
        var color = TeamColors[team];

        _renderer.material.color = color;
    }
}