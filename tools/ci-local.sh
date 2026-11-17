#!/usr/bin/env bash
# Локальний еквівалент конвеєра .github/workflows/ci.yml: ті самі кроки й пороги.
# Запуск із кореня репозиторію: bash tools/ci-local.sh
set -euo pipefail

dotnet restore
dotnet format --verify-no-changes --no-restore
bash tools/check-standard.sh
dotnet build -c Release --no-restore -warnaserror
dotnet test -c Release --no-build \
    /p:CollectCoverage=true /p:CoverletOutput="$PWD/coverage/" /p:Threshold=70
echo "ci-local: усі критерії якості виконано"
