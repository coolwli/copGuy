using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class gunSettings : MonoBehaviour
{
    //public Transform guns;
    public int gunId, scopeId, silencerId;
    public Sprite[] gunSprites;
    public Image gunIcon;
    public Image[] scopeBg, silencerBg;
    public GameObject[] scopesUi;
    public GameObject silencerUi;
    void Start()
    {
        gunId = 1;

        //Update cursor lock state.
        //Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {

        gunIcon.sprite = gunSprites[gunId];

        for (int j = 0; j <= 4; j++)
        {
            if (j == scopeId)
                scopeBg[j].color = new Color32(255, 255, 225, 100);
            else
                scopeBg[j].color = new Color32(0, 0, 0, 100);

        }
        if (silencerId == 1)
        {
            silencerBg[0].color = new Color32(0, 0, 0, 100);
            silencerBg[1].color = new Color32(255, 255, 225, 100);
        }
        else
        {
            silencerBg[1].color = new Color32(0, 0, 0, 100);
            silencerBg[0].color = new Color32(255, 255, 225, 100);
        }

        if (gunId == 5 || gunId == 6 || gunId == 7 || gunId == 8)
        {
            scopesUi[1].SetActive(true);
            scopesUi[2].SetActive(true);
            scopesUi[0].SetActive(false);
            scopesUi[3].SetActive(false);
            silencerUi.SetActive(true);


        }
        else if (gunId == 16 || gunId == 17 || gunId == 18)
        {
            scopesUi[1].SetActive(false);
            scopesUi[2].SetActive(false);
            scopesUi[0].SetActive(false);
            scopesUi[3].SetActive(false);
            silencerUi.SetActive(true);
        }
        else if (gunId == 9)
        {
            scopesUi[1].SetActive(false);
            scopesUi[2].SetActive(false);
            scopesUi[0].SetActive(false);
            scopesUi[3].SetActive(false);
            silencerUi.SetActive(false);
        }
        else if (gunId == 4 || gunId == 10)
        {
            scopesUi[1].SetActive(true);
            scopesUi[2].SetActive(true);
            scopesUi[0].SetActive(true);
            scopesUi[3].SetActive(true);
            silencerUi.SetActive(false);
        }
        else
        {
            scopesUi[1].SetActive(true);
            scopesUi[2].SetActive(true);
            silencerUi.SetActive(true);
            scopesUi[0].SetActive(true);
            scopesUi[3].SetActive(true);
        }


    }
    public void selectorButton(bool isNext)
    {
        scopeId = 0;
        silencerId = 0;
        if (isNext)
        {
            if (gunId == 18)
                gunId = 1;
            else
                gunId += 1;
        }
        if (!isNext)
        {
            if (gunId == 1)
                gunId = 18;
            else
                gunId -= 1;
        }
    }
    public void setScope(int id)
    {
        scopeId = id;
    }
    public void setSilencer(int id)
    {
        silencerId = id;
    }
    public void startGame()
    {
        SceneManager.LoadScene("game");
        PlayerPrefs.SetInt("gunId", gunId);
        PlayerPrefs.SetInt("scopeId", scopeId);
        PlayerPrefs.SetInt("silencerId", silencerId);


    }
}
