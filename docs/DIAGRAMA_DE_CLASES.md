# Diagrama de Clases — Imperios en Guerra

## 1. Objetivo del diagrama

La guía del proyecto exige que el **diagrama de clases UML ilustre la arquitectura MVC y las relaciones entre las clases fundamentales**. Por eso este documento no presenta únicamente la jerarquía de unidades: primero muestra de forma explícita **Modelo, Controlador y Vista**, y después detalla el dominio principal.

### 1.1 Justificación de los elementos fundamentales

Se seleccionan estas clases y servicios porque representan el flujo esencial exigido por la guía:

- `Partida` y `Jugador`: concentran participantes, estado global y condición de finalización.
- `Mapa`, `Recurso`, `Unidad` y `Edificio`: representan el estado lógico que cambia al mover, recolectar, construir, entrenar y atacar.
- `ControladorSeleccion`, `ControladorAcciones` y `ControladorConexionApi`: reciben la interacción de Unity y coordinan las solicitudes sin trasladar reglas de negocio a la Vista.
- `VistaPartida` y `VistaHud`: representan gráficamente el estado sin decidir reglas del juego.
- `EstadoPartidaService` y `ServicioAccionesConcurrentes`: coordinan las operaciones de aplicación y el acceso sincronizado al Modelo.
- `GestorProcesosConcurrentes`: evidencia la ejecución con Task/ThreadPool y la cancelación de procesos.
- `ServicioRedPartida` y `DespachadorMensajesRed`: representan la comunicación WebSocket + JSON requerida técnicamente por la guía original.
- `ServicioArchivos`: concentra la generación de los archivos requeridos mediante `System.IO`.

La regla arquitectónica aplicada en el proyecto es:

```text
MODELO      = estado + reglas + validaciones del juego
CONTROLADOR = recibe/coordina acciones y conecta Vista con Modelo
VISTA       = Unity: escena + sprites + HUD + interacción visual
```

Regla central:

```text
C# = cerebro del juego
Unity = principalmente Vista
Controladores = puente
```

---

## 2. UML principal — Arquitectura MVC

Este es el diagrama principal de la arquitectura MVC. Los bloques muestran qué clases pertenecen a cada responsabilidad.

