﻿// Copyright 2021, Infima Games. All Rights Reserved.

using System;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Weapon Scope.
    /// </summary>
    public class Scope : ScopeBehaviour
    {
        #region FIELDS SERIALIZED
    
        [Header("Multipliers")]
        
        [Tooltip("Amount to multiply the mouse sensitivity by while aiming through this scope.")]
        [SerializeField]
        private float multiplierMouseSensitivity = 0.8f;
        
        [Tooltip("Value multiplied by the weapon's spread while aiming through this scope.")]
        [SerializeField]
        private float multiplierSpread = 0.1f;

        [Header("Interface")]

        [Tooltip("Interface Sprite.")]
        [SerializeField]
        private Sprite sprite;
        
        [Header("Sway")]
        
        [Tooltip("Weapon sway values while aiming this scope.")]
        [SerializeField]
        private Sway aimingSway;
        
        [Header("Aiming Offset")]
        
        [Tooltip("Weapon bone location offset while aiming.")]
        [SerializeField]
        private Vector3 offsetAimingLocation;
        
        [Tooltip("Weapon bone rotation offset while aiming.")]
        [SerializeField]
        private Vector3 offsetAimingRotation;
        
        [Header("Field Of View")]

        [Tooltip("Field Of View Multiplier Aim.")]
        [SerializeField]
        private float fieldOfViewMultiplierAim = 0.9f;
        
        [Tooltip("Field Of View Multiplier Aim Weapon.")]
        [SerializeField]
        private float fieldOfViewMultiplierAimWeapon = 0.7f;

        [Header("Materials")]

        [Tooltip("The index of the scope material that gets hidden when we don't aim.")]
        [SerializeField]
        private int materialIndex = 3;

        [Tooltip("Material to block the scope while not aiming through it.")]
        [SerializeField]
        private Material materialHidden;
        
        #endregion

        #region FIELDS

        /// <summary>
        /// Mesh Renderer.
        /// </summary>
        private MeshRenderer meshRenderer;
        /// <summary>
        /// Default scope material. We store it so we can re-apply it at any time, since it is
        /// usually changed at runtime.
        /// </summary>
        private Material materialDefault;

        #endregion

        #region UNITY

        private void Awake()
        {
            //Cache Scope Renderer.
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            
            //Make sure that the index can exist.
            if (!HasMaterialIndex())
                return;
            
            //Cache default material.
            materialDefault = meshRenderer.materials[materialIndex];
        }

        private void Start()
        {
            //Start at the default state.
            OnAimStop();
        }

        #endregion

        #region GETTERS

        public override float GetMultiplierMouseSensitivity() => multiplierMouseSensitivity;

        public override float GetMultiplierSpread() => multiplierSpread;
        public override Sway GetSwayAiming() => aimingSway;

        public override Vector3 GetOffsetAimingLocation() => offsetAimingLocation;
        public override Vector3 GetOffsetAimingRotation() => offsetAimingRotation;

        public override float GetFieldOfViewMultiplierAim() => fieldOfViewMultiplierAim;
        public override float GetFieldOfViewMultiplierAimWeapon() => fieldOfViewMultiplierAimWeapon;
        
        public override Sprite GetSprite() => sprite;

        /// <summary>
        /// Returns true if the Scope's Mesh Renderer could have this Material index.
        /// </summary>
        private bool HasMaterialIndex()
        {
            //Make sure that the index can exist.
            return materialIndex < meshRenderer.materials.Length && materialIndex >= 0;
        }

        #endregion

        #region METHODS

        public override void OnAim()
        {
            //Make sure that the index can exist.
            if (!HasMaterialIndex())
                return;

            //Get Materials.
            Material[] materials = meshRenderer.materials;
            //Restore to default material.
            materials[materialIndex] = materialDefault;
            //Update Materials.
            meshRenderer.materials = materials;
        }

        public override void OnAimStop()
        {
            //Make sure that the index can exist.
            if (!HasMaterialIndex())
                return;
            
            //Get Materials.
            Material[] materials = meshRenderer.materials;
            //Hide.
            materials[materialIndex] = materialHidden;
            //Update Materials.
            meshRenderer.materials = materials;
        }

        #endregion
    }
}