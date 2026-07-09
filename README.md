# GoadVerse

GoadVerse es un juego 3D hecho en Unity. El flujo actual inicia en un
`Bootstrap`, carga el menu principal, reproduce la secuencia de introduccion e
historia, permite entrar al `Lobby` y desde ahi usar el portal de futbol para
entrar al `minijuego1`.

## Estado actual del juego

- Menu principal con fondo personalizado, botones visuales, musica de inicio y
  copa 3D giratoria frente al fondo.
- Boton `Play` con secuencia narrativa: video de instrucciones, video de
  historia con narracion y entrada automatica al lobby.
- Panel `Settings` del menu principal con controles del juego.
- Lobby con personaje, camara en tercera persona y portal interactivo.
- Boton de salida en el lobby.
- Entrada al minijuego de futbol al acercarse al portal y presionar `E`.
- Movimiento 3D con teclado y mando.
- Camara con seguimiento del jugador y zoom con scroll del mouse.
- Animaciones base de caminar, correr, saltar y acciones de futbol.
- Paredes invisibles para delimitar el lobby y la cancha del minijuego.
- Balon del minijuego ajustado de escala, con rebote y respuesta al patearlo.
- UI del minijuego con botones de pausa, configuracion, controles y salida al
  lobby.
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

Desde `Menu_Principal`, el boton `Play` llama a `MainMenuManager.OnJugarPressed`.
Primero reproduce `Assets/Historia/Instrucciones.mp4`. Al terminar, reproduce
`Assets/Historia/Historia.mp4` junto con `Assets/Historia/Historia_narrada.mp3`.
Cuando finalizan el video de historia y la narracion, carga `Lobby`.

Para builds `.exe`, `MainMenuManager` busca esos archivos en
`StreamingAssets/Historia`. El script de editor
`HistoriaStreamingAssetsBuildProcessor` copia automaticamente los archivos desde
`Assets/Historia` hacia `Assets/StreamingAssets/Historia` antes de generar el
build. Tambien se puede ejecutar manualmente desde el menu de Unity:
`GoadVerse -> Sync Historia Streaming Assets`.

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

- `WASD` / flechas: mover jugador del Team Player.
- `E` / `Tab`: cambiar jugador.
- `O`: patear o pasar el balon.
- Boton `PAUSA`: detener o reanudar el partido.
- Boton `SETTINGS`: ver controles y salir al lobby.

Pruebas de animaciones auxiliares en `minijuego1`:

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
- `Assets/Editor`: utilidades de editor, incluyendo copia de archivos de
  historia para builds.
- `Assets/Scenes`: escenas jugables y flujo principal.
- `Assets/Maps`: mapa/cancha del minijuego.
- `Assets/Animations`: animaciones del personaje y acciones de futbol.
- `Assets/Audio`: musica, sonidos de botones y audio de inicio.
- `Assets/Historia`: videos y audio de la introduccion narrativa.
- `Assets/Resources`: configuracion editable de UI runtime.
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

Para ajustar UI runtime del menu, lobby y minijuego:

1. Seleccionar `Assets/Resources/SceneUtilityUISettings.asset`.
2. Editar posiciones, tamanos, textos, colores o nombres de archivos.
3. Dar Play para ver los valores aplicados.

Para preparar el `.exe`:

1. Verificar que `Assets/Historia` contenga:
   `Instrucciones.mp4`, `Historia.mp4` y `Historia_narrada.mp3`.
2. Generar el build normalmente desde Unity. Antes del build,
   `HistoriaStreamingAssetsBuildProcessor` copia esos archivos a
   `StreamingAssets`.
3. Si se quiere sincronizar manualmente antes de construir, usar
   `GoadVerse -> Sync Historia Streaming Assets`.

Para distribuir el juego en otra computadora:

1. No enviar solo `GoadVerse.exe`. Unity necesita el ejecutable y tambien la
   carpeta `GoadVerse_Data`.
2. Comprimir y enviar la carpeta completa generada por Unity, por ejemplo:
   `GoadVerse.exe`, `GoadVerse_Data`, `UnityPlayer.dll` y demas archivos del
   build.
3. Confirmar que dentro de la carpeta del build exista:
   `GoadVerse_Data/StreamingAssets/Historia`.
4. Dentro de esa carpeta deben estar `Instrucciones.mp4`, `Historia.mp4` y
   `Historia_narrada.mp3`. Si falta esa carpeta, los videos no se veran en otra
   computadora.

## Convenciones de trabajo

- Guardar escenas antes de subir cambios.
- Revisar `git status` antes de hacer commit.
- Mantener assets importados en carpetas separadas por origen.
- Agregar credito y licencia cuando se importe un asset externo.
- Probar el flujo completo: `Bootstrap -> Menu_Principal -> Lobby -> minijuego1`.
- Probar tambien el flujo narrativo:
  `Play -> Instrucciones.mp4 -> Historia.mp4 + Historia_narrada.mp3 -> Lobby`.
- Revisar el menu principal despues de tocar el Canvas o la copa, porque el
  fondo debe quedar detras del modelo 3D.
- Antes de distribuir un `.exe`, confirmar que los videos de historia se
  copiaron a `StreamingAssets/Historia`.
- Para compartir el juego, enviar la carpeta completa del build o un `.zip` de
  esa carpeta. Enviar solo el `.exe` no incluye escenas, datos ni videos.
- En escenas serializadas de Unity, limpiar cualquier conflicto de merge antes
  de abrir Play Mode.

## Creditos y agradecimientos

Gracias a las personas y grupos que hicieron posibles varios assets usados o
importados en este proyecto:

- "Copa mundial - Cup World" (https://skfb.ly/pKnU6) by marcoseec is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
- "Craven Cottage Stadium - Game Ready Asset (FREE)" (https://sketchfab.com/3d-models/craven-cottage-stadium-game-ready-asset-free-32ffa67d18974419aac433d64a223b12) by HxZy is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
- "Soccer Goal" (https://skfb.ly/pD9X8) by TepidGames is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
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
- Assets by genesismayuri:
  - `Assets/Models/ballon/Ball.blend`
  - `Assets/Textures/Menu_Inicio/Menu_Inicio.jpeg`
  - `Assets/Textures/Menu_Inicio/Fondo/Fondo_Menu.png`
  - `Assets/Textures/Botones/*.png`
  - `Assets/Audio/BotonSonido.wav`
  - `Assets/Audio/Inicio/Title_Juego.mp3`
  - `Assets/Audio/8Bit Music - 062022/*.wav`
- TextMesh Pro, Unity UI, Input System, URP and other Unity packages are provided by Unity Technologies under their package and Unity terms.

## Notas de licencia

Este README no reemplaza las licencias originales de cada asset. Cuando se
redistribuya el proyecto, conservar los archivos de licencia incluidos en las
carpetas de los paquetes importados, especialmente:

- `Assets/Textures/UI/Alebardium/Bloodlines UI/Third-Party Notices.txt`
- `Assets/Textures/UI/Alebardium/Bloodlines UI/Fonts/ManufacturingConsent/OFL.txt`
- `Assets/Textures/UI/Alebardium/Bloodlines UI/Fonts/MedievalSharp/OFL.txt`
- Licencias incluidas con TextMesh Pro y fuentes usadas por Unity.
