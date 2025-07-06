using UnityEngine;
using System.Collections;

public class MessageDisplay : MonoBehaviour
{
    // Method to display letters as dialogue with a callback
    public void DisplayDialogue(float delay, System.Action onComplete)
    {
        StartCoroutine(DisplayLettersCoroutine(delay, onComplete));
    }

    private IEnumerator DisplayLettersCoroutine(float delay, System.Action onComplete)
    {
        foreach (Transform letter in transform)
        {
         //   Debug.Log("Activating letter: " + letter.gameObject.name); // Debug log
            letter.gameObject.SetActive(true);
            yield return new WaitForSeconds(delay);
        }

        // Invoke the callback after the dialogue is done
        onComplete?.Invoke();
        AccessInventory.DisableInventory(false);
    }
}