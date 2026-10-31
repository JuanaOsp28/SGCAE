# Evidencias de funcionamiento - SGCAE

Generado automáticamente por `scripts/generar_evidencias.py` contra `http://localhost:5009` el 2026-10-31 02:35 UTC, sobre una base de datos SQL Server recién creada.

Cada paso muestra la petición enviada y la respuesta real de la API. El script se detiene si algún código HTTP o resultado no coincide con el esperado, de modo que este documento solo se genera cuando los 38 pasos se comportan como se espera.


## RF-06. Puntos de acceso

Al iniciar en Development la API aplica las migraciones y registra cuatro puntos de acceso base.

### 1. Listar puntos de acceso iniciales

`GET /api/access-points`

Respuesta: **HTTP 200**

```json
{
  "items": [
    {
      "id": "7adca0be-f672-46d2-a9c3-084fb362a4ef",
      "code": "BODEGA-01",
      "name": "Bodega de almacenamiento 1",
      "type": "Warehouse",
      "location": "Sede principal - Patio de carga",
      "isEnabled": true,
      "createdAtUtc": "2026-10-31T02:35:49.4007383Z"
    },
    {
      "id": "9b4d9104-531f-48d6-831b-96a72d37e746",
      "code": "CUARTO-SERV",
      "name": "Cuarto de servidores",
      "type": "RestrictedArea",
      "location": "Sede principal - Piso 3",
      "isEnabled": true,
      "createdAtUtc": "2026-10-31T02:35:49.4007383Z"
    },
    {
      "id": "ca7f515c-9a94-4fed-bfb2-537dfc816cca",
      "code": "PARQUEADERO",
      "name": "Parqueadero empleados",
      "type": "Parking",
      "location": "Sede principal - Sótano",
      "isEnabled": true,
      "createdAtUtc": "2026-10-31T02:35:49.4007383Z"
    },
    {
      "id": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
      "code": "RECEPCION",
      "name": "Recepción principal",
      "type": "Reception",
      "location": "Sede principal - Piso 1",
      "isEnabled": true,
      "createdAtUtc": "2026-10-31T02:35:49.4007383Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 4,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### 2. Registrar un nuevo punto de acceso

`POST /api/access-points`

Cuerpo de la petición:

```json
{
  "code": "SALA-JUNTAS",
  "name": "Sala de juntas",
  "type": "RestrictedArea",
  "location": "Piso 2"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "ebcb27f3-cd75-4e3a-9ad7-657b8266e205",
  "code": "SALA-JUNTAS",
  "name": "Sala de juntas",
  "type": "RestrictedArea",
  "location": "Piso 2",
  "isEnabled": true,
  "createdAtUtc": "2026-10-31T02:35:52.4107388Z"
}
```

### 3. Rechazo de código de punto duplicado

`POST /api/access-points`

Cuerpo de la petición:

```json
{
  "code": "sala-juntas",
  "name": "Otra sala",
  "type": "Other"
}
```

Respuesta: **HTTP 409**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "PuntoAcceso.CodigoDuplicado",
  "status": 409,
  "detail": "Ya existe un punto de acceso con ese código.",
  "code": "PuntoAcceso.CodigoDuplicado"
}
```

### 4. Deshabilitar el punto de acceso

`POST /api/access-points/ebcb27f3-cd75-4e3a-9ad7-657b8266e205/disable`

Respuesta: **HTTP 200**

```json
{
  "id": "ebcb27f3-cd75-4e3a-9ad7-657b8266e205",
  "code": "SALA-JUNTAS",
  "name": "Sala de juntas",
  "type": "RestrictedArea",
  "location": "Piso 2",
  "isEnabled": false,
  "createdAtUtc": "2026-10-31T02:35:52.4107388Z"
}
```

### 5. Consultar solo los puntos habilitados

`GET /api/access-points?isEnabled=true`

Respuesta: **HTTP 200**

