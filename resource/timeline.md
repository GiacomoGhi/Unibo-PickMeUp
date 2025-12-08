## Timeline Progetto Pick-Me-Up Aziendale

---

### 2025-09 – Ricognizione Cliente

Eseguita una ricerca sulla storia dell'Azienda e preso contatto con alcuni dipendenti, abbiamo chiarito che poteva essere il giusto target per la nostra idea di App.
Preso contatto ed illustrata l'idea, portando a riferimento soluzioni simili già presenti sul mercato, ci siamo offerti di realizzare per loro una web-app (customizzabile ed integrabile alla intranet aziendale), che sposasse alcune questioni che stanno particolarmente a cuore al team HR del cliente.
Il cliente ci ha messo a disposizione i dati aggregati relativi alle survey interne a cui facevano riferimento e sui quali abbiamo ragionato, definendo il dominio di utilizzo (interno) e i casi d’uso da prendere in esame.
Concordati i requisiti funzionali di alto livello e definita l’infrastruttura IT adeguata, tra quelle messe a disposizione dal cliente, abbiamo chiesto e ottenuto i dati aggregati relativi alla popolazione aziendale in base a:

1.  Genere
2.  Distribuzione geografica
3.  Settore di attività
4.  Età
5.  Titolo di studio

In oltre, ci sono stati forniti ad alcuni suggerimenti, pertinenti, tra quelli proposti proprio dai dipendenti in occasione delle stesse survey e che HR aveva già selezionato al tempo.

#### Conclusioni:

- Non possiamo pubblicare o distribuire i documenti e i dati forniti dal cliente
- Utilizzeremo queste indicazioni per definire i profili delle personas
- I focus group saranno eseguiti ricorrendo a persone esterne al team del cliente
- Gli user test saranno eseguiti con un gruppo di 5 persone, eterogenee, selezionate dal cliente
- Programmiamo un SAL mensile con il cliente, da svolgere in coda agli user test
- L’applicazione dovrà essere utilizzabile anche stand-alone e presso altri clienti

---

### 2025-09 – Personas

Analizzando i dati della survey abbiamo identificato 3 profili target, basati secondo 3 caratteristiche:

1.  Orario di lavoro
2.  Flessibilità
3.  Generazione di appartenenza

Da questi 3 profili abbiamo definito 6 personas:

- 3 uomini e 3 donne (la forza lavoro in azienda è quasi perfettamente bilanciata tra uomini e donne)
- Abbracciano l’intero spettro di età (media), generazioni e profili professionali coinvolti nel progetto
- Per ogni personas abbiamo estrapolato delle caratteristiche/richieste funzionali e non funzionali, che useremo come indicazioni per l’avvio del progetto
- Nei SAL periodici verificheremo e riallineeremo i requisiti alle richieste che emergeranno, ma alla chiusura del progetto verificheremo le premesse iniziali per capire quanto ce ne siamo allontanati.

---

### 2025-10 – Focus Group

Questo focus-group è composto da nostri colleghi di lavoro o parenti (4 maschi e 3 femmine).
Gli proponiamo lo sketch 2025-09, lo confrontiamo con quanto emerso dall’intervista al cliente, dal lavoro sulle personas, e con le richieste estrapolate dalla survey del cliente.
Adattiamo lo sketch in base ai suggerimenti:

- La landing rappresenta la scelta del caso d’uso e solo dopo chiede il login
- Una icona mostra le richieste pending
- La possibilità di rating dei driver
- Pagina di profilo con presentazione e info di contatto (ricercabile e visualizzabile dagli utenti “guest”)
- Memorizzazione dei POI (point of interest) degli utenti
- Censimento e memorizzazione (admin) delle sedi aziendali di riferimento
- I percorsi hanno sempre come partenza o arrivo una sede aziendale (magari condizionato dall’orario)
- Azioni possibili sulle richieste e viaggi inseriti e relative notifiche vi amail
- Necessità di rappresentare i percorsi sulla mappa e non solo come “blog”

Con queste premesse definiamo lo schema ER del DB e raccogliamo le componenti (lib e infrastrutturali) necessarie.

---

### 2025-11 – Focus Group

Lo scopo è quello di presentare una prima bozza di mockup [2025-10], in cui presentiamo solo funzionalità che, ragionevolmente, crediamo di poter integrare.
Inoltre presentiamo le form di input e le notifiche necessarie a gestire i casi d’uso proposti.
Qui abbiamo:

