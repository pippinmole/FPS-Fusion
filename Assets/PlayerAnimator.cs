using System;
using System.Collections;
using Fusion;
using InfimaGames.LowPolyShooterPack;
using UnityEngine;

[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(MagazineBehaviour))]
[RequireComponent(typeof(WeaponBehaviour))]
public class PlayerAnimator : NetworkBehaviour, ICharacterAnimationCallbacks {
    
    private static readonly int HashConstraintAlphaLeft = Animator.StringToHash("Alpha IK Hand Left");
    private static readonly int HashConstraintAlphaRight = Animator.StringToHash("Alpha IK Hand Right");
    private static readonly int HashAimingAlpha = Animator.StringToHash("Aiming");
    private static readonly int HashBoltAction = Animator.StringToHash("Bolt Action");
    private static readonly int HashMovement = Animator.StringToHash("Movement");
    private static readonly int HashLeaning = Animator.StringToHash("Leaning");
    private static readonly int HashAimingSpeedMultiplier = Animator.StringToHash("Aiming Speed Multiplier");
    private static readonly int HashTurning = Animator.StringToHash("Turning");
    private static readonly int HashHorizontal = Animator.StringToHash("Horizontal");
    private static readonly int HashVertical = Animator.StringToHash("Vertical");
    private static readonly int HashPlayRateLocomotionForward = Animator.StringToHash("Play Rate Locomotion Forward");
    private static readonly int HashPlayRateLocomotionSideways = Animator.StringToHash("Play Rate Locomotion Sideways");

    private static readonly int HashPlayRateLocomotionBackwards =
        Animator.StringToHash("Play Rate Locomotion Backwards");

    private static readonly int HashAlphaActionOffset = Animator.StringToHash("Alpha Action Offset");
    
    [Networked] private int TotalGrenades { get; set; } = 10;
    
    [SerializeField] private CharacterKinematics _kinematics;
    [SerializeField] private int _weaponIndexEquippedAtStart;
    [SerializeField] private InventoryBehaviour _inventory;
    [SerializeField] private float _grenadeSpawnOffset = 1.0f;
    [SerializeField] private GameObject _grenadePrefab;
    [SerializeField] private GameObject _knife;
    [SerializeField] private Camera _cameraWorld;
    [SerializeField] private Camera _cameraDepth;
    [SerializeField] private float _dampTimeTurning = 0.4f;
    [SerializeField] private float _dampTimeLocomotion = 0.1f;
    [SerializeField] private float _dampTimeAiming = 0.3f;
    [SerializeField] private float _aimingSpeedMultiplier = 1.0f;
    [SerializeField] private Transform _boneWeapon;
    [SerializeField] private Animator _animator;
    [SerializeField] private bool _enableWeaponSway = true;
    [SerializeField] private float _weaponSwaySmoothValueInput = 8.0f;
    [SerializeField] private float _fieldOfView = 85f;
    [SerializeField] private float _fieldOfViewWeapon = 65f;
    [SerializeField] private AudioClip[] _audioClipsMelee;
    [SerializeField] private AudioClip[] _audioClipsGrenadeThrow;

    private bool _holstered;

    /// <summary>
    /// Overlay Layer Index. Useful for playing things like firing animations.
    /// </summary>
    private int _layerOverlay;

    /// <summary>
    /// Holster Layer Index. Used to play holster animations.
    /// </summary>
    private int _layerHolster;

    /// <summary>
    /// Actions Layer Index. Used to play actions like reloading.
    /// </summary>
    private int _layerActions;

    private PlayerController _controller;
    private WeaponBehaviour _equippedWeapon;
    private WeaponAttachmentManagerBehaviour _weaponAttachmentManager;

    private ScopeBehaviour _equippedWeaponScope;
    private MagazineBehaviour _equippedWeaponMagazine;
    
    [Networked] private bool Reloading { get; set; }
    [Networked] private bool Inspecting { get; set; }
    [Networked] private bool Aiming { get; set; }
    [Networked] private bool AimingLastFrame { get; set; }

