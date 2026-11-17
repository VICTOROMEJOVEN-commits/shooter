# The Cementery of the Dead

Proyecto académico de supervivencia en primera persona desarrollado en Unity 6.3 LTS con URP y el nuevo Input System.

## Juego

Sobrevive a rondas sucesivas de zombis que entran por tres ventanas. Puedes reparar sus tablas, obtener puntos y comprar un arma aleatoria en la caja misteriosa. La partida termina únicamente cuando la vida llega a cero. Hay cuatro armas de estética ósea y dos espacios de inventario.

Controles: WASD para moverse, mouse para apuntar, clic para disparar, Espacio para saltar y Shift para correr. R recarga durante la partida y reinicia después de perder; Q cambia de arma y E usa la caja o repara una ventana.

## Organización original

Se conservan las carpetas originales. Los modelos importados están en `Assets/models`, sus materiales en `Assets/material`, sus texturas en `Assets/textures`, los clips en `Assets/animations` y los prefabs en `Assets/Pre fabs`. No hay carpetas adicionales de supervivencia ni de recursos externos.

La escena principal sigue siendo `Assets/Scenes/first person.unity`. Se conservan el zombi de Mixamo, el controlador, el suelo ampliado y las dieciocho lápidas agrupadas en dos sectores de nueve. El NavMesh se mantiene horneado y con rutas desde las tres entradas.

## Scripts originales

- `SpawnerTest`: conserva los campos del ejercicio inicial y añade las apariciones por rondas.
- `EnemyNavigation`: mantiene la persecución y la muerte del enemigo e incluye la entrada por ventanas y sus tablas. `DatosVentana` es una estructura de configuración dentro de este mismo archivo.
- `BoneWeaponView`: conserva la presentación del arma e integra el inventario, el disparo, la munición, la recarga y la interacción. El catálogo se configura en su Inspector. `DatosArma` es una estructura de datos incluida en el mismo archivo, no otro script ni un ScriptableObject independiente.
- `ReciveDamage`: conserva la vida y la recepción de impactos del enemigo. También se utiliza en el prefab del proyectil para aplicar una sola vez el daño correspondiente al arma.
- `MatchGame`: conserva la partida, los puntos y la interfaz, y coordina la caja misteriosa y los efectos de impacto.
- `PlayerHealth`: conserva la vida y el daño del jugador.
- `CombatSfx`: conserva los sonidos básicos de combate.

Los scripts mantienen sus archivos y nombres originales; las mecánicas se amplían sobre ellos. La interfaz, los modelos, los materiales y las partículas están preparados en la escena o como prefabs.

## Procedencia de los recursos

Kenney Blaster Kit 2.1 aporta las bases `blaster-a`, `blaster-d`, `blaster-f` y la mira `scope-large-a`: https://kenney.nl/assets/blaster-kit . Kenney Survival Kit aporta el cofre `chest` y el marco `fence-doorway`: https://kenney.nl/assets/survival-kit . Ambos paquetes tienen licencia CC0; sus licencias están guardadas junto a los modelos, con nombres `Licencia-KenneyArmas.txt` y `Licencia-KenneyUtileria.txt`.

Quaternius y Gonzalo Furnier aportan las animaciones de golpe, salto y reacción al daño de Universal Animation Library Standard: https://quaternius.itch.io/universal-animation-library . Licencia CC0, guardada en `Assets/models/Licencia-QuaterniusAnimaciones.txt`. El salto se adapta al paso de la ventana; se mantiene el modelo y el Avatar originales del zombi de Mixamo: https://www.mixamo.com/ .

El controlador y los modelos modulares originales provienen del paquete oficial Unity Starter Assets de Unity Asset Store. El arma Osario y sus adornos, los materiales del cementerio, la música y los sonidos del prototipo anterior se desarrollaron con asistencia de IA. Las tres armas importadas reutilizan esos adornos óseos; no se atribuye Osario a Kenney ni a Mixamo.

El GDD es un documento separado. La build publicada anteriormente en itch.io y los ZIP anteriores todavía corresponden a la versión previa, hasta que se genere una nueva build de forma explícita.
