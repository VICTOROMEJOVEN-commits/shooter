# The Cementery of the Dead — Supervivencia

Proyecto académico en Unity 6.3 LTS (6000.3.25f1), URP y el nuevo Input System.

## Cómo jugar

Sobrevive a rondas de zombis sin una ronda final. La partida termina únicamente cuando la vida llega a cero. Cada ronda aumenta la cantidad de enemigos; el máximo simultáneo es 18 para controlar el rendimiento.

Los zombis aparecen fuera del cementerio, rompen las tablas de las ventanas, atraviesan el hueco y persiguen al jugador mediante NavMesh. Puedes reparar las tablas para retrasarlos.

Empiezas con 500 puntos y Osario. La caja misteriosa cuesta 500 puntos, sortea un arma y permite recogerla con E durante 12 segundos. Se pueden llevar dos armas; si ambos espacios están ocupados, se reemplaza el arma equipada. Los impactos y las bajas dan puntos.

## Controles

- WASD: moverse; mouse: apuntar.
- Clic izquierdo: disparar; mantener para el arma automática.
- Espacio: saltar; Shift: correr.
- R: recargar durante la partida; reiniciar después de perder.
- Q: alternar las dos armas.
- E: usar la caja o recoger su premio; mantener cerca de una ventana para reparar.

## Armas

| Arma | Tipo | Daño | Cargador | Particularidad |
|---|---|---:|---:|---|
| Osario | Pistola | 25 | 12 | Semiautomática |
| Réquiem | Automática | 22 | 30 | Disparo continuo |
| Sepulturera | Escopeta | 18 por perdigón | 6 | Siete proyectiles y dispersión |
| Juicio | Fusil | 110 | 5 | Lento y preciso |

Todas tienen estética de calaveras y huesos. Sus valores están en `Assets/Supervivencia/Armas` como assets editables en el Inspector.

## Organización

- Escena principal: `Assets/Scenes/first person.unity`.
- Mecánicas nuevas: `Assets/Scripts/Supervivencia`.
- Prefabs de armas, zombi y partículas: `Assets/Supervivencia/Prefabs`.
- Ventanas y caja: grupo `Supervivencia - Escenario` en la jerarquía.
- Interfaz preparada en la escena: `HUD - Supervivencia`.
- Recursos de terceros: `Assets/RecursosExternos`.
- Origen y licencias de los recursos: [CREDITOS.md](CREDITOS.md).

Se conserva el modelo original del zombi de Mixamo. Los modelos y la interfaz se guardan en la escena y en prefabs; los scripts de ejecución no generan mallas ni pintan texturas.

El GDD se redactará por separado cuando la propuesta esté definida. La versión de itch.io y los ZIP publicados anteriormente corresponden a la versión anterior, salvo que se publique una nueva build explícitamente.
