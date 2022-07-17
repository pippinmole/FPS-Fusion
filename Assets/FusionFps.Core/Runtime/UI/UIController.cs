using Fusion;
using UnityEngine;

public class UIController : NetworkBehaviour {

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        // if ( _ui != null ) {
        //     Destroy(_ui.gameObject);
        // }
    }
}