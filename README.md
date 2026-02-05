# EasySave – Logiciel de sauvegarde (Projet ProSoft)

## Contexte du projet

EasySave est un projet de génie logiciel réalisé pour l’entreprise fictive **ProSoft**, éditeur de logiciels.
L’objectif est de concevoir et développer un **logiciel de sauvegarde professionnel**, robuste, évolutif et maintenable, destiné à des environnements informatiques variés (postes utilisateurs, serveurs, réseaux).

Le projet s’inscrit dans une logique **industrielle** avec :
- gestion de versions (majeures / mineures),
- documentation utilisateur et support,
- anticipation des évolutions fonctionnelles,
- réduction des coûts de développement futurs.


## Objectif du logiciel EasySave

EasySave permet à un utilisateur de :
- définir des **travaux de sauvegarde** (jobs),
- sauvegarder des répertoires (fichiers et sous-répertoires),
- suivre l’exécution et l’état d’avancement des sauvegardes,
- produire des **logs exploitables** par le support technique.

Un **travail de sauvegarde** représente une configuration persistante associant :
- un nom,
- un répertoire source,
- un répertoire cible,
- un type de sauvegarde (complète ou différentielle).


## Découpage du projet

Le développement est organisé en **trois livrables successifs** :

### Livrable 1 – EasySave v1.0
- Application **console .NET**
- Jusqu’à **5 travaux de sauvegarde**
- Sauvegardes complètes et différentielles
- Exécution via menu console ou ligne de commande
- Logs journaliers au format **JSON**
- Fichier d’état temps réel (JSON)
- Librairie dédiée de logging : **EasyLog.dll**

### Livrable 2 – EasySave v1.1 et v2.0
- Choix du format de logs (JSON / XML)
- Interface graphique (WPF ou Avalonia)
- Nombre de travaux illimité
- Chiffrement de fichiers via CryptoSoft
- Détection et gestion d’un logiciel métier

### Livrable 3 – EasySave v3.0
- Sauvegardes en parallèle
- Gestion des priorités de fichiers
- Contrôle des travaux (Play / Pause / Stop)
- Pause automatique en cas de logiciel métier
- Centralisation des logs via un service Docker


## Technologies utilisées

- Langage : **C#**
- Framework : **.NET 8**
- IDE : **Visual Studio 2022+**
- Gestion de version : **Git / GitHub**
- Modélisation : **UML**


## Qualité et contraintes

Le projet respecte les contraintes suivantes :
- Code en anglais
- Lisibilité et maintenabilité
- Architecture en couches
- Fonctions courtes et cohérentes


## Organisation du dépôt

- `/EasySave.App` : code source de l’application EasySave
- `/EasySave.Log` : librairie de logging (DLL)
- `/README.md` : description du projet

## Équipe projet

Projet réalisé par une équipe de 3 personnes :
- Chef de projet / coordination technique
- Développeur A : logique métier et moteur de sauvegarde
- Développeur B : logs, état temps réel, interface console


## Licence

Projet réalisé dans un cadre pédagogique.
