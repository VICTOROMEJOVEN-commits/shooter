using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

// Dos espacios de inventario. Cada espacio conserva su propia munición.
public class BoneWeaponView : MonoBehaviour
{
    // Se conserva el archivo original del arma. Sus ajustes están agrupados aquí.
    [Header("Vista original")]
    public Transform visual;
    public Transform muzzle;
    public GameObject muzzleFlash;
    public Light flashLight;
    [Header("Armas disponibles")]
    public DatosArma[] catalogo;
    public GameObject vistaPrevia;
    public Transform soporte;
    public Transform salida;
    public AudioSource sonido;
    public ParticleSystem fogonazo;
    [System.NonSerialized] public DatosArma[] armas = new DatosArma[2];
    [System.NonSerialized] public int[] balas = new int[2];
    [System.NonSerialized] public int[] reservas = new int[2];
    public int espacioActual;
    public bool Recargando { get; private set; }
    public DatosArma Actual => armas[espacioActual];
    StarterAssetsInputs entrada;
    InputAction disparar, recargar, cambiar, apuntar;
    Camera camaraArma;
    Unity.Cinemachine.CinemachineCamera camaraSeguimiento;
    float campoVisionOriginal;
    GameObject modeloActual;
    DatosVentana[] ventanas = new DatosVentana[0];
    public string MensajeInteraccion { get; private set; }
    InputAction usar;
    float retroceso, destelloHasta;
    Vector3 posicionOriginal;
    float proximoDisparo;
    Coroutine rutinaRecarga;

