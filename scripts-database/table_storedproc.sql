-- =========================
-- DROP TABLE
-- =========================
-- BENHNHAN
BEGIN 
    EXECUTE IMMEDIATE 'DROP TABLE BENHNHAN CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- NHANVIEN
BEGIN 
    EXECUTE IMMEDIATE 'DROP TABLE NHANVIEN CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- HSBA
BEGIN 
    EXECUTE IMMEDIATE 'DROP TABLE HSBA CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- HSBA_DV
BEGIN 
    EXECUTE IMMEDIATE 'DROP TABLE HSBA_DV CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- DONTHUOC
BEGIN 
    EXECUTE IMMEDIATE 'DROP TABLE DONTHUOC CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- =========================
-- TABLE: BENHNHAN
-- =========================
CREATE TABLE BENHNHAN (
    MABN VARCHAR2(10) PRIMARY KEY,
    TENBN VARCHAR2(100),
    PHAI VARCHAR2(10),
    NGAYSINH DATE,
    CCCD VARCHAR2(12),
    SONHA VARCHAR2(50),
    TENDUONG VARCHAR2(100),
    QUANHUYEN VARCHAR2(100),
    TINHTP VARCHAR2(100),
    TIENSUBENH VARCHAR2(255),
    TIENSUBENHGD VARCHAR2(255),
    DIUNGTHUOC VARCHAR2(255)
);

-- =========================
-- TABLE: NHANVIEN
-- =========================
CREATE TABLE NHANVIEN (
    MANV VARCHAR2(10) PRIMARY KEY,
    HOTEN VARCHAR2(100),
    PHAI VARCHAR2(10),
    NGAYSINH DATE,
    CMND VARCHAR2(12),
    QUEQUAN VARCHAR2(100),
    SODT VARCHAR2(15),
    VAITRO VARCHAR2(30) CHECK (UPPER(VAITRO) IN ('ĐIỀU PHỐI VIÊN', 'BÁC SĨ/Y SĨ', 'KỸ THUẬT VIÊN','BỆNH NHÂN')),
    CHUYENKHOA VARCHAR2(100)
);

-- =========================
-- TABLE: HSBA
-- =========================
CREATE TABLE HSBA (
    MAHSBA VARCHAR2(10) PRIMARY KEY,
    MABN VARCHAR2(10),
    NGAY DATE,
    CHANDOAN VARCHAR2(255),
    DIEUTRI VARCHAR2(255),
    MABS VARCHAR2(10),
    MAKHOA VARCHAR2(5),
    KETLUAN VARCHAR2(255)
);

-- =========================
-- TABLE: HSBA_DV
-- =========================
CREATE TABLE HSBA_DV (
    MAHSBA VARCHAR2(10),
    LOAIDV VARCHAR2(100),
    NGAYDV DATE,
    MAKTV VARCHAR2(10),
    KETQUA VARCHAR2(255),
    PRIMARY KEY (MAHSBA, LOAIDV, NGAYDV)
);

-- =========================
-- TABLE: DONTHUOC
-- =========================
CREATE TABLE DONTHUOC (
    MAHSBA VARCHAR2(10),
    NGAYDT DATE,
    TENTHUOC VARCHAR2(100),
    LIEUDUNG VARCHAR2(100),
    PRIMARY KEY (MAHSBA, NGAYDT, TENTHUOC)
);

-- =================================
-- ADD FOREIGN KEY HSBA -> BENHNHAN
-- =================================
ALTER TABLE HSBA
ADD CONSTRAINT FK_HSBA_BN
FOREIGN KEY (MABN)
REFERENCES BENHNHAN(MABN);

-- =================================
-- ADD FOREIGN KEY HSBA -> NHANVIEN
-- =================================
ALTER TABLE HSBA
ADD CONSTRAINT FK_HSBA_NV
FOREIGN KEY (MABS)
REFERENCES NHANVIEN(MANV);

-- =================================
-- ADD FOREIGN KEY HSBA_DV -> HSBA
-- =================================
ALTER TABLE HSBA_DV
ADD CONSTRAINT FK_HSBADV_HSBA
FOREIGN KEY (MAHSBA)
REFERENCES HSBA(MAHSBA);

