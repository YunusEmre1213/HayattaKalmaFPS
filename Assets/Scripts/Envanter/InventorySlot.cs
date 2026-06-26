using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Referanslarý")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI stackText;

    // Dýþarýdan okunabilmesi için Property (Özellik) haline getirdik
    public ItemData CurrentItem { get; private set; }
    public int CurrentStack { get; private set; }
    public bool IsEmpty => CurrentItem == null; // Yuva boþ mu kontrolü

    private void Start()
    {
        ClearSlot();
    }

    public void AddItem(ItemData item, int amount)
    {
        CurrentItem = item;
        CurrentStack = amount;

        iconImage.sprite = item.icon;
        iconImage.enabled = true;

        if (CurrentStack > 1)
        {
            stackText.text = CurrentStack.ToString();
            stackText.enabled = true;
        }
        else
        {
            stackText.enabled = false;
        }
    }

    // Yýðýnýn üzerine yeni mermi/bandaj eklemek için fonksiyon
    public void AddAmount(int amount)
    {
        CurrentStack += amount;
        stackText.text = CurrentStack.ToString();
        stackText.enabled = true;
    }

    public void ClearSlot()
    {
        CurrentItem = null;
        CurrentStack = 0;
        iconImage.sprite = null;
        iconImage.enabled = false;
        stackText.text = "";
        stackText.enabled = false;
    }

    public void RemoveAmount(int amount)
    {
        CurrentStack -= amount;
        if (CurrentStack <= 0)
        {
            ClearSlot(); // Sayý 0'a düþerse yuvayý tamamen boþalt
        }
        else
        {
            stackText.text = CurrentStack.ToString();
            stackText.enabled = (CurrentStack > 1);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsEmpty) return; 

        
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            InventoryManager.Instance.DropItemFromSlot(this);
        }
        
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.Instance.ConsumeItemFromSlot(this);
        }
    }
}