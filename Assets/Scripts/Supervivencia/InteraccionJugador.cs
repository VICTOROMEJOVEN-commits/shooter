using UnityEngine;
using UnityEngine.InputSystem;

public class InteraccionJugador : MonoBehaviour
{
    public float alcance = 2.8f;
    public string Mensaje { get; private set; }
    InputAction usar;
    ArsenalJugador arsenal;
    CajaMisteriosa caja;
    VentanaZombis[] ventanas;
    void Start()
    {
        arsenal = GetComponent<ArsenalJugador>();
        caja = FindFirstObjectByType<CajaMisteriosa>();
        ventanas = FindObjectsByType<VentanaZombis>(FindObjectsSortMode.None);
        usar = new InputAction("Interactuar", InputActionType.Button, "<Keyboard>/e"); usar.Enable();
    }
    void Update()
    {
        Mensaje = "";
        if (!MatchGame.Instance || MatchGame.Instance.Finished) return;
        if (caja && Vector3.Distance(transform.position, caja.transform.position) < alcance)
        {
            Mensaje = caja.Mensaje;
            if (usar.WasPressedThisFrame()) caja.Usar(arsenal);
            return;
        }
        foreach (var ventana in ventanas)
        {
            if (Vector3.Distance(transform.position, ventana.transform.position) >= alcance) continue;
            Mensaje = ventana.TablasRestantes < ventana.tablas.Length ? "Mantén E · Reparar tablas" : "Ventana asegurada";
            if (usar.IsPressed()) ventana.RepararTabla();
            return;
        }
    }
    void OnDestroy() { usar?.Dispose(); }
}