```mermaid
classDiagram
direction LR

namespace MODELO_CSHARP_POCO {
    class Partida {
        <<Modelo>>
        +Jugador JugadorHumano
        +IReadOnlyList~Jugador~ Jugadores
        +bool Finalizada
        +Jugador Ganador
        +IntentarFinalizar(Jugador, string)
        +ObtenerEnemigos(Jugador)
    }

    class Jugador {
        <<Modelo>>
        +string Nombre
        +TipoJugador Tipo
        +Mapa Mapa
        +RecursosJugador Recursos
        +IReadOnlyList~Unidad~ Unidades
        +IReadOnlyList~Edificio~ Edificios
    }

    class Mapa {
        <<Modelo>>
        +int Ancho
        +int Alto
        +PuedeColocar(Coordenada)
        +ObtenerCasilla(int, int)
    }

    class Recurso {
        <<Modelo>>
        +TipoRecurso Tipo
        +int CantidadRestante
        +Extraer(int)
    }

    class RecursosJugador {
        <<Modelo>>
        +Agregar(TipoRecurso, int)
        +IntentarGastar(CostoRecursos)
    }

    class Unidad {
        <<Modelo abstracto>>
        +Guid Id
        +Coordenada Coordenada
        +int VidaActual
        +int VidaMaxima
        +TipoAccionJuego OrdenActiva
        +RecibirDanio(int)
        +IntentarIniciarOrden(TipoAccionJuego)
    }

    class Aldeano {
        <<Modelo>>
        +int CapacidadCarga
        +int CargaActual
    }

    class Soldado {
        <<Modelo abstracto>>
    }

    class Monje {
        <<Modelo>>
        +int CantidadCuracion
        +int AlcanceCuracion
    }

    class Edificio {
        <<Modelo abstracto>>
        +Guid Id
        +Coordenada Coordenada
        +int VidaActual
        +RecibirDanio(int)
    }

    class CentroUrbano {
        <<Modelo>>
    }
}

namespace CONTROLADOR_Y_APLICACION {
    class ControladorSeleccion {
        <<Controlador Unity>>
        +SeleccionActual
        +BloquearInteraccion()
        +LimpiarSeleccion()
    }

    class ControladorAcciones {
        <<Controlador Unity>>
        +PrepararAccion(string)
    }

    class ControladorConexionApi {
        <<Controlador Unity / Adaptador>>
        +MoverUnidad(...)
        +IniciarRecoleccion(...)
        +Construir(...)
        +Entrenar(...)
        +Atacar(...)
        +Curar(...)
    }

    class EstadoPartidaService {
        <<Controlador de aplicación>>
        +EstablecerPartida(Partida)
        +ObtenerEstado()
        +MoverUnidad(...)
        +Atacar(...)
        +Curar(...)
    }

    class ServicioAccionesConcurrentes {
        <<Controlador de procesos>>
        +IniciarMovimiento(...)
        +IniciarRecoleccion(...)
        +IniciarConstruccion(...)
        +IniciarEntrenamiento(...)
        +IniciarAtaque(...)
        +IniciarCuracion(...)
        +CancelarTodos()
    }

    class ServicioJugadorMaquina {
        <<Controlador IA>>
        +Iniciar()
        +Detener()
        +EjecutarPaso()
    }

    class ServicioSesionJuego {
        <<Controlador sesión>>
        +Activar()
        +PausarTemporal()
        +ReanudarTemporal()
        +Pausar()
    }
}

namespace VISTA_UNITY {
    class VistaPartida {
        <<Vista>>
        +Renderizar(EstadoPartidaDto)
        +Sincronizar(EstadoPartidaDto)
        +ActualizarMovimientoUnidad(...)
    }

    class VistaHud {
        <<Vista>>
        +MostrarRecursos(...)
        +MostrarSeleccion(...)
        +MostrarMensaje(...)
        +MostrarResultadoFinal(...)
    }

    class VistaMenuInicial {
        <<Vista>>
    }

    class VistaMenuPausa {
        <<Vista>>
    }

    class EntidadSeleccionableVista {
        <<Vista>>
        +TipoLogico
        +Propietario
        +VidaActual
        +ActualizarDatosLogicos(...)
    }
}

namespace SERVICIOS_TRANSVERSALES {
    class GestorProcesosConcurrentes {
        <<Concurrencia>>
        +Iniciar(...)
        +Cancelar(Guid)
        +CancelarTodos()
    }

    class ServicioArchivos {
        <<System.IO>>
        +GuardarConfiguracionInicial(Partida)
        +RegistrarEvento(string)
        +GuardarResultadoPartidaFinalizada(Partida)
    }

    class ServicioRedPartida {
        <<Networking>>
        +AtenderClienteAsync(...)
    }

    class DespachadorMensajesRed {
        <<Networking>>
        +DespacharAsync(...)
    }
}

Unidad <|-- Aldeano
Unidad <|-- Soldado
Unidad <|-- Monje
Edificio <|-- CentroUrbano

Partida "1" o-- "1..*" Jugador
Jugador "1" --> "1" Mapa
Jugador "1" --> "1" RecursosJugador
Jugador "1" o-- "*" Unidad
Jugador "1" o-- "*" Edificio
Mapa "1" o-- "*" Recurso

ControladorSeleccion --> VistaPartida : interpreta clic/posición
ControladorAcciones --> ControladorSeleccion : usa selección
ControladorAcciones --> ControladorConexionApi : solicita acción
ControladorAcciones --> VistaHud : informa al usuario

ControladorConexionApi --> VistaPartida : actualiza snapshot
ControladorConexionApi --> VistaHud : actualiza HUD
ControladorConexionApi ..> EstadoPartidaService : REST / JSON

EstadoPartidaService --> Partida : consulta/modifica
ServicioAccionesConcurrentes --> EstadoPartidaService : ejecuta pasos
ServicioJugadorMaquina --> EstadoPartidaService : consulta estado
ServicioJugadorMaquina --> ServicioAccionesConcurrentes : inicia acciones
ServicioSesionJuego --> ServicioAccionesConcurrentes : pausa/cancela

ServicioAccionesConcurrentes --> GestorProcesosConcurrentes : Task/ThreadPool
EstadoPartidaService --> ServicioArchivos : registra estado/eventos
ServicioRedPartida --> DespachadorMensajesRed
DespachadorMensajesRed --> ServicioAccionesConcurrentes

VistaPartida o-- EntidadSeleccionableVista : representa
```

## Cómo leer este UML

### MODELO

El bloque **MODELO C# POCO** contiene el estado y las reglas principales.

Ejemplos:

- `Partida` decide si la partida finalizó;
- `Jugador` mantiene unidades, edificios y recursos;
- `Mapa` valida posiciones;
- `RecursosJugador` controla saldos y gastos;
- `Unidad` y `Edificio` contienen vida y estado.