```json
{
  "items": [
    {
      "id": "7adca0be-f672-46d2-a9c3-084fb362a4ef",
      "code": "BODEGA-01",
      "name": "Bodega de almacenamiento 1",
      "type": "Warehouse",
      "location": "Sede principal - Patio de carga",
      "isEnabled": true,
      "createdAtUtc": "2026-10-31T02:35:49.4007383Z"
    },
    {
      "id": "9b4d9104-531f-48d6-831b-96a72d37e746",
      "code": "CUARTO-SERV",
      "name": "Cuarto de servidores",
      "type": "RestrictedArea",
      "location": "Sede principal - Piso 3",
      "isEnabled": true,
      "createdAtUtc": "2026-10-31T02:35:49.4007383Z"
    },
    {
      "id": "ca7f515c-9a94-4fed-bfb2-537dfc816cca",
      "code": "PARQUEADERO",
      "name": "Parqueadero empleados",
      "type": "Parking",
      "location": "Sede principal - Sótano",
      "isEnabled": true,
      "createdAtUtc": "2026-10-31T02:35:49.4007383Z"
    },
    {
      "id": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
      "code": "RECEPCION",
      "name": "Recepción principal",
      "type": "Reception",
      "location": "Sede principal - Piso 1",
      "isEnabled": true,
      "createdAtUtc": "2026-10-31T02:35:49.4007383Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 4,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```


## RF-01. Gestión de empleados

### 6. Registrar empleado

`POST /api/employees`

Cuerpo de la petición:

```json
{
  "identification": "1035123456",
  "firstNames": "Laura",
  "lastNames": "Gómez Restrepo",
  "email": "laura.gomez@logistica.com",
  "position": "Analista de inventario",
  "area": "Bodega"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "identification": "1035123456",
  "firstNames": "Laura",
  "lastNames": "Gómez Restrepo",
  "email": "laura.gomez@logistica.com",
  "position": "Analista de inventario",
  "area": "Bodega",
  "status": "Active",
  "createdAtUtc": "2026-10-31T02:35:52.9304405Z",
  "deactivatedAtUtc": null
}
```

### 7. Registrar segundo empleado

`POST /api/employees`

Cuerpo de la petición:

```json
{
  "identification": "71234567",
  "firstNames": "Carlos",
  "lastNames": "Pérez Mejía",
  "email": "carlos.perez@logistica.com",
  "position": "Conductor",
  "area": "Transporte"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "b8c01136-db77-4b28-b682-9d27c438f91b",
  "identification": "71234567",
  "firstNames": "Carlos",
  "lastNames": "Pérez Mejía",
  "email": "carlos.perez@logistica.com",
  "position": "Conductor",
  "area": "Transporte",
  "status": "Active",
  "createdAtUtc": "2026-10-31T02:35:53.1521706Z",
  "deactivatedAtUtc": null
}
```

### 8. Validación: correo con formato inválido

`POST /api/employees`

Cuerpo de la petición:

```json
{
  "identification": "99887766",
  "firstNames": "Ana",
  "lastNames": "Ruiz",
  "email": "ana-ruiz",
  "position": "Auxiliar",
  "area": "Bodega"
}
```

Respuesta: **HTTP 400**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Correo.Invalid",
  "status": 400,
  "detail": "El correo electrónico no tiene un formato válido.",
  "code": "Correo.Invalid"
}
```

### 9. Validación: identificación duplicada

`POST /api/employees`

Cuerpo de la petición:

```json
{
  "identification": "1035123456",
  "firstNames": "Otra",
  "lastNames": "Persona",
  "email": "otra@logistica.com",
  "position": "Auxiliar",
  "area": "Bodega"
}
```

Respuesta: **HTTP 409**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Empleado.IdentificacionDuplicada",
  "status": 409,
  "detail": "Ya existe un empleado registrado con esa identificación.",
  "code": "Empleado.IdentificacionDuplicada"
}
```

### 10. Consultar empleado por id

`GET /api/employees/f5aac2d1-44fb-4caa-8650-8d3e898ea2a3`

Respuesta: **HTTP 200**

```json
{
  "id": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "identification": "1035123456",
  "firstNames": "Laura",
  "lastNames": "Gómez Restrepo",
  "email": "laura.gomez@logistica.com",
  "position": "Analista de inventario",
  "area": "Bodega",
  "status": "Active",
  "createdAtUtc": "2026-10-31T02:35:52.9304405Z",
  "deactivatedAtUtc": null
}
```

### 11. Actualizar datos del empleado

`PUT /api/employees/f5aac2d1-44fb-4caa-8650-8d3e898ea2a3`

