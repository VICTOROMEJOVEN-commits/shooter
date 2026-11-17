using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Estado de la partida. La interfaz ya existe en la escena y solo se actualiza su contenido.
public class MatchGame : MonoBehaviour
{
    public static MatchGame Instance { get; private set; }
    public bool Finished { get; private set; }
    public int Puntos { get; private set; } = 500;
    public int Eliminados { get; private set; }
    public int EnemigosVivos => enemigos.Count;
    public float TiempoSobrevivido { get; private set; }
    public TMP_Text estado, vida, municion, interaccion, mira, resultado;
    public Image barraVida, pantallaDano;
    public GameObject panelFinal;
    readonly HashSet<EnemyNavigation> enemigos = new HashSet<EnemyNavigation>();
    PlayerHealth jugador;
    ArsenalJugador arsenal;
    InteraccionJugador uso;
    GeneradorRondas rondas;
    InputAction reiniciar;
    float impactoHasta, danoHasta;
    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this; Time.timeScale = 1;
        var actor = GameObject.FindGameObjectWithTag("Player");
        if (actor) { jugador = actor.GetComponent<PlayerHealth>(); arsenal = actor.GetComponent<ArsenalJugador>(); uso = actor.GetComponent<InteraccionJugador>(); }
        rondas = GetComponent<GeneradorRondas>();
        if (panelFinal) panelFinal.SetActive(false);
        reiniciar = new InputAction("Reiniciar partida", InputActionType.Button, "<Keyboard>/r"); reiniciar.Enable();
    }
    public void Register(EnemyNavigation enemigo) { enemigos.Add(enemigo); }
    public void Retirar(EnemyNavigation enemigo) { enemigos.Remove(enemigo); }
    public void EnemyDefeated(EnemyNavigation enemigo)
    {
        if (!enemigos.Remove(enemigo)) return;
        Eliminados++; SumarPuntos(100);
        // No hay victoria: terminar una ronda permite comenzar otra.
    }
    public void SumarPuntos(int cantidad) { if (!Finished && cantidad > 0) Puntos += cantidad; }
    public bool GastarPuntos(int cantidad)
    {
        if (Finished || cantidad < 0 || Puntos < cantidad) return false;
        Puntos -= cantidad; return true;
    }
    public void ShowHit() { impactoHasta = Time.unscaledTime + 0.15f; }
    public void ShowDamage() { danoHasta = Time.unscaledTime + 0.25f; }
    void Update()
    {
        if (!Finished) TiempoSobrevivido += Time.deltaTime;
        if (Finished && reiniciar.WasPressedThisFrame()) Restart();
        int minutos = (int)TiempoSobrevivido / 60, segundos = (int)TiempoSobrevivido % 60;
        if (estado)
        {
            string ronda = rondas && rondas.EnDescanso ? "Próxima ronda en " + Mathf.CeilToInt(Mathf.Max(0, rondas.InicioProximaRonda - Time.time)) : "RONDA " + (rondas ? rondas.Ronda : 1);
            estado.text = $"{ronda}   ·   {minutos:00}:{segundos:00}\nPuntos: {Puntos}   ·   Eliminados: {Eliminados}";
        }
        if (jugador)
        {
            if (vida) vida.text = $"VIDA  {jugador.Current} / {jugador.maximumHealth}";
            if (barraVida) barraVida.fillAmount = (float)jugador.Current / jugador.maximumHealth;
        }
        if (municion && arsenal && arsenal.Actual)
            municion.text = $"{arsenal.Actual.nombre}\n{arsenal.balas[arsenal.espacioActual]} / {arsenal.reservas[arsenal.espacioActual]}" + (arsenal.Recargando ? "  ·  Recargando…" : "\nR · Recargar    Q · Cambiar arma");
        if (interaccion) interaccion.text = uso ? uso.Mensaje : "";
        if (mira) { mira.text = Time.unscaledTime < impactoHasta ? "×" : "+"; mira.color = Time.unscaledTime < impactoHasta ? new Color(1, 0.6f, 0.2f) : Color.white; }
        if (pantallaDano) pantallaDano.color = new Color(0.8f, 0, 0, Time.unscaledTime < danoHasta ? 0.22f : 0);
    }
    public void Finish(bool won)
    {
        if (Finished || won || !jugador || jugador.Current > 0) return;
        Finished = true;
        if (resultado) resultado.text = $"HAS CAÍDO\n<size=27>Ronda {(rondas ? rondas.Ronda : 1)} · {Eliminados} zombis eliminados\nSobreviviste {(int)TiempoSobrevivido / 60:00}:{(int)TiempoSobrevivido % 60:00}</size>";
        if (panelFinal) panelFinal.SetActive(true);
        foreach (var comportamiento in jugador.GetComponents<MonoBehaviour>())
            if (comportamiento.GetType().Name == "FirstPersonController" || comportamiento.GetType().Name == "StarterAssetsInputs") comportamiento.enabled = false;
        jugador.GetComponent<PlayerInput>()?.DeactivateInput();
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true; Time.timeScale = 0;
    }
    public void Restart() { Time.timeScale = 1; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    void OnDestroy() { reiniciar?.Dispose(); if (Instance == this) { Instance = null; Time.timeScale = 1; } }
}
