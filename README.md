# Chariot

Real-time chat backend project using **.NET**, **PostgreSQL**, and **SignalR**.
**Idea:** Chat + Period = Chariot (also like chatting on a chariot 🙂)

**Demo:** [https://chariot-xcce.onrender.com/api/\[SERVICE\]/\[ENDPOINT\]](https://chariot-xcce.onrender.com/api/[SERVICE]/[ENDPOINT])
> Due to render free instance limitation, The demo will spin down with inactivity, which can delay requests by 50 seconds or more.

**Postman collection:** [Download here](https://drive.google.com/file/d/1EaGbFRrR4RRaQ1Zn56F5Ih_qEPifTaYh/view?usp=sharing)

## 🔧 Installation & Setup

### Prerequisites

Ensure the following are installed:

* Latest version of **.NET SDK**
* **PostgreSQL**
* **Git**

### Steps

```bash
# Clone the repository
git clone https://github.com/tomamac/Chariot.git
cd Chariot

# Install dependencies
dotnet restore

# Configure environment variables
# Create a `.env` file in the root directory following `.env.example` and set necessary variables

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
```

## 🌐 API Endpoints

### Authentication

| Method | Endpoint             | Description           |
| ------ | -------------------- | --------------------- |
| POST   | `auth/register`      | Register a new user   |
| POST   | `auth/login`         | Login                 |
| POST   | `auth/logout`        | Logout                |
| POST   | `auth/guest-login`   | Login as a guest      |
| POST   | `auth/refresh-token` | Refresh access token  |
| GET    | `auth/me`            | Get current user info |

### Chat

| Method | Endpoint                  | Description                        |
| ------ | ------------------------- | ---------------------------------- |
| GET    | `chat/`                   | Get chat rooms the user has joined |
| GET    | `chat/[ROOM-ID]/messages` | Get messages of a chat room        |
| GET    | `chat/[ROOM-ID]/users`    | Get users in a chat room           |

## ⚡ SignalR Hub Methods

**Hub URL:**
`https://chariot-xcce.onrender.com/Chat?access_token=[ACCESS_TOKEN]`\
*(No token query needed if using cookies)*

| Method                      | Parameters                   | Description                                                                            |
| --------------------------- | ---------------------------- | -------------------------------------------------------------------------------------- |
| `CreateRoom`                | `string roomName`            | Creates a chat room and invokes `RoomCreated` with the room object                     |
| `DeleteRoom`                | `int roomId`                 | Deletes a chat room and invokes `RoomDeleted` with the roomId                          |
| `DisconnectFromDeletedRoom` | `int roomId`                 | Disconnects from a deleted room; clients should call this after notification           |
| `JoinRoom`                  | `string roomCode`            | Join a chat room, invokes `RoomJoined` and sends a system message via `ReceiveMessage` |
| `SendMessage`               | `int roomId, string message` | Sends a message and invokes `ReceiveMessage` with the message object                   |
| `LeaveRoom`                 | `int roomId`                 | Leave a room and invoke `ReceiveMessage` with a system message                         |

> This project uses **SignalR WebSocket**.
> Refer to the official [SignalR JavaScript Client documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client) for details on connecting and listening to events.

## 🗂️ Project Structure
```
/Chariot
│
├─ Controllers/     # API controllers
├─ Data/            # Database context
├─ Entities/        # Models representing database tables
├─ Hubs/            # SignalR hubs for real-time communication
├─ Models/          # DTOs
├─ Services/        # Business logic & service layer
└─ Program.cs       # App startup
```

## 🔐 Authentication

The backend uses **JWT Bearer tokens** for authentication.

1. **Login**:

```http
POST /auth/login
{
  "username": "user1",
  "password": "password123"
}
```

**Response:**

access_token and refresh_token http-only cookies

2. **Use the Token**:

Send the request with withCredentials: true
