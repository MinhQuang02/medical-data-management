-- Chuyển sang container chứa dữ liệu
ALTER SESSION SET CONTAINER = XEPDB1;

--Tăng thời gian Undo được giữ lên 2 tiếng
ALTER SYSTEM SET UNDO_RETENTION = 7200 SCOPE=BOTH;

-- Phương pháp 1: Phục hồi dữ liệu thông qua nhật ký
-- Giả lập phá hoại trong bảng
CONNECT BS002/123@localhost:1521/XEPDB1;

UPDATE QLBENHVIEN.HSBA 
SET CHANDOAN = 'Viem hong hat', DIEUTRI = 'Uong thuoc', KETLUAN = 'Theo doi them' 
WHERE MAHSBA = 'HS000001';
COMMIT;

-- Kết nối vào dữ liệu
CONNECT QLBENHVIEN/123@localhost:1521/XEPDB1;

-- Cho phép bảng được phép thay đổi vị trí dòng, sử dụng demo cho HSBA
ALTER TABLE HSBA ENABLE ROW MOVEMENT;

-- Bước 1: Truy vấn nhật ký FGA để tìm SCN của thao tác UPDATE trên bảng HSBA
-- Giả sử: Dựa trên Policy FGA_AUDIT_HSBA_COLS để tìm lệnh thành công
-- Soi lại dữ liệu thay đổi theo thời điểm
SELECT TIMESTAMP, SCN, DB_USER, SQL_TEXT 
FROM DBA_FGA_AUDIT_TRAIL 
WHERE OBJECT_NAME = 'HSBA' 
  AND POLICY_NAME = 'AUDIT_HSBA_COLUMN'
ORDER BY TIMESTAMP DESC;

-- Dùng Flashback Query để xem dữ liệu trước đó
SELECT CHANDOAN, DIEUTRI, KETLUAN 
FROM QLBENHVIEN.HSBA AS OF SCN (40810346 - 1)
WHERE MAHSBA = 'HS000001';

-- Sau khi xác định được SCN của dòng lệnh phá hoại (ví dụ lúc này là 40810346), thực hiện phục hồi về thời điểm ngay trước đó
UPDATE QLBENHVIEN.HSBA 
SET (CHANDOAN, DIEUTRI, KETLUAN) = (SELECT CHANDOAN, DIEUTRI, KETLUAN FROM QLBENHVIEN.HSBA AS OF SCN (40810346 - 1) WHERE MAHSBA = 'HS000001')
WHERE MAHSBA = 'HS000001';

COMMIT;

-- Kiểm tra lại thông tin của bảng dùng của bác sĩ
SELECT * FROM HSBA;

-- Phương pháp 2: Phục hồi dữ liệu thông qua Checkpoint
-- Connect lại vào database
CONNECT QLBENHVIEN/123@LOCALHOST:1521/XEPDB1;

-- Đưa lại dữ liệu về NULL để làm mốc
UPDATE QLBENHVIEN.HSBA 
SET CHANDOAN = NULL, DIEUTRI = NULL, KETLUAN = NULL 
WHERE MAHSBA = 'HS000001';
COMMIT;

-- Ép xuống đĩa và tạo phao
ALTER SYSTEM CHECKPOINT;
DROP RESTORE POINT BEFORE_TEST;
CREATE RESTORE POINT BEFORE_TEST;

-- Kiểm tra các cột đang trống trơn (NULL)
SELECT MAHSBA, CHANDOAN, DIEUTRI, KETLUAN FROM QLBENHVIEN.HSBA WHERE MAHSBA = 'HS000001';

-- Giả lập phá hoại dữ liệu
CONNECT BS002/123@localhost:1521/XEPDB1;

UPDATE QLBENHVIEN.HSBA 
SET CHANDOAN = 'Viem hong hat', DIEUTRI = 'Uong thuoc', KETLUAN = 'Theo doi them' 
WHERE MAHSBA = 'HS000001';
COMMIT;

-- Kiểm tra dữ liệu đã ĐẦY ĐỦ chữ nghĩa
SELECT * FROM QLBENHVIEN.HSBA WHERE MAHSBA = 'HS000001';

-- Phục hồi dùng Flashback table
CONNECT QLBENHVIEN/123@LOCALHOST:1521/XEPDB1;

ALTER TABLE HSBA ENABLE ROW MOVEMENT;

-- Lệnh thần thánh đưa tất cả về NULL như cũ
FLASHBACK TABLE HSBA TO RESTORE POINT BEFORE_TEST;
COMMIT;

-- Ép checkpoint lần cuối
ALTER SYSTEM CHECKPOINT;

-- Kiểm tra dữ liệu quay lại thành NULL trống trơn!
SELECT MAHSBA, CHANDOAN, DIEUTRI, KETLUAN FROM QLBENHVIEN.HSBA WHERE MAHSBA = 'HS000001';