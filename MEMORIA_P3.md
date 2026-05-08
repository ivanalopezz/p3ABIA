# Memoria Tecnica - Practica 3 (Busqueda Adversaria)

## 1. Introduccion

Este proyecto implementa una version reducida de Conecta 4 (tablero de 4x5) en C#, con dos modos de juego:

- Humano vs Agente.
- Agente vs Agente.

La parte central de inteligencia artificial es un agente que decide movimientos con el algoritmo **Minimax** y optimizacion por **poda alfa-beta**, tal y como se trabaja en la teoria de busqueda adversaria.

## 2. Objetivo de la practica

El objetivo general es modelar un juego competitivo de informacion completa, determinista, de dos jugadores y suma cero, y resolver la toma de decisiones del agente con una estrategia racional:

- Jugador MAX: intenta maximizar la utilidad.
- Jugador MIN: intenta minimizar la utilidad de MAX.

En este proyecto:

- Las fichas son R (roja) y A (amarilla).
- En modo Humano vs Agente, el humano juega con R y empieza.
- El agente usa Minimax para escoger la mejor columna legal.

## 3. Arquitectura del proyecto

El proyecto esta dividido en tres clases principales:

1. Program:
- Control de flujo general.
- Menu inicial y modos de juego.
- Interaccion por consola.

2. Tablero:
- Representacion del estado del juego.
- Reglas del dominio (movimientos, insercion, ganador, terminalidad).

3. Minimax:
- Busqueda adversaria.
- Poda alfa-beta.
- Evaluacion heuristica cuando no se llega a terminal completo.


## 4. Descripcion detallada por archivo

### 4.1 Program.cs

Esta clase coordina la ejecucion completa del juego.

#### 4.1.1 Parametros globales del agente

Se fijan dos constantes:

- ProfundidadAgente = 7: limite de profundidad para la exploracion del arbol.
- TiempoMaximoDecisionMs = 500: limite de 500 ms por decision (0 significaria sin limite).

Se crean dos agentes Minimax:

- AgenteAmarillo: MAX = amarilla, MIN = roja.
- AgenteRojo: MAX = roja, MIN = amarilla.

Esto permite reutilizar la misma clase para ambos colores segun el modo.

#### 4.1.2 Punto de entrada

Main() hace:

1. Mostrar cabecera.
2. Pedir modo de juego (1 o 2).
3. Ejecutar:
- JugarHumanoVsAgente() si modo == 1.
- JugarAgenteVsAgente() en otro caso.

#### 4.1.3 Interaccion y validacion de menu

PedirModoDeJuego() mantiene un bucle hasta recibir una opcion valida:

- Usa int.TryParse.
- Solo acepta 1 o 2.
- Si hay error, informa y vuelve a pedir.

Es una validacion robusta para evitar caidas por entrada invalida.

#### 4.1.4 Modo Humano vs Agente

JugarHumanoVsAgente():

- Crea un tablero vacio.
- Inicializa turno en roja (R), cumpliendo que el humano empieza.
- Mientras el estado no sea terminal:
- Muestra tablero.
- Si toca humano:
- Pide columna con PedirColumnaHumana().
- Inserta ficha roja.
- Si toca agente:
- Calcula mejor jugada con AgenteAmarillo.ElegirMejorMovimiento(tablero).
- Inserta ficha amarilla.
- Muestra jugada elegida.
- Cambia turno.
- Al final, muestra tablero final y resultado.

#### 4.1.5 Modo Agente vs Agente

JugarAgenteVsAgente():

- Tambien crea tablero vacio y empieza roja.
- Ambos turnos se resuelven con Minimax (uno con AgenteRojo, otro con AgenteAmarillo).
- Incluye Thread.Sleep(350) para que la salida sea legible.
- Termina mostrando tablero final y ganador/empate.

Este modo es util para evaluar comportamiento del algoritmo sin intervencion humana.

#### 4.1.6 Entrada del humano y legalidad de jugada

PedirColumnaHumana(Tablero tablero) valida:

- Que el dato sea entero.
- Que la columna sea legal (tablero.EsMovimientoValido(columna)), es decir:
- Dentro de rango.
- No llena.

Solo retorna cuando hay una accion valida.

#### 4.1.7 Funciones de apoyo

