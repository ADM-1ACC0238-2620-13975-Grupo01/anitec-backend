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

        var herds = new List<Herd>
        {
            new() { Name = "Hato Los Alamos", Location = "Cajamarca", Owner = ganadero.FullName, OwnerId = ganadero.Id, VeterinarianId = veterinaria.Id, MainType = "Mixto" },
            new() { Name = "Granja El Molino", Location = "Cajamarca", Owner = ganadero.FullName, OwnerId = ganadero.Id, VeterinarianId = veterinaria.Id, MainType = "Aves" },
            new() { Name = "Finca Santa Rosa", Location = "Junin", Owner = maria.FullName, OwnerId = maria.Id, VeterinarianId = veterinaria.Id, MainType = "Ovino y caprino" },
            new() { Name = "Establo La Esperanza", Location = "Junin", Owner = maria.FullName, OwnerId = maria.Id, VeterinarianId = veterinaria.Id, MainType = "Bovino" },
            new() { Name = "Fundo Alto Verde", Location = "Cusco", Owner = jose.FullName, OwnerId = jose.Id, VeterinarianId = vetPedro.Id, MainType = "Mixto" },
            new() { Name = "Corral San Miguel", Location = "Cusco", Owner = jose.FullName, OwnerId = jose.Id, VeterinarianId = vetPedro.Id, MainType = "Porcino" },
            new() { Name = "Finca Las Lomas", Location = "Ayacucho", Owner = rosa.FullName, OwnerId = rosa.Id, VeterinarianId = vetLucia.Id, MainType = "Mixto" },
            new() { Name = "Avicola Sol Andino", Location = "Ayacucho", Owner = rosa.FullName, OwnerId = rosa.Id, VeterinarianId = vetLucia.Id, MainType = "Aves" }
        };

        await context.Set<Herd>().AddRangeAsync(herds, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var animals = new List<Animal>
        {
            Animal("BOV-001", "Luna", "Bovino", "Brown Swiss", "Hembra", "2021-03-14", 410, "Saludable", herds[0].Id),
            Animal("BOV-002", "Tornado", "Bovino", "Holstein", "Macho", "2020-10-03", 620, "En tratamiento", herds[0].Id),
            Animal("OVI-001", "Nube", "Ovino", "Corriedale", "Hembra", "2022-07-21", 55, "Saludable", herds[2].Id),
            Animal("CAP-001", "Chaska", "Caprino", "Saanen", "Hembra", "2023-01-18", 42, "Observacion", herds[2].Id),
            Animal("BOV-003", "Estrella", "Bovino", "Jersey", "Hembra", "2021-11-10", 360, "Saludable", herds[3].Id),
            Animal("CER-001", "Moro", "Porcino", "Duroc", "Macho", "2023-04-02", 115, "Saludable", herds[5].Id),
            Animal("CER-002", "Pecas", "Porcino", "Yorkshire", "Hembra", "2022-12-19", 128, "Observacion", herds[5].Id),
            Animal("AVE-001", "Gallina Roja", "Pollo", "Rhode Island", "Hembra", "2024-01-05", 3, "Saludable", herds[1].Id),
            Animal("AVE-002", "Pato Norte", "Pato", "Pekin", "Macho", "2023-09-12", 4, "Saludable", herds[1].Id),
            Animal("AVE-003", "Pollon", "Pollo", "Cobb 500", "Macho", "2024-02-11", 3, "En tratamiento", herds[7].Id),
            Animal("BOV-004", "Maya", "Bovino", "Simmental", "Hembra", "2022-05-07", 390, "Saludable", herds[4].Id),
            Animal("EQU-001", "Relampago", "Equino", "Criollo", "Macho", "2019-08-25", 430, "Saludable", herds[4].Id),
            Animal("OVI-002", "Copito", "Ovino", "Junin", "Macho", "2023-06-01", 48, "Saludable", herds[6].Id),
            Animal("CAP-002", "Sierra", "Caprino", "Alpina", "Hembra", "2022-03-30", 46, "Observacion", herds[6].Id),
            Animal("BOV-005", "Canela", "Bovino", "Brown Swiss", "Hembra", "2020-01-20", 455, "Saludable", herds[0].Id),
            Animal("AVE-004", "Pato Laguna", "Pato", "Muscovy", "Hembra", "2023-11-03", 4, "Saludable", herds[7].Id)
        };

        await context.Set<Animal>().AddRangeAsync(animals, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await context.Set<VeterinarianClient>().AddRangeAsync(new[]
        {
            Client(veterinaria.Id, ganadero.Id),
            Client(veterinaria.Id, maria.Id),
            Client(vetPedro.Id, jose.Id),
            Client(vetLucia.Id, rosa.Id)
        }, cancellationToken);

        await context.Set<HealthEvent>().AddRangeAsync(new[]
        {
            Health(animals[0].Id, "Vacuna", "2026-05-10", "Vacuna anual contra carbunco", veterinaria.FullName, "Control preventivo", "Aplicacion de vacuna", "Refuerzo anual", "Revisar proxima campana", "2026-11-10"),
            Health(animals[1].Id, "Tratamiento", "2026-05-18", "Diarrea y perdida de apetito", veterinaria.FullName, "Infeccion digestiva leve", "Antibiotico y hidratacion", "Oxitetraciclina cada 24h", "Control en 7 dias", "2026-06-14"),
            Health(animals[3].Id, "Incidencia", "2026-05-22", "Cojera leve en pata delantera", veterinaria.FullName, "Lesion por golpe", "Reposo y antiinflamatorio", "Meloxicam dosis unica", "Evaluar movilidad", "2026-06-11"),
            Health(animals[6].Id, "Revision", "2026-05-28", "Control de peso y temperatura", vetPedro.FullName, "Sin fiebre", "Observacion nutricional", "Suplemento vitaminico", "Nuevo pesaje", "2026-06-20"),
            Health(animals[9].Id, "Diagnostico", "2026-06-01", "Ave con baja actividad", vetLucia.FullName, "Sospecha respiratoria", "Aislamiento y tratamiento", "Antibiotico aviar", "Monitoreo diario", "2026-06-09"),
            Health(animals[13].Id, "Tratamiento", "2026-06-03", "Parasitos externos", vetLucia.FullName, "Infestacion moderada", "Bano sanitario", "Ivermectina", "Repetir dosis", "2026-06-17")
        }, cancellationToken);

        await context.Set<FinancialRecord>().AddRangeAsync(new[]
        {
            Finance(ganadero.Id, "Ingreso", "Venta de leche", 4200, "2026-05-04", "Venta mensual a acopiador local"),
            Finance(ganadero.Id, "Egreso", "Alimento", 1350, "2026-05-08", "Concentrado y forraje"),
            Finance(ganadero.Id, "Ingreso", "Venta de aves", 980, "2026-05-17", "Venta de pollos y patos"),
            Finance(maria.Id, "Ingreso", "Venta de lana", 1650, "2026-05-12", "Produccion de ovinos"),
            Finance(maria.Id, "Egreso", "Medicamentos", 380, "2026-05-19", "Botiquin sanitario"),
            Finance(jose.Id, "Ingreso", "Venta porcina", 3100, "2026-05-23", "Lote porcino"),
            Finance(jose.Id, "Egreso", "Infraestructura", 920, "2026-05-26", "Mantenimiento de corrales"),
            Finance(rosa.Id, "Ingreso", "Venta avicola", 2100, "2026-06-02", "Produccion de aves"),
            Finance(rosa.Id, "Egreso", "Vacunas", 460, "2026-06-04", "Campana preventiva")
        }, cancellationToken);

        await context.Set<FarmActivity>().AddRangeAsync(new[]
        {
            Activity(ganadero.Id, veterinaria.Id, "Visita sanitaria Los Alamos", "VeterinaryVisit", "2026-06-12", "Alta", "Pendiente"),
            Activity(ganadero.Id, veterinaria.Id, "Refuerzo aves El Molino", "Vaccination", "2026-06-16", "Media", "Pendiente"),
            Activity(maria.Id, veterinaria.Id, "Seguimiento caprino", "TreatmentFollowUp", "2026-06-14", "Alta", "Pendiente"),
            Activity(jose.Id, vetPedro.Id, "Control porcino mensual", "VeterinaryVisit", "2026-06-18", "Media", "Pendiente"),
            Activity(rosa.Id, vetLucia.Id, "Revision avicola", "VeterinaryVisit", "2026-06-20", "Media", "Pendiente"),
            Activity(ganadero.Id, null, "Compra de alimento", "FinancialTask", "2026-06-10", "Baja", "Pendiente")
        }, cancellationToken);

        var devices = new List<Device>
        {
            Device("Balanza Los Alamos", "Scale", "SCL-001", "Online", herds[0].Id, null),
            Device("Collar Luna", "SmartCollar", "COL-001", "Online", null, animals[0].Id),
            Device("Arete Tornado", "RFIDTag", "RFID-002", "Online", null, animals[1].Id),
            Device("Camara termica El Molino", "ThermalCamera", "TH-001", "Maintenance", herds[1].Id, null),
            Device("Estacion Las Lomas", "WeatherStation", "WTH-001", "Online", herds[6].Id, null)
        };

        await context.Set<Device>().AddRangeAsync(devices, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await context.Set<DeviceMetric>().AddRangeAsync(new[]
        {
            Metric(devices[0].Id, "Weight", 455, "kg", -18),
            Metric(devices[1].Id, "Temperature", 38.4m, "C", -6),
            Metric(devices[1].Id, "Activity", 74, "%", -4),
            Metric(devices[2].Id, "Scan", 1, "read", -3),
            Metric(devices[3].Id, "SurfaceTemperature", 39.1m, "C", -10),
            Metric(devices[4].Id, "Humidity", 62, "%", -2),
            Metric(devices[4].Id, "AirTemperature", 18.7m, "C", -1)
        }, cancellationToken);

        var plans = new List<SubscriptionPlan>
        {
            new() { Name = "Ganadero Basico", Price = 49, StripePriceId = "mock_price_rancher_basic", MaxAnimals = 80, IsActive = true },
            new() { Name = "Ganadero Pro", Price = 99, StripePriceId = "mock_price_rancher_pro", MaxAnimals = 250, IsActive = true },
            new() { Name = "Veterinario Profesional", Price = 129, StripePriceId = "mock_price_vet_pro", MaxAnimals = 1000, IsActive = true }
        };

        await context.Set<SubscriptionPlan>().AddRangeAsync(plans, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var subscriptions = new List<Subscription>
        {
            Subscription(ganadero.Id, plans[1].Id, "mock_customer_1", "mock_subscription_1"),
            Subscription(veterinaria.Id, plans[2].Id, "mock_customer_2", "mock_subscription_2"),
            Subscription(maria.Id, plans[0].Id, "mock_customer_3", "mock_subscription_3")
        };

        await context.Set<Subscription>().AddRangeAsync(subscriptions, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await context.Set<Payment>().AddRangeAsync(new[]
        {
            Payment(ganadero.Id, subscriptions[0].Id, plans[1].Price, "mock_payment_1"),
            Payment(veterinaria.Id, subscriptions[1].Id, plans[2].Price, "mock_payment_2"),
            Payment(maria.Id, subscriptions[2].Id, plans[0].Price, "mock_payment_3")
        }, cancellationToken);

        await context.Set<ReportMetric>().AddRangeAsync(new[]
        {
            new ReportMetric { Label = "Animales registrados", Value = animals.Count.ToString(), Trend = "Datos reales del backend" },
            new ReportMetric { Label = "Fincas activas", Value = herds.Count.ToString(), Trend = "Distribuidas por ganadero" },
            new ReportMetric { Label = "Dispositivos IoT", Value = devices.Count.ToString(), Trend = "Lecturas basicas disponibles" }
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static Animal Animal(string tag, string name, string species, string breed, string gender, string birthDate, decimal weight, string status, int herdId)
    {
        return new Animal { Tag = tag, Name = name, Species = species, Breed = breed, Gender = gender, BirthDate = DateOnly.Parse(birthDate), Weight = weight, Status = status, HerdId = herdId };
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
