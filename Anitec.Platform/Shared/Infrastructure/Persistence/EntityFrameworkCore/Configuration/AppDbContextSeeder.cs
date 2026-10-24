using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Analytics.Domain.Model.Entities;
using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Devices.Domain.Model.Entities;
using Anitec.Platform.Financial.Domain.Model.Entities;
using Anitec.Platform.Iam.Application.Internal.OutboundServices;
using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Metrics.Domain.Model.Entities;
using Anitec.Platform.Sanitary.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public static class AppDbContextSeeder
{
    /// <summary>
    ///     Adds the subscription plans when the table is empty. The plans are catalog data that every
    ///     environment needs, so unlike the demo users this also runs in production and never overwrites
    ///     existing plans.
    /// </summary>
    public static async Task SeedSubscriptionPlansAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Set<SubscriptionPlan>().AnyAsync(cancellationToken))
            return;

        await context.Set<SubscriptionPlan>().AddRangeAsync(DefaultPlans(), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>The plans offered to ranchers and veterinarians. The checkout builds the Stripe price from these values.</summary>
    private static List<SubscriptionPlan> DefaultPlans() =>
    [
        new() { Name = "Ganadero Basico", Price = 49, StripePriceId = "mock_price_rancher_basic", MaxAnimals = 80, IsActive = true },
        new() { Name = "Ganadero Pro", Price = 99, StripePriceId = "mock_price_rancher_pro", MaxAnimals = 250, IsActive = true },
        new() { Name = "Veterinario Profesional", Price = 129, StripePriceId = "mock_price_vet_pro", MaxAnimals = 1000, IsActive = true }
    ];

    /// <summary>
    ///     One group of similar animals inside a corral. [NameBase] names them "NameBase-001", "NameBase-002"...;
    ///     without it they take a name from [Names]. The last [Archived] animals of the group start archived.
    /// </summary>
    private sealed record Lot(
        string Herd, string Corral, string Prefix, string Species, string[] Breeds, string[] Names,
        decimal Weight, int Count, string? NameBase = null, int Archived = 0);

    /// <summary>
    ///     Demo data for development: seven users, eleven farms with their corrals and about 180 animals, three
    ///     ranchers with a lot of data (ganadero, maria, jose), a smaller fourth one (rosa) and the veterinarians
    ///     that attend them. Dates are relative to today so there are always upcoming activities and follow-ups.
    ///     It only runs on an empty database.
    /// </summary>
    public static async Task SeedDevelopmentDataAsync(
        AppDbContext context,
        IHashingService hashingService,
        CancellationToken cancellationToken = default)
    {
        if (await context.Set<User>().AnyAsync(cancellationToken))
            return;

        var password = hashingService.HashPassword("anitec123");

        var users = new List<User>
        {
            new("ganadero", password, "Carlos Mendoza", "Rancher"),
            new("veterinaria", password, "Dra. Ana Lopez", "Veterinarian"),
            new("maria", password, "Maria Gonzales", "Rancher"),
            new("jose", password, "Jose Quispe", "Rancher"),
            new("vetpedro", password, "Dr. Pedro Ramirez", "Veterinarian"),
            new("rosa", password, "Rosa Huaman", "Rancher"),
            new("vetlucia", password, "Dra. Lucia Torres", "Veterinarian")
        };

        await context.Set<User>().AddRangeAsync(users, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var ganadero = users[0];
        var veterinaria = users[1];
        var maria = users[2];
        var jose = users[3];
        var vetPedro = users[4];
        var rosa = users[5];
        var vetLucia = users[6];

        // ---------- Farms ----------
        var herds = new List<Herd>
        {
            Farm("Hato Los Alamos", "Cajamarca", ganadero, veterinaria, "Mixto"),
            Farm("Granja El Molino", "Cajamarca", ganadero, veterinaria, "Aves"),
            Farm("Establo Pampa Verde", "Cajamarca", ganadero, veterinaria, "Bovino"),
            Farm("Finca Santa Rosa", "Junin", maria, veterinaria, "Ovino y caprino"),
            Farm("Establo La Esperanza", "Junin", maria, veterinaria, "Bovino"),
            Farm("Granja Los Pinos", "Junin", maria, veterinaria, "Mixto"),
            Farm("Fundo Alto Verde", "Cusco", jose, vetPedro, "Mixto"),
            Farm("Corral San Miguel", "Cusco", jose, vetPedro, "Porcino"),
            Farm("Finca Valle Sagrado", "Cusco", jose, vetPedro, "Bovino y equino"),
            Farm("Finca Las Lomas", "Ayacucho", rosa, vetLucia, "Mixto"),
            Farm("Avicola Sol Andino", "Ayacucho", rosa, vetLucia, "Aves")
        };

        await context.Set<Herd>().AddRangeAsync(herds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        var herdByName = herds.ToDictionary(herd => herd.Name);

        // ---------- Corrals ----------
        var corralNames = new (string Herd, string[] Corrals)[]
        {
            ("Hato Los Alamos", ["Corral Norte", "Corral Sur", "Establo Lechero", "Potrero Alto"]),
            ("Granja El Molino", ["Galpon 1", "Galpon 2", "Galpon 3"]),
            ("Establo Pampa Verde", ["Establo A", "Establo B", "Corral de Engorde"]),
            ("Finca Santa Rosa", ["Majada Ovina", "Corral Caprino", "Corral de Cria"]),
            ("Establo La Esperanza", ["Establo Principal", "Corral de Terneros", "Corral de Vacas Secas"]),
            ("Granja Los Pinos", ["Chiquero 1", "Chiquero 2", "Galpon de Pollos"]),
            ("Fundo Alto Verde", ["Corral Principal", "Cuadra de Caballos", "Corral de Cria", "Potrero Bajo"]),
            ("Corral San Miguel", ["Chiquero A", "Chiquero B", "Chiquero C"]),
            ("Finca Valle Sagrado", ["Corral Andino", "Cuadra Sagrada", "Potrero Rio"]),
            ("Finca Las Lomas", ["Corral Lomas 1", "Corral Lomas 2", "Majada Lomas"]),
            ("Avicola Sol Andino", ["Galpon Sol 1", "Galpon Sol 2"])
        };

        var corrals = corralNames
            .SelectMany(group => group.Corrals.Select(name => new Corral { Name = name, HerdId = herdByName[group.Herd].Id }))
            .ToList();
        await context.Set<Corral>().AddRangeAsync(corrals, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        var corralId = corrals.ToDictionary(corral => (corral.HerdId, corral.Name), corral => corral.Id);

        // ---------- Animals ----------
        string[] cattle = ["Luna", "Tornado", "Estrella", "Canela", "Maya", "Bruna", "Rosita", "Lucero", "Princesa", "Capitan", "Nevada", "Paloma", "Cholita", "Trueno", "Margarita", "Lola"];
        string[] sheep = ["Nube", "Copito", "Algodon", "Lanita", "Blanquita", "Rizos", "Vellon", "Pelusa"];
        string[] goats = ["Chaska", "Sierra", "Cabri", "Manchita", "Brisa", "Pico", "Savia", "Mota"];
        string[] pigs = ["Moro", "Pecas", "Rosado", "Gordo", "Chancho", "Panchita", "Tocino", "Morrocoy"];
        string[] horses = ["Relampago", "Centella", "Alazan", "Bravo", "Lucero Alado", "Tormenta"];
        string[] cattleBreeds = ["Brown Swiss", "Holstein", "Jersey", "Simmental", "Criollo"];
        string[] sheepBreeds = ["Corriedale", "Junin", "Hampshire"];
        string[] goatBreeds = ["Saanen", "Alpina", "Criolla"];
        string[] pigBreeds = ["Duroc", "Yorkshire", "Landrace"];
        string[] horseBreeds = ["Criollo", "Peruano de Paso"];
        string[] chickenBreeds = ["Rhode Island", "Cobb 500", "Ross 308"];
        string[] duckBreeds = ["Pekin", "Muscovy"];

        var lots = new List<Lot>
        {
            // Carlos Mendoza (ganadero)
            new("Hato Los Alamos", "Corral Norte", "BOV", "Bovino", cattleBreeds, cattle, 410, 6),
            new("Hato Los Alamos", "Corral Sur", "BOV", "Bovino", cattleBreeds, Enumerable.Reverse(cattle).ToArray(), 450, 5, Archived: 1),
            new("Hato Los Alamos", "Establo Lechero", "BOV", "Bovino", ["Brown Swiss", "Holstein"], cattle.Skip(5).ToArray(), 520, 5),
            new("Hato Los Alamos", "Potrero Alto", "EQU", "Equino", horseBreeds, horses, 430, 3),
            new("Granja El Molino", "Galpon 1", "AVE", "Pollo", chickenBreeds, ["Pollo"], 3, 10, NameBase: "polloComercio"),
            new("Granja El Molino", "Galpon 2", "AVE", "Pollo", chickenBreeds, ["Pollo"], 3, 8, NameBase: "polloPostura"),
            new("Granja El Molino", "Galpon 3", "AVE", "Pato", duckBreeds, ["Pato"], 4, 5, NameBase: "pato", Archived: 1),
            new("Establo Pampa Verde", "Establo A", "BOV", "Bovino", cattleBreeds, cattle.Skip(2).ToArray(), 395, 6),
            new("Establo Pampa Verde", "Establo B", "BOV", "Bovino", cattleBreeds, cattle.Skip(7).ToArray(), 380, 6),
            new("Establo Pampa Verde", "Corral de Engorde", "BOV", "Bovino", ["Simmental", "Criollo"], cattle.Skip(9).ToArray(), 560, 5),

            // Maria Gonzales (maria)
            new("Finca Santa Rosa", "Majada Ovina", "OVI", "Ovino", sheepBreeds, sheep, 55, 8),
            new("Finca Santa Rosa", "Corral Caprino", "CAP", "Caprino", goatBreeds, goats, 44, 6),
            new("Finca Santa Rosa", "Corral de Cria", "OVI", "Ovino", sheepBreeds, sheep.Skip(3).ToArray(), 22, 4),
            new("Establo La Esperanza", "Establo Principal", "BOV", "Bovino", ["Jersey", "Holstein"], cattle.Skip(3).ToArray(), 365, 7),
            new("Establo La Esperanza", "Corral de Terneros", "BOV", "Bovino", ["Jersey", "Holstein"], cattle.Skip(10).ToArray(), 120, 5),
            new("Establo La Esperanza", "Corral de Vacas Secas", "BOV", "Bovino", ["Jersey"], cattle.Skip(1).ToArray(), 400, 4),
            new("Granja Los Pinos", "Chiquero 1", "CER", "Porcino", pigBreeds, pigs, 110, 5),
            new("Granja Los Pinos", "Chiquero 2", "CER", "Porcino", pigBreeds, pigs.Skip(2).ToArray(), 95, 5),
            new("Granja Los Pinos", "Galpon de Pollos", "AVE", "Pollo", chickenBreeds, ["Pollo"], 3, 8, NameBase: "polloEngorde"),

            // Jose Quispe (jose)
            new("Fundo Alto Verde", "Corral Principal", "BOV", "Bovino", cattleBreeds, cattle.Skip(4).ToArray(), 390, 6),
            new("Fundo Alto Verde", "Cuadra de Caballos", "EQU", "Equino", horseBreeds, horses.Skip(1).ToArray(), 440, 4),
            new("Fundo Alto Verde", "Corral de Cria", "OVI", "Ovino", sheepBreeds, sheep.Skip(1).ToArray(), 30, 3),
            new("Corral San Miguel", "Chiquero A", "CER", "Porcino", pigBreeds, pigs, 115, 6),
            new("Corral San Miguel", "Chiquero B", "CER", "Porcino", pigBreeds, pigs.Skip(1).ToArray(), 125, 6),
            new("Corral San Miguel", "Chiquero C", "CER", "Porcino", pigBreeds, pigs.Skip(3).ToArray(), 90, 5),
            new("Finca Valle Sagrado", "Corral Andino", "BOV", "Bovino", ["Criollo", "Brown Swiss"], cattle.Skip(6).ToArray(), 370, 6),
            new("Finca Valle Sagrado", "Cuadra Sagrada", "EQU", "Equino", horseBreeds, horses, 420, 3),
            new("Finca Valle Sagrado", "Potrero Rio", "CAP", "Caprino", goatBreeds, goats.Skip(2).ToArray(), 40, 5),

            // Rosa Huaman (rosa): a smaller farm
            new("Finca Las Lomas", "Corral Lomas 1", "BOV", "Bovino", cattleBreeds, cattle.Skip(8).ToArray(), 385, 5),
            new("Finca Las Lomas", "Corral Lomas 2", "OVI", "Ovino", sheepBreeds, sheep, 48, 4),
            new("Finca Las Lomas", "Majada Lomas", "CAP", "Caprino", goatBreeds, goats.Skip(4).ToArray(), 43, 4),
            new("Avicola Sol Andino", "Galpon Sol 1", "AVE", "Pollo", chickenBreeds, ["Pollo"], 3, 8, NameBase: "polloSol"),
            new("Avicola Sol Andino", "Galpon Sol 2", "AVE", "Pato", duckBreeds, ["Pato"], 4, 6, NameBase: "patoLaguna")
        };

        var animals = new List<Animal>();
        var animalLot = new Dictionary<Animal, Lot>();
        var numberByPrefix = new Dictionary<string, int>();
        var timesNameUsed = new Dictionary<(int OwnerId, string Name), int>();
        var counter = 0;
        foreach (var lot in lots)
        {
            var herd = herdByName[lot.Herd];
            for (var i = 0; i < lot.Count; i++)
            {
                counter++;
                var number = numberByPrefix[lot.Prefix] = numberByPrefix.GetValueOrDefault(lot.Prefix) + 1;
                var name = lot.NameBase is not null
                    ? $"{lot.NameBase}-{i + 1:000}"
                    : i < lot.Names.Length ? lot.Names[i] : $"{lot.Names[i % lot.Names.Length]} {i / lot.Names.Length + 1}";
                // A rancher never has two animals with the same name: a repeated one gets a number ("Bruna 2").
                var used = timesNameUsed[(herd.OwnerId, name)] = timesNameUsed.GetValueOrDefault((herd.OwnerId, name)) + 1;
                if (used > 1) name = $"{name} {used}";
                var isBird = lot.Species is "Pollo" or "Pato";
                var status = i >= lot.Count - lot.Archived ? "Archivado"
                    : counter % 29 == 11 ? "Vendido"
                    : counter % 19 == 7 ? "En tratamiento"
                    : counter % 13 == 4 ? "Observacion"
                    : "Saludable";
                var animal = Animal(
                    $"{lot.Prefix}-{number:000}", name, lot.Species, lot.Breeds[i % lot.Breeds.Length],
                    isBird ? (i % 3 == 0 ? "Macho" : "Hembra") : (i % 2 == 0 ? "Hembra" : "Macho"),
                    Day(-(300 + i * 53 % 1500)), lot.Weight + lot.Weight * (i % 6) * 0.04m, status,
                    herd.Id, corralId[(herd.Id, lot.Corral)]);
                animals.Add(animal);
                animalLot[animal] = lot;
            }
        }

        await context.Set<Animal>().AddRangeAsync(animals, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // ---------- Veterinarian clients ----------
        await context.Set<VeterinarianClient>().AddRangeAsync(new[]
        {
            Client(veterinaria.Id, ganadero.Id),
            Client(veterinaria.Id, maria.Id),
            Client(veterinaria.Id, jose.Id),
            Client(vetPedro.Id, jose.Id),
            Client(vetLucia.Id, rosa.Id)
        }, cancellationToken);

        // ---------- Health events ----------
        var vetNameByHerd = herds.ToDictionary(herd => herd.Id, herd => users.First(user => user.Id == herd.VeterinarianId).FullName);
        var healthEvents = new List<HealthEvent>();
        foreach (var animal in animals.Where(animal => animal.Status is "Observacion" or "En tratamiento"))
        {
            var vet = vetNameByHerd[animal.HerdId];
            healthEvents.Add(animal.Status == "En tratamiento"
                ? Health(animal.Id, "Tratamiento", Day(-6 - animal.Id % 5), "Perdida de apetito y decaimiento", vet,
                    "Infeccion leve", "Antibiotico e hidratacion", "Oxitetraciclina cada 24h", "Control en una semana", Day(3 + animal.Id % 7))
                : Health(animal.Id, "Incidencia", Day(-9 - animal.Id % 6), "Cojera leve y comportamiento apatico", vet,
                    "Lesion por golpe", "Reposo y antiinflamatorio", "Meloxicam dosis unica", "Evaluar movilidad", Day(5 + animal.Id % 9)));
        }

        foreach (var group in animals.GroupBy(animal => animalLot[animal]))
        {
            var animal = group.First(candidate => candidate.Status != "Archivado");
            healthEvents.Add(Health(animal.Id, "Vacuna", Day(-40 - animal.Id % 20), "Vacunacion preventiva del lote", vetNameByHerd[animal.HerdId],
                "Control preventivo", "Aplicacion de vacuna", "Refuerzo segun calendario", "Programar refuerzo", Day(25 + animal.Id % 40)));
        }

        await context.Set<HealthEvent>().AddRangeAsync(healthEvents, cancellationToken);

        // ---------- Finances ----------
        var finances = new List<FinancialRecord>();
        void AddFinances(User rancher, (string Type, string Category, decimal Amount, int DaysAgo, string Description)[] rows) =>
            finances.AddRange(rows.Select(row => Finance(rancher.Id, row.Type, row.Category, row.Amount, Day(-row.DaysAgo), row.Description)));

        AddFinances(ganadero,
        [
            ("Ingreso", "Venta de leche", 4200, 80, "Venta mensual a acopiador local"),
            ("Egreso", "Alimento", 1350, 74, "Concentrado y forraje"),
            ("Ingreso", "Venta de ganado", 6800, 61, "Venta de 4 novillos"),
            ("Ingreso", "Venta de leche", 4350, 50, "Venta mensual a acopiador local"),
            ("Egreso", "Veterinaria", 540, 44, "Campana de vacunacion"),
            ("Egreso", "Transporte", 380, 33, "Traslado de ganado a feria"),
            ("Ingreso", "Venta de leche", 4510, 20, "Venta mensual a acopiador local"),
            ("Egreso", "Alimento", 1420, 12, "Concentrado y forraje"),
            ("Ingreso", "Otros", 980, 5, "Venta de pollos y patos")
        ]);
        AddFinances(maria,
        [
            ("Ingreso", "Otros", 1650, 85, "Venta de lana"),
            ("Egreso", "Veterinaria", 380, 70, "Botiquin sanitario"),
            ("Ingreso", "Venta de leche", 3100, 58, "Produccion lechera"),
            ("Egreso", "Alimento", 1180, 49, "Forraje y sales minerales"),
            ("Ingreso", "Venta de ganado", 5200, 36, "Venta de terneros"),
            ("Egreso", "Transporte", 260, 27, "Traslado de lana"),
            ("Ingreso", "Venta de leche", 3240, 15, "Produccion lechera"),
            ("Egreso", "Otros", 640, 8, "Reparacion de cercos")
        ]);
        AddFinances(jose,
        [
            ("Ingreso", "Venta de ganado", 3100, 82, "Lote porcino"),
            ("Egreso", "Otros", 920, 72, "Mantenimiento de corrales"),
            ("Egreso", "Alimento", 1560, 63, "Balanceado porcino"),
            ("Ingreso", "Venta de ganado", 4700, 51, "Venta de 18 cerdos"),
            ("Egreso", "Veterinaria", 410, 40, "Desparasitacion"),
            ("Ingreso", "Otros", 1300, 29, "Venta de caballo de paso"),
            ("Egreso", "Transporte", 330, 18, "Traslado a camal"),
            ("Ingreso", "Venta de ganado", 3900, 9, "Venta de bovinos")
        ]);
        AddFinances(rosa,
        [
            ("Ingreso", "Otros", 2100, 60, "Produccion de aves"),
            ("Egreso", "Veterinaria", 460, 52, "Campana preventiva"),
            ("Egreso", "Alimento", 880, 30, "Balanceado avicola")
        ]);

        await context.Set<FinancialRecord>().AddRangeAsync(finances, cancellationToken);

        // ---------- Activities ----------
        await context.Set<FarmActivity>().AddRangeAsync(new[]
        {
            Activity(ganadero.Id, veterinaria.Id, "Visita sanitaria Los Alamos", "Visita veterinaria", Day(3), "Alta", "Pendiente"),
            Activity(ganadero.Id, veterinaria.Id, "Refuerzo de aves El Molino", "Sanitario", Day(7), "Media", "Programado"),
            Activity(ganadero.Id, null, "Compra de alimento", "Financiero", Day(2), "Baja", "Pendiente"),
            Activity(ganadero.Id, veterinaria.Id, "Control de peso en Pampa Verde", "Productivo", Day(14), "Media", "Programado"),
            Activity(ganadero.Id, null, "Ordeno y registro de produccion", "Productivo", Day(-4), "Baja", "Completado"),
            Activity(maria.Id, veterinaria.Id, "Seguimiento caprino", "Sanitario", Day(4), "Alta", "Pendiente"),
            Activity(maria.Id, veterinaria.Id, "Esquila de la majada", "Productivo", Day(10), "Media", "Programado"),
            Activity(maria.Id, null, "Revision de cercos", "Productivo", Day(18), "Baja", "Pendiente"),
            Activity(maria.Id, veterinaria.Id, "Diagnostico de terneros", "Visita veterinaria", Day(-7), "Alta", "Completado"),
            Activity(jose.Id, vetPedro.Id, "Control porcino mensual", "Visita veterinaria", Day(5), "Media", "Pendiente"),
            Activity(jose.Id, vetPedro.Id, "Herraje de caballos", "Productivo", Day(12), "Baja", "Programado"),
            Activity(jose.Id, veterinaria.Id, "Vacunacion de bovinos Valle Sagrado", "Sanitario", Day(21), "Alta", "Programado"),
            Activity(jose.Id, null, "Pago de balanceado", "Financiero", Day(1), "Media", "Pendiente"),
            Activity(rosa.Id, vetLucia.Id, "Revision avicola", "Visita veterinaria", Day(6), "Media", "Pendiente"),
            Activity(rosa.Id, null, "Limpieza de galpones", "Productivo", Day(9), "Baja", "Programado")
        }, cancellationToken);

        // ---------- Devices ----------
        Animal FirstOf(string herd, string corral) =>
            animals.First(animal => animal.HerdId == herdByName[herd].Id && animal.CorralId == corralId[(herdByName[herd].Id, corral)]);

        var devices = new List<Device>
        {
            Device("Balanza Los Alamos", "Scale", "SCL-001", "Online", herdByName["Hato Los Alamos"].Id, null),
            Device("Collar Luna", "SmartCollar", "COL-001", "Online", null, FirstOf("Hato Los Alamos", "Corral Norte").Id),
            Device("Arete Tornado", "RFIDTag", "RFID-002", "Online", null, FirstOf("Hato Los Alamos", "Corral Sur").Id),
            Device("Camara termica El Molino", "ThermalCamera", "TH-001", "Maintenance", herdByName["Granja El Molino"].Id, null),
            Device("Estacion Pampa Verde", "WeatherStation", "WTH-002", "Online", herdByName["Establo Pampa Verde"].Id, null),
            Device("Balanza Santa Rosa", "Scale", "SCL-003", "Online", herdByName["Finca Santa Rosa"].Id, null),
            Device("Sensor La Esperanza", "EnvironmentalSensor", "ENV-001", "Online", herdByName["Establo La Esperanza"].Id, null),
            Device("Collar Estrella", "SmartCollar", "COL-004", "Offline", null, FirstOf("Establo La Esperanza", "Establo Principal").Id),
            Device("Estacion Alto Verde", "WeatherStation", "WTH-003", "Online", herdByName["Fundo Alto Verde"].Id, null),
            Device("Balanza San Miguel", "Scale", "SCL-005", "Online", herdByName["Corral San Miguel"].Id, null),
            Device("Arete Relampago", "RFIDTag", "RFID-006", "Online", null, FirstOf("Fundo Alto Verde", "Cuadra de Caballos").Id),
            Device("Estacion Las Lomas", "WeatherStation", "WTH-001", "Online", herdByName["Finca Las Lomas"].Id, null)
        };

        await context.Set<Device>().AddRangeAsync(devices, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await context.Set<DeviceMetric>().AddRangeAsync(new[]
        {
            Metric(devices[0].Id, "Weight", 455, "kg", -18),
            Metric(devices[0].Id, "Weight", 449, "kg", -90),
            Metric(devices[1].Id, "Temperature", 38.4m, "C", -6),
            Metric(devices[1].Id, "Activity", 74, "%", -4),
            Metric(devices[2].Id, "Scan", 1, "read", -3),
            Metric(devices[3].Id, "SurfaceTemperature", 39.1m, "C", -10),
            Metric(devices[4].Id, "AirTemperature", 16.2m, "C", -2),
            Metric(devices[4].Id, "Humidity", 71, "%", -2),
            Metric(devices[5].Id, "Weight", 58, "kg", -12),
            Metric(devices[6].Id, "AirTemperature", 14.8m, "C", -1),
            Metric(devices[6].Id, "Humidity", 66, "%", -1),
            Metric(devices[7].Id, "Temperature", 38.9m, "C", -30),
            Metric(devices[8].Id, "AirTemperature", 12.5m, "C", -3),
            Metric(devices[9].Id, "Weight", 118, "kg", -20),
            Metric(devices[10].Id, "Scan", 1, "read", -5),
            Metric(devices[11].Id, "Humidity", 62, "%", -2),
            Metric(devices[11].Id, "AirTemperature", 18.7m, "C", -1)
        }, cancellationToken);

        // ---------- Subscriptions ----------
        var plans = DefaultPlans();

        await context.Set<SubscriptionPlan>().AddRangeAsync(plans, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var subscriptions = new List<Subscription>
        {
            Subscription(ganadero.Id, plans[1].Id, "mock_customer_1", "mock_subscription_1"),
            Subscription(veterinaria.Id, plans[2].Id, "mock_customer_2", "mock_subscription_2"),
            Subscription(maria.Id, plans[0].Id, "mock_customer_3", "mock_subscription_3"),
            Subscription(jose.Id, plans[0].Id, "mock_customer_4", "mock_subscription_4")
        };

        await context.Set<Subscription>().AddRangeAsync(subscriptions, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await context.Set<Payment>().AddRangeAsync(new[]
        {
            Payment(ganadero.Id, subscriptions[0].Id, plans[1].Price, "mock_payment_1"),
            Payment(veterinaria.Id, subscriptions[1].Id, plans[2].Price, "mock_payment_2"),
            Payment(maria.Id, subscriptions[2].Id, plans[0].Price, "mock_payment_3"),
            Payment(jose.Id, subscriptions[3].Id, plans[0].Price, "mock_payment_4")
        }, cancellationToken);

        await context.Set<ReportMetric>().AddRangeAsync(new[]
        {
            new ReportMetric { Label = "Animales registrados", Value = animals.Count.ToString(), Trend = "Datos reales del backend" },
            new ReportMetric { Label = "Fincas activas", Value = herds.Count.ToString(), Trend = "Distribuidas por ganadero" },
            new ReportMetric { Label = "Dispositivos IoT", Value = devices.Count.ToString(), Trend = "Lecturas basicas disponibles" }
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>A date as the API writes it (yyyy-MM-dd), counted from today.</summary>
    private static string Day(int daysFromToday) => DateTime.UtcNow.AddDays(daysFromToday).ToString("yyyy-MM-dd");

    private static Herd Farm(string name, string location, User owner, User veterinarian, string mainType)
    {
        return new Herd { Name = name, Location = location, Owner = owner.FullName, OwnerId = owner.Id, VeterinarianId = veterinarian.Id, MainType = mainType };
    }

    private static Animal Animal(string tag, string name, string species, string breed, string gender, string birthDate, decimal weight, string status, int herdId, int corralId)
    {
        return new Animal { Tag = tag, Name = name, Species = species, Breed = breed, Gender = gender, BirthDate = DateOnly.Parse(birthDate), Weight = decimal.Round(weight, 1), Status = status, HerdId = herdId, CorralId = corralId };
    }

    private static VeterinarianClient Client(int veterinarianId, int rancherId)
    {
        return new VeterinarianClient { VeterinarianId = veterinarianId, RancherId = rancherId, Status = "Accepted", RequestedAt = DateTime.UtcNow.AddDays(-20), AcceptedAt = DateTime.UtcNow.AddDays(-20) };
    }

    private static HealthEvent Health(int animalId, string type, string date, string description, string veterinarian, string diagnosis, string treatment, string prescription, string followUp, string? nextDueDate)
    {
        return new HealthEvent { AnimalId = animalId, Type = type, Date = DateOnly.Parse(date), Description = description, Veterinarian = veterinarian, Diagnosis = diagnosis, Treatment = treatment, Prescription = prescription, FollowUp = followUp, NextDueDate = nextDueDate is null ? null : DateOnly.Parse(nextDueDate) };
    }

    private static FinancialRecord Finance(int ownerId, string type, string category, decimal amount, string date, string description)
    {
        return new FinancialRecord { OwnerId = ownerId, Type = type, Category = category, Amount = amount, Date = DateOnly.Parse(date), Description = description };
    }

    private static FarmActivity Activity(int? ownerId, int? veterinarianId, string title, string type, string date, string priority, string status)
    {
        return new FarmActivity { OwnerId = ownerId, VeterinarianId = veterinarianId, Title = title, Type = type, Date = DateOnly.Parse(date), Priority = priority, Status = status };
    }

    private static Device Device(string name, string type, string serialNumber, string status, int? herdId, int? animalId)
    {
        return new Device { Name = name, Type = type, SerialNumber = serialNumber, Status = status, HerdId = herdId, AnimalId = animalId };
    }

    private static DeviceMetric Metric(int deviceId, string type, decimal value, string unit, int hoursAgo)
    {
        return new DeviceMetric { DeviceId = deviceId, Type = type, Value = value, Unit = unit, RecordedAt = DateTime.UtcNow.AddHours(hoursAgo) };
    }

    private static Subscription Subscription(int userId, int planId, string customerId, string subscriptionId)
    {
        return new Subscription { UserId = userId, PlanId = planId, StripeCustomerId = customerId, StripeSubscriptionId = subscriptionId, Status = "Active", StartedAt = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)), EndsAt = null };
    }

    private static Payment Payment(int userId, int subscriptionId, decimal amount, string paymentId)
    {
        return new Payment { UserId = userId, SubscriptionId = subscriptionId, Amount = amount, Currency = "PEN", Provider = "MockStripe", ProviderPaymentId = paymentId, Status = "Paid", PaidAt = DateTime.UtcNow.AddDays(-12) };
    }
}
