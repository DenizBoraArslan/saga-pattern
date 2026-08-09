# Library Saga — Choreography vs Orchestration

Saga pattern'in iki temel yaklaşımını (**Choreography** ve **Orchestration**) aynı domain üzerinde sıfırdan implemente ederek karşılaştırmalı olarak öğrenmek amacıyla geliştirilmiş bir proje. .NET 8, MassTransit, RabbitMQ ve Entity Framework Core kullanılarak, aynı iş akışı iki farklı mimari yaklaşımla iki kez inşa edilmiştir.

---

## Senaryo

Bir kütüphane ödünç alma sistemi. Bir üye kitap ödünç almak istediğinde:

1. **Membership** — üyenin uygunluğu kontrol edilir (aktif üyelik, gecikmiş kitap yok).
2. **Catalog** — kitabın stoğu kontrol edilip rezerve edilir.
3. **Loan** — ödünç kaydı oluşturulur.
4. Herhangi bir adımda hata olursa (üye uygun değil, stok yetersiz), süreç **compensation** ile otomatik iptal edilir.

İki proje birbirinden bağımsız altyapı (RabbitMQ + SQL Server container'ları) kullanır ve **aynı anda paralel çalıştırılabilir.**

---

## Proje Yapısı

```
saga-choreography/
├── Contracts/              # Event tipleri
├── Trigger.Api/             # Saga'yı başlatan tek kapı (POST /api/Loans)
├── Membership.Worker/       # LoanRequestedEvent dinler
├── Catalog.Worker/          # MembershipVerifiedEvent dinler
├── Loan.Worker/             # StockReserved / *Failed event'lerini dinler
└── docker-compose.yml       # RabbitMQ (5672/15672) + SQL Server (1433)

saga-orchestration/
├── Contracts/                # Command + Event tipleri
├── Trigger.Api/               # Saga'yı başlatan tek kapı
├── Loan.Orchestrator/         # State machine — tüm akışı yönetir
├── Membership.Worker/         # VerifyMembershipCommand dinler
├── Catalog.Worker/            # ReserveStockCommand dinler
├── Loan.Worker/                # CreateLoanCommand dinler
└── docker-compose.yml         # RabbitMQ (5673/15673) + SQL Server (1434)

## Event/Akış Diyagramları

### Choreography

```
LoanRequestedEvent
        │
        ▼
MembershipVerifiedEvent ──✗──► MembershipVerificationFailedEvent ──► Loan: Cancelled
        │
        ▼
StockReservedEvent ──✗──► StockReservationFailedEvent ──► Loan: Cancelled
        │
        ▼
Loan: Completed
```

### Orchestration (State Machine)

```
Initial
   │ LoanRequested
   ▼
AwaitingMembershipVerification
   │ MembershipVerified          │ MembershipVerificationFailed
   ▼                             ▼
AwaitingStockReservation      Failed
   │ StockReserved               │ StockReservationFailed
   ▼                             ▼
AwaitingLoanCreation           Failed
   │ LoanCreated
   ▼
Completed
```

---

## Kullanılan Teknolojiler

- **.NET 8** — Worker Service (arka plan servisleri) + ASP.NET Core Web API
- **MassTransit 8.5.1** — mesajlaşma soyutlama katmanı + saga state machine desteği
- **RabbitMQ** — mesaj kuyruğu (broker), her iki proje için ayrı instance
- **Entity Framework Core** — her servisin kendi veritabanı için (worker'larda 8.x, orchestration'ın saga repository'sinde `MassTransit.EntityFrameworkCore`'un getirdiği 9.x)
- **SQL Server** — Docker container, her iki proje için ayrı instance
- **Docker Compose** — yerel geliştirme ortamı

---

## Kurulum ve Çalıştırma

### Gereksinimler

- .NET 8 SDK
- Docker Desktop
- `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`

### Choreography'yi Çalıştırmak

```bash
cd saga-choreography
docker compose up -d

cd Membership.Worker && dotnet ef database update && cd ..
cd Catalog.Worker && dotnet ef database update && cd ..
cd Loan.Worker && dotnet ef database update && cd ..
```

Sonra 4 servisi (Membership.Worker, Catalog.Worker, Loan.Worker, Trigger.Api) ayrı terminallerde `dotnet run` ile başlat.

RabbitMQ UI: `http://localhost:15672` (guest/guest)

### Orchestration'ı Çalıştırmak

```bash
cd saga-orchestration
docker compose up -d

cd Loan.Orchestrator && dotnet ef database update && cd ..
cd Membership.Worker && dotnet ef database update && cd ..
cd Catalog.Worker && dotnet ef database update && cd ..
cd Loan.Worker && dotnet ef database update && cd ..
```

Sonra 5 servisi (Loan.Orchestrator, Membership.Worker, Catalog.Worker, Loan.Worker, Trigger.Api) ayrı terminallerde `dotnet run` ile başlat.

RabbitMQ UI: `http://localhost:15673` (guest/guest)

### Test Etmek

Her iki proje için de `Trigger.Api` ayağa kalktıktan sonra:

```bash
curl -X POST https://localhost:XXXX/api/Loans \
  -H "Content-Type: application/json" \
  -d '{"memberId": "<seed-data-member-id>", "bookId": "<seed-data-book-id>"}'
```

Seed data'daki gerçek `memberId`/`bookId` değerlerini ilgili veritabanlarının `Members`/`Books` tablolarından alabilirsin.

Orchestration'da saga'nın anlık durumunu izlemek için:
```sql
SELECT * FROM LoanSagaState;
```

---

## Öne Çıkan Öğrenimler

Projeyi geliştirirken karşılaşılan mimari trade-off'lar (kurulum karmaşıklığı, "süreç nerede?" sorusuna cevap verme kolaylığı, compensation mantığının merkezi/dağıtık oluşu, saga kaydının `Finalize()` sonrası kaybolması gibi) ve pratik altyapı sorunları (versiyon uyumsuzlukları, port konfigürasyonu) 

---

## Proje Amacı

Bu, production'a yönelik değil, **saga pattern'in iki temel yaklaşımını karşılaştırmalı olarak, elle inşa ederek öğrenmek** amacıyla geliştirilmiş bir öğrenim/portföy projesidir.