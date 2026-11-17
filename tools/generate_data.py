"""Генерує умовні дані TaxiDispatch у теці data/.

Усі імена, телефони й номерні знаки вигадані (випадкові поєднання), координати — точки
Києва. Генератор детермінований: той самий seed дає ті самі файли. Вартість поїздок
в історії рахується за правилом 4.1 специфікації — так само, як FareCalculator.

Запуск із кореня репозиторію: python3 tools/generate_data.py
"""
import json
import math
import random
from datetime import datetime, timedelta, timezone
from decimal import ROUND_HALF_UP, Decimal
from pathlib import Path

SEED = 13
EARTH_RADIUS_KM = 6371.0088
ROAD_FACTOR = Decimal("1.3")
NIGHT_SURCHARGE = Decimal("1.25")
KYIV = timezone(timedelta(hours=2))
TARIFFS = {
    "Economy": (Decimal(40), Decimal(12), Decimal(90)),
    "Comfort": (Decimal(55), Decimal(16), Decimal(120)),
    "Business": (Decimal(90), Decimal(26), Decimal(220)),
}
PLACES = [
    ("Центральний залізничний вокзал", 50.4406, 30.4890),
    ("Аеропорт «Київ» (Жуляни)", 50.4017, 30.4497),
    ("Майдан Незалежності", 50.4501, 30.5234),
    ("Контрактова площа", 50.4651, 30.5154),
    ("Бессарабська площа", 50.4418, 30.5212),
    ("Палац спорту", 50.4385, 30.5215),
    ("НСК «Олімпійський»", 50.4334, 30.5216),
    ("Лук'янівська площа", 50.4623, 30.4823),
    ("Південний вокзал", 50.4372, 30.4800),
    ("Деміївська площа", 50.4058, 30.5175),
    ("Видубицький вокзал", 50.4064, 30.5598),
    ("Лівобережна", 50.4520, 30.5980),
    ("Вокзал «Дарниця»", 50.4545, 30.6305),
    ("Позняки", 50.3980, 30.6340),
    ("Осокорки", 50.3953, 30.6160),
    ("Харківська площа", 50.4005, 30.6520),
    ("Троєщина", 50.5100, 30.6040),
    ("Оболонська набережна", 50.5010, 30.5040),
    ("Мінська площа", 50.5125, 30.4985),
    ("Героїв Дніпра", 50.5225, 30.4985),
    ("Виноградар", 50.5080, 30.4300),
    ("Нивки", 50.4585, 30.4040),
    ("Святошин", 50.4580, 30.3910),
    ("Академмістечко", 50.4650, 30.3550),
    ("Борщагівка", 50.4250, 30.3700),
    ("Відрадний", 50.4290, 30.4230),
    ("Севастопольська площа", 50.4260, 30.4740),
    ("Експоцентр", 50.3810, 30.4770),
    ("Голосіївський парк", 50.3870, 30.5100),
    ("Теремки", 50.3670, 30.4540),
    ("Либідська площа", 50.4130, 30.5250),
    ("Печерська лавра", 50.4346, 30.5573),
    ("Арсенальна", 50.4446, 30.5455),
    ("Поштова площа", 50.4590, 30.5250),
    ("Андріївський узвіз", 50.4590, 30.5160),
    ("Золоті ворота", 50.4485, 30.5135),
    ("Площа Перемоги", 50.4505, 30.4930),
    ("Шулявська", 50.4550, 30.4450),
    ("КПІ", 50.4485, 30.4570),
    ("Дорогожичі", 50.4730, 30.4490),
    ("Сирець", 50.4760, 30.4310),
    ("Петрівка", 50.4860, 30.4980),
    ("Гідропарк", 50.4460, 30.5770),
    ("Русанівка", 50.4410, 30.6000),
    ("Березняки", 50.4250, 30.6050),
    ("Дарницька площа", 50.4430, 30.6150),
    ("Лісовий масив", 50.4650, 30.6440),
    ("Чернігівська", 50.4600, 30.6300),
    ("Броварський проспект", 50.4590, 30.6120),
    ("Червоний хутір", 50.4100, 30.6800),
]
MALE = ["Андрій", "Олександр", "Іван", "Тарас", "Богдан", "Дмитро", "Максим", "Сергій",
        "Василь", "Ігор", "Олег", "Роман", "Юрій", "Петро", "Микола", "Віктор"]
FEMALE = ["Олена", "Ірина", "Наталія", "Оксана", "Марія", "Тетяна", "Ганна", "Юлія",
          "Світлана", "Катерина", "Людмила", "Софія"]
SURNAMES = ["Коваленко", "Бондаренко", "Ткаченко", "Шевчук", "Кравченко", "Мельник",
            "Олійник", "Савченко", "Руденко", "Мороз", "Лисенко", "Марченко", "Гончаренко",
            "Поліщук", "Литвиненко", "Кузьменко", "Павленко", "Романенко",
            "Клименко", "Остапенко", "Гнатюк", "Дячук", "Зінченко", "Сидоренко"]
MODELS = {
    "Economy": ["Skoda Fabia", "Hyundai Accent", "Kia Rio", "Renault Logan", "Toyota Yaris"],
    "Comfort": ["Skoda Octavia", "Toyota Corolla", "Kia Ceed", "Hyundai Elantra"],
    "Business": ["Toyota Camry", "Mercedes-Benz E-Class", "BMW 5 Series", "Audi A6"],
}
CLASSES = ["Economy"] * 30 + ["Comfort"] * 18 + ["Business"] * 12

rnd = random.Random(SEED)