Cuerpo de la petición:

```json
{
  "firstNames": "Laura",
  "lastNames": "Gómez Restrepo",
  "email": "laura.gomez@logistica.com",
  "position": "Coordinadora de inventario",
  "area": "Bodega"
}
```

Respuesta: **HTTP 200**

```json
{
  "id": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "identification": "1035123456",
  "firstNames": "Laura",
  "lastNames": "Gómez Restrepo",
  "email": "laura.gomez@logistica.com",
  "position": "Coordinadora de inventario",
  "area": "Bodega",
  "status": "Active",
  "createdAtUtc": "2026-10-31T02:35:52.9304405Z",
  "deactivatedAtUtc": null
}
```

### 12. Buscar empleados con paginación

`GET /api/employees?search=g%C3%B3mez&page=1&pageSize=10`

Respuesta: **HTTP 200**

```json
{
  "items": [
    {
      "id": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "identification": "1035123456",
      "firstNames": "Laura",
      "lastNames": "Gómez Restrepo",
      "email": "laura.gomez@logistica.com",
      "position": "Coordinadora de inventario",
      "area": "Bodega",
      "status": "Active",
      "createdAtUtc": "2026-10-31T02:35:52.9304405Z",
      "deactivatedAtUtc": null
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```


## RF-02. Gestión de credenciales

### 13. Asignar credencial con código definido

`POST /api/credentials`

Cuerpo de la petición:

```json
{
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "code": "CRED-0001"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "250c1454-2fed-440c-bbbc-f5783bef6b40",
  "code": "CRED-0001",
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "employeeIdentification": "1035123456",
  "employeeName": "Laura Gómez Restrepo",
  "status": "Active",
  "canBeUsed": true,
  "issuedAtUtc": "2026-10-31T02:35:53.3555235Z",
  "statusChangedAtUtc": null,
  "statusReason": null
}
```

### 14. Asignar credencial con código generado automáticamente

`POST /api/credentials`

Cuerpo de la petición:

```json
{
  "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "74d9c8d9-326c-46fd-9dc9-7d74bfa0977b",
  "code": "CRED-8985A71D",
  "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
  "employeeIdentification": "71234567",
  "employeeName": "Carlos Pérez Mejía",
  "status": "Active",
  "canBeUsed": true,
  "issuedAtUtc": "2026-10-31T02:35:53.5670209Z",
  "statusChangedAtUtc": null,
  "statusReason": null
}
```

### 15. Regla: un empleado no puede tener dos credenciales vigentes

`POST /api/credentials`

Cuerpo de la petición:

```json
{
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "code": "CRED-0002"
}
```

Respuesta: **HTTP 409**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Credencial.YaAsignada",
  "status": 409,
  "detail": "El empleado ya tiene una credencial vigente o bloqueada. Revóquela antes de asignar una nueva.",
  "code": "Credencial.YaAsignada"
}
```

### 16. Consultar el estado de una credencial por código

`GET /api/credentials/by-code/cred-0001`

Respuesta: **HTTP 200**

```json
{
  "id": "250c1454-2fed-440c-bbbc-f5783bef6b40",
  "code": "CRED-0001",
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "employeeIdentification": "1035123456",
  "employeeName": "Laura Gómez Restrepo",
  "status": "Active",
  "canBeUsed": true,
  "issuedAtUtc": "2026-10-31T02:35:53.3555235Z",
  "statusChangedAtUtc": null,
  "statusReason": null
}
```


## RF-03. Registro de entradas y salidas

Cada solicitud genera un registro, autorizado o rechazado, con el motivo del rechazo.

### 17. Entrada autorizada

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-0001",
  "accessPointCode": "RECEPCION",
  "movementType": "Entry"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "282067f6-5571-4d28-a25e-5629dd73a739",
  "occurredAtUtc": "2026-10-31T02:35:53.7234992Z",
  "movementType": "Entry",
  "result": "Authorized",
  "rejectionReason": null,
  "requestedCredentialCode": "CRED-0001",
  "requestedAccessPointCode": "RECEPCION",
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "employeeIdentification": "1035123456",
  "employeeName": "Laura Gómez Restrepo",
  "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
  "accessPointName": "Recepción principal",
  "rejectionMessage": null
}
```

