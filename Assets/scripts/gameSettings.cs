    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class gameSettings : MonoBehaviour
{
    public bool isGamePaused;
    public GameObject settingsUi;
    public Transform guns;
    public int gunId, scopeId, silencerId;



    void Start()
    {
        settingsUi.SetActive(false);
       //Cursor.lockState = CursorLockMode.Locked;




    }

    // Update is called once per frame
    void Update()
    {
        gunId = PlayerPrefs.GetInt("gunId");
        scopeId = PlayerPrefs.GetInt("scopeId");
        silencerId = PlayerPrefs.GetInt("silencerId");

        
        for (int i = 0; i <= 18; i++)
        {
            if (i == gunId || i == 0)
                guns.GetChild(i).gameObject.SetActive(true);
            else
                guns.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void escape()
    {
        if (isGamePaused == true)
            {
                //Cursor.lockState = CursorLockMode.Locked;

                isGamePaused = false;
                settingsUi.SetActive(false);
            }
            else
            {
                //Cursor.lockState = CursorLockMode.None;

                isGamePaused = true;
                settingsUi.SetActive(true);

            }

    }
    public void restartGame()
    {
        SceneManager.LoadScene("game");

    }
    public void exitGame()
    {
        SceneManager.LoadScene("menu");

    }
}
