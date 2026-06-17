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

# ACTIVIDAD 17 DE JUNIO

**Docente:** AUX. Fernando Jose Vicente Velasquez  
**Fecha de entrega:** 17 DE JUNIO 2026

</div>

# Actividad de Laboratorio: Arquitectura Multi-Nivel (N-Tier) y Patron Logico de Software (MVC) en .NET

**Duracion estimada:** 120 minutos  
**Modalidad:** Individual  
**Entregables:** reporte Markdown y proyecto web funcional en C#.

## Parte 1: Fundamentacion Teorica y Analisis Critico

### 1. El transito hacia los sistemas distribuidos y multi-capa

#### La limitacion del monolito local

Cuando la interfaz, la logica de negocio y el almacenamiento viven de forma exclusiva en una sola maquina fisica, los datos quedan encerrados en ese equipo. Esto provoca problemas de sincronizacion, porque cada usuario o estacion podria manejar copias distintas de la informacion. Tambien limita la escalabilidad, ya que toda la carga depende de los recursos de una maquina y no puede distribuirse facilmente entre servidores especializados.

En un sistema asi, cualquier falla local puede detener toda la aplicacion. Ademas, compartir informacion con otros clientes requiere mecanismos manuales o improvisados, lo que aumenta el riesgo de inconsistencias, perdida de datos y conflictos de concurrencia.

#### Distincion critica: Layers vs. Tiers

Las **capas logicas** son divisiones internas del software segun responsabilidad. Por ejemplo, una capa de presentacion, una capa de negocio y una capa de datos pueden existir dentro del mismo proyecto o proceso. Su objetivo principal es ordenar el codigo.

Los **niveles fisicos** son separaciones reales de despliegue en infraestructura. Un nivel puede ejecutarse en una maquina, servidor, contenedor o servicio distinto. Su objetivo principal es distribuir la ejecucion del sistema. Por eso, una aplicacion puede tener varias capas logicas pero estar desplegada en un solo nivel fisico, o puede separar esas capas en distintos niveles conectados por red.

#### Responsabilidades en la arquitectura de 3 niveles

| Nivel fisico | Mision principal | Tecnologia comun |
| --- | --- | --- |
| Nivel 1: Capa de Presentacion | Mostrar la interfaz al usuario, recibir entradas y enviar solicitudes al servidor. | Navegador web, HTML, CSS, JavaScript, Razor Views. |
| Nivel 2: Capa de Aplicacion o Negocio | Procesar reglas del negocio, validar peticiones, coordinar casos de uso y decidir que datos enviar o recibir. | ASP.NET Core MVC, controladores, servicios C#. |
| Nivel 3: Capa de Datos | Persistir, consultar y proteger la informacion centralizada. | SQL Server, PostgreSQL, MySQL u otro motor de base de datos. |

#### Seguridad perimetral

Exponer publicamente el puerto de una base de datos a internet es un error critico porque convierte al componente mas sensible del sistema en un objetivo directo. Un atacante podria intentar fuerza bruta de credenciales, explotar vulnerabilidades del motor, enumerar bases, interceptar configuraciones inseguras o ejecutar acciones destructivas si encuentra una mala politica de permisos.

La buena practica es mantener la base de datos en una red privada. Solo la capa de aplicacion debe comunicarse con ella, usando reglas de firewall, credenciales con privilegios minimos, cifrado en transito, VPN o redes internas. El cliente nunca deberia conectarse directamente al servidor de datos.

### 2. Desacoplamiento logico con el patron MVC

#### La crisis del codigo espagueti

Mezclar sentencias SQL, calculos de negocio y etiquetas visuales en un mismo archivo fisico produce codigo dificil de mantener. Un cambio pequeno en la interfaz puede romper la logica, y una modificacion en las reglas del negocio puede afectar la presentacion. Esto tambien aumenta la duplicacion, hace mas dificil localizar errores y vuelve riesgoso trabajar en equipo.

