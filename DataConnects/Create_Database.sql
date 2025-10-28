/* =========================================================
   Farm Time Management - Unified Schema (Sprint 1 + Sprint 2)
   - Style: DROP TABLE IF EXISTS, no GO, DATETIME2(0), StaffId casing
   ========================================================= */

-- Xoá bảng theo thứ tự phụ thuộc (con → cha)
DROP TABLE IF EXISTS Payslip;
DROP TABLE IF EXISTS Event;
DROP TABLE IF EXISTS WorkSchedule;
DROP TABLE IF EXISTS Biometric;
DROP TABLE IF EXISTS Device;
DROP TABLE IF EXISTS History;
DROP TABLE IF EXISTS Staff;

------------------------------------------------------------
-- BẢNG STAFF (nhân sự)
------------------------------------------------------------
CREATE TABLE Staff (
    StaffId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255),
    Phone NVARCHAR(30),
    Password NVARCHAR(255),      -- hash ở sprint sau
    Address NVARCHAR(255),
    ContractType NVARCHAR(20),
    Role NVARCHAR(50),           -- Admin, Worker...
    StandardPayRate DECIMAL(10,2)
);

------------------------------------------------------------
-- BẢNG HISTORY (log hành động hệ thống)
------------------------------------------------------------
CREATE TABLE History (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME2(0) NOT NULL,   -- đồng bộ: đến giây
    Actor NVARCHAR(150),
    Ip NVARCHAR(255),
    Action NVARCHAR(50),
    Result NVARCHAR(20),
    Details NVARCHAR(MAX)
);

------------------------------------------------------------
-- BẢNG DEVICE (thiết bị chấm công)
------------------------------------------------------------
CREATE TABLE Device (
    DeviceId INT IDENTITY(1,1) PRIMARY KEY,
    Location NVARCHAR(100) NOT NULL,   -- "lat,long"
    Type NVARCHAR(50) NOT NULL,
    Status NVARCHAR(30) NOT NULL
);

------------------------------------------------------------
-- BẢNG BIOMETRIC (sinh trắc học nhân viên)
------------------------------------------------------------
CREATE TABLE Biometric (
    BiometricId INT IDENTITY(1,1) PRIMARY KEY,
    StaffId INT NOT NULL,
    Type NVARCHAR(50) NOT NULL,    -- 'finger print','face','card',...
    Data NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (StaffId) REFERENCES Staff(StaffId)
    -- FOREIGN KEY (StaffId) REFERENCES Staff(StaffId) ON DELETE CASCADE
);

------------------------------------------------------------
-- BẢNG WORKSCHEDULE (lịch làm việc cho nhân viên)
------------------------------------------------------------
CREATE TABLE WorkSchedule (
    ScheduleId INT IDENTITY(1,1) PRIMARY KEY,
    StaffId INT NOT NULL,
    StartTime DATETIME2(0) NOT NULL,   -- ngày giờ bắt đầu (tới giây)
    EndTime   DATETIME2(0) NOT NULL,   -- ngày giờ kết thúc (tới giây)
    ScheduleHours INT NOT NULL,
    FOREIGN KEY (StaffId) REFERENCES Staff(StaffId)
    -- FOREIGN KEY (StaffId) REFERENCES Staff(StaffId) ON DELETE CASCADE
);