- MostrarTablero: imprime estado actual usando ToString() de Tablero.
- CambiarTurno: alterna entre roja y amarilla.
- MostrarResultadoFinal: comprueba HayGanador(R) y HayGanador(A), y si no, declara empate.

---

### 4.2 Tablero.cs

Tablero modela completamente el estado de Conecta 4 (4 filas x 5 columnas).

#### 4.2.1 Constantes y representacion

- Filas = 4, Columnas = 5.
- Vacia = '.', Roja = 'R', Amarilla = 'A'.
- Matriz privada char[,] _celdas.

Esta separacion de constantes mejora legibilidad y evita "numeros magicos".

#### 4.2.2 Inicializacion y clonacion

- Constructor por defecto: rellena toda la matriz con Vacia.
- Constructor privado con matriz: usado al clonar.
- Clonar(): copia profunda con Array.Copy, indispensable para Minimax (evita modificar el estado original durante simulaciones).

#### 4.2.3 Generacion y comprobacion de acciones

- EsMovimientoValido(int columna):
- Verifica rango [0, Columnas-1].
- Verifica hueco en la fila superior de la columna.
- ObtenerColumnasDisponibles():
- Recorre columnas y devuelve solo acciones legales.

Estas funciones implementan ACCIONES(estado) del marco teorico.

#### 4.2.4 Transicion de estado

- InsertarFicha(int columna, char ficha):
- Primero valida accion.
- Recorre desde la fila inferior hacia arriba para simular gravedad.
- Inserta en el primer hueco.
- Devuelve true/false.

Esta funcion implementa la dinamica real del juego.

#### 4.2.5 Condiciones de fin de partida

- EstaLleno(): revisa fila superior para detectar si ya no hay jugadas.
- HayGanador(char ficha): comprueba 4 en raya en:
- Horizontal.
- Vertical.
- Diagonal descendente (\).
- Diagonal ascendente (/).
- EsTerminal(): verdadero si gana roja, gana amarilla o hay tablero lleno.

Con esto queda implementado el TERMINAL-TEST(estado).

#### 4.2.6 Visualizacion

- ToString() construye una cuadricula textual con separadores |.
- Al final imprime indices 0 1 2 3 4.

Esto facilita que el humano identifique columnas validas en consola.

---

### 4.3 Minimax.cs

Esta clase implementa el agente inteligente con estructura clasica de teoria:

- DecisionMinimax.
- ValorMax.
- ValorMin.
- PruebaTerminal.
- Utilidad.
- Heuristica para hojas no terminales por corte.

#### 4.3.1 Parametros y validaciones

Constructor:

- Exige que MAX y MIN no tengan la misma ficha.
- Exige profundidad minima 1.
- Guarda:
- fichas de MAX y MIN,
- profundidad maxima,
- limite de tiempo por decision.

#### 4.3.2 Decision principal

DecisionMinimax(Tablero estadoActual):

1. Obtiene acciones legales.
2. Si no hay acciones, retorna -1.
3. Inicializa:
- v = -infinito,
- alfa = -infinito,
- beta = +infinito.
4. Establece instante limite si hay tiempo maximo.
5. Para cada accion:
- Genera estado sucesor con Resultado.
- Evalua con ValorMin(...).
- Actualiza mejor accion y alfa.
- Si se agota tiempo y ya hay al menos una accion evaluada, corta y devuelve mejor parcial.

Esto conserva una decision valida incluso con corte temporal.

#### 4.3.3 Recursion MAX/MIN con poda alfa-beta

- ValorMax:
- Si estado terminal/corte, devuelve utilidad.
- Si no, explora hijos con ValorMin.
- Actualiza alfa.
- Si alfa >= beta, poda.

- ValorMin:
- Simetrico a MAX.
- Explora con ValorMax.
- Actualiza beta.
- Si alfa >= beta, poda.

La poda no altera el resultado minimax final, pero reduce expansion de nodos.

#### 4.3.4 Funcion Resultado

Resultado(estado, accion, fichaTurno):

- Clona tablero.
- Inserta ficha en la columna de accion.
- Devuelve nuevo estado.

Esto refleja RESULTADO(estado, accion) del pseudocodigo teorico.

#### 4.3.5 Test terminal y corte por recursos

PruebaTerminal devuelve true si:

- El estado del juego es terminal.
- Queda profundidadRestante == 0.
- Se agota tiempo.

