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

1. **Program.cs**
   - Control de flujo general
   - Menu inicial y modos de juego
   - Interaccion por consola

2. **Tablero.cs**
   - Representacion del estado del juego
   - Reglas del dominio (movimientos, insercion, ganador, terminalidad)

3. **Minimax.cs**
   - Busqueda adversaria
   - Poda alfa-beta
   - Evaluacion heuristica cuando no se llega a terminal completo

## 4. Descripcion detallada por archivo

### 4.1 Program.cs

Esta clase coordina la ejecucion completa del juego.

#### 4.1.1 Parametros globales del agente

Se fija una constante:

- **ProfundidadAgente = 7**: limite de profundidad para la exploracion del arbol.

Se crean dos agentes Minimax:

- **AgenteAmarillo**: MAX = amarilla, MIN = roja.
- **AgenteRojo**: MAX = roja, MIN = amarilla.

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

- Crea un tablero vacio.
- Inicializa turno en roja (R).
- Mientras el estado no sea terminal:
  - Muestra tablero.
  - Si toca Rojo:
    - Usa AgenteRojo para elegir movimiento.
    - Inserta ficha roja.
    - Muestra jugada elegida.
  - Si toca Amarillo:
    - Usa AgenteAmarillo para elegir movimiento.
    - Inserta ficha amarilla.
    - Muestra jugada elegida.
  - Añade pausa de 350ms para poder seguir la partida en consola.
  - Cambia turno.
- Muestra tablero final y resultado.

#### 4.1.6 Funciones auxiliares

- **PedirColumnaHumana()**: Valida entrada del usuario.
  - Verifica que sea un entero.
  - Comprueba que sea un movimiento legal (tablero.EsMovimientoValido).
  - Solo retorna cuando hay una accion valida.
  
- **MostrarTablero()**: Imprime el estado actual usando ToString() de Tablero.

- **CambiarTurno()**: Alterna entre Roja y Amarilla.

- **MostrarResultadoFinal()**: 
  - Comprueba si gana roja con HayGanador(Roja).
  - Comprueba si gana amarilla con HayGanador(Amarilla).
  - Si no hay ganador, declara empate.

### 4.2 Tablero.cs

Modela completamente el estado de Conecta 4 (4 filas x 5 columnas).

#### 4.2.1 Constantes y representacion

- **Filas = 4**: Numero de filas del tablero.
- **Columnas = 5**: Numero de columnas del tablero.
- **Vacia = '.'**: Símbolo para celda vacía.
- **Roja = 'R'**: Fichas del jugador humano.
- **Amarilla = 'A'**: Fichas del agente.
- **_celdas**: Matriz privada char[4,5] que guarda el contenido.

Esta separacion de constantes mejora legibilidad y evita "numeros magicos".

#### 4.2.2 Inicializacion y clonacion

- **Constructor por defecto**: Reserva una matriz char[4,5] e inicializa todas las celdas con Vacia.

- **Constructor privado(char[,] celdas)**: Usado internamente para crear un tablero a partir de una matriz existente (al clonar).

- **Clonar()**: Crea una copia profunda del tablero.
  - Reserva nueva matriz.
  - Copia todas las celdas con Array.Copy().
  - Devuelve nuevo Tablero con la matriz copiada.
  - Es fundamental para Minimax (evita modificar el estado original durante simulaciones).

- **Indexador [fila, columna]**: Acceso de solo lectura a las celdas.

#### 4.2.3 Generacion y comprobacion de acciones

- **EsMovimientoValido(columna)**: Devuelve true si:
  - La columna esta en rango [0, Columnas-1].
  - La celda superior (fila 0) de esa columna esta vacia.

- **ObtenerColumnasDisponibles()**: Recorre todas las columnas.
  - Anade las columnas validas a una lista.
  - Devuelve lista de todas las acciones legales.

Estas funciones implementan ACCIONES(estado) del marco teorico.

#### 4.2.4 Transicion de estado

- **InsertarFicha(columna, ficha)**:
  - Primero valida que el movimiento sea legal.
  - Recorre desde la fila inferior (3) hacia arriba (0) para simular gravedad.
  - Inserta la ficha en el primer hueco encontrado.
  - Devuelve true si logra insertar, false si la columna era invalida/llena.

Esta funcion implementa la dinamica real del juego.

#### 4.2.5 Condiciones de fin de partida

- **EstaLleno()**: 
  - Revisa la fila superior (0).
  - Si encuentra una celda vacia, devuelve false (aun hay jugadas).
  - Si no hay ningun hueco arriba, devuelve true (tablero completamente lleno).

