using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
using UnityEngine.UI;

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
    private Vector2 Facing;

    private bool DialogActive;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        // Initialize internal variable
        TargetVelocity = Vector2.zero;
        Facing = Vector2.down;
        DialogActive = false;

        // Initialize component references
        if (!TryGetComponent<Rigidbody2D>(out rb))
        {
            Debug.LogError("Rigidbody2D component not found on Player.");
        }

        if (!TryGetComponent<Animator>(out anim))
        {
            Debug.LogError("Animator component not found on Player.");
        }

        StartCoroutine(StartGame());
    }

    // This is called whenever Yarn Spinner starts dialog
    public void OnDialogStart ()
    {
        DialogActive = true;
        Facing = TargetVelocity.normalized;
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

            // Remember what direction we are facing
            Facing = TargetVelocity.normalized;
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

        // Perform a raycast to check for interactable objects
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Facing, 1.0f, LayerMask.GetMask("Interactable"));
        if (hit.collider != null)
        {
            // Get the Interactable component and call the Interact method
            if(hit.collider.TryGetComponent<Interactable>(out var interactable)) {
                interactable.Interact();
            }
        }
    }

    public IEnumerator StartGame()
    {
        GameObject CurtainObj = GameObject.FindWithTag("FadeCurtain");
        if (CurtainObj != null)
        {
            Image Curtain = CurtainObj.GetComponent<Image>();
            for (float i = 0; i <= 1; i += 0.01f)
            {
                Curtain.color = new Color(1, 1, 1, 1 - i);
                yield return new WaitForFixedUpdate();
            }
        }

        DialogueRunner Runner = FindObjectOfType<DialogueRunner>();
        if (Runner != null)
        {
            Runner.StartDialogue("Start");
        }
    }

    [YarnCommand("end_game")]
    public IEnumerator EndGame()
    {
        GameObject CurtainObj = GameObject.FindWithTag("FadeCurtain");
        if (CurtainObj != null)
        {
            Image Curtain = CurtainObj.GetComponent<Image>();
            for (float i = 0; i <= 1; i += 0.01f)
            {
                Curtain.color = new Color(1, 1, 1, i);
                yield return new WaitForFixedUpdate();
            }
        }
        yield return new WaitForSeconds(1.0f);

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
