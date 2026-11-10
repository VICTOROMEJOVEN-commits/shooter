using UnityEngine;

public class BoneWeaponView : MonoBehaviour
{
    public Transform visual;
    public GameObject muzzleFlash;
    public Light flashLight;
    Vector3 restPosition;
    Quaternion restRotation;
    float kick, flashUntil;
    void Awake()
    {
        if (visual) { restPosition = visual.localPosition; restRotation = visual.localRotation; }
        if (muzzleFlash) muzzleFlash.SetActive(false);
        if (flashLight) flashLight.enabled = false;
    }
    public void OnWeaponFired()
    {
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
            visual.localRotation = restRotation * Quaternion.Euler(-5 * kick, 0, 1.2f * kick);
        }
        kick = Mathf.Lerp(kick, 0, 1 - Mathf.Exp(-19 * Time.deltaTime));
        if (muzzleFlash && Time.time > flashUntil) muzzleFlash.SetActive(false);
        if (flashLight && Time.time > flashUntil) flashLight.enabled = false;
    }
}
