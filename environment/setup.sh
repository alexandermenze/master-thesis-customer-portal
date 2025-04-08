#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

export LC_ALL=C.UTF-8

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if ! command -v kind &>/dev/null; then
    echo "Bitte installiere kind von https://kind.sigs.k8s.io/docs/user/quick-start/"
    exit 1
fi

if ! command -v helm &>/dev/null; then
    echo "Bitte installiere helm von https://helm.sh/docs/intro/install/"
    exit 1
fi

if ! command -v kubectl &>/dev/null; then
    echo "Bitte installiere kubectl von https://kubernetes.io/docs/tasks/tools/install-kubectl/"
    exit 1
fi

clusterName="app-customer-portal-env"
context="kind-${clusterName}"

if ! kind get clusters | grep -qx "${clusterName}"; then
    echo "⚙️  Erstelle Cluster ${clusterName} ..."
    kind create cluster --name "${clusterName}" --config "${SCRIPT_DIR}/kind-config.yaml"
fi

echo "⚙️  Einrichtung von PostgreSQL ..."

helm upgrade postgres oci://registry-1.docker.io/bitnamicharts/postgresql \
    -f "${SCRIPT_DIR}/applications/postgres/values.yaml" --version "16.*" --install --kube-context "${context}"

echo "⚙️  Einrichtung von Redis ..."

helm upgrade redis oci://registry-1.docker.io/bitnamicharts/redis \
    -f "${SCRIPT_DIR}/applications/redis/values.yaml" --version "20.*" --install --kube-context "${context}"

echo "⚙️  Einrichtung von MinIO ..."

helm upgrade minio oci://registry-1.docker.io/bitnamicharts/minio \
    -f "${SCRIPT_DIR}/applications/minio/values.yaml" --version "16.*" --install --kube-context "${context}"

echo "✅ Cluster ${clusterName} ist bereit"