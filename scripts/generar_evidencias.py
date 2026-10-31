"""
Ejecuta los casos de uso del SGCAE contra la API en ejecución y genera
docs/EVIDENCIAS.md con cada petición y la respuesta real obtenida.

Uso:
    python scripts/generar_evidencias.py [url_base]

Requiere la API levantada (por defecto en http://localhost:5009) sobre una
base de datos recién creada para que los resultados sean reproducibles.
Solo usa la biblioteca estándar de Python.
"""
import json
import sys
import urllib.error
import urllib.request
from datetime import datetime, timedelta, timezone
from pathlib import Path

BASE = sys.argv[1].rstrip("/") if len(sys.argv) > 1 else "http://localhost:5009"
OUT = Path(__file__).resolve().parent.parent / "docs" / "EVIDENCIAS.md"

lines: list[str] = []
counter = 0


def call(method: str, path: str, body=None):
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(BASE + path, data=data, method=method,
                                 headers={"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req) as resp:
            status, raw = resp.status, resp.read()
    except urllib.error.HTTPError as err:
        status, raw = err.code, err.read()
    payload = json.loads(raw) if raw else None
    return status, payload


def step(title: str, method: str, path: str, body=None, expect: int | None = None, note: str | None = None):
    """Ejecuta una petición, la documenta y verifica el código HTTP esperado."""
    global counter
    counter += 1
    status, payload = call(method, path, body)
    ok = expect is None or status == expect
    lines.append(f"### {counter}. {title}\n")
    if note:
        lines.append(f"{note}\n")
    lines.append(f"`{method} {path}`\n")
    if body is not None:
        lines.append("Cuerpo de la petición:\n")
        lines.append("```json\n" + json.dumps(body, indent=2, ensure_ascii=False) + "\n```\n")
    lines.append(f"Respuesta: **HTTP {status}**" + ("" if ok else f" (se esperaba {expect})") + "\n")
    if payload is not None:
        shown = dict(payload) if isinstance(payload, dict) else payload
        if isinstance(shown, dict) and "traceId" in shown:
            shown.pop("traceId")
        lines.append("```json\n" + json.dumps(shown, indent=2, ensure_ascii=False) + "\n```\n")
    if not ok:
        raise SystemExit(f"Paso {counter} ({title}): se esperaba HTTP {expect} y se obtuvo {status}: {payload}")
    return payload


def section(title: str, text: str = ""):
    lines.append(f"\n## {title}\n")
    if text:
        lines.append(text + "\n")


def access(code: str, point: str, movement: str, expected_result: str, title: str, expected_reason: str | None = None):
    record = step(title, "POST", "/api/access-records",
                  {"credentialCode": code, "accessPointCode": point, "movementType": movement}, 201)
    assert record["result"] == expected_result, record
    assert record["rejectionReason"] == expected_reason, record
    return record


def main():
    start = datetime.now(timezone.utc) - timedelta(minutes=1)

    section("RF-06. Puntos de acceso",
            "Al iniciar en Development la API aplica las migraciones y registra cuatro puntos de acceso base.")
    points = step("Listar puntos de acceso iniciales", "GET", "/api/access-points", expect=200)
    recepcion = next(p for p in points["items"] if p["code"] == "RECEPCION")
    sala = step("Registrar un nuevo punto de acceso", "POST", "/api/access-points",
                {"code": "SALA-JUNTAS", "name": "Sala de juntas", "type": "RestrictedArea", "location": "Piso 2"}, 201)
    step("Rechazo de código de punto duplicado", "POST", "/api/access-points",
         {"code": "sala-juntas", "name": "Otra sala", "type": "Other"}, 409)
    step("Deshabilitar el punto de acceso", "POST", f"/api/access-points/{sala['id']}/disable", expect=200)
    step("Consultar solo los puntos habilitados", "GET", "/api/access-points?isEnabled=true", expect=200)

    section("RF-01. Gestión de empleados")
    laura = step("Registrar empleado", "POST", "/api/employees", {
        "identification": "1035123456", "firstNames": "Laura", "lastNames": "Gómez Restrepo",
        "email": "laura.gomez@logistica.com", "position": "Analista de inventario", "area": "Bodega"}, 201)
    carlos = step("Registrar segundo empleado", "POST", "/api/employees", {
        "identification": "71234567", "firstNames": "Carlos", "lastNames": "Pérez Mejía",
        "email": "carlos.perez@logistica.com", "position": "Conductor", "area": "Transporte"}, 201)
    step("Validación: correo con formato inválido", "POST", "/api/employees", {
        "identification": "99887766", "firstNames": "Ana", "lastNames": "Ruiz",
        "email": "ana-ruiz", "position": "Auxiliar", "area": "Bodega"}, 400)
    step("Validación: identificación duplicada", "POST", "/api/employees", {
        "identification": "1035123456", "firstNames": "Otra", "lastNames": "Persona",
        "email": "otra@logistica.com", "position": "Auxiliar", "area": "Bodega"}, 409)
    step("Consultar empleado por id", "GET", f"/api/employees/{laura['id']}", expect=200)
    step("Actualizar datos del empleado", "PUT", f"/api/employees/{laura['id']}", {
        "firstNames": "Laura", "lastNames": "Gómez Restrepo", "email": "laura.gomez@logistica.com",
        "position": "Coordinadora de inventario", "area": "Bodega"}, 200)
    step("Buscar empleados con paginación", "GET", "/api/employees?search=g%C3%B3mez&page=1&pageSize=10", expect=200)

    section("RF-02. Gestión de credenciales")
    cred_laura = step("Asignar credencial con código definido", "POST", "/api/credentials",
                      {"employeeId": laura["id"], "code": "CRED-0001"}, 201)
    cred_carlos = step("Asignar credencial con código generado automáticamente", "POST", "/api/credentials",
                       {"employeeId": carlos["id"]}, 201)
    step("Regla: un empleado no puede tener dos credenciales vigentes", "POST", "/api/credentials",
         {"employeeId": laura["id"], "code": "CRED-0002"}, 409)
    step("Consultar el estado de una credencial por código", "GET", "/api/credentials/by-code/cred-0001", expect=200)

    section("RF-03. Registro de entradas y salidas",
            "Cada solicitud genera un registro, autorizado o rechazado, con el motivo del rechazo.")
    access("CRED-0001", "RECEPCION", "Entry", "Authorized", "Entrada autorizada")
    access("CRED-0001", "RECEPCION", "Entry", "Rejected", "Entrada duplicada rechazada (ya está dentro)", "AlreadyInside")
    access("CRED-0001", "RECEPCION", "Exit", "Authorized", "Salida autorizada")
    access("CRED-0001", "RECEPCION", "Exit", "Rejected", "Salida sin entrada previa rechazada", "NotInside")
    access("CRED-9999", "RECEPCION", "Entry", "Rejected", "Credencial inexistente", "CredentialNotFound")
    access("CRED-0001", "PUERTA-X", "Entry", "Rejected", "Punto de acceso inexistente", "AccessPointNotFound")
    access("CRED-0001", "SALA-JUNTAS", "Entry", "Rejected", "Punto de acceso deshabilitado", "AccessPointDisabled")

    step("Bloquear credencial", "POST", f"/api/credentials/{cred_carlos['id']}/block",
         {"reason": "Reporte de pérdida del carné"}, 200)
    access(cred_carlos["code"], "PARQUEADERO", "Entry", "Rejected", "Ingreso con credencial bloqueada", "CredentialBlocked")
    step("Desbloquear credencial", "POST", f"/api/credentials/{cred_carlos['id']}/unblock", expect=200)
    access(cred_carlos["code"], "PARQUEADERO", "Entry", "Authorized", "Ingreso tras desbloqueo")

    step("Desactivar empleado (revoca sus credenciales)", "POST", f"/api/employees/{carlos['id']}/deactivate", expect=200)
    step("La credencial quedó revocada", "GET", f"/api/credentials/{cred_carlos['id']}", expect=200)
    access(cred_carlos["code"], "PARQUEADERO", "Exit", "Rejected",
           "Empleado desactivado: su credencial ya no permite movimientos", "CredentialRevoked")
    step("Regla: no se asignan credenciales a empleados inactivos", "POST", "/api/credentials",
         {"employeeId": carlos["id"]}, 409)
    step("Validación: tipo de movimiento inválido", "POST", "/api/access-records",
         {"credentialCode": "CRED-0001", "accessPointCode": "RECEPCION", "movementType": "Salto"}, 400)

    section("RF-04. Consulta del historial de accesos")
    end = (datetime.now(timezone.utc) + timedelta(minutes=1)).strftime("%Y-%m-%dT%H:%M:%SZ")
    begin = start.strftime("%Y-%m-%dT%H:%M:%SZ")
    step("Historial por empleado, paginado (página 1 de tamaño 3)", "GET",
         f"/api/access-records?employeeId={laura['id']}&page=1&pageSize=3", expect=200)
    step("Historial por empleado, página 2", "GET",
         f"/api/access-records?employeeId={laura['id']}&page=2&pageSize=3", expect=200)
    step("Solo accesos rechazados", "GET", "/api/access-records?result=Rejected&pageSize=5", expect=200)
    step("Por punto de acceso y resultado", "GET",
         f"/api/access-records?accessPointId={recepcion['id']}&result=Authorized", expect=200)
    step("Por rango de fechas", "GET", f"/api/access-records?fromUtc={begin}&toUtc={end}&pageSize=2", expect=200)
    step("Validación: rango de fechas invertido", "GET",
         f"/api/access-records?fromUtc={end}&toUtc={begin}", expect=400)

    header = [
        "# Evidencias de funcionamiento - SGCAE\n",
        f"Generado automáticamente por `scripts/generar_evidencias.py` contra `{BASE}` "
        f"el {datetime.now(timezone.utc):%Y-%m-%d %H:%M} UTC, sobre una base de datos SQL Server recién creada.\n",
        "Cada paso muestra la petición enviada y la respuesta real de la API. El script se detiene si algún "
        "código HTTP o resultado no coincide con el esperado, de modo que este documento solo se genera "
        f"cuando los {counter} pasos se comportan como se espera.\n",
    ]
    OUT.parent.mkdir(exist_ok=True)
    OUT.write_text("\n".join(header + lines), encoding="utf-8")
    print(f"{counter} pasos verificados. Evidencias en {OUT}")


if __name__ == "__main__":
    main()
