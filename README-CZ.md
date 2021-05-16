## Factorio Image Converter

Jan Frídl <br/>
janfridl@seznam.cz <br/>
16. 5. 2021

* Úvod
  * Účel dokumentu
    * Vytvoření a upřesnění základů pro budoucí program
  * Konvence dokumentu
    * Důležitost: 1 = nejvyšší
  * Pro koho je dokument určený
    * Převážně pro vývojáře a také pro jakékoliv zájemce o program
  * Kontakty
    * Jan Frídl - janfridl@seznam.cz
  * Odkazy na ostatní dokumenty
    * https://wiki.factorio.com/Blueprint_string_format
* Celkový popis
  * Produkt jako celek
    * Finalní program bude umožňovat uživateli nahrát obrázek, editovat ho dle potřeb a následně jej vložit do hry Factorio
  * Funkce
    * Nahrání obrázku standartního formátu
    * Úprava obrázku
    * Export jako Factorio blueprint string
  * Předpoklady
    * Aplikace bude nejspíše minimálně zatěžovat CPU a RAM s výjímkou extrémních rozlišení
* Požadavky
  * Hardwarové rozhraní
    * Funkční PC
  * Softwarové rozhraní
    * Windows
* Funkční požadavky
  1. Nahrání obrázku
    * Důležitost - 1
    * Vstup: Výběr obrázku pomocí prohlížeče souborů
    * Akce: Uložení obrázku do paměti
    * Výstup: Zobrazení obrázku v programu
  2. Manuální úprava rozlišení
    * Důležitost - 2
    * Vstup: Požadované rozlišení
    * Akce: Změna rozlišení výsledného obrázku
    * Výstup: Aktualizovace náhledu výsledného obrázku
  3. Manuální výběr / úprava palety
    * Důležitost - 2
    * Vstup: Výběr / úprava jedné z předvolených palet
    * Akce: Změna barvy obrázku na barvu palety
    * Výstup: Aktualizace náhledu výsledného obrázku
  4. Automatická úprava rozlišení
    * Důležitost - 3
    * Vstup: Nahrání obrázku
    * Akce: Automatická úprava rozlišení výsledného obrázku na vhodnou hodnotu
    * Výstup: Aktualizace náhledu výsledného obrázku
  5. Automatický výběr palety
    * Důležitost - 3
    * Vstup: Nahrání obrázku
    * Akce: Automatický výběr vhodné palety a změna barvy obrázku na barvu palety
    * Výstup: Aktualizace náhledu výsledného obrázku
  6. Převod na Factorio Blueprint String
    * Důležitost - 1
    * Vstup: Výsledný obrázek
    * Akce: Převod obrázku na bloky které jsou určeny barvou palety a následný převod bloků na Factorio Blueprint String
    * Výstup: String v textovém poli a tlačítko na zkopírování
* Nefunkční požadavky 
  * Výkonnost
    * Minimální prodlevou při práci s obrázky do Full HD
  * Spolehlivost
    * Chyba může nastat pouze při nahrávání obrázku či při práci s extrémním rozlišení
