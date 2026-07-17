-- ==========================================
-- LOCAL DATABASE INITIALIZATION SCRIPT
-- Generated from Deployed Database Environment
-- Purpose: Local Synchronization & Integration Testing
-- ==========================================

-- 1. DATABASE CREATION & SELECTION
CREATE DATABASE IF NOT EXISTS HorseRacingManagementSystem;
USE HorseRacingManagementSystem;

-- 2. SCHEMA DEFINITIONS

-- Table: Role
DROP TABLE IF EXISTS WalletTransaction;
DROP TABLE IF EXISTS Notification;
DROP TABLE IF EXISTS TournamentPrizePayout;
DROP TABLE IF EXISTS Prize;
DROP TABLE IF EXISTS Prediction;
DROP TABLE IF EXISTS Payout;
DROP TABLE IF EXISTS Bet;
DROP TABLE IF EXISTS Wallet;
DROP TABLE IF EXISTS RaceViolation;
DROP TABLE IF EXISTS RaceEntry;
DROP TABLE IF EXISTS JockeyContract;
DROP TABLE IF EXISTS RefereeReport;
DROP TABLE IF EXISTS RaceRefereeAssignment;
DROP TABLE IF EXISTS MedicalCheckRecord;
DROP TABLE IF EXISTS Registration;
DROP TABLE IF EXISTS Race;
DROP TABLE IF EXISTS Round;
DROP TABLE IF EXISTS HorseStatistic;
DROP TABLE IF EXISTS HorseDocument;
DROP TABLE IF EXISTS Horse;
DROP TABLE IF EXISTS RefereeProfile;
DROP TABLE IF EXISTS JockeyProfile;
DROP TABLE IF EXISTS AppUser;
DROP TABLE IF EXISTS RaceResult;
DROP TABLE IF EXISTS Tournament;
DROP TABLE IF EXISTS Role;

