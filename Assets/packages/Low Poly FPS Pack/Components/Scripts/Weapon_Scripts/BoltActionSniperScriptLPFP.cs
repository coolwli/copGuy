using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BoltActionSniperScriptLPFP : MonoBehaviour
{

    //Animator component attached to weapon
    Animator anim;
    public Sprite weaponSprite;
    [Header("Scripts")]
    public setUi setUi;
    public gameSettings gunSettings;
    public touchController touchController;
    public FloatingJoystick floatingJoystick;

    [Header("Gun Camera")]
    //Main gun camera
    public Camera gunCamera;

    [Header("Gun Camera Options")]
    //How fast the camera field of view changes when aiming 
    [Tooltip("How fast the camera field of view changes when aiming.")]
    public float fovSpeed = 15.0f;
    //Default camera field of view
    [Tooltip("Default value for camera field of view (40 is recommended).")]
    public float defaultFov = 40.0f;

    public float aimFOV;


    //Weapon attachments components
    [System.Serializable]
    public class weaponAttachmentRenderers
    {
        public SkinnedMeshRenderer silencerRenderer;
    }
    public weaponAttachmentRenderers WeaponAttachmentRenderers;

    [Header("Weapon Sway")]
    //Enables weapon sway
    [Tooltip("Toggle weapon sway.")]
    public bool weaponSway;

    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmoothValue = 4.0f;

    private Vector3 initialSwayPosition;

    //Used for fire rate
    private float lastFired;

    //How fast the weapon fires, higher value means faster rate of fire
    [Header("Weapon Settings")]
    [Tooltip("How fast the weapon fires, higher value means faster rate of fire.")]
    public float fireRate;
    //Eanbles auto reloading when out of ammo
    [Tooltip("Enables auto reloading when out of ammo.")]
    public bool autoReload;
    //Delay between shooting last bullet and reloading
    public float autoReloadDelay;
    //Check if reloading
    private bool isReloading;
    //Check if shooting
    private bool isShooting;
    //Check if running
    private bool isRunning;
    //Check if aiming
    private bool isAiming;
    //Check if walking
    private bool isWalking;
    //Check if inspecting weapon
    private bool isInspecting;


    [Tooltip("How much ammo the weapon should have.")]
    public int maxAmmo, totalAmmo, currentAmmo;

    //Check if out of ammo
    private bool outOfAmmo;

    [Header("Bullet Settings")]
    //Bullet
    [Tooltip("How much force is applied to the bullet when shooting.")]
    public float bulletForce = 400;

    [Header("Grenade Settings")]
    public float grenadeSpawnDelay = 0.35f;

    [Header("Scope Settings")]
    //Material used to render zoom effect
    public Material scopeRenderMaterial;
    //Scope color when not aiming
    public Color fadeColor;
    //Scope color when aiming
    public Color defaultColor;

    [Header("Muzzleflash Settings")]
    public bool randomMuzzleflash = false;
    //min should always bee 1
    private int minRandomValue = 1;

    [Range(2, 25)]
    public int maxRandomValue = 5;

    private int randomMuzzleflashValue;

    public bool enableMuzzleFlash;
    public ParticleSystem muzzleParticles;
    public bool enableSparks;
    public ParticleSystem sparkParticles;
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("Muzzleflash Light Settings")]
    public Light muzzleFlashLight;
    public float lightDuration = 0.02f;

    [Header("Audio Source")]
    //Main audio source
    public AudioSource mainAudioSource;
    //Audio source used for shoot sound
    public AudioSource shootAudioSource;



    [System.Serializable]
    public class prefabs
    {
        [Header("Prefabs")]
        public Transform bulletPrefab;
        public Transform casingPrefab;
        public Transform grenadePrefab;
    }
    public prefabs Prefabs;

    [System.Serializable]
    public class spawnpoints
    {
        [Header("Spawnpoints")]
        //Array holding casing spawn points 
        //(some weapons use more than one casing spawn)
        public float casingDelayTimer;
        //Casing spawn point array
        public Transform casingSpawnPoint;
        //Bullet prefab spawn from this point
        public Transform bulletSpawnPoint;

        public Transform grenadeSpawnPoint;
    }
    public spawnpoints Spawnpoints;

    [System.Serializable]
    public class soundClips
    {
        public AudioClip shootSound;
        public AudioClip silencerShootSound;
        public AudioClip takeOutSound;
        public AudioClip holsterSound;
        public AudioClip aimSound;
    }
    public soundClips SoundClips;

    private bool soundHasPlayed = false;

    private void Awake()
    {

        //Set the animator component
        anim = GetComponent<Animator>();
        //Set current ammo to total ammo value
        currentAmmo = maxAmmo;

        muzzleFlashLight.enabled = false;


    }

    private void Start()
    {



        //Weapon sway
        initialSwayPosition = transform.localPosition;

        //Set the shoot sound to audio source
        shootAudioSource.clip = SoundClips.shootSound;
    }

    private void Update()
    {
        if (gunSettings.isGamePaused) return;

        if (gunSettings.silencerId == 1 &&
            WeaponAttachmentRenderers.silencerRenderer)
        {
            //If scope1 is true, enable scope renderer
            WeaponAttachmentRenderers.silencerRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
        }
        else
        {
            //If scope1 is false, disable scope renderer
            WeaponAttachmentRenderers.silencerRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = false;
        }
        setUi.aim = true;

        setUi.weaponImage.sprite = weaponSprite;
        setUi.setTexts(currentAmmo, totalAmmo);
        if (touchController.aim && !isReloading && !isRunning && !isInspecting)
        {
            isAiming = true;

            gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                aimFOV, fovSpeed * Time.deltaTime);

            //Change scope color to default color when aiming
            scopeRenderMaterial.color = defaultColor;

            anim.SetBool("Aim", true);

            if (!soundHasPlayed)
            {
                mainAudioSource.clip = SoundClips.aimSound;
                mainAudioSource.Play();

                soundHasPlayed = true;
            }
        }
        else
        {
            //When right click is released
            gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                defaultFov, fovSpeed * Time.deltaTime);

            //Change scope color to fade color when not aiming
            scopeRenderMaterial.color = fadeColor;

            isAiming = false;

            anim.SetBool("Aim", false);

            soundHasPlayed = false;
        }
        //Aiming end

        //If randomize muzzleflash is true, genereate random int values
        if (randomMuzzleflash == true)
        {

            randomMuzzleflashValue = Random.Range(minRandomValue, maxRandomValue);
        }


        AnimationCheck();

        if (touchController.knife && !isInspecting && !isReloading)
        {
            anim.Play("Knife Attack 1", 0, 0f);
            touchController.knife = false;
        }

        //Throw grenade when pressing G key
        if (touchController.bomb && !isReloading && !isRunning && !isInspecting && setUi.grenadeAmmo >= 1)
        {
            touchController.bomb = false;
            setUi.grenadeAmmo -= 1;
            StartCoroutine(GrenadeSpawnDelay());
            //Play grenade throw animation
            anim.Play("GrenadeThrow", 0, 0.0f);
        }

        //If out of ammo
        if (currentAmmo == 0)
        {

            outOfAmmo = true;
            //Auto reload if true
            if (autoReload == true && !isReloading && totalAmmo > 0)
            {
                isReloading = true;

                StartCoroutine(AutoReload());
            }
        }
        else
        {

            outOfAmmo = false;
            //anim.SetBool ("Out Of Ammo", false);
        }

        //Shoot
        if (touchController.fire && !outOfAmmo && !isReloading && !isInspecting && !isRunning)
        {
            touchController.fire = false;

            if (Time.time - lastFired > 1 / fireRate)
            {
                lastFired = Time.time;

                //Remove 1 bullet from ammo
                currentAmmo -= 1;

                if (gunSettings.silencerId == 0)
                {
                    shootAudioSource.clip = SoundClips.shootSound;
                    shootAudioSource.Play();
                }
                else
                {
                    shootAudioSource.clip = SoundClips.silencerShootSound;
                    shootAudioSource.Play();
                }

                if (!isAiming) //if not aiming
                {
                    anim.Play("Fire", 0, 0f);
                    //If random muzzle is false
                    if (!randomMuzzleflash &&
                        enableMuzzleFlash == true)
                    {
                        muzzleParticles.Emit(1);
                        //Light flash start
                        StartCoroutine(MuzzleFlashLight());
                    }
                    else if (randomMuzzleflash == true)
                    {
                        //Only emit if random value is 1
                        if (randomMuzzleflashValue == 1)
                        {
                            if (enableSparks == true)
                            {
                                //Emit random amount of spark particles
                                sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
                            }
                            if (enableMuzzleFlash == true)
                            {
                                muzzleParticles.Emit(1);
                                //Light flash start
                                StartCoroutine(MuzzleFlashLight());
                            }
                        }
                    }
                }
                else //if aiming
                {
                    anim.Play("Aim Fire", 0, 0f);

                    //If random muzzle is false
                    if (!randomMuzzleflash)
                    {
                        muzzleParticles.Emit(1);
                        //If random muzzle is true
                    }
                    else if (randomMuzzleflash == true)
                    {
                        //Only emit if random value is 1
                        if (randomMuzzleflashValue == 1)
                        {
                            if (enableSparks == true)
                            {
                                //Emit random amount of spark particles
                                sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
                            }
                            if (enableMuzzleFlash == true)
                            {
                                muzzleParticles.Emit(1);
                                //Light flash start
                                StartCoroutine(MuzzleFlashLight());
                            }
                        }
                    }
                }

                //Spawn bullet at bullet spawnpoint
                var bullet = (Transform)Instantiate(
                    Prefabs.bulletPrefab,
                    Spawnpoints.bulletSpawnPoint.transform.position,
                    Spawnpoints.bulletSpawnPoint.transform.rotation);

                //Add velocity to the bullet
                bullet.GetComponent<Rigidbody>().velocity =
                    bullet.transform.forward * bulletForce;

                StartCoroutine(CasingDelay());
            }
        }

        //Inspect weapon when T key is pressed
        if (touchController.inspect)
        {
            anim.SetTrigger("Inspect");
            touchController.inspect = false;


        }

        if (touchController.reload)
        {
            if (!isReloading && !isInspecting && currentAmmo < maxAmmo && totalAmmo > 0)
            {
                StartCoroutine("reloadAnim");


            }
            else
                touchController.reload = false;

        }
        //Walking when pressing down WASD keys
        if (floatingJoystick.Horizontal != 0 && !isRunning && !isShooting ||
         (floatingJoystick.Horizontal != 0 && !isRunning && !isShooting))
        {
            anim.SetBool("Walk", true);
        }
        else
        {
            anim.SetBool("Walk", false);
        }

        if ((floatingJoystick.Horizontal > 0.8f || floatingJoystick.Vertical > 0.8f || floatingJoystick.Horizontal < -0.8f || floatingJoystick.Vertical < -0.8f))
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
        //Run anim toggle
        if (isRunning == true)
        {
            anim.SetBool("Run", true);
        }
        else
        {
            anim.SetBool("Run", false);
        }
    }

    private IEnumerator GrenadeSpawnDelay()
    {
        //Wait for set amount of time before spawning grenade
        yield return new WaitForSeconds(grenadeSpawnDelay);
        //Spawn grenade prefab at spawnpoint
        Instantiate(Prefabs.grenadePrefab,
            Spawnpoints.grenadeSpawnPoint.transform.position,
            Spawnpoints.grenadeSpawnPoint.transform.rotation);
    }

    private IEnumerator CasingDelay()
    {
        //Wait before spawning casing
        yield return new WaitForSeconds(Spawnpoints.casingDelayTimer);

        Instantiate(Prefabs.casingPrefab,
            Spawnpoints.casingSpawnPoint.transform.position,
            Spawnpoints.casingSpawnPoint.transform.rotation);
    }

    private IEnumerator AutoReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(0.5f);

        anim.Play("Reload Open", 0, 0f);
        yield return new WaitForSeconds(0.9f);
        for (int i = 0; i < (maxAmmo - currentAmmo);)
        {
            anim.Play("Insert Bullet", 0, 0f);
            yield return new WaitForSeconds(0.8f);
            currentAmmo += 1;
            totalAmmo -= 1;

        }
        anim.Play("Reload Close ", 0, 0f);
        outOfAmmo = false;
        isReloading = false;
    }

    //Reload
    private IEnumerator reloadAnim()
    {
        isReloading = true;

        yield return new WaitForSeconds(0.4f);

        anim.Play("Reload Open", 0, 0f);
        yield return new WaitForSeconds(0.9f);
        for (int i = 0; i < (maxAmmo - currentAmmo);)
        {
            anim.Play("Insert Bullet", 0, 0f);
            yield return new WaitForSeconds(0.8f);
            currentAmmo += 1;
            totalAmmo -= 1;

        }
        anim.Play("Reload Close ", 0, 0f);
        outOfAmmo = false;
        isReloading = false;
        touchController.reload = false;


    }

    //Show light when shooting, then disable after set amount of time
    private IEnumerator MuzzleFlashLight()
    {

        muzzleFlashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        muzzleFlashLight.enabled = false;
    }

    //Check current animation playing
    private void AnimationCheck()
    {

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Inspect"))
        {
            isInspecting = true;
        }
        else
        {
            isInspecting = false;
        }

        //Check if shooting
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Fire"))
        {
            isShooting = true;
        }
        else
        {
            isShooting = false;
        }
    }
}