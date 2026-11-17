using UnityEngine;

// Un asset por arma: los valores se editan en el Inspector, no en el código.
[CreateAssetMenu(menuName = "Supervivencia/Arma")]
public class DatosArma : ScriptableObject
{
    public string nombre;
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
