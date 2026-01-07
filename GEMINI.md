# Project Rules: Delivery Management System

## 1. Architecture & Core Principles
- **Pattern:** 3-Layer System (Data, Business, Presentation).
- **Primary Goal:** Track couriers, orders, and deliveries.
- **LINQ over Loops:** `foreach` should be avoided wherever possible. Always use LINQ syntax.
- **Documentation:** Every class and method must have `/// <summary>` XML documentation. Add internal comments for complex logic.

## 2. Business Layer (BL) Rules

### 2.1. Strict Namespacing
- To avoid collisions between layers, **NEVER** use `using` statements for `BO` or `DO` namespaces within any file inside the `BL` folder.
- **Code Reference:** Always use full qualification: `BO.EntityName` or `DO.EntityName`.

### 2.2. Directory Structure
- `BL/BO/`: Business Object entities (Partial PDS).
- `BL/BlApi/`: Logic interfaces.
- `BL/BlImplementation/`: Implementation of interfaces.
- `BL/Helpers/`:
    - `Tools.cs`: Generic/template static methods (one file for all).
    - `[Entity]Manager.cs`: Entity-specific static helper methods.

### 2.3. Business Object (BO) Definition Rules
Every class in the `BO` folder must follow these constraints:
- **Namespace:** Must be `BO`.
- **Class Type:** `public class`, matching the filename.
- **PODS Principle:** Business Objects should be Plain Old Data Structures (PODS). They must not contain any calculation logic, constructors, or destructors. The only allowed method is `ToString()` overloading.
- **Properties Only:**
    - All properties must be `public`.
    - **No automatic properties:** Do not initialize during declaration.
    - **Accessors:**
        - Use `{ get; init; }` for mandatory initial values that are mapped directly from the Data Layer (like IDs).
        - Use `{ get; set; }` for all other properties, including those that will be calculated and populated by the Business Logic layer.
    - **Nullability:** Follow Data Layer nullability logic (use `?` where applicable).
- **Composition:**
    - Can contain value types, reference types, and other BO entity properties.
    - Collections must be `IEnumerable<BO.EntityName>`. `List<T>` is forbidden.
    - **Forbidden:** No `DO` entities inside `BO` classes.

### 2.4. BlApi Interface Definition Rules
- **Location & Namespace:** Each interface must be in its own file under `BL/BlApi/` and in the `BlApi` namespace.
- **Naming:** The interface name and file name must match the logical service entity name, prefixed with `I` (e.g., `ICourier.cs`).
- **Access Modifier:** Must be `public`.
- **Declarations Only:** No implementation.
- **Method Signature Rules:**
    - Methods that add or update a logical BO entity receive a BO entity instance as a parameter.
    - Methods that delete or retrieve a logical BO entity receive the entity’s ID as a parameter.
    - Methods that perform specific actions (e.g., updating entity status) receive parameters as needed.
    - All parameters are passed by value; do not use `ref`, `out`, or `in`.
- **Allowed Return Types:**
    - .NET built-in types (e.g., `int`).
    - BO entity types (e.g., `BO.Courier`).
    - Collections of BO entities, but only via `IEnumerable` (e.g., `IEnumerable<BO.OrderInList>`).

### 2.5. Enums & Exceptions
- **Enums:** All BL-level enums must reside in `BO.Enums.cs`.
- **Exceptions:** All BL-level custom exceptions must reside in `BO.Exceptions.cs`.
- **Error Handling:**
    - Wrap all DAL calls in `try/catch`.
    - Catch specific `DO` exceptions and re-throw them as corresponding `BO` exceptions.
    - **Requirement:** Assign the caught `DO` exception to the `InnerException` property of the new `BO` exception to preserve the stack trace.

### 2.6. Helper Classes
- **Generic Tools:** Any generic/template method must be added to `BL/Helpers/Tools.cs`.
- **Specific Helpers:** Methods specific to an entity (e.g., mapping a Courier) go into `BL/Helpers/[Entity]Manager.cs`.
