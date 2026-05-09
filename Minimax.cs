// Alejandro Martinez Castro, Ivana Lopez Morillo
// Grupo de practicas: jueves

// Implementacion de Minimax + poda alfa-beta siguiendo la estructura de teoria:
// - DecisionMinimax(estado)
// - ValorMax(estado)
// - ValorMin(estado)
// - PruebaTerminal(estado)
// - Utilidad(estado)
//

// si no se explora el arbol completo, se aplica corte por profundidad
// y se usa una funcion de evaluacion heuristica en hojas no terminales.
public sealed class Minimax
{
    // Utilidades grandes para diferenciar claramente ganar/perder.
    private const int UtilidadVictoria = 100000000;
    private const int UtilidadDerrota = -100000000;
    private const int SinDistanciaTerminal = int.MaxValue;

    // Ficha del jugador MAX (agente).
    private readonly char _fichaMax;

    // Ficha del jugador MIN (oponente).
    private readonly char _fichaMin;

    // Limite de exploracion (N niveles).
    private readonly int _profundidadMaxima;

    private readonly struct ResultadoBusqueda
    {
        public ResultadoBusqueda(int valor, int distanciaVictoriaMax, int distanciaDerrotaMax)
        {
            Valor = valor;
            DistanciaVictoriaMax = distanciaVictoriaMax;
            DistanciaDerrotaMax = distanciaDerrotaMax;
        }

        public int Valor { get; }
        public int DistanciaVictoriaMax { get; }
        public int DistanciaDerrotaMax { get; }

        public static ResultadoBusqueda PeorParaMax()
        {
            return new ResultadoBusqueda(int.MinValue, SinDistanciaTerminal, 0);
        }

        public static ResultadoBusqueda PeorParaMin()
        {
            return new ResultadoBusqueda(int.MaxValue, 0, SinDistanciaTerminal);
        }
    }

    public Minimax(char fichaMax, char fichaMin, int profundidadMaxima)
    {
        if (fichaMax == fichaMin)
        {
            throw new ArgumentException("MAX y MIN no pueden usar la misma ficha.");
        }

        if (profundidadMaxima < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(profundidadMaxima), "La profundidad debe ser al menos 1.");
        }

