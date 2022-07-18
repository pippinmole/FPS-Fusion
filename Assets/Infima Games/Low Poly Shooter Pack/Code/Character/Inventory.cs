// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    public class Inventory : InventoryBehaviour {
        private WeaponBehaviour[] _weapons;
        private WeaponBehaviour _equipped;
        private int _equippedIndex = -1;

        public override void Init(int equippedAtStart = 0) {
            // Beware that weapons need to be parented to the object this component is on!
            _weapons = GetComponentsInChildren<WeaponBehaviour>(true);

            // Disable all weapons. This makes it easier for us to only activate the one we need.
            foreach ( var weapon in _weapons )
                weapon.gameObject.SetActive(false);

            //Equip.
            Equip(equippedAtStart);
        }

        public override WeaponBehaviour Equip(int index) {
            if ( _weapons == null ) return _equipped;
            if ( index > _weapons.Length - 1 ) return _equipped;
            if ( _equippedIndex == index ) return _equipped;

            // Disable the currently equipped weapon, if we have one.
            if ( _equipped != null )
                _equipped.gameObject.SetActive(false);

            //Update index.
            _equippedIndex = index;
            //Update equipped.
            _equipped = _weapons[_equippedIndex];
            //Activate the newly-equipped weapon.
            _equipped.gameObject.SetActive(true);

            //Return.
            return _equipped;
        }

        public override int GetLastIndex() {
            //Get last index with wrap around.
            int newIndex = _equippedIndex - 1;
            if ( newIndex < 0 )
                newIndex = _weapons.Length - 1;

            //Return.
            return newIndex;
        }

        public override int GetNextIndex() {
            //Get next index with wrap around.
            int newIndex = _equippedIndex + 1;
            if ( newIndex > _weapons.Length - 1 )
                newIndex = 0;

            //Return.
            return newIndex;
        }

        public override WeaponBehaviour GetEquipped() => _equipped;
        public override int GetEquippedIndex() => _equippedIndex;
    }
}