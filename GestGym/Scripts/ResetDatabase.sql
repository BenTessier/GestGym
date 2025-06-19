-- ******************************************************************************
-- Script: CREATE DATABASE GestGym.sql
-- Description: Script de création de la base de données GestGym pour la gestion 
--              d'une salle de sport.
-- Auteur: [Nom de l'auteur]
-- Date de création: [Date]
-- Version: 1.0
--

-- SECTION 1: CRÉATION ET CONFIGURATION DE LA BASE DE DONNÉES
-- ******************************************************************************
-- Vérification de l'existence de la base de données
-- Cette section vérifie si la base de données GestGym existe déjà
-- Si elle existe, elle sera supprimée avant d'être recréée
IF DB_ID('GestGym') IS NULL
BEGIN
    -- Création de la base de données uniquement si elle n'existe pas
    CREATE DATABASE GestGym
    COLLATE SQL_Latin1_General_CP1_CI_AI
    
    -- Étape 1 : Créer un login au niveau du serveur (si nécessaire)
    IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'GestGym_ADM')
    BEGIN
        CREATE LOGIN GestGym_ADM WITH PASSWORD = '7UqvE$mN%T7qPpQ#rr#$yzv';
    END
    
    PRINT 'Base de données vide GestGym créée avec succès.'
END
GO

-- Étape 2 : Associer ce login à un utilisateur dans la base de données
-- Cette section s'exécute même si la BD existe déjà
USE GestGym;
GO
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'GestGym_ADM')
BEGIN
    CREATE USER GestGym_ADM FOR LOGIN GestGym_ADM;
    
    -- Étape 3 : Attribuer tous les droits à cet utilisateur en l'ajoutant au rôle db_owner
    EXEC sp_addrolemember 'db_owner', 'GestGym_ADM';
    
    PRINT 'Utilisateur GestGym_ADM créé et configuré avec succès.'
END
GO

-- SECTION 2: NETTOYAGE DES STRUCTURES EXISTANTES
-- ******************************************************************************
-- Script de suppression des tables et des contraintes de clé étrangère
-- Ce script doit être exécuté avec précaution, car il supprimera toutes les données et les relations existantes.   

USE [GestGym]
GO

-- Désactiver la vérification des clés étrangères pour faciliter la suppression
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
GO

-- Suppression des contraintes de clé étrangère avant de supprimer les tables
-- Suppression des contraintes de clé étrangère manquantes

IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Utilisateur_Groupe' AND type = 'F')
    ALTER TABLE [dbo].[Utilisateurs] DROP CONSTRAINT FK_Utilisateur_Groupe;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Utilisateur_Role' AND type = 'F')
    ALTER TABLE [dbo].[Utilisateurs] DROP CONSTRAINT FK_Utilisateur_Role;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_ClientOption_OptionDeBase' AND type = 'F')
    ALTER TABLE [dbo].[ClientOption] DROP CONSTRAINT FK_ClientOption_OptionDeBase;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_ClientOption_Client' AND type = 'F')
    ALTER TABLE [dbo].[ClientOption] DROP CONSTRAINT FK_ClientOption_Client;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_ClientAbonnement_PlanDeBase' AND type = 'F')
    ALTER TABLE [dbo].[ClientAbonnement] DROP CONSTRAINT FK_ClientAbonnement_PlanDeBase;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_ClientAbonnement_Client' AND type = 'F')
    ALTER TABLE [dbo].[ClientAbonnement] DROP CONSTRAINT FK_ClientAbonnement_Client;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Clients_RendezVous' AND type = 'F')
    ALTER TABLE [dbo].[Clients] DROP CONSTRAINT FK_Clients_RendezVous;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Clients_Abonnement' AND type = 'F')
    ALTER TABLE [dbo].[Clients] DROP CONSTRAINT FK_Clients_Abonnement;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_RendezVous_Specialiste' AND type = 'F')
    ALTER TABLE [dbo].[RendezVous] DROP CONSTRAINT FK_RendezVous_Specialiste;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_RendezVous_Options' AND type = 'F')
    ALTER TABLE [dbo].[RendezVous] DROP CONSTRAINT FK_RendezVous_Options;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_RendezVous_Frequence' AND type = 'F')
    ALTER TABLE [dbo].[RendezVous] DROP CONSTRAINT FK_RendezVous_Frequence;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_RendezVous_Client' AND type = 'F')
    ALTER TABLE [dbo].[RendezVous] DROP CONSTRAINT FK_RendezVous_Client;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Abonnement_Plan' AND type = 'F')
    ALTER TABLE [dbo].[Abonnement] DROP CONSTRAINT FK_Abonnement_Plan;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Abonnement_Option' AND type = 'F')
    ALTER TABLE [dbo].[Abonnement] DROP CONSTRAINT FK_Abonnement_Option;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Abonnement_Client' AND type = 'F')
    ALTER TABLE [dbo].[Abonnement] DROP CONSTRAINT FK_Abonnement_Client;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_PlanDeBase_Frequence' AND type = 'F')
    ALTER TABLE [dbo].[PlanDeBase] DROP CONSTRAINT FK_PlanDeBase_Frequence;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_OptionDeBase_Frequence' AND type = 'F')
    ALTER TABLE [dbo].[OptionDeBase] DROP CONSTRAINT FK_OptionDeBase_Frequence;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Plan_PlanDeBase' AND type = 'F')
    ALTER TABLE [dbo].[Plan] DROP CONSTRAINT FK_Plan_PlanDeBase;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Plan_Option' AND type = 'F')
    ALTER TABLE [dbo].[Plan] DROP CONSTRAINT FK_Plan_Option;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Option_OptionDeBase' AND type = 'F')
    ALTER TABLE [dbo].[Option] DROP CONSTRAINT FK_Option_OptionDeBase;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Option_Frequence' AND type = 'F')
    ALTER TABLE [dbo].[Option] DROP CONSTRAINT FK_Option_Frequence;
GO
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'FK_Option_Plan' AND type = 'F')
    ALTER TABLE [dbo].[Option] DROP CONSTRAINT FK_Option_Plan;
GO


-- Suppression des tables dans l'ordre approprié
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RendezVous]') AND type in (N'U'))
    DROP TABLE [dbo].[RendezVous];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Abonnement]') AND type in (N'U'))
    DROP TABLE [dbo].[Abonnement];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
    DROP TABLE [dbo].[Clients];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Utilisateurs]') AND type in (N'U'))
    DROP TABLE [dbo].[Utilisateurs];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
    DROP TABLE [dbo].[Roles];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Groupes]') AND type in (N'U'))
    DROP TABLE [dbo].[Groupes];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OptionDeBase]') AND type in (N'U'))
    DROP TABLE [dbo].[OptionDeBase];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PlanDeBase]') AND type in (N'U'))
    DROP TABLE [dbo].[PlanDeBase];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Frequence]') AND type in (N'U'))
    DROP TABLE [dbo].[Frequence];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientOption]') AND type in (N'U'))
    DROP TABLE [dbo].[ClientOption];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientAbonnement]') AND type in (N'U'))
    DROP TABLE [dbo].[ClientAbonnement];
GO


PRINT 'Toutes les tables ont été supprimées avec succès.'
GO

-- SECTION 3: CRÉATION DES TABLES

-- ==================================================================================
-- Script de création des tables et des contraintes de clé étrangère
-- Ce script doit être exécuté après la suppression des tables existantes.  
USE [GestGym]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[Settings](
    [Setting_id] INT PRIMARY KEY IDENTITY(1,1),
    [Nom] [varchar](50) NOT NULL,
    [Valeur] [nvarchar](MAX) NULL
)
GO


/****** Object:  Table [dbo].[Groupes]    Script Date: 2025-05-07 10:52:33 ******/
CREATE TABLE [dbo].[Groupes](
    [Groupe_id] INT PRIMARY KEY IDENTITY(1,1),
    [Nom] [varchar](50) NOT NULL,
    [Notes] [nvarchar](MAX) NULL
)
GO