Desde el punto de vista de pruebas unitarias, el codigo espagueti es problematico porque no hay piezas aisladas que puedan probarse de forma independiente. Si la logica esta amarrada a HTML o a una conexion SQL directa, la prueba necesita demasiados elementos externos y deja de ser rapida, clara y confiable.

#### Separacion de preocupaciones

**Modelo:** representa los datos y reglas esenciales del dominio. En esta actividad, `Estudiante` modela el carne, nombre y promedio. El modelo no debe conocer como se muestran los datos porque su responsabilidad es describir la informacion, no renderizarla.

**Vista:** se encarga de presentar la informacion al usuario. Es pasiva porque recibe un modelo y lo transforma en HTML. Puede tener instrucciones de visualizacion, pero no debe contener SQL, acceso directo a base de datos ni reglas centrales de negocio.

**Controlador:** actua como intermediario tactico. Recibe la peticion HTTP, valida datos basicos, coordina el modelo o los servicios necesarios y decide que respuesta devolver. Funciona como director de orquesta porque no toca todos los instrumentos, sino que dirige el flujo entre entrada, dominio y salida.

#### Metricas de ingenieria de software

MVC favorece la **alta cohesion** porque cada componente tiene una responsabilidad clara: el modelo representa informacion, la vista renderiza y el controlador coordina. Tambien favorece el **bajo acoplamiento** porque cada pieza depende lo menos posible de las demas. Por ejemplo, la vista no necesita saber de donde vienen los datos y el modelo no necesita saber si sera mostrado en una tabla, una tarjeta o una respuesta JSON.

En un entorno profesional, esto mejora la mantenibilidad, facilita pruebas unitarias, reduce impactos colaterales y permite que distintas personas trabajen en interfaz, reglas y controladores sin pisarse constantemente.

## Parte 2: Modelado del Ciclo de Vida y Enrutamiento Semantico

### 1. Mapeo analitico de URLs

Plantilla convencional usada por ASP.NET Core MVC:

```text
{controller=Home}/{action=Index}/{id?}
```

| URL entrante del cliente | Clase controladora buscada por el framework | Metodo ejecutado | Parametro `id` inyectado |
| --- | --- | --- | --- |
| `https://ingenieria.usac.edu.gt/ControlAcademico/Login` | `ControlAcademicoController` | `Login` | Ninguno |
| `https://ingenieria.usac.edu.gt/Estudiante/Historial/20260123` | `EstudianteController` | `Historial` | `20260123` |
| `https://ingenieria.usac.edu.gt/Asignacion/Detalle/10` | `AsignacionController` | `Detalle` | `10` |
| `https://ingenieria.usac.edu.gt/Home` | `HomeController` | `Index` | Ninguno / opcional |

### 2. Diagramacion del flujo interactivo

1. El usuario hace clic en un boton o enlace desde el navegador. El navegador construye una peticion HTTP `GET` o `POST` hacia una URL especifica.
2. La peticion entra al pipeline de ASP.NET Core. El motor de enrutamiento compara la URL con la plantilla `{controller=Home}/{action=Index}/{id?}` y determina el controlador, la accion y el parametro opcional.
3. El controlador recibe la peticion. Si hay datos de entrada, realiza validaciones perimetrales y delega el trabajo al modelo o a servicios de negocio.
4. El modelo representa o prepara los datos del dominio. En una arquitectura completa, aqui tambien podria intervenir una capa de datos para consultar informacion persistente.
5. El controlador selecciona una vista y le inyecta el modelo. Razor genera HTML dinamico y ASP.NET Core devuelve la respuesta al navegador para que el usuario vea la pagina renderizada.

## Parte 3: Implementacion Practica - Sistema de Control Academico

### Creacion del espacio de trabajo

Se creo el proyecto dentro del directorio de la actividad:

```bash
dotnet new webapp -o ControlAcademicoMvc
```

