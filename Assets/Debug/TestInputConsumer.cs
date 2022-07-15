using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class TestInputConsumer : NetworkBehaviour {
    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();

        if ( GetInput(out PlayerInput.NetworkInputData input) ) {
            Debug.Log($"IsShootPressed: {input.IsDown(PlayerInput.NetworkInputData.ButtonShoot)}");
        }
    }
}
