// feedback.js - Feedback cooldown countdown functionality
let countdownInterval;

function initFeedbackCooldown() {
    checkFeedbackCooldown();

    // Kiểm tra lại khi người dùng quay lại trang
    $(window).on('focus', checkFeedbackCooldown);
}

function checkFeedbackCooldown() {
    $.get('/Feedback/GetLastFeedbackTime', function (response) {
        if (response.canSubmit === false) {
            var minutesLeft = Math.ceil(response.minutesLeft);
            startCountdown(minutesLeft);
        } else {
            enableSubmitButton();
        }
    }).fail(function () {
        enableSubmitButton(); // Mặc định enable nếu có lỗi
    });
}

function startCountdown(minutesLeft) {
    var secondsLeft = minutesLeft * 60;
    var submitBtn = $('#submitBtn');
    var cooldownMessage = $('#cooldownMessage');
    var countdownTimer = $('#countdownTimer');

    // Disable nút submit
    submitBtn.prop('disabled', true);
    submitBtn.text('Please Wait...');

    // Hiển thị thông báo
    cooldownMessage.show();

    // Cập nhật đồng hồ đếm ngược
    function updateCountdown() {
        var minutes = Math.floor(secondsLeft / 60);
        var seconds = secondsLeft % 60;

        // Hiển thị định dạng đẹp hơn
        if (minutes > 0) {
            countdownTimer.text(minutes + ':' + (seconds < 10 ? '0' : '') + seconds);
        } else {
            countdownTimer.text(seconds + ' seconds');
        }

        if (secondsLeft <= 0) {
            clearInterval(countdownInterval);
            enableSubmitButton();
        } else {
            secondsLeft--;
        }
    }

    // Cập nhật ngay lập tức
    updateCountdown();

    // Cập nhật mỗi giây
    countdownInterval = setInterval(updateCountdown, 1000);
}

function enableSubmitButton() {
    var submitBtn = $('#submitBtn');
    var cooldownMessage = $('#cooldownMessage');

    submitBtn.prop('disabled', false);
    submitBtn.text('Submit Feedback');
    cooldownMessage.hide();
    clearInterval(countdownInterval);
}

// Khởi tạo khi document ready
$(document).ready(function () {
    initFeedbackCooldown();
});