- **HayGanador(ficha)**: Comprueba si la ficha tiene 4 en raya.
  - Revisa 4 direcciones:
    1. **Horizontal**: ventanas de 4 columnas consecutivas en cada fila.
    2. **Vertical**: ventanas de 4 filas consecutivas en cada columna.
    3. **Diagonal descendente (\)**: ventanas (fila, col), (fila+1, col+1), ..., (fila+3, col+3).
    4. **Diagonal ascendente (/)**: ventanas (fila, col), (fila-1, col+1), ..., (fila-3, col+3).
  - Retorna true si encuentra 4 en raya, false en caso contrario.

- **EsTerminal()**: Devuelve true si:
  - Hay ganador de roja.
  - Hay ganador de amarilla.
  - El tablero esta lleno.

Con esto queda implementado el TERMINAL-TEST(estado) del marco teorico.

#### 4.2.6 Visualizacion

- **ToString()**: Construye una representacion de texto del tablero.
  - Usa StringBuilder para eficiencia.
  - Imprime cada fila con separadores verticales |.
  - Al final, imprime los indices de columna (0 1 2 3 4).
  - Ejemplo de salida:
    ```
    |.|.|.|.|.|
    |.|.|.|.|.|
    |R|.|A|.|.|
    |R|A|A|.|.|
     0 1 2 3 4
    ```

### 4.3 Minimax.cs

Implementa el algoritmo Minimax con poda alfa-beta siguiendo la estructura clasica de teoria.

#### 4.3.1 Constantes de utilidad

- **UtilidadVictoria = 100000000**: Valor para victoria de MAX.
- **UtilidadDerrota = -100000000**: Valor para derrota de MAX.
- **SinDistanciaTerminal = int.MaxValue**: Marcador para distancia indefinida.

Utilidades grandes diferenciadas para claridad.

#### 4.3.2 Estructura ResultadoBusqueda

Struct que encapsula el resultado de la evaluacion:

- **Valor**: El valor Minimax del nodo.
- **DistanciaVictoriaMax**: Profundidad a la que MAX podria ganar (si existe). Si MAX gana, es 0; si no es posible, es SinDistanciaTerminal.
- **DistanciaDerrotaMax**: Profundidad a la que MAX podria perder (si existe). Si MAX pierde, es 0; si no es posible, es SinDistanciaTerminal.

Metodos estaticos:

- **PeorParaMax()**: Inicializacion con Valor = int.MinValue, DistanciaVictoriaMax = SinDistanciaTerminal, DistanciaDerrotaMax = 0.

- **PeorParaMin()**: Inicializacion con Valor = int.MaxValue, DistanciaVictoriaMax = 0, DistanciaDerrotaMax = SinDistanciaTerminal.

Estos valores iniciales garantizan que cualquier movimiento evaluado sera mejor que estos peores casos.

#### 4.3.3 Constructor

```csharp
Minimax(char fichaMax, char fichaMin, int profundidadMaxima)
```

- Valida que fichaMax ≠ fichaMin (si son iguales, lanza ArgumentException).
- Valida que profundidadMaxima >= 1 (si es menor, lanza ArgumentOutOfRangeException).
- Inicializa:
  - _fichaMax con el color del jugador MAX.
  - _fichaMin con el color del jugador MIN.
  - _profundidadMaxima con el limite de profundidad.

#### 4.3.4 ElegirMejorMovimiento(tablero)

Alias publico que llama a DecisionMinimax(). Facilita que el resto del programa invoque el algoritmo de forma clara.

#### 4.3.5 DecisionMinimax(tablero) - Pseudocodigo: MINIMAX-DECISION

Funcion raiz que devuelve la accion (columna) que maximiza el valor minimax:

1. Obtiene todas las acciones legales con ObtenerColumnasDisponibles().
2. Si no hay acciones legales, retorna -1.
3. Inicializa:
   - alfa = int.MinValue (mejor valor garantizado para MAX).
   - beta = int.MaxValue (mejor valor garantizado para MIN).
   - mejoresAcciones = lista vacia de acciones empatadas con el mejor resultado.
   - hayMejorResultado = false.
   - mejorResultado = PeorParaMax() (peor resultado posible).

4. Para cada accion legal:
   - Genera estado sucesor con Resultado(estadoActual, accion, _fichaMax).
   - Evalua con ValorMin(estadoResultado, profundidadMaxima - 1, alfa, beta).
   - Suma un nivel a las distancias con SumarUnNivel().
   - Si el resultado es mejor para MAX (segun EsMejorParaMax):
     - Actualiza mejorResultado.
     - Limpia la lista mejoresAcciones.
     - Anade la accion actual como unica mejor accion provisional.
     - Marca hayMejorResultado = true.
   - Si el resultado empata exactamente con mejorResultado (segun SonResultadosEquivalentes):
     - Anade la accion actual a mejoresAcciones.
   - Actualiza alfa si mejorResultado.Valor > alfa.

5. Retorna una accion aleatoria de mejoresAcciones.

