# Crypto Exchange - .NET 10 & React 18 & Vite & Docker

Gerçek zamanlı kripto para fiyat takibi yapabilen, kullanıcı kimlik doğrulama sistemi içeren modern bir web uygulaması.

## Özellikler

- **Gerçek Zamanlı Fiyat Takibi**: OKX WebSocket API üzerinden canlı kripto para fiyat güncellemeleri
- **Kullanıcı Kimlik Doğrulama**: JWT token tabanlı güvenli giriş ve kayıt sistemi
- **WebSocket Desteği**: Backend ve frontend arasında gerçek zamanlı veri iletişimi
- **Modern UI**: React ve Vite ile geliştirilmiş responsive kullanıcı arayüzü
- **Docker Desteği**: Kolay kurulum ve dağıtım için Docker ve Docker Compose yapılandırması
- **RESTful API**: Swagger/OpenAPI dokümantasyonu ile REST API

## Teknolojiler

### Backend
- **.NET 10.0**: ASP.NET Core Web API
- **Entity Framework Core**: SQLite veritabanı ORM
- **JWT Bearer Authentication**: Güvenli kimlik doğrulama
- **WebSocket**: Gerçek zamanlı veri iletişimi
- **BCrypt**: Şifre hashleme
- **Swagger/OpenAPI**: API dokümantasyonu

### Frontend
- **React 18**: Kullanıcı arayüzü kütüphanesi
- **Vite**: Modern build tool ve dev server
- **React Router**: Sayfa yönlendirme
- **Axios**: HTTP istekleri
- **WebSocket Client**: Gerçek zamanlı veri alımı

### DevOps
- **Docker**: Containerization
- **Docker Compose**: Multi-container orchestration
- **Nginx**: Frontend web server
- **SQLite**: Veritabanı

## Proje Yapısı

```
crypto-exchange/
├── API/                    # Backend .NET API
│   ├── Controllers/        # API endpoint'leri
│   ├── Services/           # İş mantığı servisleri
│   ├── Data/              # Veritabanı context ve entity'ler
│   ├── Repositories/      # Veri erişim katmanı
│   ├── Interfaces/        # Servis arayüzleri
│   ├── Middlewares/       # Custom middleware'ler
│   ├── Websocket/         # WebSocket yönetimi
│   ├── DTOs/              # Veri transfer objeleri
│   └── Migrations/        # EF Core migrations
│
├── Client/                # Frontend React uygulaması
│   ├── src/
│   │   ├── components/    # React bileşenleri
│   │   ├── pages/         # Sayfa bileşenleri
│   │   ├── contexts/      # React context'ler
│   │   ├── hooks/         # Custom React hooks
│   │   └── services/      # API servisleri
│   └── public/            # Statik dosyalar
│
└── docker-compose.yml     # Docker Compose yapılandırması
```

## Kurulum

