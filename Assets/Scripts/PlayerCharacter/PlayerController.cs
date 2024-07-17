using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    [Range(0, 1000)]
    [Tooltip("Scale applied to the move input that determines move speed per fixed update.")]
    private float MoveSpeed = 150f;

    private Vector2 TargetVelocity;

    private bool DialogActive;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        // Initialize internal variable
        TargetVelocity = Vector2.zero;

        // Initialize component references
        if (!TryGetComponent<Rigidbody2D>(out rb))
        {
            Debug.LogError("Rigidbody2D component not found on Player.");
        }

        if (!TryGetComponent<Animator>(out anim))
        {
            Debug.LogError("Animator component not found on Player.");
        }
    }

    // This is called whenever Yarn Spinner starts dialog
    public void OnDialogStart ()
    {
        DialogActive = true;
        TargetVelocity = Vector2.zero;
    }

    // This is called whenever Yarn Spinner ends dialog
    public void OnDialogEnd ()
    {
        DialogActive = false;
    }

    // This function runs at regular intervals
    void FixedUpdate()
    {
        if (!DialogActive)
        {
            UpdateMovement();
        }
    }

    void UpdateMovement()
    {
        // Move the player based on the target velocity
        rb.velocity = TargetVelocity * Time.fixedDeltaTime;

        if (TargetVelocity == Vector2.zero)
        {
            // Don't clear the previous anim H and V values so we know what way we are facing
            anim.SetBool("Walking", false);
        }
        else
        {
            // Update the animator parameters
            anim.SetBool("Walking", true);
            anim.SetFloat("Horizontal", TargetVelocity.x);
            anim.SetFloat("Vertical", TargetVelocity.y);
        }
    }

    // Called when the Move InputAction value changes
    public void OnMove(InputValue Action)
    {
        if (DialogActive) { return; }
        Vector2 MoveInput = Action.Get<Vector2>();
        TargetVelocity = MoveInput * MoveSpeed;
    }

    // Called when the Interact button is pressed
    public void OnInteract(InputValue Action)
    {
        if (DialogActive) { return; }
        Debug.Log("Interact!");
    }
}
