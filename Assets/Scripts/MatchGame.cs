using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchGame : MonoBehaviour
{
    public static MatchGame Instance { get; private set; }
    public bool Finished { get; private set; }
    readonly HashSet<EnemyNavigation> enemies = new HashSet<EnemyNavigation>();
    int total;
    int defeated;
    PlayerHealth player;
    TMP_Text status;
    TMP_Text health;
    TMP_Text crosshair;
    TMP_Text result;
    Image bar;
    Image damageOverlay;
    GameObject endPanel;
    float hitUntil;
    float damageUntil;
    bool initialized;
    InputAction restartAction;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Time.timeScale = 1;
        var actor = GameObject.FindGameObjectWithTag("Player");
        if (actor) player = actor.GetComponent<PlayerHealth>() ?? actor.AddComponent<PlayerHealth>();
        BuildHUD();
        restartAction = new InputAction("Restart", InputActionType.Button, "<Keyboard>/r");
        restartAction.performed += _ => { if (Finished) Restart(); };
        restartAction.Enable();
    }

    void Start() { initialized = true; }
    public void Register(EnemyNavigation enemy) { if (enemies.Add(enemy)) total++; }
    public void EnemyDefeated(EnemyNavigation enemy)
    {
        if (!enemies.Remove(enemy)) return;
        defeated++;
        if (initialized && total > 0 && enemies.Count == 0) Finish(true);
    }
    public void ShowHit() { hitUntil = Time.unscaledTime + 0.15f; }
    public void ShowDamage() { damageUntil = Time.unscaledTime + 0.25f; }

    void Update()
    {
        status.text = $"OBJETIVO: ELIMINA A TODOS\nEnemigos eliminados: {defeated} / {total}";
        if (player)
        {
            health.text = $"VIDA   {player.Current} / {player.maximumHealth}";
            bar.fillAmount = (float)player.Current / player.maximumHealth;
        }
        crosshair.text = Time.unscaledTime < hitUntil ? "×" : "+";
        crosshair.color = Time.unscaledTime < hitUntil ? new Color(1, 0.65f, 0.2f) : Color.white;
        damageOverlay.color = new Color(1, 0.05f, 0.02f, Time.unscaledTime < damageUntil ? 0.18f : 0);

    }

    public void Finish(bool won)
    {
        if (Finished) return;
        Finished = true;
        result.text = won ? "¡VICTORIA!\n<size=24>Eliminaste a todos los enemigos.</size>" : "DERROTA\n<size=24>Te quedaste sin vida.</size>";
        endPanel.SetActive(true);
        if (player)
        {
            foreach (var behaviour in player.GetComponents<MonoBehaviour>())
                if (behaviour.GetType().Name == "FirstPersonController" || behaviour.GetType().Name == "StarterAssetsInputs") behaviour.enabled = false;
            var input = player.GetComponent<PlayerInput>();
            if (input) input.DeactivateInput();
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDestroy()
    {
        restartAction?.Dispose();
        if (Instance != this) return;
        Time.timeScale = 1;
        Instance = null;
    }

    RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
        return rect;
    }

    TMP_Text Label(string name, Transform parent, string text, float size, Vector2 min, Vector2 max, Vector2 a, Vector2 b)
    {
        var rect = Rect(name, parent, min, max, a, b);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text; label.fontSize = size; label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    void BuildHUD()
    {
        var canvas = new GameObject("HUD - Shooter", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.transform.SetParent(transform, false);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.GetComponent<Canvas>().sortingOrder = 100;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        var background = Rect("Panel objetivo", canvas.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -125), new Vector2(490, -20));
        background.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.04f, 0.065f, 0.85f);
        status = Label("Objetivo", background, "", 24, Vector2.zero, Vector2.one, new Vector2(16, 10), new Vector2(-12, -12));
        health = Label("Vida", canvas.transform, "", 26, Vector2.zero, Vector2.zero, new Vector2(30, 65), new Vector2(350, 110));
        var track = Rect("Fondo vida", canvas.transform, Vector2.zero, Vector2.zero, new Vector2(30, 38), new Vector2(330, 58));
        track.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.12f, 0.15f, 0.9f);
        var fill = Rect("Barra vida", track, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bar = fill.gameObject.AddComponent<Image>();
        bar.color = new Color(0.15f, 0.85f, 0.65f);
        // A built-in white sprite is unnecessary: Filled requires a sprite, so use a generated one.
        bar.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        bar.type = Image.Type.Filled; bar.fillMethod = Image.FillMethod.Horizontal;
        crosshair = Label("Mira", canvas.transform, "+", 30, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-25, -25), new Vector2(25, 25));
        crosshair.alignment = TextAlignmentOptions.Center;
        damageOverlay = Rect("Impacto recibido", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject.AddComponent<Image>();
        damageOverlay.raycastTarget = false; damageOverlay.color = Color.clear;
        endPanel = Rect("Fin de partida", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
        endPanel.AddComponent<Image>().color = new Color(0.02f, 0.035f, 0.06f, 0.94f);
        result = Label("Resultado", endPanel.transform, "", 54, new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.75f), Vector2.zero, Vector2.zero);
        result.alignment = TextAlignmentOptions.Center;
        var retry = Label("Reiniciar", endPanel.transform, "Presiona R para volver a jugar", 26, new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.4f), Vector2.zero, Vector2.zero);
        retry.alignment = TextAlignmentOptions.Center;
        endPanel.SetActive(false);
    }
}
// HUD and match state.
// Reimport match controller.