- diverse proposte di landing
- la gestione di light e dark mode
- modalità responsive per passare da desktop e mobile e vice-versa (vedremo poi il flow del layout, in modo che i vari elementi si ridispongano in modo corretto)
- per ora lo presentiamo solo in modalità desktop, perché è il caso d’uso indicato come più frequente dal cliente
- Pagina profilo
- Pagina viaggi
- sarà lo stesso frame, sia per la scelta del viaggio a cui iscriversi, sia per navigare e interagire con tutti i propri viaggi (scegli, accetta, rifiuta, annulla)
- Abbiamo la possibilità di integrare la mappa di google

Abbiamo previsto la possibilità di cercare un utente per vederne le info di contatto, mentre abbiamo cassato la gestione del rating … non essenziale in prima istanza per questo progetto.
Chiarite le funzionalità principali da implementare, dobbiamo rivedere l’intero design per presentarlo al cliente, tenendo conto delle corrette indicazioni di design, fin qui raccolte, oggetto del corso.

---

### 2025-11 – Beta Test

Amici e parenti (6 persone – tra 12 e 70 anni).
Mostriamo il mockup 2025-11 (sessioni registrate con video).
Il mockup è interattivo e mostra sia una versione desktop, sia una versione mobile.
La landing page chiede l’inserimento delle coordinate e data di riferimento, poi chiede login e porta alla pagina di scelta viaggio (guest) o alla lista viaggi (driver).
Rimane la pagina di profilo, ma non la possibilità di ricercare un utente: l’utente può vedere o modificare il proprio profilo oppure visualizzare il profilo del collega, solo dal dettaglio di un viaggio che condividono, cliccando sul nome dell’utente.
Abbiamo uniformato le dialog e le notifiche in ovelay e dato massimo spazio alla mappa.
Gli utenti più giovani hanno scoperto qualche imperfezione nel flow.
In generale, anche gli utenti boomer (una volta compreso che non potevano scrivere nella dialog e che i dati erano precaricati), hanno trovato l’applicazione intuitiva e agevole.
Nella modalità mobile abbiamo ulteriormente aggiornato le notifiche, secondo le ultime indicazioni emerse dal programma di studio e apportiamo le modifiche necessarie al test con il cliente.

---

### 2025-11 – User Test

3 persone del cliente (sessioni registrate).
Rivediamo con il cliente le pagine, gli avvisi e le funzionalità proposte nel mockup 2025-11.
Proponiamo alcune opzioni sul posizionamento e la forma dei bottoni e rivediamo insieme quanto emerso dal Focus Group precedente.

- La landing rappresenta la scelta del caso d’uso e solo dopo chiede il login
- Va bene avere già la form da compilare con le coordinate o mantenere l’approccio dello sketch
- Vedremo quello che poi funziona meglio una volta implementato
- Devono essere rese più evidenti le richieste pending
- OK eliminare la possibilità di rating dei driver
- Eliminare anche la pagina di profilo con presentazione e info di contatto (i contatti avverranno tramite le altre applicazioni, di comunicazione, già integrate nella suite aziendale)
- OK per utilizzare la APP come stand alone e non integrata nella intranet, in modo che gli utenti possano chiedere e vedere i viaggi anche senza essere nella rete aziendale (purché autenticato con SSO aziendale tramite google)
- Con l’eliminazione dei profili così strutturati, eliminiamo anche la gestione POI e Sedi, ma ogni utente potrà vedere lo storico dei viaggi di cui è stato driver (da definire una policy sdi scarto delle informazioni di viaggi e dei singoli utenti)
- Abbiamo cassato il caso d’uso ADMIN, da prevedere in caso di FASE2\*
- I percorsi hanno tragitto libero (non solo da e per le sedi), ma ogni richiesta prevede al massimo una tappa per utente e per viaggio --> non possono inserire 2 richieste per salire e scendere come fosse un autobus --> caso mai questa opzione passa in FASE2\*
- I percorsi sono rappresentati sulla mappa e i dettagli in overlay, a richiesta --> dobbiamo verificare se la mappa sarà interattiva, perché potrebbe richiedere una licenza specifica (attualmente non disponibile)

\* La FASE2 raccoglie le azioni a cui dare seguito in caso volessimo poi portare allo step successivo l’applicazione allo scopo di proporla realmente alla proprietà – naturalmente l’elenco dei requisiti minimi da soddisfare per la fase due sarà definito puntualmente solo in caso di necessità, mentre qui rappresentano meramente questioni considerate in fase di analisi, poi accantonate per necessità di tempo e “budget”.

In generale l’esito è stato molto positivo:

- l’applicazione risponde alle richieste iniziali
- è intuitiva, chiara e di design moderno
- non richiede supporto o istruzioni particolari
- è già integrata a SSO aziendale
- a livello di infrastruttura risulta economica e facilmente gestibile