Este ultimo paso evita que, ante varias acciones completamente equivalentes, el agente elija siempre la primera columna del orden de exploracion. La aleatoriedad solo se aplica cuando las acciones empatan en valor minimax, distancia a victoria de MAX y distancia a derrota de MAX; por tanto, no empeora la calidad de la decision.

#### 4.3.6 ValorMax(estado, profundidadRestante, alfa, beta) - Pseudocodigo: VALOR-MAX

Calcula el valor del nodo MAX recursivamente:

1. Si PruebaTerminal(estado, profundidadRestante):
   - Retorna Utilidad(estado, _fichaMax).

2. Inicializa mejorResultado = PeorParaMax().

3. Obtiene acciones legales.

4. Si no hay acciones:
   - Retorna Utilidad(estado, _fichaMax) (hoja forzada).

5. Para cada accion:
   - Genera estado sucesor: Resultado(estado, accion, _fichaMax).
   - Llama ValorMin(estadoResultado, profundidadRestante - 1, alfa, beta).
   - Suma un nivel: SumarUnNivel(resultadoHijo).
   - Si es mejor para MAX:
     - Actualiza mejorResultado.
   - Actualiza alfa = max(alfa, mejorResultado.Valor).
   - Si alfa > beta:
     - PODA (break).

6. Retorna mejorResultado.

#### 4.3.7 ValorMin(estado, profundidadRestante, alfa, beta) - Pseudocodigo: VALOR-MIN

Calcula el valor del nodo MIN recursivamente (simetrico a ValorMax):

1. Si PruebaTerminal(estado, profundidadRestante):
   - Retorna Utilidad(estado, _fichaMin).

2. Inicializa mejorResultado = PeorParaMin().

3. Obtiene acciones legales.

4. Si no hay acciones:
   - Retorna Utilidad(estado, _fichaMin) (hoja forzada).

5. Para cada accion:
   - Genera estado sucesor: Resultado(estado, accion, _fichaMin).
   - Llama ValorMax(estadoResultado, profundidadRestante - 1, alfa, beta).
   - Suma un nivel: SumarUnNivel(resultadoHijo).
   - Si es mejor para MIN:
     - Actualiza mejorResultado.
   - Actualiza beta = min(beta, mejorResultado.Valor).
   - Si alfa > beta:
     - PODA (break).

6. Retorna mejorResultado.

La poda alfa-beta optimiza la busqueda sin alterar el resultado final, reduciendo significativamente nodos expandidos. La implementacion usa corte estricto (alfa > beta) para no descartar empates que podrian aportar un mejor criterio secundario de desempate.

#### 4.3.8 Resultado(estado, accion, fichaTurno) - Pseudocodigo: RESULTADO

Construye el estado sucesor tras aplicar una jugada:

1. Clona el tablero con estado.Clonar().
2. Inserta la ficha en la columna indicada.
3. Devuelve la copia modificada.

Esto refleja RESULTADO(estado, accion) del pseudocodigo teorico. Usar clonacion evita modificar el estado original.

#### 4.3.9 PruebaTerminal(estado, profundidadRestante) - Pseudocodigo: TERMINAL-TEST

Devuelve true si:

- El estado del juego es terminal (algun ganador o tablero lleno).
- Se alcanza profundidadRestante == 0 (corte por limite de profundidad).

Combina corte estructural (fin real del juego) con corte por profundidad.

#### 4.3.10 Utilidad(estado, fichaTurno) - Pseudocodigo: UTILIDAD

Evalua un nodo hoja en la busqueda:

- Si MAX ha ganado: retorna ResultadoBusqueda(UtilidadVictoria, 0, SinDistanciaTerminal).
- Si MIN ha ganado: retorna ResultadoBusqueda(UtilidadDerrota, SinDistanciaTerminal, 0).
- Si tablero esta lleno (empate): retorna ResultadoBusqueda(0, SinDistanciaTerminal, SinDistanciaTerminal).
- Si no es terminal pero llegamos por corte de profundidad:
  - Calcula heuristica evaluando potenciales lineas de 4.

#### 4.3.11 Funciones auxiliares

- **SumarUnNivel(resultado)**: Incrementa DistanciaVictoria y DistanciaDerrotaMax en 1 al subir un nivel en el arbol.

- **EsMejorParaMax(nuevo, actual)**: Compara dos resultados segun:
  1. Mejor valor Minimax.
  2. Si valores son iguales, prefiere victoria mas proxima.
  3. Si valores son iguales, prefiere derrota mas lejana.

- **SonResultadosEquivalentes(primero, segundo)**: Devuelve true si dos resultados tienen el mismo valor minimax, la misma distancia a victoria de MAX y la misma distancia a derrota de MAX. DecisionMinimax lo usa para agrupar acciones indistinguibles desde el punto de vista de la busqueda y elegir una de ellas aleatoriamente en la raiz mediante Random.Shared.

