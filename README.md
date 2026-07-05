# WalletLedger

Digitalni novčanik i sistem za evidenciju finansijskih transakcija (ledger), razvijen kao projekat u okviru predmeta Napredne .NET tehnologije na Fakultetu organizacionih nauka, Univerzitet u Beogradu.

WalletLedger simulira pojednostavljenu digitalnu bankarsku platformu (slično Revolut-u ili PayPal-u) u kojoj korisnici mogu da kreiraju novčanike u više valuta, uplaćuju i podižu sredstva, prenose novac drugim korisnicima, konvertuju između valuta, plaćaju račune, i pregledaju kompletnu, nepromenljivu istoriju svake akcije izvršene na svom nalogu.

## Zašto ovaj projekat

Većina studentskih projekata tipa "novčanik" ili "praćenje troškova" koristi jednostavan CRUD model gde je stanje na računu samo broj koji se prepisuje. Ovaj projekat umesto toga tretira stanje novčanika kao nešto što mora biti **proverivo** - svaka akcija koja menja stanje beleži se kao poseban događaj, pored operativnog zapisa transakcije, tako da kompletna istorija toga kako je stanje došlo do trenutne vrednosti uvek može da se rekonstruiše i pregleda.

Cilj je bio kombinovati solidne arhitekturne obrasce (Clean Architecture, CQRS, Repository/Unit of Work) sa domenom u kome ti obrasci imaju stvaran razlog da postoje, umesto da budu dodati radi sebe samih.

## Funkcionalnosti

- **Autentifikacija** - registracija i prijava preko ASP.NET Core Identity, JWT bearer tokeni
- **Novčanici u više valuta** - svaki korisnik može imati više novčanika (RSD, EUR, USD, BAM), svaki sa sopstvenim brojem računa
- **Osnovne operacije** - uplata, isplata, transfer (preko broja računa), konverzija valuta (između sopstvenih novčanika korisnika, po fiksnom ilustrativnom kursu)
- **Plaćanje računa** - specijalizovan tok isplate za redovne uplate (komunalije, telekom, itd.) sa referencom primaoca
- **Životni ciklus transakcije** - transakcije prolaze kroz formalno definisanu mašinu stanja (`Pending → Completed / Failed`, `Completed → Reversed`), implementiranu preko [Stateless](https://github.com/dotnet-state-machine/stateless) biblioteke umesto ručno pisanih status polja
- **Odloženo automatsko odobrenje** - transferi se kreiraju kao `Pending` i automatski prelaze u `Completed` posle kratkog vremenskog perioda, simulirajući period provere/obrade
- **Storniranje transakcija** - završene uplate, isplate i transferi mogu biti stornirani, čime se vraćaju pogođena stanja
- **Prikaz rezervisanih sredstava** - iznosi transakcija na čekanju prikazuju se odvojeno od potvrđenog stanja
- **Audit log** — svaka operacija koja menja stanje dodatno upisuje nepromenljiv zapis događaja (ID agregata, tip događaja, JSON payload, vremenska oznaka), nezavisno od operativne tabele transakcija i nikad se ne prepisuje
- **Statistika** - pregled transakcija po tipu i mesečnog priliva/odliva po novčaniku, vizuelizovano preko Chart.js
- **Provera vlasništva** - svaki endpoint vezan za novčanik proverava da li ulogovani korisnik zaista poseduje novčanik kome pristupa
- **Brisanje novčanika** - prazan novčanik bez istorije transakcija može biti obrisan

## Arhitektura

Backend prati **Clean Architecture** sa četiri projekta, gde zavisnosti idu ka centru:

```
WalletLedger.Domain          → entiteti, enumi, interfejsi repozitorijuma (bez spoljnih zavisnosti)
WalletLedger.Application      → CQRS komande/upiti (MediatR), DTO-ovi, poslovna logika, validatori
WalletLedger.Infrastructure   → EF Core, SQLite, Identity, JWT, implementacije repozitorijuma
WalletLedger.API              → kontroleri, middleware, kompozicioni koren aplikacije
```

**Ključni obrasci:**

- **CQRS preko MediatR-a** - svaki slučaj korišćenja je `Command` (upis) ili `Query` (čitanje) sa sopstvenim handlerom, organizovano kao vertikalne celine unutar `Features/Wallets/{Commands,Queries}/{Operacija}/`
- **Repository pattern** - `IWalletRepository` odvaja perzistenciju od Application sloja; konkretna EF Core implementacija živi u Infrastructure sloju
- **Beleženje događaja kao audit trag** - generalizovana `StoredEvent` tabela (indeksirana po ID-ju/tipu agregata, ne vezana za jedan konkretan entitet) beleži šta se dogodilo, paralelno sa operativnim tabelama. Ovo je namerno pojednostavljena verzija event sourcing-a: operativne tabele (`Wallet.Balance`, `Transaction`) ostaju primarni put za čitanje radi performansi i jednostavnosti, dok `StoredEvent` obezbeđuje nezavisan, append-only zapis u svrhu revizije. Nije reč o punoj rekonstrukciji stanja iz eventova.
- **Mašina stanja za status transakcije** - validni prelazi statusa eksplicitno su definisani preko Stateless biblioteke, tako da transakcija nikad ne može preći iz jednog statusa u drugi na način koji nije modelovan (npr. `Failed → Reversed` nije dozvoljen prelaz i baciće grešku)

## Tehnologije

**Backend:** ASP.NET Core Web API (.NET), Entity Framework Core, SQLite, ASP.NET Core Identity, JWT Bearer autentifikacija, MediatR, Stateless, FluentValidation

**Frontend:** Angular (standalone komponente, signals), Chart.js

## Struktura projekta

```
NNT/
├── WalletLedger.Domain/
├── WalletLedger.Application/
│   └── Features/Wallets/{Commands,Queries}/
│   └── Common/{Factories,Providers,Helpers,Dtos}/
├── WalletLedger.Infrastructure/
│   ├── Data/            (AppDbContext, migracije)
│   ├── Repositories/
│   └── Services/         (JWT, Event Store)
├── WalletLedger.API/
│   └── Controllers/
└── wallet-ledger-client/ (Angular aplikacija)
```

## Pokretanje projekta

**Backend**
```bash
cd WalletLedger.API
dotnet ef database update --project ../WalletLedger.Infrastructure --startup-project .
dotnet run
```
API dostupan na `https://localhost:7040` (Swagger na `/swagger`).

**Frontend**
```bash
cd wallet-ledger-client
npm install
ng serve
```
Aplikacija dostupna na `http://localhost:4200`.

## Autor

Anja Perović - Fakultet organizacionih nauka, Univerzitet u Beogradu
