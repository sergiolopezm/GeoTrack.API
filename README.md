# **ESTRUCTURA DE SOFTWARE**

# **SERVICIO GEOTRACK API**

|  |  |
| --- | --- |
| **CAPA** | BACKEND |
| **PLATAFORMA** | SERVER – WINDOWS |
| **TIPO** | .NET |

## 1. DESCRIPCIÓN GENERAL

El servicio GeoTrack API proporciona una interfaz para la gestión de ubicaciones geográficas, permitiendo el mantenimiento de datos jerárquicos como países, departamentos y ciudades. El sistema incluye funcionalidades de autenticación basada en JWT, gestión de usuarios y registro de actividades.

La API está diseñada siguiendo principios RESTful y utiliza Entity Framework Core para la comunicación con la base de datos SQL Server. Proporciona endpoints para realizar operaciones CRUD (Crear, Leer, Actualizar, Eliminar) sobre las entidades geográficas y gestión de usuarios.

## 2. REQUISITOS PREVIOS

### 2.1. Estructura de Base de Datos

Para el funcionamiento correcto del sistema, es necesario crear las siguientes tablas en la base de datos:

#### 2.1.1. Tabla Accesos
```sql
CREATE TABLE Accesos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Sitio NVARCHAR(50) NOT NULL,
    Contraseña NVARCHAR(250) NOT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo BIT NOT NULL DEFAULT 1
);
```

#### 2.1.2. Tabla Roles
```sql
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(200) NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo BIT NOT NULL DEFAULT 1
);
```

#### 2.1.3. Tabla Usuarios
```sql
CREATE TABLE Usuarios (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    NombreUsuario NVARCHAR(100) NOT NULL UNIQUE,
    Contraseña NVARCHAR(250) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    RolId INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    FechaUltimoAcceso DATETIME2 NULL,
    FOREIGN KEY (RolId) REFERENCES Roles(Id)
);
```

#### 2.1.4. Tabla Tokens
```sql
CREATE TABLE Tokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Token NVARCHAR(1000) NOT NULL,
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    Ip NVARCHAR(45) NOT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaExpiracion DATETIME2 NOT NULL,
    Observacion NVARCHAR(200) NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 2.1.5. Tabla TokensExpirados
```sql
CREATE TABLE TokensExpirados (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Token NVARCHAR(1000) NOT NULL,
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    Ip NVARCHAR(45) NOT NULL,
    FechaCreacion DATETIME2 NOT NULL,
    FechaExpiracion DATETIME2 NOT NULL,
    Observacion NVARCHAR(200) NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 2.1.6. Tabla Logs
```sql
CREATE TABLE Logs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME2 NOT NULL DEFAULT GETDATE(),
    Tipo NVARCHAR(50) NOT NULL,
    UsuarioId UNIQUEIDENTIFIER NULL,
    Ip NVARCHAR(45) NULL,
    Accion NVARCHAR(200) NULL,
    Detalle NVARCHAR(MAX) NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

#### 2.1.7. Tabla Paises
```sql
CREATE TABLE Paises (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    CodigoISO NVARCHAR(3) NOT NULL UNIQUE,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    CreadoPorId UNIQUEIDENTIFIER NULL,
    ModificadoPorId UNIQUEIDENTIFIER NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CreadoPorId) REFERENCES Usuarios(Id),
    FOREIGN KEY (ModificadoPorId) REFERENCES Usuarios(Id)
);
```

#### 2.1.8. Tabla Departamentos
```sql
CREATE TABLE Departamentos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    PaisId INT NOT NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    CreadoPorId UNIQUEIDENTIFIER NULL,
    ModificadoPorId UNIQUEIDENTIFIER NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (PaisId) REFERENCES Paises(Id),
    FOREIGN KEY (CreadoPorId) REFERENCES Usuarios(Id),
    FOREIGN KEY (ModificadoPorId) REFERENCES Usuarios(Id)
);
```

#### 2.1.9. Tabla Ciudades
```sql
CREATE TABLE Ciudades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    DepartamentoId INT NOT NULL,
    CodigoPostal NVARCHAR(20) NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    CreadoPorId UNIQUEIDENTIFIER NULL,
    ModificadoPorId UNIQUEIDENTIFIER NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (DepartamentoId) REFERENCES Departamentos(Id),
    FOREIGN KEY (CreadoPorId) REFERENCES Usuarios(Id),
    FOREIGN KEY (ModificadoPorId) REFERENCES Usuarios(Id)
);
```

### 2.2. Datos Iniciales

Es necesario insertar los siguientes registros iniciales:

```sql
-- Insertar roles iniciales
INSERT INTO Roles (Nombre, Descripcion) VALUES 
('Administrador', 'Control total del sistema'),
('Gerente', 'Gestión de operaciones y reportes'),
('Usuario', 'Acceso a consultas básicas'),
('Empleado', 'Acceso a funciones operativas');

-- Insertar configuración de acceso
INSERT INTO Accesos (Sitio, Contraseña) VALUES 
('GeoTrack', 'GeoTrack2025');

