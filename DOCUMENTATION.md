# Documentación Completa - API Sistema Escolar

## 📋 Resumen Ejecutivo

Se ha implementado una API completa para un sistema escolar que incluye todos los módulos solicitados. La aplicación está construida con .NET 8, Entity Framework Core y PostgreSQL, con soporte completo para Docker y despliegue en Render.

## 🏗️ Arquitectura del Sistema

### Estructura de Capas
```
api-school-system/
├── Controllers/          # Controladores de la API
├── Models/              # Modelos de datos
├── Dtos/                # Objetos de transferencia de datos
├── Data/                # Contexto de base de datos
├── Helpers/             # Utilidades y helpers
├── Migrations/          # Migraciones de Entity Framework
└── Properties/          # Configuración del proyecto
```

## 📊 Modelos de Datos Implementados

### 1. Gestión de Usuarios
- **User**: Usuario base del sistema con roles y permisos
- **Permission**: Permisos del sistema
- **UserPermission**: Relación muchos a muchos entre usuarios y permisos

### 2. Entidades Académicas
- **Grade**: Grados académicos (1°, 2°, 3°, etc.)
- **Course**: Cursos/secciones específicas
- **Subject**: Asignaturas o materias
- **AcademicPeriod**: Períodos académicos (semestres, trimestres)
- **AcademicEvent**: Eventos del calendario académico

### 3. Personas del Sistema
- **Student**: Información específica de estudiantes
- **Teacher**: Información específica de docentes
- **Parent**: Información de padres/tutores
- **StudentParent**: Relación entre estudiantes y padres

### 4. Relaciones Académicas
- **CourseTeacher**: Asignación de docentes a cursos
- **SubjectTeacher**: Asignación de docentes a asignaturas
- **Enrollment**: Matrículas de estudiantes

### 5. Registros Académicos
- **Evaluation**: Evaluaciones (exámenes, tareas, proyectos)
- **StudentGrade**: Calificaciones de estudiantes
- **Attendance**: Control de asistencia
- **Schedule**: Horarios de clases

### 6. Comunicación
- **Notification**: Sistema de notificaciones internas

## 🔐 Sistema de Autenticación y Autorización

### Roles Implementados
1. **Admin**: Acceso completo al sistema
2. **Teacher**: Gestión académica y calificaciones
3. **Student**: Acceso a información personal
4. **Parent**: Acceso a información de hijos
5. **Tutor**: Similar a Parent

### JWT Implementation
- Tokens con expiración de 24 horas
- Claims personalizados para información del usuario
- Validación de roles en controladores

## 🎯 Controladores Implementados

### 1. AuthController
- **POST /api/auth/login**: Autenticación de usuarios
- **POST /api/auth/register**: Registro de nuevos usuarios
- **POST /api/auth/change-password**: Cambio de contraseña
- **GET /api/auth/profile**: Obtener perfil del usuario
- **PUT /api/auth/profile**: Actualizar perfil

### 2. UsersController
- **GET /api/users**: Listar usuarios con filtros
- **GET /api/users/{id}**: Obtener usuario específico
- **PUT /api/users/{id}**: Actualizar usuario
- **DELETE /api/users/{id}**: Desactivar usuario
- **GET /api/users/stats**: Estadísticas de usuarios

### 3. StudentsController
- **GET /api/students**: Listar estudiantes con filtros
- **GET /api/students/{id}**: Obtener estudiante con detalles completos
- **POST /api/students**: Crear nuevo estudiante
- **PUT /api/students/{id}**: Actualizar estudiante
- **DELETE /api/students/{id}**: Desactivar estudiante

### 4. TeachersController
- **GET /api/teachers**: Listar docentes con filtros
- **GET /api/teachers/{id}**: Obtener docente con detalles completos
- **POST /api/teachers**: Crear nuevo docente
- **PUT /api/teachers/{id}**: Actualizar docente
- **DELETE /api/teachers/{id}**: Desactivar docente
- **GET /api/teachers/{id}/workload**: Carga académica del docente

### 5. GradesController
- **GET /api/grades**: Listar grados académicos
- **GET /api/grades/{id}**: Obtener grado específico
- **POST /api/grades**: Crear nuevo grado
- **PUT /api/grades/{id}**: Actualizar grado
- **DELETE /api/grades/{id}**: Desactivar grado
- **GET /api/grades/stats**: Estadísticas de grados

### 6. CoursesController
- **GET /api/courses**: Listar cursos con filtros
- **GET /api/courses/{id}**: Obtener curso específico
- **POST /api/courses**: Crear nuevo curso
- **PUT /api/courses/{id}**: Actualizar curso
- **DELETE /api/courses/{id}**: Desactivar curso
- **POST /api/courses/{id}/assign-teacher**: Asignar docente al curso
- **DELETE /api/courses/{id}/remove-teacher/{teacherId}**: Remover docente del curso
- **GET /api/courses/stats**: Estadísticas de cursos

### 7. SubjectsController
- **GET /api/subjects**: Listar asignaturas con filtros
- **GET /api/subjects/{id}**: Obtener asignatura específica
- **POST /api/subjects**: Crear nueva asignatura
- **PUT /api/subjects/{id}**: Actualizar asignatura
- **DELETE /api/subjects/{id}**: Desactivar asignatura
- **POST /api/subjects/{id}/assign-teacher**: Asignar docente a la asignatura
- **DELETE /api/subjects/{id}/remove-teacher/{teacherId}**: Remover docente de la asignatura
- **GET /api/subjects/departments**: Obtener departamentos
- **GET /api/subjects/stats**: Estadísticas de asignaturas

### 8. EnrollmentsController
- **GET /api/enrollments**: Listar matrículas con filtros
- **GET /api/enrollments/{id}**: Obtener matrícula específica
- **POST /api/enrollments**: Crear nueva matrícula
- **PUT /api/enrollments/{id}**: Actualizar matrícula
- **DELETE /api/enrollments/{id}**: Cancelar matrícula
- **GET /api/enrollments/student/{studentId}**: Matrículas de un estudiante
- **GET /api/enrollments/course/{courseId}**: Matrículas de un curso
- **GET /api/enrollments/stats**: Estadísticas de matrículas

### 9. EvaluationsController
- **GET /api/evaluations**: Listar evaluaciones con filtros
- **GET /api/evaluations/{id}**: Obtener evaluación específica
- **POST /api/evaluations**: Crear nueva evaluación
- **PUT /api/evaluations/{id}**: Actualizar evaluación
- **DELETE /api/evaluations/{id}**: Desactivar evaluación
- **POST /api/evaluations/{id}/register-grade**: Registrar calificación
- **GET /api/evaluations/{id}/grades**: Calificaciones de una evaluación
- **GET /api/evaluations/subject/{subjectId}**: Evaluaciones de una asignatura
- **GET /api/evaluations/stats**: Estadísticas de evaluaciones

