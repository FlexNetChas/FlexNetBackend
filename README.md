# FlexNet Backend

The backend for FlexNet is built using .NET Web API with Clean Architecture principles to ensure scalability, maintainability, and testability. The project handles all core logic related to user management, grade tracking, and other business functionalities for the Flexnet platform. The backend is designed to interact seamlessly with a SQL Server database hosted on Azure, using Entity Framework for data access.

Live version hosted at: https://flexnetfrontend.netlify.app
---

## Features

* **API-based Backend**: Provides a RESTful API for Flexnet’s frontend to consume.
* **Clean Architecture**: Organized into distinct layers (API, Application, Domain, Infrastructure) to ensure separation of concerns.
* **JWT Bearer Authentication**: Secured endpoints with JWT token-based authentication.
* **User Management**: Handles user registration, authentication, and session management.
* **Fluent Validation**: Data validation on inputs to ensure consistency and correctness.
* **Database Integration**: Entity Framework for ORM (SQL Server on Azure).
* **Security**: Uses bcrypt for password hashing and encryption to ensure secure user authentication.
* **Testing**: Includes unit and integration tests using XUnit to verify the functionality and reliability of the API.

---

## Tech Stack

### Backend:

* **.NET Web API**: The core framework for building the RESTful API.
* **Clean Architecture**: The architecture pattern used to separate concerns into layers (API, Application, Domain, Infrastructure).
* **C#**: Primary programming language for backend development.
* **Entity Framework**: ORM for interacting with the SQL Server database.
* **SQL Server (Azure)**: Hosted database to store user and academic data.
* **JWT Bearer Authentication**: Token-based authentication to secure API endpoints.
* **FluentValidation**: Used for input validation to ensure data integrity.
* **bcrypt**: Password hashing for secure user authentication.
* **XUnit**: Unit testing framework to ensure code quality and reliability.
  
### Frontend:
https://github.com/FlexNetChas/FlexNetReact
---

## Folder Structure

```
/src
  /API               
  /Application       
  /Domain             
  /Infrastructure    
```

---

## Getting Started

### Prerequisites

Make sure you have the following installed:

* [.NET SDK 6.0 or later](https://dotnet.microsoft.com/download/dotnet)
* SQL Server (local or Azure instance)
* Visual Studio, Rider or VSCode with C# extension
* (Optional) Postman or any other API testing tool

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/FlexNetChas/FlexNetBackend.git
   ```

2. Install dependencies (this should be done automatically when you build the project in Visual Studio or VSCode):

   ```bash
   dotnet restore
   ```

3. Set up your environment variables:

   * Create a `.env` file in the root of the project or use your environment management tools.
   * Set the following variables:

     * `ConnectionStrings:DefaultConnection`: Your connection string to the SQL Server database.
     * `Jwt:Secret`: A secret key used to sign JWT tokens.
     * `Jwt:Issuer`: The issuer of the JWT token.
     * `Jwt:Audience`: The audience for the JWT token.

4. Build and run the project:

   ```bash
   dotnet build
   dotnet run
   ```

   The API will start on `http://localhost:5000` (or the port defined in your configuration).

---

## Public API Endpoints

### Authentication

* **POST /api/auth/register**: Registers a new user.
* **POST /api/auth/login**: Authenticates a user and returns a JWT token.
 
---

## Running Tests

To run the tests for the backend project:

1. Build the project:

   ```bash
   dotnet build
   ```

2. Run the tests using XUnit:


## Contributing

We welcome contributions! If you'd like to contribute, please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them.
4. Push to your fork and submit a pull request.

Please ensure your code passes all tests and that new features are thoroughly tested.

---

## License

FlexNet Backend is open source and available under the MIT License.

---

## Future Enhancements

* **API Rate Limiting**: Implement rate limiting to prevent abuse of the API.
* **Voice Command Support**: Extend the backend to support voice command integration.
* **Grade Analytics**: Provide more detailed analytics and insights on student performance.
* **Profile Customization**: Allow users to customize their profiles, including avatars and personal details.

---
