using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class VentanaZombis : MonoBehaviour
{
    public Transform aparicion;
    public Transform entrada;
    public Transform salida;
    public GameObject[] tablas;
    public BoxCollider limiteJugador;
    public AudioSource sonido;
    public AudioClip romper;
    public AudioClip reparar;
    public bool PuedeRecibir => ocupante == null;
    EnemyNavigation ocupante;
    float proximaReparacion;
    int reparacionesPremiadas;
    public int TablasRestantes
    {
        get { int cantidad = 0; foreach (var tabla in tablas) if (tabla && tabla.activeSelf) cantidad++; return cantidad; }
    }
    public IEnumerator Entrar(EnemyNavigation zombi)
    {
        ocupante = zombi;
        if (zombi.agent) zombi.agent.enabled = false;
        Vector3 inicio = zombi.transform.position;
        float recorrido = 0;
        while (recorrido < 1 && !zombi.isDead)
        {
            recorrido += Time.deltaTime / 1.5f;
            zombi.transform.position = Vector3.Lerp(inicio, entrada.position, recorrido);
            zombi.transform.rotation = entrada.rotation;
            yield return null;
        }
        if (zombi.enemyAnimator) zombi.enemyAnimator.SetBool("IsMoving", false);
        while (TablasRestantes > 0 && !zombi.isDead)
        {
            if (zombi.enemyAnimator) zombi.enemyAnimator.SetTrigger("Golpear");
            yield return new WaitForSeconds(0.65f);
            if (zombi.isDead) break;
            RomperTabla();
            yield return new WaitForSeconds(0.65f);
        }
        if (!zombi.isDead)
        {
            if (zombi.enemyAnimator) zombi.enemyAnimator.SetBool("SaltarVentana", true);
            recorrido = 0;
            while (recorrido < 1 && !zombi.isDead)
            {
                recorrido += Time.deltaTime / 1.1f;
                zombi.transform.position = Vector3.Lerp(entrada.position, salida.position, Mathf.SmoothStep(0, 1, recorrido)) + Vector3.up * Mathf.Sin(Mathf.Clamp01(recorrido) * Mathf.PI) * 0.45f;
                yield return null;
            }
            if (zombi.enemyAnimator) zombi.enemyAnimator.SetBool("SaltarVentana", false);
            if (!zombi.isDead && zombi.agent && NavMesh.SamplePosition(salida.position, out var punto, 2, zombi.agent.areaMask))
            {
                zombi.transform.position = punto.position;
                zombi.agent.enabled = true;
                zombi.agent.Warp(punto.position);
            }
        }
        zombi.Entrando = false; ocupante = null;
    }
    public void RomperTabla()
    {
        for (int i = tablas.Length - 1; i >= 0; i--)
        {
            if (!tablas[i] || !tablas[i].activeSelf) continue;
            tablas[i].SetActive(false);
            if (sonido && romper) sonido.PlayOneShot(romper);
            EfectosCombate.Instancia?.Impacto(transform.position + Vector3.up, transform.forward);
            return;
        }
    }
    public bool RepararTabla()
    {
        if (Time.time < proximaReparacion || (ocupante && ocupante.Entrando && TablasRestantes == 0)) return false;
        foreach (var tabla in tablas)
        {
            if (!tabla || tabla.activeSelf) continue;
            tabla.SetActive(true); proximaReparacion = Time.time + 0.8f;
            if (reparacionesPremiadas < 30) { MatchGame.Instance?.SumarPuntos(10); reparacionesPremiadas++; }
            if (sonido && reparar) sonido.PlayOneShot(reparar);
            return true;
        }
        return false;
    }
}