### 18. Entrada duplicada rechazada (ya está dentro)

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-0001",
  "accessPointCode": "RECEPCION",
  "movementType": "Entry"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "96b91b7c-3f13-41fc-ad1f-a2c7274db00e",
  "occurredAtUtc": "2026-10-31T02:35:54.0350291Z",
  "movementType": "Entry",
  "result": "Rejected",
  "rejectionReason": "AlreadyInside",
  "requestedCredentialCode": "CRED-0001",
  "requestedAccessPointCode": "RECEPCION",
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "employeeIdentification": "1035123456",
  "employeeName": "Laura Gómez Restrepo",
  "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
  "accessPointName": "Recepción principal",
  "rejectionMessage": "El empleado ya registró una entrada en este punto sin su salida."
}
```

### 19. Salida autorizada

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-0001",
  "accessPointCode": "RECEPCION",
  "movementType": "Exit"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "e524fb78-b495-4f3d-9406-bd503f95de27",
  "occurredAtUtc": "2026-10-31T02:35:54.0831719Z",
  "movementType": "Exit",
  "result": "Authorized",
  "rejectionReason": null,
  "requestedCredentialCode": "CRED-0001",
  "requestedAccessPointCode": "RECEPCION",
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "employeeIdentification": "1035123456",
  "employeeName": "Laura Gómez Restrepo",
  "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
  "accessPointName": "Recepción principal",
  "rejectionMessage": null
}
```

### 20. Salida sin entrada previa rechazada

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-0001",
  "accessPointCode": "RECEPCION",
  "movementType": "Exit"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "7bdcaf92-ac2c-4766-aeec-4e60c617a18b",
  "occurredAtUtc": "2026-10-31T02:35:54.1235527Z",
  "movementType": "Exit",
  "result": "Rejected",
  "rejectionReason": "NotInside",
  "requestedCredentialCode": "CRED-0001",
  "requestedAccessPointCode": "RECEPCION",
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "employeeIdentification": "1035123456",
  "employeeName": "Laura Gómez Restrepo",
  "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
  "accessPointName": "Recepción principal",
  "rejectionMessage": "El empleado no tiene una entrada previa en este punto."
}
```

### 21. Credencial inexistente

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-9999",
  "accessPointCode": "RECEPCION",
  "movementType": "Entry"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "bb50029e-c975-49c1-8bbd-8384da46af9d",
  "occurredAtUtc": "2026-10-31T02:35:54.1519364Z",
  "movementType": "Entry",
  "result": "Rejected",
  "rejectionReason": "CredentialNotFound",
  "requestedCredentialCode": "CRED-9999",
  "requestedAccessPointCode": "RECEPCION",
  "employeeId": null,
  "employeeIdentification": null,
  "employeeName": null,
  "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
  "accessPointName": "Recepción principal",
  "rejectionMessage": "La credencial no está registrada."
}
```

### 22. Punto de acceso inexistente

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-0001",
  "accessPointCode": "PUERTA-X",
  "movementType": "Entry"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "a8a69e48-29ca-41bc-ab0e-a8018b34109d",
  "occurredAtUtc": "2026-10-31T02:35:54.1890929Z",
  "movementType": "Entry",
  "result": "Rejected",
  "rejectionReason": "AccessPointNotFound",
  "requestedCredentialCode": "CRED-0001",
  "requestedAccessPointCode": "PUERTA-X",
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "employeeIdentification": "1035123456",
  "employeeName": "Laura Gómez Restrepo",
  "accessPointId": null,
  "accessPointName": null,
  "rejectionMessage": "El punto de acceso no está registrado."
}
```

### 23. Punto de acceso deshabilitado

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-0001",
  "accessPointCode": "SALA-JUNTAS",
  "movementType": "Entry"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "67e1d855-b2aa-4dd1-9713-9492bb11e0df",
  "occurredAtUtc": "2026-10-31T02:35:54.2043708Z",
  "movementType": "Entry",
  "result": "Rejected",
  "rejectionReason": "AccessPointDisabled",
  "requestedCredentialCode": "CRED-0001",
  "requestedAccessPointCode": "SALA-JUNTAS",
  "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
  "employeeIdentification": "1035123456",
  "employeeName": "Laura Gómez Restrepo",
  "accessPointId": "ebcb27f3-cd75-4e3a-9ad7-657b8266e205",
  "accessPointName": "Sala de juntas",
  "rejectionMessage": "El punto de acceso está deshabilitado."
}
```

