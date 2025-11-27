using Microsoft.EntityFrameworkCore;
using PickMeUp.Core.Common.Helpers;
using PickMeUp.Core.Database;
using PickMeUp.Core.Database.Models;
using PickMeUp.Enums.UserPickUpRequest; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PickMeUp.Core.Infrastructure;

internal static class DataSeeder
{
    /// <summary>
    /// Seed database with robust test data for development.
    /// </summary>
    public static async Task SeedDataAsync(PickMeUpDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // ==============================================================================
        // 1. UTENTI
        // ==============================================================================
        var defaultPassword = CryptographyHelper.Hash("Password123!");
        
        var users = new List<User>
        {
            new() { Email = "admin@pickmeup.com", NormalizedEmail = "ADMIN@PICKMEUP.COM", PasswordHash = defaultPassword, FirstName = "Admin", LastName = "User", IsEmailConfirmed = true },
            new() { Email = "mario.rossi@test.com", NormalizedEmail = "MARIO.ROSSI@TEST.COM", PasswordHash = defaultPassword, FirstName = "Mario", LastName = "Rossi", IsEmailConfirmed = true },
            new() { Email = "luca.bianchi@test.com", NormalizedEmail = "LUCA.BIANCHI@TEST.COM", PasswordHash = defaultPassword, FirstName = "Luca", LastName = "Bianchi", IsEmailConfirmed = true },
            new() { Email = "giulia.verdi@test.com", NormalizedEmail = "GIULIA.VERDI@TEST.COM", PasswordHash = defaultPassword, FirstName = "Giulia", LastName = "Verdi", IsEmailConfirmed = true },
            new() { Email = "anna.neri@test.com", NormalizedEmail = "ANNA.NERI@TEST.COM", PasswordHash = defaultPassword, FirstName = "Anna", LastName = "Neri", IsEmailConfirmed = true },
            new() { Email = "paolo.gialli@test.com", NormalizedEmail = "PAOLO.GIALLI@TEST.COM", PasswordHash = defaultPassword, FirstName = "Paolo", LastName = "Gialli", IsEmailConfirmed = true },
            new() { Email = "elena.blu@test.com", NormalizedEmail = "ELENA.BLU@TEST.COM", PasswordHash = defaultPassword, FirstName = "Elena", LastName = "Blu", IsEmailConfirmed = true }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var mario = users.First(u => u.Email == "mario.rossi@test.com");
        var luca = users.First(u => u.Email == "luca.bianchi@test.com");
        var giulia = users.First(u => u.Email == "giulia.verdi@test.com");
        var anna = users.First(u => u.Email == "anna.neri@test.com");
        var paolo = users.First(u => u.Email == "paolo.gialli@test.com");

        // ==============================================================================
        // 2. LOCATION (Partenze, Arrivi e Tappe Intermedie)
        // ==============================================================================
        var locations = new List<Location>
        {
            // PARTENZE/ARRIVI PRINCIPALI
            new() { ReadableAddress = "Stazione Centrale, Piazza Duca d'Aosta, 1, Milano, MI", City = "Milano", Street = "Piazza Duca d'Aosta", Number = "1", Province = "MI", Region = "Lombardia", Country = "Italia", PostalCode = "20124", Latitude = 45.4859, Longitude = 9.2047 },
            new() { ReadableAddress = "Politecnico di Milano, Piazza Leonardo da Vinci, 32, Milano, MI", City = "Milano", Street = "Piazza Leonardo da Vinci", Number = "32", Province = "MI", Region = "Lombardia", Country = "Italia", PostalCode = "20133", Latitude = 45.4781, Longitude = 9.2270 },
            new() { ReadableAddress = "Stazione Porta Nuova, Corso Vittorio Emanuele II, 58, Torino, TO", City = "Torino", Street = "Corso Vittorio Emanuele II", Number = "58", Province = "TO", Region = "Piemonte", Country = "Italia", PostalCode = "10121", Latitude = 45.0621, Longitude = 7.6785 },
            new() { ReadableAddress = "Piazza Maggiore, Bologna, BO", City = "Bologna", Street = "Piazza Maggiore", Number = "", Province = "BO", Region = "Emilia-Romagna", Country = "Italia", PostalCode = "40124", Latitude = 44.4949, Longitude = 11.3426 },
            new() { ReadableAddress = "Stazione Termini, Piazza dei Cinquecento, 1, Roma, RM", City = "Roma", Street = "Piazza dei Cinquecento", Number = "1", Province = "RM", Region = "Lazio", Country = "Italia", PostalCode = "00185", Latitude = 41.9009, Longitude = 12.5020 },
            new() { ReadableAddress = "Colosseo, Piazza del Colosseo, 1, Roma, RM", City = "Roma", Street = "Piazza del Colosseo", Number = "1", Province = "RM", Region = "Lazio", Country = "Italia", PostalCode = "00184", Latitude = 41.8902, Longitude = 12.4922 },
            new() { ReadableAddress = "Stazione Napoli Centrale, Piazza Garibaldi, Napoli, NA", City = "Napoli", Street = "Piazza Garibaldi", Number = "", Province = "NA", Region = "Campania", Country = "Italia", PostalCode = "80142", Latitude = 40.8518, Longitude = 14.2681 },

            // TAPPE INTERMEDIE (Per simulare deviazioni)
            // Novara (Tra Milano e Torino)
            new() { ReadableAddress = "Stazione Novara, Piazza Garibaldi, Novara, NO", City = "Novara", Street = "Piazza Garibaldi", Number = "", Province = "NO", Region = "Piemonte", Country = "Italia", PostalCode = "28100", Latitude = 45.4514, Longitude = 8.6234 },
            
            // Firenze (Tra Bologna e Napoli / Roma e Milano)
            new() { ReadableAddress = "Stazione Santa Maria Novella, Piazza della Stazione, Firenze, FI", City = "Firenze", Street = "Piazza della Stazione", Number = "", Province = "FI", Region = "Toscana", Country = "Italia", PostalCode = "50123", Latitude = 43.7765, Longitude = 11.2478 }
        };

        context.Locations.AddRange(locations);
        await context.SaveChangesAsync();

        // Helper locations
        var milanoCentrale = locations.First(l => l.City == "Milano" && l.Street!.Contains("Duca"));
        var milanoPoli = locations.First(l => l.Street!.Contains("Leonardo"));
        var torino = locations.First(l => l.City == "Torino");
        var bologna = locations.First(l => l.City == "Bologna");
        var romaTermini = locations.First(l => l.City == "Roma" && l.Street!.Contains("Cinquecento"));
        var romaColosseo = locations.First(l => l.City == "Roma" && l.Street!.Contains("Colosseo"));
        var napoli = locations.First(l => l.City == "Napoli");
        
        // Nuove location intermedie
        var novara = locations.First(l => l.City == "Novara");
        var firenze = locations.First(l => l.City == "Firenze");

        // ==============================================================================
        // 3. VIAGGI
        // ==============================================================================
        var travels = new List<UserTravel>
        {
            // Viaggio 1: Milano -> Torino (Passa per Novara)
            new()
            {
                UserId = mario.UserId,
                DepartureLocationId = milanoCentrale.LocationId,
                DestinationLocationId = torino.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(8).AddMinutes(30),
                TotalPassengersSeatsCount = 3,
                OccupiedPassengerSeatsCount = 1 
            },

            // Viaggio 2: Milano -> Roma (Diretto)
            new()
            {
                UserId = luca.UserId,
                DepartureLocationId = milanoPoli.LocationId,
                DestinationLocationId = romaTermini.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(3).Date.AddHours(14).AddMinutes(00), 
                TotalPassengersSeatsCount = 4,
                OccupiedPassengerSeatsCount = 0 
            },

            // Viaggio 3: Bologna -> Napoli (Passa per Firenze)
            new()
            {
                UserId = giulia.UserId,
                DepartureLocationId = bologna.LocationId,
                DestinationLocationId = napoli.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(5).Date.AddHours(9).AddMinutes(00),
                TotalPassengersSeatsCount = 2,
                OccupiedPassengerSeatsCount = 2 
            },

            // Viaggio 4: Roma -> Milano (Passa per Bologna)
            new()
            {
                UserId = luca.UserId,
                DepartureLocationId = romaColosseo.LocationId,
                DestinationLocationId = milanoCentrale.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(7).Date.AddHours(10).AddMinutes(00),
                TotalPassengersSeatsCount = 4,
                OccupiedPassengerSeatsCount = 1
            },
            
             // Viaggio 5: Torino -> Milano (Pendolare)
            new()
            {
                UserId = mario.UserId,
                DepartureLocationId = torino.LocationId,
                DestinationLocationId = milanoCentrale.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(7).AddMinutes(00),
                TotalPassengersSeatsCount = 3,
                OccupiedPassengerSeatsCount = 0
            }
        };

        context.UserTravels.AddRange(travels);
        await context.SaveChangesAsync();

        var milanoTorino = travels[0]; 
        var bolognaNapoli = travels[2]; 
        var romaMilano = travels[3]; 

        // ==============================================================================
        // 4. RICHIESTE (Deviazioni reali)
        // ==============================================================================
        var requests = new List<UserPickUpRequest>
        {
            // Richiesta 1: Viaggio Milano -> Torino. Pickup a NOVARA (nel mezzo).
            new()
            {
                UserId = anna.UserId,
                UserTravelId = milanoTorino.UserTravelId,
                LocationId = novara.LocationId, // <-- DEVIAZIONE
                Status = UserPickUpRequestStatus.Accepted,
                DeletionDateTime = null
            },

            // Richiesta 2: Viaggio Bologna -> Napoli. Pickup a FIRENZE (nel mezzo).
            new()
            {
                UserId = anna.UserId,
                UserTravelId = bolognaNapoli.UserTravelId,
                LocationId = firenze.LocationId, // <-- DEVIAZIONE
                Status = UserPickUpRequestStatus.Accepted
            },
            
            // Richiesta 3: Stesso viaggio BO -> NA, Pickup sempre a FIRENZE.
            new()
            {
                UserId = paolo.UserId,
                UserTravelId = bolognaNapoli.UserTravelId,
                LocationId = firenze.LocationId, // <-- DEVIAZIONE
                Status = UserPickUpRequestStatus.Accepted
            },

            // Richiesta 4: Viaggio Roma -> Milano. Pickup a BOLOGNA (nel mezzo).
            new()
            {
                UserId = paolo.UserId,
                UserTravelId = romaMilano.UserTravelId,
                LocationId = bologna.LocationId, // <-- DEVIAZIONE (Bologna è intermedia tra Roma e Milano)
                Status = UserPickUpRequestStatus.Pending
            }
        };

        context.UserPickUpRequests.AddRange(requests);
        await context.SaveChangesAsync();

        /*
         * ======================================================================================
         * RECAP SCENARI DI TEST GENERATI (Con Deviazioni)
         * ======================================================================================
         * * 1. CREDENZIALI
         * - Drivers: mario.rossi@test.com, luca.bianchi@test.com, giulia.verdi@test.com
         * - Pass: anna.neri@test.com, paolo.gialli@test.com
         * * 2. SCENARI VIAGGI & PICKUP
         * * A) MI -> TO (Mario) | Domani
         * - Deviazione: Anna viene raccolta a **NOVARA** (a metà strada).
         * * B) BO -> NA (Giulia) | Tra 5 giorni | FULL
         * - Deviazione: Anna e Paolo vengono raccolti a **FIRENZE**.
         * * C) RM -> MI (Luca) | Tra 7 giorni
         * - Deviazione: Paolo ha richiesto un passaggio da **BOLOGNA** (Pending).
         * * D) GEO-SEARCH
         * - Prova a cercare un passaggio da "Firenze" -> Dovrebbe uscire il viaggio BO-NA.
         * - Prova a cercare da "Novara" -> Dovrebbe uscire il viaggio MI-TO.
         * ======================================================================================
         */
    }
}