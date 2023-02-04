using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class setUi : MonoBehaviour
{

    public int grenadeAmmo;
    public GameObject cross;
    public Image weaponImage;
    public bool aim;
    [Header("texts")]
    public TMP_Text totalAmmoText;
    public TMP_Text currentAmmoText;
    public TMP_Text grenadeAmmoText;

    public void setTexts(int currentAmmo, int totalAmmo)
    {
        totalAmmoText.text = totalAmmo.ToString();
        currentAmmoText.text = currentAmmo.ToString();
        grenadeAmmoText.text = grenadeAmmo.ToString();
    }
    private void Update()
    {
        if (aim == true )
        {
            cross.SetActive(false);
        }
        else
        {
            
            cross.SetActive(true);
        }
    }
}