### 24. Bloquear credencial

`POST /api/credentials/74d9c8d9-326c-46fd-9dc9-7d74bfa0977b/block`

Cuerpo de la petición:

```json
{
  "reason": "Reporte de pérdida del carné"
}
```

Respuesta: **HTTP 200**

```json
{
  "id": "74d9c8d9-326c-46fd-9dc9-7d74bfa0977b",
  "code": "CRED-8985A71D",
  "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
  "employeeIdentification": "71234567",
  "employeeName": "Carlos Pérez Mejía",
  "status": "Blocked",
  "canBeUsed": false,
  "issuedAtUtc": "2026-10-31T02:35:53.5670209Z",
  "statusChangedAtUtc": "2026-10-31T02:35:54.2329898Z",
  "statusReason": "Reporte de pérdida del carné"
}
```

### 25. Ingreso con credencial bloqueada

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-8985A71D",
  "accessPointCode": "PARQUEADERO",
  "movementType": "Entry"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "a99b0339-8852-4664-bb64-a73f0d5d802a",
  "occurredAtUtc": "2026-10-31T02:35:54.2816115Z",
  "movementType": "Entry",
  "result": "Rejected",
  "rejectionReason": "CredentialBlocked",
  "requestedCredentialCode": "CRED-8985A71D",
  "requestedAccessPointCode": "PARQUEADERO",
  "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
  "employeeIdentification": "71234567",
  "employeeName": "Carlos Pérez Mejía",
  "accessPointId": "ca7f515c-9a94-4fed-bfb2-537dfc816cca",
  "accessPointName": "Parqueadero empleados",
  "rejectionMessage": "La credencial está bloqueada."
}
```

### 26. Desbloquear credencial

`POST /api/credentials/74d9c8d9-326c-46fd-9dc9-7d74bfa0977b/unblock`

Respuesta: **HTTP 200**

```json
{
  "id": "74d9c8d9-326c-46fd-9dc9-7d74bfa0977b",
  "code": "CRED-8985A71D",
  "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
  "employeeIdentification": "71234567",
  "employeeName": "Carlos Pérez Mejía",
  "status": "Active",
  "canBeUsed": true,
  "issuedAtUtc": "2026-10-31T02:35:53.5670209Z",
  "statusChangedAtUtc": "2026-10-31T02:35:54.3096416Z",
  "statusReason": null
}
```

### 27. Ingreso tras desbloqueo

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-8985A71D",
  "accessPointCode": "PARQUEADERO",
  "movementType": "Entry"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "a928e065-b4ef-40c0-808e-2cfeb6c092f1",
  "occurredAtUtc": "2026-10-31T02:35:54.3462723Z",
  "movementType": "Entry",
  "result": "Authorized",
  "rejectionReason": null,
  "requestedCredentialCode": "CRED-8985A71D",
  "requestedAccessPointCode": "PARQUEADERO",
  "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
  "employeeIdentification": "71234567",
  "employeeName": "Carlos Pérez Mejía",
  "accessPointId": "ca7f515c-9a94-4fed-bfb2-537dfc816cca",
  "accessPointName": "Parqueadero empleados",
  "rejectionMessage": null
}
```

### 28. Desactivar empleado (revoca sus credenciales)

`POST /api/employees/b8c01136-db77-4b28-b682-9d27c438f91b/deactivate`

Respuesta: **HTTP 200**

```json
{
  "id": "b8c01136-db77-4b28-b682-9d27c438f91b",
  "identification": "71234567",
  "firstNames": "Carlos",
  "lastNames": "Pérez Mejía",
  "email": "carlos.perez@logistica.com",
  "position": "Conductor",
  "area": "Transporte",
  "status": "Inactive",
  "createdAtUtc": "2026-10-31T02:35:53.1521706Z",
  "deactivatedAtUtc": "2026-10-31T02:35:54.3845892Z"
}
```

### 29. La credencial quedó revocada

`GET /api/credentials/74d9c8d9-326c-46fd-9dc9-7d74bfa0977b`

Respuesta: **HTTP 200**

