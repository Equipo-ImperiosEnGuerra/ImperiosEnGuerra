# Imperios en Guerra

Proyecto académico de Programación Orientada a Objetos desarrollado en C# y Unity.

**Imperios en Guerra** es un videojuego de estrategia en tiempo real (RTS) inspirado en Age of Empires. La modalidad actual del proyecto es **Humano vs Máquina**.

## Tecnologías

- C#
- Unity
- Git
- GitHub
- WebSockets
- JSON
- System.IO
- Programación concurrente con Thread y Task

## Versión de Unity

El proyecto utiliza:

**Unity 6000.6.0f1**

Los integrantes del equipo deben utilizar la misma versión para evitar problemas de compatibilidad.

## Arquitectura

El proyecto será desarrollado utilizando el patrón:

**Modelo - Vista - Controlador (MVC)**

- **Modelo:** estado, reglas y lógica del juego mediante clases C#.
- **Vista:** representación gráfica mediante Unity.
- **Controlador:** comunicación entre la Vista y el Modelo.

## Concurrencia

La Fase 4 implementa concurrencia real mediante C#:

- `Task.Run` y ThreadPool;
- `CancellationToken`;
- `lock`;
- `ConcurrentDictionary`;
- `ConcurrentQueue`.

Movimiento, recolección, construcción y entrenamiento se ejecutan mediante workers fuera del Main Thread de Unity. Los resultados regresan de forma thread-safe y Unity actualiza Vista/HUD únicamente desde su Main Thread.

Detalle técnico: [docs/FASE_4_CONCURRENCIA.md](docs/FASE_4_CONCURRENCIA.md).

## Networking

El requisito de networking permanece pendiente de revisión en una fase posterior debido al cambio de modalidad a **Humano vs Máquina**. No se eliminará ni se sustituirá sin revisar nuevamente la guía del profesor.

La integración actual entre Unity y la lógica de aplicación utiliza una API local con mensajes JSON estructurados.

## Flujo de Git

El proyecto utiliza las ramas principales:

- `main`: versiones estables.
- `develop`: integración del desarrollo.

El trabajo se realizará mediante ramas específicas por tarea.

Flujo general:

Issue → Branch → Desarrollo → Pruebas → Commit → Pull Request → Develop → Main

## Estado

El proyecto se encuentra cerrando la **Fase 4 de concurrencia, sincronización y cancelación**.

Actualmente están integrados:

- mapa, recursos físicos, jugadores y Centros Urbanos;
- selección por clic y HUD contextual;
- movimiento y flujo base de recolección;
- construcción y entrenamiento;
- flujo base de ataque con validaciones;
- comunicación Unity -> API -> Modelo -> API -> Unity;
- mensajes de éxito/error y refresco de Vista;
- `log_partida.txt` centralizado mediante `ServicioArchivos`;
- pruebas automáticas .NET y EditMode de Unity.

Los costos de construcción/entrenamiento, cantidades/ritmos de recolección y estadísticas de combate no se inventan mientras no estén definidos en los requisitos. La Fase 4 incorpora concurrencia real para movimiento, recolección, construcción y entrenamiento, con sincronización, cancelación y comunicación segura hacia Unity.