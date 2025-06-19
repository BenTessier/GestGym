-- ******************************************************************************
-- Script: CREATE DATABASE GestGym.sql
-- Description: Script de création de la base de données GestGym pour la gestion 
--              d'une salle de sport.
-- Auteur: [Nom de l'auteur]
-- Date de création: [Date]
-- Version: 1.0
--


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

