using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletPoint;
    [SerializeField] private float timeBetweenShots = 0.10f;

    private float nextShootTime;

    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private float bulletsInMagazine;

    private float bulletsLeft;

    [SerializeField] private float reloadTime;
    private bool isRealoding;

    [SerializeField] private float aimDistance = 100f;
    [SerializeField] private float minAimDistance = 2f;
    [SerializeField] private LayerMask aimMask;

    [SerializeField] private Camera fpsCam;
    [SerializeField] private GameObject muzzlePrefab;

    [SerializeField] private AudioClip bulletSound;
    [SerializeField] private float bulletSoundVolume;
    [SerializeField] private Vector2 shootPitch;

    
    private void Awake()
    {
        bulletsLeft = bulletsInMagazine;
        UpdateText();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }

        if (Input.GetKey(KeyCode.Mouse0) && CanShoot())
        {
            Shoot();
        }
    }

    private void TryReload()
    {
        if (isRealoding || bulletsLeft >= bulletsInMagazine) return;
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isRealoding = true;
        yield return new WaitForSeconds(reloadTime);

        bulletsLeft = bulletsInMagazine;
        isRealoding = false;
        UpdateText();
    }


    private bool CanShoot()
    {
        return bulletsLeft > 0 && Time.time >= nextShootTime;
    }

    private void Shoot()
    {
        Instantiate(muzzlePrefab, bulletPoint.position, bulletPoint.rotation);
        Instantiate(bulletPrefab, bulletPoint.position, GetAimRotation());

        bulletsLeft--;
        nextShootTime = Time.time + timeBetweenShots;
        UpdateText();

        float pith = Random.Range(shootPitch.x, shootPitch.y);
        AudioManager.Instance.PlaySFX(bulletSound, bulletSoundVolume, pith);
    }

    private void UpdateText()
    {
        ammoText.SetText(bulletsLeft + " / " + bulletsInMagazine);
    }

    private Quaternion GetAimRotation()
    {
        Ray aimRay = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float distance = aimDistance;

        if (Physics.Raycast(aimRay, out RaycastHit hit, aimDistance, aimMask))
        {
            distance = Mathf.Max(hit.distance, minAimDistance);
        }

        Vector3 aimPoint = aimRay.GetPoint(distance);
        return Quaternion.LookRotation(aimPoint - bulletPoint.position);
    }

}
