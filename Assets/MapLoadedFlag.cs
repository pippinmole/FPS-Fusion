using Fusion;
using FusionFps.Core;

/// <summary>
/// The purpose of this flag script is to simply notify the match manager that the game scene has loaded
/// </summary>
public class MapLoadedFlag : NetworkBehaviour {
    public override void Spawned() {
        base.Spawned();

        MatchManager.OnMatchLoaded();
    }
}
