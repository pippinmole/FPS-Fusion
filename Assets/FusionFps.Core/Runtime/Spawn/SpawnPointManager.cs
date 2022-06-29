using System.Collections.Generic;
using Fusion;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using Fusion.Editor;
#endif

public class SpawnPointManager<T> : MonoBehaviour where T : Component, ISpawnPoint {

    public enum SpawnSequence {
        PlayerId,
        RoundRobin,
        Random
    }

    /// <summary>
    /// How spawn points will be selected from the <see cref="SpawnPoints"/> collection.
    /// </summary>
    [InlineHelp] public SpawnSequence _sequence;

    /// <summary>
    /// LayerMask for which physics layers should be used for blocked spawn point checks.
    /// </summary>
    [InlineHelp] public LayerMask _blockingLayers;

    /// <summary>
    /// The search radius used for detecting if a spawn point is blocked by an object.
    /// </summary>
    [InlineHelp] public float _blockedCheckRadius = 2f;

    /// <summary>
    /// Serialized collection of all <see cref="ISpawnPointPrototype"/> of the type T found in the same scene as this component.
    /// </summary>
    [System.NonSerialized] internal List<Component> SpawnPoints = new List<Component>();

    [System.NonSerialized] public int LastSpawnIndex = -1;

    private NetworkRNG _rng;

    private void Awake() {
        _rng = new NetworkRNG(0);
    }

#if UNITY_EDITOR
    [BehaviourAction]
    protected void DrawFoundSpawnPointCount() {
        if ( Application.isPlaying ) return;

        GUILayout.BeginVertical(FusionGUIStyles.GroupBoxType.Info.GetStyle());
        GUILayout.Space(4);
        if ( GUI.Button(EditorGUILayout.GetControlRect(), "Find Spawn Points") ) {
            SpawnPoints.Clear();
            var found = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .FindObjectsOfTypeInOrder<T, Component>();
            SpawnPoints.AddRange(found);
        }

        GUILayout.Space(4);

        EditorGUI.BeginDisabledGroup(true);
        foreach ( var point in SpawnPoints ) {
            EditorGUILayout.ObjectField(point.name, point, typeof(T), true);
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField($"{typeof(T).Name}(s): {SpawnPoints.Count}");
        GUILayout.EndVertical();
    }
#endif
    
    /// <summary>
    /// Find all <see cref="ISpawnPointPrototype"/> instances in the same scene as this spawner. 
    /// This should only be done at development time if using the Photon relay for any spawn logic.
    /// </summary>
    private void CollectSpawnPoints(NetworkRunner runner) {
        SpawnPoints.Clear();
        SpawnPoints.AddRange(runner.SimulationUnityScene.FindObjectsOfTypeInOrder<T, Component>());
    }

    /// <summary>
    /// Select the next spawn point using the defined <see cref="_sequence"/>. Override this method to expand on this, such as detecting if a spawn point is blocked.
    /// </summary>
    public Transform GetNextSpawnPoint(NetworkRunner runner, PlayerRef player, bool skipIfBlocked = true) {

        CollectSpawnPoints(runner);

        var spawnCount = SpawnPoints.Count;

        if ( SpawnPoints == null || spawnCount == 0 )
            return null;

        Component next;
        int nextIndex;
        switch ( _sequence ) {
            case SpawnSequence.PlayerId:
                nextIndex = player % spawnCount;
                next = SpawnPoints[nextIndex];
                break;
            case SpawnSequence.RoundRobin:
                nextIndex = (LastSpawnIndex + 1) % spawnCount;
                next = SpawnPoints[nextIndex];
                break;
            case SpawnSequence.Random:
            default:
                nextIndex = _rng.RangeInclusive(0, spawnCount);
                next = SpawnPoints[nextIndex];
                break;
        }

        // Handling for blocked spawn points. By default this never happens, as the IsBlocked test always returns true.
        if ( skipIfBlocked && _blockingLayers.value != 0 && IsBlocked(next) ) {
            var unblocked = GetNextUnblocked(nextIndex);
            if ( unblocked.Item1 > -1 ) {
                LastSpawnIndex = unblocked.Item1;
                return unblocked.Item2.transform;
            }

            // leave LastSpawnIndex the same since we haven't arrived at a new spawn point.
            next = unblocked.Item2;
        } else {
            LastSpawnIndex = nextIndex;
            return next.transform;
        }

        return AllSpawnPointsBlockedFallback();
    }

    /// <summary>
    /// Handling for if all spawn points are blocked.
    /// </summary>
    /// <returns></returns>
    public virtual Transform AllSpawnPointsBlockedFallback() {
        return transform;
    }

    /// <summary>
    /// Cycles through all remaining spawn points searching for unblocked. Will return null if all points return <see cref="IsBlocked(Transform)"/> == true.
    /// </summary>
    /// <param name="failedIndex">The index of the first tried SpawnPoints[] element, which was blocked.</param>
    /// <returns>(<see cref="SpawnPoints"/> index, <see cref="ISpawnPointPrototype"/>)</returns>
    private (int, Component) GetNextUnblocked(int failedIndex) {
        for ( int i = 1, cnt = SpawnPoints.Count; i < cnt; ++i ) {
            var sp = SpawnPoints[i % cnt];
            if ( !IsBlocked(sp) )
                return (i, sp);
        }

        return (-1, null);
    }

    private static Collider[] _blocked3D;

    private bool IsBlocked(Component spawnPoint) {
        var physics3d = spawnPoint.gameObject.scene.GetPhysicsScene();
        _blocked3D ??= new Collider[1];

        var blockedCount = physics3d.OverlapSphere(spawnPoint.transform.position, _blockedCheckRadius, _blocked3D,
            _blockingLayers.value, QueryTriggerInteraction.UseGlobal);
        if ( blockedCount > 0 )
            Debug.LogWarning(_blocked3D[0].name + " is blocking " + spawnPoint.name);

        return blockedCount > 0;
    }
}
