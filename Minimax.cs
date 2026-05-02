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
    private const int UtilidadVictoria = 100000;
    private const int UtilidadDerrota = -100000;

    // Ficha del jugador MAX (agente).
    private readonly char _fichaMax;

    // Ficha del jugador MIN (oponente).
    private readonly char _fichaMin;

    // Limite de exploracion (N niveles).
    private readonly int _profundidadMaxima;

    // Limite de tiempo por decision (en milisegundos).
    // Si vale 0 o menos, no se aplica corte por tiempo.
    private readonly int _tiempoMaximoMs;

    public Minimax(char fichaMax, char fichaMin, int profundidadMaxima, int tiempoMaximoMs = 0)
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
        _tiempoMaximoMs = tiempoMaximoMs;
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

        // v = -infinito
        int v = int.MinValue;

        // Intervalo inicial de poda:
        // alfa = mejor valor encontrado para MAX hasta ahora.
        // beta = mejor valor encontrado para MIN hasta ahora.
        int alfa = int.MinValue;
        int beta = int.MaxValue;

        // Momento limite para el corte por tiempo.
        DateTime? instanteLimite = _tiempoMaximoMs > 0
            ? DateTime.UtcNow.AddMilliseconds(_tiempoMaximoMs)
            : null;

        // accionElegida inicial.
        int accionElegida = acciones[0];
        bool seEvaluoAlgunaAccion = false;

        // Para cada accion en ACCIONES(estado):
        //   v2 = VALOR-MIN(RESULTADO(estado, accion), alfa, beta)
        //   si v2 > v: v = v2; accionElegida = accion
        foreach (int accion in acciones)
        {
            // Si ya agotamos tiempo y al menos una accion fue evaluada, devolvemos la mejor actual.
            if (TiempoAgotado(instanteLimite) && seEvaluoAlgunaAccion)
            {
                break;
            }

            Tablero estadoResultado = Resultado(estadoActual, accion, _fichaMax);
            int v2 = ValorMin(estadoResultado, _profundidadMaxima - 1, alfa, beta, instanteLimite);
            seEvaluoAlgunaAccion = true;

            if (v2 > v)
            {
                v = v2;
                accionElegida = accion;
            }

            // MAX mejora su cota inferior.
            if (v > alfa)
            {
                alfa = v;
            }
        }

        return accionElegida;
    }

    // PSEUDOCODIGO: VALOR-MAX(estado, alfa, beta)
    private int ValorMax(Tablero estado, int profundidadRestante, int alfa, int beta, DateTime? instanteLimite)
    {
        // Si TERMINAL-TEST(estado) -> devolver UTILIDAD(estado)
        if (PruebaTerminal(estado, profundidadRestante, instanteLimite))
        {
            return Utilidad(estado, profundidadRestante);
        }

        // v = -infinito
        int v = int.MinValue;
        var acciones = estado.ObtenerColumnasDisponibles();

        // Seguridad extra: si no hay acciones, evaluamos como hoja.
        if (acciones.Count == 0)
        {
            return Utilidad(estado, profundidadRestante);
        }

        bool seEvaluoAlgunHijo = false;

        // Para cada accion:
        //   v = max(v, VALOR-MIN(RESULTADO(...), alfa, beta))
        //   alfa = max(alfa, v)
        //   si alfa >= beta => podar
        foreach (int accion in acciones)
        {
            // Si se acabo el tiempo, dejamos de expandir.
            // Si no evaluamos ningun hijo aun, devolvemos utilidad heuristica del estado actual.
            if (TiempoAgotado(instanteLimite))
            {
                break;
            }

            Tablero estadoResultado = Resultado(estado, accion, _fichaMax);
            int valorHijo = ValorMin(estadoResultado, profundidadRestante - 1, alfa, beta, instanteLimite);
            seEvaluoAlgunHijo = true;

            if (valorHijo > v)
            {
                v = valorHijo;
            }

            // Actualiza alfa (mejor valor garantizado para MAX).
            if (v > alfa)
            {
                alfa = v;
            }

            // Criterio de poda alfa-beta de teoria.
            if (alfa >= beta)
            {
                break;
            }
        }

        if (!seEvaluoAlgunHijo)
        {
            return Utilidad(estado, profundidadRestante);
        }

        return v;
    }

    // PSEUDOCODIGO: VALOR-MIN(estado, alfa, beta)
    private int ValorMin(Tablero estado, int profundidadRestante, int alfa, int beta, DateTime? instanteLimite)
    {
        // Si TERMINAL-TEST(estado) -> devolver UTILIDAD(estado)
        if (PruebaTerminal(estado, profundidadRestante, instanteLimite))
        {
            return Utilidad(estado, profundidadRestante);
        }

        // v = +infinito
        int v = int.MaxValue;
        var acciones = estado.ObtenerColumnasDisponibles();

        // Seguridad extra: si no hay acciones, evaluamos como hoja.
        if (acciones.Count == 0)
        {
            return Utilidad(estado, profundidadRestante);
        }

        bool seEvaluoAlgunHijo = false;

        // Para cada accion:
        //   v = min(v, VALOR-MAX(RESULTADO(...), alfa, beta))
        //   beta = min(beta, v)
        //   si alfa >= beta => podar
        foreach (int accion in acciones)
        {
            // Si se agota tiempo, no seguimos expandiendo.
            if (TiempoAgotado(instanteLimite))
            {
                break;
            }

            Tablero estadoResultado = Resultado(estado, accion, _fichaMin);
            int valorHijo = ValorMax(estadoResultado, profundidadRestante - 1, alfa, beta, instanteLimite);
            seEvaluoAlgunHijo = true;

            if (valorHijo < v)
            {
                v = valorHijo;
            }

            // Actualiza beta (mejor valor garantizado para MIN).
            if (v < beta)
            {
                beta = v;
            }

            // Criterio de poda alfa-beta de teoria.
            if (alfa >= beta)
            {
                break;
            }
        }

        if (!seEvaluoAlgunHijo)
        {
            return Utilidad(estado, profundidadRestante);
        }

        return v;
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
    private static bool PruebaTerminal(Tablero estado, int profundidadRestante, DateTime? instanteLimite)
    {
        return estado.EsTerminal() || profundidadRestante == 0 || TiempoAgotado(instanteLimite);
    }

    // Devuelve true si existe limite y ya se supero.
    private static bool TiempoAgotado(DateTime? instanteLimite)
    {
        return instanteLimite.HasValue && DateTime.UtcNow >= instanteLimite.Value;
    }

    // PSEUDOCODIGO: UTILIDAD(estado)
    // - Si el estado es terminal: utilidad exacta.
    // - Si no es terminal pero llegamos por corte de profundidad: heuristica.
    private int Utilidad(Tablero estado, int profundidadRestante)
    {
        // Victoria de MAX.
        if (estado.HayGanador(_fichaMax))
        {
            // +profundidadRestante para favorecer victorias mas rapidas.
            return UtilidadVictoria + profundidadRestante;
        }

        // Victoria de MIN.
        if (estado.HayGanador(_fichaMin))
        {
            // -profundidadRestante para preferir derrotas mas tardias.
            return UtilidadDerrota - profundidadRestante;
        }

        // Empate.
        if (estado.EstaLleno())
        {
            return 0;
        }

        // Estado no terminal + corte por profundidad -> evaluacion heuristica.
        return EvaluacionHeuristica(estado);
    }

    // Funcion de evaluacion para hojas no terminales.
    // Se basa en ventanas de 4:
    // - suma puntuacion si favorece a MAX
    // - resta puntuacion si favorece a MIN
    private int EvaluacionHeuristica(Tablero estado)
    {
        int puntuacion = 0;

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

    // Si la ventana de 4 celdas no cabe en el tablero, devuelve 0.
    // Si cabe, cuenta fichas MAX/MIN/vacias y asigna puntuacion.
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

        int fichasMax = 0;
        int fichasMin = 0;
        int vacias = 0;

        for (int i = 0; i < 4; i++)
        {
            int fila = filaInicio + i * deltaFila;
            int columna = columnaInicio + i * deltaColumna;
            char celda = estado[fila, columna];

            if (celda == _fichaMax)
            {
                fichasMax++;
            }
            else if (celda == _fichaMin)
            {
                fichasMin++;
            }
            else
            {
                vacias++;
            }
        }

        return PuntuarConteoVentana(fichasMax, fichasMin, vacias);
    }

    // Reglas de puntuacion de una ventana.
    // Si la ventana mezcla fichas de ambos jugadores, no aporta.
    private static int PuntuarConteoVentana(int fichasMax, int fichasMin, int vacias)
    {
        if (fichasMax > 0 && fichasMin > 0)
        {
            return 0;
        }

        if (fichasMax == 3 && vacias == 1) return 100;
        if (fichasMax == 2 && vacias == 2) return 10;
        if (fichasMax == 1 && vacias == 3) return 1;

        if (fichasMin == 3 && vacias == 1) return -100;
        if (fichasMin == 2 && vacias == 2) return -10;
        if (fichasMin == 1 && vacias == 3) return -1;

        return 0;
    }
}
