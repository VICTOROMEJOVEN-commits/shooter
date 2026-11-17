using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Crea rondas sucesivas; nunca termina la partida por eliminar una ronda.
public class GeneradorRondas : MonoBehaviour
{
    public EnemyNavigation zombiPrefab;
    public VentanaZombis[] ventanas;
    public int limiteSimultaneo = 18;
    public float descansoEntreRondas = 8;
    public int Ronda { get; private set; }
    public int Pendientes { get; private set; }
    public float InicioProximaRonda { get; private set; }
    public bool EnDescanso { get; private set; }
    int siguienteVentana;
    IEnumerator Start()
    {
        while (!MatchGame.Instance) yield return null;
        while (!MatchGame.Instance.Finished)
        {
            EnDescanso = true; InicioProximaRonda = Time.time + (Ronda == 0 ? 6 : descansoEntreRondas);
            while (Time.time < InicioProximaRonda && !MatchGame.Instance.Finished) yield return null;
            if (MatchGame.Instance.Finished) yield break;
            EnDescanso = false; Ronda++; Pendientes = 4 + Ronda * 2;
            while (Pendientes > 0 && !MatchGame.Instance.Finished)
            {
                if (MatchGame.Instance.EnemigosVivos < limiteSimultaneo && CrearZombi()) Pendientes--;
                yield return new WaitForSeconds(Mathf.Max(0.7f, 2.8f - Ronda * 0.12f));
            }
            while (MatchGame.Instance.EnemigosVivos > 0 && !MatchGame.Instance.Finished) yield return null;
        }
    }
    public bool CrearZombi()
    {
        if (!zombiPrefab || ventanas == null || ventanas.Length == 0) return false;
        var ventana = ventanas[siguienteVentana++ % ventanas.Length];
        if (!ventana || !ventana.PuedeRecibir) return false;
        var zombi = Instantiate(zombiPrefab, ventana.aparicion.position, ventana.aparicion.rotation);
        var salud = zombi.GetComponent<ReciveDamage>();
        if (salud) salud.ConfigurarVida(Mathf.Min(350, 85 + Ronda * 15));
        if (zombi.agent) zombi.agent.speed = Mathf.Min(3.6f, 1.65f + Ronda * 0.1f);
        zombi.ventanaEntrada = ventana;
        return true;
    }
}