### 10. SchedulesController
- **GET /api/schedules**: Listar horarios con filtros
- **GET /api/schedules/{id}**: Obtener horario específico
- **POST /api/schedules**: Crear nuevo horario
- **PUT /api/schedules/{id}**: Actualizar horario
- **DELETE /api/schedules/{id}**: Desactivar horario
- **GET /api/schedules/teacher/{teacherId}**: Horario de un docente
- **GET /api/schedules/course/{courseId}**: Horario de un curso
- **GET /api/schedules/stats**: Estadísticas de horarios

### 11. AttendanceController
- **GET /api/attendance**: Listar registros de asistencia con filtros
- **GET /api/attendance/{id}**: Obtener registro específico
- **POST /api/attendance**: Crear registro de asistencia
- **POST /api/attendance/bulk**: Registro masivo de asistencia
- **PUT /api/attendance/{id}**: Actualizar registro
- **DELETE /api/attendance/{id}**: Desactivar registro
- **GET /api/attendance/student/{studentId}**: Asistencia de un estudiante
- **GET /api/attendance/course/{courseId}**: Asistencia de un curso
- **GET /api/attendance/report**: Reporte de asistencia
- **GET /api/attendance/stats**: Estadísticas de asistencia

### 12. AcademicPeriodsController
- **GET /api/academicperiods**: Listar períodos académicos
- **GET /api/academicperiods/{id}**: Obtener período específico
- **POST /api/academicperiods**: Crear nuevo período
- **PUT /api/academicperiods/{id}**: Actualizar período
- **DELETE /api/academicperiods/{id}**: Desactivar período
- **GET /api/academicperiods/current**: Período académico actual
- **GET /api/academicperiods/upcoming**: Períodos próximos
- **GET /api/academicperiods/active**: Períodos activos
- **GET /api/academicperiods/{id}/enrollments**: Matrículas del período
- **GET /api/academicperiods/{id}/evaluations**: Evaluaciones del período
- **GET /api/academicperiods/{id}/events**: Eventos del período
- **GET /api/academicperiods/stats**: Estadísticas de períodos

### 13. AcademicEventsController
- **GET /api/academicevents**: Listar eventos académicos
- **GET /api/academicevents/{id}**: Obtener evento específico
- **POST /api/academicevents**: Crear nuevo evento
- **PUT /api/academicevents/{id}**: Actualizar evento
- **DELETE /api/academicevents/{id}**: Desactivar evento
- **GET /api/academicevents/calendar**: Eventos para calendario
- **GET /api/academicevents/upcoming**: Eventos próximos
- **GET /api/academicevents/today**: Eventos de hoy
- **GET /api/academicevents/period/{academicPeriodId}**: Eventos de un período
- **GET /api/academicevents/type/{type}**: Eventos por tipo
- **GET /api/academicevents/stats**: Estadísticas de eventos

### 14. NotificationsController
- **GET /api/notifications**: Listar notificaciones (filtrado por usuario)
- **GET /api/notifications/{id}**: Obtener notificación específica
- **POST /api/notifications**: Crear nueva notificación
- **POST /api/notifications/bulk**: Crear notificaciones masivas
- **PUT /api/notifications/{id}**: Actualizar notificación
- **DELETE /api/notifications/{id}**: Desactivar notificación
- **PUT /api/notifications/{id}/read**: Marcar como leída
- **PUT /api/notifications/read-all**: Marcar todas como leídas
- **GET /api/notifications/unread**: Notificaciones no leídas
- **GET /api/notifications/unread-count**: Contador de no leídas
- **GET /api/notifications/my**: Mis notificaciones con paginación
- **GET /api/notifications/sent**: Notificaciones enviadas
- **GET /api/notifications/stats**: Estadísticas de notificaciones

### 15. ParentsController
- **GET /api/parents**: Listar padres/tutores con filtros
- **GET /api/parents/{id}**: Obtener padre específico
- **POST /api/parents**: Crear nuevo padre/tutor
- **PUT /api/parents/{id}**: Actualizar padre/tutor
- **DELETE /api/parents/{id}**: Desactivar padre/tutor
- **GET /api/parents/student/{studentId}**: Padres de un estudiante
- **POST /api/parents/student/{studentId}**: Asignar padre a estudiante
- **PUT /api/parents/student/{studentParentId}**: Actualizar relación padre-estudiante
- **DELETE /api/parents/student/{studentParentId}**: Remover padre de estudiante
- **GET /api/parents/emergency-contacts**: Contactos de emergencia
- **GET /api/parents/stats**: Estadísticas de padres

### 16. PermissionsController
- **GET /api/permissions**: Listar permisos con filtros
- **GET /api/permissions/{id}**: Obtener permiso específico
- **POST /api/permissions**: Crear nuevo permiso
- **PUT /api/permissions/{id}**: Actualizar permiso
- **DELETE /api/permissions/{id}**: Desactivar permiso
- **GET /api/permissions/modules**: Obtener módulos
- **GET /api/permissions/actions**: Obtener acciones
- **GET /api/permissions/module/{module}**: Permisos por módulo
- **POST /api/permissions/assign**: Asignar permiso a usuario
- **DELETE /api/permissions/revoke/{userPermissionId}**: Revocar permiso
- **GET /api/permissions/user/{userId}**: Permisos de un usuario
- **POST /api/permissions/bulk-assign**: Asignación masiva de permisos
- **GET /api/permissions/stats**: Estadísticas de permisos

### 17. ReportsController
- **GET /api/reports/student-performance**: Reporte de rendimiento estudiantil
- **GET /api/reports/attendance-summary**: Reporte resumen de asistencia
- **GET /api/reports/enrollment-statistics**: Estadísticas de matrículas
- **GET /api/reports/teacher-workload**: Reporte de carga académica docente
- **GET /api/reports/academic-progress**: Reporte de progreso académico
- **GET /api/reports/dashboard-summary**: Resumen del dashboard

### 18. SystemParametersController
- **GET /api/systemparameters**: Listar parámetros del sistema
- **GET /api/systemparameters/{id}**: Obtener parámetro específico
- **POST /api/systemparameters**: Crear nuevo parámetro
- **PUT /api/systemparameters/{id}**: Actualizar parámetro
- **DELETE /api/systemparameters/{id}**: Desactivar parámetro
- **GET /api/systemparameters/category/{category}**: Parámetros por categoría
- **GET /api/systemparameters/categories**: Obtener categorías
- **GET /api/systemparameters/value/{name}**: Obtener valor de parámetro
- **PUT /api/systemparameters/value/{name}**: Actualizar valor de parámetro
- **POST /api/systemparameters/bulk-update**: Actualización masiva
- **GET /api/systemparameters/export**: Exportar parámetros
- **POST /api/systemparameters/import**: Importar parámetros
- **GET /api/systemparameters/stats**: Estadísticas de parámetros

## 📋 DTOs Implementados

### Autenticación
- **LoginDto**: Credenciales de inicio de sesión
- **RegisterDto**: Datos para registro de usuarios
- **ChangePasswordDto**: Cambio de contraseña
- **UpdateProfileDto**: Actualización de perfil

### Usuarios
- **UpdateUserDto**: Actualización de datos de usuario

