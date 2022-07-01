using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using FusionFps.Core;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateGamePanel : MonoBehaviour {

    [SerializeField] private MapList _mapList;
    [SerializeField] private TMP_InputField _sessionNameInput;
    [SerializeField] private CustomDropdown _mapDropdown;
    [SerializeField] private Button _startSessionButton;
    [SerializeField] private ModalWindowManager _sanitiseModal;
    [SerializeField] private GameObject _createGameLoadingCircle;

    private const int MinimumSessionNameLength = 3;
    private const int MaximumSessionNameLength = 25;

    private ISessionManager _sessionManager;
    
    private void Awake() {
        _sessionManager = SingletonProvider.Get<ISessionManager>();
        
        SetupMapDropdown();
        
        _createGameLoadingCircle.SetActive(false);
        _startSessionButton.onClick.AddListener(async () => await StartSession());
    }

    private void SetupMapDropdown() {
        _mapDropdown.dropdownItems = new List<CustomDropdown.Item>();

        foreach ( var mapIndex in _mapList.GetAllMaps() ) {
            var sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(mapIndex));
            
            _mapDropdown.CreateNewItem(sceneName, null);
        }
    }
    
    private async Task StartSession() {
        var (isSterile, description) = SanitiseSessionData();
        if ( !isSterile ) {
            // _sanitiseModal.windowDescription.SetText(description);
            _sanitiseModal.descriptionText = description;
            _sanitiseModal.UpdateUI();
            _sanitiseModal.OpenWindow();
            return;
        }
        
        var sessionName = string.IsNullOrEmpty(_sessionNameInput.text)
            ? "session_" + Random.Range(0, 99999)
            : _sessionNameInput.text;
        var mapIndex = _mapList.GetMap(_mapDropdown.index);

        var sessionProperties = new Dictionary<string, SessionProperty> {
            { "mapBuildIndex", mapIndex }
        };

        Debug.Log($"Creating game with scene index: {mapIndex}");

        _createGameLoadingCircle.SetActive(true);
        
        var result = await _sessionManager.CreateSession(sessionName, sessionProperties);
        
        _createGameLoadingCircle.SetActive(false);
        
        if ( !result.Ok ) {
            return;
        }

        // Close this menu and open the lobby list
        gameObject.SetActive(false);
    }

    private (bool, string) SanitiseSessionData() {
        // session name sanitization
        var sessionName = _sessionNameInput.text;
        if ( string.IsNullOrEmpty(sessionName) ) {
            return (false, "The provided session name is empty!");
        } else if ( sessionName.Length < MinimumSessionNameLength || sessionName.Length > MaximumSessionNameLength ) {
            return (false, $"The provided session name has to be longer than {MinimumSessionNameLength} characters and shorter than {MaximumSessionNameLength} characters");
        }

        return (true, string.Empty);
    }
}
