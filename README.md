# CitadelsBoardGame

QUE ESTÁ COMPLETADO:
  - El juego permite elegir personaje.
  - El jugador con corona empezará escogiendo primero.
  - En el turno el jugador escoge entre oro y cartas (la lógica está completa).
  - Asesino, Ladrón y Arquitecto funcionan a la totalidad.
  - La Mano tiene funcionalidad completa.
  - El jugador puede jugar cartas (Se borran de la mano, te cobran el oro) pero la funcionalidad posterior(mostrarla en la mesa) no está realizada.
  - El jugador puede terminar turno
  - La interfaz está bastante encaminada
  
QUE NO ESTÁ COMPLETADO:
  - Animaciones de carta (de mazo a mano, de retorno de carta al soltar, de construcción).
  - Habilidades de Mago, Rey(La corona sí que funciona correctamente), Obispo, Mercader, Guerrero y Artista necesitan ser desarrolladas
    Todos los personajes heredan de la clase abstracta Character que implementa 2 métodos IsPassive() -> determina si la habilidad se ejecuta de manera pasiva
    y SetSkill() -> configurara todo lo necesario para que el jugador pueda ejecutar la habilidad del personaje
  
  - Funcionalidad de Jugar carta parcialmente desarrollada(El jugador puede arrastras carta al tablero, se le cobra el oro y la carta desaparece de su mano) pero a nivel lógico
    y visual es como si simplemente se hubiese borrado la carta. Las clases PlayableCard y Gameplayer gestionan la generación de la clase PlayedCard. La clase PlayedCard es
    quien gestione la lógica de la carta jugada. NOTA!! (el jugador no debería poder jugar 2 distritos iguales).
    
  - Interfaz de usuario (no hay información del resto de usuarios, no hay un log de acciones, ..).
  - Retoques en la interfaz ya construida.