### Estudiantes
- **CreateStudentDto**: Creación de estudiantes
- **UpdateStudentDto**: Actualización de estudiantes

### Docentes
- **CreateTeacherDto**: Creación de docentes
- **UpdateTeacherDto**: Actualización de docentes

### Grados Académicos
- **CreateGradeDto**: Creación de grados
- **UpdateGradeDto**: Actualización de grados

### Cursos
- **CreateCourseDto**: Creación de cursos
- **UpdateCourseDto**: Actualización de cursos
- **AssignTeacherDto**: Asignación de docentes a cursos

### Asignaturas
- **CreateSubjectDto**: Creación de asignaturas
- **UpdateSubjectDto**: Actualización de asignaturas
- **AssignSubjectTeacherDto**: Asignación de docentes a asignaturas

### Matrículas
- **CreateEnrollmentDto**: Creación de matrículas
- **UpdateEnrollmentDto**: Actualización de matrículas

### Evaluaciones
- **CreateEvaluationDto**: Creación de evaluaciones
- **UpdateEvaluationDto**: Actualización de evaluaciones
- **RegisterGradeDto**: Registro de calificaciones

### Horarios
- **CreateScheduleDto**: Creación de horarios
- **UpdateScheduleDto**: Actualización de horarios

### Asistencia
- **CreateAttendanceDto**: Creación de registros de asistencia
- **CreateBulkAttendanceDto**: Creación masiva de asistencia
- **BulkAttendanceRecord**: Registro individual en creación masiva
- **UpdateAttendanceDto**: Actualización de asistencia

### Períodos Académicos
- **CreateAcademicPeriodDto**: Creación de períodos académicos
- **UpdateAcademicPeriodDto**: Actualización de períodos académicos

### Eventos Académicos
- **CreateAcademicEventDto**: Creación de eventos académicos
- **UpdateAcademicEventDto**: Actualización de eventos académicos

### Notificaciones
- **CreateNotificationDto**: Creación de notificaciones
- **CreateBulkNotificationDto**: Creación masiva de notificaciones
- **UpdateNotificationDto**: Actualización de notificaciones

### Padres/Tutores
- **CreateParentDto**: Creación de padres/tutores
- **UpdateParentDto**: Actualización de padres/tutores
- **AssignParentDto**: Asignación de padres a estudiantes
- **UpdateStudentParentDto**: Actualización de relación estudiante-padre

### Permisos
- **CreatePermissionDto**: Creación de permisos
- **UpdatePermissionDto**: Actualización de permisos
- **AssignPermissionDto**: Asignación de permisos a usuarios
- **BulkAssignPermissionDto**: Asignación masiva de permisos

### Parámetros del Sistema
- **CreateSystemParameterDto**: Creación de parámetros del sistema
- **UpdateSystemParameterDto**: Actualización de parámetros del sistema
- **UpdateParameterValueDto**: Actualización de valor de parámetro
- **BulkUpdateParametersDto**: Actualización masiva de parámetros
- **ParameterUpdate**: Parámetro individual en actualización masiva
- **ImportParametersDto**: Importación de parámetros
- **ImportParameter**: Parámetro individual en importación

Todos los DTOs incluyen validaciones con Data Annotations para asegurar la integridad de los datos.

## 🗄️ Configuración de Base de Datos

### Entity Framework Configuration
- Configuración de relaciones uno a uno y muchos a muchos
- Índices únicos en campos críticos
- Datos de semilla para usuario administrador
- Configuración de eliminación en cascada apropiada

### Migraciones
- Migración inicial: `CompleteSchoolSystem`
- Incluye todas las tablas y relaciones
- Datos de semilla para administrador

## 🐳 Configuración Docker

### Dockerfile
- Multi-stage build para optimización
- .NET 8 runtime
- Configuración para producción
- Variables de entorno configuradas

### Docker Compose
- Configuración para desarrollo local
- Variables de entorno para base de datos
- Puerto 8080 expuesto

## 🔧 Configuración de Swagger

### Habilitado en Producción
- Documentación completa de la API
- Interfaz interactiva para pruebas
- Redirección automática desde la raíz

## 📊 Funcionalidades por Módulo

### ✅ Autenticación y Autorización
- [x] Login/Logout con JWT
- [x] Gestión de roles y permisos
- [x] Cambio de contraseña
- [x] Perfiles de usuario

### ✅ Usuarios
- [x] Administradores
- [x] Docentes
- [x] Estudiantes
- [x] Padres/Tutores
- [x] Gestión de perfiles

### ✅ Cursos/Grados/Secciones
- [x] Crear y asignar cursos
- [x] Asignar docentes y estudiantes
- [x] Agrupaciones por nivel académico

### ✅ Asignaturas
- [x] Registro de materias
- [x] Relación con cursos y docentes

### ✅ Horarios
- [x] Crear horarios de clases
- [x] Ver horario por docente o estudiante

### ✅ Estudiantes
- [x] Registro y matrícula
- [x] Información académica
- [x] Información de contacto y emergencia

### ✅ Docentes
- [x] Registro y asignaciones
- [x] Carga académica
- [x] Información profesional

### ✅ Matrícula
- [x] Gestión de inscripciones
- [x] Asignación a períodos y cursos

### ✅ Calificaciones
- [x] Registro de notas por materia y evaluación
- [x] Sistema de evaluaciones
- [x] Cálculo de promedios

### ✅ Evaluaciones
- [x] Tareas, exámenes y proyectos
- [x] Criterios de evaluación
- [x] Ponderaciones

### ✅ Asistencia
- [x] Registro diario de asistencia
- [x] Estados de asistencia (Presente, Ausente, Tardanza, etc.)

### ✅ Calendario Académico
- [x] Eventos académicos
- [x] Feriados y fechas importantes

### ✅ Reportes
- [x] Información de calificaciones
- [x] Estadísticas de rendimiento
- [x] Reportes de asistencia

### ✅ Notificaciones
- [x] Sistema de notificaciones internas
- [x] Mensajes entre usuarios
- [x] Anuncios escolares

### ✅ Parámetros del Sistema
- [x] Períodos académicos
- [x] Catálogos (género, nacionalidad, etc.)
- [x] Configuración de roles

## 🚀 Endpoints Disponibles

### Autenticación
```
POST /api/auth/login
POST /api/auth/register
POST /api/auth/change-password
GET  /api/auth/profile
PUT  /api/auth/profile
```

### Usuarios
```
GET    /api/users
GET    /api/users/{id}
PUT    /api/users/{id}
DELETE /api/users/{id}
GET    /api/users/stats
```

### Estudiantes
```
GET    /api/students
GET    /api/students/{id}
POST   /api/students
PUT    /api/students/{id}
DELETE /api/students/{id}
```

### Docentes
```
GET    /api/teachers
GET    /api/teachers/{id}
POST   /api/teachers
PUT    /api/teachers/{id}
DELETE /api/teachers/{id}
GET    /api/teachers/{id}/workload
```

### Grados Académicos
```
GET    /api/grades
GET    /api/grades/{id}
POST   /api/grades
PUT    /api/grades/{id}
DELETE /api/grades/{id}
GET    /api/grades/stats
```

