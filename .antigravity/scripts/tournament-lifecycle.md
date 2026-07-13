┌────────────────────────────────────────────┐

│            TOURNAMENT LIFECYCLE            │

└────────────────────────────────────────────┘



&#x20;                Admin

&#x20;                  │

&#x20;                  ▼

&#x20;       Create Tournament

&#x20;                  │

&#x20;                  ▼

&#x20;     Set Registration Period

&#x20;                  │

&#x20;                  ▼

&#x20;     Registration Open

&#x20;                  │

&#x20;                  ▼

&#x20;   Horse Owner Register Horse

&#x20;                  │

&#x20;                  ▼

&#x20;     Owner Invite Jockey

&#x20;                  │

&#x20;                  ▼

&#x20;    Jockey Accept / Reject

&#x20;                  │

&#x20;                  ▼

&#x20;     Admin Approve Registration

&#x20;                  │

&#x20;                  ▼

══════════════════════════════════════════════

&#x20;         Medical Check Phase

══════════════════════════════════════════════

&#x20;                  │

&#x20;                  ▼

&#x20;    Veterinarian Examine Horse

&#x20;                  │

&#x20;                  ▼

&#x20;    MedicalResult = Passed ?

&#x20;            │

&#x20;     ┌──────┴──────┐

&#x20;     │             │

&#x20;    No            Yes

&#x20;     │             │

&#x20;     ▼             ▼

Disqualified   Doping Check

&#x20;                   │

&#x20;                   ▼

&#x20;     DopingResult = Negative ?

&#x20;            │

&#x20;     ┌──────┴──────┐

&#x20;     │             │

&#x20;    No            Yes

&#x20;     │             │

&#x20;     ▼             ▼

Disqualified   Qualified Horse

&#x20;                   │

══════════════════════════════════════════════

&#x20;     Registration Closed

══════════════════════════════════════════════

&#x20;                   │

&#x20;                   ▼

&#x20;    Count Qualified Horses

&#x20;                   │

&#x20;     ┌─────────────┼─────────────┐

&#x20;     │             │             │

&#x20;     ▼             ▼             ▼

&#x20;   < 12        12 Horses      13\~48 Horses

&#x20;     │             │             │

&#x20;     ▼             ▼             ▼

&#x20;Tournament     Auto Arrange   Auto Arrange

&#x20;Cancelled      Final Round     Pre Round

&#x20;                               │

══════════════════════════════════════════════

&#x20;           PRE ROUND

══════════════════════════════════════════════

&#x20;                               │

&#x20;                               ▼

&#x20;                Generate Multiple Races

&#x20;                               │

&#x20;                               ▼

&#x20;                   Maximum 12 Horses/Race

&#x20;                               │

&#x20;                               ▼

&#x20;                   All Pre Races Completed

&#x20;                               │

&#x20;                               ▼

&#x20;                Collect All Race Results

&#x20;                               │

&#x20;                               ▼

&#x20;                Sort by:

&#x20;                1. Finish Time ASC

&#x20;                2. Finish Position ASC

&#x20;                3. Average Time ASC

&#x20;                               │

&#x20;                               ▼

&#x20;                   Select TOP 12 Horses

&#x20;                               │

══════════════════════════════════════════════

&#x20;           FINAL ROUND

══════════════════════════════════════════════

&#x20;                               │

&#x20;                               ▼

&#x20;                 Generate ONLY ONE Final Race

&#x20;                               │

&#x20;                               ▼

&#x20;                   12 Qualified Horses

&#x20;                               │

&#x20;                               ▼

&#x20;                     Final Race Running

&#x20;                               │

&#x20;                               ▼

&#x20;                 RaceReferee Submit Results

&#x20;                               │

&#x20;                               ▼

&#x20;                     Final Race Completed

&#x20;                               │

══════════════════════════════════════════════

&#x20;          PRIZE DISTRIBUTION

══════════════════════════════════════════════

&#x20;                               │

&#x20;                               ▼

&#x20;                Calculate Champion Ranking

&#x20;                               │

&#x20;                               ▼

&#x20;                  Prize Distribution

&#x20;                               │

&#x20;                               ▼

&#x20;                Wallet \& Transactions

&#x20;                               │

&#x20;                               ▼

&#x20;                   Tournament Completed

