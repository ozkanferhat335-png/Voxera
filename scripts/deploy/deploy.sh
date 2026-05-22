#!/bin/bash
# ============================================================
# Voxera Production Deployment Script
# Usage: ./scripts/deploy/deploy.sh [environment]
# ============================================================

set -euo pipefail

ENVIRONMENT=${1:-production}
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
REGISTRY="registry.voxera.io"
VERSION=$(git describe --tags --always --dirty 2>/dev/null || echo "dev-${TIMESTAMP}")

echo "🚀 Deploying Voxera v${VERSION} to ${ENVIRONMENT}"

# Load environment variables
if [ -f ".env.${ENVIRONMENT}" ]; then
    export $(cat ".env.${ENVIRONMENT}" | grep -v '^#' | xargs)
fi

# Build Docker images
echo "📦 Building Docker images..."
docker build -t "${REGISTRY}/voxera-api:${VERSION}" -f deploy/docker/Dockerfile.api .
docker build -t "${REGISTRY}/voxera-worker:${VERSION}" -f deploy/docker/Dockerfile.worker .
docker build -t "${REGISTRY}/voxera-webpanel:${VERSION}" -f src/Voxera.WebPanel/Dockerfile src/Voxera.WebPanel/

# Tag as latest
docker tag "${REGISTRY}/voxera-api:${VERSION}" "${REGISTRY}/voxera-api:latest"
docker tag "${REGISTRY}/voxera-worker:${VERSION}" "${REGISTRY}/voxera-worker:latest"
docker tag "${REGISTRY}/voxera-webpanel:${VERSION}" "${REGISTRY}/voxera-webpanel:latest"

# Push to registry
echo "📤 Pushing to registry..."
docker push "${REGISTRY}/voxera-api:${VERSION}"
docker push "${REGISTRY}/voxera-api:latest"
docker push "${REGISTRY}/voxera-worker:${VERSION}"
docker push "${REGISTRY}/voxera-webpanel:${VERSION}"

if [ "${ENVIRONMENT}" == "kubernetes" ]; then
    # Kubernetes deployment
    echo "☸️  Deploying to Kubernetes..."
    kubectl apply -f deploy/kubernetes/namespace.yaml
    kubectl apply -f deploy/kubernetes/secrets.yaml
    kubectl apply -f deploy/kubernetes/api-deployment.yaml
    kubectl apply -f deploy/kubernetes/ingress.yaml
    kubectl set image deployment/voxera-api api="${REGISTRY}/voxera-api:${VERSION}" -n voxera
    kubectl rollout status deployment/voxera-api -n voxera
else
    # Docker Compose deployment
    echo "🐳 Deploying with Docker Compose..."
    docker-compose pull
    docker-compose up -d --remove-orphans
    docker-compose exec api dotnet ef database update
fi

echo "✅ Deployment complete! Version: ${VERSION}"
echo "📊 API: https://${API_DOMAIN:-api.voxera.io}/swagger"
echo "🌐 Web: https://${APP_DOMAIN:-app.voxera.io}"
