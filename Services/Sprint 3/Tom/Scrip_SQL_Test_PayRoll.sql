/* =========================================================
   Farm Time Management - Reset & Seed Data
   ========================================================= */

-- Xóa dữ liệu theo thứ tự phụ thuộc (con → cha)
DELETE FROM Event;
DELETE FROM WorkSchedule;
DELETE FROM Biometric;
DELETE FROM Device;
DELETE FROM History;
DELETE FROM Staff;

-- Reset lại identity (nếu muốn ID chạy từ 1)
DBCC CHECKIDENT ('Event', RESEED, 0);
DBCC CHECKIDENT ('WorkSchedule', RESEED, 0);
DBCC CHECKIDENT ('Biometric', RESEED, 0);
DBCC CHECKIDENT ('Device', RESEED, 0);
DBCC CHECKIDENT ('History', RESEED, 0);
DBCC CHECKIDENT ('Staff', RESEED, 0);

-- Thêm Staff 1: Elon Musk (Admin)
INSERT INTO Staff
(FirstName, LastName, Email, Phone, [Password], [Address], ContractType, [Role],
 StandardHoursPerWeek, StandardPayRate, OvertimePayRate, IsActive)
VALUES
(N'Elon', N'Musk', N'admin@adelaidefarm.com', N'0988136755',
 N'8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',
 N'12 King William St, Adelaide SA 5000', N'Full-time', N'Admin',
 38.00, 35.00, 52.50, 1);

-- Thêm Staff 2: John Smith (Worker)
INSERT INTO Staff
(FirstName, LastName, Email, Phone, [Password], [Address], ContractType, [Role],
 StandardHoursPerWeek, StandardPayRate, OvertimePayRate, IsActive)
VALUES
(N'John', N'Smith', N'john.smith@adelaidefarm.com', N'0988123456',
 N'8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', -- tạm dùng cùng hash pass
 N'34 North Terrace, Adelaide SA 5000', N'Full-time', N'Worker',
 38.00, 25.00, 37.50, 1);

-- ===================================================================
-- INSERT CLOCK IN/OUT EVENTS FOR JOHN SMITH (StaffId = 2)
-- Week: Monday 2024-12-30 to Sunday 2025-01-05
-- Total: 46 hours raw (43.5 hours after breaks)
-- ===================================================================

-- MONDAY 2024-12-30: 10 hours (8:00 AM - 6:30 PM with 30 min break)
INSERT INTO [Event] ([Timestamp], StaffId, DeviceId, EventType, Reason)
VALUES 
('2024-12-30 08:00:00', 2, 1, 'Clock in', NULL),
('2024-12-30 18:30:00', 2, 1, 'Clock out', NULL);

-- TUESDAY 2024-12-31: 8 hours (8:00 AM - 4:30 PM with 30 min break)
INSERT INTO [Event] ([Timestamp], StaffId, DeviceId, EventType, Reason)
VALUES 
('2024-12-31 08:00:00', 2, 1, 'Clock in', NULL),
('2024-12-31 16:30:00', 2, 1, 'Clock out', NULL);

-- WEDNESDAY 2025-01-01: 8 hours (8:00 AM - 4:30 PM with 30 min break)
INSERT INTO [Event] ([Timestamp], StaffId, DeviceId, EventType, Reason)
VALUES 
('2025-01-01 08:00:00', 2, 1, 'Clock in', NULL),
('2025-01-01 16:30:00', 2, 1, 'Clock out', NULL);

-- THURSDAY 2025-01-02: 8 hours (8:00 AM - 4:30 PM with 30 min break)
INSERT INTO [Event] ([Timestamp], StaffId, DeviceId, EventType, Reason)
VALUES 
('2025-01-02 08:00:00', 2, 1, 'Clock in', NULL),
('2025-01-02 16:30:00', 2, 1, 'Clock out', NULL);

-- FRIDAY 2025-01-03: 8 hours (8:00 AM - 4:30 PM with 30 min break)
INSERT INTO [Event] ([Timestamp], StaffId, DeviceId, EventType, Reason)
VALUES 
('2025-01-03 08:00:00', 2, 1, 'Clock in', NULL),
('2025-01-03 16:30:00', 2, 1, 'Clock out', NULL);

-- SUNDAY 2025-01-05: 4 hours (9:00 AM - 1:00 PM, no break <5 hrs)
INSERT INTO [Event] ([Timestamp], StaffId, DeviceId, EventType, Reason)
VALUES 
('2025-01-05 09:00:00', 2, 1, 'Clock in', NULL),
('2025-01-05 13:00:00', 2, 1, 'Clock out', NULL);
 