using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("Lazer (Raycast) Ayarlarý")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f; // Oyuncu eþyaya ne kadar uzanabilir?
    [SerializeField] private LayerMask interactableLayer; // Lazer sadece eþyalara çarpsýn (Duvarlarý görmezden gelsin)

    [Header("UI Ayarlarý")]
    [SerializeField] private TextMeshProUGUI promptText; // Ekranda çýkacak "E - Al" yazýsý

    [Header("Girdi (Input)")]
    [SerializeField] private InputActionReference interactAction; // E tuþu

    private ItemPickup currentTarget;

    private void OnEnable()
    {
        if (interactAction != null) interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.action.Disable();
    }

    private void Update()
    {
        CheckForInteractable();

        // E tuþuna basýldýysa ve hedefte bir eþya varsa
        if (interactAction != null && interactAction.action.WasPressedThisFrame() && currentTarget != null)
        {
            currentTarget.PickUp();
            promptText.gameObject.SetActive(false); // Yazýyý temizle
            currentTarget = null;
        }
    }

    private void CheckForInteractable()
    {
        // Ekranýn tam göbeðinden (Crosshair noktasýndan) ileriye bir lazer çiz
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        // Lazer bir þeye çarptý mý? (Sadece Interactable katmanýndakilere)
        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            // Çarptýðý objede ItemPickup kodu var mý?
            ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                currentTarget = pickup;
                promptText.text = "E - " + pickup.itemData.itemName + " Al";
                promptText.gameObject.SetActive(true);
                return; // Bulduk, aramayý býrak
            }
        }

        // Lazer boþa bakýyorsa hedefi sýfýrla ve yazýyý gizle
        currentTarget = null;
        promptText.gameObject.SetActive(false);
    }
}