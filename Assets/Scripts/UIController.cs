using Fusion;
using StarterAssets;
using UnityEngine;

public class UIController : NetworkBehaviour {

    [SerializeField] private IngameUI _uiPrefab;

    private IngameUI _ui;
    private FirstPersonController _player;

    private void Awake() {
        _player = GetComponent<FirstPersonController>();
    }

    public override void Spawned() {
        base.Spawned();

        if ( Object.HasInputAuthority ) {
            _ui = Instantiate(_uiPrefab);
            _ui.Setup(_player);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        if ( _ui != null ) {
            Destroy(_ui.gameObject);
        }
    }
}