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
