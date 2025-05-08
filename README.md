# MerchStore

## Utvecklingsmiljö

### HTTPS-konfiguration
För att köra projektet lokalt med HTTPS (krävs för inloggning och säkra cookies), följ dessa steg:

1. Generera utvecklingscertifikat:
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

2. Starta projektet med HTTPS-profilen:
```bash
dotnet run --launch-profile https
```

### Inloggningsuppgifter
- Admin: `admin` / `admin123`
- Kund: `john.doe` / `password123`

### Viktigt
- Projektet kräver HTTPS för att fungera korrekt på grund av säkerhetsinställningar
- Om du får problem med certifikat, kör om certifikatgenereringen
- Se till att använda HTTPS-profilen när du startar projektet

## Teknisk information
- ASP.NET Core MVC
- BCrypt för lösenordshashning
- Cookie-baserad autentisering med säkra inställningar
- HTTPS-krav för säker cookie-hantering