using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AutomaticGunScriptLPFP : MonoBehaviour
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

    public Sprite scope1Texture;
    public float scope1TextureSize = 0.0045f;
    //Scope 01 camera fov
    [Range(5, 40)]
    public float scope1AimFOV = 10;
    [Space(10)]

    public Sprite scope2Texture;
    public float scope2TextureSize = 0.01f;
    //Scope 02 camera fov
    [Range(5, 40)]
    public float scope2AimFOV = 25;
    [Space(10)]

    public Sprite scope3Texture;
    public float scope3TextureSize = 0.006f;
    //Scope 03 camera fov
    [Range(5, 40)]
    public float scope3AimFOV = 20;
    [Space(10)]

    public Sprite scope4Texture;
    public float scope4TextureSize = 0.0025f;
    //Scope 04 camera fov
    [Range(5, 40)]
    public float scope4AimFOV = 12;
    [Space(10)]
    //Toggle iron sights
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
        public SkinnedMeshRenderer silencerRenderer;
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
    [Header("Weapon Settings")]
    //How fast the weapon fires, higher value means faster rate of fire
    [Tooltip("How fast the weapon fires, higher value means faster rate of fire.")]
    public float fireRate;
    //Eanbles auto reloading when out of ammo
    [Tooltip("Enables auto reloading when out of ammo.")]
    public bool autoReload = true;
    //Delay between shooting last bullet and reloading
    public float autoReloadDelay;
    //Check if reloading
    private bool isReloading;

    //Check if running
    private bool isRunning;
    //Check if aiming
    private bool isAiming;

    private bool isInspecting;

    //How much ammo is currently left

    //Totalt amount of ammo
    [Tooltip("How much ammo the weapon should have.")]
    public int maxAmmo, totalAmmo, currentAmmo;
    //Check if out of ammo
    private bool outOfAmmo;

    [Header("Bullet Settings")]
    //Bullet
    [Tooltip("How much force is applied to the bullet when shooting.")]
    public float bulletForce = 400.0f;
    [Tooltip("How long after reloading that the bullet model becomes visible " +
        "again, only used for out of ammo reload animations.")]
    public float showBulletInMagDelay = 0.6f;
    [Tooltip("The bullet model inside the mag, not used for all weapons.")]
    public SkinnedMeshRenderer bulletInMagRenderer;

    [Header("Grenade Settings")]
    public float grenadeSpawnDelay = 0.35f;

    [Header("Muzzleflash Settings")]
    public bool randomMuzzleflash = false;
    //min should always bee 1
    private int minRandomValue = 1;

    [Range(2, 25)]
    public int maxRandomValue = 5;

    private int randomMuzzleflashValue;

    public bool enableMuzzleflash = true;
    public ParticleSystem muzzleParticles;
    public bool enableSparks = true;
    public ParticleSystem sparkParticles;
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("Muzzleflash Light Settings")]
    public Light muzzleflashLight;
    public float lightDuration = 0.02f;

    [Header("Audio Source")]
    //Main audio source
    public AudioSource mainAudioSource;
    //Audio source used for shoot sound
    public AudioSource shootAudioSource;

    Vector3 rot;
    public float minX, maxX, minY, maxY;


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
        public AudioClip reloadSoundOutOfAmmo;
        public AudioClip reloadSoundAmmoLeft;
        public AudioClip aimSound;
    }
    public soundClips SoundClips;

    private bool soundHasPlayed = false;

    ///////////////////////////////////////////////////////////////////////////////////////////////////


    private void Awake()
    {

        //Set the animator component
        anim = GetComponent<Animator>();
        //Set current ammo to total ammo value
        currentAmmo = maxAmmo;

        muzzleflashLight.enabled = false;


    }

    private void Start()
    {

        //Weapon sway
        initialSwayPosition = transform.localPosition;

        //Set the shoot sound to audio source
        shootAudioSource.clip = SoundClips.shootSound;
    }

    private void LateUpdate()
    {
        /*
        //Weapon sway
        if (weaponSway == true)
        {
            float movementX = -Input.GetAxis("Mouse X") * swayAmount;
            float movementY = -Input.GetAxis("Mouse Y") * swayAmount;
            //Clamp movement to min and max values
            movementX = Mathf.Clamp
                (movementX, -maxSwayAmount, maxSwayAmount);
            movementY = Mathf.Clamp
                (movementY, -maxSwayAmount, maxSwayAmount);
            //Lerp local pos
            Vector3 finalSwayPosition = new Vector3
                (movementX, movementY, 0);
            transform.localPosition = Vector3.Lerp
                (transform.localPosition, finalSwayPosition +
                    initialSwayPosition, Time.deltaTime * swaySmoothValue);
        }
        */
    }

    private void Update()
    {
        if (gunSettings.isGamePaused) return;
        if (gunSettings.scopeId == 1 && WeaponAttachmentRenderers.scope1Renderer != null)
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
        if (gunSettings.scopeId == 2 && WeaponAttachmentRenderers.scope2Renderer != null)
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
        if (gunSettings.scopeId == 3 && WeaponAttachmentRenderers.scope3Renderer != null)
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
        if (gunSettings.scopeId == 4 && WeaponAttachmentRenderers.scope4Renderer != null)
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
        if (gunSettings.scopeId == 0 && WeaponAttachmentRenderers.ironSightsRenderer != null)
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
        //If silencer is true and assigned in the inspector
        if (gunSettings.silencerId == 1 &&
            WeaponAttachmentRenderers.silencerRenderer != null)
        {
            //If scope1 is true, enable scope renderer
            WeaponAttachmentRenderers.silencerRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
        }
        else if (WeaponAttachmentRenderers.silencerRenderer != null)
        {
            //If scope1 is false, disable scope renderer
            WeaponAttachmentRenderers.silencerRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = false;
        }


        setUi.aim = isAiming;
        setUi.weaponImage.sprite = weaponSprite;
        setUi.setTexts(currentAmmo, totalAmmo);

        //Aiming
        //Toggle camera FOV when right click is held down
        if (touchController.aim && !isReloading && !isRunning && !isInspecting)
        {

            if (gunSettings.scopeId == 0 == true)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    ironSightsAimFOV, fovSpeed * Time.deltaTime);
            }
            if (gunSettings.scopeId == 1)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    scope1AimFOV, fovSpeed * Time.deltaTime);
            }
            if (gunSettings.scopeId == 2)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    scope2AimFOV, fovSpeed * Time.deltaTime);
            }
            if (gunSettings.scopeId == 3)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    scope3AimFOV, fovSpeed * Time.deltaTime);
            }
            if (gunSettings.scopeId == 4)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    scope4AimFOV, fovSpeed * Time.deltaTime);
            }

            isAiming = true;

            //If iron sights are enabled, use normal aim
            if (gunSettings.scopeId == 0)
            {
                anim.SetBool("Aim", true);
            }
            //If scope 1 is enabled, use scope 1 aim in animation
            if (gunSettings.scopeId == 1)
            {
                anim.SetBool("Aim Scope 1", true);
            }
            //If scope 2 is enabled, use scope 2 aim in animation
            if (gunSettings.scopeId == 2)
            {
                anim.SetBool("Aim Scope 2", true);
            }
            //If scope 3 is enabled, use scope 3 aim in animation
            if (gunSettings.scopeId == 3)
            {
                anim.SetBool("Aim Scope 3", true);
            }
            //If scope 4 is enabled, use scope 4 aim in animation
            if (gunSettings.scopeId == 4)
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
            if (gunSettings.scopeId == 1)
            {
                WeaponAttachmentRenderers.scope1SpriteRenderer.GetComponent
                    <SpriteRenderer>().enabled = true;
            }
            //If scope 2 is true, show scope sight texture when aiming
            if (gunSettings.scopeId == 2)
            {
                WeaponAttachmentRenderers.scope2SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = true;
            }
            //If scope 3 is true, show scope sight texture when aiming
            if (gunSettings.scopeId == 3)
            {
                WeaponAttachmentRenderers.scope3SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = true;
            }
            //If scope 4 is true, show scope sight texture when aiming
            if (gunSettings.scopeId == 4)
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
            if (gunSettings.scopeId == 0)
            {
                anim.SetBool("Aim", false);
            }
            //If scope 1 is enabled, use scope 1 aim out animation
            if (gunSettings.scopeId == 1)
            {
                anim.SetBool("Aim Scope 1", false);
            }
            //If scope 2 is enabled, use scope 2 aim out animation
            if (gunSettings.scopeId == 2)
            {
                anim.SetBool("Aim Scope 2", false);
            }
            //If scope 3 is enabled, use scope 3 aim out animation
            if (gunSettings.scopeId == 3)
            {
                anim.SetBool("Aim Scope 3", false);
            }

            //If scope 4 is enabled, use scope 4 aim out animation
            if (gunSettings.scopeId == 4)
            {
                anim.SetBool("Aim Scope 4", false);
            }

            soundHasPlayed = false;

            //If scope 1 is true, disable scope sight texture when not aiming
            if (gunSettings.scopeId == 1)
            {
                WeaponAttachmentRenderers.scope1SpriteRenderer.GetComponent
                    <SpriteRenderer>().enabled = false;
            }
            //If scope 2 is true, disable scope sight texture when not aiming
            if (gunSettings.scopeId == 2)
            {
                WeaponAttachmentRenderers.scope2SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = false;
            }
            //If scope 3 is true, disable scope sight texture when not aiming
            if (gunSettings.scopeId == 3)
            {
                WeaponAttachmentRenderers.scope3SpriteRenderer.GetComponent
                <SpriteRenderer>().enabled = false;
            }
            //If scope 4 is true, disable scope sight texture when not aiming
            if (gunSettings.scopeId == 4)
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


        AnimationCheck();

        //Play knife attack 1 animation when Q key is pressed
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
                StartCoroutine(AutoReload());
            }
        }
        else
        {

            //Toggle bool
            outOfAmmo = false;
            //anim.SetBool ("Out Of Ammo", false);
        }
        rot = transform.localRotation.eulerAngles;
        if (rot.x != 0 || rot.y != 0)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 3);
        }
        //AUtomatic fire
        //Left click hold 
        if (touchController.fire && !outOfAmmo && !isReloading && !isInspecting && !isRunning)
        {
            if (Time.time - lastFired > 1 / fireRate)
            {
                recoil();
                lastFired = Time.time;

                //Remove 1 bullet from ammo
                currentAmmo -= 1;

                //If silencer is enabled, play silencer shoot sound, don't play if there is nothing assigned in the inspector
                if (gunSettings.silencerId == 1 && WeaponAttachmentRenderers.silencerRenderer != null)
                {
                    shootAudioSource.clip = SoundClips.silencerShootSound;
                    shootAudioSource.Play();
                }
                //If silencer is not enabled, play default shoot sound
                else
                {
                    shootAudioSource.clip = SoundClips.shootSound;
                    shootAudioSource.Play();
                }

                if (!isAiming) //if not aiming
                {
                    anim.Play("Fire", 0, 0f);
                    //If random muzzle is false
                    if (!randomMuzzleflash &&
                        enableMuzzleflash == true && gunSettings.silencerId == 0)
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
                            if (enableMuzzleflash == true && gunSettings.silencerId == 0)
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
                    if (gunSettings.scopeId == 0)
                    {
                        anim.Play("Aim Fire", 0, 0f);
                    }
                    if (gunSettings.scopeId == 1)
                    {
                        anim.Play("Aim Fire Scope 1", 0, 0f);
                    }
                    if (gunSettings.scopeId == 2)
                    {
                        anim.Play("Aim Fire Scope 2", 0, 0f);
                    }
                    if (gunSettings.scopeId == 3)
                    {
                        anim.Play("Aim Fire Scope 3", 0, 0f);
                    }
                    if (gunSettings.scopeId == 4)
                    {
                        anim.Play("Aim Fire Scope 4", 0, 0f);
                    }

                    //If random muzzle is false
                    if (!randomMuzzleflash && gunSettings.silencerId == 0)
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
                            if (enableMuzzleflash == true && gunSettings.silencerId == 0)
                            {
                                muzzleParticles.Emit(1);
                                //Light flash start
                                StartCoroutine(MuzzleFlashLight());
                            }
                        }
                    }
                }

                //Spawn bullet from bullet spawnpoint
                var bullet = (Transform)Instantiate(
                    Prefabs.bulletPrefab,
                    Spawnpoints.bulletSpawnPoint.transform.position,
                    Spawnpoints.bulletSpawnPoint.transform.rotation);

                //Add velocity to the bullet
                bullet.GetComponent<Rigidbody>().velocity =
                    bullet.transform.forward * bulletForce;

                //Spawn casing prefab at spawnpoint
                Instantiate(Prefabs.casingPrefab,
                    Spawnpoints.casingSpawnPoint.transform.position,
                    Spawnpoints.casingSpawnPoint.transform.rotation);
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
                Reload();
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

    private IEnumerator AutoReload()
    {
        //Wait set amount of time
        yield return new WaitForSeconds(autoReloadDelay);

        if (outOfAmmo == true)
        {
            //Play diff anim if out of ammo
            anim.Play("Reload Out Of Ammo", 0, 0f);

            mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
            mainAudioSource.Play();

            //If out of ammo, hide the bullet renderer in the mag
            //Do not show if bullet renderer is not assigned in inspector
            if (bulletInMagRenderer != null)
            {
                bulletInMagRenderer.GetComponent
                <SkinnedMeshRenderer>().enabled = false;
                //Start show bullet delay
                StartCoroutine(ShowBulletInMag());
            }
        }
        if (totalAmmo >= maxAmmo - currentAmmo)
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

    //Reload
    private void Reload()
    {

        if (outOfAmmo == true)
        {
            //Play diff anim if out of ammo
            anim.Play("Reload Out Of Ammo", 0, 0f);

            mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
            mainAudioSource.Play();

            //If out of ammo, hide the bullet renderer in the mag
            //Do not show if bullet renderer is not assigned in inspector
            if (bulletInMagRenderer != null)
            {
                bulletInMagRenderer.GetComponent
                <SkinnedMeshRenderer>().enabled = false;
                //Start show bullet delay
                StartCoroutine(ShowBulletInMag());
            }
        }
        else
        {
            //Play diff anim if ammo left
            anim.Play("Reload Ammo Left", 0, 0f);

            mainAudioSource.clip = SoundClips.reloadSoundAmmoLeft;
            mainAudioSource.Play();

            //If reloading when ammo left, show bullet in mag
            //Do not show if bullet renderer is not assigned in inspector
            if (bulletInMagRenderer != null)
            {
                bulletInMagRenderer.GetComponent
                <SkinnedMeshRenderer>().enabled = true;
            }
        }
        if (totalAmmo >= maxAmmo - currentAmmo)
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
        touchController.reload = false;
    }

    public void recoil()
    {
        float recX = Random.Range(minX, maxX);
        float recY = Random.Range(minY, maxY);
        transform.localRotation = Quaternion.Euler(rot.x - recY, rot.y + recX, rot.z);
    }

    //Enable bullet in mag renderer after set amount of time
    private IEnumerator ShowBulletInMag()
    {

        //Wait set amount of time before showing bullet in mag
        yield return new WaitForSeconds(showBulletInMagDelay);
        bulletInMagRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
    }

    //Show light when shooting, then disable after set amount of time
    private IEnumerator MuzzleFlashLight()
    {

        muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        muzzleflashLight.enabled = false;
    }

    //Check current animation playing
    private void AnimationCheck()
    {

        //Check if reloading
        //Check both animations
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left"))
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