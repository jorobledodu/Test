using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class Survey : MonoBehaviour
{
    [SerializeField] private int _difficultyFeedbackNumber;
    [SerializeField] private TMP_InputField _generalFeedbackInputField;

    string URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLScQXGCHQJNmeB9XLrGwWfp-NK5WVEOKOQ8H7KfPCdzpwfMn_g/formResponse";

    public void DifficultySelection(int num)
    {
        _difficultyFeedbackNumber = num;
    }

    public void Send()
    {
        StartCoroutine(Post(_difficultyFeedbackNumber, _generalFeedbackInputField.text));
    }

    IEnumerator Post(int f1, string f2)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.1820899777", f1);
        form.AddField("entry.373969829", f2);

        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) Debug.Log("Feedback submitted successfully.");
            else Debug.LogWarning("Error in feedback submission: " + www.error);
        }
    }
}
