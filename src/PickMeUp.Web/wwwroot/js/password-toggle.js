// Password toggle visibility
(function () {
  // Toggle helper used by both direct listeners and delegated handler
  function togglePasswordButton(button) {
    if (!button) return;
    const targetId = button.dataset.target;
    let input = targetId ? document.getElementById(targetId) : null;
    if (!input) {
      const group = button.closest(".input-group") || button.parentElement;
      input = group
        ? group.querySelector('input[type="password"], input[type="text"]')
        : null;
    }
    const eyeOpen = button.querySelector(".eye-open");
    const eyeClosed = button.querySelector(".eye-closed");

    if (!input) return;

    if (input.type === "password") {
      input.type = "text";
      if (eyeOpen) eyeOpen.style.display = "none";
      if (eyeClosed) eyeClosed.style.display = "inline";
      button.setAttribute("aria-pressed", "true");
    } else {
      input.type = "password";
      if (eyeOpen) eyeOpen.style.display = "inline";
      if (eyeClosed) eyeClosed.style.display = "none";
      button.setAttribute("aria-pressed", "false");
    }

    // ensure focus stays on input
    try {
      input.focus();
    } catch (e) {}
  }

  // Attach click listeners to existing buttons (for completeness)
  function initPasswordToggles() {
    const toggleButtons = document.querySelectorAll(".pmu-password-toggle");
    toggleButtons.forEach((btn) => {
      // avoid double-binding
      if (!btn._pmuToggleBound) {
        btn.addEventListener("click", function (e) {
          e.preventDefault();
          togglePasswordButton(this);
        });
        btn._pmuToggleBound = true;
      }
    });
  }

  // Delegated handler: works even if script loads after DOM load or buttons are added later
  document.addEventListener("click", function (e) {
    const btn = e.target.closest && e.target.closest(".pmu-password-toggle");
    if (!btn) return;
    e.preventDefault();
    togglePasswordButton(btn);
  });

  // Initialize on page load and when modals are shown
  window.addEventListener("load", initPasswordToggles);
  document.addEventListener("shown.bs.modal", initPasswordToggles);

  // expose helper for manual invocation if needed
  window.togglePasswordButton = togglePasswordButton;
})();
