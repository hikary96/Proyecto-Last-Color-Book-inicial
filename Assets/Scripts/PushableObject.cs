using UnityEngine;

public class PushableObject : MonoBehaviour
{
    public float moveSpeed = 3f;

    void OnCollisionStay(Collision collision)
    {
        // Verifica si el objeto que está chocando tiene el Tag "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            float move = Input.GetAxis("Vertical");

            // Mueve el cubo hacia adelante o atrás
            transform.Translate(Vector3.forward * move * moveSpeed * Time.deltaTime);
        }
    }
}