```json
{
  "id": "74d9c8d9-326c-46fd-9dc9-7d74bfa0977b",
  "code": "CRED-8985A71D",
  "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
  "employeeIdentification": "71234567",
  "employeeName": "Carlos Pérez Mejía",
  "status": "Revoked",
  "canBeUsed": false,
  "issuedAtUtc": "2026-10-31T02:35:53.5670209Z",
  "statusChangedAtUtc": "2026-10-31T02:35:54.3845892Z",
  "statusReason": "Desactivación del empleado"
}
```

### 30. Empleado desactivado: su credencial ya no permite movimientos

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-8985A71D",
  "accessPointCode": "PARQUEADERO",
  "movementType": "Exit"
}
```

Respuesta: **HTTP 201**

```json
{
  "id": "ce5d79ac-f03f-4c59-9e8c-32d36288c377",
  "occurredAtUtc": "2026-10-31T02:35:54.4671821Z",
  "movementType": "Exit",
  "result": "Rejected",
  "rejectionReason": "CredentialRevoked",
  "requestedCredentialCode": "CRED-8985A71D",
  "requestedAccessPointCode": "PARQUEADERO",
  "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
  "employeeIdentification": "71234567",
  "employeeName": "Carlos Pérez Mejía",
  "accessPointId": "ca7f515c-9a94-4fed-bfb2-537dfc816cca",
  "accessPointName": "Parqueadero empleados",
  "rejectionMessage": "La credencial fue revocada."
}
```

### 31. Regla: no se asignan credenciales a empleados inactivos

`POST /api/credentials`

Cuerpo de la petición:

```json
{
  "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b"
}
```

Respuesta: **HTTP 409**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Credencial.EmpleadoInactivo",
  "status": 409,
  "detail": "No se pueden asignar credenciales a un empleado inactivo.",
  "code": "Credencial.EmpleadoInactivo"
}
```

### 32. Validación: tipo de movimiento inválido

`POST /api/access-records`

Cuerpo de la petición:

```json
{
  "credentialCode": "CRED-0001",
  "accessPointCode": "RECEPCION",
  "movementType": "Salto"
}
```

Respuesta: **HTTP 400**

```json
{
  "title": "La solicitud contiene datos con formato inválido.",
  "status": 400,
  "detail": "Revise los campos indicados (p. ej. valores de enumeraciones, fechas o identificadores).",
  "errors": {
    "$.movementType": [
      "The JSON value could not be converted to SGCAE.Application.AccessRecords.RegisterAccessCommand. Path: $.movementType | LineNumber: 0 | BytePositionInLine: 87."
    ]
  }
}
```


## RF-04. Consulta del historial de accesos

### 33. Historial por empleado, paginado (página 1 de tamaño 3)

`GET /api/access-records?employeeId=f5aac2d1-44fb-4caa-8650-8d3e898ea2a3&page=1&pageSize=3`

Respuesta: **HTTP 200**

