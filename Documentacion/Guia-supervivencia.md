# Guía de los cambios de supervivencia

Esta guía describe lo añadido y sus conexiones. No es el GDD.

## Partida y enemigos

`MatchGame` está en **Game Manager**. Cuenta puntos, bajas y tiempo sobrevivido; actualiza las referencias al Canvas **HUD - Supervivencia**. Solo permite terminar la partida si la vida llega a cero. Conserva el nombre anterior para no romper las referencias de la escena y de los otros scripts.

`GeneradorRondas`, en el mismo objeto, usa una corrutina para esperar entre rondas y entre apariciones. Su prefab es **ZombiSupervivencia**; el array de ventanas contiene las entradas norte, oeste y este. El límite simultáneo permite que sigan apareciendo zombis sin acumular objetos indefinidamente. Los cadáveres se destruyen después de su animación.

`EnemyNavigation` conserva el modelo y el Animator del zombi. Antes de perseguir al jugador, espera a terminar la entrada por su ventana. Durante el paso por el hueco, el NavMeshAgent está desactivado; al llegar al suelo interior se lo coloca sobre el NavMesh y se vuelve a activar. Así, NavMesh no intenta caminar a través del antepecho.

`ReciveDamage` aplica el daño que lleva cada `DanoProyectil`. Su nombre anterior se conserva para mantener las referencias del prefab. Al recibir daño actualiza la barra, concede puntos y reproduce la animación de impacto. Al llegar a cero llama a la muerte del enemigo.

## Ventanas

Cada **VentanaZombis** referencia tres tablas, los puntos de aparición, golpe y llegada, sus sonidos y un límite que impide que el jugador salga del mapa. `TablaVentana` solo identifica a qué ventana pertenece un colisionador.

`Entrar` es una corrutina: aproxima al zombi, reproduce el golpe, retira las tablas y luego mueve al zombi al interior mientras se reproduce un salto importado. No se cambian la malla ni el esqueleto. Es una adaptación de un salto, no captura de movimiento específica de trepar ventanas.

`RepararTabla` recupera una tabla por intervalo; se impide reconstruir la ventana justo mientras un zombi está pasando. Hay un límite de premios por reparación para evitar conseguir puntos ilimitados sin combatir.

## Armas y caja

`DatosArma` es un ScriptableObject: un archivo de configuración por arma. Contiene su prefab visual, daño, capacidad, reserva, intervalo, recarga y cantidad de proyectiles. Separar estos datos permite ajustar un arma en el Inspector sin duplicar el código de disparo.

`ArsenalJugador` está en **PlayerCapsule**. Conserva dos espacios con munición independiente. Usa InputActions para disparar, recargar y cambiar. Instancia el prefab de proyectil y lo desplaza con su Rigidbody. La escopeta crea siete proyectiles con dispersión; la automática permite mantener el clic. No se usan disparos de daño instantáneo por raycast: el raycast solo ayuda a alinear el proyectil con la mira.

`DanoProyectil` contiene el daño del arma y evita procesar dos impactos del mismo proyectil. `VistaArma` orienta el prefab hacia el objetivo de la mira. `VistaPreviaArma` retira al empezar una copia que solo sirve para ver el arma en el editor.

`CajaMisteriosa` tiene un catálogo con las cuatro configuraciones. Cobra puntos una vez, muestra el sorteo y guarda el premio durante doce segundos. La segunda pulsación de E lo recoge. Si el inventario está lleno, reemplaza el espacio equipado. Su cofre es un modelo importado.

`InteraccionJugador`, en **PlayerCapsule**, detecta la proximidad de la caja o una ventana y muestra la acción disponible. E se pulsa para la caja y se mantiene para reparar.

## Interfaz, efectos y recursos

La interfaz está guardada como objetos de la escena. `MatchGame` cambia textos y rellenos; no construye un Canvas al jugar. Los efectos están configurados como prefabs de Particle System; `EfectosCombate` únicamente los instancia al impactar. La música y los sonidos básicos del proyecto anterior se conservan.

Los modelos de terceros, sus enlaces y las adaptaciones están documentados en **CREDITOS.md**. Los prefabs de las armas se combinan con las piezas óseas de Osario. No se han dibujado texturas ni generado mallas nuevas para esta versión.

El bake se rehízo después de ampliar el suelo, reducir las lápidas y ordenarlas en dos grupos de nueve. Se revisaron las rutas de las tres ventanas hasta el jugador.
