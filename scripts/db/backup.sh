#!/bin/bash
# ============================================================
# Voxera Database Backup Script
# Usage: ./scripts/db/backup.sh
# ============================================================

set -euo pipefail

BACKUP_DIR="/var/backups/voxera"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/voxera_${TIMESTAMP}.sql.gz"
RETENTION_DAYS=30

mkdir -p "${BACKUP_DIR}"

echo "📦 Creating database backup: ${BACKUP_FILE}"

# Create backup
docker-compose exec -T postgres pg_dump \
    -U voxera \
    -d voxera \
    --no-password \
    --format=custom \
    --compress=9 \
    | gzip > "${BACKUP_FILE}"

echo "✅ Backup created: ${BACKUP_FILE} ($(du -sh ${BACKUP_FILE} | cut -f1))"

# Remove old backups
find "${BACKUP_DIR}" -name "voxera_*.sql.gz" -mtime +${RETENTION_DAYS} -delete
echo "🗑️  Cleaned up backups older than ${RETENTION_DAYS} days"

# Optional: Upload to S3
# aws s3 cp "${BACKUP_FILE}" "s3://voxera-backups/db/${TIMESTAMP}.sql.gz"
