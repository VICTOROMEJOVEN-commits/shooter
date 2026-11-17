using UnityEngine;

// Los efectos se preparan como prefabs de Particle System en el editor.
public class EfectosCombate : MonoBehaviour
{
    public static EfectosCombate Instancia { get; private set; }
    public ParticleSystem impacto;
    public ParticleSystem danoEnemigo;
    void Awake() { Instancia = this; }
    public void Impacto(Vector3 punto, Vector3 normal)
    {
        if (impacto) Destroy(Instantiate(impacto, punto, Quaternion.LookRotation(normal)).gameObject, 2);
    }
    public void Dano(Vector3 punto)
    {
        if (danoEnemigo) Destroy(Instantiate(danoEnemigo, punto, Quaternion.identity).gameObject, 2);
    }
    void OnDestroy() { if (Instancia == this) Instancia = null; }
}
