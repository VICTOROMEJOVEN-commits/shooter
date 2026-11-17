using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

// Dos espacios de inventario. Cada espacio conserva su propia munición.
public class ArsenalJugador : MonoBehaviour
{
    public DatosArma armaInicial;
    public Transform soporte;
    public Transform salida;
    public AudioSource sonido;
    public ParticleSystem fogonazo;
    public DatosArma[] armas = new DatosArma[2];
    public int[] balas = new int[2];
    public int[] reservas = new int[2];
    public int espacioActual;
    public bool Recargando { get; private set; }
    public DatosArma Actual => armas[espacioActual];
    StarterAssetsInputs entrada;
    InputAction disparar, recargar, cambiar;
    GameObject modeloActual;
    VentanaZombis[] ventanas;
    float proximoDisparo;
    Coroutine rutinaRecarga;

    void Awake()
    {
        entrada = GetComponent<StarterAssetsInputs>();
        entrada.disparoGestionado = true;
        ventanas = FindObjectsByType<VentanaZombis>(FindObjectsSortMode.None);
        disparar = new InputAction("Disparar", InputActionType.Button, "<Mouse>/leftButton");
        recargar = new InputAction("Recargar", InputActionType.Button, "<Keyboard>/r");
        cambiar = new InputAction("Cambiar arma", InputActionType.Button, "<Keyboard>/q");
        disparar.Enable(); recargar.Enable(); cambiar.Enable();
    }
    void Start() { RecibirArma(armaInicial); }
    void Update()
    {
        if (Time.timeScale == 0 || !Actual) return;
        if (cambiar.WasPressedThisFrame() && armas[1 - espacioActual]) Equipar(1 - espacioActual);
        if (recargar.WasPressedThisFrame()) Recargar();
        if (Actual.automatica ? disparar.IsPressed() : disparar.WasPressedThisFrame()) Disparar();
    }
    public void RecibirArma(DatosArma nueva)
    {
        if (!nueva) return;
        int destino = !armas[0] ? 0 : !armas[1] ? 1 : espacioActual;
        for (int i = 0; i < armas.Length; i++)
            if (armas[i] == nueva) { reservas[i] = nueva.reservaInicial; Equipar(i); return; }
        armas[destino] = nueva; balas[destino] = nueva.capacidad; reservas[destino] = nueva.reservaInicial;
        Equipar(destino);
    }
    public void Equipar(int espacio)
    {
        if (espacio < 0 || espacio >= armas.Length || !armas[espacio]) return;
        if (rutinaRecarga != null) StopCoroutine(rutinaRecarga);
        Recargando = false; rutinaRecarga = null; espacioActual = espacio;
        if (modeloActual) Destroy(modeloActual);
        modeloActual = Instantiate(Actual.modelo, soporte);
        modeloActual.transform.localPosition = Actual.posicionModelo;
        modeloActual.transform.localRotation = Quaternion.Euler(Actual.rotacionModelo);
        modeloActual.transform.localScale = Actual.escalaModelo;
        var boca = modeloActual.transform.Find("Salida");
        if (boca) salida = boca;
        proximoDisparo = Time.time + 0.2f;
    }
    public bool Disparar()
    {
        if (!Actual || Recargando || Time.time < proximoDisparo || (MatchGame.Instance && MatchGame.Instance.Finished)) return false;
        if (balas[espacioActual] <= 0) { Recargar(); return false; }
        if (!entrada.ProjectilePrefab || !salida) return false;
        balas[espacioActual]--; proximoDisparo = Time.time + Actual.intervalo;
        var camara = entrada.GetAimCamera();
        Vector3 objetivo = entrada.GetAimPoint();
        Vector3 origen = salida.position;
        // Si el arma atraviesa una pared cercana, el proyectil nace antes de ella.
        if (camara && Physics.Linecast(camara.transform.position, origen, out var pared, ~(1 << 2), QueryTriggerInteraction.Ignore)
            && !pared.transform.IsChildOf(transform) && !pared.collider.CompareTag("Projectile"))
            origen = pared.point - (origen - camara.transform.position).normalized * 0.12f;
        for (int i = 0; i < Actual.proyectiles; i++)
        {
            Vector3 direccion = (objetivo - origen).normalized;
            direccion = Quaternion.Euler(Random.Range(-Actual.dispersion, Actual.dispersion), Random.Range(-Actual.dispersion, Actual.dispersion), 0) * direccion;
            var proyectil = Instantiate(entrada.ProjectilePrefab, origen, Quaternion.LookRotation(direccion));
            var dano = proyectil.GetComponent<DanoProyectil>() ?? proyectil.AddComponent<DanoProyectil>(); dano.cantidad = Actual.dano;
            var cuerpo = proyectil.GetComponent<Rigidbody>();
            if (!cuerpo) { Destroy(proyectil); continue; }
            cuerpo.useGravity = false; cuerpo.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            cuerpo.linearVelocity = direccion * 65;
            foreach (var ventana in ventanas) if (ventana.limiteJugador) foreach (var col in proyectil.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(col, ventana.limiteJugador);
            foreach (var propio in GetComponentsInChildren<Collider>())
                foreach (var colisionador in proyectil.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(propio, colisionador);
            Destroy(proyectil, 4);
        }
        if (sonido && Actual.disparo) sonido.PlayOneShot(Actual.disparo);
        else CombatSfx.Shot(transform.position);
        if (fogonazo) { fogonazo.transform.position = salida.position; fogonazo.transform.rotation = salida.rotation; fogonazo.Play(); }
        return true;
    }
    public void Recargar()
    {
        if (!Actual || Recargando || balas[espacioActual] >= Actual.capacidad || reservas[espacioActual] <= 0) return;
        rutinaRecarga = StartCoroutine(Recarga());
    }
    IEnumerator Recarga()
    {
        Recargando = true;
        yield return new WaitForSeconds(Actual.recarga);
        int cantidad = Mathf.Min(Actual.capacidad - balas[espacioActual], reservas[espacioActual]);
        balas[espacioActual] += cantidad; reservas[espacioActual] -= cantidad;
        Recargando = false; rutinaRecarga = null;
    }
    void OnDestroy()
    {
        disparar?.Dispose(); recargar?.Dispose(); cambiar?.Dispose();
        if (entrada) entrada.disparoGestionado = false;
    }
}
