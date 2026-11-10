using UnityEngine;
using StarterAssets;

[DefaultExecutionOrder(10000)]
public class BoneWeaponView : MonoBehaviour
{
    public Transform visual;
    public Transform muzzle;
    public GameObject muzzleFlash;
    public Light flashLight;
    StarterAssetsInputs inputs;
    Vector3 restPosition;
    float kick, flashUntil;

    void Awake()
    {
        if (visual) restPosition = visual.localPosition;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) inputs = player.GetComponent<StarterAssetsInputs>();
        if (!muzzle && inputs) muzzle = inputs.ProjectileSpawnPoint;
        if (muzzleFlash) muzzleFlash.SetActive(false);
        if (flashLight) flashLight.enabled = false;
    }
    public void OnWeaponFired()
    {
        CombatSfx.Shot(transform.position);
        kick = Mathf.Min(kick + 1, 1.7f);
        flashUntil = Time.time + 0.045f;
        if (muzzleFlash) muzzleFlash.SetActive(true);
        if (flashLight) flashLight.enabled = true;
    }
    void LateUpdate()
    {
        if (visual)
        {
            visual.localPosition = restPosition + new Vector3(0, 0.009f, -0.038f) * kick;
            if (inputs && inputs.GetAimCamera())
            {
                var camera = inputs.GetAimCamera();
                Vector3 boreOrigin = visual.TransformPoint(Vector3.up * 0.055f);
                Vector3 direction = inputs.GetAimPoint() - boreOrigin;
                if (direction.sqrMagnitude > 0.0001f)
                    visual.rotation = Quaternion.LookRotation(direction, camera.transform.up) * Quaternion.Euler(-5 * kick, 0, 1.2f * kick);
            }
        }
        kick = Mathf.Lerp(kick, 0, 1 - Mathf.Exp(-19 * Time.deltaTime));
        if (muzzleFlash && Time.time > flashUntil) muzzleFlash.SetActive(false);
        if (flashLight && Time.time > flashUntil) flashLight.enabled = false;
    }
}

