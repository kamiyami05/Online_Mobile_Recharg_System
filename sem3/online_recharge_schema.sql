create database OnlineRechargeDB
go


USE OnlineRechargeDB
go

-- KÍCH HOẠT QUY TẮC FOREIGN KEY
-- Dùng GO để tách các lệnh (đặc trưng của SQL Server)
GO

-- ====================================================================
-- PHẦN 1: QUẢN LÝ NGƯỜI DÙNG & TRUY CẬP (USER & ACCESS MANAGEMENT)
-- ====================================================================

-- 1. Bảng Roles (Định nghĩa vai trò)
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName VARCHAR(50) NOT NULL UNIQUE -- Admin, User, Guest
);
GO

insert into Roles values
('Admin'), ('User')
go

-- 2. Bảng Users (Thông tin Người dùng đã Đăng ký)
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    MobileNumber VARCHAR(10) NOT NULL UNIQUE CHECK (LEN(MobileNumber) = 10), -- Tên đăng nhập
    PasswordHash NVARCHAR(128) NOT NULL, -- Mật khẩu đã băm
    FullName NVARCHAR(100),
    Email VARCHAR(100),
	Address NVARCHAR(255),
    RegistrationDate DATETIME DEFAULT GETDATE(),
	RoleID int foreign key references Roles(RoleID) default 2,
    -- (Có thể thêm các trường khác cho Edit Personal Details)
);
GO

-- 3. Bảng AdminUsers (Tài khoản Admin)
CREATE TABLE AdminUsers (
    AdminID INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(128) NOT NULL,
    Email VARCHAR(100)
);
GO

-- ====================================================================
-- PHẦN 2: CHỨC NĂNG GIAO DỊCH (TRANSACTION & PAYMENT)
-- ====================================================================

-- 4. Bảng RechargePlans (Các gói Top Up & Special Recharge)
CREATE TABLE RechargePlans (
    PlanID INT PRIMARY KEY IDENTITY(1,1),
    PlanType VARCHAR(50) NOT NULL, -- 'TOP_UP' hoặc 'SPECIAL_RECHARGE'
    Amount DECIMAL(10, 2) NOT NULL,
    TalkTimeMinutes INT,
    DataMB INT,
    Details NVARCHAR(255),
    IsActive BIT DEFAULT 1
);
GO

-- 5. Bảng Transactions (Chi tiết Giao dịch Nạp tiền/Thanh toán)
CREATE TABLE Transactions (
    TransactionID INT PRIMARY KEY IDENTITY(1,1),
    MobileNumber VARCHAR(10) NOT NULL CHECK (LEN(MobileNumber) = 10),
    UserID INT NULL, -- NULL nếu là Guest User
    TransactionType VARCHAR(50) NOT NULL, -- RECHARGE, POSTPAID_PAYMENT
    PlanID INT NULL, -- NULL nếu là Postpaid
    Amount DECIMAL(10, 2) NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL, -- SUCCESS, FAILED, PENDING
    
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
    -- FOREIGN KEY (PlanID) REFERENCES RechargePlans(PlanID) -- Có thể không cần nếu giao dịch là Postpaid
);
GO

-- 6. Bảng TransactionScripts (Biên lai giao dịch)
CREATE TABLE TransactionScripts (
    ScriptID INT PRIMARY KEY IDENTITY(1,1),
    TransactionID INT NOT NULL UNIQUE,
    ScriptContent TEXT NOT NULL, -- Nội dung biên lai để in ra
    
    FOREIGN KEY (TransactionID) REFERENCES Transactions(TransactionID)
);
GO

-- 7. Bảng PaymentDetails (Chi tiết Thanh toán)
CREATE TABLE PaymentDetails (
    PaymentDetailID INT PRIMARY KEY IDENTITY(1,1),
    TransactionID INT NOT NULL UNIQUE,
    PaymentMethod VARCHAR(50) NOT NULL, -- VISA, Master Card, Wallet, v.v.
    ReferenceNumber VARCHAR(100) UNIQUE,
    
    FOREIGN KEY (TransactionID) REFERENCES Transactions(TransactionID)
);
GO