-- Insertar usuario administrador inicial
DECLARE @adminRolId INT = (SELECT Id FROM Roles WHERE Nombre = 'Administrador');
INSERT INTO Usuarios (NombreUsuario, Contraseña, Nombre, Apellido, Email, RolId) 
VALUES ('admin', 'Admin123', 'Administrador', 'Sistema', 'admin@geotrack.com', @adminRolId);
```

## 3. MÉTODOS

### 3.1. Autenticación

#### 3.1.1. Login

Autentica un usuario en el sistema y devuelve un token JWT.

Acceso: `api/Auth/login`  
Formato: JSON  
Servicio: REST / POST

##### 3.1.1.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombreUsuario | Nombre de usuario | String | Sí |
| contraseña | Contraseña del usuario | String | Sí |
| ip | Dirección IP del cliente | String | No |

Ejemplo de entrada:
```json
{
  "nombreUsuario": "admin",
  "contraseña": "Admin123",
  "ip": "192.168.1.1"
}
```

##### 3.1.1.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con datos del usuario autenticado | Object |
| resultado.usuario | Datos del usuario | Object |
| resultado.usuario.id | Identificador único del usuario | GUID |
| resultado.usuario.nombreUsuario | Nombre de usuario | String |
| resultado.usuario.nombre | Nombre real del usuario | String |
| resultado.usuario.apellido | Apellido del usuario | String |
| resultado.usuario.email | Correo electrónico del usuario | String |
| resultado.usuario.rol | Rol del usuario en el sistema | String |
| resultado.usuario.rolId | ID del rol del usuario | Integer |
| resultado.usuario.activo | Estado de activación del usuario | Boolean |
| resultado.usuario.fechaCreacion | Fecha de creación del usuario | DateTime |
| resultado.usuario.fechaUltimoAcceso | Fecha del último acceso | DateTime |
| resultado.token | Token JWT para autenticación | String |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Usuario autenticado",
  "detalle": "El usuario admin se ha autenticado correctamente.",
  "resultado": {
    "usuario": {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "nombreUsuario": "admin",
      "nombre": "Administrador",
      "apellido": "Sistema",
      "email": "admin@geotrack.com",
      "rol": "Administrador",
      "rolId": 1,
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaUltimoAcceso": "2024-01-17T12:30:45"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

#### 3.1.2. Registro

Registra un nuevo usuario en el sistema.

Acceso: `api/Auth/registro`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.1.2.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombreUsuario | Nombre de usuario | String | Sí |
| contraseña | Contraseña del usuario | String | Sí |
| nombre | Nombre real del usuario | String | Sí |
| apellido | Apellido del usuario | String | Sí |
| email | Correo electrónico | String | Sí |
| rolId | ID del rol asignado | Integer | Sí |

Ejemplo de entrada:
```json
{
  "nombreUsuario": "usuario1",
  "contraseña": "Password123",
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan.perez@ejemplo.com",
  "rolId": 3
}
```

##### 3.1.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con datos del usuario registrado | Object |
| resultado.id | Identificador único del usuario | GUID |
| resultado.nombreUsuario | Nombre de usuario | String |
| resultado.email | Correo electrónico del usuario | String |
| resultado.rol | Rol del usuario en el sistema | String |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Usuario registrado",
  "detalle": "El usuario se ha registrado correctamente",
  "resultado": {
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "nombreUsuario": "usuario1",
    "email": "juan.perez@ejemplo.com",
    "rol": "Usuario"
  }
}
```

#### 3.1.3. Obtener Perfil

Obtiene el perfil del usuario autenticado.

Acceso: `api/Auth/perfil`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.1.3.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con datos del perfil | Object |
| resultado.id | Identificador único del usuario | GUID |
| resultado.nombreUsuario | Nombre de usuario | String |
| resultado.nombre | Nombre real del usuario | String |
| resultado.apellido | Apellido del usuario | String |
| resultado.email | Correo electrónico del usuario | String |
| resultado.rol | Rol del usuario en el sistema | String |
| resultado.rolId | ID del rol del usuario | Integer |
| resultado.activo | Estado de activación del usuario | Boolean |
| resultado.fechaCreacion | Fecha de creación del usuario | DateTime |
| resultado.fechaUltimoAcceso | Fecha del último acceso | DateTime |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Perfil obtenido",
  "detalle": "Perfil de usuario obtenido correctamente",
  "resultado": {
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "nombreUsuario": "admin",
    "nombre": "Administrador",
    "apellido": "Sistema",
    "email": "admin@geotrack.com",
    "rol": "Administrador",
    "rolId": 1,
    "activo": true,
    "fechaCreacion": "2023-01-01T00:00:00",
    "fechaUltimoAcceso": "2024-01-17T12:30:45"
  }
}
```

#### 3.1.4. Logout

Cierra la sesión del usuario invalidando el token actual.

Acceso: `api/Auth/logout`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.1.4.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto de resultado (nulo en este caso) | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Logout exitoso",
  "detalle": "Sesión cerrada correctamente",
  "resultado": null
}
```

### 3.2. Gestión de Países

#### 3.2.1. Obtener Todos los Países

Obtiene la lista de todos los países.

Acceso: `api/Pais`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.2.1.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de países | Array |
| resultado[].id | Identificador único del país | Integer |
| resultado[].nombre | Nombre del país | String |
| resultado[].codigoISO | Código ISO del país | String |
| resultado[].activo | Estado de activación del país | Boolean |
| resultado[].fechaCreacion | Fecha de creación | DateTime |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado[].departamentosCount | Cantidad de departamentos asociados | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Países obtenidos",
  "detalle": "Se han obtenido 5 países",
  "resultado": [
    {
      "id": 1,
      "nombre": "Colombia",
      "codigoISO": "COL",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "departamentosCount": 4
    },
    {
      "id": 2,
      "nombre": "Estados Unidos",
      "codigoISO": "USA",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "departamentosCount": 0
    }
  ]
}
```

#### 3.2.2. Obtener País por ID

Obtiene un país específico por su ID.

Acceso: `api/Pais/{id}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.2.2.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del país | Integer | Sí |