/****** Object:  Table [dbo].[Roles]    Script Date: 2025-05-07 10:52:33 ******/
CREATE TABLE [dbo].[Roles](
	[Role_id] [int] PRIMARY KEY IDENTITY(1,1),
	[Nom] [varchar](50) NOT NULL,
	[Notes] [nvarchar](MAX) NULL
)
GO

/****** Object:  Table [dbo].[RendezVous]    Script Date: 2025-05-07 10:54:01 ******/
CREATE TABLE [dbo].[RendezVous](
	[RDV_id] [int] PRIMARY KEY IDENTITY(1,1),
	[DateHeureDebut] [datetime] NOT NULL,
	[DateHeureFin] [datetime] NOT NULL,
	[REF_Options_ID] [int] NOT NULL,
	[REF_Frequence_ID] [int] NOT NULL,
	[REF_Specialiste_ID] [int] NOT NULL,
	[REF_Client_ID] [int] NOT NULL
)
GO


/****** Object:  Table [dbo].[Utilisateurs]    Script Date: 2025-05-07 10:48:08 ******/
CREATE TABLE [dbo].[Utilisateurs](
    [id] [int] PRIMARY KEY IDENTITY(1,1),
    [Nom] [varchar](50) NOT NULL,
    [Notes] [nvarchar](MAX) NULL,
    [Telephone] [varchar](12) NULL,
    [NumEmploye] [int] NULL,
    [Courriel] [varchar](100) NOT NULL,
    [REF_Roles_id] [int] NULL,  -- Relation n:1 avec Roles
    [REF_Groupes_id] [int] NULL,  -- Relation n:1 avec Groupes
    [Identifiant] [varchar](50) NULL,
    [MotDePasse] [varchar](50) NULL,
    [DateInscription] [datetime] NULL,
    [Statut] [int] null
)
GO

/****** Object:  Table [dbo].[Clients]    Script Date: 2025-05-07 10:50:00 ******/
CREATE TABLE [dbo].[Clients](
    [Client_id] [int] PRIMARY KEY IDENTITY(1,1),
    [Nom] [varchar](50) NOT NULL,
    [Notes] [nvarchar](MAX) NULL,
    [Telephone] [varchar](12) NULL,
    [NoMembre] [int] NULL,
    [Courriel] [varchar](100) NOT NULL,
    [REF_Abonnements_id] [int] NULL, 
    [REF_RendezVous_id] [int] NULL,
    [Statut] [int] NULL
)
GO

/****** Object:  Table [dbo].[RendezVous]    Script Date: 2025-05-07 10:54:01 ******/
CREATE TABLE [dbo].[Frequence](
    [Frequence_id] [int] PRIMARY KEY IDENTITY(1,1),
    [Frequence] [varchar](50) NOT NULL,
    [Statut] [int] NULL
)
GO

       
/****** Object:  Table [dbo].[PlanDeBase]    Script Date: 2025-05-07 10:52:33 ******/
CREATE TABLE [dbo].[OptionDeBase](
    [OptionBase_id] [int] PRIMARY KEY IDENTITY(1,1),
    [Nom] [varchar](250) NOT NULL,
    [Notes] [nvarchar](MAX) NULL,
    [Prix] [decimal](18, 2) NOT NULL,
    [Horaire] varchar(50) NULL,
    [NbreSeance] [int] NOT NULL,
    [REF_Frequence_ID] [int] NOT NULL,
    [DateDebutDisponible] [date] NULL,
    [DateFinDisponible] [date] NULL,
    [Statut] [int] NULL,
)
GO

/****** Object:  Table [dbo].[Plan]    Script Date: 2025-05-07 10:52:33 ******/
CREATE TABLE [dbo].[PlanDeBase](
    [PlanBase_id] [int] PRIMARY KEY IDENTITY(1,1),
    [Nom] [varchar](250) NOT NULL,
    [Notes] [nvarchar](MAX) NULL,
    [Prix] [decimal](18, 2) NOT NULL,
    [Duree] [int] NOT NULL,
    [DateDebutDisponible] [date] NULL,
    [DateFinDisponible] [date] NULL,
    [Statut] [int] NULL
)
GO