### Gereksinimler

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker](https://www.docker.com/) ve [Docker Compose](https://docs.docker.com/compose/) (opsiyonel)

### Yerel Geliştirme

#### Backend Kurulumu

```bash
cd API

# Bağımlılıkları restore et
dotnet restore

# Veritabanı migration'larını çalıştır
dotnet ef database update

# Uygulamayı çalıştır
dotnet run
```

Backend varsayılan olarak `http://localhost:5001` adresinde çalışacaktır.

#### Frontend Kurulumu

```bash
cd Client

# Bağımlılıkları yükle
npm install

# Geliştirme sunucusunu başlat
npm run dev
```

Frontend varsayılan olarak `http://localhost:5173` adresinde çalışacaktır.

### Docker ile Kurulum

Tüm uygulamayı Docker Compose ile çalıştırmak için:

```bash
# Proje kök dizininde
docker-compose up --build
```

Bu komut:
- Backend API'yi `http://localhost:5001` adresinde başlatır
- Frontend'i `http://localhost:3000` adresinde başlatır

## Yapılandırma

### Backend Yapılandırması

`API/appsettings.json` dosyasında aşağıdaki ayarları yapabilirsiniz:

```json
{
  "ConnectionStrings": {
    "TestConnection": "Data Source=cryptoexchange.db"
  },
  "OkxSettings": {
    "WebSocketUrl": "wss://ws.okx.com:8443/ws/v5/public"
  }
}
```

### Frontend Yapılandırması

WebSocket URL'ini ayarlamak için `.env` dosyası oluşturun:

```env
VITE_WS_URL=ws://localhost:5001/ws/crypto
VITE_API_URL=http://localhost:5001
```

## API Endpoints

### Kimlik Doğrulama

- `POST /api/Auth/Register` - Yeni kullanıcı kaydı
  ```json
  {
    "username": "string",
    "email": "string",
    "password": "string"
  }
  ```

- `POST /api/Auth/Login` - Kullanıcı girişi
  ```json
  {
    "username": "string",
    "password": "string"
  }
  ```

### Kripto Para Fiyatları

- `GET /api/Crypto/prices` - Tüm kripto para fiyatlarını getir (Authentication gerekmez)

### WebSocket

- `ws://localhost:5001/ws/crypto` - Gerçek zamanlı fiyat güncellemeleri için WebSocket bağlantısı

## Kimlik Doğrulama

Uygulama JWT (JSON Web Token) tabanlı kimlik doğrulama kullanır:

1. Kullanıcı kaydı veya giriş yapar
2. Backend JWT token döner
3. Token localStorage'da saklanır
4. Sonraki isteklerde token `Authorization: Bearer <token>` header'ı ile gönderilir

## WebSocket Kullanımı

Backend, OKX WebSocket API'sine bağlanarak kripto para fiyatlarını gerçek zamanlı olarak alır ve tüm bağlı istemcilere yayınlar.

Frontend, WebSocket bağlantısı üzerinden gerçek zamanlı fiyat güncellemelerini alır ve UI'ı otomatik olarak günceller.

## Geliştirme

### Backend Test

```bash
cd API
dotnet test
```

### Frontend Lint

```bash
cd Client
npm run lint
```

### Production Build

#### Backend

```bash
cd API
dotnet publish -c Release
```

#### Frontend

```bash
cd Client
npm run build
```

Build çıktısı `Client/dist` klasöründe oluşur.

## Veritabanı

Uygulama SQLite veritabanı kullanır. Veritabanı dosyası `API/cryptoexchange.db` konumunda oluşturulur.

### Migration Oluşturma

```bash
cd API
dotnet ef migrations add MigrationName
```

### Migration Uygulama

```bash
dotnet ef database update
```

## Docker Detayları

### Backend Container

- Base image: `mcr.microsoft.com/dotnet/aspnet:10.0`
- Port: `80` (container içinde), `5001` (host'ta)
- Environment: `ASPNETCORE_ENVIRONMENT=Development`

### Frontend Container

- Build: Node.js 18 Alpine
- Runtime: Nginx Alpine
- Port: `80` (container içinde), `3000` (host'ta)

## Sorun Giderme

### Backend bağlantı sorunları

- Veritabanı dosyasının yazma izinlerini kontrol edin
- Migration'ların uygulandığından emin olun

### WebSocket bağlantı sorunları

- CORS ayarlarını kontrol edin
- WebSocket URL'inin doğru olduğundan emin olun
- Firewall/proxy ayarlarını kontrol edin

### Frontend build sorunları

- Node.js versiyonunun 18+ olduğundan emin olun
- `node_modules` klasörünü silip `npm install` tekrar çalıştırın

## Lisans

Bu proje case çalışması amaçlı geliştirilmiştir.

---

**Not**: Bu uygulama OKX WebSocket API'sini kullanmaktadır. Production ortamında API rate limit'lerini ve kullanım koşullarını göz önünde bulundurun.

