ALTER SESSION SET CONTAINER = XEPDB1;

SHOW PARAMETER AUDIT_TRAIL;

NOAUDIT ALL;
/

AUDIT CREATE SESSION;
/

AUDIT TABLE;
AUDIT PROCEDURE;
AUDIT VIEW;
AUDIT TRIGGER;
/

AUDIT SYSTEM GRANT;
/

COLUMN AUDIT_OPTION FORMAT A30
COLUMN SUCCESS FORMAT A15
COLUMN FAILURE FORMAT A15
SELECT 
    USER_NAME,
    AUDIT_OPTION,
    SUCCESS,
    FAILURE
FROM DBA_STMT_AUDIT_OPTS
ORDER BY AUDIT_OPTION;

SELECT 
    'Audit Trail Status' AS CHECK_ITEM,
    CASE 
        WHEN (SELECT VALUE FROM V$PARAMETER WHERE NAME = 'audit_trail') IS NOT NULL 
        THEN 'ENABLED' 
        ELSE 'DISABLED' 
    END AS STATUS
FROM DUAL;

COMMIT;
/
-- ============================================================
-- 0. CONNECT AS DBA / QLBENHVIEN
-- ============================================================
ALTER SESSION SET CONTAINER = XEPDB1;

-- ============================================================
-- 1. CHECK AUDIT TRAIL
-- ============================================================
SHOW PARAMETER audit_trail;

-- ============================================================
-- 2. RESET OLD AUDIT
-- ============================================================
NOAUDIT ALL;
/

-- ============================================================
-- 3. SCENARIO 1 :Audit DELETE on HSBA_DV
-- ============================================================
AUDIT DELETE ON QLBENHVIEN.HSBA_DV BY ACCESS;
/

-- ============================================================
-- 4. SCENARIO 2: AUDIT UPDATE ON HSBA
-- ============================================================
AUDIT UPDATE ON QLBENHVIEN.HSBA BY ACCESS;
/

-- ============================================================
-- 5. SCENARIO 3 : AUDIT UPDATE ON DONTHUOC
-- ============================================================
AUDIT UPDATE ON QLBENHVIEN.DONTHUOC BY ACCESS;
/

-- ============================================================
-- 6. SCENARIO 4: AUDIT UPDATE ON VIEW (KTV)
-- ============================================================
AUDIT UPDATE ON QLBENHVIEN.KTV_XEMHSBA_DV BY ACCESS;
/

-- ============================================================
-- 7. SCENARIO 5: AUDIT DELETE ON DONTHUOC
-- ============================================================
AUDIT DELETE ON QLBENHVIEN.DONTHUOC BY ACCESS;
/

-- ============================================================
-- 8. GENERATE TEST DATA (SUCCESS + FAILURE)
-- ============================================================

-- ===== SCENARIO 1 =====
-- SUCCESS (Doctor)
CONN BS001/123@localhost:1521/XEPDB1
DELETE FROM QLBENHVIEN.HSBA_DV
WHERE ROWNUM = 1;

-- FAILURE (Patient)
CONN BN00001/123@localhost:1521/XEPDB1
DELETE FROM QLBENHVIEN.HSBA_DV;
-- ===== SCENARIO 2 =====
-- SUCCESS (Doctor)
CONN BS001/123@localhost:1521/XEPDB1
UPDATE QLBENHVIEN.HSBA
SET CHANDOAN = 'Test Audit'
WHERE ROWNUM = 1;

-- FAILURE (Technician)
CONN KTV001/123@localhost:1521/XEPDB1
UPDATE QLBENHVIEN.HSBA
SET CHANDOAN = 'Hack';

-- ===== SCENARIO 3  =====
-- SUCCESS (Doctor)
CONN BS001/123@localhost:1521/XEPDB1
UPDATE QLBENHVIEN.DONTHUOC
SET LIEUDUNG = 'UPDATED'
WHERE ROWNUM = 1;

-- FAILURE (Patient - no privilege)
CONN BN00001/123@localhost:1521/XEPDB1
UPDATE QLBENHVIEN.DONTHUOC
SET LIEUDUNG = 'HACK';