        _fichaMax = fichaMax;
        _fichaMin = fichaMin;
        _profundidadMaxima = profundidadMaxima;
    }

    // Alias para uso desde el resto del programa.
    // Internamente llama a la funcion del pseudocodigo: DecisionMinimax.
    public int ElegirMejorMovimiento(Tablero tablero)
    {
        return DecisionMinimax(tablero);
    }

    // PSEUDOCODIGO: MINIMAX-DECISION(estado)
    // Devuelve la accion (columna) que maximiza el valor minimax.
    public int DecisionMinimax(Tablero estadoActual)
    {
        var acciones = estadoActual.ObtenerColumnasDisponibles();

        // Si no hay acciones legales, no hay jugada posible.
        if (acciones.Count == 0)
        {
            return -1;
        }

        // Intervalo inicial de poda:
        // alfa = mejor valor encontrado para MAX hasta ahora.
        // beta = mejor valor encontrado para MIN hasta ahora.
        int alfa = int.MinValue;
        int beta = int.MaxValue;

        // Acciones empatadas con el mejor resultado encontrado.
        var mejoresAcciones = new List<int>();
        bool hayMejorResultado = false;
        ResultadoBusqueda mejorResultado = ResultadoBusqueda.PeorParaMax();

        // Para cada accion en ACCIONES(estado):
        //   v2 = VALOR-MIN(RESULTADO(estado, accion), alfa, beta)
        //   si v2 mejora a v: v = v2; accionElegida = accion
        foreach (int accion in acciones)
        {
            Tablero estadoResultado = Resultado(estadoActual, accion, _fichaMax);
            ResultadoBusqueda resultadoAccion = SumarUnNivel(
                ValorMin(estadoResultado, _profundidadMaxima - 1, alfa, beta));

            if (!hayMejorResultado || EsMejorParaMax(resultadoAccion, mejorResultado))
            {
                mejorResultado = resultadoAccion;
                mejoresAcciones.Clear();
                mejoresAcciones.Add(accion);
                hayMejorResultado = true;
            }
            else if (SonResultadosEquivalentes(resultadoAccion, mejorResultado))
            {
                mejoresAcciones.Add(accion);
            }

            // MAX mejora su cota inferior.
            if (mejorResultado.Valor > alfa)
            {
                alfa = mejorResultado.Valor;
            }
        }

        return mejoresAcciones[Random.Shared.Next(mejoresAcciones.Count)];
    }

    // PSEUDOCODIGO: VALOR-MAX(estado, alfa, beta)
    private ResultadoBusqueda ValorMax(Tablero estado, int profundidadRestante, int alfa, int beta)
    {
        // Si TERMINAL-TEST(estado) -> devolver UTILIDAD(estado)
        if (PruebaTerminal(estado, profundidadRestante))
        {
            return Utilidad(estado, _fichaMax);
        }

        // v = -infinito
        ResultadoBusqueda mejorResultado = ResultadoBusqueda.PeorParaMax();
        var acciones = estado.ObtenerColumnasDisponibles();

        // Seguridad extra: si no hay acciones, evaluamos como hoja.
        if (acciones.Count == 0)
        {
            return Utilidad(estado, _fichaMax);
        }

        // Para cada accion:
        //   v = max(v, VALOR-MIN(RESULTADO(...), alfa, beta))
        //   alfa = max(alfa, v)
        //   si alfa >= beta => podar
        foreach (int accion in acciones)
        {
            Tablero estadoResultado = Resultado(estado, accion, _fichaMax);
            ResultadoBusqueda resultadoHijo = SumarUnNivel(
                ValorMin(estadoResultado, profundidadRestante - 1, alfa, beta));

            if (EsMejorParaMax(resultadoHijo, mejorResultado))
            {
                mejorResultado = resultadoHijo;
            }

            // Actualiza alfa (mejor valor garantizado para MAX).
            if (mejorResultado.Valor > alfa)
            {
                alfa = mejorResultado.Valor;
            }

            // Usamos corte estricto para no descartar empates con mejor desempate.
            if (alfa > beta)
            {
                break;
            }
        }

        return mejorResultado;
    }

    // PSEUDOCODIGO: VALOR-MIN(estado, alfa, beta)
    private ResultadoBusqueda ValorMin(Tablero estado, int profundidadRestante, int alfa, int beta)
    {
        // Si TERMINAL-TEST(estado) -> devolver UTILIDAD(estado)
        if (PruebaTerminal(estado, profundidadRestante))
        {
            return Utilidad(estado, _fichaMin);
        }

        // v = +infinito
        ResultadoBusqueda mejorResultado = ResultadoBusqueda.PeorParaMin();
        var acciones = estado.ObtenerColumnasDisponibles();

        // Seguridad extra: si no hay acciones, evaluamos como hoja.
        if (acciones.Count == 0)
        {
            return Utilidad(estado, _fichaMin);
        }

        // Para cada accion:
        //   v = min(v, VALOR-MAX(RESULTADO(...), alfa, beta))
        //   beta = min(beta, v)
        //   si alfa >= beta => podar
        foreach (int accion in acciones)
        {
            Tablero estadoResultado = Resultado(estado, accion, _fichaMin);
            ResultadoBusqueda resultadoHijo = SumarUnNivel(
                ValorMax(estadoResultado, profundidadRestante - 1, alfa, beta));

            if (EsMejorParaMin(resultadoHijo, mejorResultado))
            {
                mejorResultado = resultadoHijo;
            }

            // Actualiza beta (mejor valor garantizado para MIN).
            if (mejorResultado.Valor < beta)
            {
                beta = mejorResultado.Valor;
            }

            // Usamos corte estricto para no descartar empates con mejor desempate.
            if (alfa > beta)
            {
                break;
            }
        }

        return mejorResultado;
    }

    // PSEUDOCODIGO: RESULTADO(estado, accion)
    // Construye el estado sucesor tras aplicar una jugada.
    private static Tablero Resultado(Tablero estado, int accion, char fichaTurno)
    {
        Tablero copia = estado.Clonar();
        copia.InsertarFicha(accion, fichaTurno);
        return copia;
    }

    // PSEUDOCODIGO: TERMINAL-TEST(estado)
    // En esta practica tambien cortamos si se alcanza profundidad 0.
    private static bool PruebaTerminal(Tablero estado, int profundidadRestante)
    {
        return estado.EsTerminal() || profundidadRestante == 0;
    }

    // PSEUDOCODIGO: UTILIDAD(estado)
    // - Si el estado es terminal: utilidad exacta.
    // - Si no es terminal pero llegamos por corte de profundidad: heuristica.
    private ResultadoBusqueda Utilidad(Tablero estado, char fichaTurno)
    {
        // Victoria de MAX.
        if (estado.HayGanador(_fichaMax))
        {
            return new ResultadoBusqueda(UtilidadVictoria, 0, SinDistanciaTerminal);
        }

        // Victoria de MIN.
        if (estado.HayGanador(_fichaMin))
        {
            return new ResultadoBusqueda(UtilidadDerrota, SinDistanciaTerminal, 0);
        }

        // Empate.
        if (estado.EstaLleno())
        {
            return new ResultadoBusqueda(0, SinDistanciaTerminal, SinDistanciaTerminal);
        }

        // Estado no terminal + corte por profundidad -> evaluacion heuristica.
        return new ResultadoBusqueda(EvaluacionHeuristica(estado, fichaTurno), SinDistanciaTerminal, SinDistanciaTerminal);
    }

    // Al subir un nivel del arbol, una victoria/derrota terminal queda un movimiento mas lejos.
    private static ResultadoBusqueda SumarUnNivel(ResultadoBusqueda resultado)
    {
        return new ResultadoBusqueda(
            resultado.Valor,
            SumarUnoSiExiste(resultado.DistanciaVictoriaMax),
            SumarUnoSiExiste(resultado.DistanciaDerrotaMax));
    }

    private static int SumarUnoSiExiste(int distancia)
    {
        return distancia == SinDistanciaTerminal ? SinDistanciaTerminal : distancia + 1;
    }

    private static bool EsMejorParaMax(ResultadoBusqueda candidato, ResultadoBusqueda actual)
    {
        if (candidato.Valor != actual.Valor)
        {
            return candidato.Valor > actual.Valor;
        }

        if (candidato.DistanciaVictoriaMax != actual.DistanciaVictoriaMax)
        {
            return candidato.DistanciaVictoriaMax < actual.DistanciaVictoriaMax;
        }

        if (candidato.DistanciaDerrotaMax != actual.DistanciaDerrotaMax)
        {
            return candidato.DistanciaDerrotaMax > actual.DistanciaDerrotaMax;
        }

        return false;
    }

    private static bool SonResultadosEquivalentes(ResultadoBusqueda primero, ResultadoBusqueda segundo)
    {
        return primero.Valor == segundo.Valor &&
            primero.DistanciaVictoriaMax == segundo.DistanciaVictoriaMax &&
            primero.DistanciaDerrotaMax == segundo.DistanciaDerrotaMax;
    }

    private static bool EsMejorParaMin(ResultadoBusqueda candidato, ResultadoBusqueda actual)
    {
        if (candidato.Valor != actual.Valor)
        {
            return candidato.Valor < actual.Valor;
        }

        if (candidato.DistanciaDerrotaMax != actual.DistanciaDerrotaMax)
        {
            return candidato.DistanciaDerrotaMax < actual.DistanciaDerrotaMax;
        }

        if (candidato.DistanciaVictoriaMax != actual.DistanciaVictoriaMax)
        {
            return candidato.DistanciaVictoriaMax > actual.DistanciaVictoriaMax;
        }

        return false;
    }

    // Funcion de evaluacion para hojas no terminales.
    // Se basa en ventanas de 4:
    // - suma puntuacion si favorece a MAX
    // - resta puntuacion si favorece a MIN
    private int EvaluacionHeuristica(Tablero estado)
    {
        return EvaluacionHeuristica(estado, _fichaMax);
    }

    private int EvaluacionHeuristica(Tablero estado, char fichaTurno)
    {
        int puntuacion = 0;

        puntuacion += PuntuarAmenazasInmediatas(estado, fichaTurno);

        for (int fila = 0; fila < Tablero.Filas; fila++)
        {
            for (int columna = 0; columna < Tablero.Columnas; columna++)
            {
                puntuacion += PuntuarVentanaSiExiste(estado, fila, columna, 0, 1);   // horizontal
                puntuacion += PuntuarVentanaSiExiste(estado, fila, columna, 1, 0);   // vertical
                puntuacion += PuntuarVentanaSiExiste(estado, fila, columna, 1, 1);   // diagonal \
                puntuacion += PuntuarVentanaSiExiste(estado, fila, columna, -1, 1);  // diagonal /
            }
        }

        return puntuacion;
    }

    // Si una ficha puede ganar en una columna legal, importa mucho quien mueve ahora.
    private int PuntuarAmenazasInmediatas(Tablero estado, char fichaTurno)
    {
        int puntuacion = 0;

        foreach (int columna in estado.ObtenerColumnasDisponibles())
        {
            if (MovimientoGana(estado, columna, _fichaMax))
            {
                puntuacion += fichaTurno == _fichaMax ? 5000000 : 1200000;
            }

            if (MovimientoGana(estado, columna, _fichaMin))
            {
                puntuacion -= fichaTurno == _fichaMin ? 5000000 : 1200000;
            }
        }

        return puntuacion;
    }

    private static bool MovimientoGana(Tablero estado, int columna, char ficha)
    {
        Tablero copia = Resultado(estado, columna, ficha);
        return copia.HayGanador(ficha);
    }

    // Si la ventana de 4 celdas no cabe en el tablero, devuelve 0.
    // Si cabe, puntua fichas, huecos y cuantos huecos son jugables por gravedad.
    private int PuntuarVentanaSiExiste(
        Tablero estado,
        int filaInicio,
        int columnaInicio,
        int deltaFila,
        int deltaColumna)
    {
        int filaFin = filaInicio + 3 * deltaFila;
        int columnaFin = columnaInicio + 3 * deltaColumna;

        if (filaFin < 0 || filaFin >= Tablero.Filas || columnaFin < 0 || columnaFin >= Tablero.Columnas)
        {
            return 0;
        }

        bool[] posicionesMax = new bool[4];
        bool[] posicionesMin = new bool[4];
        int fichasMax = 0;
        int fichasMin = 0;
        int vacias = 0;
        int vaciasJugables = 0;

        for (int i = 0; i < 4; i++)
        {
            int fila = filaInicio + i * deltaFila;
            int columna = columnaInicio + i * deltaColumna;
            char celda = estado[fila, columna];

            if (celda == _fichaMax)
            {
                fichasMax++;
                posicionesMax[i] = true;
            }
            else if (celda == _fichaMin)
            {
                fichasMin++;
                posicionesMin[i] = true;
            }
            else
            {
                vacias++;

                if (EsCasillaJugable(estado, fila, columna))
                {
                    vaciasJugables++;
                }
            }
        }

        return PuntuarVentana(
            fichasMax,
            fichasMin,
            vacias,
            vaciasJugables,
            posicionesMax,
            posicionesMin);
    }

    // Reglas de puntuacion de una ventana de 4.
    // Si mezcla fichas de ambos jugadores, no aporta porque esta bloqueada.
    private static int PuntuarVentana(
        int fichasMax,
        int fichasMin,
        int vacias,
        int vaciasJugables,
        bool[] posicionesMax,
        bool[] posicionesMin)
    {
        if (fichasMax > 0 && fichasMin > 0)
        {
            return 0;
        }

        if (fichasMax > 0)
        {
            return PuntuarPatron(fichasMax, vacias, vaciasJugables, posicionesMax);
        }

        if (fichasMin > 0)
        {
            return -PuntuarPatron(fichasMin, vacias, vaciasJugables, posicionesMin);
        }

        return 0;
    }

    // Una casilla vacia solo es una amenaza inmediata si se puede jugar en ella ahora.
    private static bool EsCasillaJugable(Tablero estado, int fila, int columna)
    {
        return estado[fila, columna] == Tablero.Vacia &&
            (fila == Tablero.Filas - 1 || estado[fila + 1, columna] != Tablero.Vacia);
    }

    // Pesos exagerados pero dependientes de si los huecos son jugables por gravedad.
    private static int PuntuarPatron(
        int fichas,
        int vacias,
        int vaciasJugables,
        bool[] posiciones)
    {
        if (fichas == 3 && vacias == 1)
        {
            // Si el unico hueco es jugable, es amenaza de ganar en la siguiente jugada.
            return vaciasJugables == 1 ? 900000 : 60000;
        }

        if (fichas == 2 && vacias == 2)
        {
            int basePatron = PuntuarPatronDeDos(posiciones);

            return vaciasJugables switch
            {
                2 => basePatron * 2,
                1 => basePatron,
                _ => basePatron / 5
            };
        }

        if (fichas == 1 && vacias == 3)
        {
            return vaciasJugables > 0 ? 50 : 10;
        }

        return 0;
    }

    private static int PuntuarPatronDeDos(bool[] posiciones)
    {
        bool p0 = posiciones[0];
        bool p1 = posiciones[1];
        bool p2 = posiciones[2];
        bool p3 = posiciones[3];

        if (p1 && p2) return 12000;                 // .XX.
        if ((p0 && p1) || (p2 && p3)) return 5000;  // XX.. o ..XX
        if ((p0 && p2) || (p1 && p3)) return 1500;  // X.X. o .X.X

        return 500; // X..X
    }
}