### Cursos
```
GET    /api/courses
GET    /api/courses/{id}
POST   /api/courses
PUT    /api/courses/{id}
DELETE /api/courses/{id}
POST   /api/courses/{id}/assign-teacher
DELETE /api/courses/{id}/remove-teacher/{teacherId}
GET    /api/courses/stats
```

### Asignaturas
```
GET    /api/subjects
GET    /api/subjects/{id}
POST   /api/subjects
PUT    /api/subjects/{id}
DELETE /api/subjects/{id}
POST   /api/subjects/{id}/assign-teacher
DELETE /api/subjects/{id}/remove-teacher/{teacherId}
GET    /api/subjects/departments
GET    /api/subjects/stats
```

### Matrículas
```
GET    /api/enrollments
GET    /api/enrollments/{id}
POST   /api/enrollments
PUT    /api/enrollments/{id}
DELETE /api/enrollments/{id}
GET    /api/enrollments/student/{studentId}
GET    /api/enrollments/course/{courseId}
GET    /api/enrollments/stats
```

### Evaluaciones
```
GET    /api/evaluations
GET    /api/evaluations/{id}
POST   /api/evaluations
PUT    /api/evaluations/{id}
DELETE /api/evaluations/{id}
POST   /api/evaluations/{id}/register-grade
GET    /api/evaluations/{id}/grades
GET    /api/evaluations/subject/{subjectId}
GET    /api/evaluations/stats
```

### Horarios
```
GET    /api/schedules
GET    /api/schedules/{id}
POST   /api/schedules
PUT    /api/schedules/{id}
DELETE /api/schedules/{id}
GET    /api/schedules/teacher/{teacherId}
GET    /api/schedules/course/{courseId}
GET    /api/schedules/stats
```

### Asistencia
```
GET    /api/attendance
GET    /api/attendance/{id}
POST   /api/attendance
POST   /api/attendance/bulk
PUT    /api/attendance/{id}
DELETE /api/attendance/{id}
GET    /api/attendance/student/{studentId}
GET    /api/attendance/course/{courseId}
GET    /api/attendance/report
GET    /api/attendance/stats
```

### Períodos Académicos
```
GET    /api/academicperiods
GET    /api/academicperiods/{id}
POST   /api/academicperiods
PUT    /api/academicperiods/{id}
DELETE /api/academicperiods/{id}
GET    /api/academicperiods/current
GET    /api/academicperiods/upcoming
GET    /api/academicperiods/active
GET    /api/academicperiods/{id}/enrollments
GET    /api/academicperiods/{id}/evaluations
GET    /api/academicperiods/{id}/events
GET    /api/academicperiods/stats
```

### Eventos Académicos
```
GET    /api/academicevents
GET    /api/academicevents/{id}
POST   /api/academicevents
PUT    /api/academicevents/{id}
DELETE /api/academicevents/{id}
GET    /api/academicevents/calendar
GET    /api/academicevents/upcoming
GET    /api/academicevents/today
GET    /api/academicevents/period/{academicPeriodId}
GET    /api/academicevents/type/{type}
GET    /api/academicevents/stats
```

### Notificaciones
```
GET    /api/notifications
GET    /api/notifications/{id}
POST   /api/notifications
POST   /api/notifications/bulk
PUT    /api/notifications/{id}
DELETE /api/notifications/{id}
PUT    /api/notifications/{id}/read
PUT    /api/notifications/read-all
GET    /api/notifications/unread
GET    /api/notifications/unread-count
GET    /api/notifications/my
GET    /api/notifications/sent
GET    /api/notifications/stats
```

### Padres/Tutores
```
GET    /api/parents
GET    /api/parents/{id}
POST   /api/parents
PUT    /api/parents/{id}
DELETE /api/parents/{id}
GET    /api/parents/student/{studentId}
POST   /api/parents/student/{studentId}
PUT    /api/parents/student/{studentParentId}
DELETE /api/parents/student/{studentParentId}
GET    /api/parents/emergency-contacts
GET    /api/parents/stats
```

### Permisos
```
GET    /api/permissions
GET    /api/permissions/{id}
POST   /api/permissions
PUT    /api/permissions/{id}
DELETE /api/permissions/{id}
GET    /api/permissions/modules
GET    /api/permissions/actions
GET    /api/permissions/module/{module}
POST   /api/permissions/assign
DELETE /api/permissions/revoke/{userPermissionId}
GET    /api/permissions/user/{userId}
POST   /api/permissions/bulk-assign
GET    /api/permissions/stats
```

### Reportes
```
GET    /api/reports/student-performance
GET    /api/reports/attendance-summary
GET    /api/reports/enrollment-statistics
GET    /api/reports/teacher-workload
GET    /api/reports/academic-progress
GET    /api/reports/dashboard-summary
```

### Parámetros del Sistema
```
GET    /api/systemparameters
GET    /api/systemparameters/{id}
POST   /api/systemparameters
PUT    /api/systemparameters/{id}
DELETE /api/systemparameters/{id}
GET    /api/systemparameters/category/{category}
GET    /api/systemparameters/categories
GET    /api/systemparameters/value/{name}
PUT    /api/systemparameters/value/{name}
POST   /api/systemparameters/bulk-update
GET    /api/systemparameters/export
POST   /api/systemparameters/import
GET    /api/systemparameters/stats
```

## 🔐 Seguridad

### JWT Configuration
- Algoritmo: HMAC SHA256
- Expiración: 24 horas
- Claims: userId, username, email, role, firstName, lastName

### Autorización
- Roles requeridos en controladores
- Validación de permisos
- Protección de endpoints sensibles

### Validación de Datos
- Data Annotations en modelos
- Validación en DTOs
- Manejo de errores consistente

## 📈 Escalabilidad

### Diseño de Base de Datos
- Normalización apropiada
- Índices en campos de búsqueda
- Relaciones optimizadas

### Arquitectura
- Separación de responsabilidades
- DTOs para transferencia de datos
- Controladores especializados

## 🧪 Testing

### Endpoints de Prueba
- Swagger UI disponible en `/swagger`
- Documentación automática de endpoints
- Ejemplos de uso incluidos

### Datos de Prueba
- Usuario administrador: `admin` / `admin123`
- Configuración inicial de base de datos

## 🚀 Despliegue

### Render Configuration
- Docker deployment
- Variables de entorno configuradas
- Base de datos PostgreSQL
- SSL automático

### Variables de Entorno Requeridas
```
ConnectionStrings__DefaultConnection
Jwt__Key
ASPNETCORE_ENVIRONMENT=Production
```

## 📋 Próximos Pasos
# Documentación Completa - API Sistema Escolar

## 📋 Resumen Ejecutivo

Se ha implementado una API completa para un sistema escolar que incluye todos los módulos solicitados. La aplicación está construida con .NET 8, Entity Framework Core y PostgreSQL, con soporte completo para Docker y despliegue en Render.

## 🏗️ Arquitectura del Sistema

