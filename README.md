  # Simulátor hokejových turnajů

**Autor:** Jakub Trejbal (st72584)  


## Popis projektu

Simulátor hokejových turnajů

## Hlavní funkce

1. **Vytváření a editace entit**  
   - Hráči (útočníci, obránci, brankáři)  
   - Týmy  
   - Turnaje  

2. **Automatické generování zápasů**  
   - Round-robin systém (každý s každým)  

3. **Grafické zobrazení zápasu**  
   - Kluziště s pozicemi hráčů  
   - Výsledková tabule  
   - Dynamické střídání lajn v reálném čase  

4. **Realistická simulace zápasů**  
   - Ovlivněna individuálními statistikami hráčů (střelba, přihrávky, obrana, bruslení, celkové hodnocení)  
   - Náhodné prvky  
   - Role hráče (Playmaker, Sniper, TwoWay, Offensive, Defensive atd.)  

5. **Statistiky**  
   - Záznam výsledků zápasu  
   - Individuální statistiky hráčů (góly, asistence, střely)  

6. **Import / Export dat**  
   - Ukládání a načítání dat ve formátu JSON  

7. **Persistience dat**  
   - Použití LiteDB (lokální NoSQL databáze)  

## Návod na spuštění a používání (lze využít testingData.jsonu v rootu projektu)

1. **Vytvoření turnaje**  
   - Vyžaduje výběr minimálně 2 týmů  

2. **Zobrazení zápasů**  
   - Po výběru turnaje ve výpisu dat se zobrazí všechny zápasy v pravém dolním panelu zobrazí všechny naplánované zápasy  

3. **Spuštění simulace**  
   - Vyberte konkrétní zápas  
   - Klikněte na tlačítko **PLAY** pod výsledkovou tabulí  
   - Zápas se začne simulovat

4. **Po dokončení simulace**  
   - Výsledek a individuální statistiky hráčů jsou uloženy  
   - Zápas již nelze znovu simulovat (lze se pouze vrátit k výsledkům)  

## Technologie

- .NET 8  
- WPF (Windows Presentation Foundation)  
- CommunityToolkit.Mvvm – MVVM framework  
- LiteDB – lokální databáze  

## Co aktuálně chybí / plánované vylepšení

- Plná inline validace při editaci hráčů
- Automatický generátor týmů a hráčů (pro rychlé testování)  
- Automatické ukládání změn při inline editaci  
- Detailnější statistiky sezóny a tabulka turnaje  
- Možnost play-off formátu