Asi se combinan corte estructural (fin real), corte por profundidad y corte temporal.

#### 4.3.6 Funcion de utilidad y preferencia temporal

Utilidad(estado, profundidadRestante):

- Victoria MAX: +100000 + profundidadRestante.
- Victoria MIN: -100000 - profundidadRestante.
- Empate: 0.
- No terminal en hoja de corte: heuristica.

Sumar/restar profundidadRestante introduce preferencia por:

- Ganar antes.
- Perder mas tarde.

#### 4.3.7 Heuristica basada en ventanas de 4

EvaluacionHeuristica recorre celdas y puntua ventanas en 4 direcciones:

- Horizontal.
- Vertical.
- Diagonal descendente.
- Diagonal ascendente.

PuntuarVentanaSiExiste:

- Descarta ventanas fuera de limites.
- Cuenta fichas MAX, MIN y vacias.
- Llama a PuntuarConteoVentana.

PuntuarConteoVentana:

- Si hay mezcla MAX+MIN, valor 0 (ventana bloqueada).
- MAX:
- 3 + 1 vacia => +100.
- 2 + 2 vacias => +10.
- 1 + 3 vacias => +1.
- MIN simetrico con valores negativos.

Es una heuristica simple y coherente con el objetivo estrategico de construir lineas de 4.



## 5. Flujo global de ejecucion

1. Inicio en Main.
2. Seleccion de modo.
3. Bucle principal de turnos mientras !EsTerminal().
4. En cada turno:
- Humano: entrada validada.
- Agente: decision Minimax.
5. Insercion de ficha y cambio de turno.
6. Al finalizar, impresion de tablero y anuncio de resultado.

## 6. Correspondencia con la teoria de busqueda adversaria

La implementacion respeta los bloques teoricos principales:

- ACCIONES(estado) -> ObtenerColumnasDisponibles.
- RESULTADO(estado, accion) -> Resultado (clonar + insertar).
- TERMINAL-TEST(estado) -> EsTerminal y PruebaTerminal.
- UTILIDAD(estado) -> Utilidad.
- Alternancia MAX/MIN -> ValorMax y ValorMin.
- Optimizacion -> poda alfa-beta con condicion alfa >= beta.

Tambien incorpora aspectos practicos que suelen exigirse al pasar teoria a codigo:

- Limite de profundidad para controlar coste.
- Limite de tiempo para mantener respuesta interactiva.
- Heuristica para evaluar hojas no terminales.

## 7. Complejidad y rendimiento

Sin poda, Minimax crece aproximadamente como O(b^d):

- b: factor de ramificacion (numero de columnas legales, maximo 5).
- d: profundidad.

Con poda alfa-beta:

- En el mejor caso se reduce notablemente el numero de nodos expandidos.
- En el peor caso mantiene orden exponencial, pero en la practica mejora tiempo.

Con tablero 4x5 y profundidad 7, el coste es manejable para ejecucion por consola.

## 8. Robustez y buenas decisiones de implementacion

Aspectos positivos:

- Validacion fuerte de entradas de usuario.
- Separacion clara de responsabilidades (Program / Tablero / Minimax).
- Clonado de estados para evitar efectos laterales en busqueda.
- Compatibilidad con dos modos de juego en una misma arquitectura.
- Comentarios abundantes y alineados con pseudocodigo teorico.

## 9. Posibles mejoras futuras

1. Ordenacion de movimientos (por ejemplo centro primero) para mejorar eficacia de poda.
2. Tabla de transposiciones para reutilizar evaluaciones.
3. Ajuste fino de pesos heuristicos segun experimentacion.
4. Iterative deepening sobre el limite temporal.
5. Registro de metricas (nodos expandidos, podas realizadas, tiempo por turno).
6. Suite de tests unitarios para reglas de tablero y casos limite de Minimax.

## 10. Conclusiones

El proyecto implementa de forma correcta y clara una solucion de busqueda adversaria para Conecta 4 reducido. Se cumplen los requisitos funcionales (dos modos, tablero 4x5, humano empieza con roja, agente con minimax) y se aplican adecuadamente las tecnicas de IA estudiadas (Minimax, poda alfa-beta y heuristica bajo corte de profundidad/tiempo).

En conjunto, el codigo muestra una buena traduccion de la teoria a una implementacion practica mantenible y defendible en una presentacion de practica.