/****** Object:  Table [dbo].[ClientOption]    Script Date: 2025-05-07 10:52:33 ******/
CREATE TABLE [dbo].[ClientOption](
    [ClientOption_id] [int] PRIMARY KEY IDENTITY(1,1),
    [REF_OptionDeBase_ID] [int] NOT NULL,
    [REF_Client_ID] [int] NOT NULL,
    [DateDebut] [date] NULL,
    [DateFin] [date] NULL,
    [Statut] [int] NULL
)
GO

CREATE TABLE [dbo].[ClientAbonnement](
    [ClientAbonnement_id] [int] PRIMARY KEY IDENTITY(1,1),
    [REF_PlanDeBase_ID] [int] NOT NULL,
    [REF_Client_ID] [int] NOT NULL,
    [DateDebut] [date] NULL,
    [DateFin] [date] NULL,
    [Statut] [int] NULL
)
GO


/****** Object:  Table [dbo].[Abonnement]    Script Date: 2025-05-07 10:52:33 ******/
CREATE TABLE [dbo].[Abonnement](
    [Abonnement_id] [int] PRIMARY KEY IDENTITY(1,1),
    [REF_CLIENT_ID] [int] NOT NULL,
    [REF_Plan_ID] [int] NOT NULL,
    [DateDebut] [date] NULL,
    [DateFin] [date] NULL,
    [Statut] [int] NULL
)
GO


-- SECTION 4: CRÉATION DES CONTRAINTES DE CLÉ ÉTRANGÈRE

-- Ajout des contraintes de clé étrangère pour la relation 1:N

-- Utilisateurs
ALTER TABLE [dbo].[Utilisateurs]
    ADD CONSTRAINT FK_Utilisateur_Groupe FOREIGN KEY ([REF_Groupes_id]) REFERENCES [dbo].[Groupes] ([Groupe_id])
GO

ALTER TABLE [dbo].[Utilisateurs] 
    ADD  CONSTRAINT [DF_Utilisateurs_Statut]  DEFAULT ((1)) FOR [Statut]
GO

ALTER TABLE [dbo].[Utilisateurs]
    ADD CONSTRAINT FK_Utilisateur_Role FOREIGN KEY ([REF_Roles_id]) REFERENCES [dbo].[Roles] ([Role_id])
GO

--- ClientsOption
ALTER TABLE [dbo].[ClientOption]
    ADD CONSTRAINT FK_ClientOption_OptionDeBase FOREIGN KEY ([REF_OptionDeBase_ID]) REFERENCES [dbo].[OptionDeBase] ([OptionBase_id])
GO
ALTER TABLE [dbo].[ClientOption]
    ADD CONSTRAINT FK_ClientOption_Client FOREIGN KEY ([REF_Client_ID]) REFERENCES [dbo].[Clients] ([Client_id])
GO
ALTER TABLE [dbo].[ClientOption]
    ADD CONSTRAINT DF_ClientOption_Statut DEFAULT ((1)) FOR [Statut]
GO


-- ClientsAbonnement
ALTER TABLE [dbo].[ClientAbonnement]
    ADD CONSTRAINT FK_ClientAbonnement_PlanDeBase FOREIGN KEY ([REF_PlanDeBase_ID]) REFERENCES [dbo].[PlanDeBase] ([PlanBase_id])
GO
ALTER TABLE [dbo].[ClientAbonnement]
    ADD CONSTRAINT FK_ClientAbonnement_Client FOREIGN KEY ([REF_Client_ID]) REFERENCES [dbo].[Clients] ([Client_id])
GO
ALTER TABLE [dbo].[ClientAbonnement]
    ADD CONSTRAINT DF_ClientAbonnement_Statut DEFAULT ((1)) FOR [Statut]
GO



---  Clients
ALTER TABLE [dbo].[Clients]
    ADD CONSTRAINT FK_Clients_RendezVous FOREIGN KEY ([REF_RendezVous_id]) REFERENCES [dbo].[RendezVous] ([RDV_id])
GO
ALTER TABLE [dbo].[Clients]
    ADD CONSTRAINT FK_Clients_Abonnement FOREIGN KEY ([REF_Abonnements_id]) REFERENCES [dbo].[Abonnement] ([Abonnement_id])
