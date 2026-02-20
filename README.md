# EasySave – Logiciel de sauvegarde (Projet ProSoft)

## Contexte du projet

EasySave est un projet de génie logiciel réalisé pour l’entreprise fictive **ProSoft**, éditeur de logiciels.
L’objectif est de concevoir et développer un **logiciel de sauvegarde professionnel**, robuste, évolutif et maintenable, destiné à des environnements informatiques variés (postes utilisateurs, serveurs, réseaux).

Le projet s’inscrit dans une logique **industrielle** avec :
- Gestion de versions (majeures / mineures).
- Documentation utilisateur et support.
- Anticipation des évolutions fonctionnelles.
- Réduction des coûts de développement futurs.

## Objectif du logiciel EasySave

EasySave permet à un utilisateur de :
- Définir des **travaux de sauvegarde** (jobs).
- Sauvegarder des répertoires (fichiers et sous-répertoires).
- Suivre l’exécution et l’état d’avancement des sauvegardes.
- Produire des **logs exploitables** par le support technique (Local ou Distant).

Un **travail de sauvegarde** représente une configuration persistante associant :
- Un nom.
- Un répertoire source.
- Un répertoire cible.
- Un type de sauvegarde (complète ou différentielle).

## Découpage du projet

Le développement est organisé en **trois livrables successifs** :

### Livrable 1 – EasySave v1.0
- Application **console .NET**.
- Jusqu’à **5 travaux de sauvegarde**.
- Sauvegardes complètes et différentielles.
- Logs journaliers au format **JSON**.
- Fichier d’état temps réel (JSON).

### Livrable 2 – EasySave v1.1 et v2.0
- Choix du format de logs (JSON / XML).
- Interface graphique **WPF (MVVM)**.
- Nombre de travaux illimité.
- Chiffrement de fichiers via **CryptoSoft**.
- Détection et gestion d’un logiciel métier (Business Software).

### Livrable 3 – EasySave v3.0 (Version Actuelle)
- **Architecture Log Distante** via Docker (Serveur TCP).
- Sauvegardes en **parallèle** (Multithreading).
- Gestion des priorités de fichiers.
- Contrôle des travaux temps réel (Play / Pause / Stop).
- Pause automatique globale en cas de lancement d'un logiciel métier.

## Installation et Démarrage (Serveur de Logs Docker)

La version 3.0 intègre un module de logs distants. Pour l'utiliser, le serveur de réception doit être lancé via Docker.

**Prérequis :**
- Docker Desktop installé et lancé.

**Procédure de lancement :**
1. Ouvrez un terminal à la racine du projet (là où se trouve le fichier `docker-compose.yml`).
2. Exécutez la commande suivante :
   ```bash
   docker-compose up -d --build
   ```
3. Le serveur écoute sur le port 9000.
4. Les logs reçus sont stockés automatiquement dans le dossier ./logs à la racine du projet.   
Pour arrêter le serveur :
   ```bash
   docker-compose down
   ```
## Technologies utilisées
* Langage : C#
* Framework : .NET 8
* Interface : WPF (Pattern MVVM)
* Infrastructure : Docker (pour les logs distants)
* IDE : Visual Studio 2022+
* Gestion de version : Git / GitHub

## Organisation du dépôt
* /EasySave.WPF : Application principale (Interface graphique).
* /EasySave.Log : Librairie de logging (DLL / Interface ILogger).
* /EasySave.LogServer : Serveur de logs TCP (Application Console pour Docker).
* /CryptoSoft : Module de chiffrement externe (XOR).
* docker-compose.yml : Configuration pour le déploiement du serveur de logs.
* /README.md : Documentation du projet.

## Équipe projet
Projet réalisé par une équipe de 3 personnes :
* Chef de projet / coordination technique.
* Développeur A : Logique métier et moteur de sauvegarde parallèle.
* Développeur B : Logs distants, état temps réel, interface utilisateur.

## Licence
* Projet réalisé dans un cadre pédagogique.