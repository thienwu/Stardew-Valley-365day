# Stardew-365

**Stardew-365** là một bản mod mở rộng thời gian cho Stardew Valley, được tuỳ biến và phát triển lại từ nền tảng của bản mod **Longer Seasons** ban đầu. Mục tiêu của bản mod là mang đến một bộ lịch 365 ngày sát với thực tế, cung cấp cho bạn nhiều thời gian hơn để tận hưởng từng mùa màng, sự kiện và nhịp sống của Pelican Town.

## Tính năng chính

* **Lịch 365 Ngày:** Thay vì 112 ngày/năm như mặc định, mỗi mùa giờ đây sẽ kéo dài khoảng 3 tháng:
  * Mùa Xuân: 90 ngày (Tháng 1: 31, Tháng 2: 28, Tháng 3: 31)
  * Mùa Hè: 91 ngày (Tháng 4: 30, Tháng 5: 31, Tháng 6: 31)
  * Mùa Thu: 92 ngày (Tháng 7: 31, Tháng 8: 30, Tháng 9: 31)
  * Mùa Đông: 92 ngày (Tháng 10: 30, Tháng 11: 31, Tháng 12: 31)
* **Hệ thống Năm Nhuận (Leap Year):**
  * Các năm lẻ trong game (Năm 1, 3, 5, 7...) sẽ được tính là Năm Nhuận.
  * Vào Năm Nhuận, tháng 2 của Mùa Xuân sẽ có 29 ngày, nâng tổng số ngày trong năm lên 366 ngày.
* **Tương thích File Save cũ (Vanilla):** 
  * Bạn hoàn toàn có thể sử dụng file save cũ đang chơi. 
  * Khi tải game, hệ thống sẽ tự động tính toán và quy đổi số ngày đã chơi (DaysPlayed) của bạn sang hệ 365 ngày. Điều này giúp các sự kiện phụ thuộc vào thời gian (như lễ Đánh giá của ông nội) diễn ra đúng tiến độ mà không bị sai lệch.
* **Tương thích Sự kiện & UI:**
  * Bảng tin (Billboard) và Cuốn lịch đã được làm lại để hỗ trợ cuộn nhiều trang trong một mùa, hiển thị đúng các ngày từ 1 đến 31.
  * Lịch xuất hiện của NPC Bán Sách (Bookseller) đã được điều chỉnh. Trong Năm Nhuận, ngày Bán Sách ở tháng 3 mùa Xuân sẽ tự động được dời lại 1 ngày để đảm bảo luôn đúng lịch.
  * Bụi quả mọng (Berry bushes) được kéo dài thời gian kết trái tương ứng với độ dài mới của mùa.

## Dành cho người chơi (Cài đặt Mod)

1. Đảm bảo bạn đã cài đặt [SMAPI](https://smapi.io/).
2. Tải bản release (đã biên dịch) mới nhất tại mục **[Releases]** của kho lưu trữ này (nếu có).
3. Giải nén và bỏ thư mục `Stardew-365` vào thư mục `Mods` của Stardew Valley.
4. (Tuỳ chọn) Bạn có thể điều chỉnh cấu hình trong game thông qua **Generic Mod Config Menu**.

## Dành cho nhà phát triển (Biên dịch mã nguồn)

Kho lưu trữ này chứa **mã nguồn gốc** của bản mod. Nếu bạn muốn tự biên dịch (build):
1. Yêu cầu cài đặt **.NET 6 SDK** (hoặc mới hơn).
2. Tải hoặc clone mã nguồn này về máy: `git clone https://github.com/thienwu/Stardew-365.git`
3. Mở terminal/powershell tại thư mục mã nguồn và chạy lệnh:
   ```bash
   dotnet build
   ```
4. Bản mod sau khi build thành công sẽ nằm trong thư mục `bin/Debug/net6.0/`. Bạn có thể copy nội dung trong đó thả vào thư mục `Mods` của game.

## Nguồn gốc

Dự án này được xây dựng và phát triển dựa trên mã nguồn của bản mod **Longer Seasons**. Các tính năng về lịch thực tế, tương thích save game, và năm nhuận được viết lại hoàn toàn để tăng cường độ ổn định cho trò chơi.

---
*Chúc bạn có những phút giây thư giãn với một năm dài và trọn vẹn hơn tại Stardew Valley!*
