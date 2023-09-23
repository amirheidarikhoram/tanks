using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Setup : MonoBehaviour
{
    public InputField inputField;

    public void Join() {
        string serverAddress = inputField.text;

        PlayerPrefs.SetString("address", serverAddress);

        SceneManager.LoadScene("GameScene");
    }
}
