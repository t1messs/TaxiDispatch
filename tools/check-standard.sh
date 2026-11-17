#!/usr/bin/env bash
# Перевірки стандарту кодування, яких немає в аналізаторах Roslyn (CODING_STANDARD.md, розділ 6).
# Запуск: bash tools/check-standard.sh   Код завершення 0 — порушень немає, 1 — є.
set -u

failed=0

report() {
    # $1 — правило, $2 — опис, решта — знайдені рядки
    local rule="$1" text="$2" found="$3"
    if [ -n "$found" ]; then
        echo "error $rule: $text"
        echo "$found" | sed 's/^/    /'
        failed=1
    fi
}

# Код, на який поширюється стандарт: усі проєкти src/ і tests/.
files=$(git ls-files 'src/*.cs' 'tests/*.cs')
# Бібліотеки логіки: домен і прикладний шар (без інфраструктури та консолі).
core=$(git ls-files 'src/TaxiDispatch.Domain/*.cs' 'src/TaxiDispatch.Application/*.cs')
libs=$(git ls-files 'src/TaxiDispatch.Domain/*.cs' 'src/TaxiDispatch.Application/*.cs' \
    'src/TaxiDispatch.Infrastructure/*.cs')

# F-03: рядок не довший за 100 символів. Рахуємо символи, а не байти: awk на macOS вважає
# кожну кириличну літеру за два символи, тому довжину рахує perl із декодуванням UTF-8.
report F-03 "рядок довший за 100 символів (файл:рядок: довжина)" \
    "$(perl -CSD -ne 'chomp; print "$ARGV:$.: ", length($_), "\n" if length($_) > 100;
        close ARGV if eof' $files)"

# S-13: файл не довший за 500 рядків
report S-13 "файл довший за 500 рядків" \
    "$(wc -l $files | awk '$2 != "total" && $1 > 500 { print $2 ": " $1 " рядків" }')"

# A-05: точкове придушення директивою заборонено
report A-05 "#pragma warning disable замість .editorconfig або SuppressMessage" \
    "$(grep -n '#pragma warning disable' $files)"

# S-05: goto заборонено
report S-05 "оператор goto" "$(grep -nw 'goto' $files)"

# C-05: TODO лише з номером запису реєстру боргу
report C-05 "TODO без номера запису TD" "$(grep -n 'TODO' $files | grep -v 'TODO(TD-')"

# C-02: закоментований код (евристика: коментар, що закінчується на «;»)
report C-02 "схоже на закоментований код" "$(grep -nE '^[[:space:]]*//[^/].*;[[:space:]]*$' $files)"

# D-14, X-04: double і float у логіці — лише в src/TaxiDispatch.Domain/Geo
report D-14 "double або float у домені чи прикладному шарі поза Geo/" \
    "$(grep -nwE 'double|float' $(echo "$core" | grep -v '^src/TaxiDispatch.Domain/Geo/'))"

# D-15: округлення грошей лише з MidpointRounding.AwayFromZero
report D-15 "Round без MidpointRounding.AwayFromZero" \
    "$(grep -n 'Round(' $libs | grep -v 'MidpointRounding.AwayFromZero')"

if [ "$failed" -eq 0 ]; then
    echo "check-standard: порушень не знайдено"
fi
exit "$failed"