def km_between(a, b):
    lat1, lat2 = math.radians(a[0]), math.radians(b[0])
    dlon = math.radians(b[1] - a[1])
    h = math.sin((lat2 - lat1) / 2) ** 2 + math.cos(lat1) * math.cos(lat2) * math.sin(dlon / 2) ** 2
    angle = 2 * math.asin(math.sqrt(min(1.0, h)))
    # Як (decimal)double у .NET: 15 значущих цифр, потім округлення до метра.
    return Decimal(f"{EARTH_RADIUS_KM * angle:.15g}").quantize(Decimal("0.001"), ROUND_HALF_UP)


def route_km(a, b):
    return (km_between(a, b) * ROAD_FACTOR).quantize(Decimal("0.001"), ROUND_HALF_UP)


def fare(car_class, km, started):
    boarding, per_km, minimum = TARIFFS[car_class]
    value = max(minimum, boarding + per_km * km)
    if started.hour >= 22 or started.hour < 6:
        value *= NIGHT_SURCHARGE
    return value.quantize(Decimal("0.01"), ROUND_HALF_UP)


def name():
    first = rnd.choice(MALE + FEMALE)
    return f"{first} {rnd.choice(SURNAMES)}"


def point(lat, lon):
    return {"latitude": lat, "longitude": lon}


def place_json(i):
    title, lat, lon = PLACES[i]
    return {"code": f"P{i + 1:02d}", "name": title, "location": point(lat, lon)}


def iso(moment):
    return moment.isoformat()


def build_drivers():
    drivers = []
    for i, car_class in enumerate(CLASSES, start=1):
        lat = round(rnd.uniform(50.38, 50.52), 4)
        lon = round(rnd.uniform(30.37, 30.66), 4)
        plate = f"{rnd.choice(['AA', 'KA'])}{rnd.randint(1000, 9999)}{rnd.choice(['BC', 'XE', 'MP', 'TO', 'HK'])}"
        drivers.append({
            "profile": {
                "id": i,
                "name": name(),
                "vehicle": {"plate": plate, "model": rnd.choice(MODELS[car_class]), "carClass": car_class},
                "rating": float(Decimal(rnd.randint(41, 50)) / 10),
            },
            "location": point(lat, lon),
            "status": "OffShift" if i % 6 == 0 else "Free",
        })
    return drivers


def build_passengers():
    phones = set()
    passengers = []
    while len(passengers) < 80:
        phone = f"+380{rnd.choice(['50', '63', '66', '67', '68', '73', '93', '95', '97', '99'])}{rnd.randint(0, 9999999):07d}"
        if phone in phones:
            continue
        phones.add(phone)
        passengers.append({"id": len(passengers) + 1, "name": name(), "phone": phone})
    return passengers


def build_orders(drivers):
    orders = []
    start = datetime(2026, 11, 14, 7, 0, tzinfo=KYIV)
    for order_id in range(1, 121):
        created = start + timedelta(minutes=order_id * 42 + rnd.randint(0, 30))
        pickup_i, dropoff_i = rnd.sample(range(len(PLACES)), 2)
        pickup, dropoff = PLACES[pickup_i][1:], PLACES[dropoff_i][1:]
        car_class = rnd.choices(["Economy", "Comfort", "Business"], [6, 3, 1])[0]
        plan = {"pickup": place_json(pickup_i), "dropoff": place_json(dropoff_i),
                "carClass": car_class, "routeKm": float(route_km(pickup, dropoff))}
        order = {"id": order_id, "passengerId": rnd.randint(1, 80), "plan": plan,
                 "createdAt": iso(created), "status": "New", "assignment": None,
                 "startedAt": None, "result": None, "cancellation": None}
        if order_id > 115:
            orders.append(order)  # останні п'ять — нові, для демонстрації UC-02
            continue
        driver = rnd.choice([d for d in drivers if d["profile"]["vehicle"]["carClass"] == car_class])
        assigned = created + timedelta(minutes=2)
        order["assignment"] = {"driverId": driver["profile"]["id"],
                               "pickupKm": float(Decimal(rnd.randint(300, 4900)) / 1000),
                               "assignedAt": iso(assigned)}
        if order_id % 8 == 0:
            order["status"] = "Cancelled"
            order["cancellation"] = {"reason": rnd.choice(["пасажир передумав", "не вийшов до авто",
                                                           "змінилися плани"]),
                                     "cancelledAt": iso(assigned + timedelta(minutes=5))}
            orders.append(order)
            continue
        started = assigned + timedelta(minutes=rnd.randint(4, 14))
        actual = (Decimal(plan["routeKm"]).quantize(Decimal("0.001")) * Decimal(rnd.randint(100, 112)) / 100
                  ).quantize(Decimal("0.001"), ROUND_HALF_UP)
        completed = started + timedelta(minutes=int(actual * 2) + 5)
        order["status"] = "Completed"
        order["startedAt"] = iso(started)
        order["result"] = {"actualKm": float(actual), "fare": float(fare(car_class, actual, started)),
                           "completedAt": iso(completed)}
        orders.append(order)
    return orders


def write(name_, value):
    path = Path("data") / name_
    path.parent.mkdir(exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"{path}: {len(value)} записів")


def main():
    drivers = build_drivers()
    write("places.json", [place_json(i) for i in range(len(PLACES))])
    write("drivers.json", drivers)
    write("passengers.json", build_passengers())
    write("tariffs.json", [{"carClass": c, "boarding": float(b), "perKm": float(k), "minimum": float(m)}
                           for c, (b, k, m) in TARIFFS.items()])
    write("orders.json", build_orders(drivers))


if __name__ == "__main__":
    main()
