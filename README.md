**Management Student API Secure**
A secure API built using .NET 8 and C#, designed to manage student and user records. The project relies on an N-Tier Architecture to separate business logic from direct data access operations.

**Key Features & Characteristics:**

* **Student Management:** Provides complete operations for processing and managing student data through dedicated classes like `clsStudent`.


* **User Management:** An integrated system for managing user accounts and permissions within the API using `clsUsers`.


* **Audit Logs:** Tracks and records all operations and events occurring within the system to ensure reliability and monitoring using `clsAuditLogs`.


* **Secure Authentication & Authorization:** Secures endpoints using JWT (JSON Web Tokens) via the `JwtBearer` library.


* **Custom Security Policies:** Implements advanced authorization rules (such as `StudentOwnerOrAdminRequirement`) to ensure that only authorized users have access.


* **Interactive API Documentation:** Integrates Swagger tools (via `Swashbuckle.AspNetCore` libraries) to generate an interactive interface that documents the API and facilitates testing and development.


* **Encryption & Data Management:** Incorporates the `BCrypt-Net-Next` library for secure data handling, and utilizes `Microsoft.Data.SqlClient` for managing SQL database connections.



**Technical Structure (Project Layers):**

* **API Layer:** Receives incoming requests, manages routing, and enforces security and authentication policies.


* **Business Layer:** Contains the application's business logic and entity management classes.


* **Data Access Layer:** Directly handles database queries and includes data execution classes such as `clsStudentData` and `clsUsersData`.











