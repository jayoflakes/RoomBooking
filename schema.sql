-- Meeting Room Booking Service - Database Schema
-- Run this against a database called RoomBookingDb (create the database first if it doesn't exist).

CREATE TABLE Rooms (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Capacity INT NOT NULL CHECK (Capacity > 0)
);

CREATE TABLE Bookings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoomId INT NOT NULL,
    OrganizerName NVARCHAR(200) NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    AttendeeCount INT NOT NULL CHECK (AttendeeCount > 0),
    CONSTRAINT FK_Bookings_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
    CONSTRAINT CHK_EndAfterStart CHECK (EndTime > StartTime)
);

-- Optional seed data for local testing
INSERT INTO Rooms (Name, Capacity) VALUES
('Boardroom', 12),
('Focus Pod A', 2),
('The Hive', 6);