-- ===== SCENARIO 4 =====
-- SUCCESS (Technician correct row)
CONN KTV001/123@localhost:1521/XEPDB1
UPDATE QLBENHVIEN.KTV_XEMHSBA_DV
SET KETQUA = 'DONE';

-- FAILURE (Technician wrong object)
CONN KTV001/123@localhost:1521/XEPDB1
UPDATE QLBENHVIEN.HSBA_DV
SET KETQUA = 'HACK';

-- ===== SCENARIO 5 =====
-- SUCCESS (Doctor)
CONN BS001/123@localhost:1521/XEPDB1
DELETE FROM QLBENHVIEN.DONTHUOC
WHERE ROWNUM = 1;

-- FAILURE (Technician)
CONN KTV001/123@localhost:1521/XEPDB1
DELETE FROM QLBENHVIEN.DONTHUOC;

-- ============================================================
-- 9. VIEW AUDIT LOG
-- ============================================================
CONN QLBENHVIEN/123@localhost:1521/XEPDB1

COLUMN USERNAME FORMAT A15
COLUMN ACTION_NAME FORMAT A20
COLUMN OBJ_NAME FORMAT A25

SELECT 
    USERNAME,
    ACTION_NAME,
    OBJ_NAME,
    RETURNCODE,
    TO_CHAR(TIMESTAMP, 'DD-MM-YYYY HH24:MI:SS') AS TIME
FROM DBA_AUDIT_TRAIL
WHERE OBJ_NAME IN (
    'HSBA',
    'HSBA_DV',
    'KTV_XEMHSBA_DV',
    'DONTHUOC'
)
ORDER BY TIMESTAMP DESC;

-- ============================================================
-- 10. INTERPRETATION NOTE
-- ============================================================
-- RETURNCODE = 0  → SUCCESS
-- RETURNCODE ≠ 0 → FAILURE

COMMIT;   

-- ================================================================
-- YÊU CẦU 3
-- ================================================================
-- Kết nối vào PDB (Pluggable Database) với quyền DBA
ALTER SESSION SET CONTAINER = XEPDB1;

-- ============================================================
-- PHẦN A: TẠO CÁC CHÍNH SÁCH KIỂM TOÁN (POLICIES)
-- ============================================================

-- Xóa các policy cũ nếu có (để tránh lỗi khi chạy lại script)
BEGIN
  DBMS_FGA.DROP_POLICY(object_schema => 'QLBENHVIEN', object_name => 'DONTHUOC', policy_name => 'FGA_AUDIT_DONTHUOC_COLS');
  DBMS_FGA.DROP_POLICY(object_schema => 'QLBENHVIEN', object_name => 'HSBA', policy_name => 'FGA_AUDIT_HSBA_COLS');
EXCEPTION
  WHEN OTHERS THEN NULL;
END;
/

-- ------------------------------------------------------------
-- Tình huống 3a: Hành vi cập nhật trên các cột MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG của DONTHUOC
-- ------------------------------------------------------------
BEGIN
  DBMS_FGA.ADD_POLICY(
    object_schema   => 'QLBENHVIEN',
    object_name     => 'DONTHUOC',
    policy_name     => 'FGA_AUDIT_DONTHUOC_COLS',
    audit_column    => 'MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG',
    statement_types => 'UPDATE',
    audit_trail     => DBMS_FGA.DB + DBMS_FGA.EXTENDED
  );
END;
/

-- ------------------------------------------------------------
-- Tình huống 3b & 3c: Kiểm toán trên các trường CHANDOAN, DIEUTRI, KETLUAN của HSBA
-- ------------------------------------------------------------
BEGIN
  DBMS_FGA.ADD_POLICY(
    object_schema   => 'QLBENHVIEN',
    object_name     => 'HSBA',
    policy_name     => 'FGA_AUDIT_HSBA_COLS',
    audit_column    => 'CHANDOAN, DIEUTRI, KETLUAN',
    statement_types => 'UPDATE',
    audit_trail     => DBMS_FGA.DB + DBMS_FGA.EXTENDED
  );
END;
/