GO
ALTER TABLE [dbo].[Clients]
    ADD CONSTRAINT DF_Clients_Statut DEFAULT ((1)) FOR [Statut] 
GO
ALTER TABLE [dbo].[Clients]
    ADD CONSTRAINT DF_Clients_Notes DEFAULT ('Aucune') FOR [Notes]

-- Rendez-vous
ALTER TABLE [dbo].[RendezVous]
    ADD CONSTRAINT FK_RendezVous_Specialiste FOREIGN KEY ([REF_Specialiste_ID]) REFERENCES [dbo].[Utilisateurs] ([id])
GO
ALTER TABLE [dbo].[RendezVous]
    ADD CONSTRAINT FK_RendezVous_Options FOREIGN KEY ([REF_Options_ID]) REFERENCES [dbo].[ClientOption] ([ClientOption_id])
GO
ALTER TABLE [dbo].[RendezVous]
    ADD CONSTRAINT FK_RendezVous_Frequence FOREIGN KEY ([REF_Frequence_ID]) REFERENCES [dbo].[Frequence] ([Frequence_id])
GO
ALTER TABLE [dbo].[RendezVous]
    ADD CONSTRAINT FK_RendezVous_Client FOREIGN KEY ([REF_Client_ID]) REFERENCES [dbo].[Clients] ([Client_id])
GO

-- Abonnement
ALTER TABLE [dbo].[Abonnement]
    ADD CONSTRAINT FK_Abonnement_Plan FOREIGN KEY ([REF_Plan_ID]) REFERENCES [dbo].[ClientAbonnement] ([ClientAbonnement_id])
GO

ALTER TABLE [dbo].[Abonnement]
    ADD CONSTRAINT FK_Abonnement_Client FOREIGN KEY ([REF_Client_ID]) REFERENCES [dbo].[Clients] ([Client_id])
GO
ALTER TABLE [dbo].[Abonnement]
    ADD CONSTRAINT DF_Abonnement_Statut DEFAULT ((1)) FOR [Statut]
GO


-- PlanDeBase
ALTER TABLE [dbo].[PlanDeBase]
    ADD CONSTRAINT DF_PlanDeBase_Statut DEFAULT ((1)) FOR [Statut]
GO

-- OptionDeBase
ALTER TABLE [dbo].[OptionDeBase]
    ADD CONSTRAINT DF_OptionDeBase_Statut DEFAULT ((1)) FOR [Statut]
GO
ALTER TABLE [dbo].[OptionDeBase]
    ADD CONSTRAINT FK_OptionDeBase_Frequence FOREIGN KEY ([REF_Frequence_ID]) REFERENCES [dbo].[Frequence] ([Frequence_id])
GO


-- Frequence
ALTER TABLE [dbo].[Frequence]
    ADD CONSTRAINT DF_Frequence_Statut DEFAULT ((1)) FOR [Statut]
GO


-- SECTION 5: INSERTION DES DONNÉES DE RÉFÉRENCE

INSERT INTO [dbo].[Settings] (Nom, Valeur)
VALUES ('NomApplication', 'GestGym'),
       ('Version', '0.9'),
       ('DateCreation', '2025-06-10'),
       ('Developpeur', 'Benoit Tessier'),
       ('RunNumber', "0");
GO

INSERT INTO [dbo].[Frequence] (Frequence)
VALUES ('Selon besoins'),
       ('Hebdomadaire'),
       ('Mensuel'),
       ('Trimestrielle');
GO

INSERT INTO [dbo].[PlanDeBase] (Nom, Notes, Prix, Duree, DateDebutDisponible, DateFinDisponible, Statut)
VALUES ('Accès appareils conditionnement (Mensuel, 80$, 2025-01 au 2025-12)', 'Mensuel', 80, 1, '2025-01-01', '2025-12-31',1),
       ('Accès appareils conditionnement (Trimestriel, 65$, 2025-01 au 2025-12)', 'Trimestriel', 65, 3, '2025-01-01', '2025-12-31',1),
       ('Accès appareils conditionnement (Annuel, 75$, 2025-01 au 2025-12)', 'Annuel', 75, 12, '2025-01-01', '2025-12-31',1);
