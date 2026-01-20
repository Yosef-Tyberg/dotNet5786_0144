# Project Rules: Delivery Management System

## 1. Architecture & Core Principles
- **Pattern:** 3-Layer System (Data, Business, Presentation).
- **Primary Goal:** Track couriers, orders, and deliveries.
- **LINQ over Loops:** `foreach` should be avoided wherever possible. Always use LINQ syntax.
- **Documentation:** Every class and method must have `/// <summary>` XML documentation. Add internal comments for complex logic.

## 2. Business Layer (BL) Rules

### 2.1. Strict Namespacing
- To avoid collisions between layers, **NEVER** use `using` statements for `BO` or `DO` namespaces within any file inside the `BL` folder. The `using Helpers;` statement is permitted as it does not cause naming conflicts.
- **Code Reference:** Always use full qualification for data entities: `BO.EntityName` or `DO.EntityName`.

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
- **PODS Principle:** Business Objects should be Plain Old Data Structures (PODS). They must not contain any calculation logic, constructors, or destructors. The only allowed method is an override of `ToString()` that provides a comprehensive description of the entity's properties.
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

### 2.8. Manager Method Usage
- Where appropriate, other manager methods should be used instead of direct DL access.

## 3. Delivery Completion Types
The `DeliveryEndTypes` enum defines how a delivery concludes. This affects the final `OrderStatus`.
- **Delivered**: The order was delivered successfully. The Order Status becomes `Delivered`.
- **CustomerRefused**: The courier arrived, but the customer refused the order. The package returns to dispatch, and the Order Status becomes `Refused` (Closed).
- **Cancelled**: The order was cancelled. The Order Status becomes `Cancelled`.
  - **Before Pickup**: An order awaiting assignment is closed via a "dummy" delivery (Start Time = End Time, CourierId = 0, EndType = `Cancelled`). This functionality is supported in the deliveryImplementation/manager classes.
  - **During Delivery**: The courier is recalled. The delivery is closed with EndType `Cancelled`.
- **RecipientNotFound**: The courier arrived, but the recipient was absent. The delivery is closed. The Order Status reverts to `Open`, and the order returns to the dispatch pool to be available for a new delivery attempt by any courier.
- **Failed**: Technical or route failure. The delivery closes, and the order remains `Open`.

## 4. Order Lifecycle and Timing
### 4.1. Order Lifecycle Timestamps
- **Order Opening Time**: Time the admin creates the order. Stored in `DO.Order`.
- **Delivery Start Time**: When a courier collects an order. Set by the `AdminManager.Now` (System Clock) at the moment of pickup.
- **Delivery Ending Time**: When the delivery is finished, triggering order closure or reopening.

### 4.2. Business Layer Timestamp Properties
- **Expected Delivery Time**: An estimated delivery date and time, based on delivery type (means of transportation), distance from the company address, and average speed (from config). This is calculated dynamically in the Business Layer and is not stored in the Data Layer.
- **Maximum Delivery Time**: Latest allowable delivery date and time, calculated as `OrderOpeningTime + MaxDeliveryTimeSpan`. This is also calculated in the Business Layer and is not stored directly in the Data Layer.
- **Schedule Status**: Calculated in the BL (`BO.Delivery`) based on `Config` settings. Relevant for active or completed deliveries:
  - **OnTime**: The delivery is active with sufficient time, or completed before the deadline.
  - **AtRisk**: The delivery is active, and the remaining time is less than `Config.RiskRange`.
  - **Late**: The delivery is active but exceeded `Maximum Delivery Time`, or was completed after the deadline.

### 4.3. Courier Activity
- An active courier's `Active` property should be set to false if a timespan greater than `Config.InactivityRange` has passed since the courier was last involved in a delivery. This includes deliveries in progress.

## 5. Distance Calculation Rules

### 5.1. Aerial Distance
- **Method:** `Tools.GetAerialDistance`
- **Configuration:**
    - `MaxGeneralDeliveryDistanceKm`: A configuration setting that limits the distance for all orders.
    - `PersonalMaxDeliveryDistance`: A courier-specific setting that limits which orders a courier can accept. This must be less than or equal to `MaxGeneralDeliveryDistanceKm`.
