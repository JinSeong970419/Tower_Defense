using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statusText;

    void Update()
    {
        if (!string.IsNullOrEmpty(statusText.text))
        {
            StartCoroutine(MakeBlank());
        }
    }

    IEnumerator MakeBlank()
    {
        yield return new WaitForSeconds(3);
        statusText.text = null;
    }

}