Estas clases no necesitan `GameObject`, `Transform`, sprites ni otros objetos de `UnityEngine`.

### VISTA

El bloque **VISTA UNITY** representa gráficamente el estado.

Ejemplos:

- `VistaPartida` dibuja/sincroniza mapa, unidades, edificios y recursos;
- `VistaHud` muestra recursos, selección y resultados;
- `VistaMenuInicial` y `VistaMenuPausa` manejan presentación de menús;
- `EntidadSeleccionableVista` representa visualmente una entidad lógica.

La Vista no decide costos, daño, victoria ni reglas de ocupación.

### CONTROLADOR

El bloque **CONTROLADOR Y APLICACIÓN** es el puente.

Ejemplo de movimiento:

```text
clic del jugador
→ ControladorSeleccion
→ ControladorAcciones
→ ControladorConexionApi
→ API / EstadoPartidaService
→ Modelo valida y modifica
→ snapshot de respuesta
→ VistaPartida actualiza gráficos
```

Por eso el Controlador está entre Vista y Modelo y no reemplaza a ninguno de los dos.

### SERVICIOS TRANSVERSALES

No todo encaja estrictamente como entidad MVC. El proyecto también tiene servicios técnicos:

- `GestorProcesosConcurrentes`: ejecución concurrente;
- `ServicioArchivos`: `System.IO`;
- `ServicioRedPartida`: comunicación WebSocket + JSON;
- `DespachadorMensajesRed`: JSON/red.

Estos apoyan al Controlador y al Modelo sin convertir la Vista en responsable de lógica del juego.

---

## 3. UML del dominio del Modelo

Este segundo diagrama muestra con más detalle la parte de POO del Modelo.

```mermaid
classDiagram
direction TB

class Partida {
    +Jugador JugadorHumano
    +IReadOnlyList~Jugador~ Jugadores
    +IReadOnlyList~Jugador~ JugadoresMaquina
    +bool Finalizada
    +Jugador Ganador
    +string MotivoFinalizacion
    +BuscarJugadorPorUnidad(Guid)
    +BuscarJugadorPorEdificio(Guid)
    +ObtenerEnemigos(Jugador)
    +SonAliados(Jugador, Jugador)
    +SonEnemigos(Jugador, Jugador)
    +IntentarFinalizar(Jugador, string)
}

class Jugador {
    +string Nombre
    +TipoJugador Tipo
    +Mapa Mapa
    +RecursosJugador Recursos
    +IReadOnlyList~Unidad~ Unidades
    +IReadOnlyList~Edificio~ Edificios
    +IReadOnlyList~ObraConstruccion~ ObrasConstruccion
    +AgregarUnidad(Unidad)
    +EliminarUnidad(Unidad)
    +AgregarEdificio(Edificio)
    +EliminarEdificio(Edificio)
}

class Mapa {
    +int Ancho
    +int Alto
    +ObtenerCasilla(int, int)
    +PuedeColocar(Coordenada)
    +ColocarRecurso(Recurso)
    +ObtenerRecursoEn(Coordenada)
}

class Casilla {
    +bool EsTransitable
    +bool EstaOcupada
    +Ocupar()
    +Liberar()
}

class Coordenada {
    +int X
    +int Y
}

class Recurso {
    +TipoRecurso Tipo
    +Coordenada Coordenada
    +int CantidadRestante
    +bool Agotado
    +Extraer(int)
}

class RecursosJugador {
    +ObtenerCantidad(TipoRecurso)
    +Agregar(TipoRecurso, int)
    +PuedePagar(TipoRecurso, int)
    +IntentarGastar(CostoRecursos)
}

class Unidad {
    <<abstract>>
    +Guid Id
    +Coordenada Coordenada
    +bool Disponible
    +EstadoUnidad Estado
    +TipoAccionJuego OrdenActiva
    +int VidaMaxima
    +int VidaActual
    +int DanioAtaque
    +int AlcanceAtaque
    +RecibirDanio(int)
    +RecuperarVida(int)
    +IntentarIniciarOrden(TipoAccionJuego)
    +CancelarOrden()
    +CompletarOrden()
}

class Aldeano {
    +int CapacidadCarga
    +int CargaActual
    +TipoRecurso TipoCarga
    +RecolectarDesde(Recurso, int)
    +VaciarCarga(...)
}

class Soldado {
    <<abstract>>
}

class Guerrero
class Lancero
class Arquero

class Monje {
    +int CantidadCuracion
    +int AlcanceCuracion
    +double IntervaloCuracionSegundos
}

class Edificio {
    <<abstract>>
    +Guid Id
    +Coordenada Coordenada
    +int VidaMaxima
    +int VidaActual
    +RecibirDanio(int)
}

class CentroUrbano {
    +bool EstaEntrenando
    +IReadOnlyList~EntrenamientoPendiente~ ColaEntrenamiento
    +EncolarEntrenamiento(...)
    +AvanzarEntrenamiento(...)
    +CompletarEntrenamiento(...)
}

class EntrenamientoPendiente
class ObraConstruccion

Unidad <|-- Aldeano
Unidad <|-- Soldado
Unidad <|-- Monje
Soldado <|-- Guerrero
Soldado <|-- Lancero
Soldado <|-- Arquero
Edificio <|-- CentroUrbano

Partida "1" o-- "1..*" Jugador
Jugador "1" o-- "*" Unidad
Jugador "1" o-- "*" Edificio
Jugador "1" o-- "*" ObraConstruccion
Jugador "1" --> "1" RecursosJugador
Jugador "1" --> "1" Mapa

Mapa "1" o-- "*" Casilla
Mapa "1" o-- "*" Recurso

Recurso --> Coordenada
Unidad --> Coordenada
Edificio --> Coordenada
ObraConstruccion --> Coordenada

CentroUrbano "1" o-- "*" EntrenamientoPendiente
```

