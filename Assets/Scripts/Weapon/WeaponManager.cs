using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging; // YENÝ: IK sistemini koda dahil ettik

public class WeaponManager : MonoBehaviour
{
    [Header("Silah Referanslarý")]
    [SerializeField] private GameObject equippedWeaponModel;
    [SerializeField] private Animator animator;
    [SerializeField] private Rig weaponRig; // YENÝ: IK mýknatýs sistemimiz

    [Header("Niþan Alma (ADS) Ayarlarý")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float aimFOV = 40f;
    [SerializeField] private float adsSpeed = 10f;

    private PlayerInputActions inputActions;
    private bool isArmed = false;
    private bool isAiming = false;
    private float targetFOV;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.EquipWeapon.performed += context => ToggleWeapon();
        inputActions.Player.Aim.performed += context => isAiming = true;
        inputActions.Player.Aim.canceled += context => isAiming = false;
        inputActions.Player.Fire.performed += context => Shoot();
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Start()
    {
        targetFOV = normalFOV;

        if (equippedWeaponModel != null) equippedWeaponModel.SetActive(false);
        if (weaponRig != null) weaponRig.weight = 0f; // Baþlangýçta kollar serbest
    }

    private void Update()
    {
        HandleADS();

        // YENÝ: Silah çekiliyse elleri silaha yapýþtýr (1), silahsýzsak elleri serbest býrak (0)
        if (weaponRig != null)
        {
            float targetWeight = isArmed ? 1f : 0f;
            weaponRig.weight = Mathf.Lerp(weaponRig.weight, targetWeight, Time.deltaTime * 10f);
        }
    }

    private void ToggleWeapon()
    {
        isArmed = !isArmed;

        if (equippedWeaponModel != null) equippedWeaponModel.SetActive(isArmed);
        if (animator != null) animator.SetBool("IsArmed", isArmed);
        if (!isArmed) isAiming = false;
    }

    private void HandleADS()
    {
        if (!isArmed)
        {
            targetFOV = normalFOV;
            return;
        }

        targetFOV = isAiming ? aimFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * adsSpeed);
    }

    private void Shoot()
    {
        if (!isArmed) return;
        Debug.Log("ATEÞ EDÝLDÝ: Pew Pew!");
    }
}