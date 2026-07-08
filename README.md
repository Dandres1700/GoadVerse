# GoadVerse

GoadVerse es un juego 3D hecho en Unity. El flujo actual inicia en un
`Bootstrap`, carga el menu principal, permite entrar al `Lobby` y desde ahi
usar el portal de futbol para entrar al `minijuego1`.

## Estado actual del juego

- Menu principal con fondo personalizado, botones visuales, musica de inicio y
  copa 3D giratoria frente al fondo.
- Lobby con personaje, camara en tercera persona y portal interactivo.
- Entrada al minijuego de futbol al acercarse al portal y presionar `E`.
- Movimiento 3D con teclado y mando.
- Camara con seguimiento del jugador y zoom con scroll del mouse.
- Animaciones base de caminar, correr, saltar y acciones de futbol.
- Paredes invisibles para delimitar el lobby y la cancha del minijuego.
- Balon del minijuego ajustado de escala, con rebote y respuesta al patearlo.
- Demo Football OS en `minijuego1` con jugadores de prueba, balon, camara
  cinematic/override y acciones de pase/recepcion.
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

`Menu_Principal` usa un fondo 2D en `Canvas3D` y una copa 3D por delante. Ese
canvas esta renderizado por camara para que el fondo no tape la copa. La copa se
mantiene girando con `CopaRotator`.

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

El demo automatico de Football OS se apoya en `FootballOSAnimationDemoAuto`,
`FootballOSCinematicCamera` y `FootballOSCameraOverride`. Si se edita esta demo,
verificar que no queden marcadores de merge (`<<<<<<<`, `=======`, `>>>>>>>`) en
el script ni en la escena.

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
- `Assets/Scripts/ButtonHoverEffect`: efectos de hover para botones del menu.
- `Assets/Scenes`: escenas jugables y flujo principal.
- `Assets/Maps`: mapa/cancha del minijuego.
- `Assets/Animations`: animaciones del personaje y acciones de futbol.
- `Assets/Audio`: musica, sonidos de botones y audio de inicio.
- `Assets/Textures/Menu_Inicio`: fondos e imagenes del menu principal.
- `Assets/Textures/UI`: paquetes de UI/VFX importados y organizados.
- `Assets/Textures` y `Assets/Models`: modelos, imagenes y texturas generales.

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

Para ajustar la copa del menu principal:

1. Seleccionar `Copa mundial (1)` en `Menu_Principal`.
2. Ajustar el `Transform` en el Inspector.
3. Copiar esos mismos valores al componente `CopaRotator`:
   `fixedLocalPosition` y `fixedLocalScale`.
4. Si se quiere moverla libremente sin que el script la corrija al dar Play,
   desactivar `forceExactLocalPosition` y `forceExactLocalScale`.

Valores actuales de la copa del menu:

- Position: `X -105`, `Y -133`, `Z 316`.
- Rotation: `X 0`, `Y 0`, `Z 0`.
- Scale: `X 350`, `Y 350`, `Z 350`.

## Convenciones de trabajo

- Guardar escenas antes de subir cambios.
- Revisar `git status` antes de hacer commit.
- Mantener assets importados en carpetas separadas por origen.
- Agregar credito y licencia cuando se importe un asset externo.
- Probar el flujo completo: `Bootstrap -> Menu_Principal -> Lobby -> minijuego1`.
- Revisar el menu principal despues de tocar el Canvas o la copa, porque el
  fondo debe quedar detras del modelo 3D.
- En escenas serializadas de Unity, limpiar cualquier conflicto de merge antes
  de abrir Play Mode.

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
- "8Bit Music - 062022" by GWriterStudio is imported under
  `Assets/Audio/8Bit Music - 062022`. The local notes credit GWriterStudio and
  list 10 tracks; confirm the original license/source before distributing.
- TextMesh Pro, Unity UI, Input System, URP and other Unity packages are provided by Unity Technologies under their package and Unity terms.

## Recursos pendientes por confirmar

Los siguientes recursos estan dentro del proyecto, pero no tienen una fuente o
licencia externa documentada en los archivos locales. Si alguno fue descargado
de un tercero, agregar aqui su autor, URL y licencia antes de publicar una
version distribuible:

- `Assets/Models/ballon/Ball.blend`
- `Assets/Textures/Menu_Inicio/Menu_Inicio.jpeg`
- `Assets/Textures/Menu_Inicio/Fondo/Fondo_Menu.png`
- `Assets/Textures/Botones/*.png`
- `Assets/Audio/BotonSonido.wav`
- `Assets/Audio/Inicio/Title_Juego.mp3`
- `Assets/Audio/8Bit Music - 062022/*.wav`

## Notas de licencia

Este README no reemplaza las licencias originales de cada asset. Cuando se
redistribuya el proyecto, conservar los archivos de licencia incluidos en las
carpetas de los paquetes importados, especialmente:

- `Assets/Textures/UI/Alebardium/Bloodlines UI/Third-Party Notices.txt`
- `Assets/Textures/UI/Alebardium/Bloodlines UI/Fonts/ManufacturingConsent/OFL.txt`
- `Assets/Textures/UI/Alebardium/Bloodlines UI/Fonts/MedievalSharp/OFL.txt`
- Licencias incluidas con TextMesh Pro y fuentes usadas por Unity.
