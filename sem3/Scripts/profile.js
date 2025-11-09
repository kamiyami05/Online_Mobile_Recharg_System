$(document).ready(function () {
    let userId = $(".profile-wrapper").data("user-id");
    let isEditMode = false;


    // Hàm hiển thị password prompt với SweetAlert
    async function showPasswordPrompt(title, text) {

        const { value: password } = await Swal.fire({
            title: title,
            text: text,
            input: 'password',
            inputLabel: 'Enter your current password',
            inputPlaceholder: 'Enter your password...',
            inputAttributes: {
                maxlength: 50,
                autocapitalize: 'off',
                autocorrect: 'off'
            },
            showCancelButton: true,
            confirmButtonText: 'Confirm',
            cancelButtonText: 'Cancel',
            showLoaderOnConfirm: true,
            preConfirm: (password) => {
                if (!password) {
                    Swal.showValidationMessage('Please enter your password');
                    return false;
                }
                return $.post("/Profile/VerifyPassword", { password: password })
                    .then(response => {
                        if (!response.success) {
                            throw new Error(response.message || 'Incorrect password');
                        }
                        return password;
                    })
                    .catch(error => {
                        Swal.showValidationMessage(error.responseJSON?.message || 'Authentication failed');
                    });
            },
            customClass: {
                confirmButton: 'btn-confirm',
                cancelButton: 'btn-cancel'
            },
            buttonsStyling: false
        });

        return password;
    }

    // Bật/tắt chế độ chỉnh sửa profile
    $("#editBtn").click(async function () {

        // Nếu đang ở chế độ edit, thì save
        if (isEditMode) {
            await saveProfile();
            return;
        }

        // Mở modal xác nhận mật khẩu để vào chế độ edit
        try {
            const password = await showPasswordPrompt(
                "Enter Edit Mode",
                "Please verify your identity to edit your profile information"
            );

            if (password) {
                enableEditMode();
            }
        } catch (error) {
            console.error("Edit mode error:", error);
        }
    });

    // Đổi mật khẩu
    $("#changePasswordBtn").click(async function () {

        try {
            const { value: formValues } = await Swal.fire({
                title: 'Change Password',
                html:
                    '<input id="swal-input1" type="password" placeholder="Current Password" class="swal2-input">' +
                    '<input id="swal-input2" type="password" placeholder="New Password" class="swal2-input">' +
                    '<input id="swal-input3" type="password" placeholder="Confirm New Password" class="swal2-input">',
                focusConfirm: false,
                showCancelButton: true,
                confirmButtonText: 'Change Password',
                cancelButtonText: 'Cancel',
                preConfirm: () => {
                    const currentPassword = document.getElementById('swal-input1').value;
                    const newPassword = document.getElementById('swal-input2').value;
                    const confirmPassword = document.getElementById('swal-input3').value;

                    if (!currentPassword || !newPassword || !confirmPassword) {
                        Swal.showValidationMessage('Please fill in all fields');
                        return false;
                    }

                    if (newPassword.length < 6) {
                        Swal.showValidationMessage('New password must be at least 6 characters long');
                        return false;
                    }

                    if (newPassword !== confirmPassword) {
                        Swal.showValidationMessage('New passwords do not match');
                        return false;
                    }

                    return [currentPassword, newPassword, confirmPassword];
                },
                customClass: {
                    confirmButton: 'btn-confirm',
                    cancelButton: 'btn-cancel'
                },
                buttonsStyling: false
            });

            if (formValues) {
                const [currentPassword, newPassword, confirmPassword] = formValues;

                // Hiển thị loading
                Swal.fire({
                    title: 'Changing Password...',
                    text: 'Please wait while we update your password',
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                // Gửi request đổi mật khẩu
                const response = await $.post("/Profile/ChangePassword", {
                    OldPassword: currentPassword,
                    NewPassword: newPassword,
                    ConfirmPassword: confirmPassword,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                });

                Swal.close();

                if (response.success) {
                    await Swal.fire({
                        title: 'Success!',
                        text: response.message,
                        icon: 'success',
                        confirmButtonText: 'OK',
                        confirmButtonColor: '#1a73e8'
                    });
                } else {
                    throw new Error(response.message || 'Failed to change password');
                }
            }
        } catch (error) {
            Swal.fire({
                title: 'Error!',
                text: error.message,
                icon: 'error',
                confirmButtonText: 'OK',
                confirmButtonColor: '#d33'
            });
        }
    });

    function enableEditMode() {
        $("input").prop("disabled", false).removeClass("readonly");
        $("#editBtn").text("Save Changes").addClass("save-mode");
        isEditMode = true;

        // Thêm nút Cancel nếu chưa có
        if ($("#cancelBtn").length === 0) {
            $("<button class='edit-btn cancel-btn' id='cancelBtn' type='button'>Cancel</button>")
                .insertAfter("#editBtn")
                .click(cancelEdit);
        }
    }

    function cancelEdit() {
        Swal.fire({
            title: 'Discard Changes?',
            text: 'All unsaved changes will be lost',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, discard',
            cancelButtonText: 'No, continue editing',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6'
        }).then((result) => {
            if (result.isConfirmed) {
                location.reload();
            }
        });
    }

    async function saveProfile() {
        try {
            const result = await Swal.fire({
                title: 'Save Changes?',
                text: 'Are you sure you want to update your profile information?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, save changes',
                cancelButtonText: 'No, cancel',
                confirmButtonColor: '#1a73e8',
                cancelButtonColor: '#6c757d'
            });

            if (result.isConfirmed) {
                // Hiển thị loading
                Swal.fire({
                    title: 'Saving...',
                    text: 'Please wait while we update your profile',
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                const response = await $.post("/Profile/UpdateInfo", {
                    UserID: userId,
                    FullName: $("#FullName").val(),
                    MobileNumber: $("#MobileNumber").val(),
                    Email: $("#Email").val(),
                    Address: $("#Address").val()
                });

                Swal.close();

                if (response.success) {
                    await Swal.fire({
                        title: 'Success!',
                        text: response.message,
                        icon: 'success',
                        confirmButtonText: 'OK',
                        confirmButtonColor: '#1a73e8'
                    });
                    location.reload();
                } else {
                    throw new Error(response.message || 'Failed to update profile');
                }
            }
        } catch (error) {
            Swal.fire({
                title: 'Error!',
                text: error.message,
                icon: 'error',
                confirmButtonText: 'OK',
                confirmButtonColor: '#d33'
            });
        }
    }

    // Xử lý phím Enter trong form khi đang edit
    $(document).keypress(function (e) {
        if (e.which == 13 && isEditMode) {
            $("#editBtn").click();
        }
    });

    // Do Not Disturb Toggle
    $("#doNotDisturbToggle").change(function () {
        const isEnabled = $(this).is(':checked');

        Swal.fire({
            title: 'Confirm',
            text: `Are you sure you want to ${isEnabled ? 'enable' : 'disable'} Do Not Disturb?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                $.post("/Profile/ToggleDoNotDisturb", function (response) {
                    if (response.success) {
                        updateDoNotDisturbUI(response.isEnabled);
                        Swal.fire('Success!', response.message, 'success');
                    } else {
                        // Revert toggle if failed
                        $("#doNotDisturbToggle").prop('checked', !isEnabled);
                        Swal.fire('Error!', response.message, 'error');
                    }
                }).fail(function () {
                    $("#doNotDisturbToggle").prop('checked', !isEnabled);
                    Swal.fire('Error!', 'An error occurred', 'error');
                });
            } else {
                // Revert toggle if cancelled
                $(this).prop('checked', !isEnabled);
            }
        });
    });

    // Caller Tunes Select
    $("#callerTuneSelect").change(function () {
        const selectedTune = $(this).val();
        $("#saveTuneBtn").prop('disabled', false).show();
        $("#cancelTuneBtn").show();

        // Preview nhạc nếu chọn Waiting Music
        if (selectedTune === "Waiting.mp3") {
            previewCallerTune();
        } else {
            stopCallerTunePreview();
        }
    });

    // Preview Caller Tune
    function previewCallerTune() {
        // Tạo hoặc sử dụng audio element có sẵn
        let audioPlayer = $("#callerTunePreview")[0];
        if (!audioPlayer) {
            audioPlayer = new Audio('/Content/audio/Waiting.mp3');
            audioPlayer.id = 'callerTunePreview';
            audioPlayer.volume = 0.7;
            document.body.appendChild(audioPlayer);
        }

        // Hiển thị player controls
        $("#tunePreview").show();
        audioPlayer.play().catch(function (error) {
            console.log("Audio play failed:", error);
            Swal.fire('Info', 'Click the play button to preview the tune', 'info');
        });
    }

    // Dừng preview
    function stopCallerTunePreview() {
        const audioPlayer = $("#callerTunePreview")[0];
        if (audioPlayer) {
            audioPlayer.pause();
            audioPlayer.currentTime = 0;
        }
        $("#tunePreview").hide();
    }

    // Play/Pause preview manually
    $("#playPreviewBtn").click(function () {
        const audioPlayer = $("#callerTunePreview")[0];
        if (audioPlayer) {
            if (audioPlayer.paused) {
                audioPlayer.play();
                $(this).html('<i class="fas fa-pause"></i> Pause Preview');
            } else {
                audioPlayer.pause();
                $(this).html('<i class="fas fa-play"></i> Play Preview');
            }
        }
    });

    // Stop preview
    $("#stopPreviewBtn").click(function () {
        stopCallerTunePreview();
        $("#playPreviewBtn").html('<i class="fas fa-play"></i> Play Preview');
    });

    // Save Caller Tune (cập nhật)
    $("#saveTuneBtn").click(function () {
        const selectedTune = $("#callerTuneSelect").val();

        // Dừng preview trước khi save
        stopCallerTunePreview();

        Swal.fire({
            title: 'Update Caller Tune?',
            text: `Set your caller tune to ${selectedTune === 'Default' ? 'Default (no music)' : selectedTune}?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, update',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                $.post("/Profile/UpdateCallerTune", { selectedTune: selectedTune }, function (response) {
                    if (response.success) {
                        updateCallerTuneUI(response.selectedTune, response.isEnabled);
                        $("#saveTuneBtn").prop('disabled', true).hide();
                        $("#cancelTuneBtn").hide();
                        Swal.fire('Success!', response.message, 'success');
                    } else {
                        Swal.fire('Error!', response.message, 'error');
                    }
                }).fail(function () {
                    Swal.fire('Error!', 'An error occurred', 'error');
                });
            }
        });
    });

    // Cancel Caller Tune changes (cập nhật)
    $("#cancelTuneBtn").click(function () {
        const originalTune = $(this).data('original-tune');
        $("#callerTuneSelect").val(originalTune);
        $("#saveTuneBtn").prop('disabled', true).hide();
        $(this).hide();

        // Dừng preview nếu đang chạy
        stopCallerTunePreview();
    });

    // Initialize services (cập nhật)
    function initializeServices(doNotDisturbEnabled, callerTune, callerTuneEnabled) {
        $("#doNotDisturbToggle").prop('checked', doNotDisturbEnabled);
        updateDoNotDisturbUI(doNotDisturbEnabled);

        $("#callerTuneSelect").val(callerTune);
        $("#cancelTuneBtn").data('original-tune', callerTune);
        updateCallerTuneUI(callerTune, callerTuneEnabled);

        // Hide action buttons initially
        $("#saveTuneBtn").hide();
        $("#cancelTuneBtn").hide();

        // Ẩn preview panel ban đầu
        $("#tunePreview").hide();
    } 

    // Volume control
    $("#volumeSlider").on('input', function () {
        const volume = $(this).val();
        const audioPlayer = $("#callerTunePreview")[0];
        if (audioPlayer) {
            audioPlayer.volume = volume;
        }
    });

    // Auto-stop preview khi rời trang
    $(window).on('beforeunload', function () {
        stopCallerTunePreview();
    });

    // Cleanup khi đóng modal (nếu có)
    $(document).on('hidden.bs.modal', function () {
        stopCallerTunePreview();
    });

    // UI Update functions
    function updateDoNotDisturbUI(isEnabled) {
        const statusElement = $("#doNotDisturbStatus");
        statusElement.text(isEnabled ? 'Enabled' : 'Disabled');
        statusElement.removeClass('status-enabled status-disabled')
            .addClass(isEnabled ? 'status-enabled' : 'status-disabled');
    }

    function updateCallerTuneUI(selectedTune, isEnabled) {
        const statusElement = $("#callerTuneStatus");
        statusElement.text(isEnabled ? 'Enabled' : 'Disabled');
        statusElement.removeClass('status-enabled status-disabled')
            .addClass(isEnabled ? 'status-enabled' : 'status-disabled');

        // Update cancel button's original tune
        $("#cancelTuneBtn").data('original-tune', selectedTune);
    }

    // Initialize service states
    function initializeServices(doNotDisturbEnabled, callerTune, callerTuneEnabled) {
        $("#doNotDisturbToggle").prop('checked', doNotDisturbEnabled);
        updateDoNotDisturbUI(doNotDisturbEnabled);

        $("#callerTuneSelect").val(callerTune);
        $("#cancelTuneBtn").data('original-tune', callerTune);
        updateCallerTuneUI(callerTune, callerTuneEnabled);

        // Hide action buttons initially
        $("#saveTuneBtn").hide();
        $("#cancelTuneBtn").hide();
    }
});