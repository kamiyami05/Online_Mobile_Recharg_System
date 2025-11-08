$(document).ready(function () {

    function openModal() {
        $("#passwordModal").fadeIn();
    }

    function closeModal() {
        $("#PasswordCheck").val("");
        $("#passwordModal").fadeOut();
    }

    let userId = $(".profile-wrapper").data("user-id");

    // Xem mật khẩu
    $("#showPasswordBtn").click(function () {
        openModal();
        $("#ConfirmPasswordBtn").off().click(function () {
            $.post("/Profile/VerifyPassword", { password: $("#PasswordCheck").val() }, function (res) {
                if (res.success) {
                    $("#PasswordMask").attr("type", "text").val("Password Verified (hidden for security)");
                    closeModal();
                } else {
                    alert("Incorrect password.");
                }
            });
        });
    });

    // Bật chế độ chỉnh sửa
    $("#editBtn").click(function () {
        openModal();
        $("#ConfirmPasswordBtn").off().click(function () {
            $.post("/Profile/VerifyPassword", { password: $("#PasswordCheck").val() }, function (res) {
                if (res.success) {
                    closeModal();

                    $("input").prop("disabled", false).removeClass("readonly");
                    $("#PasswordMask").prop("disabled", true);

                    $("#editBtn").text("Save").off().click(function () {

                        $.post("/Profile/UpdateInfo", {
                            UserID: userId,
                            FullName: $("#FullName").val(),
                            MobileNumber: $("#MobileNumber").val(),
                            Email: $("#Email").val(),
                            Address: $("#Address").val()
                        }, function (response) {
                            alert("Profile updated successfully!");
                            location.reload();
                        });
                    });

                } else {
                    alert("Incorrect password.");
                }
            });
        });
    });

    $("#CancelPasswordModal").click(closeModal);
});