-- ------------------------------------------------------------
-- Tình huống 3c: Hành vi cập nhật bất hợp pháp trên CHANDOAN, DIEUTRI, KETLUAN
-- ------------------------------------------------------------
CREATE  AUDIT POLICY POL_HSBA_UPDATE_FAIL 
ACTIONS UPDATE ON QLBENHVIEN.HSBA;

AUDIT POLICY POL_HSBA_UPDATE_FAIL WHENEVER NOT SUCCESSFUL;

-- ------------------------------------------------------------
CREATE AUDIT POLICY POL_HSBADV_DML_FAIL 
ACTIONS INSERT ON QLBENHVIEN.HSBA_DV, 
        UPDATE ON QLBENHVIEN.HSBA_DV, 
        DELETE ON QLBENHVIEN.HSBA_DV;

AUDIT POLICY POL_HSBADV_DML_FAIL WHENEVER NOT SUCCESSFUL;


-- ============================================================
-- PHẦN B: TẠO NGỮ CẢNH ĐỂ SINH DỮ LIỆU NHẬT KÝ KIỂM TOÁN
-- ============================================================

-- [TEST 3a]: Bác sĩ cập nhật thông tin liều dùng trong ĐƠN THUỐC (Hợp lệ)
CONNECT BS002/123@localhost:1521/XEPDB1
UPDATE QLBENHVIEN.DONTHUOC 
SET LIEUDUNG = 'Sang 1 vien, Toi 1 vien' 
WHERE MAHSBA = 'HS001' AND TENTHUOC = 'PARACETAMOL';
COMMIT;

-- [TEST 3b]: Bác sĩ cập nhật thành công CĐ, ĐT, KL trên HSBA (Hợp lệ)
CONN BS002/123@localhost:1521/XEPDB1
UPDATE QLBENHVIEN.HSBA 
SET CHANDOAN = 'Viem hong hat', DIEUTRI = 'Uong thuoc', KETLUAN = 'Theo doi them' 
WHERE MAHSBA = 'HS001';
COMMIT;

-- [TEST 3c]: Bệnh nhân cố tình cập nhật CHANDOAN trên HSBA (Bất hợp pháp - Lỗi thiếu quyền)
CONN BN00001/123@localhost:1521/XEPDB1
UPDATE QLBENHVIEN.HSBA 
SET CHANDOAN = 'Khong co benh' 
WHERE MAHSBA = 'HS001';

-- [TEST 3d]: Bệnh nhân cố tình INSERT/DELETE vào HSBA_DV (Bất hợp pháp)
CONN BN00001/123@localhost:1521/XEPDB1
INSERT INTO QLBENHVIEN.HSBA_DV (MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA) 
VALUES ('HS001', 'X-Quang', SYSDATE, 'KTV001', 'Binh Thuong');

DELETE FROM QLBENHVIEN.HSBA_DV WHERE MAHSBA = 'HS001';

SELECT 
    DB_USER, 
    OBJECT_NAME, 
    POLICY_NAME, 
    SQL_TEXT, -- Cột này sẽ hiển thị rõ câu lệnh UPDATE của Bác sĩ
    TO_CHAR(TIMESTAMP, 'DD-MM-YYYY HH24:MI:SS') AS TIME
FROM DBA_FGA_AUDIT_TRAIL
ORDER BY TIMESTAMP DESC;
-- Kết nối lại bằng tài khoản Quản trị
CONN QLBENHVIEN/123@localhost:1521/XEPDB1;
EXEC DBMS_AUDIT_MGMT.FLUSH_UNIFIED_AUDIT_TRAIL;

SELECT 
    DBUSERNAME, 
    OBJECT_NAME, 
    ACTION_NAME, 
    UNIFIED_AUDIT_POLICIES, 
    RETURN_CODE, -- Cột này sẽ hiện mã lỗi ORA-01031
    TO_CHAR(EVENT_TIMESTAMP, 'DD-MM-YYYY HH24:MI:SS') AS TIME
FROM UNIFIED_AUDIT_TRAIL
WHERE UNIFIED_AUDIT_POLICIES IN ('POL_HSBA_UPDATE_FAIL', 'POL_HSBADV_DML_FAIL')
ORDER BY EVENT_TIMESTAMP DESC;
