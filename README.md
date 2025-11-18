# 🖥️ Cafe Management System

![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-green.svg)
![SignalR](https://img.shields.io/badge/SignalR-8.0-blue.svg)
![WPF](https://img.shields.io/badge/WPF-.NET-blue.svg)
![SQLite](https://img.shields.io/badge/SQLite-3.0-red.svg)
![License](https://img.shields.io/badge/License-MIT-yellow.svg)

A comprehensive, modern internet cafe management system built with .NET 10.0, featuring real-time client management, user authentication, billing, and remote control capabilities.

## ✨ Features

### 🎯 Core Features
- **Real-time Client Management** - Live status tracking with SignalR
- **User Authentication** - Secure JWT-based login system
- **Session Management** - Time tracking and billing automation
- **Remote Control** - Screen sharing and remote desktop control
- **Billing System** - Automated cost calculation and management
- **Multi-role Support** - Admin, Operator, and Customer roles

### 🛠️ Admin Capabilities
- **Client Dashboard** - Real-time monitoring of all workstations
- **Remote Desktop Control** - 10 FPS screen sharing with mouse/keyboard control
- **User Management** - Add credits, manage accounts, view history
- **Session Control** - Start, extend, and end user sessions
- **Live Monitoring** - Real-time screen viewing and messaging
- **Billing Reports** - Detailed usage and revenue tracking

### 💻 Client Features
- **Animated Lockscreen** - Beautiful gradient UI with particle effects
- **User Login Interface** - Secure credential input with session timer
- **Live Session Dashboard** - Real-time cost and duration tracking
- **System Tray Integration** - Background operation with easy access
- **Auto Registration** - Automatic client discovery and server connection
- **Session Extensions** - Easy time purchase and renewal

### 🔄 Real-time Communication
- **SignalR Hub** - Instant bidirectional communication
- **Status Broadcasting** - Live updates to all connected operators
- **Screen Sharing** - Compressed JPEG streaming at 10 FPS
- **Text Messaging** - Direct admin-to-user communication
- **Remote Commands** - Lock/unlock/restart operations

## 🏗️ Architecture Overview

### Clean Architecture Design
```
CafeManagement.Core          # Domain layer (Entities, Enums, Interfaces)
CafeManagement.Application   # Application layer (Services, DTOs, CQRS)
CafeManagement.Infrastructure # Infrastructure layer (EF Core, Repositories)
CafeManagement.Server        # ASP.NET Core Web API with SignalR
CafeManagement.Client        # WPF Desktop Client Application
```

### Technology Stack
- **Backend**: ASP.NET Core 8.0 with Entity Framework Core
- **Frontend**: WPF with MVVM pattern
- **Database**: SQLite with automatic migrations
- **Real-time**: SignalR for live communication
- **Authentication**: JWT tokens with password hashing
- **Mapping**: AutoMapper for object mapping
- **Patterns**: CQRS with MediatR, Repository Pattern

### Component Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   WPF Client    │    │  Web Admin UI   │    │  Mobile Admin   │
│                 │    │                 │    │                 │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                    ┌─────────────┴─────────────┐
                    │   ASP.NET Core Server     │
                    │  (Web API + SignalR Hub)  │
                    └─────────────┬─────────────┘
                                 │
                    ┌─────────────┴─────────────┐
                    │   SQLite Database         │
                    │   (EF Core Migrations)    │
                    └───────────────────────────┘
```

## 🔄 Business Logic & Workflows

### Client State Management
```
Offline ──> Idle ──> Online ──> InSession ──> Locked
   ↑                                      ↓
   └────────────── Session End ────────────┘
```

### Session Lifecycle
1. **Client Registration** - Automatic discovery and server registration
2. **User Authentication** - Login with credit validation
3. **Session Activation** - Time tracking and billing start
4. **Real-time Monitoring** - Live cost/duration updates
5. **Session Extension** - Additional time purchases
6. **Session Completion** - Final billing and client reset

### User Roles & Permissions
- **Administrator**: Full system access and user management
- **Operator**: Client control and session management
- **Customer**: Basic workstation usage with time tracking

## 🚀 Installation & Setup

### Prerequisites
- **.NET 10.0 SDK** - [Download .NET 10.0](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** or **VS Code** - Development environment
- **Windows OS** - Required for WPF client application
- **Git** - Version control system

### Quick Start

1. **Clone the Repository**
   ```bash
   git clone https://github.com/xAhmedAdel/Cafe-Management.git
   cd Cafe-Management
   ```

2. **Navigate to Server Project**
   ```bash
   cd CafeManagement.Server
   ```

3. **Run the Server**
   ```bash
   dotnet run
   ```
   The server will start on `http://localhost:5032`

4. **In New Terminal, Navigate to Client Project**
   ```bash
   cd CafeManagement.Client
   ```

5. **Run the Client Application**
   ```bash
   dotnet run
   ```

### Database Setup
The SQLite database is automatically created and configured on first run:
- Database file: `CafeManagement.Server/cafemanagement.db`
- Migrations are applied automatically
- Default admin user is created

### Configuration

#### Server Configuration (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=cafemanagement.db"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "ExpiryDays": 7
  },
  "SignalR": {
    "EnableDetailedErrors": true
  }
}
```

#### Client Configuration (`appsettings.json`)
```json
{
  "ServerSettings": {
    "BaseUrl": "http://localhost:5032",
    "HubUrl": "http://localhost:5032/hub/cafemanagement"
  }
}
```

## 📖 Usage Guide

### Accessing the Applications

#### Admin Web Interface
1. Open your browser and navigate to: `http://localhost:5032/admin`
2. Login with default credentials:
   - Username: `admin`
   - Password: `admin123`

#### Client Desktop Application
1. Launch the WPF client application
2. The client will auto-register with the server
3. Use the lockscreen interface to log in as a customer

### Key Features Usage

#### Starting a User Session
1. **From Client**: Enter username and password on the lockscreen
2. **From Admin**: Select a client and click "Start Session"
3. **Real-time Tracking**: Monitor session duration and cost live

#### Remote Desktop Control
1. **Select Client**: Choose a client from the admin dashboard
2. **Click Remote Control**: View the client's screen in real-time
3. **Take Control**: Enable mouse/keyboard control for assistance
4. **Send Messages**: Communicate directly with the user

#### Managing User Credits
1. **Select User**: Choose a user from the user management section
2. **Add Credits**: Enter amount and payment method
3. **View History**: Track usage patterns and payments

## 🔌 API Documentation

### Authentication Endpoints
```
POST /api/auth/login          - User authentication
POST /api/auth/register       - New user registration
POST /api/auth/refresh        - Refresh JWT token
```

### Client Management
```
GET  /api/clients            - Get all clients with status
GET  /api/clients/{id}       - Get specific client details
POST /api/clients/register   - Register new client
PUT  /api/clients/{id}       - Update client information
```

### Session Management
```
POST /api/sessions/start     - Start new session
POST /api/sessions/{id}/end  - End active session
GET  /api/sessions/active/{clientId} - Get active session
GET  /api/sessions/{id}      - Get session details
```

### SignalR Hub Methods
```csharp
// Client to Server
RegisterClient(ClientInfo client)
StartSession(int userId)
EndSession(int sessionId)
SendScreenCapture(byte[] imageData)

// Server to Client
LockWorkstation()
UnlockWorkstation()
ShowMessage(string message)
RemoteControl(bool enable)
```

## 🔌 Real-time Features

### **SignalR Hub**
Connect to the SignalR hub for real-time updates:
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub/cafemanagement")
    .build();

connection.on("ClientStatusUpdated", (client) => {
    // Handle client status updates
});

connection.on("SessionStarted", (session) => {
    // Handle new session
});
```

### **Screen Sharing**
The system supports high-performance screen sharing with:
- 10 FPS real-time streaming
- JPEG compression for bandwidth optimization
- Adaptive quality based on network conditions
- Multi-monitor support

### **Remote Control**
Full remote control capabilities including:
- Mouse movement and clicks
- Keyboard input simulation
- Text message transmission
- Workstation lock/unlock

## 🎨 UI Features

### **LockScreen**
- **Animated Background**: Gradient transitions with particle effects
- **Real-time Countdown**: Live session timer display
- **Status Indicators**: Color-coded warnings for time remaining
- **Session Information**: Display of current session details
- **Admin Controls**: Unlock functionality for administrators

### **Client Application**
- **Modern WPF Interface**: Clean, responsive design
- **MVVM Architecture**: Proper separation of concerns
- **Real-time Updates**: Live status and session information
- **Configuration Management**: Integrated settings panel

## 📊 Database Schema

The system uses SQLite with the following main entities:
- **Users**: Authentication and role management
- **Clients**: Computer registration and status tracking
- **Sessions**: Time tracking and billing records
- **LockScreenConfigs**: Custom lock screen settings
- **UsageLogs**: Audit trail and activity logs

## 🔒 Security Features

- **JWT Authentication**: Token-based authentication with role-based authorization
- **Password Hashing**: Secure password storage with salt
- **Role-Based Access**: Admin, Operator, and Customer permissions
- **Audit Logging**: Comprehensive activity tracking
- **Data Encryption**: Sensitive data protection

## 📱 Monitoring & Analytics

### **Usage Analytics**
- Real-time client status monitoring
- Session duration and revenue tracking
- Peak usage time analysis
- Client performance metrics

### **System Monitoring**
- API performance metrics
- Database query optimization
- SignalR connection health
- Error tracking and logging

## 🚀 Deployment

### **Production Deployment**

1. **Server Setup**:
   ```bash
   # Configure production database
   # Set up reverse proxy (nginx/IIS)
   # Configure SSL certificates
   ```

2. **Client Deployment**:
   ```bash
   # Automatic deployment via server
   # One-click installer creation
   # Windows service configuration
   ```

### **Auto-Update System**
- Automatic version checking
- Silent update installation
- Rollback capabilities
- Update notification system

## 🛠️ Development & Contributing

### Development Environment Setup
1. **Clone Repository**
   ```bash
   git clone https://github.com/xAhmedAdel/Cafe-Management.git
   ```

2. **Open Solution** in Visual Studio 2022 or VS Code
   ```bash
   # VS Code
   code Cafe-Management.sln

   # Visual Studio
   Cafe-Management.sln
   ```

3. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

4. **Build Solution**
   ```bash
   dotnet build
   ```

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test CafeManagement.Tests
```

### Project Structure
```
Cafe-Management/
├── 📁 CafeManagement.Core/
│   ├── 📁 Entities/           # Domain entities
│   ├── 📁 Enums/              # System enums
│   └── 📁 Interfaces/         # Domain interfaces
├── 📁 CafeManagement.Application/
│   ├── 📁 DTOs/               # Data transfer objects
│   ├── 📁 Services/           # Application services
│   └── 📁 Features/           # CQRS features
├── 📁 CafeManagement.Infrastructure/
│   ├── 📁 Data/               # EF Core DbContext
│   ├── 📁 Repositories/       # Repository implementations
│   └── 📁 Configurations/     # Entity configurations
├── 📁 CafeManagement.Server/
│   ├── 📁 Controllers/        # API controllers
│   ├── 📁 Hubs/               # SignalR hubs
│   └── 📁 wwwroot/admin/      # Web admin interface
├── 📁 CafeManagement.Client/
│   ├── 📁 Views/              # WPF windows
│   ├── 📁 ViewModels/         # MVVM view models
│   ├── 📁 Services/           # Client services
│   └── 📁 Models/             # Client models
└── 📁 Tests/                  # Unit and integration tests
```

### Contributing Guidelines
1. **Fork the Repository**
2. **Create Feature Branch** (`git checkout -b feature/amazing-feature`)
3. **Commit Changes** (`git commit -m 'Add amazing feature'`)
4. **Push to Branch** (`git push origin feature/amazing-feature`)
5. **Open Pull Request**

### Code Style
- Follow **C# naming conventions**
- Use **clean architecture** principles
- Write **unit tests** for new features
- Update **documentation** for API changes
- Use **async/await** for I/O operations

## 🔒 Security Features

### Authentication & Authorization
- **JWT Token-based Authentication** with expiration
- **Password Hashing** using BCrypt with salt
- **Role-based Access Control** (Admin, Operator, Customer)
- **Session Management** with secure logout
- **CORS Configuration** for web interface security

### Data Protection
- **SQL Injection Prevention** via Entity Framework Core
- **XSS Protection** in web interface
- **Input Validation** for all API endpoints
- **Secure File Upload** restrictions
- **Audit Logging** for compliance

## 📊 Performance Optimizations

### Real-time Communication
- **SignalR Connection Pooling** for scalability
- **Compressed Screen Sharing** (JPEG at 10 FPS)
- **Efficient Message Broadcasting** with groups
- **Connection Health Monitoring** with auto-reconnect
- **Memory-efficient Image Processing**

### Database Optimizations
- **Indexed Queries** for fast data retrieval
- **Connection Pooling** for concurrent access
- **Lazy Loading** for related entities
- **Efficient Migrations** with minimal downtime
- **SQLite Optimizations** for embedded deployment

## 🚀 Deployment

### Production Server Setup
1. **IIS Hosting** for ASP.NET Core application
2. **Windows Service** for background processing
3. **SQL Server** migration option for scaling
4. **Load Balancer** for high availability
5. **SSL Certificate** for HTTPS security

### Client Deployment
1. **ClickOnce Deployment** for easy updates
2. **Windows Installer** (.msi) package
3. **Auto-update Service** for version management
4. **System Tray Integration** for background operation
5. **Startup Configuration** for automatic launch

## 🖼️ Screenshots & Demo

### Admin Interface
![Admin Dashboard](docs/images/admin-dashboard.png)
*Real-time client monitoring and control*

### Client Lockscreen
![Client Lockscreen](docs/images/lockscreen.png)
*Animated user authentication interface*

### Remote Control
![Remote Control](docs/images/remote-control.png)
*Live desktop sharing and control*

### User Dashboard
![User Dashboard](docs/images/user-dashboard.png)
*Session tracking and management*

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Support & Contact

### Getting Help
- **GitHub Issues**: Report bugs and request features
- **Wiki**: Detailed documentation and guides
- **Discussions**: Community support and Q&A

### Contributors
- **[@xAhmedAdel](https://github.com/xAhmedAdel)** - Project Owner & Lead Developer
- **Contributors** - Thank you to all who contribute!

### Acknowledgments
- **Microsoft** for .NET and ASP.NET Core
- **SignalR Team** for real-time communication framework
- **Entity Framework Team** for excellent ORM
- **Community** for feedback and contributions

---

## 🎯 Project Highlights

⭐ **Modern Technology Stack** - Built with latest .NET 10.0 and best practices
⭐ **Real-time Capabilities** - Instant communication and screen sharing
⭐ **Production Ready** - Comprehensive error handling and security
⭐ **Scalable Architecture** - Clean design for easy extension and maintenance
⭐ **User-Friendly Interface** - Professional UI with excellent UX
⭐ **Complete Feature Set** - Everything needed for cafe management operations

**Ready to transform your internet cafe operations?** 🚀

[⬆️ Back to Top](#-cafe-management-system)