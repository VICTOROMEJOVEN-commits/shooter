using UnityEngine;

public class DanoProyectil : MonoBehaviour
{
    public int cantidad = 25;
    bool consumido;
    public bool Consumir()
    {
        if (consumido) return false;
        consumido = true;
        return true;
    }
    void OnCollisionEnter(Collision colision)
    {
        if (consumido || colision.collider.GetComponentInParent<ReciveDamage>()) return;
        consumido = true;
        var tabla = colision.collider.GetComponentInParent<TablaVentana>();
        if (tabla) tabla.ventana.RomperTabla();
        if (colision.contactCount > 0) EfectosCombate.Instancia?.Impacto(colision.GetContact(0).point, colision.GetContact(0).normal);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
