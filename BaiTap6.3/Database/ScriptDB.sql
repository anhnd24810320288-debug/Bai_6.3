/* =====================================================
   BÀI 6.3 - ProjectTaskDB : Quản lý công việc & tiến độ dự án
   Chạy toàn bộ script này trong SSMS (SQL Server)
   ===================================================== */
USE master;
GO
-- Xóa CSDL cũ (nếu có) bằng dynamic SQL để không bị lỗi khi CSDL chưa tồn tại
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'ProjectTaskDB')
BEGIN
    EXEC (N'ALTER DATABASE [ProjectTaskDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE');
    EXEC (N'DROP DATABASE [ProjectTaskDB]');
END
GO
CREATE DATABASE [ProjectTaskDB];
GO
USE [ProjectTaskDB];
GO

CREATE TABLE DuAn (
    MaDuAn      INT IDENTITY(1,1) PRIMARY KEY,
    TenDuAn     NVARCHAR(150) NOT NULL,
    NgayBatDau  DATETIME NULL,
    NgayKetThuc DATETIME NULL,
    TrangThai   NVARCHAR(50) NULL,
    CONSTRAINT CK_DuAn_Ngay CHECK (NgayKetThuc IS NULL OR NgayBatDau IS NULL OR NgayKetThuc >= NgayBatDau)
);

CREATE TABLE ThanhVien (
    MaThanhVien INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    Email       VARCHAR(100) NULL UNIQUE,
    ChuyenMon   NVARCHAR(100) NULL
);

CREATE TABLE CongViec (
    MaTask    INT IDENTITY(1,1) PRIMARY KEY,
    MaDuAn    INT NOT NULL,
    TenTask   NVARCHAR(200) NOT NULL,
    MoTa      NVARCHAR(MAX) NULL,
    HanNop    DATETIME NULL,
    DoUuTien  NVARCHAR(20) NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'To Do',
    CONSTRAINT FK_CongViec_DuAn FOREIGN KEY (MaDuAn) REFERENCES DuAn(MaDuAn),
    CONSTRAINT CK_CongViec_UuTien CHECK (DoUuTien IN (N'Cao', N'Trung bình', N'Thấp')),
    CONSTRAINT CK_CongViec_TrangThai CHECK (TrangThai IN (N'To Do', N'In Progress', N'Review', N'Done'))
);

CREATE TABLE PhanCong (
    MaTask      INT NOT NULL,
    MaThanhVien INT NOT NULL,
    NgayGiao    DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_PhanCong PRIMARY KEY (MaTask, MaThanhVien),
    CONSTRAINT FK_PhanCong_CongViec  FOREIGN KEY (MaTask)      REFERENCES CongViec(MaTask),
    CONSTRAINT FK_PhanCong_ThanhVien FOREIGN KEY (MaThanhVien) REFERENCES ThanhVien(MaThanhVien)
);
GO

/* ---------------- DỮ LIỆU MẪU ---------------- */
INSERT INTO DuAn (TenDuAn, NgayBatDau, NgayKetThuc, TrangThai) VALUES
(N'Website Bán Hàng',        DATEADD(DAY,-30,GETDATE()), DATEADD(DAY,30,GETDATE()), N'Đang chạy'),
(N'Ứng dụng Mobile Học Tập', DATEADD(DAY,-60,GETDATE()), DATEADD(DAY,-5,GETDATE()), N'Hoàn thành'),
(N'Hệ thống ERP Nội Bộ',     DATEADD(DAY,-10,GETDATE()), DATEADD(DAY,90,GETDATE()), N'Tạm dừng');

INSERT INTO ThanhVien (HoTen, Email, ChuyenMon) VALUES
(N'Nguyễn Văn An',  'an.nv@example.com',   N'Backend (C#/.NET)'),
(N'Trần Thị Bình',  'binh.tt@example.com', N'Frontend'),
(N'Lê Hoàng Cường', 'cuong.lh@example.com',N'Kiểm thử (QA)'),
(N'Phạm Minh Dũng', 'dung.pm@example.com', N'Thiết kế UI/UX');

INSERT INTO CongViec (MaDuAn, TenTask, MoTa, HanNop, DoUuTien, TrangThai) VALUES
(1, N'Thiết kế CSDL',            N'Vẽ ERD và tạo script SQL',          DATEADD(DAY,-20,GETDATE()), N'Cao',        N'Done'),
(1, N'Xây dựng trang đăng nhập', N'Form đăng nhập + xác thực',         DATEADD(DAY,-2, GETDATE()), N'Cao',        N'In Progress'),
(1, N'Trang danh sách sản phẩm', N'Hiển thị, phân trang, tìm kiếm',    DATEADD(DAY,7,  GETDATE()), N'Trung bình', N'In Progress'),
(1, N'Giỏ hàng & thanh toán',    N'Thêm/xóa sản phẩm, tính tổng',      DATEADD(DAY,14, GETDATE()), N'Cao',        N'To Do'),
(1, N'Kiểm thử chức năng',       N'Viết test case, chạy kiểm thử',     DATEADD(DAY,21, GETDATE()), N'Thấp',       N'To Do'),
(1, N'Review mã nguồn',          N'Code review module đăng nhập',      DATEADD(DAY,-1, GETDATE()), N'Trung bình', N'Review'),
(2, N'Thiết kế giao diện app',   N'Wireframe và mockup',               DATEADD(DAY,-40,GETDATE()), N'Cao',        N'Done'),
(2, N'Phát hành lên store',      N'Đóng gói và đăng ứng dụng',         DATEADD(DAY,-10,GETDATE()), N'Cao',        N'Done'),
(3, N'Khảo sát yêu cầu',         N'Phỏng vấn phòng ban',               DATEADD(DAY,-3, GETDATE()), N'Trung bình', N'To Do');

INSERT INTO PhanCong (MaTask, MaThanhVien) VALUES
(1,1),(2,1),(2,2),(3,2),(4,1),(4,2),(5,3),(6,3),(6,1),(7,4),(8,2),(9,3);
GO
SELECT 'Tao CSDL ProjectTaskDB thanh cong' AS KetQua;