```json
{
  "items": [
    {
      "id": "67e1d855-b2aa-4dd1-9713-9492bb11e0df",
      "occurredAtUtc": "2026-10-31T02:35:54.2043708Z",
      "movementType": "Entry",
      "result": "Rejected",
      "rejectionReason": "AccessPointDisabled",
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "SALA-JUNTAS",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": "ebcb27f3-cd75-4e3a-9ad7-657b8266e205",
      "accessPointName": "Sala de juntas",
      "rejectionMessage": "El punto de acceso está deshabilitado."
    },
    {
      "id": "a8a69e48-29ca-41bc-ab0e-a8018b34109d",
      "occurredAtUtc": "2026-10-31T02:35:54.1890929Z",
      "movementType": "Entry",
      "result": "Rejected",
      "rejectionReason": "AccessPointNotFound",
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "PUERTA-X",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": null,
      "accessPointName": null,
      "rejectionMessage": "El punto de acceso no está registrado."
    },
    {
      "id": "7bdcaf92-ac2c-4766-aeec-4e60c617a18b",
      "occurredAtUtc": "2026-10-31T02:35:54.1235527Z",
      "movementType": "Exit",
      "result": "Rejected",
      "rejectionReason": "NotInside",
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "RECEPCION",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
      "accessPointName": "Recepción principal",
      "rejectionMessage": "El empleado no tiene una entrada previa en este punto."
    }
  ],
  "page": 1,
  "pageSize": 3,
  "totalCount": 6,
  "totalPages": 2,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 34. Historial por empleado, página 2

`GET /api/access-records?employeeId=f5aac2d1-44fb-4caa-8650-8d3e898ea2a3&page=2&pageSize=3`

Respuesta: **HTTP 200**

```json
{
  "items": [
    {
      "id": "e524fb78-b495-4f3d-9406-bd503f95de27",
      "occurredAtUtc": "2026-10-31T02:35:54.0831719Z",
      "movementType": "Exit",
      "result": "Authorized",
      "rejectionReason": null,
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "RECEPCION",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
      "accessPointName": "Recepción principal",
      "rejectionMessage": null
    },
    {
      "id": "96b91b7c-3f13-41fc-ad1f-a2c7274db00e",
      "occurredAtUtc": "2026-10-31T02:35:54.0350291Z",
      "movementType": "Entry",
      "result": "Rejected",
      "rejectionReason": "AlreadyInside",
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "RECEPCION",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
      "accessPointName": "Recepción principal",
      "rejectionMessage": "El empleado ya registró una entrada en este punto sin su salida."
    },
    {
      "id": "282067f6-5571-4d28-a25e-5629dd73a739",
      "occurredAtUtc": "2026-10-31T02:35:53.7234992Z",
      "movementType": "Entry",
      "result": "Authorized",
      "rejectionReason": null,
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "RECEPCION",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
      "accessPointName": "Recepción principal",
      "rejectionMessage": null
    }
  ],
  "page": 2,
  "pageSize": 3,
  "totalCount": 6,
  "totalPages": 2,
  "hasPreviousPage": true,
  "hasNextPage": false
}
```

### 35. Solo accesos rechazados

`GET /api/access-records?result=Rejected&pageSize=5`

Respuesta: **HTTP 200**

```json
{
  "items": [
    {
      "id": "ce5d79ac-f03f-4c59-9e8c-32d36288c377",
      "occurredAtUtc": "2026-10-31T02:35:54.4671821Z",
      "movementType": "Exit",
      "result": "Rejected",
      "rejectionReason": "CredentialRevoked",
      "requestedCredentialCode": "CRED-8985A71D",
      "requestedAccessPointCode": "PARQUEADERO",
      "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
      "employeeIdentification": "71234567",
      "employeeName": "Carlos Pérez Mejía",
      "accessPointId": "ca7f515c-9a94-4fed-bfb2-537dfc816cca",
      "accessPointName": "Parqueadero empleados",
      "rejectionMessage": "La credencial fue revocada."
    },
    {
      "id": "a99b0339-8852-4664-bb64-a73f0d5d802a",
      "occurredAtUtc": "2026-10-31T02:35:54.2816115Z",
      "movementType": "Entry",
      "result": "Rejected",
      "rejectionReason": "CredentialBlocked",
      "requestedCredentialCode": "CRED-8985A71D",
      "requestedAccessPointCode": "PARQUEADERO",
      "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
      "employeeIdentification": "71234567",
      "employeeName": "Carlos Pérez Mejía",
      "accessPointId": "ca7f515c-9a94-4fed-bfb2-537dfc816cca",
      "accessPointName": "Parqueadero empleados",
      "rejectionMessage": "La credencial está bloqueada."
    },
    {
      "id": "67e1d855-b2aa-4dd1-9713-9492bb11e0df",
      "occurredAtUtc": "2026-10-31T02:35:54.2043708Z",
      "movementType": "Entry",
      "result": "Rejected",
      "rejectionReason": "AccessPointDisabled",
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "SALA-JUNTAS",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": "ebcb27f3-cd75-4e3a-9ad7-657b8266e205",
      "accessPointName": "Sala de juntas",
      "rejectionMessage": "El punto de acceso está deshabilitado."
    },
    {
      "id": "a8a69e48-29ca-41bc-ab0e-a8018b34109d",
      "occurredAtUtc": "2026-10-31T02:35:54.1890929Z",
      "movementType": "Entry",
      "result": "Rejected",
      "rejectionReason": "AccessPointNotFound",
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "PUERTA-X",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": null,
      "accessPointName": null,
      "rejectionMessage": "El punto de acceso no está registrado."
    },
    {
      "id": "bb50029e-c975-49c1-8bbd-8384da46af9d",
      "occurredAtUtc": "2026-10-31T02:35:54.1519364Z",
      "movementType": "Entry",
      "result": "Rejected",
      "rejectionReason": "CredentialNotFound",
      "requestedCredentialCode": "CRED-9999",
      "requestedAccessPointCode": "RECEPCION",
      "employeeId": null,
      "employeeIdentification": null,
      "employeeName": null,
      "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
      "accessPointName": "Recepción principal",
      "rejectionMessage": "La credencial no está registrada."
    }
  ],
  "page": 1,
  "pageSize": 5,
  "totalCount": 7,
  "totalPages": 2,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 36. Por punto de acceso y resultado

`GET /api/access-records?accessPointId=78bf3d54-afe6-4a31-83ae-aa4805d05fe9&result=Authorized`

Respuesta: **HTTP 200**

```json
{
  "items": [
    {
      "id": "e524fb78-b495-4f3d-9406-bd503f95de27",
      "occurredAtUtc": "2026-10-31T02:35:54.0831719Z",
      "movementType": "Exit",
      "result": "Authorized",
      "rejectionReason": null,
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "RECEPCION",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
      "accessPointName": "Recepción principal",
      "rejectionMessage": null
    },
    {
      "id": "282067f6-5571-4d28-a25e-5629dd73a739",
      "occurredAtUtc": "2026-10-31T02:35:53.7234992Z",
      "movementType": "Entry",
      "result": "Authorized",
      "rejectionReason": null,
      "requestedCredentialCode": "CRED-0001",
      "requestedAccessPointCode": "RECEPCION",
      "employeeId": "f5aac2d1-44fb-4caa-8650-8d3e898ea2a3",
      "employeeIdentification": "1035123456",
      "employeeName": "Laura Gómez Restrepo",
      "accessPointId": "78bf3d54-afe6-4a31-83ae-aa4805d05fe9",
      "accessPointName": "Recepción principal",
      "rejectionMessage": null
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 2,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### 37. Por rango de fechas

`GET /api/access-records?fromUtc=2026-10-31T02:34:52Z&toUtc=2026-10-31T02:36:54Z&pageSize=2`

Respuesta: **HTTP 200**

```json
{
  "items": [
    {
      "id": "ce5d79ac-f03f-4c59-9e8c-32d36288c377",
      "occurredAtUtc": "2026-10-31T02:35:54.4671821Z",
      "movementType": "Exit",
      "result": "Rejected",
      "rejectionReason": "CredentialRevoked",
      "requestedCredentialCode": "CRED-8985A71D",
      "requestedAccessPointCode": "PARQUEADERO",
      "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
      "employeeIdentification": "71234567",
      "employeeName": "Carlos Pérez Mejía",
      "accessPointId": "ca7f515c-9a94-4fed-bfb2-537dfc816cca",
      "accessPointName": "Parqueadero empleados",
      "rejectionMessage": "La credencial fue revocada."
    },
    {
      "id": "a928e065-b4ef-40c0-808e-2cfeb6c092f1",
      "occurredAtUtc": "2026-10-31T02:35:54.3462723Z",
      "movementType": "Entry",
      "result": "Authorized",
      "rejectionReason": null,
      "requestedCredentialCode": "CRED-8985A71D",
      "requestedAccessPointCode": "PARQUEADERO",
      "employeeId": "b8c01136-db77-4b28-b682-9d27c438f91b",
      "employeeIdentification": "71234567",
      "employeeName": "Carlos Pérez Mejía",
      "accessPointId": "ca7f515c-9a94-4fed-bfb2-537dfc816cca",
      "accessPointName": "Parqueadero empleados",
      "rejectionMessage": null
    }
  ],
  "page": 1,
  "pageSize": 2,
  "totalCount": 10,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 38. Validación: rango de fechas invertido

`GET /api/access-records?fromUtc=2026-10-31T02:36:54Z&toUtc=2026-10-31T02:34:52Z`

Respuesta: **HTTP 400**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Historial.RangoInvalido",
  "status": 400,
  "detail": "La fecha inicial no puede ser posterior a la fecha final.",
  "code": "Historial.RangoInvalido"
}
```
