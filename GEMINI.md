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
    - Any exception caught within the BL must be re-thrown as a custom `BO` exception (defined in `BO.Exceptions.cs`).
    - Wrap all DAL calls in `try/catch`.
    - When catching a `DO` exception, re-throw it as its corresponding `BO` exception, nesting the original `DO` exception in the `InnerException` property. For all other caught exceptions, do not nest them.
    - Use the dedicated `BO.InvalidNullInputException` for `null` arguments passed to public methods where `null` is not a valid value.

### 2.6. Helper Classes
- **Generic Tools:** Any generic/template method must be added to `BL/Helpers/Tools.cs`.
- **Specific Helpers:** Methods specific to an entity (e.g., mapping a Courier) go into `BL/Helpers/[Entity]Manager.cs`.

### 2.7. System Clock Access
- All access to the system clock from within the BL must go only through the `AdminManager.Now` property. Never directly through the DAL.

## 3. Delivery Completion Types
- **Delivered**: The order was delivered (regardless of timing) and closed.
- **Customer Refused**: The courier arrived, but the customer refused to accept the order. The package is returned to the dispatch center and the order is closed.
- **Canceled**: The order was canceled after creation.
  - **Before Delivery**: An order awaiting assignment is closed with a "dummy" delivery (start = end time), a type "Canceled", and a courier ID of 0.
  - **During Delivery**: An active order where the admin requests the courier to return to dispatch. The order closes with an updated end time.
- **Recipient Not Found**: The courier arrives, but the recipient is absent. The package returns to dispatch, the delivery closes, and the order reopens for another delivery attempt.
- **Failed**: A route calculation error occurs when creating the delivery. The delivery closes, and the order remains open.

## 4. Order Lifecycle and Timing
### 4.1. Order Lifecycle Timestamps
- **Order Opening Time**: Time the admin creates the order. Stored in `DO.Order`.
- **Delivery Start Time**: When a courier collects an order. Set by the system clock at the moment of pickup.
- **Delivery Ending Time**: When the delivery is finished, triggering order closure or reopening.

### 4.2. Business Layer Timestamp Properties
- **Expected Delivery Time**: An estimated delivery date and time, based on delivery type (means of transportation), distance from the company address, and average speed (from config).
- **Maximum Delivery Time**: Latest allowable delivery date and time, calculated as `OrderOpeningTime + MaxDeliveryTimeSpan`.