-- ====================================
-- ADD FOREIGN KEY HSBA_DV -> NHANVIEN
-- ====================================
ALTER TABLE HSBA_DV
ADD CONSTRAINT FK_HSBADV_NV
FOREIGN KEY (MAKTV)
REFERENCES NHANVIEN(MANV);

-- ====================================
-- ADD FOREIGN KEY DONTHUOC -> HSBA
-- ====================================
ALTER TABLE DONTHUOC
ADD CONSTRAINT FK_DT_HSBA
FOREIGN KEY (MAHSBA)
REFERENCES HSBA(MAHSBA);

-- ====================================
-- INSERT DATA
-- ====================================

-- INSERT 20 DIEU PHOI VIEN
BEGIN
    FOR i IN 1..20 LOOP
        MERGE INTO NHANVIEN t
        USING (
            SELECT 
                'DP' || LPAD(i,3,'0') AS MANV,
                'Điều phối ' || i AS HOTEN,
                'Nam' AS PHAI,
                DATE '1990-01-01' + i AS NGAYSINH,
                '079' || LPAD(i,8,'0') AS CMND,
                'HCM' AS QUEQUAN,
                '090' || LPAD(i,6,'0') AS SODT,
                'Điều phối viên' AS VAITRO,
                'Tổng hợp' AS CHUYENKHOA
            FROM DUAL
        ) s
        ON (t.MANV = s.MANV)
        WHEN NOT MATCHED THEN
        INSERT VALUES (
            s.MANV, s.HOTEN, s.PHAI, s.NGAYSINH,
            s.CMND, s.QUEQUAN, s.SODT, s.VAITRO, s.CHUYENKHOA
        );
    END LOOP;
END;
/

-- INSERT 100 BAC SI/Y SI
BEGIN
    FOR i IN 1..100 LOOP
        MERGE INTO NHANVIEN t
        USING (
            SELECT 
                'BS' || LPAD(i,3,'0') AS MANV,
                'Bác sĩ ' || i AS HOTEN,
                'Nam' AS PHAI,
                DATE '1985-01-01' + i AS NGAYSINH,
                '083' || LPAD(i,8,'0') AS CMND,
                'HCM' AS QUEQUAN,
                '091' || LPAD(i,6,'0') AS SODT,
                'Bác sĩ/Y sĩ' AS VAITRO,
                'Nội khoa' AS CHUYENKHOA
            FROM DUAL
        ) s
        ON (t.MANV = s.MANV)
        WHEN NOT MATCHED THEN
        INSERT VALUES (
            s.MANV, s.HOTEN, s.PHAI, s.NGAYSINH,
            s.CMND, s.QUEQUAN, s.SODT, s.VAITRO, s.CHUYENKHOA
        );
    END LOOP;
END;
/

-- INSERT 50 KY THUAT VIEN
BEGIN
    FOR i IN 1..50 LOOP
        MERGE INTO NHANVIEN t
        USING (
            SELECT 
                'KTV' || LPAD(i,3,'0') AS MANV,
                'Kỹ thuật viên ' || i AS HOTEN,
                'Nữ' AS PHAI,
                DATE '1992-01-01' + i AS NGAYSINH,
                '086' || LPAD(i,8,'0') AS CMND,
                'HCM' AS QUEQUAN,
                '092' || LPAD(i,6,'0') AS SODT,
                'Kỹ thuật viên' AS VAITRO,
                'Xét nghiệm' AS CHUYENKHOA
            FROM DUAL
        ) s 
        ON (t.MANV = s.MANV)
        WHEN NOT MATCHED THEN
        INSERT VALUES (
            s.MANV, s.HOTEN, s.PHAI, s.NGAYSINH,
            s.CMND, s.QUEQUAN, s.SODT, s.VAITRO, s.CHUYENKHOA
        );
    END LOOP;
END;
/

