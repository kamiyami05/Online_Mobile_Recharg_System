// auth-validation.js

document.addEventListener('DOMContentLoaded', function () {
    initializeAuthValidation();
});

// Initialize validation for login and registration forms
function initializeAuthValidation() {
    const loginForm = document.querySelector('form[action*="Login"]');
    const registerForm = document.querySelector('form[action*="Register"]');

    if (loginForm) {
        setupLoginValidation(loginForm);
    }

    if (registerForm) {
        setupRegisterValidation(registerForm);
    }
}

// Setup validation for login form
function setupLoginValidation(form) {
    const emailInput = form.querySelector('input[type="email"]');
    const passwordInput = form.querySelector('input[type="password"]');

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        const isValid = validateLoginForm(emailInput, passwordInput);

        if (isValid) {
            form.submit();
        }
    });

    // Real-time validation
    emailInput.addEventListener('blur', function () {
        validateEmail(emailInput);
    });

    passwordInput.addEventListener('blur', function () {
        validatePassword(passwordInput);
    });
}

// Setup validation for registration form
function setupRegisterValidation(form) {
    const fullNameInput = form.querySelector('input[name="FullName"]');
    const phoneInput = form.querySelector('input[name="Phone"]');
    const emailInput = form.querySelector('input[type="email"]');
    const passwordInput = form.querySelector('input[name="Password"]');
    const confirmPasswordInput = form.querySelector('input[name="ConfirmPassword"]');

    // THÊM HÀM NÀY - Tạo password strength indicator
    function createPasswordStrengthIndicator() {
        const container = document.createElement('div');
        container.className = 'password-strength';

        const bar = document.createElement('div');
        bar.className = 'password-strength-bar';

        container.appendChild(bar);
        return container;
    }

    // Chỉ thêm password strength indicator nếu tìm thấy password input
    if (passwordInput) {
        const passwordStrength = createPasswordStrengthIndicator();
        passwordInput.parentNode.appendChild(passwordStrength);

        passwordInput.addEventListener('input', function () {
            updatePasswordStrength(this.value, passwordStrength);
        });
    }

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        const isValid = validateRegisterForm(
            fullNameInput,
            phoneInput,
            emailInput,
            passwordInput,
            confirmPasswordInput
        );

        if (isValid) {
            form.submit();
        }
    });

    // Real-time validation
    fullNameInput.addEventListener('blur', function () {
        validateFullName(fullNameInput);
    });

    phoneInput.addEventListener('blur', function () {
        validateVietnamesePhone(phoneInput);
    });

    emailInput.addEventListener('blur', function () {
        validateEmail(emailInput);
    });

    passwordInput.addEventListener('blur', function () {
        validatePassword(passwordInput);
    });

    confirmPasswordInput.addEventListener('blur', function () {
        validateConfirmPassword(passwordInput, confirmPasswordInput);
    });
}

// Create password strength indicator element - HOẶC THÊM Ở ĐÂY
function createPasswordStrengthIndicator() {
    const container = document.createElement('div');
    container.className = 'password-strength';

    const bar = document.createElement('div');
    bar.className = 'password-strength-bar';

    container.appendChild(bar);
    return container;
}

// Update password strength visual indicator
function updatePasswordStrength(password, container) {
    const bar = container.querySelector('.password-strength-bar');
    let strength = 0;

    if (password.length >= 6) strength += 1;
    if (password.length >= 8) strength += 1;
    if (/[A-Z]/.test(password)) strength += 1;
    if (/[0-9]/.test(password)) strength += 1;
    if (/[^A-Za-z0-9]/.test(password)) strength += 1;

    container.className = 'password-strength';

    if (password.length === 0) {
        bar.style.width = '0%';
    } else if (strength <= 2) {
        container.classList.add('weak');
    } else if (strength <= 4) {
        container.classList.add('medium');
    } else {
        container.classList.add('strong');
    }
}

