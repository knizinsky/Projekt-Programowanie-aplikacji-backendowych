# Dokumentacja Aplikacji Sklepu Spożywczego ASP.NET

## Spis Treści
- [Wstęp](#wstęp)
- [Wymagania Systemowe](#wymagania-systemowe)
- [Instalacja](#instalacja)
- [Konfiguracja](#konfiguracja)
- [Uruchamianie Aplikacji](#uruchamianie-aplikacji)
- [Działanie Aplikacji](#działanie-aplikacji)

## Wstęp
Ten projekt jest aplikacją backendową dla sklepu spożywczego. Został zaprojektowany z myślą o efektywnej obsłudze operacji sklepu, takich jak zarządzanie produktami, zamówieniami i klientami. Aplikacja została napisana w języku C# z użyciem ASP.NET Core.

## 🖥️ Wymagania Systemowe
Aby uruchomić aplikację, komputer musi spełniać następujące wymagania:

- **System Operacyjny:** Windows, Linux, lub macOS
- **Framework:** .NET 6.0
- **Przeglądarka:** Zalecane korzystanie z najnowszych wersji przeglądarek, takich jak Google Chrome, Mozilla Firefox, lub Microsoft Edge.
- **System do zarządzania bazą danych:** Zalecane jets korzystanie z SQL Server Management Studio.

## ⬇️ Instalacja
Aby zainstalować aplikację, wykonaj poniższe kroki:

1. Pobierz kod źródłowy z repozytorium GitHub.
2. Otwórz projekt w środowisku programistycznym, na przykład Visual Studio.
3. Zainstaluj wymagane zależności za pomocą menadżera pakietów NuGet.
4. Otwórz SSMS i utwórz bazę danych.
5. Zaktualizuj ustawienia połączenia w pliku appsettings.json oraz ApplicationDbContext.cs, aby wskazywał na poprawną bazę danych.
6. Dodaj migrację poleceniem `Add-Migration [nazwa migracji]` w konsoli menedżera pakietów NuGet, w folderze WebAPI,
7. Utwórz odpowiednie tabele w bazie poleceniem `Update-Database` w konsoli menedżera pakietów NuGet, w folderze WebAPI,
8. Uruchom aplikację.

## 🛠️ Konfiguracja
### Łańcuch Połączenia z Bazą Danych
Konfiguracja łańcucha połączenia z bazą danych znajduje się w pliku `appsettings.json` oraz `ApplicationDbContext.cs`. Edytuj sekcję `ConnectionStrings` i dostosuj łańcuch połączenia według potrzeb:

```json
"Server=ACERASPIRE5\\SQLEXPRESS;Database=Grocery1;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

### 🔑 Dane Testowych dla Użytkowników
Poniżej znajdują się dane logowania dla admina oraz zwykłego użytkownika:

**Admin**:
- **Login:** admin
- **Hasło:** !Administrator123

**Użytkownik**:
- **Login:** user
- **Hasło:** !User123


## ▶️ Uruchamianie Aplikacji
Aplikację można uruchomić, korzystając z dowolnego środowiska programistycznego obsługującego .NET 6.0. W przypadku korzystania z Visual Studio, kliknij przycisk "Start" lub użyj poniższej komendy w terminalu:

```bash
dotnet run
```

## 🖥️ Działanie Aplikacji
Autoryzacja użytkownika jest realizowana za pomocą tokenu JWT po wysłaniu żądania z poprawnymi danymi logowania. Dokumentacja endpointów jest dostępna w swaggerze po uruchomieniu projektu WebApi. Komendy rejestracji oraz logowania nie wymagają autoryzacji. Wszystkie polecenia usuwania są zabezpieczone, aby tylko administratorzy mieli prawo do ich wykonania. Aplikacja umożliwia kompleksowe zarządzanie użytkownikami, klientami, zamówieniami i pozycjami zamówień. Funkcje te obejmują tworzenie, pobieranie, aktualizowanie i usuwanie danych we wszystkich wymienionych kategoriach, co pozwala na pełne zarządzanie cyklem życia klienta oraz jego zamówień.

