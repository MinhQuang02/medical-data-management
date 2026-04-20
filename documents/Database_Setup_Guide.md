# Database Configuration Setup Guide

This guide explains how to update the database configuration for the `MedicalModule` after it has been decoupled and refactored to use environment variables (`.env`) for enhanced security and configurable deployment.

## Prerequisites

- .NET 8.0 SDK
- The project relies on **DotNetEnv** library to parse `.env` files.

## 1. Creating the Configuration File

We have extracted all sensitive database credentials into a configurable format. In the `MedicalModule` folder, there should be a file named `.env`.

If the file does not exist, create a new text file and name it `.env` (ensure it does not have a `.txt` extension).

## 2. Configuration Parameters

Open the `.env` file and input the following configuration manually. Modify the values to match your local or remote Oracle Database configuration:

```ini
# The master database service account username
DB_USER=QLBENHVIEN

# The password for the master database service account
DB_PASSWORD=123

# The Oracle Database Connection Data Source (Host:Port/Service)
DB_SERVER=localhost:1521/xepdb1
```

### Explanation of parameters:
* `DB_USER`: The service account username used by the application to execute queries (e.g., QLBENHVIEN or SYS).
* `DB_PASSWORD`: The password for the specified database user.
* `DB_SERVER`: The host, port, and Oracle service name. By default, Oracle 21c XE uses `localhost:1521/xepdb1` (or `xe`). Keep it matching your local installation.

## 3. How the Authentication Works Now

Previously, the module used Oracle's Virtual Private Database (VPD) allowing users to connect using their direct Oracle schema credentials.
Now, the application connects to the database utilizing **a single master session pool** defined by the `.env` configuration. 

When users attempt to log in:
1. The system connects using the master `DB_USER` credentials.
2. It queries standard internal tables (`NHANVIEN`, `BENHNHAN`) to verify the Username.
3. The system assigns Role-Based Access Control (RBAC) via the in-memory **SessionManager** representing the authorization state.
4. Subsequent database operations are protected by strictly enforcing role validation prior to executing Oracle queries.

## 4. Running the Application

Once the `.env` configuration is updated, you can safely launch the application via CMD from the root workspace directory constraint-free.

```ps1
cd MedicalModule
dotnet run
```

Similarly, the `AdminModule` runs completely independently:

```ps1
cd AdminModule
dotnet run
```
