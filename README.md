# 🏢 Cafe Management System

A comprehensive, production-ready internet cafe management system built with modern .NET 10 technology stack featuring real-time communication, automatic deployment, and advanced remote control capabilities.

## 🎯 Key Features

### **Core Functionality**
- **User Management**: Admin, Operator, and Customer roles with authentication
- **Client Management**: Real-time client status tracking and monitoring
- **Session Management**: Time-based billing with automatic calculations
- **LockScreen System**: Beautiful animated lockscreen with countdown timers
- **Remote Control**: Full screen sharing with mouse and keyboard control
- **Real-time Updates**: SignalR-powered live communication
- **Automatic Deployment**: One-click client installation and updates

### **Technical Excellence**
- **Modern Architecture**: Clean Architecture with CQRS pattern
- **High Performance**: Optimized screen capture and data transmission
- **Database**: SQLite with EF Core for reliable data storage
- **Real-time**: SignalR for instant client-server communication
- **Cross-platform**: .NET 10 with WPF client application
- **Comprehensive Testing**: Unit and integration test coverage

## 🏗️ Architecture

### **Clean Architecture Layers**
```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                           │
├─────────────────────────────────────────────────────────────┤
│  Server (ASP.NET Core)      │    Client (WPF)                │
│  ┌─────────────────────┐     │    ┌─────────────────────────┐     │
│  │  Controllers      │     │    │  Views & Controls        │     │
│  │  SignalR Hub       │◄────┼── │    │  ViewModels            │     │
│  │  Deployment       │     │    │  Services             │     │
│  └─────────────────────┘�     │    └─────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                           │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────────┐     │    ┌─────────────────────┐     │
│  │  Commands & Queries │     │    │  Services           │     │
│  │  DTOs & Validators│     │    │  Handlers           │     │
│  │  AutoMapper       │     │    │  Mapping Profiles     │     │
│  └─────────────────────┘     │    └─────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Infrastructure Layer                        │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────────┐     │    ┌─────────────────────┐     │
│  │  EF Core Context │     │    │  Repositories        │     │
│  │  Data Seeding     │     │  │  Services           │     │
│  │  SignalR         │     │    │  Authentication      │     │
│  └─────────────────────┘     │    └─────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                        Core Layer                           │
│  ┌─────────────────────┐     │  ┌─────────────────────┐  │
│  │  Entities          │     │  │  Enums            │  │
│  │  Value Objects    │     │  │  Interfaces        │  │
│  │  Domain Events    │     │  │  Services         │  │
│  └─────────────────────┘     │  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 Getting Started

### **Prerequisites**
- .NET 10.0 SDK
- Visual Studio 2022 or VS Code
- Git

### **Installation**
1. Clone the repository:
```bash
git clone <repository-url>
cd CafeManagementSystem
```

2. Build the solution:
```bash
dotnet restore
dotnet build
```

3. Run the server:
```bash
dotnet run --project CafeManagement.Server
```

4. Run the client (from another terminal):
```bash
dotnet run --project CafeManagement.Client
```

## 🔧 Configuration

### **Server Configuration**
The server uses the following default configuration in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=cafemanagement.db"
  },
  "JwtSettings": {
    "Secret": "ThisIsASecretKeyForJWTTokenGeneration12345678901234567890",
    "Issuer": "CafeManagementSystem",
    "Audience": "CafeManagementClients",
    "ExpiryInHours": "24"
  }
}
```

### **Client Configuration**
The client automatically configures itself on first run:
- Registers with the server
- Saves configuration to `%APPDATA%\CafeManagement\config.json`
- Sets up Windows startup task for auto-launch

## 📖 API Documentation

### **Authentication**
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

### **Client Management**
```http
GET /api/clients
POST /api/clients
PUT /api/clients/{id}/status
DELETE /api/clients/{id}
```

### **Session Management**
```http
GET /api/sessions
POST /api/sessions
POST /api/sessions/{id}/end
POST /api/sessions/{id}/extend
```

### **Deployment**
```http
GET /api/deployment/check-updates?currentVersion=1.0.0.0
GET /api/deployment/download
GET /api/deployment/installer
POST /api/deployment/register-client
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

## 🧪 Testing

### **Run Tests**
```bash
dotnet test CafeManagement.Tests
```

### **Test Coverage**
- Unit tests for business logic
- Integration tests for API endpoints
- SignalR real-time communication tests
- UI automation for client application

## 📋 Project Structure

```
CafeManagementSystem/
├── src/
│   ├── CafeManagement.Core/          # Domain Layer
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   └── ValueObjects/
│   ├── CafeManagement.Application/  # Application Layer
│   │   ├── Services/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── DTOs/
│   │   └── Validators/
│   ├── CafeManagement.Infrastructure/ # Infrastructure Layer
│   │   ├── Data/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   └── ExternalServices/
│   ├── CafeManagement.Server/        # Server Application
│   │   ├── Controllers/
│   │   ├── Hubs/
│   │   ├── Services/
│   │   └── Configuration/
│   └── CafeManagement.Client/        # Client Application
│       ├── Services/
│       ├── Views/
│       ├── ViewModels/
│       ├── Controls/
│       └── Resources/
├── tests/
│   ├── Unit/
│   ├── Integration/
│   └── E2E/
└── docs/
    ├── API.md
    ├── Deployment.md
    └── UserGuide.md
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Email: support@cafemanagement.com
- Documentation: [Wiki](https://github.com/username/CafeManagementSystem/wiki)

---

**Built with ❤️ using .NET 10 and modern software engineering practices.**