-- 8. Bảng PostpaidBills (Thông tin Hóa đơn Trả sau)
CREATE TABLE PostpaidBills (
    BillID INT PRIMARY KEY IDENTITY(1,1),
    MobileNumber VARCHAR(10) NOT NULL CHECK (LEN(MobileNumber) = 10),
    BillingCycle DATE NOT NULL,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    PaymentDueDate DATE,
    PaymentTransactionID INT NULL UNIQUE, -- NULL nếu chưa thanh toán
    IsPaid BIT DEFAULT 0,
    
    FOREIGN KEY (PaymentTransactionID) REFERENCES Transactions(TransactionID)
);
GO

-- ====================================================================
-- PHẦN 3: DỊCH VỤ & TÍNH NĂNG TÀI KHOẢN (ACCOUNT SERVICES)
-- ====================================================================

-- 9. Bảng Services (Danh sách các dịch vụ)
CREATE TABLE Services (
    ServiceID INT PRIMARY KEY IDENTITY(1,1),
    ServiceName NVARCHAR(100) NOT NULL UNIQUE, -- Do not Disturb, Caller tunes
    ServiceDescription NVARCHAR(255)
);
GO

INSERT INTO Services (ServiceName, ServiceDescription) VALUES
('Do Not Disturb', 'Block promotional calls and messages'),
('Caller Tunes', 'Customize your call waiting music')
GO

-- 10. Bảng UserServices (Dịch vụ đã kích hoạt)
CREATE TABLE UserServices (
    UserServiceID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    ServiceID INT NOT NULL,
    ActivationDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ServiceID) REFERENCES Services(ServiceID),
    UNIQUE (UserID, ServiceID) -- Ngăn người dùng kích hoạt cùng một dịch vụ hai lần
);
GO

-- 14. Bảng UserServiceSettings (Cài đặt dịch vụ của người dùng)
CREATE TABLE UserServiceSettings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    ServiceID INT NOT NULL,
    IsEnabled BIT DEFAULT 0, -- Trạng thái bật/tắt dịch vụ
    SelectedTune VARCHAR(100) NULL, -- Tên file nhạc chờ (chỉ cho Caller Tunes)
    UpdatedDate DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ServiceID) REFERENCES Services(ServiceID),
    UNIQUE (UserID, ServiceID)
);
GO

-- 11. Bảng Feedback (Phản hồi)
CREATE TABLE Feedback (
    FeedbackID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NULL, -- NULL nếu là Guest User
    Name VARCHAR(100),
    Email VARCHAR(100),
    FeedbackText TEXT NOT NULL,
    SubmitDate DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- ====================================================================
-- PHẦN 4: NỘI DUNG TĨNH (STATIC CONTENT)
-- ====================================================================

-- 12. Bảng SiteContent (Nội dung About Us, Site Map)
CREATE TABLE SiteContent (
    ContentID INT PRIMARY KEY IDENTITY(1,1),
    PageName VARCHAR(50) NOT NULL UNIQUE, -- About Us, Site Map
    Title NVARCHAR(100),
    ContentText NVARCHAR(MAX)
);
GO

-- 13. Bảng ContactPoints (Chi tiết liên hệ)
CREATE TABLE ContactPoints (
    ContactID INT PRIMARY KEY IDENTITY(1,1),
    ContactType VARCHAR(50) NOT NULL UNIQUE, -- Customer Care, Mobile Centre
    Details NVARCHAR(255) NOT NULL, -- Số điện thoại, Địa chỉ, Email
    Description NVARCHAR(255)
);
GO

-- Add Column Name for RechargePlans table
alter table rechargeplans
add PlanName nvarchar(100) NOT NULL Default N'Unnamed Plan';