    void Awake()
    {
        entrada = GetComponent<StarterAssetsInputs>();
        if (!entrada) { var jugador = GameObject.FindGameObjectWithTag("Player"); if (jugador) entrada = jugador.GetComponent<StarterAssetsInputs>(); }
        if (visual) posicionOriginal = visual.localPosition;
        if (muzzleFlash) muzzleFlash.SetActive(false);
        if (flashLight) flashLight.enabled = false;
        if (catalogo == null || catalogo.Length == 0 || !entrada) return;
        entrada.disparoGestionado = true;
        disparar = new InputAction("Disparar", InputActionType.Button, "<Mouse>/leftButton");
        recargar = new InputAction("Recargar", InputActionType.Button, "<Keyboard>/r");
        cambiar = new InputAction("Cambiar arma", InputActionType.Button, "<Keyboard>/q");
        usar = new InputAction("Interactuar", InputActionType.Button, "<Keyboard>/e");
        apuntar = new InputAction("Apuntar", InputActionType.Button, "<Mouse>/rightButton");
        camaraArma = entrada.GetAimCamera();
        camaraSeguimiento = UnityEngine.Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        campoVisionOriginal = camaraSeguimiento ? camaraSeguimiento.Lens.FieldOfView : camaraArma ? camaraArma.fieldOfView : 60;
        disparar.Enable(); recargar.Enable(); cambiar.Enable(); usar.Enable(); apuntar.Enable();
    }
    void Start()
    {
        if (catalogo == null || catalogo.Length == 0) return;
        var generador = MatchGame.Instance.GetComponent<SpawnerTest>();
        if (generador) ventanas = generador.ventanas;
        if (vistaPrevia) Destroy(vistaPrevia);
        RecibirArma(catalogo[0]);
    }
    void Update()
    {
        if (Time.timeScale == 0 || Actual == null) return;
        float objetivoVision = apuntar.IsPressed() ? Actual.zoom : campoVisionOriginal;
        if (camaraSeguimiento) camaraSeguimiento.Lens.FieldOfView = Mathf.Lerp(camaraSeguimiento.Lens.FieldOfView, objetivoVision, 12 * Time.deltaTime);
        else if (camaraArma) camaraArma.fieldOfView = Mathf.Lerp(camaraArma.fieldOfView, objetivoVision, 12 * Time.deltaTime);
        ActualizarInteraccion();
        if (cambiar.WasPressedThisFrame() && armas[1 - espacioActual] != null) Equipar(1 - espacioActual);
        if (recargar.WasPressedThisFrame()) Recargar();
        if (Actual.automatica ? disparar.IsPressed() : disparar.WasPressedThisFrame()) Disparar();
    }
    public void RecibirArma(DatosArma nueva)
    {
        if (nueva == null) return;
        int destino = armas[0] == null ? 0 : armas[1] == null ? 1 : espacioActual;
        for (int i = 0; i < armas.Length; i++)
            if (armas[i] == nueva) { reservas[i] = nueva.reservaInicial; Equipar(i); return; }
        armas[destino] = nueva; balas[destino] = nueva.capacidad; reservas[destino] = nueva.reservaInicial;
        Equipar(destino);
    }
    public void Equipar(int espacio)
    {
        if (espacio < 0 || espacio >= armas.Length || armas[espacio] == null) return;
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
        if (Recargando && Actual != null && Actual.recargaPorCartucho && balas[espacioActual] > 0) { StopCoroutine(rutinaRecarga); rutinaRecarga = null; Recargando = false; }
        if (Actual == null || Recargando || Time.time < proximoDisparo || (MatchGame.Instance && MatchGame.Instance.Finished)) return false;
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
        var disparados = new System.Collections.Generic.List<Collider>();
        for (int i = 0; i < Actual.proyectiles; i++)
        {
            Vector3 direccion = (objetivo - origen).normalized;
            direccion = Quaternion.Euler(Random.Range(-Actual.dispersion, Actual.dispersion) * (apuntar.IsPressed() ? 0.55f : 1), Random.Range(-Actual.dispersion, Actual.dispersion) * (apuntar.IsPressed() ? 0.55f : 1), 0) * direccion;
            var proyectil = Instantiate(entrada.ProjectilePrefab, origen, Quaternion.LookRotation(direccion));
            var dano = proyectil.GetComponent<ReciveDamage>();
            if (!dano) { Destroy(proyectil); continue; }
            dano.danoProyectil = Actual.dano;
            var cuerpo = proyectil.GetComponent<Rigidbody>();
            if (!cuerpo) { Destroy(proyectil); continue; }
            cuerpo.useGravity = false; cuerpo.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            cuerpo.linearVelocity = direccion * Actual.velocidadProyectil;
            foreach (var col in proyectil.GetComponentsInChildren<Collider>()) { foreach (var anterior in disparados) Physics.IgnoreCollision(col, anterior); disparados.Add(col); }
            foreach (var ventana in ventanas) if (ventana.limiteJugador) foreach (var col in proyectil.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(col, ventana.limiteJugador);
            foreach (var propio in GetComponentsInChildren<Collider>())
                foreach (var colisionador in proyectil.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(propio, colisionador);
            Destroy(proyectil, Actual.alcance / Actual.velocidadProyectil);
        }
        retroceso = Mathf.Min(retroceso + Actual.retroceso, 2.5f);
        if (sonido && Actual.disparo) { sonido.pitch = Random.Range(0.98f, 1.02f); sonido.PlayOneShot(Actual.disparo, Actual.volumen); }
        else CombatSfx.Shot(transform.position);
        if (fogonazo) { fogonazo.transform.position = salida.position; fogonazo.transform.rotation = salida.rotation; fogonazo.Play(); }
        return true;
    }
    public void Recargar()
    {
        if (Actual == null || Recargando || balas[espacioActual] >= Actual.capacidad || reservas[espacioActual] <= 0) return;
        rutinaRecarga = StartCoroutine(Recarga());
    }
    IEnumerator Recarga()
    {
        Recargando = true;
        if (Actual.recargaPorCartucho)
        {
            while (balas[espacioActual] < Actual.capacidad && reservas[espacioActual] > 0)
            {
                yield return new WaitForSeconds(Actual.recarga);
                balas[espacioActual]++; reservas[espacioActual]--;
            }
            Recargando = false; rutinaRecarga = null; yield break;
        }
        yield return new WaitForSeconds(Actual.recarga);
        int cantidad = Mathf.Min(Actual.capacidad - balas[espacioActual], reservas[espacioActual]);
        balas[espacioActual] += cantidad; reservas[espacioActual] -= cantidad;
        Recargando = false; rutinaRecarga = null;
    }
    void OnDestroy()
    {
        disparar?.Dispose(); recargar?.Dispose(); cambiar?.Dispose(); usar?.Dispose(); apuntar?.Dispose();
        if (camaraArma) camaraArma.fieldOfView = campoVisionOriginal;
        if (camaraSeguimiento) camaraSeguimiento.Lens.FieldOfView = campoVisionOriginal;
        if (entrada) entrada.disparoGestionado = false;
    }

    void ActualizarInteraccion()
    {
        MensajeInteraccion = "";
        var partida = MatchGame.Instance;
        if (!partida || partida.Finished) return;
        if (partida.raizCaja && Vector3.Distance(transform.position, partida.raizCaja.position) < 2.8f)
        {
            MensajeInteraccion = partida.MensajeCaja;
            if (usar.WasPressedThisFrame()) partida.UsarCaja(this);
            return;
        }
        foreach (var ventana in ventanas)
        {
            if (!ventana.raiz || Vector3.Distance(transform.position, ventana.raiz.position) >= 2.8f) continue;
            MensajeInteraccion = ventana.TablasRestantes < ventana.tablas.Length ? "Mantén E · Reparar tablas" : "Ventana asegurada";
            if (usar.IsPressed()) ventana.RepararTabla();
            return;
        }
    }
    // Conserva la presentación y la respuesta al disparo del arma original.
    public void OnWeaponFired()
    {
        CombatSfx.Shot(transform.position);
        retroceso = Mathf.Min(retroceso + 1, 1.7f);
        destelloHasta = Time.time + 0.045f;
        if (muzzleFlash) muzzleFlash.SetActive(true);
        if (flashLight) flashLight.enabled = true;
    }
    void LateUpdate()
    {
        if (entrada && entrada.GetAimCamera())
        {
            var modelo = soporte ? soporte : visual;
            if (modelo)
            {
                if (!soporte) modelo.localPosition = posicionOriginal + new Vector3(0, 0.009f, -0.038f) * retroceso;
                var direccion = entrada.GetAimPoint() - modelo.position;
                if (direccion.sqrMagnitude > 0.01f) modelo.rotation = Quaternion.LookRotation(direccion, entrada.GetAimCamera().transform.up) * Quaternion.Euler(-5 * retroceso, 0, 0);
            }
        }
        retroceso = Mathf.Lerp(retroceso, 0, 1 - Mathf.Exp(-19 * Time.deltaTime));
        if (muzzleFlash && Time.time > destelloHasta) muzzleFlash.SetActive(false);
        if (flashLight && Time.time > destelloHasta) flashLight.enabled = false;
    }
}

// Datos de cada arma guardados en el componente, sin scripts ni assets adicionales.
[System.Serializable]
public class DatosArma
{
    public string nombre;
    public string tipo;
    public float retroceso = 1;
    public float zoom = 48;
    public float volumen = 0.8f;
    public float velocidadProyectil = 80;
    public float alcance = 70;
    public bool recargaPorCartucho;
    public GameObject modelo;
    public int dano = 25;
    public int capacidad = 12;
    public int reservaInicial = 96;
    public int proyectiles = 1;
    public float intervalo = 0.3f;
    public float recarga = 1.5f;
    public float dispersion = 0.5f;
    public bool automatica;
    public AudioClip disparo;
    public Vector3 posicionModelo;
    public Vector3 rotacionModelo;
    public Vector3 escalaModelo = Vector3.one;
}
