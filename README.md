Rapport – ML-Agents agent met target en groene zone
1. Titel
Training van een Unity ML-Agents agent die eerst het target zoekt en daarna een groene zone bereikt.

2. Inleiding
In deze oefening werd oefening 1 uitgebreid met een extra doel. De agent moet eerst het target vinden in de omgeving. Wanneer de agent het target aanraakt, verdwijnt het target. Daarna moet de agent naar een groene zone bewegen. Als de agent deze zone bereikt, eindigt de episode.
Het doel van dit experiment is om een agent te trainen die meerdere stappen in de juiste volgorde kan uitvoeren.

3. Methoden
Behaviour Parameters
De agent gebruikt 2 continue acties:
- vooruit bewegen
- draaien
Hierdoor kan de agent zich vrij bewegen in de omgeving.

Agent
De agent is gemaakt met een script dat gebaseerd is op de Unity ML-Agents Agent class.
Belangrijke onderdelen:
OnEpisodeBegin()
- De agent wordt terug op de startpositie gezet. Het target krijgt een willekeurige positie en wordt opnieuw zichtbaar gemaakt.

OnActionReceived()
- Hier worden de acties uitgevoerd (bewegen en draaien).
- De agent krijgt een kleine negatieve reward per stap zodat hij sneller een oplossing leert zoeken. Als de agent van het platform valt, krijgt hij een straf en stopt de episode.

OnTriggerEnter()
Wanneer de agent het target raakt:
- krijgt de agent een reward
- verdwijnt het blok
Wanneer daarna de groene zone wordt bereikt:
- krijgt de agent een grotere reward
- eindigt de episode.

4. Resultaten
Tijdens het trainen beweegt de agent in het begin willekeurig. Na meerdere episodes leert de agent sneller het target te vinden en daarna naar de groene zone te gaan. Uiteindelijk bereikt de training een plateau waarbij het gedrag stabiel wordt.

5. Conclusie
De agent leert via reinforcement learning om eerst het target te zoeken en daarna de groene zone te bereiken. Het beloningssysteem helpt de agent om efficiënt gedrag aan te leren.