### Estructura de Capas
```
api-school-system/
├── Controllers/          # Controladores de la API
├── Models/              # Modelos de datos
├── Dtos/                # Objetos de transferencia de datos
├── Data/                # Contexto de base de datos
├── Helpers/             # Utilidades y helpers
├── Migrations/          # Migraciones de Entity Framework
└── Properties/          # Configuración del proyecto
```

## 📊 Modelos de Datos Implementados

### 1. Gestión de Usuarios
- **User**: Usuario base del sistema con roles y permisos
- **Permission**: Permisos del sistema
- **UserPermission**: Relación muchos a muchos entre usuarios y permisos

### 2. Entidades Académicas
- **Grade**: Grados académicos (1°, 2°, 3°, etc.)
- **Course**: Cursos/secciones específicas
- **Subject**: Asignaturas o materias
- **AcademicPeriod**: Períodos académicos (semestres, trimestres)
- **AcademicEvent**: Eventos del calendario académico

### 3. Personas del Sistema
- **Student**: Información específica de estudiantes
- **Teacher**: Información específica de docentes
- **Parent**: Información de padres/tutores
- **StudentParent**: Relación entre estudiantes y padres

### 4. Relaciones Académicas
- **CourseTeacher**: Asignación de docentes a cursos
- **SubjectTeacher**: Asignación de docentes a asignaturas
- **Enrollment**: Matrículas de estudiantes

### 5. Registros Académicos
- **Evaluation**: Evaluaciones (exámenes, tareas, proyectos)
- **StudentGrade**: Calificaciones de estudiantes
- **Attendance**: Control de asistencia
- **Schedule**: Horarios de clases

### 6. Comunicación
- **Notification**: Sistema de notificaciones internas

## 🔐 Sistema de Autenticación y Autorización

### Roles Implementados
1. **Admin**: Acceso completo al sistema
2. **Teacher**: Gestión académica y calificaciones
3. **Student**: Acceso a información personal
4. **Parent**: Acceso a información de hijos
5. **Tutor**: Similar a Parent

### JWT Implementation
- Tokens con expiración de 24 horas
- Claims personalizados para información del usuario
- Validación de roles en controladores

## 🎯 Controladores Implementados

### 1. AuthController
- **POST /api/auth/login**: Autenticación de usuarios
- **POST /api/auth/register**: Registro de nuevos usuarios
- **POST /api/auth/change-password**: Cambio de contraseña
- **GET /api/auth/profile**: Obtener perfil del usuario
- **PUT /api/auth/profile**: Actualizar perfil

### 2. UsersController
- **GET /api/users**: Listar usuarios con filtros
- **GET /api/users/{id}**: Obtener usuario específico
- **PUT /api/users/{id}**: Actualizar usuario
- **DELETE /api/users/{id}**: Desactivar usuario
- **GET /api/users/stats**: Estadísticas de usuarios

### 3. StudentsController
- **GET /api/students**: Listar estudiantes con filtros
- **GET /api/students/{id}**: Obtener estudiante con detalles completos
- **POST /api/students**: Crear nuevo estudiante
- **PUT /api/students/{id}**: Actualizar estudiante
- **DELETE /api/students/{id}**: Desactivar estudiante

### 4. TeachersController
- **GET /api/teachers**: Listar docentes con filtros
- **GET /api/teachers/{id}**: Obtener docente con detalles completos
- **POST /api/teachers**: Crear nuevo docente
- **PUT /api/teachers/{id}**: Actualizar docente
- **DELETE /api/teachers/{id}**: Desactivar docente
- **GET /api/teachers/{id}/workload**: Carga académica del docente

### 5. GradesController
- **GET /api/grades**: Listar grados académicos
- **GET /api/grades/{id}**: Obtener grado específico
- **POST /api/grades**: Crear nuevo grado
- **PUT /api/grades/{id}**: Actualizar grado
- **DELETE /api/grades/{id}**: Desactivar grado
- **GET /api/grades/stats**: Estadísticas de grados

### 6. CoursesController
- **GET /api/courses**: Listar cursos con filtros
- **GET /api/courses/{id}**: Obtener curso específico
- **POST /api/courses**: Crear nuevo curso
- **PUT /api/courses/{id}**: Actualizar curso
- **DELETE /api/courses/{id}**: Desactivar curso
- **POST /api/courses/{id}/assign-teacher**: Asignar docente al curso
- **DELETE /api/courses/{id}/remove-teacher/{teacherId}**: Remover docente del curso
- **GET /api/courses/stats**: Estadísticas de cursos

### 7. SubjectsController
- **GET /api/subjects**: Listar asignaturas con filtros
- **GET /api/subjects/{id}**: Obtener asignatura específica
- **POST /api/subjects**: Crear nueva asignatura
- **PUT /api/subjects/{id}**: Actualizar asignatura
- **DELETE /api/subjects/{id}**: Desactivar asignatura
- **POST /api/subjects/{id}/assign-teacher**: Asignar docente a la asignatura
- **DELETE /api/subjects/{id}/remove-teacher/{teacherId}**: Remover docente de la asignatura
- **GET /api/subjects/departments**: Obtener departamentos
- **GET /api/subjects/stats**: Estadísticas de asignaturas

### 8. EnrollmentsController
- **GET /api/enrollments**: Listar matrículas con filtros
- **GET /api/enrollments/{id}**: Obtener matrícula específica
- **POST /api/enrollments**: Crear nueva matrícula
- **PUT /api/enrollments/{id}**: Actualizar matrícula
- **DELETE /api/enrollments/{id}**: Cancelar matrícula
- **GET /api/enrollments/student/{studentId}**: Matrículas de un estudiante
- **GET /api/enrollments/course/{courseId}**: Matrículas de un curso
- **GET /api/enrollments/stats**: Estadísticas de matrículas

### 9. EvaluationsController
- **GET /api/evaluations**: Listar evaluaciones con filtros
- **GET /api/evaluations/{id}**: Obtener evaluación específica
- **POST /api/evaluations**: Crear nueva evaluación
- **PUT /api/evaluations/{id}**: Actualizar evaluación
- **DELETE /api/evaluations/{id}**: Desactivar evaluación
- **POST /api/evaluations/{id}/register-grade**: Registrar calificación
- **GET /api/evaluations/{id}/grades**: Calificaciones de una evaluación
- **GET /api/evaluations/subject/{subjectId}**: Evaluaciones de una asignatura
- **GET /api/evaluations/stats**: Estadísticas de evaluaciones

### 10. SchedulesController
- **GET /api/schedules**: Listar horarios con filtros
- **GET /api/schedules/{id}**: Obtener horario específico
- **POST /api/schedules**: Crear nuevo horario
- **PUT /api/schedules/{id}**: Actualizar horario
- **DELETE /api/schedules/{id}**: Desactivar horario
- **GET /api/schedules/teacher/{teacherId}**: Horario de un docente
- **GET /api/schedules/course/{courseId}**: Horario de un curso
- **GET /api/schedules/stats**: Estadísticas de horarios

