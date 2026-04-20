-- ============================================================
-- FIX 1: BS_HSBENHNHAN – Extra ')' in BN predicate causes ORA-28113
-- Bug:  RETURN 'MABN = '''||v_user||''')'   <-- extra )
-- Fix:  RETURN 'MABN = '''||v_user||''''
-- ============================================================
CREATE OR REPLACE FUNCTION QLBENHVIEN.BS_HSBENHNHAN (
    P_SCHEMA VARCHAR2,
    P_OBJECT VARCHAR2
)
RETURN VARCHAR2
AS
    v_user varchar2(30);
BEGIN
    v_user := SYS_CONTEXT('USERENV', 'SESSION_USER');

    IF v_user LIKE 'DP%' THEN
        RETURN '1=1';
    ELSIF v_user LIKE 'BS%' THEN
        RETURN 'MABN IN (SELECT MABN FROM QLBENHVIEN.HSBA WHERE MABS = ''' || v_user || ''')';
    ELSIF v_user LIKE 'BN%' THEN
        RETURN 'MABN = ''' || v_user || '''';   -- fixed: removed extra ')'
    ELSIF v_user LIKE 'KTV%' THEN
        RETURN '1=0';
    ELSE
        RETURN '1=1';
    END IF;
END;
/

-- ============================================================
-- FIX 2: BS_HSBA – KTV% case queries HSBA_DV from within a policy
--         ON HSBA_DV causing ORA-28113 (recursive policy evaluation)
-- Fix:  Return predicate directly using MAKTV column available on HSBA_DV
-- ============================================================
CREATE OR REPLACE FUNCTION QLBENHVIEN.BS_HSBA (
    P_SCHEMA VARCHAR2,
    P_OBJECT VARCHAR2
)
RETURN VARCHAR2
AS
    v_user VARCHAR2(30);
BEGIN
    v_user := SYS_CONTEXT('USERENV', 'SESSION_USER');

    IF v_user LIKE 'BS%' THEN
        RETURN 'MAHSBA IN (SELECT MAHSBA FROM QLBENHVIEN.HSBA WHERE MABS = ''' || v_user || ''')';
    ELSIF v_user LIKE 'DP%' THEN
        RETURN '1=1';
    ELSIF v_user LIKE 'KTV%' THEN
        -- Fixed: use MAKTV directly (the column exists on HSBA_DV and DONTHUOC)
        -- For DONTHUOC: join via MAHSBA; for HSBA_DV: MAKTV = USER
        -- We use a safe non-recursive predicate
        RETURN 'MAKTV = ''' || v_user || '''';
    ELSE
        RETURN '1=0';
    END IF;
END;
/

-- ============================================================
-- FIX 3: BS_VPD – DP% incorrectly returns '1=0' on HSBA
-- Bug:  ELSIF USER LIKE 'DP%' ... RETURN '1=0'
-- Fix:  DP% should see all HSBA ('1=1') to coordinate assignments
-- ============================================================
CREATE OR REPLACE FUNCTION QLBENHVIEN.BS_VPD (
    P_SCHEMA VARCHAR2,
    P_OBJECT VARCHAR2
)
RETURN VARCHAR2
AS
BEGIN
    IF USER LIKE 'BS%' THEN
        RETURN 'MABS = ''' || USER || '''';
    ELSIF USER LIKE 'DP%' THEN
        RETURN '1=1';   -- fixed: coordinator must see all records
    ELSE
        RETURN '1=0';
    END IF;
END;
/

COMMIT;
