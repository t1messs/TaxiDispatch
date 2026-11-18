#!/usr/bin/env bash
# Семінар 2, тема 23: відтворення конфлікту злиття за GitFlow на коді TaxiDispatch.
# Працює в тимчасовому клоні — робоча копія й GitHub не змінюються.
#   bash tools/sem2/conflict-demo.sh          — довга гілка feature + hotfix у main → конфлікт
#   bash tools/sem2/conflict-demo.sh resolve  — розв'язання, складання й тести
set -eu

demo="${TMPDIR:-/tmp}/taxidispatch-gitflow"
file="src/TaxiDispatch.Domain/Pricing/FareCalculator.cs"

if [ "${1:-}" = "resolve" ]; then
    cd "$demo"
    # Беремо обидві зміни: змінна trip із hotfix і коефіцієнт surge із feature.
    perl -0pi -e 's/<<<<<<< HEAD\n.*?=======\n.*?>>>>>>> feature\/surge-pricing\n/        decimal trip = tariff.Boarding + (tariff.PerKm * distanceKm);\n        decimal fare = surge * Math.Max(tariff.Minimum, trip);\n/s' "$file"
    git add "$file"
    git commit -q --no-edit
    git log --oneline --graph -8
    dotnet build -c Release -warnaserror -v q
    dotnet test --no-build -c Release -v q
    exit 0
fi

rm -rf "$demo"
git clone -q --branch main "$(git rev-parse --show-toplevel)" "$demo"
cd "$demo"

# GitFlow: develop від main, функція від develop.
git switch -q -c develop
git switch -q -c feature/surge-pricing
perl -pi -e 's/(\/\/\/ <param name="isNight">.*)$/$1\n    \/\/\/ <param name="surge">Коефіцієнт попиту, 1 — без надбавки.<\/param>/;
    s/^    public static decimal Calculate\(Tariff tariff, decimal distanceKm, bool isNight\)$/    public static decimal Calculate(\n        Tariff tariff, decimal distanceKm, bool isNight, decimal surge = 1m)/;
    s/^        decimal fare = Math\.Max\(tariff\.Minimum, (.*)\);$/        decimal fare = surge * Math.Max(\n            tariff.Minimum, $1);/' "$file"
git commit -q -am "feat(pricing): коефіцієнт попиту в Calculate"

# Поки функція живе у своїй гілці, у main виправляють той самий рядок.
git switch -q main
git switch -q -c hotfix/1.0.1
perl -pi -e 's/^        decimal fare = Math\.Max\(tariff\.Minimum, (.*)\);$/        decimal trip = $1;\n        decimal fare = Math.Max(tariff.Minimum, trip);/' "$file"
git commit -q -am "fix(pricing): вартість поїздки — окремою змінною"
git switch -q main
git merge -q --no-ff -m "Merge branch 'hotfix/1.0.1'" hotfix/1.0.1
git tag v1.0.1
git switch -q develop
git merge -q --no-ff -m "Merge branch 'hotfix/1.0.1' into develop" hotfix/1.0.1

# Завершення функції: злиття в develop.
echo "\$ git merge --no-ff feature/surge-pricing"
git merge --no-ff feature/surge-pricing || true
echo
git status --short
echo
sed -n '/public static decimal Calculate/,/^    }/p' "$file"