### 11. AttendanceController
- **GET /api/attendance**: Listar registros de asistencia con filtros
- **GET /api/attendance/{id}**: Obtener registro específico
- **POST /api/attendance**: Crear registro de asistencia
- **POST /api/attendance/bulk**: Registro masivo de asistencia
- **PUT /api/attendance/{id}**: Actualizar registro
- **DELETE /api/attendance/{id}**: Desactivar registro
- **GET /api/attendance/student/{studentId}**: Asistencia de un estudiante
- **GET /api/attendance/course/{courseId}**: Asistencia de un curso
- **GET /api/attendance/report**: Reporte de asistencia
- **GET /api/attendance/stats**: Estadísticas de asistencia

### 12. AcademicPeriodsController
- **GET /api/academicperiods**: Listar períodos académicos
- **GET /api/academicperiods/{id}**: Obtener período específico
- **POST /api/academicperiods**: Crear nuevo período
- **PUT /api/academicperiods/{id}**: Actualizar período
- **DELETE /api/academicperiods/{id}**: Desactivar período
- **GET /api/academicperiods/current**: Período académico actual
- **GET /api/academicperiods/upcoming**: Períodos próximos
- **GET /api/academicperiods/active**: Períodos activos
- **GET /api/academicperiods/{id}/enrollments**: Matrículas del período
- **GET /api/academicperiods/{id}/evaluations**: Evaluaciones del período
- **GET /api/academicperiods/{id}/events**: Eventos del período
- **GET /api/academicperiods/stats**: Estadísticas de períodos

### 13. AcademicEventsController
- **GET /api/academicevents**: Listar eventos académicos
- **GET /api/academicevents/{id}**: Obtener evento específico
- **POST /api/academicevents**: Crear nuevo evento
- **PUT /api/academicevents/{id}**: Actualizar evento
- **DELETE /api/academicevents/{id}**: Desactivar evento
- **GET /api/academicevents/calendar**: Eventos para calendario
- **GET /api/academicevents/upcoming**: Eventos próximos
- **GET /api/academicevents/today**: Eventos de hoy
- **GET /api/academicevents/period/{academicPeriodId}**: Eventos de un período
- **GET /api/academicevents/type/{type}**: Eventos por tipo
- **GET /api/academicevents/stats**: Estadísticas de eventos

### 14. NotificationsController
- **GET /api/notifications**: Listar notificaciones (filtrado por usuario)
- **GET /api/notifications/{id}**: Obtener notificación específica
- **POST /api/notifications**: Crear nueva notificación
- **POST /api/notifications/bulk**: Crear notificaciones masivas
- **PUT /api/notifications/{id}**: Actualizar notificación
- **DELETE /api/notifications/{id}**: Desactivar notificación
- **PUT /api/notifications/{id}/read**: Marcar como leída
- **PUT /api/notifications/read-all**: Marcar todas como leídas
- **GET /api/notifications/unread**: Notificaciones no leídas
- **GET /api/notifications/unread-count**: Contador de no leídas
- **GET /api/notifications/my**: Mis notificaciones con paginación
- **GET /api/notifications/sent**: Notificaciones enviadas
- **GET /api/notifications/stats**: Estadísticas de notificaciones

### 15. ParentsController
- **GET /api/parents**: Listar padres/tutores con filtros
- **GET /api/parents/{id}**: Obtener padre específico
- **POST /api/parents**: Crear nuevo padre/tutor
- **PUT /api/parents/{id}**: Actualizar padre/tutor
- **DELETE /api/parents/{id}**: Desactivar padre/tutor
- **GET /api/parents/student/{studentId}**: Padres de un estudiante
- **POST /api/parents/student/{studentId}**: Asignar padre a estudiante
- **PUT /api/parents/student/{studentParentId}**: Actualizar relación padre-estudiante
- **DELETE /api/parents/student/{studentParentId}**: Remover padre de estudiante
- **GET /api/parents/emergency-contacts**: Contactos de emergencia
- **GET /api/parents/stats**: Estadísticas de padres

### 16. PermissionsController
- **GET /api/permissions**: Listar permisos con filtros
- **GET /api/permissions/{id}**: Obtener permiso específico
- **POST /api/permissions**: Crear nuevo permiso
- **PUT /api/permissions/{id}**: Actualizar permiso
- **DELETE /api/permissions/{id}**: Desactivar permiso
- **GET /api/permissions/modules**: Obtener módulos
- **GET /api/permissions/actions**: Obtener acciones
- **GET /api/permissions/module/{module}**: Permisos por módulo
- **POST /api/permissions/assign**: Asignar permiso a usuario
- **DELETE /api/permissions/revoke/{userPermissionId}**: Revocar permiso
- **GET /api/permissions/user/{userId}**: Permisos de un usuario
- **POST /api/permissions/bulk-assign**: Asignación masiva de permisos
- **GET /api/permissions/stats**: Estadísticas de permisos

### 17. ReportsController
- **GET /api/reports/student-performance**: Reporte de rendimiento estudiantil
- **GET /api/reports/attendance-summary**: Reporte resumen de asistencia
- **GET /api/reports/enrollment-statistics**: Estadísticas de matrículas
- **GET /api/reports/teacher-workload**: Reporte de carga académica docente
- **GET /api/reports/academic-progress**: Reporte de progreso académico
- **GET /api/reports/dashboard-summary**: Resumen del dashboard

### 18. SystemParametersController
- **GET /api/systemparameters**: Listar parámetros del sistema
- **GET /api/systemparameters/{id}**: Obtener parámetro específico
- **POST /api/systemparameters**: Crear nuevo parámetro
- **PUT /api/systemparameters/{id}**: Actualizar parámetro
- **DELETE /api/systemparameters/{id}**: Desactivar parámetro
- **GET /api/systemparameters/category/{category}**: Parámetros por categoría
- **GET /api/systemparameters/categories**: Obtener categorías
- **GET /api/systemparameters/value/{name}**: Obtener valor de parámetro
- **PUT /api/systemparameters/value/{name}**: Actualizar valor de parámetro
- **POST /api/systemparameters/bulk-update**: Actualización masiva
- **GET /api/systemparameters/export**: Exportar parámetros
- **POST /api/systemparameters/import**: Importar parámetros
- **GET /api/systemparameters/stats**: Estadísticas de parámetros

## 📋 DTOs Implementados

### Autenticación
- **LoginDto**: Credenciales de inicio de sesión
- **RegisterDto**: Datos para registro de usuarios
- **ChangePasswordDto**: Cambio de contraseña
- **UpdateProfileDto**: Actualización de perfil

### Usuarios
- **UpdateUserDto**: Actualización de datos de usuario

### Estudiantes
- **CreateStudentDto**: Creación de estudiantes
- **UpdateStudentDto**: Actualización de estudiantes

### Docentes
- **CreateTeacherDto**: Creación de docentes
- **UpdateTeacherDto**: Actualización de docentes

### Grados Académicos
- **CreateGradeDto**: Creación de grados
- **UpdateGradeDto**: Actualización de grados

