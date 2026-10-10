# Guía de ejecución paso a paso

Esta guía te lleva desde cero hasta tener la práctica funcionando y lista para entregar.
Sigue los pasos **en orden**; cada paso dice qué resultado debes ver antes de continuar.

> Nota: el código fue escrito siguiendo la guía de la UNI, pero **no se compiló en el entorno donde se generó**
> (no tenía .NET instalado). Si al compilar aparece algún error, ver la sección 10.

---

## 0. Mapa: qué sección de la guía corresponde a qué archivo

| Sección de la guía | Dónde está |
|---|---|
| 5. C4 Nivel 1 | `docs/diagramas/c4-nivel1-contexto.mmd` |
| 6. C4 Nivel 2 | `docs/diagramas/c4-nivel2-contenedores.mmd` |
| 7. Secuencia login + proyectos | `docs/diagramas/secuencia-login-proyectos.mmd` |
| 8. Secuencia registrar propuesta | `docs/diagramas/secuencia-registrar-propuesta.mmd` |
| 9. Secuencia revisión del tutor | `docs/diagramas/secuencia-revision-tutor.mmd` |
| 10. README con Mermaid | `README.md` |
| 11. API en C# / ASP.NET Core | `src/GraduacionUni/` |
| 12. Script SQL Server | `database/01_crear_base_de_datos.sql` |
| 13. Diseño API-First | tabla en `README.md` + Swagger |
| 15. Ejercicios 1 a 4 | ver paso 8 de esta guía |
| 16. Investigación | `Informe_Investigacion_C4_API_First.docx` (aparte) |

---

## 1. Instalar lo necesario

| Herramienta | Para qué | Cómo comprobar |
|---|---|---|
| .NET SDK 8 (o más nuevo) | Compilar y ejecutar la API | `dotnet --version` |
| SQL Server (Developer o Express) | Base de datos | abrir SSMS y conectarse |
| SSMS o Azure Data Studio | Ejecutar el script SQL | — |
| Git | Control de versiones | `git --version` |
| VS Code o Visual Studio 2022 | Editar código | — |
| Cuenta de GitHub | Publicar el README con los diagramas | — |

Si usas Mac/Linux o no puedes instalar SQL Server, usa Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<UNA_CLAVE_FUERTE>" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

**Resultado esperado:** `dotnet --version` muestra 8.x o superior.

---

## 2. Probar los diagramas en Mermaid Live (secciones 5 a 9 de la guía)

Para **cada** archivo de `docs/diagramas/`:

1. Abre <https://mermaid.live/>.
2. Borra el ejemplo que aparece.
3. Abre el `.mmd` con un editor de texto, copia todo y pégalo.
4. Verifica que el diagrama se dibuje sin errores.
5. Revisa los puntos de la guía: actores legibles, sistema central con nombre y descripción, externos fuera del límite.
6. Toma una **captura de pantalla** (la pide el informe) y/o exporta SVG/PNG con el menú *Actions*.

**Resultado esperado:** los 8 diagramas se dibujan. Si un diagrama C4 sale desordenado, es normal: el soporte C4 de Mermaid es experimental; lo importante es que renderice.

---

## 3. Crear la base de datos (sección 12)

1. Abre SSMS y conéctate a tu instancia (por ejemplo `localhost` o `localhost\SQLEXPRESS`).
2. *File → Open → File…* y elige `database/01_crear_base_de_datos.sql`.
3. Presiona **F5**.
4. Verifica: *Databases → GraduacionUNI → Tables* muestra `Users`, `Projects`, `Reviews`, `ProjectDocuments`.

> Este paso es opcional: en modo Development, la API crea las tablas sola si la base está vacía.
> Pero ejecutar el script te sirve para mostrarlo en la defensa.

---

## 4. Configurar la conexión y los secretos

Entra a la carpeta del proyecto:

```bash
cd src/GraduacionUni
```

**4.1 Cadena de conexión.** El archivo `appsettings.Development.json` trae:

```
Server=localhost;Database=GraduacionUNI;Trusted_Connection=True;TrustServerCertificate=True;
```

Si tu instancia es otra, **no edites el archivo**: guarda la tuya en secretos.

| Tu caso | Cadena de conexión |
|---|---|
| SQL Server Express | `Server=localhost\SQLEXPRESS;Database=GraduacionUNI;Trusted_Connection=True;TrustServerCertificate=True;` |
| LocalDB (Visual Studio) | `Server=(localdb)\MSSQLLocalDB;Database=GraduacionUNI;Trusted_Connection=True;TrustServerCertificate=True;` |
| Docker / usuario sa | `Server=localhost,1433;Database=GraduacionUNI;User Id=sa;Password=<TU_CLAVE>;TrustServerCertificate=True;` |

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<tu cadena de conexión>"
```

**4.2 Clave del JWT** (obligatoria; la API no arranca sin ella). Debe tener **32 caracteres o más** y ser aleatoria:

```bash
# Genera una (Git Bash o Linux/Mac):
openssl rand -base64 48