-- Table: Role
CREATE TABLE Role (
    RoleId INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL,
    PRIMARY KEY (RoleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Tournament
CREATE TABLE Tournament (
    TournamentId BIGINT NOT NULL AUTO_INCREMENT,
    CancelCount INT NOT NULL DEFAULT 0,
    Description TEXT NOT NULL,
    EndDate DATETIME(6) NULL,
    Name VARCHAR(255) NOT NULL,
    RegistrationEndDate DATETIME(6) NULL,
    RegistrationStartDate DATETIME(6) NULL,
    StartDate DATETIME(6) NULL,
    Status VARCHAR(255) NOT NULL,
    PRIMARY KEY (TournamentId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: RaceResult
CREATE TABLE RaceResult (
    Id INT NOT NULL AUTO_INCREMENT,
    CreatedAt DATETIME(6) NOT NULL,
    RaceId BIGINT NOT NULL,
    ResultRecordedAt DATETIME(6) NOT NULL,
    Winner VARCHAR(255) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: AppUser
CREATE TABLE AppUser (
    UserId INT NOT NULL AUTO_INCREMENT,
    CreatedAt DATETIME(6) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    FullName VARCHAR(255) NOT NULL,
    IsEmailConfirmed TINYINT(1) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    RoleId INT NOT NULL,
    Status VARCHAR(255) NOT NULL,
    TokenExpiresAt DATETIME(6) NULL,
    Username VARCHAR(255) NOT NULL,
    VerificationToken VARCHAR(255) NULL,
    PRIMARY KEY (UserId),
    FOREIGN KEY (RoleId) REFERENCES Role(RoleId) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: JockeyProfile
CREATE TABLE JockeyProfile (
    JockeyId INT NOT NULL AUTO_INCREMENT,
    ExperienceYears INT NOT NULL,
    RankingPoint INT NOT NULL,
    Status VARCHAR(255) NOT NULL,
    UserId INT NOT NULL,
    PRIMARY KEY (JockeyId),
    FOREIGN KEY (UserId) REFERENCES AppUser(UserId) ON DELETE CASCADE,
    UNIQUE KEY UQ_JockeyProfile_UserId (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: RefereeProfile
CREATE TABLE RefereeProfile (
    RefereeId INT NOT NULL AUTO_INCREMENT,
    ExperienceYears INT NOT NULL,
    LicenseNumber VARCHAR(255) NOT NULL,
    Status VARCHAR(255) NOT NULL,
    UserId INT NOT NULL,
    PRIMARY KEY (RefereeId),
    FOREIGN KEY (UserId) REFERENCES AppUser(UserId) ON DELETE CASCADE,
    UNIQUE KEY UQ_RefereeProfile_UserId (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Horse
CREATE TABLE Horse (
    Id INT NOT NULL AUTO_INCREMENT,
    Age DATETIME(6) NOT NULL,
    AverageTime DECIMAL(10, 2) NULL,
    Breed VARCHAR(255) NOT NULL,
    Gender VARCHAR(255) NOT NULL,
    HealthStatus VARCHAR(255) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    OwnerId INT NOT NULL,
    RecentAverageTime DECIMAL(10, 2) NULL,
    WinRate DECIMAL(5, 2) NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (OwnerId) REFERENCES AppUser(UserId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: HorseDocument
CREATE TABLE HorseDocument (
    Id INT NOT NULL AUTO_INCREMENT,
    DocumentType VARCHAR(255) NOT NULL,
    DocumentUrl VARCHAR(2048) NOT NULL,
    HorseId INT NOT NULL,
    UploadedAt DATETIME(6) NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (HorseId) REFERENCES Horse(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: HorseStatistic
CREATE TABLE HorseStatistic (
    Id INT NOT NULL AUTO_INCREMENT,
    AverageSpeed DECIMAL(10, 2) NOT NULL,
    HorseId INT NOT NULL,
    TotalRaces INT NOT NULL,
    TotalSecondPlaces INT NOT NULL,
    TotalThirdPlaces INT NOT NULL,
    TotalWins INT NOT NULL,
    UpdatedAt DATETIME(6) NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (HorseId) REFERENCES Horse(Id) ON DELETE CASCADE,
    UNIQUE KEY UQ_HorseStatistic_HorseId (HorseId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Round
CREATE TABLE Round (
    RoundId BIGINT NOT NULL AUTO_INCREMENT,
    EndDate DATETIME(6) NULL,
    Name VARCHAR(255) NULL,
    RoundNumber INT NOT NULL,
    StartDate DATETIME(6) NULL,
    Status VARCHAR(255) NOT NULL,
    TournamentId BIGINT NOT NULL,
    PRIMARY KEY (RoundId),
    FOREIGN KEY (TournamentId) REFERENCES Tournament(TournamentId) ON DELETE CASCADE,
    UNIQUE KEY UQ_Round_TournamentId_RoundNumber (TournamentId, RoundNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Race
CREATE TABLE Race (
    RaceId BIGINT NOT NULL AUTO_INCREMENT,
    DistanceMeter INT NOT NULL,
    MaxLanes INT NOT NULL,
    Name VARCHAR(255) NULL,
    RaceDate DATETIME(6) NOT NULL,
    RoundId BIGINT NOT NULL,
    Status VARCHAR(255) NOT NULL,
    PRIMARY KEY (RaceId),
    FOREIGN KEY (RoundId) REFERENCES Round(RoundId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Registration
CREATE TABLE Registration (
    Id INT NOT NULL AUTO_INCREMENT,
    HorseId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    Status VARCHAR(255) NOT NULL,
    TournamentId BIGINT NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (HorseId) REFERENCES Horse(Id) ON DELETE RESTRICT,
    FOREIGN KEY (TournamentId) REFERENCES Tournament(TournamentId) ON DELETE RESTRICT,
    UNIQUE KEY UQ_Registration_TournamentId_HorseId (TournamentId, HorseId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: MedicalCheckRecord
CREATE TABLE MedicalCheckRecord (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    CheckType VARCHAR(255) NOT NULL,
    CheckedAt DATETIME(6) NOT NULL,
    DopingResult VARCHAR(255) NOT NULL,
    FailReason TEXT NULL,
    HeartRate INT NULL,
    MedicalResult VARCHAR(255) NOT NULL,
    Notes TEXT NULL,
    RegistrationId INT NOT NULL,
    Temperature DECIMAL(10, 2) NULL,
    UserId INT NOT NULL,
    Weight DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (RegistrationId) REFERENCES Registration(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UserId) REFERENCES AppUser(UserId) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: RaceRefereeAssignment
CREATE TABLE RaceRefereeAssignment (
    AssignmentId BIGINT NOT NULL AUTO_INCREMENT,
    AssignedAt DATETIME(6) NULL,
    RaceId BIGINT NOT NULL,
    RefereeId INT NOT NULL,
    Status VARCHAR(255) NOT NULL,
    PRIMARY KEY (AssignmentId),
    FOREIGN KEY (RaceId) REFERENCES Race(RaceId) ON DELETE CASCADE,
    FOREIGN KEY (RefereeId) REFERENCES RefereeProfile(RefereeId) ON DELETE RESTRICT,
    UNIQUE KEY UQ_RaceRefereeAssignment_RaceId_RefereeId (RaceId, RefereeId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: RefereeReport
CREATE TABLE RefereeReport (
    ReportId BIGINT NOT NULL AUTO_INCREMENT,
    AssignmentId BIGINT NOT NULL,
    Content TEXT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    ReportedHorseId INT NULL,
    ReportedUserId INT NULL,
    ViolationNote TEXT NULL,
    PRIMARY KEY (ReportId),
    FOREIGN KEY (AssignmentId) REFERENCES RaceRefereeAssignment(AssignmentId) ON DELETE CASCADE,
    FOREIGN KEY (ReportedHorseId) REFERENCES Horse(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ReportedUserId) REFERENCES AppUser(UserId) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: JockeyContract
CREATE TABLE JockeyContract (
    ContractId INT NOT NULL AUTO_INCREMENT,
    CreatedAt DATETIME(6) NOT NULL,
    EndDate DATETIME(6) NOT NULL,
    HorseId INT NOT NULL,
    InvitationExpiredAt DATETIME(6) NOT NULL,
    JockeyId INT NOT NULL,
    StartDate DATETIME(6) NOT NULL,
    Status VARCHAR(255) NOT NULL,
    TournamentId BIGINT NOT NULL,
    PRIMARY KEY (ContractId),
    FOREIGN KEY (HorseId) REFERENCES Horse(Id) ON DELETE RESTRICT,
    FOREIGN KEY (JockeyId) REFERENCES AppUser(UserId) ON DELETE RESTRICT,
    FOREIGN KEY (TournamentId) REFERENCES Tournament(TournamentId) ON DELETE RESTRICT,
    UNIQUE KEY UQ_JockeyContract_TournamentId_HorseId_JockeyId (TournamentId, HorseId, JockeyId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: RaceEntry
CREATE TABLE RaceEntry (
    RaceEntryId BIGINT NOT NULL AUTO_INCREMENT,
    CurrentOdds DECIMAL(10, 2) NULL,
    FinishPosition INT NULL,
    FinishTime DECIMAL(10, 2) NULL,
    JockeyId INT NULL,
    LaneNo INT NOT NULL,
    RaceId BIGINT NOT NULL,
    RegistrationId INT NOT NULL,
    Status VARCHAR(255) NOT NULL,
    WinningProbability DECIMAL(5, 2) NULL,
    WithdrawReason TEXT NULL,
    WithdrawTime DATETIME(6) NULL,
    PRIMARY KEY (RaceEntryId),
    FOREIGN KEY (JockeyId) REFERENCES JockeyProfile(JockeyId) ON DELETE RESTRICT,
    FOREIGN KEY (RaceId) REFERENCES Race(RaceId) ON DELETE RESTRICT,
    FOREIGN KEY (RegistrationId) REFERENCES Registration(Id) ON DELETE RESTRICT,
    UNIQUE KEY UQ_RaceEntry_RaceId_LaneNo (RaceId, LaneNo),
    UNIQUE KEY UQ_RaceEntry_RaceId_RegistrationId (RaceId, RegistrationId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: RaceViolation
CREATE TABLE RaceViolation (
    Id INT NOT NULL AUTO_INCREMENT,
    Description TEXT NOT NULL,
    Penalty VARCHAR(255) NOT NULL,
    RaceId BIGINT NOT NULL,
    Status VARCHAR(255) NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (RaceId) REFERENCES Race(RaceId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Wallet
CREATE TABLE Wallet (
    WalletId INT NOT NULL AUTO_INCREMENT,
    Balance DECIMAL(18, 2) NOT NULL,
    UserId INT NOT NULL,
    PRIMARY KEY (WalletId),
    FOREIGN KEY (UserId) REFERENCES AppUser(UserId) ON DELETE CASCADE,
    UNIQUE KEY UQ_Wallet_UserId (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Bet
CREATE TABLE Bet (
    Id INT NOT NULL AUTO_INCREMENT,
    Amount DECIMAL(18, 2) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    HorseId INT NOT NULL,
    Odds DECIMAL(10, 2) NOT NULL,
    RaceEntryId BIGINT NULL,
    RaceId BIGINT NOT NULL,
    Status VARCHAR(255) NOT NULL,
    UserId INT NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (HorseId) REFERENCES Horse(Id) ON DELETE RESTRICT,
    FOREIGN KEY (RaceEntryId) REFERENCES RaceEntry(RaceEntryId) ON DELETE RESTRICT,
    FOREIGN KEY (RaceId) REFERENCES Race(RaceId) ON DELETE RESTRICT,
    FOREIGN KEY (UserId) REFERENCES AppUser(UserId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Payout
CREATE TABLE Payout (
    Id INT NOT NULL AUTO_INCREMENT,
    Amount DECIMAL(18, 2) NOT NULL,
    BetId INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (BetId) REFERENCES Bet(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Prediction
CREATE TABLE Prediction (
    PredictionId INT NOT NULL AUTO_INCREMENT,
    IsCorrect TINYINT(1) NULL,
    Point INT NOT NULL,
    PredictedAt DATETIME(6) NOT NULL,
    RaceEntryId BIGINT NOT NULL,
    RaceId BIGINT NOT NULL,
    UserId INT NOT NULL,
    PRIMARY KEY (PredictionId),
    FOREIGN KEY (RaceEntryId) REFERENCES RaceEntry(RaceEntryId) ON DELETE RESTRICT,
    FOREIGN KEY (RaceId) REFERENCES Race(RaceId) ON DELETE RESTRICT,
    FOREIGN KEY (UserId) REFERENCES AppUser(UserId) ON DELETE CASCADE,
    UNIQUE KEY UQ_Prediction_UserId_RaceId (UserId, RaceId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Prize
CREATE TABLE Prize (
    Id INT NOT NULL AUTO_INCREMENT,
    Amount DECIMAL(18, 2) NOT NULL,
    JockeyPercentage DECIMAL(5, 2) NOT NULL,
    OwnerPercentage DECIMAL(5, 2) NOT NULL,
    RankPosition INT NOT NULL,
    TournamentId BIGINT NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (TournamentId) REFERENCES Tournament(TournamentId) ON DELETE CASCADE,
    UNIQUE KEY UQ_Prize_TournamentId_RankPosition (TournamentId, RankPosition)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: TournamentPrizePayout
CREATE TABLE TournamentPrizePayout (
    Id INT NOT NULL AUTO_INCREMENT,
    Amount DECIMAL(18, 2) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    Role VARCHAR(255) NOT NULL,
    TournamentId BIGINT NOT NULL,
    UserId INT NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (TournamentId) REFERENCES Tournament(TournamentId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES AppUser(UserId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: Notification
CREATE TABLE Notification (
    Id INT NOT NULL AUTO_INCREMENT,
    ActionUrl VARCHAR(2048) NULL,
    Content TEXT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    IsDeleted TINYINT(1) NOT NULL,
    IsRead TINYINT(1) NOT NULL,
    Message TEXT NOT NULL,
    ReadAt DATETIME(6) NULL,
    ReferenceId INT NULL,
    Thumbnail VARCHAR(2048) NULL,
    Title VARCHAR(255) NOT NULL,
    Type VARCHAR(255) NOT NULL,
    UserId INT NOT NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (UserId) REFERENCES AppUser(UserId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Table: WalletTransaction
CREATE TABLE WalletTransaction (
    TransactionId INT NOT NULL AUTO_INCREMENT,
    Amount DECIMAL(18, 2) NOT NULL,
    BetId INT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    Description TEXT NULL,
    GatewayTransactionId VARCHAR(255) NULL,
    PaymentMethod VARCHAR(255) NULL,
    PayoutId INT NULL,
    PrizePayoutId INT NULL,
    Status VARCHAR(255) NULL,
    Type VARCHAR(255) NOT NULL,
    WalletId INT NOT NULL,
    PRIMARY KEY (TransactionId),
    FOREIGN KEY (BetId) REFERENCES Bet(Id) ON DELETE RESTRICT,
    FOREIGN KEY (PayoutId) REFERENCES Payout(Id) ON DELETE RESTRICT,
    FOREIGN KEY (PrizePayoutId) REFERENCES TournamentPrizePayout(Id) ON DELETE RESTRICT,
    FOREIGN KEY (WalletId) REFERENCES Wallet(WalletId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
