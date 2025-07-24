# API Sistema Escolar

Una API completa para la gestión de un sistema escolar, desarrollada con .NET 8, Entity Framework Core y PostgreSQL.

## 🚀 Características

### Módulos Principales

#### 🔐 Autenticación y Autorización
- Login/Logout con JWT
- Gestión de roles y permisos
- Cambio de contraseña
- Perfiles de usuario

#### 👥 Gestión de Usuarios
- **Administradores**: Gestión completa del sistema
- **Docentes**: Gestión académica y calificaciones
- **Estudiantes**: Registro y seguimiento académico
- **Padres/Tutores**: Acceso a información de sus hijos

#### 📚 Gestión Académica
- **Cursos/Grados/Secciones**: Organización por niveles
- **Asignaturas**: Registro de materias y docentes
- **Horarios**: Programación de clases
- **Matrícula**: Gestión de inscripciones
- **Calificaciones**: Registro de notas y promedios
- **Evaluaciones**: Tareas, exámenes y proyectos
- **Asistencia**: Control diario de presencia

#### 📅 Calendario y Eventos
- Eventos académicos
- Feriados y fechas importantes
- Fechas de exámenes

#### 📊 Reportes
- Boletines de notas
- Estadísticas de rendimiento
- Reportes de asistencia

#### 💬 Comunicación
- Notificaciones internas
- Mensajes entre usuarios
- Anuncios escolares

## 🛠️ Tecnologías

- **.NET 8**
- **Entity Framework Core**
- **PostgreSQL**
- **JWT Authentication**
- **Swagger/OpenAPI**
- **Docker**

## 📋 Requisitos

- .NET 8 SDK
- PostgreSQL
- Docker (opcional)

## 🚀 Instalación

### 1. Clonar el repositorio
```bash
git clone <repository-url>
cd api-school-system
```

### 2. Configurar la base de datos
Editar `appsettings.json` con tu cadena de conexión:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=school_db;Username=your_user;Password=your_password;"
  }
}
```

### 3. Ejecutar migraciones
```bash
dotnet ef database update
```

### 4. Ejecutar la aplicación
```bash
dotnet run
```

### 5. Acceder a Swagger
```
https://localhost:7001/swagger
```

## 🐳 Docker

### Construir imagen
```bash
docker build -t school-api .
```

### Ejecutar con Docker Compose
```bash
docker-compose up --build
```

## 📚 Endpoints Principales

### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/change-password` - Cambiar contraseña
- `GET /api/auth/profile` - Obtener perfil
- `PUT /api/auth/profile` - Actualizar perfil

### Usuarios
- `GET /api/users` - Listar usuarios
- `GET /api/users/{id}` - Obtener usuario
- `PUT /api/users/{id}` - Actualizar usuario
- `DELETE /api/users/{id}` - Desactivar usuario
- `GET /api/users/stats` - Estadísticas de usuarios

### Estudiantes
- `GET /api/students` - Listar estudiantes
- `GET /api/students/{id}` - Obtener estudiante
- `POST /api/students` - Crear estudiante
- `PUT /api/students/{id}` - Actualizar estudiante
- `DELETE /api/students/{id}` - Desactivar estudiante

### Docentes
- `GET /api/teachers` - Listar docentes
- `GET /api/teachers/{id}` - Obtener docente
- `POST /api/teachers` - Crear docente
- `PUT /api/teachers/{id}` - Actualizar docente
- `DELETE /api/teachers/{id}` - Desactivar docente
- `GET /api/teachers/{id}/workload` - Carga académica

## 🔐 Roles y Permisos

### Admin
- Acceso completo al sistema
- Gestión de usuarios
- Configuración del sistema

### Teacher
- Gestión de sus cursos
- Calificaciones de estudiantes
- Control de asistencia
- Horarios de clases

### Student
- Ver sus calificaciones
- Ver su horario
- Ver su asistencia

### Parent
- Ver calificaciones de sus hijos
- Ver asistencia de sus hijos
- Comunicación con docentes

## 📊 Estructura de la Base de Datos

### Entidades Principales
- **Users**: Usuarios del sistema
- **Students**: Información específica de estudiantes
- **Teachers**: Información específica de docentes
- **Parents**: Información de padres/tutores
- **Grades**: Grados académicos
- **Courses**: Cursos/secciones
- **Subjects**: Asignaturas
- **Enrollments**: Matrículas
- **Evaluations**: Evaluaciones
- **StudentGrades**: Calificaciones
- **Attendance**: Asistencia
- **Schedules**: Horarios
- **AcademicPeriods**: Períodos académicos
- **AcademicEvents**: Eventos del calendario
- **Notifications**: Notificaciones

## 🔧 Configuración

### Variables de Entorno
- `ConnectionStrings__DefaultConnection`: Cadena de conexión a PostgreSQL
- `Jwt__Key`: Clave secreta para JWT

### Configuración de Swagger
La documentación de la API está disponible en `/swagger` cuando la aplicación está en ejecución.

## 📝 Ejemplos de Uso

### Login
```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

### Crear Estudiante
```bash
curl -X POST "https://localhost:7001/api/students" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan.perez@email.com",
    "username": "juan.perez",
    "password": "password123",
    "role": "Student"
  }'
```

## 🚀 Despliegue en Render

1. Conectar el repositorio a Render
2. Crear un nuevo Web Service
3. Seleccionar "Docker" como entorno
4. Configurar variables de entorno
5. Desplegar

## 🤝 Contribución

1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.

## 📞 Soporte

Para soporte, email: support@schoolsystem.com
