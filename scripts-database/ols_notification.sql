-- ================================================================
-- YÊU CẦU 2: CƠ CHẾ PHÂN PHỐI THÔNG BÁO – ORACLE LABEL SECURITY
-- ================================================================
-- Script này thiết lập Oracle Label Security (OLS) cho bảng THONGBAO
-- trong schema QLBENHVIEN, tạo hệ thống bảo mật cấp dòng (row-level)
-- dựa trên 3 thành phần: Level, Compartment, Group.
--
-- ĐỂ CHẠY SCRIPT NÀY:
--   1. Kết nối vào XEPDB1 với quyền SYSDBA
--      sqlplus sys/1234567890@localhost:1521/xepdb1 as sysdba
--   2. Chạy từng khối lệnh theo thứ tự
-- ================================================================

-- ============================================================
-- BƯỚC 0: MỞ KHÓA LBACSYS VÀ CẤP QUYỀN
-- LBACSYS là schema sở hữu các package OLS (SA_SYSDBA, SA_COMPONENTS...)
-- ============================================================
ALTER USER LBACSYS IDENTIFIED BY lbacsys ACCOUNT UNLOCK;

GRANT EXECUTE ON LBACSYS.SA_SYSDBA       TO LBACSYS WITH GRANT OPTION;
GRANT EXECUTE ON LBACSYS.SA_COMPONENTS   TO LBACSYS WITH GRANT OPTION;
GRANT EXECUTE ON LBACSYS.SA_USER_ADMIN   TO LBACSYS WITH GRANT OPTION;
GRANT EXECUTE ON LBACSYS.SA_POLICY_ADMIN TO LBACSYS WITH GRANT OPTION;
GRANT EXECUTE ON LBACSYS.SA_LABEL_ADMIN  TO LBACSYS WITH GRANT OPTION;

-- ============================================================
-- BƯỚC 1: TẠO POLICY OLS
-- Policy THONGBAO_POL sẽ quản lý cột OLS_LABEL trên bảng THONGBAO
-- ============================================================
BEGIN
    LBACSYS.SA_SYSDBA.CREATE_POLICY(
        policy_name     => 'THONGBAO_POL',
        column_name     => 'OLS_LABEL',
        default_options => 'NO_CONTROL'
    );
END;
/

-- ============================================================
-- BƯỚC 2: TẠO CÁC LEVEL (CẤP BẬC)
-- Số level_num càng cao → quyền càng cao
-- GiamDoc(300) > LanhDao(200) > NhanVien(100)
-- ============================================================
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL(
        policy_name => 'THONGBAO_POL',
        level_num   => 300,
        short_name  => 'GD',
        long_name   => 'GiamDoc'
    );
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL(
        policy_name => 'THONGBAO_POL',
        level_num   => 200,
        short_name  => 'LD',
        long_name   => 'LanhDao'
    );
    LBACSYS.SA_COMPONENTS.CREATE_LEVEL(
        policy_name => 'THONGBAO_POL',
        level_num   => 100,
        short_name  => 'NV',
        long_name   => 'NhanVien'
    );
END;
/

-- ============================================================
-- BƯỚC 3: TẠO CÁC COMPARTMENT (KHOA/PHÒNG BAN)
-- Compartment KHÔNG phân cấp — mỗi khoa là độc lập
-- ============================================================
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT(
        policy_name => 'THONGBAO_POL',
        comp_num    => 10,
        short_name  => 'TH',
        long_name   => 'TieuHoa'
    );
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT(
        policy_name => 'THONGBAO_POL',
        comp_num    => 20,
        short_name  => 'TK',
        long_name   => 'ThanKinh'
    );
    LBACSYS.SA_COMPONENTS.CREATE_COMPARTMENT(
        policy_name => 'THONGBAO_POL',
        comp_num    => 30,
        short_name  => 'TM',
        long_name   => 'TimMach'
    );
END;
/

-- ============================================================
-- BƯỚC 4: TẠO CÁC GROUP (CƠ SỞ/ĐỊA ĐIỂM)
-- Group CÓ THỂ phân cấp — ở đây dùng ngang hàng (flat)
-- ============================================================
BEGIN
    LBACSYS.SA_COMPONENTS.CREATE_GROUP(
        policy_name => 'THONGBAO_POL',
        group_num   => 10,
        short_name  => 'HCM',
        long_name   => 'HoChiMinh'
    );
    LBACSYS.SA_COMPONENTS.CREATE_GROUP(
        policy_name => 'THONGBAO_POL',
        group_num   => 20,
        short_name  => 'HP',
        long_name   => 'HaiPhong'
    );
    LBACSYS.SA_COMPONENTS.CREATE_GROUP(
        policy_name => 'THONGBAO_POL',
        group_num   => 30,
        short_name  => 'HN',
        long_name   => 'HaNoi'
    );
