using UnityEngine;

public class StartScreenMenu : MonoBehaviour {

    [SerializeField] private GameObject _serverList;
    
    public void CreateGame() {
        Debug.Log("Creating Game");
        // GameManager.Instance.StartHost();
    }

    public void OpenServerList() {
        _serverList.SetActive(true);
    }
}