- **Usage:**
    - **Order Validation:** When creating or updating an order, the aerial distance from the company headquarters to the order's destination is calculated. This distance must not exceed `MaxGeneralDeliveryDistanceKm` defined in the configuration.
    - **Delivery Validation:** When a courier accepts an order, the aerial distance from the company headquarters to the order's destination is calculated. This distance must not exceed the courier's `PersonalMaxDeliveryDistance`.

### 5.2. Actual Traveled Distance
- **Methods:**
    - `Tools.GetDrivingDistance`: For `Car` and `Motorcycle` deliveries.
    - `Tools.GetWalkingDistance`: For `Bicycle` and `OnFoot` deliveries.
- **Usage:**
    - **Delivery Creation:** When a delivery is created, the actual distance traveled is calculated using one of the methods above, based on the delivery type. This value is stored in the `ActualDistance` property of the delivery. The distance is calculated from the company headquarters to the order's destination.
    - **Time Estimation:** These methods are also used for estimating the delivery time.

## 6. Delivery Entity Definition
- **Role:** The Delivery object is a linking class between an Order and a Courier.
- **Creation:** The Delivery object is created when the courier picks it up from the company.
- **Single Active Task Rule:** A courier may only handle **one** delivery at a time. A courier cannot pick up a new order if they have a delivery with a `null` EndTime.

## 7. Order Status Logic
The `BO.OrderStatus` is derived from the history of deliveries associated with that order:
- **Open**: No deliveries exist, or the last delivery ended in `RecipientNotFound` / `Failed`.
- **InProgress**: The last delivery has a `Start Time` but no `End Time`.
- **Delivered / Refused / Cancelled**: Determined by the `DeliveryEndType` of the last completed delivery.

## 8. Data Visibility & Permissions
- **Admin**: Can view the full delivery history of all orders and couriers.
- **Courier**: Can view only their own delivery history and open orders within their allowed range.

## 9. Functional Requirements
### 9.1. Statistics & Analysis
- **Courier Statistics**: The system must provide statistical data for couriers (e.g., delivery counts, performance metrics) to support the Admin's "Manage Couriers" view.

### 9.2. List Management
- **Filtering & Grouping**: List retrieval operations (for Orders, Couriers, Deliveries) must support filtering, sorting, and grouping capabilities (e.g., by Status, Area, or Type) to satisfy Admin and Courier interface requirements.

### 9.3. Simulation Control
- **Clock Management**: The BL must support operations to initialize and advance the System Clock to facilitate simulation scenarios.

---
## 10. Presentation Layer (PL) Rules and Requirements

### 10.1. Error Handling
- **Catch All Exceptions:** In the Presentation Layer (PL), it is mandatory to catch all exceptions and display a well-organized message to the user (MessageBox) about the problem.
- **User-Friendly Messages:** The error message must be clear and understandable to the system user (an ordinary person and not just a programmer). The goal is not to display the exception message, variable names, etc., but to display an informative message with a meaningful title, understandable content, and an appropriate icon.

### 10.2. Window Management and Data Flow
- **Automatic Window Closing:**
    - In entity addition screens, after successful addition, the addition screen must close automatically.
    - In entity update screens, after successful update, the update screen must close automatically.
- **Automatic UI Updates:**
    - The list screen must update automatically to include the newly added or updated entity.
    - This automatic update must be performed using the **Observer design pattern** in the Logical Layer (BL), and the registration for the observers will be done from the PL layer.
- **No Direct Window Communication:** There will be no direct information sharing between screens. Information transfer will be performed by passing arguments for the screen's constructor parameters.

### 10.3. MVVM, Data Binding, and Code-Behind Best Practices
To maintain the MVVM principle (Model-View-ViewModel), which separates logic from design:
- **No x:Name for Logic:** It is forbidden to name controls via XAML to use them in the Code-behind. Instead, you must use the Data Binding mechanism.
- **Minimize Code-Behind:** The Code-behind must be minimized to the necessary minimum for handling user input and other WPF events. Project logic will not be performed in this layer, except for basic input validation.
- **Data Binding Mandatory:**
    - For data displayed on the screen (single entity or list), it is mandatory to define corresponding properties in the Code-behind.
    - Updating these properties via the Code-behind must automatically update the display.
    - Properties must be defined using `INotifyPropertyChanged` or as `Dependency Properties`.
    - Binding of control attributes to data must be done only through XAML.
    - **Forbidden:** Changing `DataContext` or `ItemsSource` through Code-behind is forbidden. They must be bound from XAML.
