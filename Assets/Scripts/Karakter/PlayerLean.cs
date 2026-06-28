using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLean : MonoBehaviour
{
    [Header("Eðilecek Objeler")]
    [Tooltip("Kamerayý ve Karakterin kollarýný buraya sürükleyin")]
    public Transform[] leanObjects;

    [Header("Eðilme Ayarlarý")]
    [Tooltip("Yana doðru ne kadar kayacaðýmýz (Metre cinsinden)")]
    public float leanDistance = 0.5f;
    public float leanAngle = 15f;     // Yana ne kadar yatacaðýmýz (Derece)
    public float leanSpeed = 8f;      // Eðilme hýzý/yumuþaklýðý

    private Vector3[] initialPositions; // Baþlangýç pozisyonlarýný hafýzada tutacak dizi

    private void Start()
    {
        // Oyun baþlarken objelerin orijinal pozisyonlarýný (X, Y, Z) hafýzaya alýyoruz
        initialPositions = new Vector3[leanObjects.Length];

        for (int i = 0; i < leanObjects.Length; i++)
        {
            if (leanObjects[i] != null)
            {
                initialPositions[i] = leanObjects[i].localPosition;
            }
        }
    }

    private void Update()
    {
        float targetZAngle = 0f;
        float targetXPosition = 0f;

        // Q tuþuna basýlý tutuluyorsa Sola yat ve Sola kay
        if (Keyboard.current.qKey.isPressed)
        {
            targetZAngle = leanAngle;
            targetXPosition = -leanDistance;
        }
        // E tuþuna basýlý tutuluyorsa Saða yat ve Saða kay
        else if (Keyboard.current.eKey.isPressed)
        {
            targetZAngle = -leanAngle;
            targetXPosition = leanDistance;
        }

        // Listeye eklediðimiz tüm objeleri iþliyoruz
        for (int i = 0; i < leanObjects.Length; i++)
        {
            if (leanObjects[i] != null)
            {
                // --- 1. ROTASYON (YATMA) ÝÞLEMÝ ---
                // Farenin tavan/zemin bakýþýný bozmadan sadece Z eksenini eðiyoruz
                Vector3 currentAngles = leanObjects[i].localEulerAngles;
                float newZ = Mathf.LerpAngle(currentAngles.z, targetZAngle, Time.deltaTime * leanSpeed);
                leanObjects[i].localEulerAngles = new Vector3(currentAngles.x, currentAngles.y, newZ);

                // --- 2. POZÝSYON (YANA KAYMA) ÝÞLEMÝ ---
                // Karakterin zýplama veya yürüme (Y ve Z) pozisyonunu bozmadan sadece X (Sað/Sol) ekseninde kaydýrýyoruz
                Vector3 currentPos = leanObjects[i].localPosition;
                float newX = Mathf.Lerp(currentPos.x, initialPositions[i].x + targetXPosition, Time.deltaTime * leanSpeed);
                leanObjects[i].localPosition = new Vector3(newX, currentPos.y, currentPos.z);
            }
        }
    }
}