-- INSERT 100 BENH NHAN (Giảm số lượng để tránh timeout khi tạo User thực tế trên CSDL)
BEGIN
    FOR i IN 1..100 LOOP
        MERGE INTO BENHNHAN t
        USING (
            SELECT 
                'BN' || LPAD(i,5,'0') AS MABN,
                'Bệnh nhân ' || i AS TENBN,
                CASE WHEN MOD(i,2)=0 THEN 'Nam' ELSE 'Nữ' END AS PHAI,
                DATE '2000-01-01' + i AS NGAYSINH,
                '080' || LPAD(i,8,'0') AS CCCD,
                'Số ' || i AS SONHA,
                'Đường ABC' AS TENDUONG,
                'Quận 1' AS QUANHUYEN,
                'HCM' AS TINHTP,
                NULL AS TIENSUBENH, 
                NULL AS TIENSUBENHGD, 
                NULL AS DIUNGTHUOC
            FROM DUAL
        ) s 
        ON (t.MABN = s.MABN)
        WHEN NOT MATCHED THEN
        INSERT VALUES (
            s.MABN, s.TENBN, s.PHAI, s.NGAYSINH,
            s.CCCD, s.SONHA, s.TENDUONG, s.QUANHUYEN, s.TINHTP,
            s.TIENSUBENH, s.TIENSUBENHGD, s.DIUNGTHUOC
        );
    END LOOP;
END;
/

-- INSERT 100 HSBA
BEGIN
    FOR i IN 1..100 LOOP
        MERGE INTO HSBA t
        USING (
            SELECT 
                'HS' || LPAD(i,3,'0') AS MAHSBA,
                'BN' || LPAD(i,5,'0') AS MABN,
                DATE '2025-01-01' AS NGAY,
                NULL AS CHANDOAN, 
                NULL AS DIEUTRI,
                'BS' || LPAD(MOD(i,100)+1,3,'0') AS MABS,
                'K0001' AS MAKHOA,
                NULL AS KETLUAN
            FROM DUAL
        ) s
        ON (t.MAHSBA = s.MAHSBA)
        WHEN NOT MATCHED THEN
        INSERT VALUES (
            s.MAHSBA, s.MABN, s.NGAY,
            s.CHANDOAN, s.DIEUTRI,
            s.MABS, s.MAKHOA, s.KETLUAN
        );
    END LOOP;
END;
/

-- INSERT 100 HSBA_DV
BEGIN
    FOR i IN 1..100 LOOP
        MERGE INTO HSBA_DV t
        USING (
            SELECT 
                'HS' || LPAD(i,3,'0') AS MAHSBA,
                'Xét nghiệm' AS LOAIDV,
                DATE '2025-01-02' AS NGAYDV,
                'KTV' || LPAD(MOD(i,50)+1,3,'0') AS MAKTV,
                NULL AS KETQUA
            FROM DUAL
        ) s 
        ON (
            t.MAHSBA = s.MAHSBA AND 
            t.LOAIDV = s.LOAIDV AND 
            t.NGAYDV = s.NGAYDV
        )
        WHEN NOT MATCHED THEN
        INSERT VALUES (
            s.MAHSBA, s.LOAIDV, s.NGAYDV, s.MAKTV, s.KETQUA
        );
    END LOOP;
END;
/

-- INSERT 100 DONTHUOC
BEGIN
    FOR i IN 1..100 LOOP
        MERGE INTO DONTHUOC t
        USING (
            SELECT 
                'HS' || LPAD(i,3,'0') AS MAHSBA,
                DATE '2025-01-03' AS NGAYDT,
                'Paracetamol' AS TENTHUOC,
                '2 viên/ngày' AS LIEUDUNG
            FROM DUAL
        ) s
        ON (
            t.MAHSBA = s.MAHSBA AND 
            t.NGAYDT = s.NGAYDT AND 
            t.TENTHUOC = s.TENTHUOC
        )
        WHEN NOT MATCHED THEN
        INSERT VALUES (
            s.MAHSBA, s.NGAYDT, s.TENTHUOC, s.LIEUDUNG
        );
    END LOOP;
END;
/
COMMIT;

-- CREATE TEST STORED PROCEDURE
CREATE OR REPLACE PROCEDURE SP_UPDATE_CHANDOAN (
    P_MAHSBA VARCHAR2,
    P_CHANDOAN VARCHAR2,
    P_DIEUTRI VARCHAR2,
    P_KETLUAN VARCHAR2
)
AS
BEGIN
    UPDATE HSBA
    SET CHANDOAN = P_CHANDOAN,
        DIEUTRI = P_DIEUTRI,
        KETLUAN = P_KETLUAN
    WHERE MAHSBA = P_MAHSBA;
END;
/