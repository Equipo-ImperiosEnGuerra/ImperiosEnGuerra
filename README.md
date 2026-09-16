# Imperios en Guerra

Proyecto académico de Programación Orientada a Objetos desarrollado en C# y Unity.

**Imperios en Guerra** es un videojuego de estrategia en tiempo real (RTS) para dos jugadores, inspirado en Age of Empires.

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

La comunicación entre las dos instancias del juego se realizará mediante:

**WebSockets**

Los mensajes de red utilizarán estructuras definidas, preferiblemente mediante JSON.

## Flujo de Git

El proyecto utiliza las ramas principales:

- `main`: versiones estables.
- `develop`: integración del desarrollo.

El trabajo se realizará mediante ramas específicas por tarea.

Flujo general:

Issue → Branch → Desarrollo → Pruebas → Commit → Pull Request → Develop → Main

## Estado

Proyecto actualmente en fase de configuración técnica y construcción de la base del sistema.