- **No Sender Access:** It is forbidden to access the properties of `sender` that comes as a parameter in event handling methods (even after casting). Instead, you must always rely on Data Binding.
- **Converters:** Use `IConverter` in combination with Data Binding for conversions, dynamic input blocking, or dynamic hiding of controls.
- **Comments:** Use XAML comments (`<!-- ... -->`) to describe controls.

### 10.4. Business Logic (BL) Interaction
- **Single Call per Action:** In every operation performed from the GUI, a maximum of one request should be sent to the BL.
- **Exception Handling:** Remember to catch the possible exceptions resulting from this operation.

---

### 10.5. Advanced WPF Features

While not strictly mandatory until Stage 6, the use of the following advanced WPF features is highly encouraged to create a more robust and maintainable UI:

- **Layout Panels:** `Grid`, `StackPanel`, `DockPanel`, etc.
- **Resources:** Defining reusable objects and values in `Window.Resources` or `Application.Resources`.
- **Styles:** Creating consistent appearances for controls using `<Style>`.
- **Data Templates:** Customizing the visual representation of data objects using `<DataTemplate>`, especially for items in lists.

### 10.6. Single Item View Window Rules

- **Modes:** The window must support two modes based on the entity ID passed to the constructor:
    - **Add Mode (ID = null):** Create a new BO instance with default values. Button text must be "Add".
    - **Update Mode (ID != null):** Retrieve the existing entity from the BL. Button text must be "Update".
- **Data Binding:**
    - The window's `DataContext` must be bound to itself (`{Binding RelativeSource={RelativeSource Mode=Self}}`).
    - Use a **Dependency Property** (e.g., `CurrentEntity`) to hold the BO entity.
    - Use a **Dependency Property** (e.g., `ButtonText`) for the action button's label.
    - Bind all UI controls to the properties of `CurrentEntity`.
    - Use `TwoWay` binding mode where appropriate.
    - Enable validation notification: `NotifyOnValidationError=true, ValidatesOnExceptions=true`.
- **Control Behavior:**
    - **ReadOnly/Visibility:** Use converters (e.g., `IdToReadOnlyConverter`) to toggle `IsReadOnly` or `Visibility` of controls based on the window mode (Add vs. Update).
    - **ID Field:**
        - For auto-generated IDs (Order, Delivery): Always ReadOnly.
        - For manual IDs (Courier): Editable in Add mode, ReadOnly in Update mode.
- **Action Handling:**
    - A single button handles both Add and Update actions.
    - The click handler must check the mode (via `ButtonText` or ID) and call the appropriate BL method (`Create` or `Update`).
    - **Success:** Show a success message and close the window.
    - **Failure:** Catch BL exceptions and show a user-friendly `MessageBox`.
- **Observers:**
    - In **Update Mode**, the window must register as an observer for the specific entity ID to reflect external changes immediately.
    - Unregister the observer when the window closes.
- **UI Layout:**
    - Use a `Grid` to align labels (description) and controls (values).
    - Match control types to data types (e.g., `ComboBox` for Enums, `CheckBox` for Booleans, `DatePicker` for DateTime).
    - Populate `ComboBox` items using `ObjectDataProvider` or static resources defined in `PL/Enums.cs`.

### 10.7. List to Detail View Interaction

- **Selection Binding:** Define a simple public property (getter/setter) in the code-behind for the selected item (e.g., `SelectedEntity`). Bind the `SelectedItem` of the list control (DataGrid/ListView) to this property.
- **Opening Details:**
    - **Update Mode:** Handle `MouseDoubleClick` on the list control. If the selected item property is not null, open the Single Item Window passing the entity's ID.
    - **Add Mode:** Handle the 'Add' button click. Open the Single Item Window without passing an ID (or passing 0).
    - **Window Mode:** Use `.Show()` to allow multiple windows to be open simultaneously.