# Guárdala:
dotnet user-secrets set "Jwt:Key" "<pega-aquí-la-clave-generada>"
```

**Resultado esperado:** `dotnet user-secrets list` muestra `Jwt:Key` (y la conexión si la cambiaste).
Esto cumple el punto de la lista de comprobación: *"el repositorio no contiene secretos reales"*.

---

## 5. Compilar y ejecutar

```bash
dotnet dev-certs https --trust      # solo la primera vez (certificado HTTPS de desarrollo)
dotnet restore
dotnet build
dotnet run --launch-profile https
```

**Resultado esperado:** la consola muestra `Now listening on: https://localhost:7080` y se abre el navegador en
`https://localhost:7080/swagger`. En el primer arranque se crean los 4 usuarios de prueba y un proyecto de ejemplo.
En desarrollo también puedes abrir `http://localhost:5080/swagger`; las solicitudes HTTP no se redirigen a HTTPS.

| Usuario | Rol | Contraseña (solo desarrollo) |
|---|---|---|
| estudiante@uni.edu.ni | Estudiante | `Uni2026*Dev` |
| tutor@uni.edu.ni | Tutor | `Uni2026*Dev` |
| coordinador@uni.edu.ni | Coordinador | `Uni2026*Dev` |
| admin@uni.edu.ni | Administrador | `Uni2026*Dev` |

---

## 6. Probar la API en Swagger (sección 13)

Haz esta secuencia; así demuestras cada flecha de los diagramas.

| # | Acción | Resultado esperado |
|---|---|---|
| 1 | `POST /api/auth/login` con estudiante y clave **incorrecta** | **401** `Credenciales inválidas.` |
| 2 | `POST /api/auth/login` con estudiante y clave correcta | **200** con `token`. Cópialo |
| 3 | Botón **Authorize** (arriba a la derecha) → pega el token → *Authorize* | candado cerrado |
| 4 | `GET /api/projects/mine` | **200** con el proyecto de ejemplo |
| 5 | `POST /api/projects` con título de 2 letras | **400** (validación) |
| 6 | `POST /api/projects` con título válido | **201** y el nuevo proyecto en estado `Propuesta` |
| 7 | `POST /api/projects/{id}/documents` con un PDF pequeño | **201** con metadatos |
| 8 | `GET /api/projects/{id}` | **200** con documentos incluidos |
| 9 | *Authorize* con token del **tutor** → `POST /api/projects/1/reviews` | **201** (y en la consola: «Notificación simulada») |
| 10 | Tutor intenta `PATCH /api/projects/1/status` | **403** (rol no permitido) |
| 11 | *Authorize* con token del **coordinador** → `PATCH /api/projects/1/status` `{"status":"Revision"}` | **200** |
| 12 | Mismo endpoint con `{"status":"Cerrada"}` | **400** (transición no permitida) |
| 13 | `PATCH /api/projects/2/tutor` `{"tutorId":2}` | **200** |
| 14 | Pedir un proyecto inexistente (`/api/projects/999`) | **404** |

También puedes usar el archivo `src/GraduacionUni/GraduacionUni.http` (extensión REST Client de VS Code o Visual Studio) o Postman.

**Tip para la defensa:** cada fila de esta tabla corresponde a una flecha de un diagrama de secuencia.

---

## 7. Publicar en GitHub y comprobar el README (sección 10)

1. En GitHub crea un repositorio vacío llamado `graduacion-uni` (sin README).
2. En la carpeta raíz del proyecto (donde está `README.md`):

```bash
git init
git add .
git status          # verifica que NO aparezcan bin/, obj/ ni archivos de secretos
git commit -m "feat: arquitectura C4, secuencias Mermaid y API ASP.NET Core"
git branch -M main
git remote add origin https://github.com/<tu-usuario>/graduacion-uni.git
git push -u origin main
```

3. Abre el repositorio en GitHub y revisa que los bloques `mermaid` del README **se dibujen** como diagramas.
4. Toma una captura del README renderizado (la pide el informe).

> Si un diagrama C4 no se dibuja en GitHub, deja el bloque `mermaid` y agrega debajo la imagen exportada
> desde Mermaid Live (`![Contexto](docs/img/contexto.png)`). Es una solución válida y se explica en el informe.

