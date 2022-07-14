using System;
using Fusion;
using FusionFps.Core;
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

    private LobbyPlayer _player;
    
    [SerializeField] private Image _image;

    public void Set(int number, PlayerRef player) {
        // _player = player;
        
        SetColor(number);
    }

    private void Update() {
        if ( !MatchManager.Instance.IsRunning )
            return;
        
        var controller = _player.Controller;
        
        _nameText.SetText(_player.ToString());

        if ( controller == null || controller.Object == null || !controller.Object.IsValid ) {
            _numberText.SetText("");
            _killCountText.SetText("0");
            _deathCountText.SetText("0");
            _killDeathRatioText.SetText("0");
        } else {
            _numberText.SetText("");
            _killCountText.SetText(controller.Kills.ToString());
            _deathCountText.SetText(controller.Deaths.ToString());

            var kd = controller.KillDeathRatio;
            
            _killDeathRatioText.SetText($"{kd:F2}");
        }
    }

    private void SetColor(int number) {
        var isEqual = number % 2 == 0;

        _image.color = isEqual ? _secondColor : _firstColor;
    }
}
