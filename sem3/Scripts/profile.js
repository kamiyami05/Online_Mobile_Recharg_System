$(document).ready(function () {
    let userId = $(".profile-wrapper").data("user-id");
    let isEditMode = false;
    let audioPlayer = null;
    let isPlaying = false;

    // Utility function: Hiển thị password prompt
    async function showPasswordPrompt(title, text) {
        const { value: password } = await Swal.fire({
            title,
            text,
            html: `
            <div class="swal-input-wrapper" style="position: relative; margin-top: 10px;">
                <input id="swal-verify-password" type="password" class="swal2-input" placeholder="Enter your current password" style="padding-right: 40px;">
                <i class="fas fa-eye toggle-password" data-target="swal-verify-password" style="position: absolute; right: 12px; top: 50%; transform: translateY(-50%); cursor: pointer; color: #666; font-size: 14px;"></i>
            </div>
        `,
            focusConfirm: false,
            showCancelButton: true,
            confirmButtonText: 'Confirm',
            cancelButtonText: 'Cancel',
            customClass: {
                confirmButton: 'btn-primary',
                cancelButton: 'btn-secondary'
            },
            buttonsStyling: false,
            didOpen: () => {
                $('.toggle-password').off('click').on('click', function () {
                    const targetId = $(this).data('target');
                    const input = $(`#${targetId}`);
                    const type = input.attr('type') === 'password' ? 'text' : 'password';
                    input.attr('type', type);
                    $(this).toggleClass('fa-eye fa-eye-slash');
                });
            },
            preConfirm: () => {
                const pwd = $('#swal-verify-password').val();
                if (!pwd) {
                    Swal.showValidationMessage('Please enter your password');
                    return false;
                }
                return $.post("/Profile/VerifyPassword", { password: pwd })
                    .then(response => {
                        if (!response.success) {
                            throw new Error(response.message || 'Incorrect password');
                        }
                        return pwd;
                    })
                    .catch(error => {
                        Swal.showValidationMessage(error.responseJSON?.message || 'Password Incorrect');
                    });
            }
        });

        return password;
    }

    // Utility function: Hiển thị loading
    function showLoading(title = 'Loading...', text = 'Please wait') {
        return Swal.fire({
            title: title,
            text: text,
            allowOutsideClick: false,
            didOpen: () => Swal.showLoading()
        });
    }

    // Utility function: Hiển thị kết quả
    function showResult(icon, title, text) {
        return Swal.fire({
            icon: icon,
            title: title,
            text: text,
            confirmButtonText: 'OK',
            confirmButtonColor: icon === 'success' ? '#1a73e8' : '#d33'
        });
    }

    // Profile Edit Functions
    $("#editBtn").click(async function () {
        if (isEditMode) {
            await saveProfile();
            return;
        }

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

    $("#changePasswordBtn").click(handlePasswordChange);

    function enableEditMode() {
        $("input").prop("disabled", false).removeClass("readonly");

        // Thay đổi cách áp dụng class
        $("#editBtn")
            .text("Save Changes")
            .removeClass("edit-btn") // Xóa class edit-btn cũ
            .addClass("save-btn")    // Thêm class mới
            .css({
                'background': '#4caf50',
                'margin-bottom': '8px'
            });

        if ($("#cancelBtn").length === 0) {
            $("<button>")
                .text("Cancel")
                .addClass("cancel-btn")
                .attr("id", "cancelBtn")
                .attr("type", "button")
                .css({
                    'background': '#f44336',
                    'margin-top': '0'
                })
                .insertAfter("#editBtn")
                .click(cancelEdit);
        }

        isEditMode = true;
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
                confirmButtonText: '<i class="fas fa-check"></i> Yes, save changes',
                cancelButtonText: '<i class="fas fa-times"></i> No, cancel',
                customClass: {
                    confirmButton: 'btn-primary',
                    cancelButton: 'btn-secondary'
                },
                buttonsStyling: false
            });

            if (result.isConfirmed) {
                showLoading('Saving...', 'Please wait while we update your profile');

                const response = await $.post("/Profile/UpdateInfo", {
                    UserID: userId,
                    FullName: $("#FullName").val(),
                    MobileNumber: $("#MobileNumber").val(),
                    Email: $("#Email").val(),
                    Address: $("#Address").val()
                });

                Swal.close();

                if (response.success) {
                    await showResult('success', 'Success!', response.message);
                    location.reload();
                } else {
                    throw new Error(response.message || 'Failed to update profile');
                }
            }
        } catch (error) {
            showResult('error', 'Error!', error.message);
        }
    }

    async function handlePasswordChange() {
        try {
            const { value: formValues } = await Swal.fire({
                title: 'Change Password',
                html: `
                <div class="swal-password-grid">
                    <div class="swal-label">Current Password</div>
                    <div class="swal-input-wrapper">
                        <input id="swal-current-password" type="password" class="swal2-input" placeholder="Current Password">
                        <i class="fas fa-eye toggle-password" data-target="swal-current-password"></i>
                    </div>

                    <div class="swal-label">New Password</div>
                    <div class="swal-input-wrapper">
                        <input id="swal-new-password" type="password" class="swal2-input" placeholder="New Password">
                        <i class="fas fa-eye toggle-password" data-target="swal-new-password"></i>
                    </div>

                    <div class="swal-label">Confirm New Password</div>
                    <div class="swal-input-wrapper">
                        <input id="swal-confirm-password" type="password" class="swal2-input" placeholder="Confirm New Password">
                        <i class="fas fa-eye toggle-password" data-target="swal-confirm-password"></i>
                    </div>
                </div>
            `,
                focusConfirm: false,
                showCancelButton: true,
                confirmButtonText: '<i class="fas fa-check"></i> Change Password',
                cancelButtonText: '<i class="fas fa-times"></i> Cancel',
                customClass: {
                    popup: 'swal-password-popup',
                    confirmButton: 'btn-primary',
                    cancelButton: 'btn-secondary'
                },
                didOpen: () => {
                    // Xử lý toggle mắt
                    $('.toggle-password').off('click').on('click', function () {
                        const targetId = $(this).data('target');
                        const input = $(`#${targetId}`);
                        const type = input.attr('type') === 'password' ? 'text' : 'password';
                        input.attr('type', type);
                        $(this).toggleClass('fa-eye fa-eye-slash');
                    });
                },
                preConfirm: () => {
                    const cur = $('#swal-current-password').val();
                    const nw = $('#swal-new-password').val();
                    const cnf = $('#swal-confirm-password').val();

                    if (!cur || !nw || !cnf) return Swal.showValidationMessage('Please fill all fields');
                    if (nw.length < 6) return Swal.showValidationMessage('New password must be at least 6 characters');
                    if (nw !== cnf) return Swal.showValidationMessage('New passwords do not match');

                    return { currentPassword: cur, newPassword: nw, confirmPassword: cnf };
                }
            });

            if (formValues) {
                showLoading('Changing Password...', 'Please wait');
                const response = await $.post("/Profile/ChangePassword", {
                    OldPassword: formValues.currentPassword,
                    NewPassword: formValues.newPassword,
                    ConfirmPassword: formValues.confirmPassword,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                });
                Swal.close();
                if (response.success) {
                    await showResult('success', 'Success!', response.message);
                } else {
                    throw new Error(response.message || 'Failed to change password');
                }
            }
        } catch (e) {
            showResult('error', 'Error!', e.message);
        }
    }

    // Service Management Functions
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
                        showResult('success', 'Success!', response.message);
                    } else {
                        $("#doNotDisturbToggle").prop('checked', !isEnabled);
                        showResult('error', 'Error!', response.message);
                    }
                }).fail(function () {
                    $("#doNotDisturbToggle").prop('checked', !isEnabled);
                    showResult('error', 'Error!', 'An error occurred');
                });
            } else {
                $(this).prop('checked', !isEnabled);
            }
        });
    });

    // Caller Tune Functions
    $(document).on('click', '.tune-card', function () {
        const tuneValue = $(this).data('tune-value');

        $(".tune-card").removeClass('selected');
        $(this).addClass('selected');

        if (tuneValue === "upload") {
            $("#fileUploadSection").slideDown();
            stopCallerTunePreview();
        } else {
            $("#fileUploadSection").slideUp();
            tuneValue !== "Default" ? previewCallerTune(tuneValue) : stopCallerTunePreview();
            $("#actionButtons").show();
            $("#saveTuneBtn").data('selected-tune', tuneValue);
        }
    });

    $("#saveTuneBtn").click(function () {
        const selectedTune = $(this).data('selected-tune');
        if (selectedTune) {
            updateCallerTune(selectedTune);
        }
    });

    // Hàm preview Caller Tune
    function previewCallerTune(tuneFileName) {
        const player = initializeAudioPlayer();
        let displayName = "";

        if (tuneFileName === "Waiting.mp3") {
            player.src = '/Content/audio/Waiting.mp3';
            displayName = "Waiting Music";
        } else if (tuneFileName.startsWith('user_')) {
            player.src = '/Content/audio/uploads/' + tuneFileName;
            displayName = "My Custom Tune";
        } else {
            return;
        }

        updatePreviewName(displayName);
        $("#tunePreview").show();

        setPlayButtonText('Pause Preview');

        player.play().catch(() => {
            setPlayButtonText('Play Preview');
        });
    }

    function previewUploadedFile(file) {
        const player = initializeAudioPlayer();
        player.src = URL.createObjectURL(file);
        updatePreviewName(file.name);
        $("#tunePreview").show();

        setPlayButtonText('Pause Preview');
        player.play().catch(() => setPlayButtonText('Play Preview'));
    }

    function stopCallerTunePreview() {
        if (audioPlayer) {
            audioPlayer.pause();
            audioPlayer.currentTime = 0;
        }
        $("#tunePreview").hide();
        $("#playPreviewBtn").html('<i class="fas fa-play"></i> Play Preview');
    }

    function updateCallerTune(selectedTune) {
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
                showLoading('Updating...', 'Please wait');

                $.post("/Profile/UpdateCallerTune", { selectedTune: selectedTune }, function (response) {
                    Swal.close();
                    if (response.success) {
                        showResult('success', 'Success!', response.message).then(() => {
                            showLoading('Reloading...', '');
                            setTimeout(() => location.reload(), 800);
                        });
                    } else {
                        showResult('error', 'Error!', response.message);
                    }
                }).fail(() => {
                    Swal.close();
                    showResult('error', 'Error!', 'An error occurred');
                });
            }
        });
    }

    // Xử lý nút Cancel khi chọn Caller Tune
    $("#cancelTuneBtn").click(function () {
        // Reset chọn tune
        const currentTune = '@ViewBag.SelectedTune';
        $(`.tune-card[data-tune-value="${currentTune}"]`).addClass('selected');
        $(".tune-card").not(`[data-tune-value="${currentTune}"]`).removeClass('selected');

        // Ẩn action buttons, preview, upload
        $("#actionButtons").hide();
        $("#tunePreview").hide();
        $("#fileUploadSection").slideUp();
        stopCallerTunePreview();

        // Reset file input nếu đang upload
        resetFileInput();
    });

    // File Upload Functions
    function initializeDragAndDrop() {
        const uploadArea = $("#uploadArea")[0];

        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            uploadArea.addEventListener(eventName, preventDefaults, false);
        });

        function preventDefaults(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        ['dragenter', 'dragover'].forEach(eventName => {
            uploadArea.addEventListener(eventName, () => $("#uploadArea").addClass('drag-over'), false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            uploadArea.addEventListener(eventName, () => $("#uploadArea").removeClass('drag-over'), false);
        });

        uploadArea.addEventListener('drop', handleDrop, false);

        function handleDrop(e) {
            const files = e.dataTransfer.files;
            files.length > 0 && handleFiles(files);
        }
    }

    // Thêm sự kiện click cho nút Upload & Set
    $("#uploadTuneBtn").click(function () {
        uploadCallerTuneFile();
    });

    // Hàm upload file lên server
    function uploadCallerTuneFile() {
        const fileInput = $("#tuneFileInput")[0];
        const file = fileInput.files[0];

        if (!file) {
            showResult('error', 'Error!', 'Please select a file first');
            return;
        }

        if (!validateFile(file)) return;

        showLoading('Uploading...', 'Please wait while we upload your file');

        const formData = new FormData();
        formData.append('file', file);

        $.ajax({
            url: '/Profile/UploadCallerTune',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                Swal.close();

                if (response.success) {
                    showResult('success', 'Success!', response.message).then(() => {
                        showLoading('Reloading...', '');
                        setTimeout(() => location.reload(), 800);
                    });
                } else {
                    showResult('error', 'Error!', response.message);
                    resetFileInput();
                }
            },
            error: function (xhr, status, error) {
                Swal.close();
                showResult('error', 'Error!', 'Upload failed: ' + error);
                resetFileInput();
            }
        });
    }

    // Thêm sự kiện cho nút Close trong upload section
    $("#closeUploadBtn").click(function () {
        $("#fileUploadSection").slideUp();
        resetFileInput();

        // Reset selection
        $(".tune-card").removeClass('selected');
        const currentTune = '@ViewBag.SelectedTune';
        $(`.tune-card[data-tune-value="${currentTune}"]`).addClass('selected');
    });

    // Thêm sự kiện cho các nút trong custom tune card (Replace, Remove)
    $(document).on('click', '.replace-btn', function (e) {
        e.stopPropagation();
        $(".tune-card").removeClass('selected');
        $(".upload-card").addClass('selected');
        $("#fileUploadSection").slideDown();
        $("#actionButtons").hide();
    });

    $(document).on('click', '.remove-btn', function (e) {
        e.stopPropagation();
        const tuneValue = $(this).data('tune-value');

        Swal.fire({
            title: 'Remove Custom Tune?',
            text: 'Are you sure you want to remove your custom caller tune?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, remove',
            cancelButtonText: 'Cancel',
            confirmButtonColor: '#d33'
        }).then((result) => {
            if (result.isConfirmed) {
                showLoading('Removing...', 'Please wait');

                // Set back to Default when removing custom tune
                $.post("/Profile/UpdateCallerTune", { selectedTune: "Default" }, function (response) {
                    Swal.close();
                    if (response.success) {
                        showResult('success', 'Success!', response.message).then(() => {
                            location.reload();
                        });
                    } else {
                        showResult('error', 'Error!', response.message);
                    }
                });
            }
        });
    });

    function handleFiles(files) {
        $("#tuneFileInput")[0].files = files;
        $("#tuneFileInput").trigger('change');
    }

    $("#tuneFileInput").change(function () {
        const file = this.files[0];
        if (file) {
            if (!validateFile(file)) return;

            displayFileInfo(file);
            previewUploadedFile(file);
        } else {
            resetFileInput();
        }
    });

    function validateFile(file) {
        if (file.type !== 'audio/mp3' && !file.name.toLowerCase().endsWith('.mp3')) {
            showResult('error', 'Error!', 'Please select an MP3 file');
            resetFileInput();
            return false;
        }

        if (file.size > 10 * 1024 * 1024) {
            showResult('error', 'Error!', 'File size must be less than 10MB');
            resetFileInput();
            return false;
        }

        return true;
    }

    function displayFileInfo(file) {
        $("#fileName").text(file.name);
        $("#fileSize").text(formatFileSize(file.size));
        $("#fileInfo").show();
        $("#uploadArea").hide();
        $("#fileUploadSection").addClass('has-file');
    }

    function resetFileInput() {
        $("#tuneFileInput").val('');
        $("#fileName").text('');
        $("#fileSize").text('');
        $("#fileInfo").hide();
        $("#uploadArea").show();
        $("#fileUploadSection").removeClass('has-file');
        stopCallerTunePreview();
    }

    // Utility Functions
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    function updateDoNotDisturbUI(isEnabled) {
        const statusElement = $("#doNotDisturbStatus");
        statusElement.text(isEnabled ? 'Enabled' : 'Disabled')
            .removeClass('status-enabled status-disabled')
            .addClass(isEnabled ? 'status-enabled' : 'status-disabled');
    }

    function updateCallerTuneUI(selectedTune, isEnabled) {
        const statusElement = $("#callerTuneStatus");
        statusElement.text(isEnabled ? 'Enabled' : 'Disabled')
            .removeClass('status-enabled status-disabled')
            .addClass(isEnabled ? 'status-enabled' : 'status-disabled');
    }

    // Event Handlers
    $("#playPreviewBtn").off('click').on('click', function () {
        if (!audioPlayer) return;

        if (audioPlayer.paused) {
            audioPlayer.play();
            setPlayButtonText('Pause Preview');
        } else {
            audioPlayer.pause();
            setPlayButtonText('Play Preview');
        }
    });

    $("#stopPreviewBtn").off('click').on('click', stopCallerTunePreview);

    function stopCallerTunePreview() {
        if (audioPlayer) {
            audioPlayer.pause();
            audioPlayer.currentTime = 0;
            resetProgress();
            setPlayButtonText('Play Preview');
        }
        $("#tunePreview").hide();
    }

    function stopCallerTunePreview() {
        if (audioPlayer) {
            audioPlayer.pause();
            audioPlayer.currentTime = 0;
            resetProgress();
            $("#playPreviewBtn").html('<i class="fas fa-play"></i> Play Preview');
        }
        $("#tunePreview").hide();
    }

    $("#stopPreviewBtn").click(stopCallerTunePreview);

    $("#volumeSlider").on('input', function () {
        const volume = $(this).val();
        audioPlayer && (audioPlayer.volume = volume);
    });

    $(window).on('beforeunload', stopCallerTunePreview);
    $(document).on('hidden.bs.modal', stopCallerTunePreview);

    // Kích hoạt input file khi bấm "Browse Files"
    $("#browseBtn").click(function () {
        $("#tuneFileInput").click();
    });

    // Khởi tạo audio player với các sự kiện
    function initializeAudioPlayer() {
        if (!audioPlayer) {
            audioPlayer = new Audio();
            audioPlayer.id = 'callerTunePreview';
            audioPlayer.volume = 0.7;
            document.body.appendChild(audioPlayer);

            // Cập nhật thời gian khi đang phát
            audioPlayer.ontimeupdate = updateProgress;
            audioPlayer.onloadedmetadata = updateDuration;
            audioPlayer.onended = () => {
                isPlaying = false;
                $("#playPreviewBtn").html('<i class="fas fa-play"></i> Play Preview');
                resetProgress();
            };
        }
        return audioPlayer;
    }

    function updateDuration() {
        const d = audioPlayer.duration || 0;
        const m = Math.floor(d / 60);
        const s = ('0' + Math.floor(d % 60)).slice(-2);
        $("#totalTime").text(`${m}:${s}`);
        $("#previewDuration").text(`${m}:${s}`);
    }
    function updateProgress() {
        if (!audioPlayer) return;
        const c = audioPlayer.currentTime;
        const d = audioPlayer.duration || 1;
        const percent = (c / d) * 100;
        $("#progressFill").css("width", percent + "%");
        const m = Math.floor(c / 60);
        const s = ('0' + Math.floor(c % 60)).slice(-2);
        $("#currentTime").text(`${m}:${s}`);
    }
    function resetProgress() {
        $("#progressFill").css("width", "0%");
        $("#currentTime").text("0:00");
    }

    // Cập nhật tên file trong preview
    function updatePreviewName(name) {
        $("#previewTuneName").text(name);
    }

    function setPlayButtonText(txt) {
        $("#playPreviewBtn").html(`<i class="fas fa-${txt.includes('Pause') ? 'pause' : 'play'}"></i> ${txt}`);
    }

    // Khởi tạo trạng thái ban đầu
    function initializeServices(doNotDisturbEnabled, callerTune, callerTuneEnabled) {
        $("#doNotDisturbToggle").prop('checked', doNotDisturbEnabled);
        updateDoNotDisturbUI(doNotDisturbEnabled);

        $(`.tune-card[data-tune-value="${callerTune}"]`).addClass('selected');
        updateCallerTuneUI(callerTune, callerTuneEnabled);

        initializeDragAndDrop();
        $("#actionButtons").hide();
        $("#fileUploadSection").hide();
        $("#tunePreview").hide();

        // Khởi tạo player ngay để tránh lỗi
        initializeAudioPlayer();
    }
});
