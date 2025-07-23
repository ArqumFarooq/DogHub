# 🐾 DogHub – Dog Breed Management System

DogHub is a responsive and user-friendly web application built with ASP.NET MVC that allows users to explore dog breeds and provides administrators with complete control to manage breeds and users.

---

## 🔗 Live Preview

🌐 [View DogHub](https://doghub-dshybzhgcgd5fcem.canadacentral-01.azurewebsites.net/)  
💻 [View GitHub Repository](https://github.com/ArqumFarooq/DogHub)

---

## 📸 Preview Screenshots

| Login Page |
| ------------ |
| ![7](https://github.com/user-attachments/assets/846ff901-4dff-4f6d-9551-fc327bd8e8a9) |

| Landing Page | Detail Page |
| ------------ | ---------- |
| ![3](https://github.com/user-attachments/assets/67ec9370-b2a9-4825-a8ef-11189e2ffbc1) | ![4](https://github.com/user-attachments/assets/24d8791e-9cfa-44e5-b00a-2bfb82c501cd) |

| Admin Dashboard | User Dashboard |
| --------------- | --------------- |
| ![1](https://github.com/user-attachments/assets/d93c6308-f7d3-4518-abf3-9ff37f0f9d4e) | ![5](https://github.com/user-attachments/assets/15dcdb1a-da5e-45aa-a079-132e30425a19) |

| Dog Breed List | DogHub API |
| --------------- | ---------------- |
| ![2](https://github.com/user-attachments/assets/09e84b77-32c1-43fe-9b48-b07e32524598) | ![6](https://github.com/user-attachments/assets/21ae4bfc-2aa0-4e1a-9ab5-5c7dcd2c0ccd) |


---

## ✨ Features

### 👨‍💼 For Admin
- Full CRUD on users & dog breeds
- Manage user accounts and roles
- Audit logging for critical actions

### 👤 For Users
- Register and log in to explore breeds
- View detailed dog breed information
- Mobile-friendly UI with Bootstrap

### 🧩 APIs
- ASP.NET Web API for Dog Breed CRUD operations
- Swagger documentation for testing and exploring API endpoints

---

## 🛠 Technology Stack

| Category | Technologies |
|----------|--------------|
| **Frontend** | Bootstrap 5, jQuery, Razor Views |
| **Backend** | ASP.NET MVC 5, ADO.NET |
| **Database** | SQL Server 2018 |
| **DevOps** | Azure App Services, GitHub Actions |
| **API** | ASP.NET Web API, Swagger UI |

---

## 📂 Project Structure

| Folder/File    | Description                 |
| -------------- | --------------------------- |
| `Controllers/` | MVC Controllers             |
| `Models/`      | Domain Models               |
| `Views/`       | Razor Views for UI          |
| `DAL/`         | Data Access Layer           |
| `BL/`          | Business Logic Layer        |
| `App_Start/`   | Application Configuration   |
| `Content/`     | Static Assets (CSS, images) |
| `Scripts/`     | Client-side Scripts (JS)    |
| `App_Data/`    | Dog Breed Data Json Files   |
| `DogHub.sql`   | SQL Server Database Schema  |


---

## 🚀 Getting Started

### Prerequisites
- Visual Studio 2017+
- SQL Server 2016+
- .NET Framework 4.6

  
## Installation Guide

Follow the steps below to set up and run the DogHub project locally:

### 1. **Clone the repository**
   ```bash
   git clone https://github.com/ArqumFarooq/DogHub.git
   cd DogHub

2. Open the Solution
Launch Visual Studio 2017 or later

Open the DogHub.sln file from the cloned directory

3. Restore NuGet Packages
Go to Tools → NuGet Package Manager → Manage NuGet Packages for Solution

Click Restore or build the solution (Ctrl + Shift + B) to restore all required packages automatically

4. Set Up the Database
Open SQL Server Management Studio (SSMS)

Create a new database (e.g., DogHubDB)

Run the SQL script DogHub.sql (located in the root folder) to:

Create tables

Insert seed data (dog breeds, admin/user accounts)

5. Update Connection String
Open Web.config

Locate the <connectionStrings> section and update the value to match your local SQL Server setup:
<connectionStrings>
  <add name="DogHubDB" 
       connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=DogHubDB;Integrated Security=True" 
       providerName ="System.Data.SqlClient" />
</connectionStrings>

6. Configure Google Authentication
DogHub uses Google OAuth for external login support. Follow these steps:

6.1. Go to Google Developer Console

6.2. Create a new project

6.3. Enable OAuth consent screen

6.4. Create OAuth 2.0 Client ID under "Credentials"

6.4.1. Application type: Web Application

6.4.2. Add this to Authorized redirect URIs:
http://localhost:[PORT]/signin-google

6.5. Copy the Client ID and Client Secret

6.6. Open Startup.Auth.cs or App_Start\Startup.Auth.cs and replace the placeholders:

app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
{
    ClientId = "YOUR_GOOGLE_CLIENT_ID",
    ClientSecret = "YOUR_GOOGLE_CLIENT_SECRET"
});

7. Run the Application
Set DogHub as the startup project

Press F5 or click Start Debugging in Visual Studio 


8. (Optional) Access the API via Swagger
Navigate to:
http://localhost:[PORT]/swagger
(replace [PORT] with your local port number)
```

---

## 🤝 Contributing
I welcome contributions! Please follow these steps:

Fork the repository

Create your feature branch (git checkout -b feature/AmazingFeature)

Commit your changes (git commit -m 'Add some amazing feature')

Push to the branch (git push origin feature/AmazingFeature)

Open a pull request

---

## 👤 Author - Arqum Farooq

📧 Email: arqumfarooq1@gmail.com
🔗 [LinkedIn](https://www.linkedin.com/in/arqumfarooq/)

⭐️ If you find this project helpful, please consider starring the repository! Pull requests and suggestions are welcome :)
