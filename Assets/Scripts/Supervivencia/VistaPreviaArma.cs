using UnityEngine;
// Solo muestra el arma en la escena antes de jugar; el inventario crea su propia instancia.
public class VistaPreviaArma : MonoBehaviour { void Awake() { Destroy(gameObject); } }
