using UnityEngine;

public class TriggerZonePanel : MonoBehaviour
{
    [Header("Panel a mostrar")]
    public GameObject panel;

    [Header("Tag del jugador")]
    public string playerTag = "Player";

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && panel != null)
            panel.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && panel != null)
            panel.SetActive(false);
    }
}