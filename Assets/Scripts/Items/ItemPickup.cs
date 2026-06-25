using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Eþya Bilgileri")]
    public ItemData itemData;
    public int amount = 1;

    public void PickUp()
    {
        // Envanter yöneticisine ulaþýp eþyayý çantaya eklemeyi deniyoruz
        bool hasSpace = InventoryManager.Instance.AddItemToInventory(itemData, amount);

        if (hasSpace)
        {
            // Eðer çantada yer varsa ve baþarýyla eklendiyse objeyi dünyadan sil
            Debug.Log(amount + " adet " + itemData.itemName + " envantere istiflendi.");
            Destroy(gameObject);
        }
        else
        {
            // Çantada yer yoksa obje yerde kalmaya devam eder
            Debug.Log("Çanta dolu olduðu için eþya alýnamadý!");
        }
    }
}