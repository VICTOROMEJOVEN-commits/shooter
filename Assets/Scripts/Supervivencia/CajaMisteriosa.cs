using System.Collections;
using UnityEngine;

public class CajaMisteriosa : MonoBehaviour
{
    public DatosArma[] catalogo;
    public int precio = 500;
    public Transform tapa;
    public Transform expositor;
    public Light luz;
    public AudioSource sonido;
    public AudioClip giro;
    public DatosArma Premio { get; private set; }
    public bool Girando { get; private set; }
    GameObject muestra;
    float vencePremio;
    Quaternion tapaCerrada;
    void Awake() { if (tapa) tapaCerrada = tapa.localRotation; }
    void Update()
    {
        if (muestra) muestra.transform.Rotate(Vector3.up, 45 * Time.deltaTime, Space.World);
        if (Premio && Time.time > vencePremio) Cerrar();
        if (luz) luz.intensity = Girando ? 2 + Mathf.Sin(Time.time * 12) : Premio ? 2 : 0.5f;
    }
    public string Mensaje => Girando ? "La caja está eligiendo..." : Premio ? "E · Recoger " + Premio.nombre : "E · Caja misteriosa (" + precio + " puntos)";
    public void Usar(ArsenalJugador jugador)
    {
        if (Girando) return;
        if (Premio) { jugador.RecibirArma(Premio); Cerrar(); return; }
        if (catalogo == null || catalogo.Length == 0 || !MatchGame.Instance.GastarPuntos(precio)) return;
        StartCoroutine(Sortear(jugador));
    }
    IEnumerator Sortear(ArsenalJugador jugador)
    {
        Girando = true;
        if (tapa) tapa.localRotation = tapaCerrada * Quaternion.Euler(-65, 0, 0);
        if (sonido && giro) sonido.PlayOneShot(giro);
        for (int i = 0; i < 15; i++) { Mostrar(catalogo[Random.Range(0, catalogo.Length)]); yield return new WaitForSeconds(0.2f); }
        // Evita entregar otra vez el arma que el jugador tiene en las manos.
        int elegido = Random.Range(0, catalogo.Length);
        if (catalogo.Length > 1 && catalogo[elegido] == jugador.Actual) elegido = (elegido + 1) % catalogo.Length;
        Premio = catalogo[elegido]; Mostrar(Premio); Girando = false; vencePremio = Time.time + 12;
    }
    void Mostrar(DatosArma arma)
    {
        if (muestra) Destroy(muestra);
        if (!arma || !arma.modelo) return;
        muestra = Instantiate(arma.modelo, expositor);
        muestra.transform.localPosition = Vector3.zero;
        muestra.transform.localRotation = Quaternion.Euler(0, 90, 0);
        muestra.transform.localScale = Vector3.one * 1.2f;
        foreach (var colisionador in muestra.GetComponentsInChildren<Collider>()) colisionador.enabled = false;
    }
    void Cerrar()
    {
        Premio = null;
        if (muestra) Destroy(muestra);
        if (tapa) tapa.localRotation = tapaCerrada;
    }
}