Il prossimo mockup presentato dovrà mostrare già la soluzione “finale”, quindi solo elementi effettivamente implementabili.

---

### 2025-12 – Focus Group (Interno)

Lavoriamo al mockup 2025-12.
Lo scopo è quello di rendere il mockup i più vicino possibile a quello che andremo a mettere in produzione e soprattutto di ricondurre ogni componente a quelli più standard di Bootstrap; inoltre dobbiamo applicare ogni accortezza di design illustrata durante il corso e quelle emerse nel corso dello user test (soprattutto i boomer-test) anche a costo di portarle all’eccesso.

- Torniamo alla modalità dicotomica: l’applicazione presenta subito le due modalità di utilizzo, Driver e Guest
- Adottiamo una palette di colori più limitata, consona ai messaggi da veicolare e idonea alla modalità chiara, come alla scura
- Ripiliamo da ogni dettaglio non indispensabile (il mockup rimane un elemento dimostrativo – per la demo – poi gli affiancheremo la demo vera e propria, sull’applicazione implementata, per il test funzionale vero e proprio)
- I messaggi sono rappresentati in modale e con eventuali tasti dedicati alle azioni/opzioni indicate
- Le notifiche a video sono sempre dettagliate, contestualizzate, formattate bene e formulate con attenzione
- Le soft delete e alcune azioni critiche sono seguite da notifiche che consentono la rollback dell’azione
- Su indicazione degli utenti rappresentiamo i dettagli del viaggio, accanto alla mappa, ma questo in mobile naturalmente non è funzionale, quindi nell’applicazione ci limiteremo a rendere più evidente come attivare il relativo overlay – ben curata
- Tralasciamo ogni azione e funzionalità più standard (comunque implementata) e banale (accantonata) per concentrare i test utente sulle pagine e le funzioni core del progetto
- La gestione del profilo sparisce totalmente (l’eventuale scenario d’uso da ADMIN) è rinviato alla FASE2
- L’aggiunta di un Viaggio, passa per una form totalmente rivista e porta alla pagine “I Tuoi Viaggi”
- Ne “I Tuoi Viaggi” abbiamo previsto 3 “selettori”, che appaiono sono a determinate condizioni e che fanno anche da indicatori per le diverse “istanze” di viaggio o richieste pending in lista, oltre alla possibilità di utilizzare i normali filtri di ricerca, ma ora “a scomparsa”, così che non disturbino l‘utente durante la UX
- Quando “Chiedi un Passaggio” sei rinviato ad una Vista filtrata dei viaggi disponibili; questa può essere ulteriormente raffinata con i filtri aggiuntivi. Selezionato il Viaggio di interesse, se ne vede il dettaglio ed è possibile aggiungervi un'unica “tappa”
- Purtroppo, abbiamo la possibilità di mostrare una mappa, ma non di interagirci, perché richiederebbe una licenza speciale, a pagamento – la selezione al momento non può essere che da una lista, ma abbiamo scelto una lista di elementi, invece che tabellare dei semplici link o post
- Il driver accetta o rifiuta le richieste di pickup in attesa direttamente dal frame di dettaglio del suo viaggio
- L’utente guest può annullare la richiesta in qualsiasi momento, annullando il suo (tratto) di viaggio
- Il viaggio può essere annullato dal Driver in qualsiasi momento (o con la cancellazione dell’account)
- Ogni azione scatena relativi alert, richieste di conferma e notifiche

---

### 2025-12 – User Test (Con il Cliente)

Rappresentiamo in Figma come si presentano le varie finestre tra il mockup 2025-12 e l’applicazione implementata.
Il design che rappresenta il doppio scopo dell’applicazione, si ripropone nello stile delle pagine di accounting (log-in, registrazione, ecc.).
Mettiamo in evidenza le differenze tra il mockup e la resa della soluzione nella versione implementata, confrontando le varie pagine nella modalità chiara, scura e mobile.

- L’applicazione risponde bene ai vari scenari testati, anche in modalità “multi utente”
- Apprezzata la mappa a tutto schermo e i dettagli in overlay ben rappresentati, anche grazie all’impiego di colori
- Emergono alcuni “comportamenti” di default di alcuni componenti che devono essere raffinati per allinearli meglio alle aspettative (trovano tutti spazio nei commenti in FIGMA)

---

### 2025-12 – Final

In questa fase completiamo e pubblichiamo al Cliente la presentazione del progetto [CANVA](https://www.canva.com/design/DAG1lKlyGjw/SXHpjLWYVulxo5cQtBjKQw/view)

Il Progetto scorre le fasi di analisi, dalla definizione del contesto, fino alla descrizione degli scenari rappresentati nella **customerjouney**
