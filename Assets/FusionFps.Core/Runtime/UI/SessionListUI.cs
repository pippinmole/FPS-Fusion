using System.Collections.Generic;
using Fusion;
using FusionFps.Core;
using UnityEngine;

public class SessionListUI : MonoBehaviour {

    [SerializeField] private SessionListObject _sessionItemPrefab;
    [SerializeField] private Transform _sessionListParent;
    [SerializeField] private GameObject _loadingCircle;
    [SerializeField] private GameObject _noSessionsFound;

    private ISessionManager _sessionManager;
    
    private void Awake() {
        _sessionManager = ServiceProvider.Get<ISessionManager>();
        
        // _loadingCircle.SetActive(true);
        _noSessionsFound.SetActive(false);
        
        _sessionManager.SessionListUpdated += UpdateSessionList;
        _sessionManager.ConnectionStatusChanged += OnConnectionStatusChanged;
    }
    
    private void OnDestroy() {
        _sessionManager.SessionListUpdated -= UpdateSessionList;
        _sessionManager.ConnectionStatusChanged -= OnConnectionStatusChanged;
    }

    private void OnConnectionStatusChanged(NetworkRunner runner, ConnectionStatus connectionStatus) {
        _loadingCircle.SetActive(connectionStatus is ConnectionStatus.Connecting);
        // _loadingCircle.SetActive(false);
    }

    private void UpdateSessionList(List<SessionInfo> sessions) {
        // _loadingCircle.SetActive(false);
        _noSessionsFound.SetActive(sessions.Count <= 0);
        
        ClearParent(_sessionListParent);

        foreach ( var session in sessions ) {
            var obj = Instantiate(_sessionItemPrefab, _sessionListParent);
            obj.Set(session);
        }
    }
    
    private void ClearParent<T>(T parent) where T : Component {
        foreach (var item in parent.GetComponentsInChildren<T>()) {
            if ( parent == item ) continue;
            
            Destroy(item.gameObject);
        }
    }
}
