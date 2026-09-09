# 🏨 Hotel Booking API 
### *A Robust Backend Demo with Concurrency & Idempotency*

[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/MySQL-8.0-orange)](https://www.mysql.com/)
[![Architecture](https://img.shields.io/badge/Architecture-RESTful-brightgreen)](https://restfulapi.net/)
[![License](https://img.shields.io/badge/License-MIT-lightgrey)](LICENSE)

---

## 📖 Table of Contents
- [About The Project](#-about-the-project)
- [Key Features](#-key-features)
- [Tech Stack](#-tech-stack)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [Live Examples](#-live-examples-curl--json)
- [Project Structure](#-project-structure)
- [Running Tests](#-Running-Unit-Tests)

---

## 🚀 About The Project

This project is a **clean, production-ready RESTful API** for a hotel reservation system. It was built to demonstrate advanced backend engineering principles, specifically tailored for the travel and hospitality industry.

The core challenge in hotel booking is **preventing double-booking** when two users try to reserve the same room simultaneously. This API elegantly solves that using **Optimistic Concurrency Control** while also ensuring **Idempotent Payment Processing** to prevent duplicate charges.

> *"Designed for scalability, maintainability, and absolute data integrity."*

---

## ✨ Key Features

- **✅ Optimistic Concurrency Control** – Prevents double-booking using `RowVersion` (Timestamp) in MySQL.
- **🛡️ Idempotent Requests** – Supports `Idempotency-Key` headers to safely retry bookings and payments without duplication.
- **🧱 SOLID Principles** – Clean separation of concerns (Controllers → Services → Repositories → Data).
- **🌐 RESTful Design** – Proper HTTP methods, status codes, and resource-based routing.
- **🧪 Unit Testing** – Includes xUnit tests for critical business logic.
- **📚 Swagger/OpenAPI** – Interactive API documentation available at `/swagger`.
- **⚠️ Global Exception Handling** – Centralized middleware that returns standardized error responses.

---

## 🧰 Tech Stack

| Technology | Purpose |
| :--- | :--- |
| **.NET 8** (C#) | Core framework |
| **ASP.NET Core** | Web API & Middleware |
| **Entity Framework Core** | ORM (Code-First) |
| **MySQL** (Pomelo) | Relational Database |
| **xUnit & Moq** | Unit Testing |
| **Swashbuckle** | Swagger UI |

---

## 🏗️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [MySQL Server](https://www.mysql.com/downloads/) (Local or Docker)

---

## 🔬 Live Examples (cURL & JSON)

Here are practical examples to demonstrate the API flow and error handling.

### 1. Fetch All Rooms
**Request:**
```bash
curl -X GET "https://localhost:5001/api/rooms"
```

### 2. Installation & Setup
1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/hotel-booking-api.git
   cd hotel-booking-api
   ```
   <br/>
2. **Configure the Database**  
   Update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=HotelBookingDB;User=root;Password=your_password;"
   }
   ```
   <br/>
3. **Apply Migrations & Seed Data**
   ```bash
   dotnet ef database update
   ```
   (This will create the schema and insert 3 sample rooms: Single, Double, and Suite.)

   <br/>
4. **Run the Application**
   ```bash
   dotnet run
   ```
   The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

   <br/>
5. **Explore Swagger**
   Navigate to `https://localhost:5001/swagger` to test endpoints interactively.

   
---

- ## 📡 API Endpoints

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/rooms` | Retrieves all available hotel rooms. | None |
| `POST` | `/api/bookings` | Creates a new booking. *Requires Idempotency-Key header.* | None |
| `GET` | `/api/bookings/{id}` | Fetches a specific booking by ID. | None |
| `DELETE` | `/api/bookings/{id}` | Cancels an unconfirmed booking. | None |
| `POST` | `/api/payments` | Processes payment for a booking. *Requires Idempotency-Key.* | None |


---

- ## 🔬 Live Examples (cURL & JSON)
  Here are practical examples to demonstrate the API flow and error handling.

### 1. Fetch All Rooms
**Request:**
```bash
curl -X GET "https://localhost:5001/api/rooms"
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "roomNumber": "101",
    "type": "Single",
    "pricePerNight": 50.00,
    "capacity": 1,
    "isAvailable": true
  },
  {
    "id": 2,
    "roomNumber": "102",
    "type": "Double",
    "pricePerNight": 80.00,
    "capacity": 2,
    "isAvailable": true
  }
]
```
<br/>
### 2. Create a Booking (with Idempotency)

Notice the `Idempotency-Key` header. If you send the same key again, the server returns the previous result without creating a duplicate.

**Request:**
```bash

curl -X POST "https://localhost:5001/api/bookings" \
  -H "Idempotency-Key: booking-001-xyz" \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": 1,
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "customerPhone": "+1234567890",
    "checkInDate": "2026-09-15T14:00:00",
    "checkOutDate": "2026-09-17T11:00:00"
  }'
```

**Response (201 Created):**
```json

{
  "id": 1,
  "roomId": 1,
  "roomNumber": "101",
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "checkInDate": "2026-09-15T14:00:00",
  "checkOutDate": "2026-09-17T11:00:00",
  "totalPrice": 100.00,
  "isConfirmed": false,
  "createdAt": "2026-09-09T10:30:00.123Z",
  "idempotencyKey": "booking-001-xyz"
}
```
<br/>
### 3. Process Payment for the Booking

If you accidentally send this request twice with the same `transaction-id`, it will not charge the user again.

**Request:**
```bash
curl -X POST "https://localhost:5001/api/payments" \
  -H "Content-Type: application/json" \
  -d '{
    "bookingId": 1,
    "amount": 100.00,
    "currency": "USD",
    "paymentMethod": "CreditCard",
    "transactionId": "txn-998877"
  }'
```

**Response (200 OK):**
```json
{
  "id": 1,
  "bookingId": 1,
  "amount": 100.00,
  "currency": "USD",
  "status": "Completed",
  "transactionId": "txn-998877",
  "paymentDate": "2026-09-09T10:35:00.456Z"
}
```
\
### 4. Conflict! (Handling Double-Booking)

If another user tries to book the same room simultaneously, the API detects the conflict via the `RowVersion` concurrency check and returns a 409 Conflict.

**Request** (Attempting to book Room #101 again):
```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Idempotency-Key: booking-002-abc" \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": 1,
    "customerName": "Jane Smith",
    "customerEmail": "jane@example.com",
    "customerPhone": "+0987654321",
    "checkInDate": "2026-09-15T14:00:00",
    "checkOutDate": "2026-09-17T11:00:00"
  }'
  ```
\
**Response (409 Conflict):**
```json
{
  "error": "DbUpdateConcurrencyException",
  "message": "اتاق توسط کاربر دیگری در حال رزرو است. لطفاً دوباره تلاش کنید.",
  "stackTrace": null
}
```

---

- ## 📂 Project Structure
  This clean architecture follows the **Separation of Concerns** principle:

  ```bash
      HotelBookingAPI/
      ├── Controllers/        # HTTP Layer (Request/Response)
      │   ├── RoomsController
      │   ├── BookingsController
      │   └── PaymentsController
      ├── Services/           # Business Logic Layer
      │   ├── BookingService
      │   ├── PaymentService
      │   └── IdempotencyService
      ├── Repositories/       # Data Access Layer (Abstractions & Implementations)
      ├── Models/             # Database Entities (Rooms, Bookings, Payments)
      ├── DTOs/               # Data Transfer Objects (Request/Response contracts)
      ├── Middleware/         # Global Exception Handling
      ├── Data/               # DbContext and EF Core Configurations
      ├── Extensions/         # Service Registration Helpers
      └── UnitTests/          # xUnit + Moq Test Cases
  ```


---

- ## 🧪 Running Unit Tests
To verify the core booking logic works perfectly:
``` bash
dotnet test
```
The suite includes tests for successful bookings, conflict scenarios, and duplicate request prevention.


---

- ## 🤝 Contributing

This is a demo project for learning and showcasing purposes. Suggestions and feedback are highly appreciated!

