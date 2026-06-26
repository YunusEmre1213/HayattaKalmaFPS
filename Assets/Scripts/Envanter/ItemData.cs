using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Envanter Sistemi/Yeni Eþya")]
public class ItemData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string itemID; 
    public string itemName;

    [Header("3D Model")]
    public GameObject dropPrefab; 

    [TextArea(3, 5)] 
    public string description;

    public Sprite icon; 

    [Header("Envanter Ayarlarý")]
    public ItemType type; 
    public int maxStack = 1;

    [Header("Tüketim Ayarlarý")]
    public int healAmount = 25;


}


public enum ItemType
{
    Weapon,     // Silahlar
    Ammo,       // Mermiler
    Consumable, // Aðrý kesici, su, bandaj
    Lore,       // Eski notlar, fotoðraflar, kasetler
    Resource    // Piller, hurdalar
}