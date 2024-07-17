using UnityEngine;
using Yarn.Unity;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private string ItemName;

    [SerializeField]
    private string YarnTitle;

    [SerializeField]
    private bool Collectable;

    private bool Collected;

    private DialogueRunner Runner;

    void Start()
    {
        Runner = FindObjectOfType<DialogueRunner>();
        if (Runner == null)
        {
            Debug.LogError("DialogueRunner not found in scene.");
        }
        Collected = false;

        Runner.onDialogueComplete.AddListener(OnDialogEnd);
    }

    public void Interact ()
    {
        if (Runner != null)
        {
            // Keep track of any collectable items
            if (Collectable)
            {
                VariableStorageBehaviour variableStorage = Runner.GetComponent<VariableStorageBehaviour>();
                if (variableStorage != null && variableStorage.TryGetValue<float>($"${ItemName}Count", out var count))
                {
                    variableStorage.SetValue($"${ItemName}Count", count + 1);
                }
                Collected = true;
            }

            // Trigger any related dialog
            if (!string.IsNullOrEmpty(YarnTitle))
            {
                Runner.StartDialogue(YarnTitle);
            }
            else if (Collected)
            {
                Destroy(gameObject);
            }
        }
    }

    public void OnDialogEnd ()
    {
        // If this object is collectible, destroy the object
        if (Collected)
        {
            Destroy(gameObject);
        }
    }
}