GO


INSERT INTO [dbo].[Roles] (Nom)
VALUES ('Gestionnaire'),
       ('Utilisateurs')
GO

INSERT INTO [dbo].[Groupes] (Nom)
VALUES ('Gestionnaire'),
       ('Entraineurs'),
       ('Physiothérapeutes'),
       ('Nutritionnistes')
    GO

INSERT INTO [dbo].[OptionDeBase] (Nom, Notes, Prix, Horaire, NbreSeance, REF_Frequence_ID, DateDebutDisponible, DateFinDisponible, Statut)
VALUES ('Entraineur Privé (75$/h, 1h, 2025-01, 2025-12)', 'Taux horaire', 75, 'Sur RDV', 1, 1, '2025-01-01', '2025-12-31',1),
       ('Entraineur Privé (65$/h, 10h, 2025-01, 2025-12)', 'Bloc de 10 heures', 65, 'Sur RDV', 10, 2, '2025-01-01', '2025-12-31',1),
       ('Nutritioniste (75$/h, 1h, 2025-01, 2025-12)', 'Taux horaire', 75, 'Sur RDV', 1, 1, '2025-01-01', '2025-12-31',1),
       ('Nutritioniste (65$/h, 10h, 2025-01, 2025-12)', 'Bloc de 10 heures', 65, 'Sur RDV', 10, 2, '2025-01-01', '2025-12-31',1),
       ('Physiothérapeute (75$/h, 1h, 2025-01, 2025-12)', 'Taux horaire', 75, 'Sur RDV', 1, 1, '2025-01-01', '2025-12-31',1),
       ('Physiothérapeute (65$/h, 10h, 2025-01, 2025-12)', 'Bloc de 10 heures', 65, 'Sur RDV', 10, 2, '2025-01-01', '2025-12-31',1),
       ('Special Groupe 1 (25$/mois, 4 séances, 2025-01, 2025-12)', '1 cours de groupe par semaine au prix de 25$ par mois ', 25, 'Selon Horaire', 4, 2, '2025-01-01', '2025-12-31',1),
       ('Special Groupe 2 (60$, 12 séances, 2025-01, 2025-12)', '1 cours de groupe par semaine au prix de 60$ pour 3 mois.', 60, 'Selon Horaire', 12, 2, '2025-01-01', '2025-12-31',1),
       ('Special Groupe 3 (200$, 52 séances, 2025-01, 2025-12)', '1 cours de groupe par semaine au prix de 200$ pour l\?ensemble de l\?année.', 20, 'Selon Horaire', 52, 2, '2025-01-01', '2025-12-31',1),
       ('Cours de Groupe Mensuel (50$, 4 séances, 2025-01, 2025-12)', 'Cours de groupe', 50, 'Selon Horaire', 4, 2, '2025-01-01', '2025-12-31',1),
       ('Cours de Groupe Trimestriel (135$, 12 séances, 2025-01, 2025-12)', 'Cours de groupe', 135, 'Selon Horaire', 12, 2, '2025-01-01', '2025-12-31',1),
       ('Cours de Groupe Annuel (500$, 50 séances, 2025-01, 2025-12)', 'Cours de groupe', 500, 'Selon Horaire', 50, 2, '2025-01-01', '2025-12-31',1);
GO

INSERT INTO [dbo].[Utilisateurs] 
    ([Nom], [Courriel], [Identifiant], [MotDePasse], [DateInscription], [REF_Roles_id], [REF_Groupes_id], [Statut])
VALUES 
    ('Admin', 'admin@gestgym.com', 'admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', GETDATE(), 1, 1, 3)
GO

INSERT INTO [dbo].[Clients] 
    ([Nom], [Courriel], [Statut])
VALUES
    ('John Smith', 'jsmith@myemail.com', 1),
    ('Jane Doe', 'jdoe@myemail.com', 1)
GO


PRINT 'Toutes les tables ont été créées avec succès.'