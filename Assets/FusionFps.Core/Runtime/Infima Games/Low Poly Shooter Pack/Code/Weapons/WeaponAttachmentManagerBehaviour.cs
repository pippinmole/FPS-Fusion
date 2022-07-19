// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    public abstract class WeaponAttachmentManagerBehaviour : MonoBehaviour {
        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void LateUpdate() { }

        public abstract ScopeBehaviour GetEquippedScope();
        public abstract ScopeBehaviour GetEquippedScopeDefault();

        public abstract MagazineBehaviour GetEquippedMagazine();
        public abstract MuzzleBehaviour GetEquippedMuzzle();

        public abstract LaserBehaviour GetEquippedLaser();
        public abstract GripBehaviour GetEquippedGrip();
    }
}