 ```
     _    _                _                        ____ 
    / \  | | ___ __   __ _| | _____ _ __ ___  ___  /_/_/
   / _ \ | |/ / '_ \ / _` | |/ / _ \ '__/ _ \/ __|/ _ \ 
  / ___ \|   <| | | | (_| |   <  __/ | |  __/\__ \ |_| |
 /_/   \_\_|\_\_| |_|\__,_|_|\_\___|_|  \___||___/\___/
```
# Aknakereső telepítése Windows 7-re

Az aknakeresőm telepítése Windows 7 re már nehezebb feladat. Itt vannak a lépések amelyek telepítésével lehet játszani Windows 7-en.

> ⚠️ Nagyon régi Windows 7 verziókat offline Windows update fájlokkal kell frisíteni, amiket [innen](https://www.catalog.update.microsoft.com/Home.aspx) kell letölteni, de már nem elmlékszemmelyikeket

## [Telepítő](../inno-setup/scripts/output/minesweeper_setup.exe) letöltése
## Windows Update ellenőrzése
- Keress frissítéseket a Windows Update ban
- Ha van frissítés telepítsd, majd indítsd újra a géped!
- Ha ez nem megy:
    - Frissítések engedélyezése 
    - Nyomj Windows + R billentyűkombinációt és írd be, hogy `inetcpl.cpl` és nyomj Entert
    - Menj a Speciális fülre
    - Tiltsd le az SSL-t
    - Engedélyezd az összes TLS kapcsolatot
    - Nyomj OK-t
## .NET 4.8 telepítése
- [Töltsd le a .NET 4.8-at](https://go.microsoft.com/fwlink/?LinkId=2085155), majd telepítsd
- **Minden megvan és lehet használni az aknakeresőt**
