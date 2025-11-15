// Password validation with live feedback
(function () {
  const passwordRules = {
    length: (password) => password.length >= 8,
    number: (password) => /\d/.test(password),
    uppercase: (password) => /[A-Z]/.test(password),
    special: (password) =>
      /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password),
  };

  function validatePassword(password) {
    const results = {};
    for (const [rule, validator] of Object.entries(passwordRules)) {
      results[rule] = validator(password);
    }
    return results;
  }

  function updatePasswordStrength(inputId, strengthId, requirementsId) {
    const input = document.getElementById(inputId);
    const strengthContainer = document.getElementById(strengthId);
    const requirementsContainer = document.getElementById(requirementsId);

    if (!input || !strengthContainer) return;

    input.addEventListener("input", function () {
      const password = this.value;
      const validation = validatePassword(password);

      // Update strength indicators (dots)
      const dots = strengthContainer.querySelectorAll(".pmu-strength-dot");
      dots.forEach((dot) => {
        const rule = dot.dataset.rule;
        if (validation[rule]) {
          dot.classList.add("valid");
          dot.classList.remove("invalid");
        } else if (password.length > 0) {
          dot.classList.add("invalid");
          dot.classList.remove("valid");
        } else {
          dot.classList.remove("valid", "invalid");
        }
      });

      // Update requirements list if visible
      if (requirementsContainer) {
        const requirements =
          requirementsContainer.querySelectorAll(".pmu-requirement");
        requirements.forEach((req) => {
          const rule = req.dataset.rule;
          if (validation[rule]) {
            req.classList.add("valid");
            req.classList.remove("invalid");
          } else if (password.length > 0) {
            req.classList.add("invalid");
            req.classList.remove("valid");
          } else {
            req.classList.remove("valid", "invalid");
          }
        });
      }
    });

    // Toggle requirements on info button hover
    // Find info button in the same form group
    const formGroup = input.closest(".pmu-form-group");
    const infoBtn = formGroup?.querySelector(".pmu-password-info-btn");

    if (infoBtn && requirementsContainer) {
      infoBtn.addEventListener("mouseenter", function () {
        requirementsContainer.style.display = "block";
      });

      infoBtn.addEventListener("mouseleave", function () {
        requirementsContainer.style.display = "none";
      });

      // Also show on focus
      input.addEventListener("focus", function () {
        requirementsContainer.style.display = "block";
      });

      input.addEventListener("blur", function () {
        // Delay hiding to allow clicking on info button
        setTimeout(() => {
          if (!infoBtn.matches(":hover")) {
            requirementsContainer.style.display = "none";
          }
        }, 200);
      });
    }
  }

  // Initialize on page load
  window.addEventListener("load", function () {
    // Signup modal
    updatePasswordStrength(
      "signupPassword",
      "signupPasswordStrength",
      "signupPasswordRequirements"
    );

    // Reset password page (if exists)
    updatePasswordStrength(
      "resetPassword",
      "resetPasswordStrength",
      "resetPasswordRequirements"
    );
  });
})();
