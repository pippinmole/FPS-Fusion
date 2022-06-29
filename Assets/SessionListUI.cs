using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SessionListUI : MonoBehaviour {

    [SerializeField] private SessionListObject _sessionItemPrefab;
    [SerializeField] private Transform _sessionListParent;
    [SerializeField] private GameObject _loadingCircle;
    [SerializeField] private GameObject _noSessionsFound;
    
    private void Awake() {
        _loadingCircle.SetActive(true);
        _noSessionsFound.SetActive(false);
        
        SessionManager.SessionListUpdated += UpdateSessionList;
    }

    private void OnDestroy() {
        SessionManager.SessionListUpdated -= UpdateSessionList;
    }

    private void UpdateSessionList(List<SessionInfo> sessions) {
        _loadingCircle.SetActive(false);
        _noSessionsFound.SetActive(sessions.Count <= 0);
        
        Debug.Log($"SessionListUpdated with length {sessions.Count}");
        
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
