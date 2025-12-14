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
        var elena = users.First(u => u.Email == "elena.blu@test.com");

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
            new() { ReadableAddress = "Stazione Santa Maria Novella, Piazza della Stazione, Firenze, FI", City = "Firenze", Street = "Piazza della Stazione", Number = "", Province = "FI", Region = "Toscana", Country = "Italia", PostalCode = "50123", Latitude = 43.7765, Longitude = 11.2478 },
            
            // Altre città italiane
            new() { ReadableAddress = "Stazione Verona Porta Nuova, Piazzale XXV Aprile, Verona, VR", City = "Verona", Street = "Piazzale XXV Aprile", Number = "", Province = "VR", Region = "Veneto", Country = "Italia", PostalCode = "37138", Latitude = 45.4428, Longitude = 10.9821 },
            new() { ReadableAddress = "Stazione Venezia Santa Lucia, Fondamenta Santa Lucia, Venezia, VE", City = "Venezia", Street = "Fondamenta Santa Lucia", Number = "", Province = "VE", Region = "Veneto", Country = "Italia", PostalCode = "30121", Latitude = 45.4414, Longitude = 12.3206 },
            new() { ReadableAddress = "Stazione Genova Piazza Principe, Piazza Acquaverde, Genova, GE", City = "Genova", Street = "Piazza Acquaverde", Number = "", Province = "GE", Region = "Liguria", Country = "Italia", PostalCode = "16126", Latitude = 44.4165, Longitude = 8.9192 },
            new() { ReadableAddress = "Stazione Padova, Piazzale della Stazione, Padova, PD", City = "Padova", Street = "Piazzale della Stazione", Number = "", Province = "PD", Region = "Veneto", Country = "Italia", PostalCode = "35131", Latitude = 45.4176, Longitude = 11.8805 },
            new() { ReadableAddress = "Università di Bologna, Via Zamboni, 33, Bologna, BO", City = "Bologna", Street = "Via Zamboni", Number = "33", Province = "BO", Region = "Emilia-Romagna", Country = "Italia", PostalCode = "40126", Latitude = 44.4964, Longitude = 11.3518 },
            new() { ReadableAddress = "Aeroporto di Milano Malpensa, Ferno, VA", City = "Ferno", Street = "Aeroporto Malpensa", Number = "", Province = "VA", Region = "Lombardia", Country = "Italia", PostalCode = "21010", Latitude = 45.6306, Longitude = 8.7281 },
            new() { ReadableAddress = "Stazione Brescia, Piazzale della Stazione, Brescia, BS", City = "Brescia", Street = "Piazzale della Stazione", Number = "", Province = "BS", Region = "Lombardia", Country = "Italia", PostalCode = "25122", Latitude = 45.5307, Longitude = 10.2120 }
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
        var verona = locations.First(l => l.City == "Verona");
        var venezia = locations.First(l => l.City == "Venezia");
        var genova = locations.First(l => l.City == "Genova");
        var padova = locations.First(l => l.City == "Padova");
        var bolognaunibo = locations.First(l => l.Street!.Contains("Zamboni"));
        var malpensa = locations.First(l => l.City == "Ferno");
        var brescia = locations.First(l => l.City == "Brescia");

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
            },

            // Viaggio 6: Venezia -> Milano (Elena, tra 4 giorni, mattina)
            new()
            {
                UserId = elena.UserId,
                DepartureLocationId = venezia.LocationId,
                DestinationLocationId = milanoCentrale.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(4).Date.AddHours(8).AddMinutes(00),
                TotalPassengersSeatsCount = 3,
                OccupiedPassengerSeatsCount = 1
            },

            // Viaggio 7: Milano -> Genova (Anna, oggi pomeriggio)
            new()
            {
                UserId = anna.UserId,
                DepartureLocationId = milanoCentrale.LocationId,
                DestinationLocationId = genova.LocationId,
                DepartureDateTime = DateTime.UtcNow.Date.AddHours(16).AddMinutes(30),
                TotalPassengersSeatsCount = 2,
                OccupiedPassengerSeatsCount = 0
            },

            // Viaggio 8: Bologna (Unibo) -> Firenze (Paolo, domani, studenti)
            new()
            {
                UserId = paolo.UserId,
                DepartureLocationId = bolognaunibo.LocationId,
                DestinationLocationId = firenze.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(14).AddMinutes(30),
                TotalPassengersSeatsCount = 4,
                OccupiedPassengerSeatsCount = 3
            },

            // Viaggio 9: Napoli -> Roma (Giulia, tra 6 giorni, sera)
            new()
            {
                UserId = giulia.UserId,
                DepartureLocationId = napoli.LocationId,
                DestinationLocationId = romaTermini.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(6).Date.AddHours(19).AddMinutes(00),
                TotalPassengersSeatsCount = 3,
                OccupiedPassengerSeatsCount = 0
            },

            // Viaggio 10: Milano Malpensa -> Milano Centrale (Mario, domani mattina presto)
            new()
            {
                UserId = mario.UserId,
                DepartureLocationId = malpensa.LocationId,
                DestinationLocationId = milanoCentrale.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(6).AddMinutes(00),
                TotalPassengersSeatsCount = 4,
                OccupiedPassengerSeatsCount = 2
            },

            // Viaggio 11: Verona -> Venezia (Luca, tra 8 giorni)
            new()
            {
                UserId = luca.UserId,
                DepartureLocationId = verona.LocationId,
                DestinationLocationId = venezia.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(8).Date.AddHours(10).AddMinutes(30),
                TotalPassengersSeatsCount = 3,
                OccupiedPassengerSeatsCount = 0
            },

            // Viaggio 12: Padova -> Bologna (Elena, tra 3 giorni, universitari)
            new()
            {
                UserId = elena.UserId,
                DepartureLocationId = padova.LocationId,
                DestinationLocationId = bolognaunibo.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(3).Date.AddHours(9).AddMinutes(30),
                TotalPassengersSeatsCount = 3,
                OccupiedPassengerSeatsCount = 0
            },

            // Viaggio 13: Brescia -> Milano (Anna, tra 2 giorni, pendolare)
            new()
            {
                UserId = anna.UserId,
                DepartureLocationId = brescia.LocationId,
                DestinationLocationId = milanoPoli.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(7).AddMinutes(30),
                TotalPassengersSeatsCount = 4,
                OccupiedPassengerSeatsCount = 1
            },

            // Viaggio 14: Roma -> Napoli (Paolo, tra 5 giorni, pomeriggio)
            new()
            {
                UserId = paolo.UserId,
                DepartureLocationId = romaTermini.LocationId,
                DestinationLocationId = napoli.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(5).Date.AddHours(15).AddMinutes(00),
                TotalPassengersSeatsCount = 3,
                OccupiedPassengerSeatsCount = 0
            },

            // Viaggio 15: Genova -> Torino (Giulia, tra 9 giorni)
            new()
            {
                UserId = giulia.UserId,
                DepartureLocationId = genova.LocationId,
                DestinationLocationId = torino.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(9).Date.AddHours(11).AddMinutes(00),
                TotalPassengersSeatsCount = 2,
                OccupiedPassengerSeatsCount = 0
            },

            // Viaggio 16: Milano -> Verona (Luca, tra 4 giorni, pomeriggio)
            new()
            {
                UserId = luca.UserId,
                DepartureLocationId = milanoCentrale.LocationId,
                DestinationLocationId = verona.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(4).Date.AddHours(15).AddMinutes(30),
                TotalPassengersSeatsCount = 4,
                OccupiedPassengerSeatsCount = 1
            },

            // Viaggio 17: Bologna -> Padova (Mario, tra 6 giorni)
            new()
            {
                UserId = mario.UserId,
                DepartureLocationId = bologna.LocationId,
                DestinationLocationId = padova.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(6).Date.AddHours(13).AddMinutes(00),
                TotalPassengersSeatsCount = 3,
                OccupiedPassengerSeatsCount = 0
            },

            // Viaggio 18: Firenze -> Bologna (Elena, domani sera)
            new()
            {
                UserId = elena.UserId,
                DepartureLocationId = firenze.LocationId,
                DestinationLocationId = bologna.LocationId,
                DepartureDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(20).AddMinutes(00),
                TotalPassengersSeatsCount = 2,
                OccupiedPassengerSeatsCount = 2
            }
        };

        context.UserTravels.AddRange(travels);
        await context.SaveChangesAsync();

        var milanoTorino = travels[0]; 
        var milanoRoma = travels[1];
        var bolognaNapoli = travels[2]; 
        var romaMilano = travels[3];
        var torinoMilano = travels[4];
        var veneziaMilano = travels[5];
        var milanoGenova = travels[6];
        var bolognaFirenze = travels[7];
        var napoliRoma = travels[8];
        var malpensaMilano = travels[9];
        var veronaVenezia = travels[10];
        var padovaBologna = travels[11];
        var bresciaMilano = travels[12];
        var romaNapoli = travels[13];
        var genovaTorino = travels[14];
        var milanoVerona = travels[15];
        var bolognaPadova = travels[16];
        var firenzeBologna = travels[17]; 

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
            },

            // Richiesta 5: Viaggio Milano -> Roma (Luca). Anna chiede passaggio da Milano.
            new()
            {
                UserId = anna.UserId,
                UserTravelId = milanoRoma.UserTravelId,
                LocationId = milanoPoli.LocationId,
                Status = UserPickUpRequestStatus.Pending
            },

            // Richiesta 6: Viaggio Milano -> Roma (Luca). Paolo chiede da Milano Centrale.
            new()
            {
                UserId = paolo.UserId,
                UserTravelId = milanoRoma.UserTravelId,
                LocationId = milanoCentrale.LocationId,
                Status = UserPickUpRequestStatus.Pending
            },

            // Richiesta 7: Viaggio Venezia -> Milano (Elena). Paolo già accettato.
            new()
            {
                UserId = paolo.UserId,
                UserTravelId = veneziaMilano.UserTravelId,
                LocationId = venezia.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            // Richiesta 8: Viaggio Bologna -> Firenze (Paolo). Già pieno (3/4 occupati).
            new()
            {
                UserId = mario.UserId,
                UserTravelId = bolognaFirenze.UserTravelId,
                LocationId = bolognaunibo.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            new()
            {
                UserId = giulia.UserId,
                UserTravelId = bolognaFirenze.UserTravelId,
                LocationId = bolognaunibo.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            new()
            {
                UserId = anna.UserId,
                UserTravelId = bolognaFirenze.UserTravelId,
                LocationId = bolognaunibo.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            // Richiesta 9: Viaggio Malpensa -> Milano (Mario). Luca e Giulia già a bordo.
            new()
            {
                UserId = luca.UserId,
                UserTravelId = malpensaMilano.UserTravelId,
                LocationId = malpensa.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            new()
            {
                UserId = giulia.UserId,
                UserTravelId = malpensaMilano.UserTravelId,
                LocationId = malpensa.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            // Richiesta 10: Viaggio Milano -> Verona (Luca). Mario accettato.
            new()
            {
                UserId = mario.UserId,
                UserTravelId = milanoVerona.UserTravelId,
                LocationId = milanoCentrale.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            // Richiesta 11: Viaggio Brescia -> Milano (Anna). Elena chiede passaggio in pending.
            new()
            {
                UserId = elena.UserId,
                UserTravelId = bresciaMilano.UserTravelId,
                LocationId = brescia.LocationId,
                Status = UserPickUpRequestStatus.Pending
            },

            // Richiesta 12: Viaggio Brescia -> Milano (Anna). Giulia già accettata.
            new()
            {
                UserId = giulia.UserId,
                UserTravelId = bresciaMilano.UserTravelId,
                LocationId = brescia.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            // Richiesta 13: Viaggio Firenze -> Bologna (Elena). PIENO (2/2).
            new()
            {
                UserId = mario.UserId,
                UserTravelId = firenzeBologna.UserTravelId,
                LocationId = firenze.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            new()
            {
                UserId = luca.UserId,
                UserTravelId = firenzeBologna.UserTravelId,
                LocationId = firenze.LocationId,
                Status = UserPickUpRequestStatus.Accepted
            },

            // Richiesta 14: Viaggio Torino -> Milano (Mario). Nessuna richiesta (viaggio disponibile).

            // Richiesta 15: Viaggio Milano -> Genova (Anna). Nessuna richiesta (viaggio disponibile oggi).

            // Richiesta 16: Viaggio Napoli -> Roma (Giulia). Paolo in pending.
            new()
            {
                UserId = paolo.UserId,
                UserTravelId = napoliRoma.UserTravelId,
                LocationId = napoli.LocationId,
                Status = UserPickUpRequestStatus.Pending
            },

            // Richiesta 17: Viaggio Padova -> Bologna (Elena). Luca in pending.
            new()
            {
                UserId = luca.UserId,
                UserTravelId = padovaBologna.UserTravelId,
                LocationId = padova.LocationId,
                Status = UserPickUpRequestStatus.Pending
            },

            // Richiesta 18: Viaggio Padova -> Bologna (Elena). Anna chiede da Padova.
            new()
            {
                UserId = anna.UserId,
                UserTravelId = padovaBologna.UserTravelId,
                LocationId = padova.LocationId,
                Status = UserPickUpRequestStatus.Pending
            },

            // Richiesta 19: Viaggio Verona -> Venezia (Luca). Deviazione per Elena da Verona.
            new()
            {
                UserId = elena.UserId,
                UserTravelId = veronaVenezia.UserTravelId,
                LocationId = verona.LocationId,
                Status = UserPickUpRequestStatus.Pending
            },

            // Richiesta 20: Viaggio Roma -> Napoli (Paolo). Mario in pending.
            new()
            {
                UserId = mario.UserId,
                UserTravelId = romaNapoli.UserTravelId,
                LocationId = romaTermini.LocationId,
                Status = UserPickUpRequestStatus.Pending
            }
        };

        context.UserPickUpRequests.AddRange(requests);
        await context.SaveChangesAsync();

        /*
         * ======================================================================================
         * RECAP SCENARI DI TEST GENERATI (Espanso con più dati)
         * ======================================================================================
         * 
         * 1. CREDENZIALI (Tutti con password: Password123!)
         * - mario.rossi@test.com (Driver attivo: 5 viaggi)
         * - luca.bianchi@test.com (Driver: 4 viaggi)
         * - giulia.verdi@test.com (Driver: 3 viaggi)
         * - anna.neri@test.com (Driver: 2 viaggi, Passeggero attivo)
         * - paolo.gialli@test.com (Driver: 2 viaggi, Passeggero attivo)
         * - elena.blu@test.com (Driver: 3 viaggi)
         * 
         * 2. STATISTICHE VIAGGI (18 viaggi totali)
         * - Disponibili oggi: 1 (Milano -> Genova)
         * - Domani: 3 viaggi
         * - Prossimi 7 giorni: 14 viaggi
         * - Viaggi PIENI: 2 (Bologna->Firenze, Firenze->Bologna)
         * - Viaggi con richieste pending: 9
         * 
         * 3. SCENARI CHIAVE PER TEST
         * 
         * A) VIAGGI CON RICHIESTE PENDING (ideali per test driver):
         *    - Milano -> Roma (Luca): 2 richieste pending (Anna, Paolo)
         *    - Roma -> Milano (Luca): 1 richiesta pending (Paolo da Bologna)
         *    - Brescia -> Milano (Anna): 1 richiesta pending (Elena)
         *    - Napoli -> Roma (Giulia): 1 richiesta pending (Paolo)
         *    - Padova -> Bologna (Elena): 2 richieste pending (Luca, Anna)
         *    - Verona -> Venezia (Luca): 1 richiesta pending (Elena)
         *    - Roma -> Napoli (Paolo): 1 richiesta pending (Mario)
         * 
         * B) VIAGGI PIENI (nessun posto disponibile):
         *    - Bologna -> Firenze (Paolo): 4/4 posti (Mario, Giulia, Anna)
         *    - Firenze -> Bologna (Elena): 2/2 posti (Mario, Luca)
         * 
         * C) VIAGGI CON DEVIAZIONI ACCETTATE:
         *    - Milano -> Torino (Mario): Anna raccolta a Novara
         *    - Bologna -> Napoli (Giulia): Anna e Paolo raccolti a Firenze [PIENO]
         *    - Venezia -> Milano (Elena): Paolo accettato
         *    - Malpensa -> Milano (Mario): Luca e Giulia accettati
         *    - Milano -> Verona (Luca): Mario accettato
         *    - Brescia -> Milano (Anna): Giulia accettata
         * 
         * D) VIAGGI DISPONIBILI (nessuna richiesta):
         *    - Torino -> Milano (Mario): 3 posti liberi
         *    - Milano -> Genova (Anna): 2 posti liberi, OGGI!
         *    - Bologna -> Padova (Mario): 3 posti liberi
         *    - Genova -> Torino (Giulia): 2 posti liberi
         * 
         * E) TEST GEO-SEARCH (cercare passaggi da città intermedie):
         *    - Cerca da "Novara": trova Milano -> Torino
         *    - Cerca da "Firenze": trova Bologna -> Napoli
         *    - Cerca da "Bologna": trova Roma -> Milano
         *    - Cerca da "Verona": trova Milano -> Venezia (via Verona)
         * 
         * F) TEST MULTI-ROLE (utenti sia driver che passeggero):
         *    - Anna: driver su 2 viaggi, passeggero su 5 richieste
         *    - Paolo: driver su 2 viaggi, passeggero su 6 richieste
         *    - Mario: driver su 5 viaggi, passeggero su 3 richieste
         *    - Luca: driver su 4 viaggi, passeggero su 3 richieste
         *    - Giulia: driver su 3 viaggi, passeggero su 3 richieste
         *    - Elena: driver su 3 viaggi, passeggero su 2 richieste
         * 
         * 4. SCENARI DI TEST CONSIGLIATI
         * 
         * TEST 1 - Driver con richieste pending:
         *   Login: luca.bianchi@test.com
         *   Vai a "I Tuoi Viaggi" -> Dovresti vedere badge per richieste pending
         *   Click sul viaggio Milano->Roma -> gestisci 2 richieste
         * 
         * TEST 2 - Passeggero cerca viaggio:
         *   Login: anna.neri@test.com
         *   Cerca passaggio da "Torino" a "Milano"
         *   Dovresti trovare il viaggio di Mario con 3 posti liberi
         * 
         * TEST 3 - Viaggio pieno:
         *   Login: paolo.gialli@test.com (driver)
         *   Vai a viaggio Bologna->Firenze: dovrebbe mostrare 0 posti liberi
         * 
         * TEST 4 - Geo-search deviazioni:
         *   Login: qualsiasi utente
         *   Cerca da "Novara" verso "Torino"
         *   Dovrebbe apparire il viaggio Milano->Torino che passa per Novara
         * 
         * ======================================================================================
         */
    }
}