------------------------------------------------------------
-- BẢNG EVENT (điểm danh thực tế)
------------------------------------------------------------
CREATE TABLE Event (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME2(0) NOT NULL,  -- ngày giờ tới giây
    StaffId INT NOT NULL,
    DeviceId INT NULL,
    EventType NVARCHAR(50) NOT NULL,  -- dùng EventType (đồng bộ với model C#)
    Reason NVARCHAR(255) NULL,
    FOREIGN KEY (StaffId) REFERENCES Staff(StaffId),
    -- FOREIGN KEY (StaffId) REFERENCES Staff(StaffId) ON DELETE CASCADE
    FOREIGN KEY (DeviceId) REFERENCES Device(DeviceId)
    -- FOREIGN KEY (DeviceId) REFERENCES Device(DeviceId) ON DELETE CASCADE
);

------------------------------------------------------------
-- BẢNG PAYSLIP (bảng lương - Sprint 3)
------------------------------------------------------------
CREATE TABLE Payslip (
    PayslipId INT IDENTITY(1,1) PRIMARY KEY,
    StaffId INT NOT NULL,
    StandardPayRate DECIMAL(10,2) NOT NULL,
    WeekStartDate DATE NOT NULL,
    TotalHoursWorked DECIMAL(8,2) NOT NULL,
    GrossWeeklyPay DECIMAL(10,2) NOT NULL,
    AnnualIncome DECIMAL(12,2) NOT NULL,
    AnnualTax DECIMAL(10,2) NOT NULL,
    WeeklyPAYG DECIMAL(10,2) NOT NULL,
    NetPay DECIMAL(10,2) NOT NULL,
    EmployerSuperannuation DECIMAL(10,2) NOT NULL,
    DateCreated DATETIME2(0) NOT NULL,
    FOREIGN KEY (StaffId) REFERENCES Staff(StaffId)
);

------------------------------------------------------------
-- INSERT SAMPLE DATA
------------------------------------------------------------

-- Sample Staff data
INSERT INTO Staff (FirstName, LastName, Email, Phone, Password, Address, ContractType, Role, StandardPayRate)
VALUES 
    ('Elon', 'Musk', 'admin@adelaidefarm.com', '0988136755', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', '12 King William St, Adelaide SA 5000', 'Full-time', 'Admin', 50.00),
    ('John', 'Smith', 'john.smith@adelaidefarm.com', '0988123456', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', '34 North Terrace, Adelaide SA 5000', 'Full-time', 'Worker', 25.00);

-- Sample Device data
INSERT INTO Device (Location, Type, Status)
VALUES 
    ('Gate A', 'Card', 'Online'),
    ('Gate B', 'Card', 'Online');

-- Sample WorkSchedule data for John Smith (Oct 13-17, 2025)
INSERT INTO WorkSchedule (StaffId, StartTime, EndTime, ScheduleHours)
VALUES 
    (2, '2025-10-13 09:00:00', '2025-10-13 17:00:00', 8),
    (2, '2025-10-14 09:00:00', '2025-10-14 17:00:00', 8),
    (2, '2025-10-15 09:00:00', '2025-10-15 17:00:00', 8),
    (2, '2025-10-16 09:00:00', '2025-10-16 17:00:00', 8),
    (2, '2025-10-17 09:00:00', '2025-10-17 17:00:00', 8);

-- Sample Biometric data for John Smith (StaffId = 2)
INSERT INTO Biometric (StaffId, Type, Data)
VALUES 
    (2, 'Card', '123456789');

-- Sample Event data for John Smith (Oct 13-17, 2025)
-- Each day has: Clock in at 9AM, Break start at 12PM, Break end at 1PM, Clock out at 5PM
-- DeviceId alternates between 1 and 2 for each day
INSERT INTO Event (Timestamp, StaffId, DeviceId, EventType, Reason)
VALUES 
    -- Oct 13, 2025 (DeviceId = 1)
    ('2025-10-13 09:00:00', 2, 1, 'Clock in', NULL),
    ('2025-10-13 12:00:00', 2, 1, 'Break', NULL),
    ('2025-10-13 13:00:00', 2, 1, 'Break', NULL),
    ('2025-10-13 17:00:00', 2, 1, 'Clock out', NULL),
    
    -- Oct 14, 2025 (DeviceId = 2)
    ('2025-10-14 09:00:00', 2, 2, 'Clock in', NULL),
    ('2025-10-14 12:00:00', 2, 2, 'Break', NULL),
    ('2025-10-14 13:00:00', 2, 2, 'Break', NULL),
    ('2025-10-14 17:00:00', 2, 2, 'Clock out', NULL),
    
    -- Oct 15, 2025 (DeviceId = 1)
    ('2025-10-15 09:00:00', 2, 1, 'Clock in', NULL),
    ('2025-10-15 12:00:00', 2, 1, 'Break', NULL),
    ('2025-10-15 13:00:00', 2, 1, 'Break', NULL),
    ('2025-10-15 17:00:00', 2, 1, 'Clock out', NULL),
    
    -- Oct 16, 2025 (DeviceId = 2)
    ('2025-10-16 09:00:00', 2, 2, 'Clock in', NULL),
    ('2025-10-16 12:00:00', 2, 2, 'Break', NULL),
    ('2025-10-16 13:00:00', 2, 2, 'Break', NULL),
    ('2025-10-16 17:00:00', 2, 2, 'Clock out', NULL),
    
    -- Oct 17, 2025 (DeviceId = 1)
    ('2025-10-17 09:00:00', 2, 1, 'Clock in', NULL),
    ('2025-10-17 12:00:00', 2, 1, 'Break', NULL),
    ('2025-10-17 13:00:00', 2, 1, 'Break', NULL),
    ('2025-10-17 17:00:00', 2, 1, 'Clock out', NULL);

-- Sample WorkSchedule data for John Smith (Oct 20-24, 2025)
INSERT INTO WorkSchedule (StaffId, StartTime, EndTime, ScheduleHours)
VALUES 
    (2, '2025-10-20 09:00:00', '2025-10-20 17:00:00', 8),
    (2, '2025-10-21 09:00:00', '2025-10-21 17:00:00', 8),
    (2, '2025-10-22 09:00:00', '2025-10-22 17:00:00', 8),
    (2, '2025-10-23 09:00:00', '2025-10-23 17:00:00', 8),
    (2, '2025-10-24 09:00:00', '2025-10-24 17:00:00', 8);

-- Sample Event data for John Smith (Oct 20-24, 2025)
-- Each day has: Clock in at 9AM, Break start at 12PM, Break end at 1PM, Clock out at 5PM
-- DeviceId alternates between 1 and 2 for each day
INSERT INTO Event (Timestamp, StaffId, DeviceId, EventType, Reason)
VALUES 
    -- Oct 20, 2025 (DeviceId = 1)
    ('2025-10-20 09:00:00', 2, 1, 'Clock in', NULL),
    ('2025-10-20 12:00:00', 2, 1, 'Break', NULL),
    ('2025-10-20 13:00:00', 2, 1, 'Break', NULL),
    ('2025-10-20 17:00:00', 2, 1, 'Clock out', NULL),
    
    -- Oct 21, 2025 (DeviceId = 2)
    ('2025-10-21 09:00:00', 2, 2, 'Clock in', NULL),
    ('2025-10-21 12:00:00', 2, 2, 'Break', NULL),
    ('2025-10-21 13:00:00', 2, 2, 'Break', NULL),
    ('2025-10-21 17:00:00', 2, 2, 'Clock out', NULL),
    
    -- Oct 22, 2025 (DeviceId = 1)
    ('2025-10-22 09:00:00', 2, 1, 'Clock in', NULL),
    ('2025-10-22 12:00:00', 2, 1, 'Break', NULL),
    ('2025-10-22 13:00:00', 2, 1, 'Break', NULL),
    ('2025-10-22 17:00:00', 2, 1, 'Clock out', NULL),
    
    -- Oct 23, 2025 (DeviceId = 2)
    ('2025-10-23 09:00:00', 2, 2, 'Clock in', NULL),
    ('2025-10-23 12:00:00', 2, 2, 'Break', NULL),
    ('2025-10-23 13:00:00', 2, 2, 'Break', NULL),
    ('2025-10-23 17:00:00', 2, 2, 'Clock out', NULL),
    
    -- Oct 24, 2025 (DeviceId = 1)
    ('2025-10-24 09:00:00', 2, 1, 'Clock in', NULL),
    ('2025-10-24 12:00:00', 2, 1, 'Break', NULL),
    ('2025-10-24 13:00:00', 2, 1, 'Break', NULL),
    ('2025-10-24 17:00:00', 2, 1, 'Clock out', NULL);