    private bool _throwingGrenade;
    private bool _meleeing;
    private bool _crouching;
    private Vector3 _swayLocation;
    private Vector3 _swayRotation;
    private bool _holstering;
    private float _lastShotTime;

    /// <summary>
    /// Alpha Aiming Value. Zero to one value representing aiming. Zero if we're not aiming, and one if we are fully aiming.
    /// </summary>
    private float _aimingAlpha;

    /// <summary>
    /// Smoothed Look Axis Values. Used for Sway.
    /// </summary>
    private Vector2 _axisLookSmooth;

    /// <summary>
    /// Smoothed Movement Axis Values. Used for Sway.
    /// </summary>
    private Vector2 _axisMovementSmooth;

    /// <summary>
    /// True if the character is playing the bolt-action animation.
    /// </summary>
    private bool _bolting;

    private void Awake() {
        _controller = GetComponent<PlayerController>();

        _inventory.Init(_weaponIndexEquippedAtStart);

        RefreshWeaponSetup();
    }
    
    private void Start() {
        // Hide knife. We do this so we don't see a giant knife stabbing through the character's hands all the time!
        if ( _knife != null )
            _knife.SetActive(false);

        _layerHolster = _animator.GetLayerIndex("Layer Holster");
        _layerActions = _animator.GetLayerIndex("Layer Actions");
        _layerOverlay = _animator.GetLayerIndex("Layer Overlay");
    }

    private bool _state;
    
    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();

