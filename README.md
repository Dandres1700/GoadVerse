# GoadVerse

GoadVerse es un juego 3D hecho en Unity. El flujo actual inicia en un
`Bootstrap`, carga el menu principal, permite entrar al `Lobby` y desde ahi
usar el portal de futbol para entrar al `minijuego1`.

## Estado actual del juego

- Menu principal con botones para jugar, opciones y salir.
- Lobby con personaje, camara en tercera persona y portal interactivo.
- Entrada al minijuego de futbol al acercarse al portal y presionar `E`.
- Movimiento 3D con teclado y mando.
- Camara con seguimiento del jugador y zoom con scroll del mouse.
- Animaciones base de caminar, correr, saltar y acciones de futbol.
- Paredes invisibles para delimitar el lobby y la cancha del minijuego.
- Copa giratoria en el lobby/menu con posicion controlada por escena.

## Requisitos

- Unity `6000.4.8f1`.
- Universal Render Pipeline incluido por el proyecto.
- Input System de Unity incluido por el proyecto.
- Git para control de versiones.

## Como abrir el proyecto

1. Clonar el repositorio.
2. Abrir la carpeta del proyecto en Unity Hub.
3. Usar Unity `6000.4.8f1`.
4. Esperar a que Unity importe los assets.
5. Ejecutar desde la escena `Assets/Scenes/Bootstrap.unity`.

Las escenas activas en Build Settings son:

- `Assets/Scenes/Bootstrap.unity`
- `Assets/Scenes/Menu_Principal.unity`
- `Assets/Scenes/Lobby.unity`
- `Assets/Scenes/minijuego1.unity`

## Flujo de escenas

El juego arranca en `Bootstrap`. Esa escena mantiene los managers globales y
carga `Menu_Principal` usando `SceneLoader`.

Desde `Menu_Principal`, el boton `Play` llama a `MainMenuManager.OnJugarPressed`
y carga `Lobby`.

En `Lobby`, el objeto `Portal_Futbol` usa `MinigamePortal`. El jugador debe
estar cerca y presionar `E` para cargar `minijuego1`.

`minijuego1` contiene la cancha, el estadio y las paredes invisibles del mapa.
El sistema esta preparado para que `MinigameSceneManager` pueda instanciar mapas
si se agregan prefabs de mapas en el futuro.

## Controles

Teclado:

- `WASD`: mover jugador 1.
- `Shift izquierdo`: correr.
- `Space`: saltar.
- Mouse: mover camara.
- Scroll del mouse: acercar o alejar camara.
- `E`: interactuar con portal.
- `Esc`: liberar cursor.
- Click izquierdo: bloquear cursor otra vez.

Pruebas de animaciones de futbol en `minijuego1`:

- `O`: tiro.
- `P`: pase.
- `R`: recibir.
- `T`: tackle.
- `C`: celebracion.

Mando:

- Stick izquierdo: movimiento.
- Stick derecho: camara.
- Boton sur / `JoystickButton0`: salto e interaccion.
- Shoulder / triggers configurados: correr.

## Como estamos construyendo el juego

El proyecto esta organizado por responsabilidades:

- `Assets/Scripts/Managers`: carga de escenas, estado global y delimitantes.
- `Assets/Scripts/Player`: movimiento, camara y control de animaciones.
- `Assets/Scripts/Interactables`: portal hacia minijuegos.
- `Assets/Scripts/UI`: comportamiento visual de elementos como la copa.
- `Assets/Scenes`: escenas jugables y flujo principal.
- `Assets/Maps`: mapa/cancha del minijuego.
- `Assets/Animations`: animaciones del personaje y acciones de futbol.
- `Assets/Textures` y `Assets/Models`: modelos, imagenes y texturas.

Para agregar un nuevo minijuego:

1. Crear una escena nueva en `Assets/Scenes`.
2. Agregarla a Build Settings.
3. Crear o ubicar el portal correspondiente en `Lobby`.
4. Configurar `MinigamePortal.minigameSceneName` con el nombre exacto de la
   escena.
