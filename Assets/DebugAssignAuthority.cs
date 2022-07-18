using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class DebugAssignAuthority : NetworkBehaviour {
    [SerializeField] private bool _inputAuth = true;
    
    public override void Spawned() {
        base.Spawned();

        if ( _inputAuth ) {
            Object.AssignInputAuthority(Runner.LocalPlayer);
        }
    }
}
