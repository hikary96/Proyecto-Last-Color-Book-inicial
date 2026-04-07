using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3f;
    public float jumpForce = 5f;
    public float crouchScale = 0.5f;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 originalScale;
    // private Animator anim;

    private float moveX;
    private float moveZ;
    private bool jumpRequested;

    private bool is3DMovement = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // anim = GetComponent<Animator>();
        originalScale = transform.localScale;

        // Importante: Interpolate ayuda a que el movimiento por velocidad se vea suave
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // CollisionDetectionMode continuo evita tunneling en paredes delgadas
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        SetMovementMode(false);
    }

    public void SetMovementMode(bool enable3D)
    {
        is3DMovement = enable3D;

        if (enable3D)
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        else
            rb.constraints = RigidbodyConstraints.FreezeRotation
                           | RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        moveX = Input.GetAxis("Horizontal");
        moveZ = is3DMovement ? Input.GetAxis("Vertical") : 0f;

        if (Input.GetKeyDown(KeyCode.W) && !is3DMovement && isGrounded)
            jumpRequested = true;

        if (Input.GetKeyDown(KeyCode.Space) && is3DMovement && isGrounded)
            jumpRequested = true;

        if (Input.GetKey(KeyCode.C))
            transform.localScale = new Vector3(originalScale.x, crouchScale, originalScale.z);
        else
            transform.localScale = originalScale;

        // float moveMagnitude = new Vector3(moveX, 0f, moveZ).magnitude;
        // anim.SetFloat("Speed", moveMagnitude);
    }

    void FixedUpdate()
    {
        // Calculamos la velocidad horizontal deseada
        Vector3 desiredVelocity = new Vector3(moveX, 0f, moveZ) * speed;

        // Conservamos la velocidad Y actual (gravedad y salto) y solo cambiamos X y Z
        rb.linearVelocity = new Vector3(
            desiredVelocity.x,
            rb.linearVelocity.y,   // ← no tocamos Y para no romper gravedad ni salto
            desiredVelocity.z
        );

        if (jumpRequested)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            jumpRequested = false;
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