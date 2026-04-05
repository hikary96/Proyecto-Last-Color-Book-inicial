using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    public float crouchScale = 0.5f;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 originalScale;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Movimiento izquierda / derecha
        float moveX = Input.GetAxis("Horizontal");

        Vector3 movement = new Vector3(moveX, 0, 0) * speed;

        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);

        // ANIMACIÓN
        anim.SetFloat("Speed", Mathf.Abs(moveX));

        // Salto
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
        }

        // Agacharse
        if (Input.GetKey(KeyCode.S))
        {
            transform.localScale = new Vector3(originalScale.x, crouchScale, originalScale.z);
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
