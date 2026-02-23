# EasySave – Backup Software (ProSoft Project)

## Project Context

EasySave is a software engineering project developed for the fictional company **ProSoft**, a software publisher.
The objective is to design and develop a **professional backup software solution** that is robust, scalable, and maintainable, suitable for various IT environments (workstations, servers, networks).

The project follows an **industrial-oriented approach**, including:

* Version management (major / minor releases).
* User and technical support documentation.
* Anticipation of future functional evolutions.
* Reduction of long-term development and maintenance costs.

---

## EasySave Software Objective

EasySave enables users to:

* Define **backup jobs**.
* Back up directories (files and subdirectories).
* Monitor execution and progress status.
* Generate **logs usable by technical support** (Local or Remote).

A **backup job** represents a persistent configuration including:

* A name.
* A source directory.
* A target directory.
* A backup type (full or differential).

---

## Project Breakdown

The development is organized into **three successive deliverables**:

### Deliverable 1 – EasySave v1.0

* **.NET Console** application.
* Up to **5 backup jobs**.
* Full and differential backups.
* Daily logs in **JSON** format.
* Real-time state file (JSON).

---

### Deliverable 2 – EasySave v1.1 and v2.0

* Log format selection (JSON / XML).
* **WPF graphical interface (MVVM pattern)**.
* Unlimited number of backup jobs.
* File encryption via **CryptoSoft**.
* Business software detection and management.

---

### Deliverable 3 – EasySave v3.0 (Current Version)

* **Remote Log Architecture** via Docker (TCP Server).
* **Parallel backups** (Multithreading).
* File priority management.
* Real-time job control (Play / Pause / Stop).
* Global automatic pause when business software is launched.

---

## Installation and Startup (Docker Log Server)

Version 3.0 integrates a remote logging module. To use it, the log reception server must be started via Docker.

### Prerequisites:

* Docker Desktop installed and running.

### Startup Procedure:

1. Open a terminal at the root of the project (where the `docker-compose.yml` file is located).
2. Run the following command:

   ```bash
   docker-compose up -d --build
   ```
3. The server listens on port **9000**.
4. Received logs are automatically stored in the `./logs` folder at the project root.

To stop the server:

```bash
docker-compose down
```

---

## Technologies Used

* Language: **C#**
* Framework: **.NET 8**
* Interface: **WPF (MVVM Pattern)**
* Infrastructure: **Docker** (for remote logs)
* IDE: **Visual Studio 2022+**
* Version Control: **Git / GitHub**

---

## Repository Structure

* `/EasySave.WPF` : Main application (Graphical Interface).
* `/EasySave.Log` : Logging library (DLL / ILogger interface).
* `/EasySave.LogServer` : TCP Log Server (Console application for Docker).
* `/CryptoSoft` : External encryption module (XOR-based).
* `docker-compose.yml` : Configuration for log server deployment.
* `/README.md` : Project documentation.

---

## Project Team

Project carried out by a team of 3 members:

* Project Manager / Technical Coordinator.
* Developer A: Business logic and parallel backup engine.
* Developer B: Remote logs, real-time state management, user interface.

---

## License

* Project developed within an educational context.