using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PumpShotgunScriptLPFP : MonoBehaviour
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



    [Header("Weapon Attachments (Only use one scope attachment)")]
    [Space(10)]
    //Toggle weapon attachments (loads at start)
    //Toggle scope 01
    public bool scope1;
    public Sprite scope1Texture;
    public float scope1TextureSize = 0.0045f;
    //Scope 01 camera fov
    [Range(5, 40)]
    public float scope1AimFOV = 10;
    [Space(10)]
    //Toggle scope 02
    public bool scope2;
    public Sprite scope2Texture;
    public float scope2TextureSize = 0.01f;
    //Scope 02 camera fov
    [Range(5, 40)]
    public float scope2AimFOV = 25;
    [Space(10)]
    //Toggle scope 03
    public bool scope3;
    public Sprite scope3Texture;
    public float scope3TextureSize = 0.006f;
    //Scope 03 camera fov
    [Range(5, 40)]
    public float scope3AimFOV = 20;
    [Space(10)]
    //Toggle scope 04
    public bool scope4;
    public Sprite scope4Texture;
    public float scope4TextureSize = 0.0025f;
    //Scope 04 camera fov
    [Range(5, 40)]
    public float scope4AimFOV = 12;
    [Space(10)]
    //Toggle iron sights
    public bool ironSights;
    public bool alwaysShowIronSights;
    //Iron sights camera fov
    [Range(5, 40)]
    public float ironSightsAimFOV = 16;

    //Weapon attachments components
    [System.Serializable]
    public class weaponAttachmentRenderers
    {
        [Header("Scope Model Renderers")]
        [Space(10)]
        //All attachment renderer components
        public SkinnedMeshRenderer scope1Renderer;
        public SkinnedMeshRenderer scope2Renderer;
        public SkinnedMeshRenderer scope3Renderer;
        public SkinnedMeshRenderer scope4Renderer;
        public SkinnedMeshRenderer ironSightsRenderer;
        [Header("Scope Sight Mesh Renderers")]
        [Space(10)]
        //Scope render meshes
        public GameObject scope1RenderMesh;
        public GameObject scope2RenderMesh;
        public GameObject scope3RenderMesh;
        public GameObject scope4RenderMesh;
        [Header("Scope Sight Sprite Renderers")]
        [Space(10)]
        //Scope sight textures
        public SpriteRenderer scope1SpriteRenderer;
        public SpriteRenderer scope2SpriteRenderer;
        public SpriteRenderer scope3SpriteRenderer;
        public SpriteRenderer scope4SpriteRenderer;
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
    //Chek if shooting


    //Check if running
    private bool isRunning;
    //Check if aiming
    private bool isAiming;
    //Check if walking
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
        public Transform[] bulletSpawnPoint;
        public bool useRandomBulletSpawnRotation;
        [Range(-10, 10)]
        public float bulletSpawnPointMinRotation = -5.0f;
        [Range(-10, 10)]
        public float bulletSpawnPointMaxRotation = 5.0f;

        public Transform grenadeSpawnPoint;
    }
    public spawnpoints Spawnpoints;

    [System.Serializable]
    public class soundClips
    {
        public AudioClip shootSound;
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
        currentAmmo = maxAmmo;

        muzzleFlashLight.enabled = false;


        //Weapon attachments
        //If scope1 is true

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

        if (scope1 == true && WeaponAttachmentRenderers.scope1Renderer != null)
        {
            //If scope1 is true, enable scope renderer
            WeaponAttachmentRenderers.scope1Renderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
            //Also enable the scope sight render mesh
            WeaponAttachmentRenderers.scope1RenderMesh.SetActive(true);
            //Set the scope sight texture
            WeaponAttachmentRenderers.scope1SpriteRenderer.GetComponent
                <SpriteRenderer>().sprite = scope1Texture;
            //Set the scope texture size
            WeaponAttachmentRenderers.scope1SpriteRenderer.transform.localScale = new Vector3
                (scope1TextureSize, scope1TextureSize, scope1TextureSize);
        }
        else if (WeaponAttachmentRenderers.scope1Renderer != null)
        {
            //If scope1 is false, disable scope renderer
            WeaponAttachmentRenderers.scope1Renderer.GetComponent<
            SkinnedMeshRenderer>().enabled = false;
            //Also disable the scope sight render mesh
            WeaponAttachmentRenderers.scope1RenderMesh.SetActive(false);
        }
        //If scope 2 is true
        if (scope2 == true && WeaponAttachmentRenderers.scope2Renderer != null)
        {
            //If scope2 is true, enable scope renderer
            WeaponAttachmentRenderers.scope2Renderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
            //Also enable the scope sight render mesh
            WeaponAttachmentRenderers.scope2RenderMesh.SetActive(true);
            //Set the scope sight texture
            WeaponAttachmentRenderers.scope2SpriteRenderer.GetComponent
            <SpriteRenderer>().sprite = scope2Texture;
            //Set the scope texture size
            WeaponAttachmentRenderers.scope2SpriteRenderer.transform.localScale = new Vector3
                (scope2TextureSize, scope2TextureSize, scope2TextureSize);
        }
        else if (WeaponAttachmentRenderers.scope2Renderer != null)
        {
            //If scope2 is false, disable scope renderer
            WeaponAttachmentRenderers.scope2Renderer.GetComponent
            <SkinnedMeshRenderer>().enabled = false;
            //Also disable the scope sight render mesh
            WeaponAttachmentRenderers.scope2RenderMesh.SetActive(false);
        }
        //If scope 3 is true
        if (scope3 == true && WeaponAttachmentRenderers.scope3Renderer != null)
        {
            //If scope3 is true, enable scope renderer
            WeaponAttachmentRenderers.scope3Renderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
            //Also enable the scope sight render mesh
            WeaponAttachmentRenderers.scope3RenderMesh.SetActive(true);
            //Set the scope sight texture
            WeaponAttachmentRenderers.scope3SpriteRenderer.GetComponent
            <SpriteRenderer>().sprite = scope3Texture;
            //Set the scope texture size
            WeaponAttachmentRenderers.scope3SpriteRenderer.transform.localScale = new Vector3
                (scope3TextureSize, scope3TextureSize, scope3TextureSize);
        }
        else if (WeaponAttachmentRenderers.scope3Renderer != null)
        {
            //If scope3 is false, disable scope renderer
            WeaponAttachmentRenderers.scope3Renderer.GetComponent
            <SkinnedMeshRenderer>().enabled = false;
            //Also disable the scope sight render mesh
            WeaponAttachmentRenderers.scope3RenderMesh.SetActive(false);
        }
        //If scope 4 is true
        if (scope4 == true && WeaponAttachmentRenderers.scope4Renderer != null)
        {
            //If scope4 is true, enable scope renderer
            WeaponAttachmentRenderers.scope4Renderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
            //Also enable the scope sight render mesh
            WeaponAttachmentRenderers.scope4RenderMesh.SetActive(true);
            //Set the scope sight texture
            WeaponAttachmentRenderers.scope4SpriteRenderer.GetComponent
            <SpriteRenderer>().sprite = scope4Texture;
            //Set the scope texture size
            WeaponAttachmentRenderers.scope4SpriteRenderer.transform.localScale = new Vector3
                (scope4TextureSize, scope4TextureSize, scope4TextureSize);
        }
        else if (WeaponAttachmentRenderers.scope4Renderer != null)
        {
            //If scope4 is false, disable scope renderer
            WeaponAttachmentRenderers.scope4Renderer.GetComponent
            <SkinnedMeshRenderer>().enabled = false;
            //Also enable the scope sight render mesh
            WeaponAttachmentRenderers.scope4RenderMesh.SetActive(false);
        }

        //If alwaysShowIronSights is true
        if (alwaysShowIronSights == true && WeaponAttachmentRenderers.ironSightsRenderer != null)
        {
            WeaponAttachmentRenderers.ironSightsRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
        }

        //If ironSights is true
        if (ironSights == true && WeaponAttachmentRenderers.ironSightsRenderer != null)
        {
            //If scope1 is true, enable scope renderer
            WeaponAttachmentRenderers.ironSightsRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
            //If always show iron sights is enabled, don't disable 
            //Do not use if iron sight renderer is not assigned in inspector
        }
        else if (!alwaysShowIronSights &&
            WeaponAttachmentRenderers.ironSightsRenderer != null)
        {
            //If scope1 is false, disable scope renderer
            WeaponAttachmentRenderers.ironSightsRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = false;
        }
        setUi.aim = isAiming;
        setUi.weaponImage.sprite = weaponSprite;

        setUi.setTexts(currentAmmo, totalAmmo);
        if (touchController.aim && !isReloading && !isRunning && !isInspecting)
        {
            if (ironSights == true)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    ironSightsAimFOV, fovSpeed * Time.deltaTime);
            }
            if (scope1 == true)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    scope1AimFOV, fovSpeed * Time.deltaTime);
            }
            if (scope2 == true)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    scope2AimFOV, fovSpeed * Time.deltaTime);
            }
            if (scope3 == true)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    scope3AimFOV, fovSpeed * Time.deltaTime);
            }
            if (scope4 == true)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    scope4AimFOV, fovSpeed * Time.deltaTime);
            }

            isAiming = true;

            //If iron sights are enabled, use normal aim
            if (ironSights == true)
            {
                anim.SetBool("Aim", true);
            }
            //If scope 1 is enabled, use scope 1 aim in animation
            if (scope1 == true)
            {
                anim.SetBool("Aim Scope 1", true);
            }
            //If scope 2 is enabled, use scope 2 aim in animation
            if (scope2 == true)
            {
                anim.SetBool("Aim Scope 2", true);
            }
            //If scope 3 is enabled, use scope 3 aim in animation
            if (scope3 == true)
            {
                anim.SetBool("Aim Scope 3", true);
            }
            //If scope 4 is enabled, use scope 4 aim in animation
            if (scope4 == true)
            {
                anim.SetBool("Aim Scope 4", true);
            }

            if (!soundHasPlayed)
            {
                mainAudioSource.clip = SoundClips.aimSound;
                mainAudioSource.Play();

                soundHasPlayed = true;
            }

            //If scope 1 is true, show scope sight texture when aiming
            if (scope1 == true)
            {
                WeaponAttachmentRenderers.scope1SpriteRenderer.GetComponent
                    <SpriteRenderer>().enabled = true;
            }
            //If scope 2 is true, show scope sight texture when aiming
            if (scope2 == true)
            {
                WeaponAttachmentRenderers.scope2SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = true;
            }
            //If scope 3 is true, show scope sight texture when aiming
            if (scope3 == true)
            {
                WeaponAttachmentRenderers.scope3SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = true;
            }
            //If scope 4 is true, show scope sight texture when aiming
            if (scope4 == true)
            {
                WeaponAttachmentRenderers.scope4SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = true;
            }
        }
        else
        {
            //When right click is released
            gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                defaultFov, fovSpeed * Time.deltaTime);

            isAiming = false;

            //If iron sights are enabled, use normal aim out
            if (ironSights == true)
            {
                anim.SetBool("Aim", false);
            }
            //If scope 1 is enabled, use scope 1 aim out animation
            if (scope1 == true)
            {
                anim.SetBool("Aim Scope 1", false);
            }
            //If scope 2 is enabled, use scope 2 aim out animation
            if (scope2 == true)
            {
                anim.SetBool("Aim Scope 2", false);
            }
            //If scope 3 is enabled, use scope 3 aim out animation
            if (scope3 == true)
            {
                anim.SetBool("Aim Scope 3", false);
            }

            //If scope 4 is enabled, use scope 4 aim out animation
            if (scope4 == true)
            {
                anim.SetBool("Aim Scope 4", false);
            }

            soundHasPlayed = false;

            //If scope 1 is true, disable scope sight texture when not aiming
            if (scope1 == true)
            {
                WeaponAttachmentRenderers.scope1SpriteRenderer.GetComponent
                    <SpriteRenderer>().enabled = false;
            }
            //If scope 2 is true, disable scope sight texture when not aiming
            if (scope2 == true)
            {
                WeaponAttachmentRenderers.scope2SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = false;
            }
            //If scope 3 is true, disable scope sight texture when not aiming
            if (scope3 == true)
            {
                WeaponAttachmentRenderers.scope3SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = false;
            }
            //If scope 4 is true, disable scope sight texture when not aiming
            if (scope4 == true)
            {
                WeaponAttachmentRenderers.scope4SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = false;
            }
        }
        //Aiming end

        //If randomize muzzleflash is true, genereate random int values
        if (randomMuzzleflash == true)
        {
            randomMuzzleflashValue = Random.Range(minRandomValue, maxRandomValue);
        }

        //Timescale settings
        //Change timescale to normal when 1 key is pressed


        //Continosuly check which animation 
        //is currently playing
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
            //Show out of ammo text

            //Toggle bool
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

        //Fire
        if (touchController.fire && !outOfAmmo && !isReloading && !isInspecting && !isRunning)
        {
            if (Time.time - lastFired > 1 / fireRate)
            {
                lastFired = Time.time;

                //Remove 1 bullet from ammo
                currentAmmo -= 1;

                shootAudioSource.clip = SoundClips.shootSound;
                shootAudioSource.Play();

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
                    if (ironSights == true)
                    {
                        anim.Play("Aim Fire", 0, 0f);
                    }
                    if (scope1 == true)
                    {
                        anim.Play("Aim Fire Scope 1", 0, 0f);
                    }
                    if (scope2 == true)
                    {
                        anim.Play("Aim Fire Scope 2", 0, 0f);
                    }
                    if (scope3 == true)
                    {
                        anim.Play("Aim Fire Scope 3", 0, 0f);
                    }
                    if (scope4 == true)
                    {
                        anim.Play("Aim Fire Scope 4", 0, 0f);
                    }

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

                //Bullet spawnpoints array
                for (int i = 0; i < Spawnpoints.bulletSpawnPoint.Length; i++)
                {
                    //If random bullet spawn point is enabled
                    // (Used for shotgun bullet spread)
                    if (Spawnpoints.useRandomBulletSpawnRotation == true)
                    {
                        //Rotate all bullet spawnpoints in array randomly based on min and max values
                        Spawnpoints.bulletSpawnPoint[i].transform.localRotation = Quaternion.Euler(
                            //Rotate random X
                            Random.Range(Spawnpoints.bulletSpawnPointMinRotation,
                                Spawnpoints.bulletSpawnPointMaxRotation),
                            //Rotate random Y
                            Random.Range(Spawnpoints.bulletSpawnPointMinRotation,
                                Spawnpoints.bulletSpawnPointMaxRotation),
                            //Don't rotate z
                            0);
                    }

                    //Spawn bullets from bullet spawnpoint positions using array
                    var bullet = (Transform)Instantiate(
                        Prefabs.bulletPrefab,
                        Spawnpoints.bulletSpawnPoint[i].transform.position,
                        Spawnpoints.bulletSpawnPoint[i].transform.rotation);

                    //Add velocity to the bullets
                    bullet.GetComponent<Rigidbody>().velocity =
                        bullet.transform.forward * bulletForce;
                }

                StartCoroutine(CasingDelay());
            }
        }

        if (touchController.inspect)
        {
            anim.SetTrigger("Inspect");
            touchController.inspect = false;


        }

        if (touchController.reload)
        {
            if (!isReloading && !isInspecting && currentAmmo < maxAmmo && totalAmmo > 0)
                StartCoroutine("reloadAnim");

            else
                touchController.reload = false;

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
        //Instantiate casing prefab at spawnpoint
        Instantiate(Prefabs.casingPrefab,
            Spawnpoints.casingSpawnPoint.transform.position,
            Spawnpoints.casingSpawnPoint.transform.rotation);
    }

    private IEnumerator AutoReload()
    {
        yield return new WaitForSeconds(0.5f);

        anim.Play("Reload Open", 0, 0f);
        yield return new WaitForSeconds(0.9f);
        for (int i = 0; i < (maxAmmo - currentAmmo);)
        {
            anim.Play("Insert Shell  ", 0, 0f);
            yield return new WaitForSeconds(0.9f);
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
            anim.Play("Insert Shell  ", 0, 0f);
            yield return new WaitForSeconds(0.9f);
            currentAmmo += 1;
            totalAmmo -= 1;

        }
        anim.Play("Reload Close ", 0, 0f);
        outOfAmmo = false;
        isReloading = false;
        touchController.reload = false;



    }
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