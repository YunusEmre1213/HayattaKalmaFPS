using UnityEngine;
using UnityEngine.UI; 

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Can Ayarlarý")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Arayüz (UI) Ayarlarý")]
    [SerializeField] private Slider healthSlider; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentHealth = maxHealth;
    }

    private void Start()
    {
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void Update()
    {
        // Klavyeden 'K' tuþuna basýldýðýnda karaktere 15 hasar ver (Test Amaçlý)
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(15);
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthUI(); 
        Debug.Log("Ýyileþme saðlandý! Kalan Can: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        UpdateHealthUI(); 
        Debug.Log("Hasar alýndý! Kalan Can: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Karakter Öldü!");
        }
    }


    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}