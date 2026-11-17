using UnityEngine;
using StarterAssets;

// Alinea el modelo con la mira. El modelo sigue siendo un prefab, no se dibuja con código.
public class VistaArma : MonoBehaviour
{
    public StarterAssetsInputs entrada;
    Quaternion reposo;
    void Awake() { reposo = transform.localRotation; }
    void LateUpdate()
    {
        if (!entrada || !entrada.GetAimCamera()) return;
        Vector3 direccion = entrada.GetAimPoint() - transform.position;
        if (direccion.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(direccion, entrada.GetAimCamera().transform.up);
    }
}
