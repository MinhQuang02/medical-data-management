-- Scripts tạo user cho bệnh nhân và nhân viên (được chạy bằng C#)
-- QLBENHVIEN schema đã được tạo bởi DatabaseSeeder.cs
--Danh sách bệnh nhân
begin
    for r in (select MABN from BENHNHAN) loop
        begin
            execute immediate 'drop user ' || r.MABN || ' cascade';
        exception when others then null;
        end;
    end loop;
end;
/

--Danh sách nhân viên
begin
    for r in (select MANV from NHANVIEN) loop
        begin
            execute immediate 'drop user ' || r.MANV || ' cascade';
        exception when others then null;
        end;
    end loop;
end;
/

--Tạo danh sách người dùng là bệnh nhân
begin
    for r in (select MABN from BENHNHAN) loop
        execute immediate 'create user ' || r.MABN || ' identified by 123';
        execute immediate 'grant create session to ' || r.MABN;
    end loop;
end;
/

--Tạo danh sách người dùng là nhân viên
begin
    for r in (select MANV from NHANVIEN) loop
        execute immediate 'create user ' || r.MANV || ' identified by 123';
        execute immediate 'grant create session to ' || r.MANV;
    end loop;
end;
/
    
--Tạo role cho quản lý dữ liệu y tế
--Xóa các role để tạo lại
begin
    execute immediate 'drop role BenhNhan';
exception when others then null;
end;
/

begin
    execute immediate 'drop role DieuPhoiVien';
exception when others then null;
end;
/

begin
    execute immediate 'drop role BacSi';
exception when others then null;
end;
/

begin
    execute immediate 'drop role KyThuatVien';
exception when others then null;
end;
/

--Tạo các role và gán người dùng thích hợp
--Tạo và gán role BenhNhan
create role BenhNhan;

begin
    for r in (select MABN from BENHNHAN) loop
        execute immediate 'grant BenhNhan to '|| r.MABN;
    end loop;
end;
/

--Tạo và gán role DieuPhoiVien
create role DieuPhoiVien;

begin
    for r in (select MANV from NHANVIEN where MANV like 'DP%') loop
        execute immediate 'grant DieuPhoiVien to ' || r.MANV;
    end loop;
end;
/

--Tạo và gán role BacSi
create role BacSi;

begin
    for r in (select MANV from NHANVIEN where MANV like 'BS%') loop
        execute immediate 'grant BacSi to ' || r.MANV;
    end loop;
end;
/

--Tạo và gán role KyThuatVien
create role KyThuatVien;

begin
    for r in (select MANV from NHANVIEN where MANV like 'KTV%') loop
        execute immediate 'grant KyThuatVien to ' || r.MANV;
    end loop;
end;
/

--Câu 2: Dùng RBAC để thiết lập cho 'Kỹ thuật viên' và 'Bệnh nhân'
--Thiết lập cho 'Kỹ thuật viên'
--Tạo view để có thể thấy/ update được dòng của người đó
create or replace view KTV_XemHSBA_DV as
select *
from HSBA_DV
where MAKTV = USER
with check option;

--Gán view cho role
grant select on KTV_XemHSBA_DV to KyThuatVien;
grant update (KETQUA) on KTV_XemHSBA_DV to KyThuatVien;

--Tạo view để thấy/update được thông tin cá nhân chính mình
create or replace view KTV_XemTTCaNhan as
select *
from NHANVIEN
where MANV = USER
with check option;

--Gán view cho role
grant select on KTV_XemTTCaNhan to KyThuatVien;
grant update (QUEQUAN, SODT) on KTV_XemTTCaNhan to KyThuatVien;

--Thiết lập cho 'Bệnh nhân'
--Tạo view chỉ để xem thông tin và update cho chính mình
create or replace view BN_XemThongTin_BN as
select *
from BENHNHAN
where MABN = USER;

--Gán view cùng với update trên các cột theo đề
grant select, update (SONHA, TENDUONG, QUANHUYEN, TINHTP, TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC) on BN_XemThongTin_BN to BenhNhan;

--Câu 3: Ép thỏa chính sách bảo mật cho 'Điều phối viên' và 'Y sĩ/ Bác sĩ' thông qua VPD
--Cho 'Điều phối viên'
grant select, insert, update on BENHNHAN to DieuPhoiVien;
grant insert on HSBA to DieuPhoiVien;
grant update (MAKHOA, MABS) on HSBA to DieuPhoiVien;
grant update (MAKTV) on HSBA_DV to DieuPhoiVien;

--(BỔ SUNG) Quyền xem để điều phối
grant select on HSBA to DieuPhoiVien;
grant select on HSBA_DV to DieuPhoiVien;

--Tạo function để người đó thấy mỗi chính mình
create or replace function NV_VPD (
    p_schema in varchar2,
    p_object in varchar2
)
return varchar2
as
begin
    return 'MANV = ''' || USER || '''';
end;
/

--Dùng VPD để chuyển cho các nhân viên
begin
    dbms_rls.add_policy (
        object_schema => 'QLBENHVIEN',
        object_name => 'NHANVIEN',
        policy_name => 'nv_thay_ttin_ca_nhan',
        function_schema => 'QLBENHVIEN',
        policy_function => 'NV_VPD',
        statement_types => 'select, update'
    );
end;
/

--Phân quyền chỉnh sửa thêm trên cột cho QUEQUAN và SODT cùng quyền select bảng
grant update (QUEQUAN, SODT) on NHANVIEN to DieuPhoiVien;
grant select on NHANVIEN to DieuPhoiVien;

--Cho 'Bác sĩ/ Y sĩ'
--Tạo function để bác sĩ chỉ thấy dòng của chính mình
create or replace function BS_VPD (
    p_schema in varchar2,
    p_object in varchar2
)
return varchar2
as
begin
    return 'MABS = ''' || USER || '''';
end;
/

--Dùng VPD cho Bác sĩ
begin
    dbms_rls.add_policy (
        object_schema => 'QLBENHVIEN',
        object_name => 'HSBA',
        policy_name => 'bs_thay_dong_chinh_minh',
        function_schema => 'QLBENHVIEN',
        policy_function => 'BS_VPD',
        statement_types => 'select, insert, update, delete'
    );
end;
/

--Cho bác sĩ thấy dòng của chính mình
grant select on NHANVIEN to BacSi;
grant update (QUEQUAN, SODT) on NHANVIEN to BacSi;

--Xem các hồ sơ bệnh án mà mình đã điều trị, cập nhật CHUANDOAN, DIEUTRI, KETLUAN
grant select on HSBA to BacSi;
grant update (CHANDOAN, DIEUTRI, KETLUAN) on HSBA to BacSi;

--Thêm, xóa dòng trên HSBA_DV
--Tạo function cho việc lọc các mã hồ sơ bệnh án của một user đó
create or replace function BS_HSBA (
    p_schema in varchar2,
    p_object in varchar2
)
return varchar2
as
begin
    return 'MAHSBA in (
        select MAHSBA
        from HSBA
        where MABS = ''' || USER || '''
    )';
end;
/

--Thêm nó vào VPD cho bảng HSBA_DV
begin
    dbms_rls.add_policy (
        object_schema => 'QLBENHVIEN',
        object_name => 'HSBA_DV',
        policy_name => 'bs_them_xoa_dv',
        function_schema => 'QLBENHVIEN',
        policy_function => 'BS_HSBA',
        statement_types => 'select, insert, delete'
    );
end;
/

--Thêm vào bảng DONTHUOC
begin
    dbms_rls.add_policy (
        object_schema => 'QLBENHVIEN',
        object_name => 'DONTHUOC',
        policy_name => 'bs_them_xoa_capnhat_thuoc',
        function_schema => 'QLBENHVIEN',
        policy_function => 'BS_HSBA',
        statement_types => 'select, insert, delete, update'
    );
end;
/

grant select, insert, delete on DONTHUOC to BacSi;
grant update (TENTHUOC, LIEUDUNG) on DONTHUOC to BacSi;

--Grant quyền cho 'Bác sĩ'
grant insert, delete on HSBA_DV to BacSi;

--Tạo function để lọc ra hồ sơ bệnh nhân mà bác sĩ đó có khám
create or replace function BS_HSBenhNhan (
    p_schema in varchar2,
    p_object in varchar2
)
return varchar2
as
begin
    return 'MABN in (
        select MABN
        from HSBA
        where MABS = ''' || USER || '''
    )';
end;
/

--Gán VPD cho 'Bác sĩ'
begin
    dbms_rls.add_policy (
        object_schema => 'QLBENHVIEN',
        object_name => 'BENHNHAN',
        policy_name => 'bs_capnhat_benhnhan',
        function_schema => 'QLBENHVIEN',
        policy_function => 'BS_HSBenhNhan',
        statement_types => 'select, update'
    );
end;
/

grant select on BENHNHAN to BacSi;
grant update (TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC) on BENHNHAN to BacSi;

--audit trên hsba
create audit policy audit_hsba_update
actions update on QLBENHVIEN.HSBA;

audit policy audit_hsba_update;

--audit trên hsba_dv
create audit policy audit_hsba_dv_update
actions update on QLBENHVIEN.HSBA_DV;

audit policy audit_hsba_dv_update;

--audit trên donthuoc
create audit policy audit_donthuoc_update
actions update on QLBENHVIEN.DONTHUOC;

audit policy audit_donthuoc_update;