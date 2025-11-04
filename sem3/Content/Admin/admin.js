// Xác nhận trước khi đăng xuất
document.addEventListener('DOMContentLoaded', function () {
    const logoutLink = document.querySelector('a[href="/Admin/Logout"]');
    if (logoutLink) {
        logoutLink.addEventListener('click', function (e) {
            e.preventDefault();
            if (confirm('Bạn có chắc muốn đăng xuất?')) {
                window.location.href = this.href;
            }
        });
    }
});