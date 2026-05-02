// Alejandro Martinez Castro, Ivana Lopez Morillo
// Grupo de practicas: jueves

// Clase que representa un estado del juego Conecta 4 (tablero 4x5).
// Esta clase no decide jugadas: solo modela el estado y sus reglas basicas.
public sealed class Tablero
{
    // Numero de filas del tablero.
    public const int Filas = 4;

    // Numero de columnas del tablero.
    public const int Columnas = 5;

    // Simbolo para casilla vacia.
    public const char Vacia = '.';

    // Simbolo para ficha del jugador humano.
    public const char Roja = 'R';

    // Simbolo para ficha del agente.
    public const char Amarilla = 'A';

    // Matriz interna que guarda el contenido del tablero.
    // Acceso: _celdas[fila, columna].
    private readonly char[,] _celdas;

    // Constructor por defecto: crea tablero vacio.
    public Tablero()
    {
        // Reservamos memoria para las 4x5 celdas.
        _celdas = new char[Filas, Columnas];

        // Recorremos todas las posiciones para inicializarlas a Vacia.
        for (int fila = 0; fila < Filas; fila++)
        {
            for (int columna = 0; columna < Columnas; columna++)
            {
                _celdas[fila, columna] = Vacia;
            }
        }
    }

    // Constructor privado para crear un tablero a partir de una matriz existente.
    // Lo usamos al clonar estados en Minimax.
    private Tablero(char[,] celdas)
    {
        _celdas = celdas;
    }

    // Indexador de solo lectura:
    // permite consultar una celda asi: tablero[fila, columna]
    public char this[int fila, int columna] => _celdas[fila, columna];

    // Crea una copia profunda del tablero actual (refs distintas).
    // Es fundamental para explorar jugadas sin modificar el estado original.
    public Tablero Clonar()
    {
        // Creamos una matriz nueva con el mismo tamaño.
        var copia = new char[Filas, Columnas];

        // Copiamos todas las celdas a la nueva matriz.
        // _celdas.Length = Filas * Columnas.
        Array.Copy(_celdas, copia, _celdas.Length);

        // Devolvemos un nuevo objeto Tablero que usa la matriz copiada.
        return new Tablero(copia);
    }

    // Devuelve true si se puede jugar en esa columna.
    // Condiciones:
    // 1) Columna dentro del rango [0, Columnas-1].
    // 2) La celda superior de esa columna esta vacia.
    public bool EsMovimientoValido(int columna)
    {
        bool columnaEnRango = columna >= 0 && columna < Columnas;

        // Solo miramos la fila superior:
        // si esta ocupada, la columna entera esta llena.
        bool columnaConHueco = columnaEnRango && _celdas[0, columna] == Vacia;

        return columnaEnRango && columnaConHueco;
    }

    // Devuelve una lista con todas las columnas donde se puede jugar.
    // Esta funcion representa las acciones validas desde este estado.
    public List<int> ObtenerColumnasDisponibles()
    {
        // Lista donde acumularemos las columnas jugables.
        var columnas = new List<int>();

        // Revisamos todas las columnas del tablero.
        for (int columna = 0; columna < Columnas; columna++)
        {
            // Si la columna es valida, la anadimos.
            if (EsMovimientoValido(columna))
            {
                columnas.Add(columna);
            }
        }

        return columnas;
    }

    // Inserta una ficha en la columna indicada.
    // La ficha cae a la fila libre mas baja.
    // Devuelve true si inserta; false si la columna era invalida/llena.
    public bool InsertarFicha(int columna, char ficha)
    {
        // Primer filtro: jugada valida o no.
        if (!EsMovimientoValido(columna))
        {
            return false;
        }

        // Buscamos hueco desde abajo hacia arriba:
        // fila 3, luego 2, luego 1, luego 0.
        for (int fila = Filas - 1; fila >= 0; fila--)
        {
            // Si encontramos la primera casilla vacia...
            if (_celdas[fila, columna] == Vacia)
            {
                // ...colocamos la ficha ahi.
                _celdas[fila, columna] = ficha;
                return true;
            }
        }

        // Caso defensivo: no deberia ocurrir si EsMovimientoValido devolvio true.
        return false;
    }

    // Comprueba si el tablero esta completamente lleno.
    // Basta revisar si en la fila superior queda alguna casilla vacia.
    public bool EstaLleno()
    {
        for (int columna = 0; columna < Columnas; columna++)
        {
            // Si la parte superior de una columna esta vacia,
            // aun se puede jugar en esa columna.
            if (_celdas[0, columna] == Vacia)
            {
                return false;
            }
        }

        // Si no hay ningun hueco arriba, no queda ninguna jugada posible.
        return true;
    }