### Cursos
- **CreateCourseDto**: Creación de cursos
- **UpdateCourseDto**: Actualización de cursos
- **AssignTeacherDto**: Asignación de docentes a cursos

### Asignaturas
- **CreateSubjectDto**: Creación de asignaturas
- **UpdateSubjectDto**: Actualización de asignaturas
- **AssignSubjectTeacherDto**: Asignación de docentes a asignaturas

### Matrículas
- **CreateEnrollmentDto**: Creación de matrículas
- **UpdateEnrollmentDto**: Actualización de matrículas

### Evaluaciones
- **CreateEvaluationDto**: Creación de evaluaciones
- **UpdateEvaluationDto**: Actualización de evaluaciones
- **RegisterGradeDto**: Registro de calificaciones

### Horarios
- **CreateScheduleDto**: Creación de horarios
- **UpdateScheduleDto**: Actualización de horarios

### Asistencia
- **CreateAttendanceDto**: Creación de registros de asistencia
- **CreateBulkAttendanceDto**: Creación masiva de asistencia
- **BulkAttendanceRecord**: Registro individual en creación masiva
- **UpdateAttendanceDto**: Actualización de asistencia

### Períodos Académicos
- **CreateAcademicPeriodDto**: Creación de períodos académicos
- **UpdateAcademicPeriodDto**: Actualización de períodos académicos

### Eventos Académicos
- **CreateAcademicEventDto**: Creación de eventos académicos
- **UpdateAcademicEventDto**: Actualización de eventos académicos

### Notificaciones
- **CreateNotificationDto**: Creación de notificaciones
- **CreateBulkNotificationDto**: Creación masiva de notificaciones
- **UpdateNotificationDto**: Actualización de notificaciones

### Padres/Tutores
- **CreateParentDto**: Creación de padres/tutores
- **UpdateParentDto**: Actualización de padres/tutores
- **AssignParentDto**: Asignación de padres a estudiantes
- **UpdateStudentParentDto**: Actualización de relación estudiante-padre

### Permisos
- **CreatePermissionDto**: Creación de permisos
- **UpdatePermissionDto**: Actualización de permisos
- **AssignPermissionDto**: Asignación de permisos a usuarios
- **BulkAssignPermissionDto**: Asignación masiva de permisos

### Parámetros del Sistema
- **CreateSystemParameterDto**: Creación de parámetros del sistema
- **UpdateSystemParameterDto**: Actualización de parámetros del sistema
- **UpdateParameterValueDto**: Actualización de valor de parámetro
- **BulkUpdateParametersDto**: Actualización masiva de parámetros
- **ParameterUpdate**: Parámetro individual en actualización masiva
- **ImportParametersDto**: Importación de parámetros
- **ImportParameter**: Parámetro individual en importación

Todos los DTOs incluyen validaciones con Data Annotations para asegurar la integridad de los datos.

## 🗄️ Configuración de Base de Datos

### Entity Framework Configuration
- Configuración de relaciones uno a uno y muchos a muchos
- Índices únicos en campos críticos
- Datos de semilla para usuario administrador
- Configuración de eliminación en cascada apropiada

### Migraciones
- Migración inicial: `CompleteSchoolSystem`
- Incluye todas las tablas y relaciones
- Datos de semilla para administrador

## 🐳 Configuración Docker

### Dockerfile
- Multi-stage build para optimización
- .NET 8 runtime
- Configuración para producción
- Variables de entorno configuradas

### Docker Compose
- Configuración para desarrollo local
- Variables de entorno para base de datos
- Puerto 8080 expuesto

## 🔧 Configuración de Swagger

### Habilitado en Producción
- Documentación completa de la API
- Interfaz interactiva para pruebas
- Redirección automática desde la raíz

## 📊 Funcionalidades por Módulo

### ✅ Autenticación y Autorización
- [x] Login/Logout con JWT
- [x] Gestión de roles y permisos
- [x] Cambio de contraseña
- [x] Perfiles de usuario

### ✅ Usuarios
- [x] Administradores
- [x] Docentes
- [x] Estudiantes
- [x] Padres/Tutores
- [x] Gestión de perfiles

### ✅ Cursos/Grados/Secciones
- [x] Crear y asignar cursos
- [x] Asignar docentes y estudiantes
- [x] Agrupaciones por nivel académico

### ✅ Asignaturas
- [x] Registro de materias
- [x] Relación con cursos y docentes

### ✅ Horarios
- [x] Crear horarios de clases
- [x] Ver horario por docente o estudiante

### ✅ Estudiantes
- [x] Registro y matrícula
- [x] Información académica
- [x] Información de contacto y emergencia

### ✅ Docentes
- [x] Registro y asignaciones
- [x] Carga académica
- [x] Información profesional

### ✅ Matrícula
- [x] Gestión de inscripciones
- [x] Asignación a períodos y cursos

### ✅ Calificaciones
- [x] Registro de notas por materia y evaluación
- [x] Sistema de evaluaciones
- [x] Cálculo de promedios

### ✅ Evaluaciones
- [x] Tareas, exámenes y proyectos
- [x] Criterios de evaluación
- [x] Ponderaciones

### ✅ Asistencia
- [x] Registro diario de asistencia
- [x] Estados de asistencia (Presente, Ausente, Tardanza, etc.)

### ✅ Calendario Académico
- [x] Eventos académicos
- [x] Feriados y fechas importantes

### ✅ Reportes
- [x] Información de calificaciones
- [x] Estadísticas de rendimiento
- [x] Reportes de asistencia

### ✅ Notificaciones
- [x] Sistema de notificaciones internas
- [x] Mensajes entre usuarios
- [x] Anuncios escolares

### ✅ Parámetros del Sistema
- [x] Períodos académicos
- [x] Catálogos (género, nacionalidad, etc.)
- [x] Configuración de roles

## 🚀 Endpoints Disponibles

### Autenticación
```
POST /api/auth/login
POST /api/auth/register
POST /api/auth/change-password
GET  /api/auth/profile
PUT  /api/auth/profile
```

### Usuarios
```
GET    /api/users
GET    /api/users/{id}
PUT    /api/users/{id}
DELETE /api/users/{id}
GET    /api/users/stats
```

### Estudiantes
```
GET    /api/students
GET    /api/students/{id}
POST   /api/students
PUT    /api/students/{id}
DELETE /api/students/{id}
```

### Docentes
```
GET    /api/teachers
GET    /api/teachers/{id}
POST   /api/teachers
PUT    /api/teachers/{id}
DELETE /api/teachers/{id}
GET    /api/teachers/{id}/workload
```

### Grados Académicos
```
GET    /api/grades
GET    /api/grades/{id}
POST   /api/grades
PUT    /api/grades/{id}
DELETE /api/grades/{id}
GET    /api/grades/stats
```

### Cursos
```
GET    /api/courses
GET    /api/courses/{id}
POST   /api/courses
PUT    /api/courses/{id}
DELETE /api/courses/{id}
POST   /api/courses/{id}/assign-teacher
DELETE /api/courses/{id}/remove-teacher/{teacherId}
GET    /api/courses/stats
```

