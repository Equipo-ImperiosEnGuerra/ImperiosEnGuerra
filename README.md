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

El proyecto implementará concurrencia real mediante herramientas de C# como:

- Thread
- Task
- mecanismos de sincronización

La concurrencia se aplicará progresivamente en procesos como recolección, construcción, entrenamiento, movimiento y comunicación de red.

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

El proyecto se encuentra cerrando la **Fase 3 de gameplay base**.

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

Los costos de construcción/entrenamiento y las estadísticas de combate no se inventan mientras no estén definidos en los requisitos. La concurrencia real con `Task`/`Thread`, sincronización y cancelación corresponde a la Fase 4.