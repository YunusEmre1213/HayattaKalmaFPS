using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    
    public static PlayerStamina Instance { get; private set; }

    [Header("Stamina (Nefes) Ayarlarý")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Tooltip("Koþarken saniyede ne kadar enerji gidecek?")]
    public float drainRate = 20f;

    [Tooltip("Dinlenirken saniyede ne kadar enerji dolacak?")]
    public float regenRate = 15f;

    [Tooltip("Koþmayý býraktýktan kaç saniye sonra nefesimiz yerine gelmeye baþlasýn?")]
    public float regenDelay = 1.0f;

    [Header("Arayüz (UI)")]
    public Slider staminaSlider;

    private float lastRunTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentStamina = maxStamina;
    }

    private void Start()
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    private void Update()
    {
        
        if (staminaSlider != null) staminaSlider.value = currentStamina;
    }

    public void DrainStamina()
    {
        currentStamina -= drainRate * Time.deltaTime;
        if (currentStamina < 0f) currentStamina = 0f;

        
        lastRunTime = Time.time;
    }

    public void RegenStamina()
    {
        
        if (Time.time - lastRunTime > regenDelay)
        {
            currentStamina += regenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }

   
    public bool CanSprint()
    {
      
        return currentStamina > 1f;
    }
}