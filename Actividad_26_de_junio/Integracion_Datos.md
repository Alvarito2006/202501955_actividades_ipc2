<div align="center">

Universidad de San Carlos de Guatemala

Facultad de Ingenieria - Escuela de Ciencias y Sistemas

</div>

**Curso:** LAB. Introduccion a la Programacion y Computacion 2  
**Seccion:** P  
**Nombre:** Alvaro Moises Giron Morales  
**Carne:** 202501955  
**CUI:** 3055401650208  

<div align="center">

# ACTIVIDAD 26 DE JUNIO

**Docente:** AUX. Fernando Jose Vicente Velasquez  
**Fecha de entrega:** 26 DE JUNIO 2026

</div>


## Parte 1: Evaluacion Conceptual y Buenas Practicas

### Formatos de intercambio

| Formato | Ventajas | Desventajas |
| --- | --- | --- |
| CSV | Es liviano, facil de generar, compatible con hojas de calculo y adecuado para cargas masivas simples porque cada fila representa un registro. | No representa jerarquias complejas, no incluye metadatos fuertes, requiere validacion manual de tipos y puede presentar problemas con separadores, saltos de linea o codificacion. |
| XML | Permite estructuras jerarquicas, usa etiquetas descriptivas, puede validarse con esquemas y facilita el intercambio entre sistemas heterogeneos. | Es mas verboso que CSV, produce archivos mas pesados, requiere mayor costo de procesamiento y su lectura o escritura suele ser mas compleja. |

### 1. Diferenciacion de procesos: Serializacion y Deserializacion

La **serializacion** es el proceso de convertir un objeto de C# en una representacion transportable, normalmente texto JSON. Con la libreria nativa `System.Text.Json`, esto se realiza con metodos como `JsonSerializer.Serialize()` o `JsonSerializer.SerializeAsync()`. Por ejemplo, un objeto `Alumno` puede convertirse en una cadena JSON para enviarse por HTTP o guardarse en un archivo.

La **deserializacion** es el proceso inverso: toma un JSON recibido desde una API, archivo o mensaje externo y lo convierte de nuevo en un objeto tipado de C#. Con `System.Text.Json`, esto se realiza con `JsonSerializer.Deserialize<T>()` o `JsonSerializer.DeserializeAsync<T>()`. En este paso es importante manejar errores de formato mediante `JsonException` y configurar opciones como `PropertyNameCaseInsensitive = true` cuando los nombres de propiedades del JSON no coinciden exactamente en mayusculas y minusculas con las propiedades del modelo.

### 2. El antipatron del rendimiento N+1

El problema **N+1** ocurre cuando, durante la lectura de un archivo masivo, el sistema ejecuta una operacion extra contra la base de datos por cada registro procesado. Por ejemplo, si el archivo tiene 5,000 lineas y por cada linea se hace un `INSERT`, una consulta de validacion o un `SaveChangesAsync()`, el servidor termina realizando miles de viajes innecesarios a la base de datos.

Este comportamiento degrada el rendimiento porque aumenta la latencia, consume mas conexiones y sobrecarga el motor de base de datos. La estrategia correcta es aplicar **Batching**: leer el archivo linea por linea para no saturar la memoria, mapear los datos a una lista intermedia y luego insertar los registros en bloques. Para la actividad, la solucion solicitada consiste en usar `AddRange()` y una unica llamada a `SaveChangesAsync()` al finalizar el ciclo de lectura.

## Parte 2: Implementacion Practica en C#

### Desafio 1: Consumo de Endpoints y Deserializacion

El siguiente metodo realiza una peticion `GET` segura a `https://api.usac.edu/v1/alumnos`, valida el codigo de estado con `EnsureSuccessStatusCode()` y deserializa el JSON recibido hacia un objeto `Alumno`.

```csharp
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class Alumno
{
    public int Carne { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public decimal Promedio { get; set; }
}

public class AlumnoApiClient
{
    private readonly HttpClient _httpClient;

    public AlumnoApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Alumno?> ObtenerAlumnoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(
                "https://api.usac.edu/v1/alumnos",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Alumno? alumno = JsonSerializer.Deserialize<Alumno>(json, opciones);

            return alumno;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error al consultar el endpoint: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error al deserializar la respuesta JSON: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"La peticion fue cancelada o excedio el tiempo de espera: {ex.Message}");
            return null;
        }
    }
}
```

### Desafio 2: Endpoint para Carga Masiva CSV

El siguiente endpoint recibe un archivo mediante `IFormFile`, lo procesa con `StreamReader` y `ReadLineAsync()`, almacena los registros mapeados en una lista intermedia y realiza la insercion con `AddRange()` y una sola llamada a `SaveChangesAsync()`.

```csharp
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AlumnosController : ControllerBase
{
    private readonly ControlAcademicoContext _context;

    public AlumnosController(ControlAcademicoContext context)
    {
        _context = context;
    }

    [HttpPost("carga-masiva-csv")]
    public async Task<IActionResult> CargarMasivaCsv(IFormFile archivo)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest("Debe enviar un archivo CSV valido.");
        }

        var alumnos = new List<Alumno>();

        using Stream stream = archivo.OpenReadStream();
        using var reader = new StreamReader(stream);

        await reader.ReadLineAsync(); // Encabezado: Carne,Nombre,Correo,Promedio

        string? linea;
        int numeroLinea = 1;

        while ((linea = await reader.ReadLineAsync()) is not null)
        {
            numeroLinea++;

            if (string.IsNullOrWhiteSpace(linea))
            {
                continue;
            }

            string[] columnas = linea.Split(',');

            if (columnas.Length < 4)
            {
                return BadRequest($"La linea {numeroLinea} no tiene el formato esperado.");
            }

            bool carneValido = int.TryParse(columnas[0].Trim(), out int carne);
            bool promedioValido = decimal.TryParse(
                columnas[3].Trim(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal promedio);

            if (!carneValido || !promedioValido)
            {
                return BadRequest($"La linea {numeroLinea} contiene datos numericos invalidos.");
            }

            alumnos.Add(new Alumno
            {
                Carne = carne,
                Nombre = columnas[1].Trim(),
                Correo = columnas[2].Trim(),
                Promedio = promedio
            });
        }

        _context.Alumnos.AddRange(alumnos);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Carga masiva completada correctamente.",
            registrosInsertados = alumnos.Count
        });
    }
}
```

## Parte 3: Referencias Bibliograficas

- Facultad de Ingenieria, USAC. (2026). **Sesion 20: Integracion de Datos. Consumo de APIs Externas y Carga Masiva (CSV/XML).** Laboratorio del curso Introduccion a la Programacion y Computacion 2. Guatemala.