**Buena práctica de commits** (punto 13 de la sección 14): haz commits pequeños con mensajes claros,
por ejemplo `docs: agrega diagrama de secuencia de revisión` o `fix: valida extensión de documentos`.

---

## 8. Ejercicios propuestos (sección 15)

| Ejercicio | Dónde está resuelto | Qué hacer |
|---|---|---|
| 1. C4 Nivel 1 con Registro Académico | `docs/diagramas/ejercicio1-c4-registro-academico.mmd` | Pegar en Mermaid Live y capturar |
| 2. C4 Nivel 2 con notificaciones | `docs/diagramas/ejercicio2-c4-notificaciones.mmd` | Responder: ¿qué pasa por la API? **Todo**: la web solo habla con la API; la API habla con la BD y con notificaciones |
| 3. Secuencia aprobar proyecto | `docs/diagramas/secuencia-aprobar-proyecto.mmd` | Probar en Swagger el paso 11 del apartado 6 |
| 4. `GET /api/projects/{id}` sin devolver entidades | `ProjectsController.GetById` + `ProjectService.GetByIdAsync` | Mostrar que devuelve `ProjectDetailResponse` (DTO), no `Project` |

---

## 9. Informe y defensa (sección 16)

El informe (5 a 8 páginas) se entrega aparte: `Informe_Investigacion_C4_API_First.docx`. Antes de entregarlo:

1. Reemplaza los marcadores `[PEGAR CAPTURA…]` por tus capturas (Mermaid Live y README en GitHub).
2. Pon tu nombre, equipo y fecha en la portada.
3. Verifica que las referencias abran (son documentación oficial).
4. Ajusta las conclusiones con lo que **tú** viviste al implementarlo.

**Defensa oral (8 a 10 minutos):** estructura sugerida.

| Minutos | Contenido |
|---|---|
| 0–1 | Problema y alcance |
| 1–3 | C4 Nivel 1 y 2: ¿por qué esas cajas y flechas? |
| 3–5 | Una secuencia (login) conectada a un endpoint real |
| 5–7 | Demo en Swagger (tabla del apartado 6) |
| 7–9 | Decisiones: API-First, DTO, DI, seguridad |
| 9–10 | Limitaciones y qué cambiarías con 10,000 usuarios |

La rúbrica evalúa que **expliques sin leer**: practica explicando cada flecha con tus palabras.

---

## 10. Solución de problemas

| Síntoma | Causa probable | Solución |
|---|---|---|
| `Falta la clave JWT` al arrancar | No configuraste `Jwt:Key` | Paso 4.2 |
| `Falta la cadena de conexión` | Ejecutas fuera de Development | Usa `--launch-profile https` o `set ASPNETCORE_ENVIRONMENT=Development` |
| `A connection was successfully established… certificate chain not trusted` | Falta `TrustServerCertificate=True` | Agrégalo a la cadena |
| `Login failed for user` / `Cannot open database` | Instancia o credenciales incorrectas | Revisa tu cadena (tabla del paso 4.1) |
| `A network-related or instance-specific error` | SQL Server apagado o nombre de instancia errado | Inicia el servicio de SQL Server; verifica el nombre en SSMS |
| El navegador avisa de certificado no confiable | No ejecutaste `dev-certs` | `dotnet dev-certs https --trust` |
| `429 Too Many Requests` en login | Más de 10 intentos por minuto desde tu IP | Espera 1 minuto |
| `401` en endpoints protegidos | Token vencido (60 min) o sin *Authorize* | Haz login de nuevo |
| El diagrama C4 no se ve en GitHub | Soporte C4 experimental | Ver nota del paso 7 |
| Error de compilación | Código no probado en el entorno de generación | Copia el mensaje exacto del error y pídeme que lo corrija |
| `NU1903` (advertencia de vulnerabilidad en un paquete) | Paquete con aviso de seguridad | Es una advertencia, no un error; `dotnet add package <nombre>` lo actualiza |

---

## 11. Lista final de entrega (sección 21 de la guía)

- [ ] C4 Nivel 1 identifica actores y sistemas externos
- [ ] C4 Nivel 2 identifica aplicaciones y almacenes de datos
- [ ] Ningún actor se conecta directo a SQL Server
- [ ] Los endpoints de las secuencias existen (tabla de `README.md`)
- [ ] Los Mermaid funcionan en Mermaid Live Editor (paso 2)
- [ ] El README tiene bloques `mermaid` bien cerrados y se ven en GitHub (paso 7)
- [ ] El código C# separa responsabilidades y usa inyección de dependencias
- [ ] Las contraseñas nunca se guardan en texto plano (`PasswordHasher`)
- [ ] El repositorio no contiene secretos reales (`git status` revisado)
- [ ] Puedes explicar cada flecha sin leer
