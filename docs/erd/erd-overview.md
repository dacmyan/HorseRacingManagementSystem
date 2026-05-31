\# ERD Overview



\## 1. System Name



Horse Racing Management System



\## 2. Purpose



This document describes the initial Entity Relationship Diagram overview for the Horse Racing Management System. The purpose is to identify the main entities, their responsibilities, and the relationships between them before implementing the database.



\## 3. Main Entities



\### 3.1 User



Represents a system account.



A user can be an Admin, Horse Owner, Jockey, Race Referee, or Spectator depending on assigned roles.



Main information:



\* User ID

\* Full name

\* Email

\* Password hash

\* Phone number

\* Status

\* Created date



\### 3.2 Role



Represents user permissions in the system.



Main roles:



\* Admin

\* Horse Owner

\* Jockey

\* Race Referee

\* Spectator



\### 3.3 UserRole



Represents the relationship between User and Role.



A user can have one or more roles.



\### 3.4 Horse



Represents a horse that can participate in tournaments and races.



Main information:



\* Horse ID

\* Horse name

\* Age

\* Gender

\* Breed

\* Health status

\* Owner ID



\### 3.5 JockeyProfile



Represents detailed information of a jockey.



A jockey is also a user in the system.



Main information:



\* Jockey ID

\* User ID

\* Experience

\* Weight

\* Achievement

\* Status



\### 3.6 Tournament



Represents a horse racing tournament.



Main information:



\* Tournament ID

\* Tournament name

\* Start date

\* End date

\* Location

\* Status



\### 3.7 Registration



Represents the registration of a horse for a tournament.



Main information:



\* Registration ID

\* Tournament ID

\* Horse ID

\* Owner ID

\* Registration status

\* Created date



\### 3.8 Race



Represents a specific race in a tournament.



Main information:



\* Race ID

\* Tournament ID

\* Race name

\* Race date

\* Location

\* Distance

\* Status



\### 3.9 RaceParticipant



Represents a horse and jockey participating in a specific race.



Main information:



\* RaceParticipant ID

\* Race ID

\* Horse ID

\* Jockey ID

\* Lane number

\* Participation status



\### 3.10 RaceResult



Represents the final result of a race.



Main information:



\* RaceResult ID

\* Race ID

\* RaceParticipant ID

\* Finish position

\* Finish time

\* Result status



\### 3.11 RefereeAssignment



Represents the assignment of a referee to a race.



Main information:



\* RefereeAssignment ID

\* Race ID

\* Referee ID

\* Assigned date



\### 3.12 RefereeReport



Represents the report created by a referee after or during a race.



Main information:



\* Report ID

\* Race ID

\* Referee ID

\* Content

\* Created date



\### 3.13 Violation



Represents violations recorded during a race.



Main information:



\* Violation ID

\* Race ID

\* RaceParticipant ID

\* Referee ID

\* Description

\* Penalty

\* Created date



\### 3.14 Bet



Represents a spectator's prediction for a race result.



Main information:



\* Bet ID

\* Spectator ID

\* Race ID

\* Predicted horse ID

\* Bet amount

\* Bet status

\* Created date



\### 3.15 Prize



Represents prize information for race results or prediction rewards.



Main information:



\* Prize ID

\* Race ID

\* User ID

\* Horse ID

\* Amount

\* Prize type

\* Status



\### 3.16 Wallet



Represents the balance of a user.



Main information:



\* Wallet ID

\* User ID

\* Balance



\### 3.17 WalletTransaction



Represents wallet activities such as deposit, withdrawal, betting, refund, and reward.



Main information:



\* Transaction ID

\* Wallet ID

\* Amount

\* Transaction type

\* Description

\* Created date



\### 3.18 Notification



Represents system notifications sent to users.



Main information:



\* Notification ID

\* User ID

\* Title

\* Content

\* Is read

\* Created date



\## 4. Main Relationships



\### User and Role



One user can have many roles.



One role can belong to many users.



Relationship:



\* User 1 - N UserRole

\* Role 1 - N UserRole



\### User and Horse



One horse owner can own many horses.



One horse belongs to one horse owner.



Relationship:



\* User 1 - N Horse



\### User and JockeyProfile



One jockey user has one jockey profile.



Relationship:



\* User 1 - 1 JockeyProfile



\### Tournament and Race



One tournament can have many races.



One race belongs to one tournament.



Relationship:



\* Tournament 1 - N Race



\### Tournament and Registration



One tournament can have many registrations.



One registration belongs to one tournament.



Relationship:



\* Tournament 1 - N Registration



\### Horse and Registration



One horse can register for many tournaments.



One registration belongs to one horse.



Relationship:



\* Horse 1 - N Registration



\### Race and RaceParticipant



One race can have many race participants.



One race participant belongs to one race.



Relationship:



\* Race 1 - N RaceParticipant



\### Horse and RaceParticipant



One horse can participate in many races.



One race participant record belongs to one horse.



Relationship:



\* Horse 1 - N RaceParticipant



\### JockeyProfile and RaceParticipant



One jockey can participate in many races.



One race participant record has one jockey.



Relationship:



\* JockeyProfile 1 - N RaceParticipant



\### Race and RaceResult



One race can have many race results.



One race result belongs to one race.



Relationship:



\* Race 1 - N RaceResult



\### RaceParticipant and RaceResult



One race participant has one result in a race.



Relationship:



\* RaceParticipant 1 - 1 RaceResult



\### Race and RefereeAssignment



One race can have many assigned referees.



One referee can be assigned to many races.



Relationship:



\* Race 1 - N RefereeAssignment

\* User 1 - N RefereeAssignment



\### Race and RefereeReport



One race can have many referee reports.



One referee can create many reports.



Relationship:



\* Race 1 - N RefereeReport

\* User 1 - N RefereeReport



\### Race and Violation



One race can have many violations.



One violation belongs to one race.



Relationship:



\* Race 1 - N Violation



\### Spectator and Bet



One spectator can create many predictions or bets.



One bet belongs to one spectator.



Relationship:



\* User 1 - N Bet



\### Race and Bet



One race can have many bets.



One bet belongs to one race.



Relationship:



\* Race 1 - N Bet



\### User and Wallet



One user has one wallet.



Relationship:



\* User 1 - 1 Wallet



\### Wallet and WalletTransaction



One wallet can have many transactions.



One wallet transaction belongs to one wallet.



Relationship:



\* Wallet 1 - N WalletTransaction



\### User and Notification



One user can receive many notifications.



One notification belongs to one user.



Relationship:



\* User 1 - N Notification



\## 5. Notes



This ERD overview is the first version and may be updated later when the database design becomes more detailed. Some entities may be adjusted during API design and backend implementation.