### Asignaturas
```
GET    /api/subjects
GET    /api/subjects/{id}
POST   /api/subjects
PUT    /api/subjects/{id}
DELETE /api/subjects/{id}
POST   /api/subjects/{id}/assign-teacher
DELETE /api/subjects/{id}/remove-teacher/{teacherId}
GET    /api/subjects/departments
GET    /api/subjects/stats
```

### Matrículas
```
GET    /api/enrollments
GET    /api/enrollments/{id}
POST   /api/enrollments
PUT    /api/enrollments/{id}
DELETE /api/enrollments/{id}
GET    /api/enrollments/student/{studentId}
GET    /api/enrollments/course/{courseId}
GET    /api/enrollments/stats
```

### Evaluaciones
```
GET    /api/evaluations
GET    /api/evaluations/{id}
POST   /api/evaluations
PUT    /api/evaluations/{id}
DELETE /api/evaluations/{id}
POST   /api/evaluations/{id}/register-grade
GET    /api/evaluations/{id}/grades
GET    /api/evaluations/subject/{subjectId}
GET    /api/evaluations/stats
```

### Horarios
```
GET    /api/schedules
GET    /api/schedules/{id}
POST   /api/schedules
PUT    /api/schedules/{id}
DELETE /api/schedules/{id}
GET    /api/schedules/teacher/{teacherId}
GET    /api/schedules/course/{courseId}
GET    /api/schedules/stats
```

### Asistencia
```
GET    /api/attendance
GET    /api/attendance/{id}
POST   /api/attendance
POST   /api/attendance/bulk
PUT    /api/attendance/{id}
DELETE /api/attendance/{id}
GET    /api/attendance/student/{studentId}
GET    /api/attendance/course/{courseId}
GET    /api/attendance/report
GET    /api/attendance/stats
```

### Períodos Académicos
```
GET    /api/academicperiods
GET    /api/academicperiods/{id}
POST   /api/academicperiods
PUT    /api/academicperiods/{id}
DELETE /api/academicperiods/{id}
GET    /api/academicperiods/current
GET    /api/academicperiods/upcoming
GET    /api/academicperiods/active
GET    /api/academicperiods/{id}/enrollments
GET    /api/academicperiods/{id}/evaluations
GET    /api/academicperiods/{id}/events
GET    /api/academicperiods/stats
```

### Eventos Académicos
```
GET    /api/academicevents
GET    /api/academicevents/{id}
POST   /api/academicevents
PUT    /api/academicevents/{id}
DELETE /api/academicevents/{id}
GET    /api/academicevents/calendar
GET    /api/academicevents/upcoming
GET    /api/academicevents/today
GET    /api/academicevents/period/{academicPeriodId}
GET    /api/academicevents/type/{type}
GET    /api/academicevents/stats
```

### Notificaciones
```
GET    /api/notifications
GET    /api/notifications/{id}
POST   /api/notifications
POST   /api/notifications/bulk
PUT    /api/notifications/{id}
DELETE /api/notifications/{id}
PUT    /api/notifications/{id}/read
PUT    /api/notifications/read-all
GET    /api/notifications/unread
GET    /api/notifications/unread-count
GET    /api/notifications/my
GET    /api/notifications/sent
GET    /api/notifications/stats
```

### Padres/Tutores
```
GET    /api/parents
GET    /api/parents/{id}
POST   /api/parents
PUT    /api/parents/{id}
DELETE /api/parents/{id}
GET    /api/parents/student/{studentId}
POST   /api/parents/student/{studentId}
PUT    /api/parents/student/{studentParentId}
DELETE /api/parents/student/{studentParentId}
GET    /api/parents/emergency-contacts
GET    /api/parents/stats
```

### Permisos
```
GET    /api/permissions
GET    /api/permissions/{id}
POST   /api/permissions
PUT    /api/permissions/{id}
DELETE /api/permissions/{id}
GET    /api/permissions/modules
GET    /api/permissions/actions
GET    /api/permissions/module/{module}
POST   /api/permissions/assign
DELETE /api/permissions/revoke/{userPermissionId}
GET    /api/permissions/user/{userId}
POST   /api/permissions/bulk-assign
GET    /api/permissions/stats
```

### Reportes
```
GET    /api/reports/student-performance
GET    /api/reports/attendance-summary
GET    /api/reports/enrollment-statistics
GET    /api/reports/teacher-workload
GET    /api/reports/academic-progress
GET    /api/reports/dashboard-summary
```

### Parámetros del Sistema
```
GET    /api/systemparameters
GET    /api/systemparameters/{id}
POST   /api/systemparameters
PUT    /api/systemparameters/{id}
DELETE /api/systemparameters/{id}
GET    /api/systemparameters/category/{category}
GET    /api/systemparameters/categories
GET    /api/systemparameters/value/{name}
PUT    /api/systemparameters/value/{name}
POST   /api/systemparameters/bulk-update
GET    /api/systemparameters/export
POST   /api/systemparameters/import
GET    /api/systemparameters/stats
```

## 🔐 Seguridad

### JWT Configuration
- Algoritmo: HMAC SHA256
- Expiración: 24 horas
- Claims: userId, username, email, role, firstName, lastName

### Autorización
- Roles requeridos en controladores
- Validación de permisos
- Protección de endpoints sensibles

### Validación de Datos
- Data Annotations en modelos
- Validación en DTOs
- Manejo de errores consistente

## 📈 Escalabilidad

### Diseño de Base de Datos
- Normalización apropiada
- Índices en campos de búsqueda
- Relaciones optimizadas

### Arquitectura
- Separación de responsabilidades
- DTOs para transferencia de datos
- Controladores especializados

## 🧪 Testing

### Endpoints de Prueba
- Swagger UI disponible en `/swagger`
- Documentación automática de endpoints
- Ejemplos de uso incluidos

### Datos de Prueba
- Usuario administrador: `admin` / `admin123`
- Configuración inicial de base de datos

## 🚀 Despliegue

### Render Configuration
- Docker deployment
- Variables de entorno configuradas
- Base de datos PostgreSQL
- SSL automático

### Variables de Entorno Requeridas
```
ConnectionStrings__DefaultConnection
Jwt__Key
ASPNETCORE_ENVIRONMENT=Production
```

## 📋 Próximos Pasos

### Funcionalidades Adicionales Sugeridas
1. **Controladores para otros módulos**:
   - GradesController
   - CoursesController
   - SubjectsController
   - EnrollmentsController
   - EvaluationsController
   - GradesController (calificaciones)
   - AttendanceController
   - SchedulesController
   - AcademicPeriodsController
   - AcademicEventsController
   - NotificationsController

2. **Reportes avanzados**:
   - Boletines de notas
   - Estadísticas de rendimiento
   - Reportes de asistencia

3. **Funcionalidades adicionales**:
   - Subida de archivos (fotos de perfil)
   - Exportación de datos
   - Integración con sistemas externos

## 📞 Soporte

Para cualquier consulta sobre la implementación o funcionalidades adicionales, contactar al equipo de desarrollo. 