    // Comprueba si la ficha indicada (Roja o Amarilla) tiene 4 en raya.
    // Se revisan 4 direcciones:
    // 1) Horizontal
    // 2) Vertical
    // 3) Diagonal descendente (\)
    // 4) Diagonal ascendente (/)
    public bool HayGanador(char ficha)
    {
        // 1) HORIZONTAL
        // Recorremos todas las filas.
        for (int fila = 0; fila < Filas; fila++)
        {
            // Ventanas horizontales de longitud 4.
            // Con 5 columnas, inicio posible: 0 y 1.
            for (int columna = 0; columna <= Columnas - 4; columna++)
            {
                if (_celdas[fila, columna] == ficha &&
                    _celdas[fila, columna + 1] == ficha &&
                    _celdas[fila, columna + 2] == ficha &&
                    _celdas[fila, columna + 3] == ficha)
                {
                    return true;
                }
            }
        }

        // 2) VERTICAL
        // Ventanas de 4 filas. Como Filas=4, solo empieza en fila 0.
        for (int fila = 0; fila <= Filas - 4; fila++)
        {
            // Probamos cada columna.
            for (int columna = 0; columna < Columnas; columna++)
            {
                if (_celdas[fila, columna] == ficha &&
                    _celdas[fila + 1, columna] == ficha &&
                    _celdas[fila + 2, columna] == ficha &&
                    _celdas[fila + 3, columna] == ficha)
                {
                    return true;
                }
            }
        }

        // 3) DIAGONAL DESCENDENTE (\)
        // Comparamos (fila,col), (fila+1,col+1), (fila+2,col+2), (fila+3,col+3).
        for (int fila = 0; fila <= Filas - 4; fila++)
        {
            for (int columna = 0; columna <= Columnas - 4; columna++)
            {
                if (_celdas[fila, columna] == ficha &&
                    _celdas[fila + 1, columna + 1] == ficha &&
                    _celdas[fila + 2, columna + 2] == ficha &&
                    _celdas[fila + 3, columna + 3] == ficha)
                {
                    return true;
                }
            }
        }

        // 4) DIAGONAL ASCENDENTE (/)
        // Comparamos (fila,col), (fila-1,col+1), (fila-2,col+2), (fila-3,col+3).
        // La fila inicial minima es 3 para poder restar hasta 0.
        for (int fila = 3; fila < Filas; fila++)
        {
            for (int columna = 0; columna <= Columnas - 4; columna++)
            {
                if (_celdas[fila, columna] == ficha &&
                    _celdas[fila - 1, columna + 1] == ficha &&
                    _celdas[fila - 2, columna + 2] == ficha &&
                    _celdas[fila - 3, columna + 3] == ficha)
                {
                    return true;
                }
            }
        }

        // Si ninguna direccion dio 4 en raya, no hay victoria de esa ficha.
        return false;
    }

    // Un estado terminal es fin de partida:
    // - gana Roja
    // - gana Amarilla
    // - empate por tablero lleno.
    public bool EsTerminal()
    {
        return HayGanador(Roja) || HayGanador(Amarilla) || EstaLleno();
    }

    // Construye una representacion de texto para imprimir el tablero en consola.
    // Ejemplo:
    // |.|.|.|.|.|
    // |.|.|.|.|.|
    // |.|.|R|.|.|
    // |A|R|A|.|.|
    //  0 1 2 3 4
    public override string ToString()
    {
        // StringBuilder evita crear muchas cadenas intermedias.
        var sb = new System.Text.StringBuilder();

        // Recorremos filas de arriba a abajo.
        for (int fila = 0; fila < Filas; fila++)
        {
            // Borde izquierdo de la fila.
            sb.Append("|");

            // Recorremos columnas de izquierda a derecha.
            for (int columna = 0; columna < Columnas; columna++)
            {
                // Escribimos contenido de celda.
                sb.Append(_celdas[fila, columna]);

                // Escribimos separador vertical.
                sb.Append("|");
            }

            // Terminamos linea de esa fila.
            sb.AppendLine();
        }

        // Mostramos indices de columna para que el usuario elija.
        sb.AppendLine(" 0 1 2 3 4");

        return sb.ToString();
    }
}
