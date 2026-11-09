# Bomberman – žaidimas su Abstract Factory, Builder ir Decorator

Java Swing klientas ir C# (.NET 9 / ASP.NET Core SignalR) serveris.

## Paleidimas kaip ankstesnėje versijoje (be Maven)

Reikia **JDK 17 ar naujesnio** (`java` ir `javac`) ir **.NET 9 SDK** (`dotnet`). 

### 1. Paleisti serverį
- Dukart spustelėkite `Paleisti_Serveri.bat` **arba** VS Code terminale: `cd Server` → `dotnet run`.
- Serveris klausosi `http://localhost:5000` (WebSocket `ws://localhost:5000/game`).

### 2. Paleisti vieną klientą
- Dukart spustelėkite `Paleisti_Klienta.bat` (automatiškai sukompiliuoja Java kodą su `javac`, tada paleidžia `bomberman.Main`).
- **Arba kaip anksčiau:** VS Code atidarykite projektą `Bomberman-main`, failą `Client/src/main/java/bomberman/Main.java`, virš `main` paspauskite **Run Java** / **Run**. Reikalinga VS Code „Extension Pack for Java“.
- **Arba** iš `Client` katalogo: `run-client.bat`.
- Norint prisijungti antram žaidėjui, dar kartą paleiskite `Paleisti_Klienta.bat`.

**Svarbu:** vienam žaidėjui pakanka **vieno** kliento lango, bet serverį paleisti vis tiek būtina. `Main.java` išsaugotas ir lieka pagrindinis įėjimo taškas, kaip pradinėje versijoje.

### Patikra, jei nepasileidžia
VS Code terminale: `java -version`, `javac -version`, `dotnet --version`. JDK turi būti 17+, .NET SDK – 9.0. Jei `javac` neatpažįstamas, reikia JDK (ne tik JRE) ir jo `bin` katalogo Windows `PATH`.

## Valdymas
- Rodyklės – judėjimas
- `Space` – padėti bombą
- `Z` – Undo
- `T` – perjungti **Standard / Neon** grafikos temas (Abstract Factory)

## Ryšio realizacija
`Client/src/main/java/bomberman/network/SignalRWebSocket.java` naudoja JDK `java.net.http.WebSocket` ir minimalų SignalR JSON Hub Protocol. `SignalRJson.java` apdoroja klientui aktualias JSON žinutes. Dėl to Java klientui **nereikia** `com.microsoft.signalr` ir kitų išorinių bibliotekų. Tai ne bendros paskirties SignalR biblioteka, o šiam projektui skirtas protokolo poaibis.

**Apribojimai:** šioje aplinkoje Java kodas sukompiliuotas ir tinklo mainai patikrinti su bandomuoju WebSocket/SignalR protokolo serveriu. Pilnas C# serverio paleidimas ir abiejų klientų integracija nėra patikrinti, nes nėra .NET 9 SDK.

