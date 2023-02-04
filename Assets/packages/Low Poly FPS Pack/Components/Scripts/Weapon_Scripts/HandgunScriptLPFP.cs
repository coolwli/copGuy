using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HandgunScriptLPFP : MonoBehaviour
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

    [Header("UI Weapon Name")]
    [Tooltip("Name of the current weapon, shown in the game UI.")]
    public string weaponName;
    private string storedWeaponName;

    [Header("Weapon Attachments (Only use one scope attachment)")]
    [Space(10)]

    public Sprite scope2Texture;
    public float scope2TextureSize = 0.01f;
    //Scope 02 camera fov
    [Range(5, 40)]
    public float scope2AimFOV = 25;
    [Space(10)]
    //Toggle scope 03
    public Sprite scope3Texture;
    public float scope3TextureSize = 0.025f;
    //Scope 03 camera fov
    [Range(5, 40)]
    public float scope3AimFOV = 20;
    [Space(10)]
    //Toggle iron sights
    public bool alwaysShowIronSights;
    //Iron sights camera fov
    [Range(5, 40)]
    public float ironSightsAimFOV = 16;

    [System.Serializable]
    public class weaponAttachmentRenderers
    {
        [Header("Scope Model Renderers")]
        [Space(10)]
        //All attachment renderer components
        public SkinnedMeshRenderer scope2Renderer;
        public SkinnedMeshRenderer scope3Renderer;
        public SkinnedMeshRenderer ironSightsRenderer;
        public SkinnedMeshRenderer silencerRenderer;
        [Header("Scope Sight Mesh Renderers")]
        [Space(10)]
        //Scope render meshes
        public GameObject scope2RenderMesh;
        public GameObject scope3RenderMesh;
        [Header("Scope Sight Sprite Renderers")]
        [Space(10)]
        //Scope sight textures
        public SpriteRenderer scope2SpriteRenderer;
        public SpriteRenderer scope3SpriteRenderer;
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

    [Header("Weapon Settings")]

    public float sliderBackTimer = 1.58f;
    private bool hasStartedSliderBack;

    //Eanbles auto reloading when out of ammo
    [Tooltip("Enables auto reloading when out of ammo.")]
    public bool autoReload;
    //Delay between shooting last bullet and reloading
    public float autoReloadDelay;
    //Check if reloading
    private bool isReloading;

    //Check if running
    private bool isRunning;
    //Check if aiming
    private bool isAiming;
    //Check if walking
    private bool isWalking;
    //Check if inspecting weapon
    private bool isInspecting;

    //How much ammo is currently left

    //Totalt amount of ammo
    [Tooltip("How much ammo the weapon should have.")]
    public int maxAmmo, totalAmmo, currentAmmo;
    private bool outOfAmmo;

    [Header("Bullet Settings")]
    //Bullet
    [Tooltip("How much force is applied to the bullet when shooting.")]
    public float bulletForce = 400;
    [Tooltip("How long after reloading that the bullet model becomes visible " +
        "again, only used for out of ammo reload aniamtions.")]
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
        //Casing spawn point array
        public Transform casingSpawnPoint;
        //Bullet prefab spawn from this point
        public Transform bulletSpawnPoint;
        //Grenade prefab spawn from this point
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

    private void Awake()
    {
        //Set the animator component
        anim = GetComponent<Animator>();

        currentAmmo = maxAmmo;
        muzzleflashLight.enabled = false;

        //Weapon attachments
        //If scope 2 is true

    }

    private void Start()
    {
        //Save the weapon name


        //Weapon sway
        initialSwayPosition = transform.localPosition;

        //Set the shoot sound to audio source
        shootAudioSource.clip = SoundClips.shootSound;
    }



    private void Update()
    {
        if (gunSettings.isGamePaused) return;

        if (gunSettings.scopeId == 2)
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
        else
        {
            //If scope2 is false, disable scope renderer
            WeaponAttachmentRenderers.scope2Renderer.GetComponent
            <SkinnedMeshRenderer>().enabled = false;
            //Also disable the scope sight render mesh
            WeaponAttachmentRenderers.scope2RenderMesh.SetActive(false);
        }
        //If scope 3 is true
        if (gunSettings.scopeId == 3)
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
        else
        {
            //If scope3 is false, disable scope renderer
            WeaponAttachmentRenderers.scope3Renderer.GetComponent
            <SkinnedMeshRenderer>().enabled = false;
            //Also disable the scope sight render mesh
            WeaponAttachmentRenderers.scope3RenderMesh.SetActive(false);
        }

        //If alwaysShowIronSights is true
        if (alwaysShowIronSights == true)
        {
            WeaponAttachmentRenderers.ironSightsRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
        }

        //If ironSights is true
        if (gunSettings.scopeId == 0)
        {
            //If scope1 is true, enable scope renderer
            WeaponAttachmentRenderers.ironSightsRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
            //If always show iron sights is enabled, don't disable 
            //Do not use if iron sight renderer is not assigned in inspector
        }
        else
        {
            //If scope1 is false, disable scope renderer
            WeaponAttachmentRenderers.ironSightsRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = false;
        }
        //If silencer is true and assigned in the inspector
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
        setUi.aim = isAiming;
        setUi.weaponImage.sprite = weaponSprite;
        setUi.setTexts(currentAmmo, totalAmmo);
        //Toggle camera FOV when right click is held down
        if (touchController.aim && !isReloading && !isRunning && !isInspecting)
        {
            if (gunSettings.scopeId == 0)
            {
                gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                    ironSightsAimFOV, fovSpeed * Time.deltaTime);
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

            isAiming = true;

            //If iron sights are enabled, use normal aim
            if (gunSettings.scopeId == 0)
            {
                anim.SetBool("Aim", true);
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

            if (!soundHasPlayed)
            {
                mainAudioSource.clip = SoundClips.aimSound;
                mainAudioSource.Play();

                soundHasPlayed = true;
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

            soundHasPlayed = false;

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

            //Toggle bool
            outOfAmmo = true;
            //Auto reload if true
            if (autoReload == true && !isReloading && totalAmmo > 0)
            {
                StartCoroutine(AutoReload());
            }

            //Set slider back
            anim.SetBool("Out Of Ammo Slider", true);
            //Increase layer weight for blending to slider back pose
            anim.SetLayerWeight(1, 1.0f);
        }
        else
        {

            outOfAmmo = false;
            //anim.SetBool ("Out Of Ammo", false);
            anim.SetLayerWeight(1, 0.0f);
        }

        //Shooting 
        if (touchController.fire && !outOfAmmo && !isReloading && !isInspecting && !isRunning)
        {
            touchController.fire = false;
            anim.Play("Fire", 0, 0f);
            if (gunSettings.silencerId == 0)
            {
                muzzleParticles.Emit(1);
            }

            //Remove 1 bullet from ammo
            currentAmmo -= 1;

            //If silencer is enabled, play silencer shoot sound
            if (gunSettings.silencerId == 1)
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

            //Light flash start
            StartCoroutine(MuzzleFlashLight());

            if (!isAiming) //if not aiming
            {
                anim.Play("Fire", 0, 0f);
                if (gunSettings.silencerId == 0)
                {
                    muzzleParticles.Emit(1);
                }

                if (enableSparks == true)
                {
                    //Emit random amount of spark particles
                    sparkParticles.Emit(Random.Range(1, 6));
                }
            }
            else //if aiming
            {
                if (gunSettings.scopeId == 0)
                {
                    anim.Play("Aim Fire", 0, 0f);
                }
                if (gunSettings.scopeId == 2)
                {
                    anim.Play("Aim Fire Scope 2", 0, 0f);
                }
                if (gunSettings.scopeId == 3)
                {
                    anim.Play("Aim Fire Scope 3", 0, 0f);
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
                            sparkParticles.Emit(Random.Range(1, 6));
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

            //Spawn bullet at bullet spawnpoint
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


        //Toggle weapon holster when pressing E key
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

    private IEnumerator HandgunSliderBackDelay()
    {
        //Wait set amount of time
        yield return new WaitForSeconds(sliderBackTimer);
        //Set slider back
        anim.SetBool("Out Of Ammo Slider", false);
        //Increase layer weight for blending to slider back pose
        anim.SetLayerWeight(1, 0.0f);

        hasStartedSliderBack = false;
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

        if (!hasStartedSliderBack)
        {
            hasStartedSliderBack = true;

            StartCoroutine(HandgunSliderBackDelay());
        }
        //Wait for set amount of time
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