- **EsMejorParaMin(nuevo, actual)**: Compara dos resultados de forma simetrica:
  1. Mejor valor Minimax (minimizando).
  2. Criterios de desempate por distancias.

## 5. Flujo global de ejecucion

1. **Inicio**: Main() muestra cabecera y pide modo.
2. **Seleccion de modo**: Si 1 -> Humano vs Agente; Si 2 -> Agente vs Agente.
3. **Bucle principal**: Mientras !tablero.EsTerminal():
   - Mostrar tablero actual.
   - Si toca humano: pedir columna validada.
   - Si toca agente: invocar DecisionMinimax (que hace busqueda completa).
   - Insertar ficha en tablero.
   - Cambiar turno.
4. **Fin**: Mostrar tablero final y anunciar ganador/empate.

## 6. Correspondencia con la teoria de busqueda adversaria

La implementacion respeta los bloques teoricos principales:

| Concepto teorico | Implementacion |
|---|---|
| ACCIONES(estado) | ObtenerColumnasDisponibles() |
| RESULTADO(estado, accion) | Resultado() (clonar + insertar) |
| TERMINAL-TEST(estado) | EsTerminal() y PruebaTerminal() |
| UTILIDAD(estado) | Utilidad() |
| Alternancia MAX/MIN | ValorMax() y ValorMin() recursivos |
| Poda alfa-beta | Condicion alfa > beta en ambas funciones |
| Desempate final | SonResultadosEquivalentes() + seleccion aleatoria con Random.Shared |

Tambien incorpora aspectos practicos indispensables:

- Limite de profundidad para controlar coste computacional.
- Heuristica para evaluar hojas no terminales.
- Validacion robusta de entradas.

## 7. Complejidad y rendimiento

Sin poda, Minimax crece como O(b^d):

- b: factor de ramificacion (numero de columnas legales, maximo 5).
- d: profundidad.

Con poda alfa-beta:

- En el mejor caso, reduce significativamente el numero de nodos expandidos a O(b^(d/2)).
- En el peor caso, mantiene orden exponencial O(b^d).
- En la practica, la mejora depende del orden de exploracion y de cuantos cortes alfa-beta se produzcan.

Con tablero 4x5 y profundidad 7:

- Factor de ramificacion promedio: 3-4 (algunas columnas llenas).
- Nodos totales sin poda: ~3^7 = 2187.
- Con poda alfa-beta: estimado 500-1000 nodos.
- Tiempo de respuesta: < 1 segundo por turno en hardware moderno.

## 8. Robustez y buenas decisiones de implementacion

Aspectos positivos:

- **Validacion fuerte**: Entradas de usuario validadas en rango y legalidad.
- **Separacion de responsabilidades**: Program, Tablero y Minimax con interfaces claros.
- **Clonado de estados**: Evita efectos laterales durante busqueda.
- **Compatibilidad**: Una sola clase Minimax sirve ambos colores segun parametros.
- **Desempate no sesgado**: Si varias acciones son exactamente equivalentes para Minimax en la decision raiz, se elige una aleatoriamente en lugar de favorecer siempre la primera columna disponible.
- **Comentarios**: Abundantes y alineados con pseudocodigo teorico.
- **Manejo de casos limite**: Profundidad minima, fichas distintas, columnas llenas, etc.

## 9. Posibles mejoras futuras

1. **Ordenacion de movimientos**: Priorizar columna central (mejor para Conecta 4).
2. **Tabla de transposiciones**: Reutilizar evaluaciones de estados duplicados.
3. **Ajuste de heuristica**: Experimentar con pesos de lineas amenazadas.
4. **Iterative deepening**: Explorar gradualmente profundidades mayores.
5. **Metricas de debug**: Contar nodos expandidos, podas realizadas, tiempo por turno.
6. **Tests unitarios**: Validar reglas de tablero y casos limite de Minimax.
7. **Modo desafio**: Diferentes niveles de dificultad (profundidad variable).

## 10. Conclusiones

El proyecto implementa de forma correcta y clara una solucion de busqueda adversaria para Conecta 4 reducido. Se cumplen los requisitos funcionales:

- Dos modos de juego (Humano vs Agente, Agente vs Agente).
- Tablero 4x5.
- Humano empieza con fichas rojas.
- Agente usa Minimax con poda alfa-beta.

Se aplican adecuadamente las tecnicas de IA estudiadas:

- Algoritmo Minimax como busqueda exhaustiva.
- Poda alfa-beta como optimizacion.
- Heuristica y corte por profundidad para casos practicos.

En conjunto, el codigo muestra una buena traduccion de la teoria a una implementacion mantenible, eficiente y defendible para una presentacion de practica.
