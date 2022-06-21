using System.Globalization;
using Fusion;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatEntry : MonoBehaviour {

    [SerializeField] private TMP_Text _numberText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _killCountText;
    [SerializeField] private TMP_Text _deathCountText;
    [SerializeField] private TMP_Text _killDeathRatioText;

    [SerializeField] private Color _firstColor = new(34, 45, 57, 255);
    [SerializeField] private Color _secondColor = new(47, 62, 78, 255);

    private static NetworkDictionary<PlayerRef, FirstPersonController> Players => LobbyManager.Instance.Players;
    private FirstPersonController PlayerController => Players[_player];
    
    private PlayerRef _player;
    private Image _image;

    private void Awake() {
        _image = GetComponent<Image>();
    }

    public void Set(int number, PlayerRef player) {
        _player = player;
        
        SetColor(number);
    }

    private void Update() {
        var player = PlayerController;
        
        _nameText.SetText(_player.ToString());

        if ( player == null ) {
            _numberText.SetText("");
            _killCountText.SetText("0");
            _deathCountText.SetText("0");
            _killDeathRatioText.SetText("0");
        } else {
            _numberText.SetText("");
            _killCountText.SetText(player.Kills.ToString());
            _deathCountText.SetText(player.Deaths.ToString());

            var kd = player.KillDeathRatio;
            
            _killDeathRatioText.SetText($"{kd:F2}");
        }
    }

    private void SetColor(int number) {
        var isEqual = number % 2 == 0;
        
        _image.color = isEqual ? _secondColor : _firstColor;
    }
}
