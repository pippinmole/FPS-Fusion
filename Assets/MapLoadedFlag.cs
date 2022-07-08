using Fusion;
using FusionFps.Core;

/// <summary>
/// The purpose of this flag script is to simply notify the match manager that the game scene has loaded
/// </summary>
public class MapLoadedFlag : NetworkBehaviour {

    private IMatchManager _matchManager;

    private void Awake() {
        _matchManager = ServiceProvider.Get<IMatchManager>();
    }

    public override void Spawned() {
        base.Spawned();

        _matchManager.OnMatchLoaded();
    }
}
