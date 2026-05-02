// Alejandro Martinez Castro, Ivana Lopez Morillo
// Grupo de practicas: jueves

public static class Program
{
    // Profundidad de busqueda de ambos agentes.
    private const int ProfundidadAgente = 7;

    // Tiempo maximo por decision (ms). 0 = sin limite de tiempo.
    private const int TiempoMaximoDecisionMs = 500;

    // Agente que juega con amarillas (MAX=amarilla, MIN=roja).
    private static readonly Minimax AgenteAmarillo = new Minimax(
        Tablero.Amarilla,
        Tablero.Roja,
        ProfundidadAgente,
        TiempoMaximoDecisionMs);

    // Agente que juega con rojas (MAX=roja, MIN=amarilla).
    private static readonly Minimax AgenteRojo = new Minimax(
        Tablero.Roja,
        Tablero.Amarilla,
        ProfundidadAgente,
        TiempoMaximoDecisionMs);

    public static void Main()
    {
        MostrarEncabezado();

        int modo = PedirModoDeJuego();

        if (modo == 1)
        {
            JugarHumanoVsAgente();
        }
        else
        {
            JugarAgenteVsAgente();
        }
    }

    // Muestra una cabecera simple de la practica.
    private static void MostrarEncabezado()
    {
        Console.WriteLine("===============================================");
        Console.WriteLine("      PRACTICA 3 - BUSQUEDA ADVERSARIA");
        Console.WriteLine("              CONECTA 4 (4x5)");
        Console.WriteLine("===============================================");
        Console.WriteLine();
    }

    // Pide el modo hasta que el usuario introduzca 1 o 2.
    private static int PedirModoDeJuego()
    {
        while (true)
        {
            Console.WriteLine("Selecciona modo de juego:");
            Console.WriteLine("1. Humano VS Agente");
            Console.WriteLine("2. Agente VS Agente");
            Console.Write("Opcion: ");

            string? entrada = Console.ReadLine();

            if (int.TryParse(entrada, out int opcion) && (opcion == 1 || opcion == 2))
            {
                Console.WriteLine();
                return opcion;
            }

            Console.WriteLine("Opcion no valida. Escribe 1 o 2.");
            Console.WriteLine();
        }
    }

    // Modo 1:
    // - El humano juega con R y empieza siempre.
    // - El agente juega con A usando Minimax con poda alfa-beta.
    private static void JugarHumanoVsAgente()
    {
        Tablero tablero = new Tablero();
        char turnoActual = Tablero.Roja;

        Console.WriteLine("Modo Humano VS Agente");
        Console.WriteLine("El humano usa R y empieza primero.");
        Console.WriteLine("El agente usa A.");
        Console.WriteLine();

        while (!tablero.EsTerminal())
        {
            MostrarTablero(tablero);

            if (turnoActual == Tablero.Roja)
            {
                int columnaHumano = PedirColumnaHumana(tablero);
                tablero.InsertarFicha(columnaHumano, Tablero.Roja);
            }
            else
            {
                int columnaAgente = AgenteAmarillo.ElegirMejorMovimiento(tablero);
                tablero.InsertarFicha(columnaAgente, Tablero.Amarilla);
                Console.WriteLine($"Agente (A) juega en columna: {columnaAgente}");
                Console.WriteLine();
            }

            turnoActual = CambiarTurno(turnoActual);
        }

        MostrarTablero(tablero);
        MostrarResultadoFinal(tablero, "Humano", "Agente");
    }

    // Modo 2:
    // - Dos agentes se enfrentan entre si.
    // - Rojo comienza primero.
    private static void JugarAgenteVsAgente()
    {
        Tablero tablero = new Tablero();
        char turnoActual = Tablero.Roja;

        Console.WriteLine("Modo Agente VS Agente");
        Console.WriteLine("Agente Rojo (R) empieza.");
        Console.WriteLine("Agente Amarillo (A) responde.");
        Console.WriteLine();

        while (!tablero.EsTerminal())
        {
            MostrarTablero(tablero);

            if (turnoActual == Tablero.Roja)
            {
                int columnaRoja = AgenteRojo.ElegirMejorMovimiento(tablero);
                tablero.InsertarFicha(columnaRoja, Tablero.Roja);
                Console.WriteLine($"Agente Rojo (R) juega en columna: {columnaRoja}");
            }
            else
            {
                int columnaAmarilla = AgenteAmarillo.ElegirMejorMovimiento(tablero);
                tablero.InsertarFicha(columnaAmarilla, Tablero.Amarilla);
                Console.WriteLine($"Agente Amarillo (A) juega en columna: {columnaAmarilla}");
            }

            Console.WriteLine();

            // Pausa breve para poder seguir la partida en consola.
            System.Threading.Thread.Sleep(350);

            turnoActual = CambiarTurno(turnoActual);
        }

        MostrarTablero(tablero);
        MostrarResultadoFinal(tablero, "Agente Rojo", "Agente Amarillo");
    }

    // Pide columna al humano y valida:
    // - formato entero
    // - rango y disponibilidad (movimiento legal)
    private static int PedirColumnaHumana(Tablero tablero)
    {
        while (true)
        {
            Console.Write("Tu turno (R). Elige columna [0-4]: ");
            string? entrada = Console.ReadLine();

            if (!int.TryParse(entrada, out int columna))
            {
                Console.WriteLine("Entrada no valida. Escribe un numero entero.");
                Console.WriteLine();
                continue;
            }

            if (!tablero.EsMovimientoValido(columna))
            {
                Console.WriteLine("Movimiento no valido. Columna fuera de rango o llena.");
                Console.WriteLine();
                continue;
            }

            Console.WriteLine();
            return columna;
        }
    }

    // Muestra el tablero usando ToString del propio objeto.
    private static void MostrarTablero(Tablero tablero)
    {
        Console.WriteLine("Tablero actual:");
        Console.WriteLine(tablero);
    }

    // Alterna turno entre R y A.
    private static char CambiarTurno(char turnoActual)
    {
        return turnoActual == Tablero.Roja ? Tablero.Amarilla : Tablero.Roja;
    }

    // Muestra el resultado final en funcion del ganador.
    private static void MostrarResultadoFinal(Tablero tablero, string nombreRoja, string nombreAmarilla)
    {
        if (tablero.HayGanador(Tablero.Roja))
        {
            Console.WriteLine($"Fin de partida: gana {nombreRoja} (R).");
        }
        else if (tablero.HayGanador(Tablero.Amarilla))
        {
            Console.WriteLine($"Fin de partida: gana {nombreAmarilla} (A).");
        }
        else
        {
            Console.WriteLine("Fin de partida: empate.");
        }
    }
}