END;
/

-- ============================================================
-- BƯỚC 5: TẠO BẢNG THONGBAO
-- Bảng lưu trữ các thông báo họp khẩn
-- Cột OLS_LABEL sẽ tự động được thêm bởi SA_POLICY_ADMIN.APPLY_TABLE_POLICY
-- ============================================================
CREATE TABLE QLBENHVIEN.THONGBAO (
    NOIDUNG   NVARCHAR2(500),
    NGAYGIO   TIMESTAMP DEFAULT SYSTIMESTAMP,
    DIADIEM   NVARCHAR2(200)
);

-- ============================================================
-- BƯỚC 6: ÁP DỤNG POLICY OLS LÊN BẢNG THONGBAO
-- READ_CONTROL: OLS chỉ kiểm soát quyền ĐỌC (SELECT)
-- Oracle tự thêm cột OLS_LABEL vào bảng
-- ============================================================
BEGIN
    LBACSYS.SA_POLICY_ADMIN.APPLY_TABLE_POLICY(
        policy_name   => 'THONGBAO_POL',
        schema_name   => 'QLBENHVIEN',
        table_name    => 'THONGBAO',
        table_options => 'READ_CONTROL'
    );
END;
/

-- ============================================================
-- BƯỚC 7: TẠO 8 ORACLE USER (U1 - U8) VÀ CẤP QUYỀN
-- Mỗi user đại diện một vai trò với clearance khác nhau
-- ============================================================

-- Xóa users cũ nếu tồn tại (bỏ comment nếu cần chạy lại)
-- BEGIN
--     FOR r IN (SELECT USERNAME FROM ALL_USERS WHERE USERNAME IN ('U1','U2','U3','U4','U5','U6','U7','U8')) LOOP
--         EXECUTE IMMEDIATE 'DROP USER ' || r.USERNAME || ' CASCADE';
--     END LOOP;
-- END;
-- /

CREATE USER U1 IDENTIFIED BY 123;
CREATE USER U2 IDENTIFIED BY 123;
CREATE USER U3 IDENTIFIED BY 123;
CREATE USER U4 IDENTIFIED BY 123;
CREATE USER U5 IDENTIFIED BY 123;
CREATE USER U6 IDENTIFIED BY 123;
CREATE USER U7 IDENTIFIED BY 123;
CREATE USER U8 IDENTIFIED BY 123;

GRANT CREATE SESSION TO U1, U2, U3, U4, U5, U6, U7, U8;
GRANT SELECT ON QLBENHVIEN.THONGBAO TO U1, U2, U3, U4, U5, U6, U7, U8;