        if ( GetInput(out PlayerInput.NetworkInputData input) ) {
            var aiming = input.IsDown(PlayerInput.NetworkInputData.ButtonScope) && CanAim();
            var running = input.IsDown(PlayerInput.NetworkInputData.ButtonSprint) && CanRun();

            var axisLook = new Vector2((float) input.YawDelta, (float) input.PitchDelta);
            axisLook *= aiming ? _equippedWeaponScope.GetMultiplierMouseSensitivity() : 1.0f;

            if ( _equippedWeapon == null ) return;
            if ( _equippedWeaponScope == null ) return;

            if ( _state != aiming ) {
                if ( aiming ) {
                    _equippedWeaponScope.OnAim();
                } else {
                    _equippedWeaponScope.OnAimStop();
                }

                _state = aiming;
            }

            // switch ( aiming ) {
            //     case true when !AimingLastFrame:
            //         _equippedWeaponScope.OnAim();
            //         break;
            //     case false when AimingLastFrame:
            //         _equippedWeaponScope.OnAimStop();
            //         break;
            // }

            if ( input.IsDown(PlayerInput.NetworkInputData.ButtonMelee) && CanPlayAnimationMelee() ) {
                PlayMelee();
            }

            if ( input.IsDown(PlayerInput.NetworkInputData.ButtonGrenade) && CanPlayAnimationGrenadeThrow() ) {
                PlayGrenadeThrow();
            }

            if ( input.IsDown(PlayerInput.NetworkInputData.ButtonInspect) && CanPlayAnimationInspect() ) {
                Inspect();
            }

            if ( input.IsDown(PlayerInput.NetworkInputData.ButtonReload) && CanPlayAnimationReload() ) {
                PlayReloadAnimation();
            }

            // Holding the firing button.
            if ( input.IsDown(PlayerInput.NetworkInputData.ButtonShoot) ) {

                if ( CanPlayAnimationFire() ) {
                    Debug.Log($"Firing {_equippedWeapon.name}");

                    if ( _equippedWeapon.HasAmmunition() & !_equippedWeapon.IsAutomatic() ) {
                        if ( Time.time - _lastShotTime > 60.0f / _equippedWeapon.GetRateOfFire() ) {
                            Fire(input);
                        }
                    } else {
                        FireEmpty();
                    }
                }
            }

            _axisMovementSmooth =
                Vector2.Lerp(_axisMovementSmooth, input.Move, Time.deltaTime * _weaponSwaySmoothValueInput);
            _axisLookSmooth = Vector2.Lerp(_axisLookSmooth, axisLook, Time.deltaTime * _weaponSwaySmoothValueInput);

            UpdateAnimator(input);

            // Update Aiming Alpha. We need to get this here because we're using the Animator to interpolate the aiming value.
            _aimingAlpha = _animator.GetFloat(HashAimingAlpha);

            // Interpolate the world camera's field of view based on whether we are aiming or not.
            _cameraWorld.fieldOfView = Mathf.Lerp(_fieldOfView,
                _fieldOfView * _equippedWeapon.GetFieldOfViewMultiplierAim(),
                _aimingAlpha);

            // Interpolate the depth camera's field of view based on whether we are aiming or not.
            _cameraDepth.fieldOfView = Mathf.Lerp(_fieldOfViewWeapon,
                _fieldOfViewWeapon * _equippedWeapon.GetFieldOfViewMultiplierAimWeapon(), _aimingAlpha);
        }
    }

    // private void SetWeapon(PlayerInput.NetworkInputData input) {
    //     // Get the index increment direction for our inventory using the scroll wheel direction. If we're not
    //     // actually using one, then just increment by one.
    //
    //     var indexNext = input.Scroll > 0 ? _inventory.GetNextIndex() : _inventory.GetLastIndex();
    //     var indexCurrent = _inventory.GetEquippedIndex();
    //     
    //     //Make sure we're allowed to change, and also that we're not using the same index, otherwise weird things happen!
    //     if ( CanChangeWeapon() && (indexCurrent != indexNext) )
    //         StartCoroutine(nameof(Equip), indexNext);
    // }

    public override void Render() {
        base.Render();

        // Try and calculate the sway so we can apply it.
        if ( _enableWeaponSway )
            CalculateSway();
    }

    private void LateUpdate() {
        if ( _boneWeapon == null ) return;
        if ( _equippedWeapon == null ) return;
        if ( _equippedWeaponScope == null ) return;

        //Get the weapon offsets.
        var weaponOffsets = _equippedWeapon.GetWeaponOffsets();

        //Frame Location Local.
        var frameLocationLocal = _swayLocation;
        frameLocationLocal +=
            Vector3.Lerp(weaponOffsets.StandingLocation, weaponOffsets.AimingLocation, _aimingAlpha);
        frameLocationLocal += Vector3.Lerp(default, _equippedWeaponScope.GetOffsetAimingLocation(), _aimingAlpha);
        frameLocationLocal +=
            Vector3.Lerp(weaponOffsets.ActionLocation * _animator.GetFloat(HashAlphaActionOffset), default,
                _aimingAlpha);

        var frameRotationLocal = _swayRotation;
        frameRotationLocal +=
            Vector3.Lerp(weaponOffsets.StandingRotation, weaponOffsets.AimingRotation, _aimingAlpha);
        frameRotationLocal += Vector3.Lerp(default, _equippedWeaponScope.GetOffsetAimingRotation(), _aimingAlpha);
        frameRotationLocal +=
            Vector3.Lerp(weaponOffsets.ActionRotation * _animator.GetFloat(HashAlphaActionOffset), default,
                _aimingAlpha);

        // Transform socketScopeCorrected = equippedWeaponScope.transform.GetChild(0).GetChild(0).GetChild(0);
        // Transform socketScopes = equippedWeaponScope.transform.parent.parent;
        //
        // Log.wtf(equippedWeaponScope.transform.parent.parent.parent.localPosition);
        // Vector3 localPosition = equippedWeaponScope.transform.GetChild(0).GetChild(0).localPosition;
        // Log.wtf(localPosition);
        // boneWeapon.localPosition -= Vector3.Lerp(default, localPosition, aimingAlpha);

        //Add to the weapon position and rotation.
        _boneWeapon.localPosition += frameLocationLocal;
        _boneWeapon.localEulerAngles += frameRotationLocal;

        //Make sure that we have a kinematics component!
        if ( _kinematics != null ) {
            //Get Left Constraint Alpha.
            var alphaLeft = _animator.GetFloat(HashConstraintAlphaLeft);
            //Get Right Constraint Alpha.
            var alphaRight = _animator.GetFloat(HashConstraintAlphaRight);

            //Compute.
            _kinematics.Compute(alphaLeft, alphaRight);
        }
    }

    // public Camera GetCameraWorld() => cameraWorld;
    // public override Camera GetCameraDepth() => cameraDepth;

    /// <summary>
    /// Returns the looking sway values for this frame.
    /// </summary>
    /// <param name="sway">Sway values to use for the calculation.</param>
    private (Vector3 location, Vector3 rotation) GetSwayLook(Sway sway) {
        var horizontalAxis = Mathf.Clamp(_axisLookSmooth.x, -1.0f, 1.0f);
        var verticalAxis = Mathf.Clamp(_axisLookSmooth.y, -1.0f, 1.0f);

        var horizontalLocation = horizontalAxis * sway.Look.Location.Horizontal;
        var horizontalRotation = horizontalAxis * sway.Look.Rotation.Horizontal;

        var verticalLocation = verticalAxis * sway.Look.Location.Vertical;
        var verticalRotation = verticalAxis * sway.Look.Rotation.Vertical;

        return (horizontalLocation + verticalLocation, horizontalRotation + verticalRotation);
    }

    /// <summary>
    /// Returns the movement sway values for this frame.
    /// </summary>
    /// <param name="sway">Sway values to use for the calculation.</param>
    private (Vector3 location, Vector3 rotation) GetSwayMovement(Sway sway) {
        var horizontalAxis = Mathf.Clamp(_axisMovementSmooth.x, -1.0f, 1.0f);
        var verticalAxis = Mathf.Clamp(_axisMovementSmooth.y, -1.0f, 1.0f);

        var horizontalLocation = horizontalAxis * sway.Movement.Location.Horizontal;
        var horizontalRotation = horizontalAxis * sway.Movement.Rotation.Horizontal;

        var verticalLocation = verticalAxis * sway.Movement.Location.Vertical;
        var verticalRotation = verticalAxis * sway.Movement.Rotation.Vertical;

        return (horizontalLocation + verticalLocation, horizontalRotation + verticalRotation);
    }

    public InventoryBehaviour GetInventory() => _inventory;

    public int GetGrenadesCurrent() => TotalGrenades;
    public int GetGrenadesTotal() => 10;

    // public bool IsCrosshairVisible() => !_aiming && !_holstered;
    // public bool IsRunning() => _running;
    // public bool IsAiming() => _aiming;
    //
    // public Vector2 GetInputMovement() => _axisMovement;
    // public Vector2 GetInputLook() => _axisLook;

    public AudioClip[] GetAudioClipsGrenadeThrow() => _audioClipsGrenadeThrow;
    public AudioClip[] GetAudioClipsMelee() => _audioClipsMelee;

    /// <summary>
    /// Updates all the animator properties for this frame.
    /// </summary>
    /// <param name="input"></param>
    private void UpdateAnimator(PlayerInput.NetworkInputData input) {
        var dt = Runner.DeltaTime;
        
        //Check if we're currently reloading cycled.
        const string boolNameReloading = "Reloading";
        if ( _animator.GetBool(boolNameReloading) ) {
            //If we only have one more bullet to reload, then we can change the boolean already.
            if ( _equippedWeapon.GetAmmunitionTotal() - _equippedWeapon.GetAmmunitionCurrent() < 1 ) {
                //Update the character animator.
                _animator.SetBool(boolNameReloading, false);
                //Update the weapon animator.
                _equippedWeapon.GetAnimator().SetBool(boolNameReloading, false);
            }
        }

        //Leaning. Affects how much the character should apply of the leaning additive animation.
        _animator.SetFloat(HashLeaning, Mathf.Clamp01(input.Move.y), 0.5f, dt);

        //Movement Value. This value affects absolute movement. Aiming movement uses this, as opposed to per-axis movement.
        _animator.SetFloat(HashMovement, Mathf.Clamp01(Mathf.Abs(input.Move.x) + Mathf.Abs(input.Move.y)),
            _dampTimeLocomotion, dt);

        //Aiming Speed Multiplier.
        _animator.SetFloat(HashAimingSpeedMultiplier, _aimingSpeedMultiplier);

        //Turning Value. This determines how much of the turning animation to play based on our current look rotation.
        _animator.SetFloat(HashTurning, Mathf.Abs((float) input.YawDelta), _dampTimeTurning, dt);

        //Horizontal Movement Float.
        _animator.SetFloat(HashHorizontal, _axisMovementSmooth.x, _dampTimeLocomotion, dt);
        //Vertical Movement Float.
        _animator.SetFloat(HashVertical, _axisMovementSmooth.y, _dampTimeLocomotion, dt);

        //Update the aiming value, but use interpolation. This makes sure that things like firing can transition properly.
        _animator.SetFloat(HashAimingAlpha,
            Convert.ToSingle(input.IsDown(PlayerInput.NetworkInputData.ButtonScope)),
            0.25f / _aimingSpeedMultiplier * _dampTimeAiming, dt);

        //Update Forward Multiplier. This allows us to change the play rate of our animations based on our movement multipliers.
        _animator.SetFloat(HashPlayRateLocomotionForward, _controller.GetMultiplierForward(), 0.2f,
            dt);
        //Update Sideways Multiplier. This allows us to change the play rate of our animations based on our movement multipliers.
        _animator.SetFloat(HashPlayRateLocomotionSideways, _controller.GetMultiplierSideways(), 0.2f,
            dt);
        //Update Backwards Multiplier. This allows us to change the play rate of our animations based on our movement multipliers.
        _animator.SetFloat(HashPlayRateLocomotionBackwards, _controller.GetMultiplierBackwards(), 0.2f,
            dt);

        //Update Animator Aiming.
        const string boolNameAim = "Aim";
        _animator.SetBool(boolNameAim, input.IsDown(PlayerInput.NetworkInputData.ButtonScope));

        //Update Animator Running.
        const string boolNameRun = "Running";
        _animator.SetBool(boolNameRun, input.IsDown(PlayerInput.NetworkInputData.ButtonSprint));
    }

    /// <summary>
    /// Plays the inspect animation.
    /// </summary>
    private void Inspect() {
        //State.
        Inspecting = true;
        //Play.
        _animator.CrossFade("Inspect", 0.0f, _layerActions, 0);
    }

    /// <summary>
    /// Calculates the sway values.
    /// </summary>
    private void CalculateSway() {
        //We need a scope!
        if ( _equippedWeaponScope == null )
            return;

        //Weapon Sway Values.
        var sway = _equippedWeapon.GetSway();
        //Weapon Sway Smooth Value.
        var swaySmoothValue = _equippedWeapon.GetSwaySmoothValue();

        var swayLookStanding = GetSwayLook(sway);
        var swayLookAiming = GetSwayLook(_equippedWeaponScope.GetSwayAiming());

        var swayMovementStanding = GetSwayMovement(sway);
        var swayMovementAiming = GetSwayMovement(_equippedWeaponScope.GetSwayAiming());

        //Get Look Sway.
        (Vector3 location, Vector3 rotation) swayLook = default;
        swayLook.location = Vector3.Lerp(swayLookStanding.location, swayLookAiming.location, _aimingAlpha);
        swayLook.rotation = Vector3.Lerp(swayLookStanding.rotation, swayLookAiming.rotation, _aimingAlpha);

        //Get Movement Sway.
        (Vector3 location, Vector3 rotation) swayMovement = default;
        swayMovement.location = Vector3.Lerp(swayMovementStanding.location, swayMovementAiming.location, _aimingAlpha);
        swayMovement.rotation = Vector3.Lerp(swayMovementStanding.rotation, swayMovementAiming.rotation, _aimingAlpha);

        //Calculate Sway Location.
        var frameLocation = swayLook.location + swayMovement.location;
        //Interpolate.
        _swayLocation = Vector3.LerpUnclamped(_swayLocation, frameLocation, Time.deltaTime * swaySmoothValue);

        //Calculate Sway Rotation.
        var frameRotation = swayLook.rotation + swayMovement.rotation;
        //Interpolate.
        _swayRotation = Vector3.LerpUnclamped(_swayRotation, frameRotation, Time.deltaTime * swaySmoothValue);
    }

    /// <summary>
    /// Fires the character's weapon.
    /// </summary>
    private void Fire(PlayerInput.NetworkInputData input) {
        //Save the shot time, so we can calculate the fire rate correctly.
        // _lastShotTime = Time.time;
        //Fire the weapon! Make sure that we also pass the scope's spread multiplier if we're aiming.
        _equippedWeapon.Fire(input.IsDown(PlayerInput.NetworkInputData.ButtonShoot)
            ? _equippedWeaponScope.GetMultiplierSpread()
            : 1.0f);

        //Play firing animation.
        const string stateName = "Fire";
        _animator.CrossFade(stateName, 0.05f, _layerOverlay, 0);

        //Play bolt actioning animation if needed, and if we have ammunition. We don't play this for the last shot.
        if ( _equippedWeapon.IsBoltAction() && _equippedWeapon.HasAmmunition() )
            UpdateBolt(true);

        // Automatically reload the weapon if we need to. This is very helpful for things like grenade launchers or rocket launchers.
        if ( !_equippedWeapon.HasAmmunition() && _equippedWeapon.GetAutomaticallyReloadOnEmpty() )
            StartCoroutine(nameof(TryReloadAutomatic));
    }

    private void PlayReloadAnimation() {
        //Get the name of the animation state to play, which depends on weapon settings, and ammunition!
        var stateName = _equippedWeapon.HasCycledReload()
            ? "Reload Open"
            : (_equippedWeapon.HasAmmunition() ? "Reload" : "Reload Empty");

        //Play the animation state!
        _animator.Play(stateName, _layerActions, 0.0f);

        //Set Reloading Bool. This helps cycled reloads know when they need to stop cycling.
        const string boolName = "Reloading";
        _animator.SetBool(boolName, Reloading = true);

        //Reload.
        _equippedWeapon.Reload();
    }

    /// <summary>
    /// Plays The Reload Animation After A Delay. Helpful to reload automatically after running out of ammunition.
    /// </summary>
    private IEnumerator TryReloadAutomatic() {
        yield return new WaitForSeconds(_equippedWeapon.GetAutomaticallyReloadOnEmptyDelay());

        PlayReloadAnimation();
    }

    /// <summary>
    /// Equip Weapon Coroutine.
    /// </summary>
    private IEnumerator Equip(int index = 0) {
        //Only if we're not holstered, holster. If we are already, we don't need to wait.
        if ( !_holstered ) {
            //Holster.
            SetHolstered(_holstering = true);
            //Wait.
            yield return new WaitUntil(() => _holstering == false);
        }

        // We do this just in case we were holstered
        SetHolstered(false);

        _animator.Play("Unholster", _layerHolster, 0);

        _inventory.Equip(index);
        RefreshWeaponSetup();
    }

    /// <summary>
    /// Refresh all weapon things to make sure we're all set up!
    /// </summary>
    private void RefreshWeaponSetup() {
        //Make sure we have a weapon. We don't want errors!
        if ( (_equippedWeapon = _inventory.GetEquipped()) == null )
            return;

        // Update Animator Controller. We do this to update all animations to a specific weapon's set.
        _animator.runtimeAnimatorController = _equippedWeapon.GetAnimatorController();

        //Get the attachment manager so we can use it to get all the attachments!
        _weaponAttachmentManager = _equippedWeapon.GetAttachmentManager();
        if ( _weaponAttachmentManager == null )
            return;

        //Get equipped scope. We need this one for its settings!
        _equippedWeaponScope = _weaponAttachmentManager.GetEquippedScope();
        //Get equipped magazine. We need this one for its settings!
        _equippedWeaponMagazine = _weaponAttachmentManager.GetEquippedMagazine();
    }

    private void FireEmpty() {
        /*
         * Save Time. Even though we're not actually firing, we still need this for the fire rate between
         * empty shots.
         */
        _lastShotTime = Time.time;
        //Play.
        _animator.CrossFade("Fire Empty", 0.05f, _layerOverlay, 0);
    }

    /// <summary>
    /// Plays The Grenade Throwing Animation.
    /// </summary>
    private void PlayGrenadeThrow() {
        //Start State.
        _throwingGrenade = true;

        //Play Normal.
        _animator.CrossFade("Grenade Throw", 0.15f,
            _animator.GetLayerIndex("Layer Actions Arm Left"), 0.0f);

        //Play Additive.
        _animator.CrossFade("Grenade Throw", 0.05f,
            _animator.GetLayerIndex("Layer Actions Arm Right"), 0.0f);
    }

    /// <summary>
    /// Play The Melee Animation.
    /// </summary>
    private void PlayMelee() {
        //Start State.
        _meleeing = true;

        //Play Normal.
        _animator.CrossFade("Knife Attack", 0.05f,
            _animator.GetLayerIndex("Layer Actions Arm Left"), 0.0f);

        //Play Additive.
        _animator.CrossFade("Knife Attack", 0.05f,
            _animator.GetLayerIndex("Layer Actions Arm Right"), 0.0f);
    }

    /// <summary>
    /// Changes the value of bolting, and updates the animator.
    /// </summary>
    private void UpdateBolt(bool value) {
        //Update.
        _animator.SetBool(HashBoltAction, _bolting = value);
    }

    /// <summary>
    /// Updates the "Holstered" variable, along with the Character's Animator value.
    /// </summary>
    private void SetHolstered(bool value = true) {
        //Update value.
        _holstered = value;

        //Update Animator.
        const string boolName = "Holstered";
        _animator.SetBool(boolName, _holstered);
    }

    /// <summary>
    /// Can Fire.
    /// </summary>
    private bool CanPlayAnimationFire() {
        if ( _holstered || _holstering ) return false;
        if ( _meleeing || _throwingGrenade ) return false;
        if ( Reloading || _bolting ) return false;
        return !Inspecting;
    }

    /// <summary>
    /// Determines if we can play the reload animation.
    /// </summary>
    private bool CanPlayAnimationReload() {
        if ( Reloading ) return false;
        if ( _meleeing ) return false;
        if ( _bolting ) return false;
        if ( _throwingGrenade ) return false;
        if ( Inspecting ) return false;
        return _equippedWeapon.CanReloadWhenFull() || !_equippedWeapon.IsFull();

        //Return.
    }

    /// <summary>
    /// Returns true if the character is able to throw a grenade.
    /// </summary>
    private bool CanPlayAnimationGrenadeThrow() {
        if ( _holstered || _holstering ) return false;
        if ( _meleeing || _throwingGrenade ) return false;
        if ( Reloading || _bolting ) return false;
        if ( Inspecting ) return false;

        return TotalGrenades != 0;
    }

    /// <summary>
    /// Returns true if the Character is able to melee attack.
    /// </summary>
    private bool CanPlayAnimationMelee() {
        if ( _holstered || _holstering ) return false;
        if ( _meleeing || _throwingGrenade ) return false;
        if ( Reloading || _bolting ) return false;
        return !Inspecting;
    }

    /// <summary>
    /// Returns true if the character is able to holster their weapon.
    /// </summary>
    /// <returns></returns>
    private bool CanPlayAnimationHolster() {
        if ( _meleeing || _throwingGrenade ) return false;
        if ( Reloading || _bolting ) return false;
        return !Inspecting;
    }

    /// <summary>
    /// Returns true if the Character can change their Weapon.
    /// </summary>
    /// <returns></returns>
    private bool CanChangeWeapon() {
        if ( _holstering ) return false;
        if ( _meleeing || _throwingGrenade ) return false;
        if ( Reloading || _bolting ) return false;
        return !Inspecting;
    }

    /// <summary>
    /// Returns true if the Character can play the Inspect animation.
    /// </summary>
    private bool CanPlayAnimationInspect() {
        if ( _holstered || _holstering ) return false;
        if ( _meleeing || _throwingGrenade ) return false;
        if ( Reloading || _bolting ) return false;
        return !Inspecting;
    }

    /// <summary>
    /// Returns true if the Character can Aim.
    /// </summary>
    /// <returns></returns>
    private bool CanAim() {
        if ( _holstered || Inspecting ) return false;
        if ( _meleeing || _throwingGrenade ) return false;
        return !Reloading && !_holstering;
    }

    /// <summary>
    /// Returns true if the character can run.
    /// </summary>
    /// <returns></returns>
    private bool CanRun() {
        if ( Inspecting || _bolting ) return false;
        if ( _meleeing || _throwingGrenade ) return false;
        if ( Reloading || Aiming ) return false;

        return true;

        // // While trying to fire, we don't want to run. We do this just in case we do fire.
        // if ( _holdingButtonFire && _equippedWeapon.HasAmmunition() )
        //     return false;
        //
        // // This blocks running backwards, or while fully moving sideways.
        // return _axisMovement.y > 0 && !(Math.Abs(Mathf.Abs(_axisMovement.x) - 1) < 0.01f);
    }

    // /// <summary>
    // /// Holster.
    // /// </summary>
    // public void OnTryHolster(InputAction.CallbackContext context) {
    //     switch ( context.phase ) {
    //         //Performed.
    //         case InputActionPhase.Performed:
    //             //Check.
    //             if ( CanPlayAnimationHolster() ) {
    //                 //Set.
    //                 SetHolstered(!_holstered);
    //                 //Holstering.
    //                 _holstering = true;
    //             }
    //
    //             break;
    //     }
    // }

    public void EjectCasing() {
        // Notify the weapon.
        if ( _equippedWeapon != null )
            _equippedWeapon.EjectCasing();
    }

    public void FillAmmunition(int amount) {
        //Notify the weapon to fill the ammunition by the amount.
        if ( _equippedWeapon != null )
            _equippedWeapon.FillAmmunition(amount);
    }

    public void ThrowGrenade() {
        if ( _grenadePrefab == null ) return;
        if ( _cameraWorld == null ) return;

        TotalGrenades--;

        var cTransform = _cameraWorld.transform;
        var position = cTransform.position;
        position += cTransform.forward * _grenadeSpawnOffset;
        Instantiate(_grenadePrefab, position, cTransform.rotation);
    }

    public void SetActiveMagazine(int active) {
        _equippedWeaponMagazine.gameObject.SetActive(active != 0);
    }

    public void AnimationEndedBolt() => UpdateBolt(false);
    public void AnimationEndedReload() => Reloading = false;
    public void AnimationEndedGrenadeThrow() => _throwingGrenade = false;
    public void AnimationEndedMelee() => _meleeing = false;
    public void AnimationEndedInspect() => Inspecting = false;
    public void AnimationEndedHolster() => _holstering = false;

    public void SetSlideBack(int back) {
        if ( _equippedWeapon != null )
            _equippedWeapon.SetSlideBack(back);
    }

    public void SetActiveKnife(int active) {
        _knife.SetActive(active != 0);
    }
}