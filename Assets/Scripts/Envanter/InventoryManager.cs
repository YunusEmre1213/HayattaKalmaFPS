using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    // SINGLETON PATTERN: Sahnedeki diðer tüm kodlarýn bu envantere ulaþmasýný saðlar
    public static InventoryManager Instance { get; private set; }

    [Header("UI Referanslarý")]
    [SerializeField] private GameObject inventoryCanvas;
    [SerializeField] private Transform gridPanel; // 6x4 slotlarýn olduðu panel

    [Header("Dýþarýdan Eklenen Karakter Referansý")]
    [SerializeField] private GameObject infimaPlayerPrefab;

    [Header("Girdi (Input) Ayarlarý")]
    [SerializeField] private InputActionReference toggleInventoryAction;

    [Header("Yere Atma (Drop) Ayarlarý")]
    [SerializeField] private Transform dropPoint;

    private PlayerInput playerInputComponent;
    private InventorySlot[] slots; // Çantadaki tüm karelerin listesi
    private bool isInventoryOpen = false;

    private void Awake()
    {
        // Singleton kurulumu
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.action.Enable();
            toggleInventoryAction.action.performed += ToggleInventory;
        }
    }

    private void OnDisable()
    {
        if (toggleInventoryAction != null)
            toggleInventoryAction.action.performed -= ToggleInventory;
    }

    private void Start()
    {
        if (inventoryCanvas != null) inventoryCanvas.SetActive(false);
        SetCursorState(false);

        if (infimaPlayerPrefab != null)
            playerInputComponent = infimaPlayerPrefab.GetComponentInChildren<PlayerInput>();

        // SÝHÝRLÝ KISIM: GridPanel altýndaki tüm kareleri otomatik olarak hafýzaya alýyoruz
        if (gridPanel != null)
        {
            slots = gridPanel.GetComponentsInChildren<InventorySlot>();
        }
    }

    // YERDEN EÞYA ALINDIÐINDA ÇALIÞACAK ANA FONKSÝYON
    public bool AddItemToInventory(ItemData item, int amount)
    {
        // 1. DURUM: Eþya üst üste birikebiliyorsa (Örn: Mermi veya Bandaj)
        if (item.maxStack > 1)
        {
            foreach (InventorySlot slot in slots)
            {
                // Çantada ayný eþyadan varsa ve o kare henüz tamamen dolmadýysa
                if (!slot.IsEmpty && slot.CurrentItem == item && slot.CurrentStack < item.maxStack)
                {
                    slot.AddAmount(amount);
                    return true; // Baþarýyla eklendi, aramayý bitir
                }
            }
        }

        // 2. DURUM: Eþya birikmiyorsa veya çantada aynýsý yoksa -> Ýlk BOÞ kareyi bul
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.AddItem(item, amount);
                return true; // Baþarýyla yerleþtirildi
            }
        }

        // Çantada hiç boþ yer kalmadýysa
        Debug.LogWarning("Envanter aðzýna kadar dolu!");
        return false;
    }

    public void DropItemFromSlot(InventorySlot slot)
    {
        if (slot.IsEmpty || slot.CurrentItem.dropPrefab == null) return;

        // 1. 3D Objeyi dünyada yarat (DropPoint'in konumunda)
        Instantiate(slot.CurrentItem.dropPrefab, dropPoint.position, Quaternion.identity);

        // 2. Çantadaki sayýyý azalt (Bu fonksiyon sayý 0 olursa yuvayý kendi temizler)
        slot.RemoveAmount(1);
    }

    public void ConsumeItemFromSlot(InventorySlot slot)
    {
        if (slot.IsEmpty) return;

        // Eþyanýn can verme özelliði var mý kontrol et (healAmount > 0)
        if (slot.CurrentItem.healAmount > 0)
        {
            // Eðer canýmýz zaten full ise eþyayý boþa harcatma
            if (PlayerHealth.Instance.currentHealth >= PlayerHealth.Instance.maxHealth)
            {
                Debug.Log("Canýn zaten dolu, bandajý boþuna harcama!");
                return;
            }

            // Karaktere can bas
            PlayerHealth.Instance.Heal(slot.CurrentItem.healAmount);

            // Kullanýlan eþyayý çantadan 1 adet eksilt
            slot.RemoveAmount(1);
        }
        else
        {
            Debug.Log("Bu eþya tüketilemez!");
        }
    }

    // --- MERMÝ SÝSTEMÝ ÝÇÝN YENÝ EKLENEN KONTROLLER ---

    /// <summary>
    /// Çantada belirtilen isimde bir eþya olup olmadýðýný kontrol eder.
    /// </summary>
    public bool HasItem(string searchItemName)
    {
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty && slot.CurrentItem.name == searchItemName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Çantada belirtilen isimdeki eþyadan toplam kaç adet olduðunu sayar.
    /// </summary>
    public int GetItemCount(string targetItemName)
    {
        int totalCount = 0;
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty && slot.CurrentItem.name == targetItemName)
            {
                // HATA DÜZELTÝLDÝ: slot.amount yerine senin kodundaki doðru deðiþken olan slot.CurrentStack kullanýldý.
                totalCount += slot.CurrentStack;
            }
        }
        return totalCount;
    }

    /// <summary>
    /// Çantadan belirtilen isimdeki eþyadan belirli bir miktar siler.
    /// </summary>
    public void RemoveItemByName(string targetItemName, int amountToRemove)
    {
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty && slot.CurrentItem.name == targetItemName)
            {
                slot.RemoveAmount(amountToRemove);
                return;
            }
        }
    }

    public void ToggleInventory(InputAction.CallbackContext context)
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryCanvas != null) inventoryCanvas.SetActive(isInventoryOpen);
        SetCursorState(isInventoryOpen);

        if (playerInputComponent != null)
        {
            if (isInventoryOpen)
            {
                Time.timeScale = 0f;
                playerInputComponent.actions.FindAction("Look").Disable();
                playerInputComponent.actions.FindAction("Fire").Disable();
                playerInputComponent.actions.FindAction("Aim").Disable();
            }
            else
            {
                Time.timeScale = 1f;
                playerInputComponent.actions.FindAction("Look").Enable();
                playerInputComponent.actions.FindAction("Fire").Enable();
                playerInputComponent.actions.FindAction("Aim").Enable();
            }
        }
    }

    private void SetCursorState(bool isVisible)
    {
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isVisible;
    }
}