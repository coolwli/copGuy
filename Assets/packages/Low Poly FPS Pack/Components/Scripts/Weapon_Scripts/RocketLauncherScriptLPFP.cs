using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RocketLauncherScriptLPFP : MonoBehaviour
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

    public float aimFov = 18.0f;

    [Header("Rocket Launcher Projectile")]
    [Space(10)]
    //Rocket launcher projectile renderer
    public SkinnedMeshRenderer projectileRenderer;

    [Header("Weapon Sway")]
    //Enables weapon sway
    [Tooltip("Toggle weapon sway.")]
    public bool weaponSway;

    public float swayAmount;
    public float maxSwayAmount;
    public float swaySmoothValue;

    private Vector3 initialSwayPosition;

    [Header("Weapon Settings")]

    public float autoReloadDelay;
    public float showProjectileDelay;

    //Check if reloading
    private bool isReloading;


    //Check if running
    private bool isRunning;
    //Check if aiming
    private bool isAiming;
    //Check if walking
    //Check if inspecting weapon
    private bool isInspecting;

    //How much ammo is currently left
    private int maxAmmo = 1, currentAmmo;
    public int totalAmmo;
    //Check if out of ammo
    private bool outOfAmmo;

    [Header("Grenade Settings")]
    public float grenadeSpawnDelay;

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
        public Transform projectilePrefab;
        public Transform grenadePrefab;
        public GameObject projectile;
    }
    public prefabs Prefabs;

    [System.Serializable]
    public class spawnpoints
    {
        [Header("Spawnpoints")]
        //Array holding casing spawn points 
        //(some weapons use more than one casing spawn)
        //Bullet prefab spawn from this point
        public Transform bulletSpawnPoint;

        public Transform grenadeSpawnPoint;
    }
    public spawnpoints Spawnpoints;


    [System.Serializable]
    public class soundClips
    {
        public AudioClip shootSound;
        public AudioClip takeOutSound;
        public AudioClip holsterSound;
        public AudioClip reloadSound;
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

        initialSwayPosition = transform.localPosition;

        //Set the shoot sound to audio source
        shootAudioSource.clip = SoundClips.shootSound;
    }


    private void Update()
    {
        if (gunSettings.isGamePaused) return;

        setUi.aim = isAiming;
        setUi.weaponImage.sprite = weaponSprite;
        setUi.setTexts(currentAmmo, totalAmmo);
        //Aiming
        //Toggle camera FOV when right click is held down
        if (touchController.aim && !isReloading && !isRunning && !isInspecting)
        {
            isAiming = true;

            gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
            aimFov, fovSpeed * Time.deltaTime);

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

            isAiming = false;

            anim.SetBool("Aim", false);

            soundHasPlayed = false;
        }
        //Aim end

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

        if (currentAmmo <= 0 && !outOfAmmo && totalAmmo > 0)
        {
            outOfAmmo = true;
            //Start reload when out of ammo
            StartCoroutine(AutoReload());
            StartCoroutine(ShowProjectileDelay());
        }
        if (totalAmmo == 0 && currentAmmo == 0)
        {
            Prefabs.projectile.SetActive(false);
        }
        else
        {
            Prefabs.projectile.SetActive(true);

        }

        //Shooting 
        if (touchController.fire && !outOfAmmo && !isReloading && !isInspecting && !isRunning && currentAmmo > 0)
        {
            touchController.fire = false;
            anim.Play("Fire", 0, 0f);

            muzzleParticles.Emit(1);

            //Spawn projectile prefab
            Instantiate(
                Prefabs.projectilePrefab,
                Spawnpoints.bulletSpawnPoint.transform.position,
                Spawnpoints.bulletSpawnPoint.transform.rotation);

            //Remove 1 bullet from ammo
            currentAmmo -= 1;

            shootAudioSource.clip = SoundClips.shootSound;
            shootAudioSource.Play();

            //Light flash start
            StartCoroutine(MuzzleFlashLight());

            if (!isAiming) //if not aiming
            {
                anim.Play("Fire", 0, 0f);

                muzzleParticles.Emit(1);

                if (enableSparks == true)
                {
                    //Emit random amount of spark particles
                    sparkParticles.Emit(Random.Range(1, 6));
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
                            sparkParticles.Emit(Random.Range(1, 6));
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
        }

        if (touchController.inspect)
        {
            anim.SetTrigger("Inspect");
            touchController.inspect = false;


        }



        if ((floatingJoystick.Horizontal != 0 && !isRunning) || (floatingJoystick.Vertical != 0 && !isRunning))
        {
            anim.SetBool("Walk", true);
        }
        else
        {
            anim.SetBool("Walk", false);
        }
        //Running when pressing down W and Left Shift key
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

    private IEnumerator ShowProjectileDelay()
    {
        //Disable projectile renderer
        projectileRenderer.GetComponent<SkinnedMeshRenderer>().enabled = false;
        //Wait for set amount of time
        yield return new WaitForSeconds(showProjectileDelay);
        //Enable projectile renderer
        projectileRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
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

    private IEnumerator AutoReload()
    {
        //Wait for set amount of time
        yield return new WaitForSeconds(autoReloadDelay);

        if (outOfAmmo == true)
        {
            //Play diff anim if out of ammo
            anim.Play("Reload", 0, 0f);

            mainAudioSource.clip = SoundClips.reloadSound;
            mainAudioSource.Play();
        }
        if (totalAmmo >= 1)
        {
            totalAmmo = totalAmmo - (maxAmmo - currentAmmo);
            currentAmmo = maxAmmo;

        }
        else
        {
            currentAmmo = currentAmmo + totalAmmo;
            totalAmmo = 0;
        }
        outOfAmmo = false;
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
        //Check if reloading
        //Check both animations
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Reload"))
        {
            isReloading = true;
        }
        else
        {
            isReloading = false;
        }

        //Check if inspecting weapon
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Inspect"))
        {
            isInspecting = true;
        }
        else
        {
            isInspecting = false;
        }
    }
}