5. Agregar un objeto con `MapBoundaryWalls` para delimitar el area jugable.
6. Si el minijuego usa mapas por prefab, asignarlos a `MinigameSceneManager`.

Para agregar animaciones:

1. Importar los `.fbx` en `Assets/Animations`.
2. Configurar el rig como Humanoid si aplica.
3. Agregar estados al Animator Controller.
4. Usar parametros existentes cuando sea posible:
   `Speed`, `IsRunning`, `IsGrounded`, `VerticalSpeed`, `Jump`, `Shoot`,
   `Pass`, `Receive`, `Tackle`, `Celebrate`.

## Convenciones de trabajo

- Guardar escenas antes de subir cambios.
- Revisar `git status` antes de hacer commit.
- Mantener assets importados en carpetas separadas por origen.
- Agregar credito y licencia cuando se importe un asset externo.
- Probar el flujo completo: `Bootstrap -> Menu_Principal -> Lobby -> minijuego1`.

## Creditos y agradecimientos

Gracias a las personas y grupos que hicieron posibles varios assets usados o
importados en este proyecto:

- "Copa mundial - Cup World" (https://skfb.ly/pKnU6) by marcoseec is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
- "Craven Cottage Stadium - Game Ready Asset (FREE)" (https://sketchfab.com/3d-models/craven-cottage-stadium-game-ready-asset-free-32ffa67d18974419aac433d64a223b12) by HxZy is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
- "Human Basic Motions FREE" (https://kevdev.itch.io/basic-motions-free) by Kevin Iglesias is used under the asset terms for the pack / Standard Unity Asset Store EULA.
- Mixamo animations (https://www.mixamo.com/) by Adobe Mixamo are used for soccer/action animation prototyping. Adobe's Mixamo FAQ states that characters and animations can be used royalty-free in personal, commercial and non-profit projects, including video games.
- "Bloodlines - Dark UI" (https://xgaida.itch.io/bloodlines-ui) by xGaida and Shieldomirs is used for UI assets. The package states it is free for personal and commercial projects and should not be resold or repackaged as raw assets.
- Bloodlines UI bundled fonts:
  - "Manufacturing Consent" by The Manufacturing Consent Project Authors, licensed under SIL Open Font License 1.1.
  - "MedievalSharp" by wmk69, licensed under SIL Open Font License 1.1.
- "Demo VFX - Impact & Hit" / VFX Impact assets (https://wallcoeur.itch.io/demo-vfx-impact-hit) by wallcoeur / Cartoon VFX by Wallcoeur are used as imported VFX resources.
- TextMesh Pro, Unity UI, Input System, URP and other Unity packages are provided by Unity Technologies under their package and Unity terms.

## Recursos pendientes por confirmar

Los siguientes recursos estan dentro del proyecto, pero no tienen una fuente o
licencia externa documentada en los archivos locales. Si alguno fue descargado
de un tercero, agregar aqui su autor, URL y licencia antes de publicar una
version distribuible:

- `Assets/Models/ballon/Ball.blend`
- `Assets/Textures/Menu_Inicio/Menu_Inicio.jpeg`
- `Assets/Textures/Botones/*.png`
- `Assets/Audio/BotonSonido.wav`

## Notas de licencia

Este README no reemplaza las licencias originales de cada asset. Cuando se
redistribuya el proyecto, conservar los archivos de licencia incluidos en las
carpetas de los paquetes importados, especialmente:

- `Assets/Alebardium/Bloodlines UI/Third-Party Notices.txt`
- `Assets/Alebardium/Bloodlines UI/Fonts/ManufacturingConsent/OFL.txt`
- `Assets/Alebardium/Bloodlines UI/Fonts/MedievalSharp/OFL.txt`
- Licencias incluidas con TextMesh Pro y fuentes usadas por Unity.