##### 3.2.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del país | Object |
| resultado.id | Identificador único del país | Integer |
| resultado.nombre | Nombre del país | String |
| resultado.codigoISO | Código ISO del país | String |
| resultado.activo | Estado de activación del país | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.creadoPor | Nombre del usuario que creó el registro | String |
| resultado.modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado.departamentosCount | Cantidad de departamentos asociados | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "País obtenido",
  "detalle": "Se ha obtenido el país 'Colombia'",
  "resultado": {
    "id": 1,
    "nombre": "Colombia",
    "codigoISO": "COL",
    "activo": true,
    "fechaCreacion": "2023-01-01T00:00:00",
    "fechaModificacion": null,
    "creadoPorId": null,
    "modificadoPorId": null,
    "creadoPor": null,
    "modificadoPor": null,
    "departamentosCount": 4
  }
}
```

#### 3.2.3. Obtener Países Paginados

Obtiene una lista paginada de países con opciones de filtrado.

Acceso: `api/Pais/paginado`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.2.3.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** | **Valor Predeterminado** |
|------------|-----------------|----------|---------------|--------------------------|
| pagina | Número de página | Integer | No | 1 |
| elementosPorPagina | Cantidad de elementos por página | Integer | No | 10 |
| busqueda | Texto para filtrar resultados | String | No | null |

##### 3.2.3.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con resultados paginados | Object |
| resultado.pagina | Número de página actual | Integer |
| resultado.elementosPorPagina | Cantidad de elementos por página | Integer |
| resultado.totalPaginas | Total de páginas disponibles | Integer |
| resultado.totalRegistros | Total de registros encontrados | Integer |
| resultado.lista | Lista de países | Array |
| resultado.lista[].id | Identificador único del país | Integer |
| resultado.lista[].nombre | Nombre del país | String |
| resultado.lista[].codigoISO | Código ISO del país | String |
| resultado.lista[].activo | Estado de activación del país | Boolean |
| resultado.lista[].fechaCreacion | Fecha de creación | DateTime |
| resultado.lista[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado.lista[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.lista[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.lista[].creadoPor | Nombre del usuario que creó el registro | String |
| resultado.lista[].modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado.lista[].departamentosCount | Cantidad de departamentos asociados | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Países obtenidos",
  "detalle": "Se han obtenido 5 países de un total de 5",
  "resultado": {
    "pagina": 1,
    "elementosPorPagina": 10,
    "totalPaginas": 1,
    "totalRegistros": 5,
    "lista": [
      {
        "id": 1,
        "nombre": "Colombia",
        "codigoISO": "COL",
        "activo": true,
        "fechaCreacion": "2023-01-01T00:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null,
        "departamentosCount": 4
      },
      {
        "id": 2,
        "nombre": "Estados Unidos",
        "codigoISO": "USA",
        "activo": true,
        "fechaCreacion": "2023-01-01T00:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null,
        "departamentosCount": 0
      }
    ]
  }
}
```

#### 3.2.4. Crear País

Crea un nuevo país en el sistema.

Acceso: `api/Pais`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.2.4.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombre | Nombre del país | String | Sí |
| codigoISO | Código ISO del país (3 caracteres) | String | Sí |
| activo | Estado de activación del país | Boolean | No |

Ejemplo de entrada:
```json
{
  "nombre": "Brasil",
  "codigoISO": "BRA",
  "activo": true
}
```

