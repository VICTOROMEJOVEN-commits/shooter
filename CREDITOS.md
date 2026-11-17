# Procedencia de los recursos

## Recursos incorporados para la versión de supervivencia

- **Kenney — Blaster Kit 2.1**: bases de las armas Réquiem (`blaster-a.fbx`), Sepulturera (`blaster-d.fbx`) y Juicio (`blaster-f.fbx`), y mira (`scope-large-a.fbx`).
  - Fuente: https://kenney.nl/assets/blaster-kit
  - Licencia: CC0. Copia del texto original en `Assets/RecursosExternos/KenneyArmas/License.txt`.
  - Adaptación: prefabs con los adornos óseos y los materiales del arma Osario ya existente. No se generaron mallas nuevas ni se dibujaron texturas para estas armas.
- **Kenney — Survival Kit**: cofre `chest.fbx` y marco de entrada `fence-doorway.fbx`, con los materiales importados del paquete.
  - Fuente: https://kenney.nl/assets/survival-kit
  - Licencia: CC0. Copia original en `Assets/RecursosExternos/KenneyUtileria/License.txt`.
- **Quaternius / Gonzalo Furnier — Universal Animation Library, Standard**: `Punch_Cross` para golpear, `Jump_Loop` para atravesar el hueco y `Hit_Chest` para reaccionar al impacto. Se adaptan mediante Humanoid al zombi existente.
  - Fuente: https://quaternius.itch.io/universal-animation-library
  - Sitio del autor: https://quaternius.com/packs/universalanimationlibrary.html
  - Licencia: CC0. Copia original en `Assets/RecursosExternos/QuaterniusAnimaciones/License.txt`.
  - El salto es una adaptación de una animación de salto, no una animación específica de trepar una ventana.

## Recursos que ya estaban en el proyecto

- **Mixamo (Adobe)**: modelo de zombi original y sus animaciones de reposo, caminar y morir. Se conservan el modelo, sus materiales, el esqueleto y su Avatar.
  - Fuente: https://www.mixamo.com/
  - Revisar las condiciones de Adobe/Mixamo para redistribuir los archivos fuente; no se atribuye licencia CC0 a estos recursos.
- **Unity Starter Assets — First Person + Third Person Character Controllers**: controlador, Input System y modelos modulares del escenario. Las tablas y los muros reutilizan el modelo modular de caja del paquete, escalado como objeto en el editor.
  - Fuente: Unity Asset Store, paquete oficial de Unity.
- **Osario**: arma de huesos creada anteriormente para este proyecto con asistencia de IA; no proviene de Kenney ni de Mixamo. Sus mallas, materiales y adornos se reutilizan en las cuatro armas.
- **Materiales del cementerio, música y sonidos de combate**: pertenecen a la versión anterior del proyecto y fueron desarrollados con asistencia de IA. No se presentan como materiales descargados de Kenney.
- **Portada de la versión publicada anterior**: ilustración generada con IA. No representa una captura del juego.

Las mecánicas nuevas usan prefabs, materiales y clips guardados como assets. Los scripts de ejecución no dibujan modelos ni pintan texturas.