// Login form validation
function validateLoginForm(emailInput, passwordInput) {
    let isValid = true;

    if (!validateEmail(emailInput)) isValid = false;
    if (!validatePassword(passwordInput)) isValid = false;

    return isValid;
}

// Registration form validation
function validateRegisterForm(fullNameInput, phoneInput, emailInput, passwordInput, confirmPasswordInput) {
    let isValid = true;

    if (!validateFullName(fullNameInput)) isValid = false;
    if (!validateVietnamesePhone(phoneInput)) isValid = false;
    if (!validateEmail(emailInput)) isValid = false;
    if (!validatePassword(passwordInput)) isValid = false;
    if (!validateConfirmPassword(passwordInput, confirmPasswordInput)) isValid = false;

    return isValid;
}

// Individual field validation functions
function validateFullName(input) {
    const value = input.value.trim();
    const fullNameRegex = /^[a-zA-ZÀ-ỹ\s]{2,50}$/;

    if (!value) {
        showError(input, 'Full name is required');
        return false;
    }

    if (!fullNameRegex.test(value)) {
        showError(input, 'Full name must be 2-50 characters and contain only letters and spaces');
        return false;
    }

    clearError(input);
    return true;
}

// Validate Vietnamese phone number
function validateVietnamesePhone(input) {
    const value = input.value.trim();
    // Mở rộng regex để hỗ trợ nhiều định dạng hơn
    const phoneRegex = /^(0|\+84)(3[2-9]|5[2689]|7[06-9]|8[1-689]|9[0-9])[0-9]{7}$/;

    if (!value) {
        showError(input, 'Phone number is required');
        return false;
    }

    // Chuẩn hóa số điện thoại
    const normalizedPhone = value.replace(/^\+84/, '0').replace(/\s/g, '');

    if (!phoneRegex.test(normalizedPhone)) {
        showError(input, 'Please enter a valid Vietnamese phone number (e.g., 0912345678 or +84912345678)');
        return false;
    }

    clearError(input);
    return true;
}

// Validate email format
function validateEmail(input) {
    const value = input.value.trim();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!value) {
        showError(input, 'Email is required');
        return false;
    }

    if (!emailRegex.test(value)) {
        showError(input, 'Please enter a valid email address');
        return false;
    }

    clearError(input);
    return true;
}

// Validate password strength
function validatePassword(input) {
    const value = input.value;

    if (!value) {
        showError(input, 'Password is required');
        return false;
    }

    if (value.length < 6) {
        showError(input, 'Password must be at least 6 characters long');
        return false;
    }

    clearError(input);
    return true;
}

// Validate confirm password matches password
function validateConfirmPassword(passwordInput, confirmPasswordInput) {
    const password = passwordInput.value;
    const confirmPassword = confirmPasswordInput.value;

    if (!confirmPassword) {
        showError(confirmPasswordInput, 'Please confirm your password');
        return false;
    }

    if (password !== confirmPassword) {
        showError(confirmPasswordInput, 'Passwords do not match');
        return false;
    }

    clearError(confirmPasswordInput);
    return true;
}

// Utility functions
function showError(input, message) {
    clearError(input);

    const errorElement = document.createElement('div');
    errorElement.className = 'text-red-500 validation-error';
    errorElement.textContent = message;
    errorElement.style.fontSize = '0.85rem';
    errorElement.style.marginTop = '6px';
    errorElement.style.color = '#ff6b6b';

    input.style.borderColor = '#ff6b6b';
    input.parentNode.appendChild(errorElement);
}

// Clear existing error message
function clearError(input) {
    const existingError = input.parentNode.querySelector('.validation-error');
    if (existingError) {
        existingError.remove();
    }
    input.style.borderColor = '';
}

// Add CSS animation
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
    
    .validation-error {
        animation: slideIn 0.3s ease;
    }
`;
document.head.appendChild(style);