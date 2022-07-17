using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TeamSelectUI : MonoBehaviour {

    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _redTeamButton;
    [SerializeField] private Button _blueTeamButton;
    [SerializeField] private Button _randomTeamButton;
    [SerializeField] private Button _spectatorButton;

    private void Awake() {
        _redTeamButton.onClick.AddListener(() => SetTeam(ETeam.Red));
        _blueTeamButton.onClick.AddListener(() => SetTeam(ETeam.Blue));
        _spectatorButton.onClick.AddListener(() => SetTeam(ETeam.Spectator));
        _randomTeamButton.onClick.AddListener(SetRandomTeam);
    }

    public void OpenPanel() {
        _panel.SetActive(true);
    }

    public void ClosePanel() {
        _panel.SetActive(false);
    }
    
    private void SetRandomTeam() {
        var random = Random.Range(0, 999);
        SetTeam(random % 2 == 0 ? ETeam.Blue : ETeam.Red);
    }
    
    private void SetTeam(ETeam team) {
        LobbyPlayer.LocalPlayer.RPC_SetTeam(team);
        
        ClosePanel();
    }
}