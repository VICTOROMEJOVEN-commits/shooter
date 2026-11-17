using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Crea rondas sucesivas; nunca termina la partida por eliminar una ronda.
public class SpawnerTest : MonoBehaviour
{
    // Campos del spawner original conservados para el ejercicio inicial.
    public GameObject objectToSpawn;
    public Vector3 SpawnPosition;
    [Header("Apariciones por rondas")]
    public EnemyNavigation zombiPrefab;
    public DatosVentana[] ventanas = new DatosVentana[0];
    public int limiteSimultaneo = 20;
    public int enemigosIniciales = 8;
    public int aumentoPorRonda = 4;
    public float intervaloAparicion = 1.4f;
    public float descansoEntreRondas = 8;
    public int Ronda { get; private set; }
    public int Pendientes { get; private set; }
    public float InicioProximaRonda { get; private set; }
    public bool EnDescanso { get; private set; }
    int siguienteVentana;
    IEnumerator Start()
    {
        if (!zombiPrefab) { if (objectToSpawn) Instantiate(objectToSpawn, SpawnPosition, transform.rotation); yield break; }
        while (!MatchGame.Instance) yield return null;
        while (!MatchGame.Instance.Finished)
        {
            EnDescanso = true; InicioProximaRonda = Time.time + (Ronda == 0 ? 6 : descansoEntreRondas);
            while (Time.time < InicioProximaRonda && !MatchGame.Instance.Finished) yield return null;
            if (MatchGame.Instance.Finished) yield break;
            EnDescanso = false; Ronda++; Pendientes = enemigosIniciales + (Ronda - 1) * aumentoPorRonda;
            while (Pendientes > 0 && !MatchGame.Instance.Finished)
            {
                if (MatchGame.Instance.EnemigosVivos < limiteSimultaneo && CrearZombi()) Pendientes--;
                yield return new WaitForSeconds(Mathf.Max(0.45f, intervaloAparicion - Ronda * 0.07f));
            }
            while (MatchGame.Instance.EnemigosVivos > 0 && !MatchGame.Instance.Finished) yield return null;
        }
    }
    public bool CrearZombi()
    {
        if (!zombiPrefab || ventanas == null || ventanas.Length == 0) return false;
        var ventana = ventanas[siguienteVentana++ % ventanas.Length];
        if (ventana == null || !ventana.PuedeRecibir) return false;
        var zombi = Instantiate(zombiPrefab, ventana.aparicion.position, ventana.aparicion.rotation);
        var salud = zombi.GetComponent<ReciveDamage>();
        if (salud) salud.ConfigurarVida(Mathf.Min(350, 85 + Ronda * 15));
        if (zombi.agent) zombi.agent.speed = Mathf.Min(3.6f, 2.15f + Ronda * 0.1f);
        zombi.ventanaEntrada = ventana;
        return true;
    }
}
