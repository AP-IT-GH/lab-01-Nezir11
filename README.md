Obelix MLAgent

Inleiding
In dit labo werd een autonome agent (“Obelix”) ontwikkeld en getraind in een Unity-omgeving met behulp van de ML-Agents toolkit. Het doel was om de agent te leren om menhirs te zoeken, op te nemen en correct af te leveren op destinations. De moeilijkheid van de taak werd stapsgewijs verhoogd van één object naar meerdere objecten, zodat het leervermogen, de efficiëntie en het gedrag van de agent geëvalueerd konden worden.

Methoden

De agent maakt gebruik van Behaviour Parameters met een vector observation space van grootte 6. De observaties bestaan uit:

Of de agent een menhir draagt (0 of 1)
De voortgang van de taak (0 tot 1)
De richting naar de dichtstbijzijnde menhir (x en z)
De richting naar de dichtstbijzijnde destination (x en z)

De agent gebruikt geen Ray Perception Sensor, maar werkt met directionele vectoren naar de dichtstbijzijnde objecten. Hierdoor krijgt de agent directe informatie over waar hij naartoe moet bewegen.

De agent beschikt over twee discrete actie-branches:

Beweging (stil, vooruit, achteruit)
Rotatie (stil, links, rechts)

Het beloningssysteem is opgebouwd binnen de range [-1, 1]:

Kleine straf per stap (-0.001) om traag gedrag te vermijden
Beloning voor het oppakken van een menhir (+0.3)
Beloning voor het afleveren (+0.7)
Straf bij vallen (-1.0)
Straf bij een verkeerde interactie (-0.2)

Daarnaast wordt een directionele beloning toegevoegd op basis van de richting waarin de agent kijkt ten opzichte van zijn doel. Dit helpt de agent om sneller naar menhirs en destinations te navigeren.

De omgeving wordt bij elke episode opnieuw gegenereerd via OnEpisodeBegin(). Hierbij worden menhirs en destinations willekeurig gespawned binnen een bepaalde spawnRange, waarbij een minimale afstand tussen objecten wordt gerespecteerd om overlap te voorkomen. In dit experiment werd de spawnRange verkleind om de taak in de beginfase eenvoudiger te maken.

De training werd opgebouwd via curriculum learning. Eerst werd de agent getraind met één menhir, waarna de taak werd uitgebreid naar meerdere menhirs (tot zes). Dit zorgt ervoor dat de agent eerst de basis leert voordat de complexiteit wordt verhoogd.

Resultaten

De trainingsresultaten werden geanalyseerd met TensorBoard.
Scenario A (1 menhir)

In deze fase leert de agent snel de basis van de taak. De cumulatieve reward stijgt snel van negatieve waarden naar positieve waarden en stabiliseert. De agent begrijpt hoe hij een menhir moet oppakken en afleveren. De episode length blijft relatief laag, wat wijst op efficiënt gedrag.

Scenario B (meerdere menhirs zonder optimalisaties)

Wanneer meerdere objecten worden toegevoegd, wordt de taak duidelijk moeilijker. De cumulatieve reward daalt in het begin en de episode length stijgt sterk. Dit toont aan dat de agent moeite heeft met navigatie en vaak inefficiënt gedrag vertoont, zoals rondlopen zonder doel of vast blijven hangen.

Scenario C (verbeterde agent met direction en shaped rewards)

<img width="1117" height="747" alt="image" src="https://github.com/user-attachments/assets/ebe99734-95d1-4134-8437-467c9f71e9a2" />


Na het toevoegen van directionele observaties en extra beloningen is een duidelijke verbetering zichtbaar.

De Cumulative Reward grafiek toont een stijging van ongeveer -4 naar waarden rond 0 en zelfs positief. Dit betekent dat de agent steeds vaker succesvolle acties uitvoert.

De Episode Length grafiek daalt na verloop van tijd, wat aangeeft dat de agent sneller oplossingen vindt en efficiënter wordt in zijn gedrag.

De histogramgrafiek verschuift richting hogere waarden, wat betekent dat slechte episodes minder vaak voorkomen en de agent consistenter presteert.

De Policy Loss blijft relatief stabiel, wat wijst op een stabiel leerproces. De Value Loss stijgt licht, wat logisch is doordat de agent met een complexer beloningssysteem werkt en meer informatie moet verwerken.

Conclusie

De resultaten tonen aan dat de Obelix-agent effectief leert via reinforcement learning. In de eerste fase leert de agent de basis van het oppakken en afleveren van objecten. Wanneer de taak complexer wordt, blijkt dat extra informatie en een beter beloningssysteem noodzakelijk zijn.

Door het toevoegen van directionele observaties en shaped rewards verandert het gedrag van de agent van willekeurig naar doelgericht. De agent leert niet alleen wat hij moet doen, maar ook hoe hij dit efficiënt kan uitvoeren.

Belangrijkste inzichten uit dit experiment:

Enkel eindbeloningen zijn niet voldoende voor complexe taken
Goede observaties zijn essentieel voor het leerproces
Shaped rewards versnellen de training aanzienlijk
Curriculum learning (van 1 naar meerdere objecten) zorgt voor stabielere en snellere learning

Uiteindelijk is de agent in staat om zelfstandig menhirs te vinden, op te nemen en af te leveren op een efficiënte manier. Dit toont aan dat reinforcement learning in Unity geschikt is voor het oplossen van navigatie- en taakgebaseerde problemen.

Referenties
Unity ML-Agents Toolkit
Unity: A General Platform for Intelligent Agents (arXiv)