##### 3.2.4.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del país creado | Object |
| resultado.id | Identificador único del país | Integer |
| resultado.nombre | Nombre del país | String |
| resultado.codigoISO | Código ISO del país | String |
| resultado.activo | Estado de activación del país | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "País creado",
  "detalle": "El país 'Brasil' ha sido creado correctamente",
  "resultado": {
    "id": 6,
    "nombre": "Brasil",
    "codigoISO": "BRA",
    "activo": true,
    "fechaCreacion": "2024-01-17T15:30:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.2.5. Actualizar País

Actualiza un país existente.

Acceso: `api/Pais/{id}`  
Formato: JSON  
Servicio: REST / PUT  
Autenticación: JWT requerido

##### 3.2.5.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del país | Integer | Sí |

##### 3.2.5.2. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del país | Integer | No |
| nombre | Nombre del país | String | Sí |
| codigoISO | Código ISO del país (3 caracteres) | String | Sí |
| activo | Estado de activación del país | Boolean | No |

Ejemplo de entrada:
```json
{
  "nombre": "Brasil",
  "codigoISO": "BRA",
  "activo": true
}
```

##### 3.2.5.3. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del país actualizado | Object |
| resultado.id | Identificador único del país | Integer |
| resultado.nombre | Nombre del país | String |
| resultado.codigoISO | Código ISO del país | String |
| resultado.activo | Estado de activación del país | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "País actualizado",
  "detalle": "El país 'Brasil' ha sido actualizado correctamente",
  "resultado": {
    "id": 6,
    "nombre": "Brasil",
    "codigoISO": "BRA",
    "activo": true,
    "fechaCreacion": "2024-01-17T15:30:00",
    "fechaModificacion": "2024-01-17T16:45:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "modificadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.2.6. Eliminar País

Elimina (desactiva) un país existente.

Acceso: `api/Pais/{id}`  
Formato: JSON  
Servicio: REST / DELETE  
Autenticación: JWT requerido

##### 3.2.6.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del país | Integer | Sí |

##### 3.2.6.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto de resultado (nulo en este caso) | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "País eliminado",
  "detalle": "El país 'Brasil' ha sido eliminado correctamente",
  "resultado": null
}
```

### 3.3. Gestión de Departamentos

#### 3.3.1. Obtener Todos los Departamentos

Obtiene la lista de todos los departamentos.

Acceso: `api/Departamento`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.3.1.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de departamentos | Array |
| resultado[].id | Identificador único del departamento | Integer |
| resultado[].nombre | Nombre del departamento | String |
| resultado[].paisId | ID del país al que pertenece | Integer |
| resultado[].pais | Nombre del país al que pertenece | String |
| resultado[].activo | Estado de activación del departamento | Boolean |
| resultado[].fechaCreacion | Fecha de creación | DateTime |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado[].ciudadesCount | Cantidad de ciudades asociadas | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Departamentos obtenidos",
  "detalle": "Se han obtenido 4 departamentos",
  "resultado": [
    {
      "id": 1,
      "nombre": "Antioquia",
      "paisId": 1,
      "pais": "Colombia",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "ciudadesCount": 3
    },
    {
      "id": 2,
      "nombre": "Cundinamarca",
      "paisId": 1,
      "pais": "Colombia",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "ciudadesCount": 3
    }
  ]
}
```

#### 3.3.2. Obtener Departamentos por País

Obtiene la lista de departamentos pertenecientes a un país específico.

Acceso: `api/Departamento/por-pais/{paisId}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.3.2.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| paisId | ID del país | Integer | Sí |

##### 3.3.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de departamentos | Array |
| resultado[].id | Identificador único del departamento | Integer |
| resultado[].nombre | Nombre del departamento | String |
| resultado[].paisId | ID del país al que pertenece | Integer |
| resultado[].pais | Nombre del país al que pertenece | String |
| resultado[].activo | Estado de activación del departamento | Boolean |
| resultado[].fechaCreacion | Fecha de creación | DateTime |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado[].ciudadesCount | Cantidad de ciudades asociadas | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Departamentos obtenidos",
  "detalle": "Se han obtenido 4 departamentos para el país ID: 1",
  "resultado": [
    {
      "id": 1,
      "nombre": "Antioquia",
      "paisId": 1,
      "pais": "Colombia",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "ciudadesCount": 3
    },
    {
      "id": 2,
      "nombre": "Cundinamarca",
      "paisId": 1,
      "pais": "Colombia",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null,
      "ciudadesCount": 3
    }
  ]
}
```

#### 3.3.3. Obtener Departamento por ID

Obtiene un departamento específico por su ID.

Acceso: `api/Departamento/{id}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.3.3.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del departamento | Integer | Sí |

##### 3.3.3.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del departamento | Object |
| resultado.id | Identificador único del departamento | Integer |
| resultado.nombre | Nombre del departamento | String |
| resultado.paisId | ID del país al que pertenece | Integer |
| resultado.pais | Nombre del país al que pertenece | String |
| resultado.activo | Estado de activación del departamento | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.creadoPor | Nombre del usuario que creó el registro | String |
| resultado.modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado.ciudadesCount | Cantidad de ciudades asociadas | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Departamento obtenido",
  "detalle": "Se ha obtenido el departamento 'Antioquia'",
  "resultado": {
    "id": 1,
    "nombre": "Antioquia",
    "paisId": 1,
    "pais": "Colombia",
    "activo": true,
    "fechaCreacion": "2023-01-01T00:00:00",
    "fechaModificacion": null,
    "creadoPorId": null,
    "modificadoPorId": null,
    "creadoPor": null,
    "modificadoPor": null,
    "ciudadesCount": 3
  }
}
```

#### 3.3.4. Obtener Departamentos Paginados

Obtiene una lista paginada de departamentos con opciones de filtrado.

Acceso: `api/Departamento/paginado`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.3.4.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** | **Valor Predeterminado** |
|------------|-----------------|----------|---------------|--------------------------|
| pagina | Número de página | Integer | No | 1 |
| elementosPorPagina | Cantidad de elementos por página | Integer | No | 10 |
| paisId | ID del país para filtrar | Integer | No | null |
| busqueda | Texto para filtrar resultados | String | No | null |

##### 3.3.4.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con resultados paginados | Object |
| resultado.pagina | Número de página actual | Integer |
| resultado.elementosPorPagina | Cantidad de elementos por página | Integer |
| resultado.totalPaginas | Total de páginas disponibles | Integer |
| resultado.totalRegistros | Total de registros encontrados | Integer |
| resultado.lista | Lista de departamentos | Array |
| resultado.lista[].id | Identificador único del departamento | Integer |
| resultado.lista[].nombre | Nombre del departamento | String |
| resultado.lista[].paisId | ID del país al que pertenece | Integer |
| resultado.lista[].pais | Nombre del país al que pertenece | String |
| resultado.lista[].activo | Estado de activación del departamento | Boolean |
| resultado.lista[].fechaCreacion | Fecha de creación | DateTime |
| resultado.lista[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado.lista[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.lista[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.lista[].creadoPor | Nombre del usuario que creó el registro | String |
| resultado.lista[].modificadoPor | Nombre del usuario que modificó el registro | String |
| resultado.lista[].ciudadesCount | Cantidad de ciudades asociadas | Integer |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Departamentos obtenidos",
  "detalle": "Se han obtenido 4 departamentos de un total de 4",
  "resultado": {
    "pagina": 1,
    "elementosPorPagina": 10,
    "totalPaginas": 1,
    "totalRegistros": 4,
    "lista": [
      {
        "id": 1,
        "nombre": "Antioquia",
        "paisId": 1,
        "pais": "Colombia",
        "activo": true,
        "fechaCreacion": "2023-01-01T00:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null,
        "ciudadesCount": 3
      },
      {
        "id": 2,
        "nombre": "Cundinamarca",
        "paisId": 1,
        "pais": "Colombia",
        "activo": true,
        "fechaCreacion": "2023-01-01T00:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null,
        "ciudadesCount": 3
      }
    ]
  }
}
```

#### 3.3.5. Crear Departamento

Crea un nuevo departamento en el sistema.

Acceso: `api/Departamento`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.3.5.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombre | Nombre del departamento | String | Sí |
| paisId | ID del país al que pertenece | Integer | Sí |
| activo | Estado de activación del departamento | Boolean | No |

Ejemplo de entrada:
```json
{
  "nombre": "Santander",
  "paisId": 1,
  "activo": true
}
```

##### 3.3.5.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del departamento creado | Object |
| resultado.id | Identificador único del departamento | Integer |
| resultado.nombre | Nombre del departamento | String |
| resultado.paisId | ID del país al que pertenece | Integer |
| resultado.pais | Nombre del país al que pertenece | String |
| resultado.activo | Estado de activación del departamento | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Departamento creado",
  "detalle": "El departamento 'Santander' ha sido creado correctamente",
  "resultado": {
    "id": 5,
    "nombre": "Santander",
    "paisId": 1,
    "pais": "Colombia",
    "activo": true,
    "fechaCreacion": "2024-01-17T15:30:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.3.6. Actualizar Departamento

Actualiza un departamento existente.

Acceso: `api/Departamento/{id}`  
Formato: JSON  
Servicio: REST / PUT  
Autenticación: JWT requerido

##### 3.3.6.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del departamento | Integer | Sí |

##### 3.3.6.2. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del departamento | Integer | No |
| nombre | Nombre del departamento | String | Sí |
| paisId | ID del país al que pertenece | Integer | Sí |
| activo | Estado de activación del departamento | Boolean | No |

Ejemplo de entrada:
```json
{
  "nombre": "Santander",
  "paisId": 1,
  "activo": true
}
```

##### 3.3.6.3. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información del departamento actualizado | Object |
| resultado.id | Identificador único del departamento | Integer |
| resultado.nombre | Nombre del departamento | String |
| resultado.paisId | ID del país al que pertenece | Integer |
| resultado.pais | Nombre del país al que pertenece | String |
| resultado.activo | Estado de activación del departamento | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Departamento actualizado",
  "detalle": "El departamento 'Santander' ha sido actualizado correctamente",
  "resultado": {
    "id": 5,
    "nombre": "Santander",
    "paisId": 1,
    "pais": "Colombia",
    "activo": true,
    "fechaCreacion": "2024-01-17T15:30:00",
    "fechaModificacion": "2024-01-17T16:45:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "modificadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.3.7. Eliminar Departamento

Elimina (desactiva) un departamento existente.

Acceso: `api/Departamento/{id}`  
Formato: JSON  
Servicio: REST / DELETE  
Autenticación: JWT requerido

##### 3.3.7.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del departamento | Integer | Sí |

##### 3.3.7.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto de resultado (nulo en este caso) | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Departamento eliminado",
  "detalle": "El departamento 'Santander' ha sido eliminado correctamente",
  "resultado": null
}
```

### 3.4. Gestión de Ciudades

#### 3.4.1. Obtener Todas las Ciudades

Obtiene la lista de todas las ciudades.

Acceso: `api/Ciudad`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.4.1.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de ciudades | Array |
| resultado[].id | Identificador único de la ciudad | Integer |
| resultado[].nombre | Nombre de la ciudad | String |
| resultado[].departamentoId | ID del departamento al que pertenece | Integer |
| resultado[].departamento | Nombre del departamento al que pertenece | String |
| resultado[].paisId | ID del país al que pertenece | Integer |
| resultado[].pais | Nombre del país al que pertenece | String |
| resultado[].codigoPostal | Código postal de la ciudad | String |
| resultado[].activo | Estado de activación de la ciudad | Boolean |
| resultado[].fechaCreacion | Fecha de creación | DateTime |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado[].modificadoPorId | ID del usuario que modificó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Ciudades obtenidas",
  "detalle": "Se han obtenido 6 ciudades",
  "resultado": [
    {
      "id": 1,
      "nombre": "Medellín",
      "departamentoId": 1,
      "departamento": "Antioquia",
      "paisId": 1,
      "pais": "Colombia",
      "codigoPostal": "050001",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null
    },
    {
      "id": 2,
      "nombre": "Envigado",
      "departamentoId": 1,
      "departamento": "Antioquia",
      "paisId": 1,
      "pais": "Colombia",
      "codigoPostal": "055420",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null
    }
  ]
}
```

#### 3.4.2. Obtener Ciudades por Departamento

Obtiene la lista de ciudades pertenecientes a un departamento específico.

Acceso: `api/Ciudad/por-departamento/{departamentoId}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.4.2.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| departamentoId | ID del departamento | Integer | Sí |

##### 3.4.2.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de ciudades | Array |
| resultado[].id | Identificador único de la ciudad | Integer |
| resultado[].nombre | Nombre de la ciudad | String |
| resultado[].departamentoId | ID del departamento al que pertenece | Integer |
| resultado[].departamento | Nombre del departamento al que pertenece | String |
| resultado[].paisId | ID del país al que pertenece | Integer |
| resultado[].pais | Nombre del país al que pertenece | String |
| resultado[].codigoPostal | Código postal de la ciudad | String |
| resultado[].activo | Estado de activación de la ciudad | Boolean |
| resultado[].fechaCreacion | Fecha de creación | DateTime |
| resultado[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado[].modificadoPorId | ID del usuario que modificó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Ciudades obtenidas",
  "detalle": "Se han obtenido 3 ciudades para el departamento ID: 1",
  "resultado": [
    {
      "id": 1,
      "nombre": "Medellín",
      "departamentoId": 1,
      "departamento": "Antioquia",
      "paisId": 1,
      "pais": "Colombia",
      "codigoPostal": "050001",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null
    },
    {
      "id": 2,
      "nombre": "Envigado",
      "departamentoId": 1,
      "departamento": "Antioquia",
      "paisId": 1,
      "pais": "Colombia",
      "codigoPostal": "055420",
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaModificacion": null,
      "creadoPorId": null,
      "modificadoPorId": null
    }
  ]
}
```

#### 3.4.3. Obtener Ciudad por ID

Obtiene una ciudad específica por su ID.

Acceso: `api/Ciudad/{id}`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.4.3.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la ciudad | Integer | Sí |

##### 3.4.3.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la ciudad | Object |
| resultado.id | Identificador único de la ciudad | Integer |
| resultado.nombre | Nombre de la ciudad | String |
| resultado.departamentoId | ID del departamento al que pertenece | Integer |
| resultado.departamento | Nombre del departamento al que pertenece | String |
| resultado.paisId | ID del país al que pertenece | Integer |
| resultado.pais | Nombre del país al que pertenece | String |
| resultado.codigoPostal | Código postal de la ciudad | String |
| resultado.activo | Estado de activación de la ciudad | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de última modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.creadoPor | Nombre del usuario que creó el registro | String |
| resultado.modificadoPor | Nombre del usuario que modificó el registro | String |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Ciudad obtenida",
  "detalle": "Se ha obtenido la ciudad 'Medellín'",
  "resultado": {
    "id": 1,
    "nombre": "Medellín",
    "departamentoId": 1,
    "departamento": "Antioquia",
    "paisId": 1,
    "pais": "Colombia",
    "codigoPostal": "050001",
    "activo": true,
    "fechaCreacion": "2023-01-01T00:00:00",
    "fechaModificacion": null,
    "creadoPorId": null,
    "modificadoPorId": null,
    "creadoPor": null,
    "modificadoPor": null
  }
}
```

#### 3.4.4. Obtener Ciudades Paginadas

Obtiene una lista paginada de ciudades con opciones de filtrado.

Acceso: `api/Ciudad/paginado`  
Formato: JSON  
Servicio: REST / GET  
Autenticación: JWT requerido

##### 3.4.4.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** | **Valor Predeterminado** |
|------------|-----------------|----------|---------------|--------------------------|
| pagina | Número de página | Integer | No | 1 |
| elementosPorPagina | Cantidad de elementos por página | Integer | No | 10 |
| departamentoId | ID del departamento para filtrar | Integer | No | null |
| busqueda | Texto para filtrar resultados | String | No | null |

##### 3.4.4.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con resultados paginados | Object |
| resultado.pagina | Número de página actual | Integer |
| resultado.elementosPorPagina | Cantidad de elementos por página | Integer |
| resultado.totalPaginas | Total de páginas disponibles | Integer |
| resultado.totalRegistros | Total de registros encontrados | Integer |
| resultado.lista | Lista de ciudades | Array |
| resultado.lista[].id | Identificador único de la ciudad | Integer |
| resultado.lista[].nombre | Nombre de la ciudad | String |
| resultado.lista[].departamentoId | ID del departamento al que pertenece | Integer |
| resultado.lista[].departamento | Nombre del departamento al que pertenece | String |
| resultado.lista[].paisId | ID del país al que pertenece | Integer |
| resultado.lista[].pais | Nombre del país al que pertenece | String |
| resultado.lista[].codigoPostal | Código postal de la ciudad | String |
| resultado.lista[].activo | Estado de activación de la ciudad | Boolean |
| resultado.lista[].fechaCreacion | Fecha de creación | DateTime |
| resultado.lista[].fechaModificacion | Fecha de última modificación | DateTime |
| resultado.lista[].creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.lista[].modificadoPorId | ID del usuario que modificó el registro | GUID |
| resultado.lista[].creadoPor | Nombre del usuario que creó el registro | String |
| resultado.lista[].modificadoPor | Nombre del usuario que modificó el registro | String |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Ciudades obtenidas",
  "detalle": "Se han obtenido 6 ciudades de un total de 6",
  "resultado": {
    "pagina": 1,
    "elementosPorPagina": 10,
    "totalPaginas": 1,
    "totalRegistros": 6,
    "lista": [
      {
        "id": 1,
        "nombre": "Medellín",
        "departamentoId": 1,
        "departamento": "Antioquia",
        "paisId": 1,
        "pais": "Colombia",
        "codigoPostal": "050001",
        "activo": true,
        "fechaCreacion": "2023-01-01T00:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null
      },
      {
        "id": 2,
        "nombre": "Envigado",
        "departamentoId": 1,
        "departamento": "Antioquia",
        "paisId": 1,
        "pais": "Colombia",
        "codigoPostal": "055420",
        "activo": true,
        "fechaCreacion": "2023-01-01T00:00:00",
        "fechaModificacion": null,
        "creadoPorId": null,
        "modificadoPorId": null,
        "creadoPor": null,
        "modificadoPor": null
      }
    ]
  }
}
```

#### 3.4.5. Crear Ciudad

Crea una nueva ciudad en el sistema.

Acceso: `api/Ciudad`  
Formato: JSON  
Servicio: REST / POST  
Autenticación: JWT requerido

##### 3.4.5.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombre | Nombre de la ciudad | String | Sí |
| departamentoId | ID del departamento al que pertenece | Integer | Sí |
| codigoPostal | Código postal de la ciudad | String | No |
| activo | Estado de activación de la ciudad | Boolean | No |

Ejemplo de entrada:
```json
{
  "nombre": "Bucaramanga",
  "departamentoId": 5,
  "codigoPostal": "680001",
  "activo": true
}
```

##### 3.4.5.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la ciudad creada | Object |
| resultado.id | Identificador único de la ciudad | Integer |
| resultado.nombre | Nombre de la ciudad | String |
| resultado.departamentoId | ID del departamento al que pertenece | Integer |
| resultado.departamento | Nombre del departamento al que pertenece | String |
| resultado.paisId | ID del país al que pertenece | Integer |
| resultado.pais | Nombre del país al que pertenece | String |
| resultado.codigoPostal | Código postal de la ciudad | String |
| resultado.activo | Estado de activación de la ciudad | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Ciudad creada",
  "detalle": "La ciudad 'Bucaramanga' ha sido creada correctamente",
  "resultado": {
    "id": 7,
    "nombre": "Bucaramanga",
    "departamentoId": 5,
    "departamento": "Santander",
    "paisId": 1,
    "pais": "Colombia",
    "codigoPostal": "680001",
    "activo": true,
    "fechaCreacion": "2024-01-17T15:30:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.4.6. Actualizar Ciudad

Actualiza una ciudad existente.

Acceso: `api/Ciudad/{id}`  
Formato: JSON  
Servicio: REST / PUT  
Autenticación: JWT requerido

##### 3.4.6.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la ciudad | Integer | Sí |

##### 3.4.6.2. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la ciudad | Integer | No |
| nombre | Nombre de la ciudad | String | Sí |
| departamentoId | ID del departamento al que pertenece | Integer | Sí |
| codigoPostal | Código postal de la ciudad | String | No |
| activo | Estado de activación de la ciudad | Boolean | No |

Ejemplo de entrada:
```json
{
  "nombre": "Bucaramanga",
  "departamentoId": 5,
  "codigoPostal": "680001",
  "activo": true
}
```

##### 3.4.6.3. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Información de la ciudad actualizada | Object |
| resultado.id | Identificador único de la ciudad | Integer |
| resultado.nombre | Nombre de la ciudad | String |
| resultado.departamentoId | ID del departamento al que pertenece | Integer |
| resultado.departamento | Nombre del departamento al que pertenece | String |
| resultado.paisId | ID del país al que pertenece | Integer |
| resultado.pais | Nombre del país al que pertenece | String |
| resultado.codigoPostal | Código postal de la ciudad | String |
| resultado.activo | Estado de activación de la ciudad | Boolean |
| resultado.fechaCreacion | Fecha de creación | DateTime |
| resultado.fechaModificacion | Fecha de modificación | DateTime |
| resultado.creadoPorId | ID del usuario que creó el registro | GUID |
| resultado.modificadoPorId | ID del usuario que modificó el registro | GUID |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Ciudad actualizada",
  "detalle": "La ciudad 'Bucaramanga' ha sido actualizada correctamente",
  "resultado": {
    "id": 7,
    "nombre": "Bucaramanga",
    "departamentoId": 5,
    "departamento": "Santander",
    "paisId": 1,
    "pais": "Colombia",
    "codigoPostal": "680001",
    "activo": true,
    "fechaCreacion": "2024-01-17T15:30:00",
    "fechaModificacion": "2024-01-17T16:45:00",
    "creadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "modificadoPorId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

#### 3.4.7. Eliminar Ciudad

Elimina (desactiva) una ciudad existente.

Acceso: `api/Ciudad/{id}`  
Formato: JSON  
Servicio: REST / DELETE  
Autenticación: JWT requerido

##### 3.4.7.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID de la ciudad | Integer | Sí |

##### 3.4.7.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto de resultado (nulo en este caso) | Object |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Ciudad eliminada",
  "detalle": "La ciudad 'Bucaramanga' ha sido eliminada correctamente",
  "resultado": null
}
```

## 4. CONSIDERACIONES TÉCNICAS

### 4.1. Configuración

El sistema requiere la siguiente configuración en el archivo appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SERVER;Initial Catalog=GeoTrack;Persist Security Info=True;User ID=SA;Password=YOUR_PASSWORD;Trust Server Certificate=True;Connect Timeout=3600"
  },
  "JwtSettings": {
    "Key": "YOUR_SECRET_KEY",
    "Issuer": "GeoTrack.WebApi",
    "Audience": "GeoTrack.Client",
    "TiempoExpiracionMinutos": 30,
    "TiempoExpiracionBDMinutos": 60
  },
  "AllowedHosts": "*"
}
```

### 4.2. Arquitectura del Proyecto

#### 4.2.1. Estructura de Capas

El proyecto sigue una arquitectura en capas:

- **Controllers**: Contiene los controladores API que manejan las solicitudes HTTP.
- **Domain**:
  - **Contracts**: Interfaces que definen las operaciones de los repositorios.
  - **Services**: Implementaciones concretas de los repositorios.
- **Infrastructure**: Modelos de Entity Framework y configuración de la base de datos.
- **Shared**: DTOs y modelos compartidos entre capas.
- **Util**: Clases de utilidad y extensiones.
- **Attributes**: Atributos y middleware personalizados.

#### 4.2.2. Patrones Implementados

- **Repository Pattern**: Abstracción de acceso a datos a través de interfaces.
- **Dependency Injection**: Inyección de dependencias para facilitar pruebas y modularidad.
- **Data Transfer Objects (DTO)**: Objetos para transferir datos entre capas.
- **Unit of Work**: Gestión de transacciones a través de Entity Framework.
- **Middleware Pipeline**: Procesamiento de solicitudes HTTP a través de componentes middleware.

### 4.3. Dependencias

El sistema requiere las siguientes dependencias principales:

#### 4.3.1. Paquetes NuGet

- **Microsoft.AspNetCore.Authentication.JwtBearer**: Para autenticación JWT.
- **Microsoft.EntityFrameworkCore.SqlServer**: Para acceso a base de datos SQL Server.
- **Microsoft.EntityFrameworkCore.Tools**: Herramientas de EF Core para migraciones.
- **Swashbuckle.AspNetCore**: Generación de documentación OpenAPI/Swagger.
- **System.IdentityModel.Tokens.Jwt**: Manipulación de tokens JWT.

### 4.4. Seguridad

#### 4.4.1. Autenticación y Autorización

- **JWT (JSON Web Tokens)**: Sistema de autenticación basado en tokens firmados.
- **Validación de IP**: Verificación de IP en cada solicitud para prevenir robo de sesión.
- **Tiempo de expiración configurable**: Tokens con tiempo de vida limitado.
- **Almacenamiento seguro de tokens**: Registro de tokens activos y expirados.
- **Hashing de contraseñas**: Almacenamiento seguro de credenciales usando SHA-256.

#### 4.4.2. Middleware de Seguridad

- **Manejo de excepciones**: Middleware para capturar y registrar errores.
- **Logging**: Registro de actividades y solicitudes.
- **Validación de modelos**: Verificación de datos de entrada.
- **CORS configurado**: Control de acceso desde orígenes externos.

### 4.5. Logging y Auditoría

#### 4.5.1. Sistema de Logs

- **Logging en base de datos**: Registro de actividades en tabla de logs.
- **Logging en archivo**: Registro detallado usando Serilog.
- **Niveles de log**: Diferentes niveles según la severidad (INFO, ERROR, etc.).
- **Rastreo de auditoría**: Registro de quién modificó cada registro y cuándo.

#### 4.5.2. Tipos de Logs

- **200**: Acciones exitosas (creación, actualización, eliminación).
- **400**: Información general y advertencias.
- **500**: Errores y excepciones.

### 4.6. Manejo de Errores

#### 4.6.1. Middleware de Excepciones

El sistema implementa un middleware para capturar todas las excepciones no manejadas y devolver respuestas estandarizadas:

```csharp
app.UseMiddleware<ErrorHandlingMiddleware>();
```

#### 4.6.2. Respuestas Estandarizadas

Todas las respuestas, incluyendo errores, siguen el formato estándar `RespuestaDto`:

```json
{
  "exito": false,
  "mensaje": "Error de servidor",
  "detalle": "Se ha producido un error al procesar la solicitud",
  "resultado": null
}
```

#### 4.6.3. Códigos de Respuesta HTTP

- **200 OK**: Operación exitosa.
- **400 Bad Request**: Error de validación o solicitud incorrecta.
- **401 Unauthorized**: Error de autenticación.
- **404 Not Found**: Recurso no encontrado.
- **500 Internal Server Error**: Error interno del servidor.

### 4.7. Validación de Datos

#### 4.7.1. Atributos de Validación

Se utilizan atributos de validación de datos en los DTOs:

```csharp
[Required(ErrorMessage = "El nombre del país es requerido")]
[StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
public string Nombre { get; set; } = null!;
```

#### 4.7.2. Validación Personalizada

Se implementan validaciones adicionales en los repositorios:

```csharp
// Validar que no exista un país con el mismo nombre
if (await ExistePorNombreAsync(paisDto.Nombre))
{
    return RespuestaDto.ParametrosIncorrectos(
        "Creación fallida",
        $"Ya existe un país con el nombre '{paisDto.Nombre}'");
}
```

### 4.8. Mejores Prácticas

#### 4.8.1. Uso de Entity Framework Core

- **AsNoTracking()**: Para consultas de solo lectura.
- **Include()**: Para cargar entidades relacionadas eficientemente.
- **Paginación**: Evitando cargar grandes conjuntos de datos.

#### 4.8.2. Seguridad

- **Sanitización de entradas**: Validación y limpieza de datos de entrada.
- **Parámetros preparados**: Prevención de inyección SQL.
- **HTTPS**: Aseguramiento de comunicaciones con certificados SSL/TLS.

#### 4.8.3. Rendimiento

- **Caché**: Implementación de caché para datos frecuentemente accedidos.
- **Paginación**: División de resultados en páginas para optimizar rendimiento.
- **Consultas optimizadas**: Uso de proyecciones y filtros eficientes.

### 4.9. Documentación

#### 4.9.1. Swagger / OpenAPI

El sistema implementa documentación automática con Swagger, accesible en:

```
https://tu-servidor/swagger
```

#### 4.9.2. Comentarios XML

El código incluye comentarios XML para generar documentación automática:

```csharp
/// <summary>
/// Obtiene todas las ciudades activas
/// </summary>
public async Task<List<CiudadDto>> ObtenerTodosAsync()
{
    _logger.LogInformation("Obteniendo todas las ciudades");
    // ...
}
```

## 5. ANEXOS

### 5.1. Diagrama de Entidad-Relación

La base de datos del sistema sigue el siguiente esquema de relaciones:

- **Usuarios** tienen **Roles** (many-to-one).
- **Usuarios** pueden generar **Tokens** (one-to-many).
- **Usuarios** pueden realizar acciones registradas en **Logs** (one-to-many).
- **Países** contienen **Departamentos** (one-to-many).
- **Departamentos** contienen **Ciudades** (one-to-many).
- **Usuarios** pueden ser creadores o modificadores de **Países**, **Departamentos** y **Ciudades** (one-to-many).

### 5.2. Ejemplos de Uso

#### 5.2.1. Flujo de Autenticación

1. Llamar a `POST /api/Auth/login` con credenciales.
2. Obtener el token JWT de la respuesta.
3. Incluir el token en el header `Authorization: Bearer {token}` en las siguientes solicitudes.
4. Incluir el ID de usuario en el header `IdUsuario` en las siguientes solicitudes.
5. Al finalizar, llamar a `POST /api/Auth/logout` para invalidar el token.

#### 5.2.2. Gestión de Ubicaciones

1. Crear un país con `POST /api/Pais`.
2. Crear departamentos para el país con `POST /api/Departamento`.
3. Crear ciudades para los departamentos con `POST /api/Ciudad`.
4. Consultar la jerarquía completa con las solicitudes GET correspondientes.