> Nota: la maquina local tiene SDK .NET 10 y .NET 6 instalados, pero no SDK .NET 8. Para mantener el proyecto compilable en el entorno actual, el archivo `.csproj` conserva `net10.0`. La estructura, patron MVC y codigo solicitado se implementaron siguiendo el enunciado de la actividad.

### Estructura implementada

```text
Actividad_17_de_Junio/
+-- Reporte.md
+-- ControlAcademicoMvc/
    +-- Controllers/
    |   +-- EstudianteController.cs
    |   +-- HomeController.cs
    +-- Models/
    |   +-- Estudiante.cs
    +-- Views/
    |   +-- Estudiante/
    |   |   +-- Historial.cshtml
    |   |   +-- Listar.cshtml
    |   +-- Shared/
    |   |   +-- _Layout.cshtml
    |   +-- _ViewImports.cshtml
    |   +-- _ViewStart.cshtml
    +-- Program.cs
```

### Modelo

```csharp
namespace ControlAcademicoMvc.Models;

public class Estudiante
{
    public int Carne { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public double Promedio { get; set; }
}
```

### Controlador

El controlador `EstudianteController` mantiene una lista estatica en memoria para simular el nivel de datos. Sus acciones son delgadas:

- `Listar()` devuelve la vista con la coleccion de estudiantes.
- `Historial(int id)` consulta un estudiante por carne y devuelve una vista de detalle.
- `Registrar([FromBody] Estudiante nuevoEstudiante)` valida datos minimos, registra en memoria y devuelve `201 Created`.

### Program.cs

El archivo `Program.cs` habilita controladores con vistas y configura la ruta convencional:

```csharp
builder.Services.AddControllersWithViews();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

### Pruebas sugeridas

```bash
dotnet run
```

Luego se pueden verificar estas rutas:

```text
GET  /Estudiante/Listar
GET  /Estudiante/Historial/2026012
POST /Estudiante/Registrar
```

Ejemplo de cuerpo JSON para registrar:

```json
{
  "carne": 202501955,
  "nombre": "Alvaro Moises Giron Morales",
  "promedio": 95.0
}
```

## Parte 4: Auditoria y Control de Calidad

### 1. Prueba de cohesion

La accion `GET /Estudiante/Listar` responde con una vista HTML limpia basada en el modelo enviado por el controlador. El controlador no mezcla SQL, HTML manual ni calculos internos complejos. Su responsabilidad se limita a despachar la coleccion de estudiantes hacia la vista.

La vista `Listar.cshtml` se encarga unicamente de presentar los datos en una tabla. El modelo `Estudiante.cs` solo contiene propiedades del dominio. Por lo tanto, se mantiene la separacion de responsabilidades esperada.

### 2. Evaluacion de antipatrones

Se reviso `EstudianteController.cs` para evitar el antipatron de Controladores Gordos. Ningun metodo supera las 20 lineas:

| Metodo | Responsabilidad | Resultado |
| --- | --- | --- |
| `Listar()` | Enviar datos a la vista | Controlador delgado |
| `Historial(int id)` | Buscar por carne y responder vista o error | Controlador delgado |
| `Registrar(...)` | Validar datos minimos y registrar en memoria | Controlador delgado |

La implementacion conserva alta cohesion y bajo acoplamiento, porque cada archivo cumple una responsabilidad especifica dentro del patron MVC.

## Parte 5: Referencias Bibliograficas

- Facultad de Ingenieria, USAC. (2026). **Sesion 11: Modelado Base y Arquitecturas de Despliegue. Evolucion de Sistemas Distribuidos, Fundamentos del Modelo Cliente-Servidor y Diseno Fisico Multi-Capas (N-Tier).** Laboratorio del curso Introduccion a la Programacion y Computacion 2. Guatemala.
- Facultad de Ingenieria, USAC. (2026). **Sesion 12: Arquitectura y Componentes del Patron MVC. Desacoplamiento Logico de Software, Ciclo de Vida de las Peticiones y Enrutamiento en Aplicaciones Interactivas Modernas.** Laboratorio del curso Introduccion a la Programacion y Computacion 2. Guatemala.