-- ============================================================
-- BƯỚC 8: GÁN NHÃN BẢO MẬT CHO NGƯỜI DÙNG (USER LABELS)
-- Cú pháp nhãn: 'LEVEL:COMPARTMENTS:GROUPS'
--
-- Quy tắc đọc (READ ACCESS):
--   ✓ User.Level  >= Row.Level
--   ✓ User.Comps  ⊇  Row.Comps  (user có TẤT CẢ comp của row)
--   ✓ User.Groups ⊇  Row.Groups (user thuộc ít nhất 1 group của row)
-- ============================================================
BEGIN
    -- u1: Giám đốc – đọc TOÀN BỘ thông báo trong hệ thống
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
        policy_name    => 'THONGBAO_POL',
        user_name      => 'U1',
        max_read_label => 'GD:TH,TK,TM:HCM,HP,HN'
    );

    -- u2: Lãnh đạo Khoa Tim mạch tại HCM
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
        policy_name    => 'THONGBAO_POL',
        user_name      => 'U2',
        max_read_label => 'LD:TM:HCM'
    );

    -- u3: Lãnh đạo Khoa Thần kinh tại Hà Nội
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
        policy_name    => 'THONGBAO_POL',
        user_name      => 'U3',
        max_read_label => 'LD:TK:HN'
    );

    -- u4: Nhân viên Khoa Thần kinh tại HCM
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
        policy_name    => 'THONGBAO_POL',
        user_name      => 'U4',
        max_read_label => 'NV:TK:HCM'
    );

    -- u5: Nhân viên Khoa Tim mạch tại HCM
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
        policy_name    => 'THONGBAO_POL',
        user_name      => 'U5',
        max_read_label => 'NV:TM:HCM'
    );

    -- u6: Lãnh đạo phòng – đọc thông báo Khoa Tim mạch tại HCM
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
        policy_name    => 'THONGBAO_POL',
        user_name      => 'U6',
        max_read_label => 'LD:TM:HCM'
    );

    -- u7: Lãnh đạo phòng – đọc TOÀN BỘ thông báo phù hợp cấp lãnh đạo
    --     (tất cả khoa, tất cả cơ sở)
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
        policy_name    => 'THONGBAO_POL',
        user_name      => 'U7',
        max_read_label => 'LD:TH,TK,TM:HCM,HP,HN'
    );

    -- u8: Nhân viên Khoa Tiêu hóa tại Hà Nội
    LBACSYS.SA_USER_ADMIN.SET_USER_LABELS(
        policy_name    => 'THONGBAO_POL',
        user_name      => 'U8',
        max_read_label => 'NV:TH:HN'
    );
END;
/

-- ============================================================
-- BƯỚC 9: CẤP QUYỀN GHI CHO QLBENHVIEN ĐỂ INSERT DỮ LIỆU CÓ LABEL
-- FULL privilege cho phép QLBENHVIEN bỏ qua kiểm tra OLS khi ghi
-- ============================================================
BEGIN
    LBACSYS.SA_USER_ADMIN.SET_USER_PRIVS(
        policy_name => 'THONGBAO_POL',
        user_name   => 'QLBENHVIEN',
        privileges  => 'FULL'
    );
END;
/

-- ============================================================
-- BƯỚC 10: CHÈN DỮ LIỆU THÔNG BÁO VỚI NHÃN OLS (t1 - t7)
--
-- Dùng CHAR_TO_LABEL chuyển nhãn text → số ID nội bộ
-- Kết nối là QLBENHVIEN (hoặc SYS) để chèn
--
-- MA TRẬN TRUY CẬP DỰ KIẾN:
-- ┌─────┬────┬────┬────┬────┬────┬────┬────┐
-- │     │ t1 │ t2 │ t3 │ t4 │ t5 │ t6 │ t7 │
-- ├─────┼────┼────┼────┼────┼────┼────┼────┤
-- │ u1  │ ✓  │ ✓  │ ✓  │ ✓  │ ✓  │ ✓  │ ✓  │  (Giám đốc - xem tất cả)
-- │ u2  │ ✓  │    │ ✓  │    │    │    │    │  (LĐ Tim mạch/HCM)
-- │ u3  │ ✓  │    │ ✓  │    │    │    │    │  (LĐ Thần kinh/HN)
-- │ u4  │ ✓  │    │    │    │    │    │    │  (NV Thần kinh/HCM)
-- │ u5  │ ✓  │    │    │    │    │    │    │  (NV Tim mạch/HCM)
-- │ u6  │ ✓  │    │ ✓  │    │    │    │    │  (LĐ Tim mạch/HCM)
-- │ u7  │ ✓  │    │ ✓  │ ✓  │ ✓  │ ✓  │ ✓  │  (LĐ toàn bộ)
-- │ u8  │ ✓  │    │    │    │    │ ✓  │    │  (NV Tiêu hóa/HN)
-- └─────┴────┴────┴────┴────┴────┴────┴────┘
-- ============================================================

-- t1: Gửi đến TOÀN BỘ nhân viên (chỉ Level, không comp, không group)
INSERT INTO QLBENHVIEN.THONGBAO (NOIDUNG, NGAYGIO, DIADIEM, OLS_LABEL)
VALUES (
    N't1: Họp toàn thể nhân viên bệnh viện – Triển khai kế hoạch năm mới',
    SYSTIMESTAMP,
    N'Hội trường chính',
    CHAR_TO_LABEL('THONGBAO_POL', 'NV')
);

