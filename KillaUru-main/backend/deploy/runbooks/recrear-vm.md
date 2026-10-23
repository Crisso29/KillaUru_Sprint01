# Runbook · Recrear la VM desde cero (≤ 4 h, RNF-24)

Para cuando Oracle recupera la instancia o la VM se pierde (RS-01).

1. Crear una VM A1 Always Free (4 OCPU / 24 GB, Ubuntu LTS arm64) en Santiago.
2. Instalar Docker y `cloudflared`; **no** abrir puertos en la lista de seguridad (todo entra por el túnel).
3. Clonar el repositorio en `/opt/killauru`.
4. Recuperar los secretos de su copia cifrada fuera de la VM (`secretos.md`) y escribir `deploy/.env`.
5. Restaurar la base con `restaurar.md`.
6. `./desplegar.sh <sha-de-la-última-versión-buena>`.
7. Volver a registrar la llave SSH de despliegue en Cloudflare Access si cambió el host.
8. Verificar: `/health/ready` = 200, Grafana recibe métricas y logs, UptimeRobot en verde.

Anotar el tiempo total en `simulacros.md`.
