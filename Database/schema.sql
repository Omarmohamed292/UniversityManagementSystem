-- Optional: run manually instead of relying on EnsureCreated() in Program.cs.
CREATE DATABASE UniversityDb;
GO
USE UniversityDb;
GO

CREATE TABLE Departments (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    Name      NVARCHAR(80) NOT NULL,
    Building  NVARCHAR(80) NULL,
    CONSTRAINT UQ_Departments_Name UNIQUE (Name)
);

CREATE TABLE Instructors (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    FirstName    NVARCHAR(50)  NOT NULL,
    LastName     NVARCHAR(50)  NOT NULL,
    Email        NVARCHAR(100) NOT NULL,
    HireDate     DATETIME2     NOT NULL,
    DepartmentId INT           NOT NULL,
    CONSTRAINT UQ_Instructors_Email UNIQUE (Email),
    CONSTRAINT FK_Instructors_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);

CREATE TABLE Students (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    FirstName      NVARCHAR(50)  NOT NULL,
    LastName       NVARCHAR(50)  NOT NULL,
    Email          NVARCHAR(100) NOT NULL,
    DateOfBirth    DATETIME2     NOT NULL,
    EnrollmentDate DATETIME2     NOT NULL,
    CONSTRAINT UQ_Students_Email UNIQUE (Email)
);

CREATE TABLE Courses (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Code         NVARCHAR(10)  NOT NULL,
    Title        NVARCHAR(100) NOT NULL,
    Credits      INT           NOT NULL CHECK (Credits BETWEEN 1 AND 6),
    DepartmentId INT           NOT NULL,
    InstructorId INT           NULL,
    CONSTRAINT UQ_Courses_Code UNIQUE (Code),
    CONSTRAINT FK_Courses_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_Courses_Instructors FOREIGN KEY (InstructorId) REFERENCES Instructors(Id) ON DELETE SET NULL
);

CREATE TABLE Enrollments (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    StudentId  INT       NOT NULL,
    CourseId   INT       NOT NULL,
    EnrolledOn DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_Enrollments_Student_Course UNIQUE (StudentId, CourseId),
    CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Enrollments_Courses  FOREIGN KEY (CourseId)  REFERENCES Courses(Id)  ON DELETE CASCADE
);
GO
