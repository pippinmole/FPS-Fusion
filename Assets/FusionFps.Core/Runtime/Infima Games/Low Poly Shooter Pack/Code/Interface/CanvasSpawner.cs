// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack.Interface {
    public class CanvasSpawner : MonoBehaviour {
        [Header("Settings")]
        [Tooltip("Canvas prefab spawned at start. Displays the player's user interface.")]
        [SerializeField]
        private GameObject canvasPrefab;

        [Tooltip(
            "Quality settings menu prefab spawned at start. Used for switching between different quality settings in-game.")]
        [SerializeField]
        private GameObject qualitySettingsPrefab;

        private void Awake() {
            if ( canvasPrefab != null ) Instantiate(canvasPrefab);
            if ( qualitySettingsPrefab != null ) Instantiate(qualitySettingsPrefab);
        }
    }
}