-- t2: Gửi đến toàn bộ BAN GIÁM ĐỐC
INSERT INTO QLBENHVIEN.THONGBAO (NOIDUNG, NGAYGIO, DIADIEM, OLS_LABEL)
VALUES (
    N't2: Họp khẩn Ban Giám đốc – Duyệt ngân sách quý II',
    SYSTIMESTAMP,
    N'Phòng họp Ban Giám đốc',
    CHAR_TO_LABEL('THONGBAO_POL', 'GD')
);

-- t3: Gửi đến tất cả LÃNH ĐẠO KHOA
INSERT INTO QLBENHVIEN.THONGBAO (NOIDUNG, NGAYGIO, DIADIEM, OLS_LABEL)
VALUES (
    N't3: Họp giao ban lãnh đạo các khoa – Báo cáo tháng',
    SYSTIMESTAMP,
    N'Phòng họp tầng 3',
    CHAR_TO_LABEL('THONGBAO_POL', 'LD')
);

-- t4: Gửi đến lãnh đạo KHOA TIÊU HÓA
INSERT INTO QLBENHVIEN.THONGBAO (NOIDUNG, NGAYGIO, DIADIEM, OLS_LABEL)
VALUES (
    N't4: Họp nội bộ Khoa Tiêu hóa – Đánh giá quy trình',
    SYSTIMESTAMP,
    N'Phòng Khoa Tiêu hóa',
    CHAR_TO_LABEL('THONGBAO_POL', 'LD:TH')
);

-- t5: Gửi đến nhân viên Khoa Tiêu hóa tại CƠ SỞ HCM
INSERT INTO QLBENHVIEN.THONGBAO (NOIDUNG, NGAYGIO, DIADIEM, OLS_LABEL)
VALUES (
    N't5: Lịch trực Khoa Tiêu hóa – Cơ sở TP. Hồ Chí Minh',
    SYSTIMESTAMP,
    N'Cơ sở HCM – Khoa Tiêu hóa',
    CHAR_TO_LABEL('THONGBAO_POL', 'NV:TH:HCM')
);

-- t6: Gửi đến nhân viên Khoa Tiêu hóa tại CƠ SỞ HÀ NỘI
INSERT INTO QLBENHVIEN.THONGBAO (NOIDUNG, NGAYGIO, DIADIEM, OLS_LABEL)
VALUES (
    N't6: Lịch trực Khoa Tiêu hóa – Cơ sở Hà Nội',
    SYSTIMESTAMP,
    N'Cơ sở Hà Nội – Khoa Tiêu hóa',
    CHAR_TO_LABEL('THONGBAO_POL', 'NV:TH:HN')
);

-- t7: Gửi đến lãnh đạo Khoa Tiêu hóa VÀ Khoa Thần kinh tại HẢI PHÒNG
INSERT INTO QLBENHVIEN.THONGBAO (NOIDUNG, NGAYGIO, DIADIEM, OLS_LABEL)
VALUES (
    N't7: Họp liên khoa Tiêu hóa & Thần kinh – Cơ sở Hải Phòng',
    SYSTIMESTAMP,
    N'Cơ sở Hải Phòng – Phòng họp liên khoa',
    CHAR_TO_LABEL('THONGBAO_POL', 'LD:TH,TK:HP')
);

COMMIT;

-- ============================================================
-- BƯỚC 11: KIỂM TRA (VERIFICATION)
-- Chạy từng lệnh để xác nhận OLS hoạt động đúng
-- ============================================================

-- Kiểm tra tổng số dòng (kết nối là QLBENHVIEN hoặc SYS):
SELECT COUNT(*) FROM QLBENHVIEN.THONGBAO;  -- Kết quả mong đợi: 7

-- Kiểm tra u1 (GiamDoc) thấy tất cả 7 dòng:
CONNECT U1/123@localhost:1521/xepdb1
SELECT NOIDUNG FROM QLBENHVIEN.THONGBAO;  -- 7 rows

-- Kiểm tra u4 (NV Thần kinh/HCM) chỉ thấy t1:
CONNECT U4/123@localhost:1521/xepdb1
SELECT NOIDUNG FROM QLBENHVIEN.THONGBAO;  -- 1 row (t1)

-- Kiểm tra u8 (NV Tiêu hóa/HN) thấy t1, t6:
CONNECT U8/123@localhost:1521/xepdb1
SELECT NOIDUNG FROM QLBENHVIEN.THONGBAO;  -- 2 rows (t1, t6)

-- ============================================================
-- KẾT THÚC SCRIPT OLS
-- ============================================================
