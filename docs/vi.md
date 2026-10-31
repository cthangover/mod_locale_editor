# Bản dịch — nhập các văn bản mà một ngôn ngữ còn thiếu

Cửa sổ bản dịch liệt kê mọi văn bản mà một ngôn ngữ còn thiếu trong từng mod, và ghi bản dịch bạn nhập vào tệp bản địa hóa của mod sở hữu văn bản đó. Trò chơi hiện văn bản mới ngay. Mở bằng nút **Bản dịch** trên thanh công cụ trong game; có trong các trò chơi kèm công cụ phát triển (`cthangover`) và trong `rpg_plus`.

## Mở

Khởi động trò chơi, mở thanh công cụ và chọn **Bản dịch**. Cửa sổ đọc các tệp bản địa hóa của mọi mod đã cài; vài giây sau hiển thị số văn bản còn thiếu.

## Dịch một văn bản

1. Chọn **ngôn ngữ** ở góc trên bên trái (ví dụ `ko`). Danh sách chỉ hiện các văn bản mà ngôn ngữ đó còn thiếu.
2. Nếu muốn, chọn một **mod** để chỉ xem văn bản của mod đó.
3. Nhấn một dòng bên trái. Bên phải hiện:
   - **Khóa** — mã định danh mà trò chơi yêu cầu, ví dụ `rpg_plus/map/village`;
   - **Mod** — mod sở hữu văn bản;
   - **Tệp** — tệp bản địa hóa sẽ nhận dòng này, ví dụ `locale/ko.properties`;
   - **Văn bản ở ngôn ngữ khác** — cùng văn bản bằng tiếng Anh hoặc tiếng Nga (hoặc chuỗi gốc của kịch bản) để gợi ý.
4. Nhập bản dịch rồi nhấn **Lưu** (hoặc Enter, hoặc Ctrl+S). Dòng được ghi vào tệp. Trò chơi đọc lại văn bản và dòng biến khỏi danh sách. **Quét lại** đọc lại các tệp.

Chỉ một dòng trong tệp thay đổi: các khóa khác, chú thích và kiểu xuống dòng giữ nguyên.

## Văn bản được ghi vào đâu

- Khóa được khai báo ở ngôn ngữ khác của mod sẽ vào tệp của ngôn ngữ này: `ui_en.properties` → `ui_ko.properties`; `en.properties` → `ko.properties`.
- Khóa mà kịch bản dùng nhưng không tệp nào khai báo sẽ vào `scenarios_<ngôn ngữ>.properties` của mod.
- Mod dạng zip chỉ đọc. Các dòng của nó vẫn hiện, nhưng nút **Lưu** bị tắt và cửa sổ cho biết điều đó. Để dịch, hãy chép mod vào thư mục mod (mod dạng thư mục thì ghi được).

## Vì sao thiếu văn bản

Cửa sổ không tự quyết định gì: nó chỉ hiện những gì bộ atlas bản địa hóa (`locale_atlas`) tìm thấy. Atlas là bộ kiểm tra; báo cáo đầy đủ với mọi cảnh báo nằm ở `tests/artifacts/locale_atlas/report.md` (dành cho nhà phát triển). Bản dịch còn thiếu là một khoảng trống, không phải lỗi build: trò chơi hiện ngôn ngữ đang có văn bản.

## Nên biết

- Cửa sổ làm việc với các mod của trò chơi đang mở nó: mod chưa cài sẽ không có trong danh sách.
- Mod mới tạo bằng **Mod mới** (trình quản lý mod) có tiêu đề bằng tiếng Anh và ngôn ngữ của người chơi; các ngôn ngữ khác sẽ hiện ở đây như tiêu đề còn thiếu.
- Văn bản đã khai báo không được đưa ra. Để đổi bản dịch, hãy sửa tệp, hoặc dùng trình sửa vật phẩm hay nhân vật (tên của chúng được nhập theo từng ngôn ngữ trong biểu mẫu của chúng).