---

## 4. Dependencias permitidas en MVC

La arquitectura del proyecto puede resumirse así:

```text
┌──────────────────────────────┐
│          VISTA UNITY         │
│ sprites · HUD · menús · mapa │
└──────────────┬───────────────┘
               │ eventos / datos visuales
               ▼
┌──────────────────────────────┐
│         CONTROLADOR          │
│ entrada · API · coordinación │
│ concurrencia · IA · sesión   │
└──────────────┬───────────────┘
               │ solicita reglas/operaciones
               ▼
┌──────────────────────────────┐
│            MODELO            │
│ estado · reglas · validación │
│ mapa · unidades · victoria   │
└──────────────────────────────┘
```

Dependencias que se evitan:

```text
MODELO  ─X→ UnityEngine
VISTA   ─X→ modificar directamente reglas internas
WORKER  ─X→ GameObject / Transform / SpriteRenderer
```

Comunicación esperada de un worker:

```text
Task / ThreadPool
→ Modelo C#
→ resultado/estado thread-safe
→ API
→ Main Thread de Unity
→ Vista
```

---

## 5. Correspondencia con carpetas reales

| Capa | Ubicación principal | Clases representativas | Responsabilidad |
|---|---|---|---|
| **Modelo** | `src/ImperiosEnGuerra.Modelo/` | `Partida`, `Jugador`, `Mapa`, `Unidad`, `Recurso`, `Edificio` | Estado, reglas y validaciones |
| **Controlador Unity** | `Assets/Scripts/Controladores/` | `ControladorSeleccion`, `ControladorAcciones`, `ControladorConexionApi` | Recibir interacción y coordinar |
| **Controlador/aplicación** | API + servicios | `EstadoPartidaService`, `ServicioAccionesConcurrentes`, `ServicioJugadorMaquina`, `ServicioSesionJuego` | Ejecutar casos de uso sobre el Modelo |
| **Vista** | `Assets/Scripts/Vistas/` | `VistaPartida`, `VistaHud`, `VistaMenuInicial`, `VistaMenuPausa` | Representación gráfica |
| **Servicios transversales** | Servicios/API | `GestorProcesosConcurrentes`, `ServicioArchivos`, `ServicioRedPartida` | Concurrencia, IO y networking |

---

## 6. Ejemplo MVC real: atacar

```text
1. VISTA
   El jugador ve y selecciona un Guerrero.

2. CONTROLADOR
   ControladorSeleccion conserva la selección.
   ControladorAcciones prepara "Atacar".
   ControladorConexionApi envía la intención.

3. CONTROLADOR / APLICACIÓN
   ServicioAccionesConcurrentes inicia el worker.
   EstadoPartidaService coordina la modificación.

4. MODELO
   El Modelo valida propietario, objetivo, alcance y vida.
   Se aplica daño y, si corresponde, se elimina la entidad.
   Se evalúa la condición de victoria.

5. VISTA
   Unity recibe el nuevo snapshot.
   VistaPartida actualiza vida/sprite.
   VistaHud actualiza estado o pantalla final.
```

Este ejemplo deja visible que **Unity no decide el daño ni la victoria** y que **el Modelo no dibuja sprites ni manipula GameObjects**.

---
