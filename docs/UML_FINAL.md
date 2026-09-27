# UML final — Imperios en Guerra

Este diagrama resume las clases y relaciones principales del código final. Se omiten DTOs y clases de resultado menores para mantener legible el modelo.

```mermaid
classDiagram
direction LR

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

class Casilla
class Coordenada
class Recurso
class RecursosJugador

class Unidad {
  <<abstract>>
  +Guid Id
  +Coordenada Coordenada
  +bool Disponible
  +EstadoUnidad Estado
  +TipoAccionJuego? OrdenActiva
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
  +TipoRecurso? TipoCarga
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

class CentroUrbano
class EntrenamientoPendiente
class ObraConstruccion

class EstadoPartidaService
class ServicioAccionesConcurrentes
class GestorProcesosConcurrentes
class ProcesoConcurrente
class ServicioJugadorMaquina
class ServicioReaccionesAutomaticas
class ServicioRegeneracionRecursos
class ServicioSesionJuego
class ServicioArchivos
class ServicioRedPartida
class DespachadorMensajesRed

class ControladorSeleccion
class ControladorAcciones
class ControladorConexionApi
class VistaPartida
class VistaHud

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

EstadoPartidaService --> Partida
EstadoPartidaService --> ServicioArchivos
ServicioAccionesConcurrentes --> EstadoPartidaService
ServicioAccionesConcurrentes --> GestorProcesosConcurrentes
GestorProcesosConcurrentes ..> ProcesoConcurrente

ServicioJugadorMaquina --> EstadoPartidaService
ServicioJugadorMaquina --> ServicioAccionesConcurrentes
ServicioReaccionesAutomaticas --> EstadoPartidaService
ServicioReaccionesAutomaticas --> ServicioAccionesConcurrentes
ServicioRegeneracionRecursos --> EstadoPartidaService
ServicioSesionJuego --> ServicioAccionesConcurrentes
ServicioSesionJuego --> ServicioJugadorMaquina
ServicioSesionJuego --> ServicioReaccionesAutomaticas
ServicioSesionJuego --> ServicioRegeneracionRecursos

ServicioRedPartida --> DespachadorMensajesRed
DespachadorMensajesRed --> ServicioAccionesConcurrentes

ControladorAcciones --> ControladorSeleccion
ControladorAcciones --> ControladorConexionApi
ControladorAcciones --> VistaHud
ControladorConexionApi --> VistaPartida
ControladorConexionApi --> VistaHud
ControladorSeleccion --> VistaPartida
```

## Lectura del diagrama

`Partida` es el agregado principal. Cada `Jugador` contiene unidades, edificios, obras, recursos económicos y referencia al mapa lógico.

`Unidad` concentra identidad, posición, disponibilidad, orden activa, vida y estadísticas. `Aldeano`, `Soldado` y `Monje` especializan ese comportamiento.

`EstadoPartidaService` centraliza el acceso sincronizado a la partida activa. `ServicioAccionesConcurrentes` crea procesos de gameplay y delega en `GestorProcesosConcurrentes`.

Unity consume DTOs de la API y mantiene la separación entre Controladores y Vistas, sin que los workers secundarios modifiquen directamente `UnityEngine`.
