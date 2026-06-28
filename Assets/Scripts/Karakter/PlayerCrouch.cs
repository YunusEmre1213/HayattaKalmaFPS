using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCrouch : MonoBehaviour
{
    // Kodu diðer scriptlerden (örneðin Stamina'dan) okuyabilmek için Singleton yapýyoruz
    public static PlayerCrouch Instance { get; private set; }

    [Header("Bileþenler")]
    [Tooltip("Karakterin fiziksel çarpýþma kapsülü (CapsuleCollider)")]
    public CapsuleCollider playerCollider;
    [Tooltip("Kamerayý ve silahý taþýyan ana obje")]
    public Transform cameraPivot;

    [Header("Boy Ayarlarý")]
    public float standingHeight = 2f;     // Ayaktayken boyumuz
    public float crouchHeight = 1f;       // Çömeldiðimizdeki boyumuz
    public float transitionSpeed = 10f;   // Çömelme/Kalkma hýzý (Yumuþaklýk)

    private float standingCamY;
    private float crouchCamY;
    public bool isCrouching { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (playerCollider == null)
            playerCollider = GetComponent<CapsuleCollider>();

        if (cameraPivot != null)
        {
            standingCamY = cameraPivot.localPosition.y;
            // Kapsül yarý yarýya küçüleceði için kamerayý da orantýlý olarak aþaðý indiriyoruz
            crouchCamY = standingCamY - ((standingHeight - crouchHeight) / 2f);
        }
    }

    private void Update()
    {
        // 'C' veya 'Sol CTRL' tuþuna basýldýðýnda durumu deðiþtir (Aç/Kapat mantýðý)
        if (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            isCrouching = !isCrouching;
        }

        // Hedef boy ve hedef kamera yüksekliðini belirliyoruz
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        float targetCamY = isCrouching ? crouchCamY : standingCamY;

        // 1. FÝZÝKSEL KÜÇÜLME (Hitbox)
        if (playerCollider != null)
        {
            // Boyu yumuþakça küçült/büyüt
            playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, Time.deltaTime * transitionSpeed);

            // Ayaklarýmýzýn yerden kesilmemesi veya yere gömülmemesi için merkez noktasýný (Center) ayarlýyoruz
            Vector3 center = playerCollider.center;
            center.y = playerCollider.height / 2f;
            playerCollider.center = center;
        }

        // 2. KAMERA VE SÝLAHIN AÞAÐI ÝNMESÝ
        if (cameraPivot != null)
        {
            Vector3 camPos = cameraPivot.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * transitionSpeed);
            cameraPivot.localPosition = camPos;
        }
    }
}